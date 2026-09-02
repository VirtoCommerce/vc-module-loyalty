# Extending the Loyalty Module — adding a new loyalty mechanic

This guide is for developers who want to add a new loyalty mechanic — a new way the module rewards users — without modifying the core dispatch service. The extension model is built around a single strategy interface, `ILoyaltyMechanic`, registered via DI.

## What "mechanic" means

A **mechanic** is one program type. The module ships two:

| Mechanic | `ProgramType` | What it rewards |
|---|---|---|
| `DefaultLoyaltyMechanic` | `"Default"` | A configurable amount derived from the order total (fixed amount or % of total) via the program's expression-tree rewards. |
| `ProductPointsLoyaltyMechanic` | `"ProductPoints"` | A per-line-item amount: `Σ (extendedPrice − discountAmount) × factor[productId]`, with a store-level default factor as fallback. |

A new mechanic could be Referral Bonus, Birthday Bonus, Cashback-by-category, Tier Upgrade, Multi-buy, etc. Each one supplies three things — discriminator, condition-tree prototype, calculation — and lets the framework handle everything else.

## The interface

[`ILoyaltyMechanic`](../src/VirtoCommerce.Loyalty.Core/Services/ILoyaltyMechanic.cs):

```csharp
public interface ILoyaltyMechanic
{
    // Unique identifier matching LoyaltyProgram.ProgramType.
    string ProgramType { get; }

    // Prototype of the condition tree the admin UI exposes for programs of this type.
    ConditionTree CreateConditionTreePrototype();

    // Compute the award amount for one satisfied program against a given context.
    Task<decimal> CalculateRewardAsync(LoyaltyProgram program, LoyaltyProgramEvaluationContext ctx);
}
```

## Lifecycle: how the platform picks a mechanic per order

```
OrderChangedEvent
       │  filter: not prototype, has payment, not LoyaltyPaymentMethod,
       │          (optional) order.Status matches Settings.General.AwardOnOrderStatus
       ▼
LoyaltyProgramHandler.Handle → BackgroundJob.Enqueue(ProcessAwardsAsync)
       │
       ▼
ProcessAwardsAsync:
   1. Filter already-processed orders (FindProcessedObjectIdsAsync — single batched query).
   2. For each remaining context: EvaluateLoyaltyProgramsAsync → LogLoyaltyProgramOperationAsync (under distributed lock).
       │
       ▼
LoyaltyLogicService.EvaluateLoyaltyProgramsAsync:
   1. Hydrate context (load order with WithItems|WithPrices, resolve user groups).
   2. Single pass over ALL active programs sorted by priority desc — mechanics interleaved.
      - Skip programs whose conditions aren't satisfied (DynamicExpression.IsSatisfiedBy).
      - Skip programs whose ProgramType isn't registered (logs a warning).
      - First satisfied + registered program wins → call its mechanic → return LoyaltyAmountResult.
       │
       ▼
LogLoyaltyProgramOperationAsync:
   - Idempotency check (IsObjectProcessedAsync by ObjectType + ObjectId).
   - Running balance = previous balance ± amount.
   - Persist LoyaltyBalanceOperationLog (OperationType = "Earned").
```

**Cross-mechanic semantics**: priority is global across types. A high-priority `ProductPoints` program wins over a low-priority `Default` program. If the highest-priority program's conditions don't match, the loop falls through to the next.

## Step-by-step: adding a `CashbackByCategory` mechanic (worked example)

Goal: reward `Σ (lineItem.net) × factor[lineItem.CategoryId]` instead of by productId.

### 1. Define the discriminator constant

[`ModuleConstants.cs`](../src/VirtoCommerce.Loyalty.Core/ModuleConstants.cs):

```csharp
public static class LoyaltyPrograms
{
    public const string EarnedOperationType = "Earned";
    public const string RedeemedOperationType = "Redeemed";
    public const string ProductProgramType = "ProductPoints";
    public const string DefaultProgramType = "Default";
    public const string CashbackByCategoryProgramType = "CashbackByCategory";  // new
}
```

### 2. (Optional) define a condition tree prototype

Skip this and reuse an existing prototype if your mechanic's allowed conditions overlap with one of the built-ins. Otherwise:

```csharp
public class LoyaltyProgramCashbackByCategoryConditionTreePrototype : ConditionTree
{
    public LoyaltyProgramCashbackByCategoryConditionTreePrototype()
    {
        IConditionTree[] children =
        [
            new BlockLoyaltyCondition()
                .WithAvailableChildren(
                    new UserGroupIsCondition(),
                    new OrderStatusCondition()),
        ];
        WithChildren(children);
        WithAvailableChildren(children);
    }
}
```

