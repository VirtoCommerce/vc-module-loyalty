// How each dynamic-expression node is rendered: a sentence of literal text and editable fields bound to the
// node's own properties (the API objects are edited in place and saved back untouched otherwise).
// Ported from Scripts/dynamicConditions/templates.html and the registrations in Scripts/module.js
// (adapted from the Marketing module's expression editor).
import type { ComputedRef, InjectionKey } from "vue";
import type { LookupSource } from "../api";

export interface ChoiceOption {
  value: string;
  label: string;
}
export type Part =
  | string
  | { kind: "number"; path: string; int?: boolean }
  | { kind: "text"; path: string; placeholder?: string }
  | { kind: "choice"; path: string; options: ChoiceOption[]; bool?: boolean }
  // compare condition select + value; a second value when "Between"
  | { kind: "range"; path: string; second: string }
  | { kind: "lookup"; path: string; source: LookupSource; multiple?: boolean }
  // the PerSku goal's link to its SKU list (opened by the mission blade)
  | { kind: "goalItems" };

export type TreeNode = Record<string, unknown> & { id?: string; children?: TreeNode[]; availableChildren?: TreeNode[] };

export interface Def {
  /** name in the "Add ..." menu */
  label?: string;
  /** group in the "Add ..." menu (the AngularJS groupName) */
  group?: string;
  parts: Part[];
  /** label of the button that adds a child */
  addLabel?: string;
  /** the AngularJS getValidationError */
  validate?: (node: TreeNode) => string | undefined;
}

const opts = (...pairs: [string, string][]): ChoiceOption[] => pairs.map(([value, label]) => ({ value, label }));
const allAny: Part = { kind: "choice", path: "all", bool: true, options: opts(["true", "all"], ["false", "any"]) };
const num = (path: string): Part => ({ kind: "number", path });
const int = (path: string): Part => ({ kind: "number", path, int: true });
const needsChild = (message: string) => (node: TreeNode) => (node.children?.length ? undefined : message);

export const compareOptions = opts(["Exactly", "exactly"], ["AtLeast", "at least"], ["IsLessThanOrEqual", "exactly or less than"], ["Between", "between"]);

const ORDER = "Order conditions";
const SPECIAL = "Special conditions";
const SHOPPER = "Shopper profile";
const GOAL = "Mission goal";

export const defs: Record<string, Def> = {
  // ---- conditions
  BlockLoyaltyCondition: {
    parts: ["If", allAny, "of the following criteria:"],
    addLabel: "Add condition",
    validate: needsChild("Your loyalty program must have at least one condition"),
  },
  BlockLoyaltyMissionCondition: {
    parts: ["If", allAny, "of the following criteria:"],
    addLabel: "Add condition",
    validate: needsChild("Your loyalty mission must have at least one condition"),
  },
  OrderStatusCondition: {
    label: "Order status is...",
    group: ORDER,
    parts: ["Order status is", { kind: "lookup", path: "orderStatuses", source: "orderStatuses", multiple: true }],
  },
  OrderTotalCondition: { label: "Order total is...", group: ORDER, parts: ["Order total is", { kind: "range", path: "totalValue", second: "totalValueSecond" }] },
  IsFirstOrderCondition: { label: "Is first order", group: ORDER, parts: ["Is first order"] },
  IsRecurringOrderCondition: { label: "Is recurring order", group: ORDER, parts: ["Is recurring order"] },
  IsRegistrationCondition: { label: "Registration", group: SPECIAL, parts: ["Registration"] },
  AnyUserGroupCondition: { label: "Any User Group", group: SHOPPER, parts: ["Any User Group"] },
  // shared shopper conditions of the Core module
  UserGroupsContainsCondition: {
    label: "User group contains ...",
    group: SHOPPER,
    parts: ["User group contains", { kind: "text", path: "group", placeholder: "Enter group" }],
  },
  UserGroupIsCondition: {
    label: "User group is ...",
    group: SHOPPER,
    parts: ["User group is", { kind: "lookup", path: "groups", source: "memberGroups", multiple: true }],
  },

  // ---- rewards
  BlockLoyaltyReward: {
    parts: ["Get the following loyalty rewards:"],
    addLabel: "Add reward",
    validate: needsChild("Your loyalty program must have at least one reward"),
  },
  FixedAmountReward: { label: "Earn fixed amount of points per order", parts: ["Get", num("amount"), "fixed points"] },
  RelativeAmountReward: { label: "Earn % of order value as points", parts: ["Get", num("amount"), "% of order value as points"] },

  // ---- mission goals
  BlockLoyaltyMissionGoals: {
    parts: ["Mission goal (choose one):"],
    addLabel: "Add goal",
    validate(node) {
      const goals = (node.children ?? []).filter((x) => !!x.missionType).length;
      if (!goals) return "Your mission must have exactly one goal: order value, order count or SKU-based";
      return goals > 1 ? "Your mission must have only one goal" : undefined;
    },
  },
  OrderValueGoal: {
    label: "Reach target order value",
    group: GOAL,
    parts: ["Reach order total of", num("value"), { kind: "lookup", path: "currencyCode", source: "currencies" }, "per mission period"],
    validate: (node) => (node.currencyCode ? undefined : "You must select a currency for the order value goal"),
  },
  OrderCountGoal: { label: "Reach target number of orders", group: GOAL, parts: ["Reach", int("count"), "orders per mission period"] },
  PerSkuGoal: { label: "Purchase target quantity of SKUs", group: GOAL, parts: ["Purchase", allAny, "of the target SKUs", { kind: "goalItems" }] },
};

const SKIP = new Set(["id", "availableChildren", "children"]);
const humanize = (id: string) => id.replace(/([a-z])([A-Z])/g, "$1 $2");

// Any node the table doesn't know (e.g. registered by another module): its id plus an editor per primitive property
export function defOf(node: TreeNode): Def {
  const id = String(node.id ?? "");
  const known = defs[id];
  if (known) return known;
  const parts: Part[] = [humanize(id)];
  for (const [key, value] of Object.entries(node)) {
    if (SKIP.has(key)) continue;
    if (typeof value === "number") parts.push(key, { kind: "number", path: key });
    else if (typeof value === "boolean") parts.push(key, { kind: "choice", path: key, bool: true, options: opts(["true", "yes"], ["false", "no"]) });
    else if (typeof value === "string" || value === null) parts.push({ kind: "text", path: key, placeholder: key });
  }
  return { label: humanize(id), parts };
}

// every validation message of the tree, as the AngularJS mission blade checked before saving
export function treeErrors(node: TreeNode | undefined | null): string[] {
  if (!node) return [];
  const own = node.id ? defs[node.id]?.validate?.(node) : undefined;
  return [...(own ? [own] : []), ...(node.children ?? []).flatMap(treeErrors)];
}

// the entity to save: its tree copied without the "Add ..." prototypes on every node (the AngularJS
// stripOffUiInformation); the server strips only the root's and adds them back when it loads the tree
export function withoutPrototypes<T extends { dynamicExpression?: object }>(entity: T): T {
  const strip = (node: TreeNode): TreeNode => ({ ...node, availableChildren: undefined, children: node.children?.map(strip) });
  return entity.dynamicExpression ? { ...entity, dynamicExpression: strip(entity.dynamicExpression as TreeNode) } : entity;
}

// the mission blade provides the PerSku goal's link to its SKU list
export const GoalItemsKey: InjectionKey<{ canOpen: ComputedRef<boolean>; open: () => void }> = Symbol("loyalty-goal-items");