Register the prototype's children with `AbstractTypeFactory<IConditionTree>` in `Module.PostInitialize` (mirror the existing loop at [`Module.cs:123-131`](../src/VirtoCommerce.Loyalty.Web/Module.cs:123)).

### 3. (Optional) add an aux entity

If your mechanic needs additional persistent data (e.g. per-category factors), add an entity + EF configuration:

- `Module.Data/Models/LoyaltyProgramCategoryFactorEntity.cs`
- Register in `LoyaltyDbContext.OnModelCreating`
- Run `dotnet ef migrations add AddCashbackByCategoryFactor` against each provider project (the user does this — don't author migration files yourself unless asked)

### 4. Implement the mechanic

`Module.Data/Services/Mechanics/CashbackByCategoryLoyaltyMechanic.cs`:

```csharp
public class CashbackByCategoryLoyaltyMechanic(
    ICustomerOrderService orderService,   // optional — context.Order is preferred
    ILoyaltyProgramCategoryFactorSearchService factorSearchService,
    IStoreService storeService)
    : ILoyaltyMechanic
{
    public string ProgramType => ModuleConstants.LoyaltyPrograms.CashbackByCategoryProgramType;

    public ConditionTree CreateConditionTreePrototype()
        => AbstractTypeFactory<LoyaltyProgramCashbackByCategoryConditionTreePrototype>.TryCreateInstance();

    public async Task<decimal> CalculateRewardAsync(LoyaltyProgram program, LoyaltyProgramEvaluationContext ctx)
    {
        var order = ctx.Order;                              // hydrated upstream — no extra DB call
        if (order?.Items is null || order.Items.Count == 0) return 0m;

        var categoryIds = order.Items.Select(x => x.CategoryId).Distinct().ToArray();
        var factors = await factorSearchService.GetFactorsAsync(program.Id, categoryIds);
        var store = await storeService.GetByIdAsync(ctx.StoreId);
        var defaultFactor = store.Settings.GetValue<decimal>(ModuleConstants.Settings.General.DefaultProductMultiplyFactor);

        return order.Items.Sum(item =>
        {
            var net = LineItemMath.NetOf(item.ExtendedPrice, item.DiscountAmount);
            if (net <= 0m) return 0m;
            var factor = factors.TryGetValue(item.CategoryId, out var f) ? f : defaultFactor;
            return net * factor;
        });
    }
}
```

Key points:
- **Read `ctx.Order`**, don't reload the order from `ICustomerOrderService` — the platform hydrates it once upstream for performance.
- **Return `0m` to opt out** if the data your mechanic needs isn't present (no order, no items, missing required field).
- **Use `LineItemMath.NetOf`** for the net-of-discount calculation if your mechanic is order-line-based — keeps the formula consistent with `ProductPointsLoyaltyMechanic` and the storefront badges.

### 5. Register the mechanic in DI

[`Module.cs`](../src/VirtoCommerce.Loyalty.Web/Module.cs):

```csharp
serviceCollection.AddTransient<ILoyaltyMechanic, CashbackByCategoryLoyaltyMechanic>();
```

That's it. No edits to `LoyaltyLogicService`, `LoyaltyProgramHandler`, `LoyaltyProgramController`, or `ProcessAwardsAsync` — they pick up the new mechanic automatically through `IEnumerable<ILoyaltyMechanic>`.

### 6. (Optional) Custom event source

Most mechanics piggyback on the existing `OrderChangedEvent` and `UserChangedEvent` handlers. If you need a different trigger (e.g. a daily Hangfire job for birthday bonuses), write your own handler that:

1. Builds a `LoyaltyProgramEvaluationContext` with the right `ContextObjectType`/`ContextObjectId`.
2. Enqueues `LoyaltyProgramHandler.ProcessAwardsAsync(contexts, objectType)` or calls `ILoyaltyLogicService.EvaluateLoyaltyProgramsAsync` / `LogLoyaltyProgramOperationAsync` directly.

The dispatch and balance-write paths are unchanged.

### 7. Tests

Mirror the test patterns under [`tests/VirtoCommerce.Loyalty.Tests/Services/Mechanics`](../tests/VirtoCommerce.Loyalty.Tests/Services/Mechanics). At minimum:

- A test per branch of `CalculateRewardAsync` (no order, no items, factor match, factor miss → default).
- `ProgramType_IsExpectedConstant`.
- `CreateConditionTreePrototype_ReturnsExpectedType`.

Use Moq for collaborators and the standard `[Trait("Category", "Unit")]` + `//Arrange //Act //Assert` template per Virto conventions.

## What you do **not** need to touch

- [`LoyaltyLogicService.EvaluateLoyaltyProgramsAsync`](../src/VirtoCommerce.Loyalty.Data/Services/LoyaltyLogicService.cs) — the dispatch loop is mechanic-agnostic.
- [`LoyaltyProgramHandler.ProcessAwardsAsync`](../src/VirtoCommerce.Loyalty.Data/Handlers/LoyaltyProgramHandler.cs) — runs your mechanic via the logic service.
- `LogLoyaltyProgramOperationAsync` — handles balance computation, idempotency, persistence.
- The distributed-lock key and Hangfire job wiring.
- The `LoyaltyProgramController` REST endpoints — they resolve the prototype via the mechanic registry automatically.

## Cross-mechanic semantics — design implications

Priority is global. Configure carefully:

- **High-priority specific mechanics shadow low-priority catch-alls.** A priority-30 ProductPoints program with `UserGroupIs VIP` followed by a priority-10 Default program with no conditions means VIPs earn via ProductPoints and everyone else earns the Default reward.
- **Mechanics whose conditions can fail let lower-priority mechanics run.** The loop continues past unsatisfied programs.
- **Single award per order across all mechanics.** Only the first satisfied program wins. If you want Default + ProductPoints to both award the same order, you'd need to extend the dispatch model — out of scope of the current design.

## Condition compatibility

Conditions are evaluated against [`LoyaltyProgramEvaluationContext`](../src/VirtoCommerce.Loyalty.Core/Models/LoyaltyProgramEvaluationContext.cs). The context exposes:

- `UserId`, `UserGroups`, `IsRegistration` — set for user-triggered evaluations.
- `OrderId`, `OrderStatus`, `OrderTotal`, `IsFirstOrder`, `IsRecurringOrder`, `Order` (full domain object with line items) — set for order-triggered evaluations.
- `StoreId`, `CurrencyCode`, `Language` — set in both.

Your condition prototype determines which condition types the admin UI exposes. Build the smallest set that's actually meaningful — listing irrelevant conditions confuses admins.

## Storefront badge integration (optional)

If your mechanic should surface an "earn N" preview on the catalog or cart (as ProductPoints does today), expose a calculator service that returns a context with a single `CalculatePoints(net, productId)`-style rule. Then call it from both your XAPI type hook and your mechanic — exactly like [`IProductPointsCalculator`](../src/VirtoCommerce.Loyalty.Core/Services/IProductPointsCalculator.cs) + [`ProductPointsContext`](../src/VirtoCommerce.Loyalty.Core/Models/ProductPointsContext.cs).

**Anti-pattern**: do not duplicate the math between the hook and the mechanic. The calculation rule must live in exactly one method. Diverging implementations will silently drift and lead to "displayed earn ≠ awarded earn" bugs.

## Anti-patterns to avoid

- ❌ Adding `switch (programType)` anywhere in `LoyaltyLogicService` or downstream services.
- ❌ Calling `LoyaltyBalanceOperationLogService.SaveChangesAsync` directly to write balance entries. Always go through `ILoyaltyLogicService.LogLoyaltyProgramOperationAsync` so idempotency and balance computation stay consistent.
- ❌ Reloading the `CustomerOrder` from `ICustomerOrderService` inside your mechanic. Read `ctx.Order`.
- ❌ Reusing another mechanic's `ProgramType` discriminator. The DI registration validates this at startup and throws `InvalidOperationException` if duplicates exist.
- ❌ Mutating `ctx` in a way another mechanic would notice. Mechanics are isolated; treat the context as read-only past `PopulateLoyaltyProgramEvaluationContextAsync`.

## Configuration knob you might want

`Settings.General.AwardOnOrderStatus` (per-store) — when set (e.g. `"Paid"` or `"Completed"`), the handler only enqueues awards for orders that have reached that status. Empty/null preserves the original behavior (fire on Add/Modify with payment, status-agnostic). Use this to align award timing with order finalization.

## See also

- [`src/VirtoCommerce.Loyalty.Core/Services/ILoyaltyMechanic.cs`](../src/VirtoCommerce.Loyalty.Core/Services/ILoyaltyMechanic.cs) — the interface.
- [`src/VirtoCommerce.Loyalty.Data/Services/Mechanics/DefaultLoyaltyMechanic.cs`](../src/VirtoCommerce.Loyalty.Data/Services/Mechanics/DefaultLoyaltyMechanic.cs) — minimal reference impl.
- [`src/VirtoCommerce.Loyalty.Data/Services/Mechanics/ProductPointsLoyaltyMechanic.cs`](../src/VirtoCommerce.Loyalty.Data/Services/Mechanics/ProductPointsLoyaltyMechanic.cs) — thin orchestrator over the shared calculator.
- [`src/VirtoCommerce.Loyalty.Data/Services/LoyaltyLogicService.cs`](../src/VirtoCommerce.Loyalty.Data/Services/LoyaltyLogicService.cs) — the dispatch loop.
