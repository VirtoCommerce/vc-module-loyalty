// How each dynamic-expression node is rendered: a sentence of literal text and editable fields bound to the
// node's own properties (the API objects are edited in place and saved back untouched otherwise).
// Ported from Scripts/dynamicConditions/templates.html and the registrations in Scripts/module.js
// (adapted from the Marketing module's expression editor).
// Every text here (labels, groups, sentence parts, option labels, messages) is an i18n key, translated where it's shown.
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
const allAny: Part = { kind: "choice", path: "all", bool: true, options: opts(["true", "LOYALTY.EXPRESSIONS.ALL"], ["false", "LOYALTY.EXPRESSIONS.ANY"]) };
const num = (path: string): Part => ({ kind: "number", path });
const int = (path: string): Part => ({ kind: "number", path, int: true });
const needsChild = (message: string) => (node: TreeNode) => (node.children?.length ? undefined : message);

export const compareOptions = opts(
  ["Exactly", "LOYALTY.EXPRESSIONS.COMPARE.EXACTLY"],
  ["AtLeast", "LOYALTY.EXPRESSIONS.COMPARE.AT_LEAST"],
  ["IsLessThanOrEqual", "LOYALTY.EXPRESSIONS.COMPARE.EXACTLY_OR_LESS"],
  ["Between", "LOYALTY.EXPRESSIONS.COMPARE.BETWEEN"],
);

const ORDER = "LOYALTY.EXPRESSIONS.GROUPS.ORDER";
const SPECIAL = "LOYALTY.EXPRESSIONS.GROUPS.SPECIAL";
const SHOPPER = "LOYALTY.EXPRESSIONS.GROUPS.SHOPPER";
const GOAL = "LOYALTY.EXPRESSIONS.GROUPS.GOAL";
const C = "LOYALTY.EXPRESSIONS.CONDITIONS";
const R = "LOYALTY.EXPRESSIONS.REWARDS";
const G = "LOYALTY.EXPRESSIONS.GOALS";
const E = "LOYALTY.EXPRESSIONS.ERRORS";

export const defs: Record<string, Def> = {
  // ---- conditions
  BlockLoyaltyCondition: {
    parts: [`${C}.IF`, allAny, `${C}.OF_CRITERIA`],
    addLabel: "LOYALTY.EXPRESSIONS.ADD_CONDITION",
    validate: needsChild(`${E}.PROGRAM_CONDITION`),
  },
  BlockLoyaltyMissionCondition: {
    parts: [`${C}.IF`, allAny, `${C}.OF_CRITERIA`],
    addLabel: "LOYALTY.EXPRESSIONS.ADD_CONDITION",
    validate: needsChild(`${E}.MISSION_CONDITION`),
  },
  OrderStatusCondition: {
    label: `${C}.ORDER_STATUS`,
    group: ORDER,
    parts: [`${C}.ORDER_STATUS_IS`, { kind: "lookup", path: "orderStatuses", source: "orderStatuses", multiple: true }],
  },
  OrderTotalCondition: {
    label: `${C}.ORDER_TOTAL`,
    group: ORDER,
    parts: [`${C}.ORDER_TOTAL_IS`, { kind: "range", path: "totalValue", second: "totalValueSecond" }],
  },
  IsFirstOrderCondition: { label: `${C}.FIRST_ORDER`, group: ORDER, parts: [`${C}.FIRST_ORDER`] },
  IsRecurringOrderCondition: { label: `${C}.RECURRING_ORDER`, group: ORDER, parts: [`${C}.RECURRING_ORDER`] },
  IsRegistrationCondition: { label: `${C}.REGISTRATION`, group: SPECIAL, parts: [`${C}.REGISTRATION`] },
  AnyUserGroupCondition: { label: `${C}.ANY_USER_GROUP`, group: SHOPPER, parts: [`${C}.ANY_USER_GROUP`] },
  // shared shopper conditions of the Core module
  UserGroupsContainsCondition: {
    label: `${C}.USER_GROUP_CONTAINS`,
    group: SHOPPER,
    parts: [`${C}.USER_GROUP_CONTAINS_TEXT`, { kind: "text", path: "group", placeholder: `${C}.USER_GROUP_PLACEHOLDER` }],
  },
  UserGroupIsCondition: {
    label: `${C}.USER_GROUP_IS`,
    group: SHOPPER,
    parts: [`${C}.USER_GROUP_IS_TEXT`, { kind: "lookup", path: "groups", source: "memberGroups", multiple: true }],
  },

  // ---- rewards
  BlockLoyaltyReward: {
    parts: [`${R}.TITLE`],
    addLabel: "LOYALTY.EXPRESSIONS.ADD_REWARD",
    validate: needsChild(`${E}.PROGRAM_REWARD`),
  },
  FixedAmountReward: { label: `${R}.FIXED_AMOUNT`, parts: [`${R}.GET`, num("amount"), `${R}.FIXED_POINTS`] },
  RelativeAmountReward: { label: `${R}.RELATIVE_AMOUNT`, parts: [`${R}.GET`, num("amount"), `${R}.RELATIVE_POINTS`] },

  // ---- mission goals
  BlockLoyaltyMissionGoals: {
    parts: [`${G}.TITLE`],
    addLabel: "LOYALTY.EXPRESSIONS.ADD_GOAL",
    validate(node) {
      const goals = (node.children ?? []).filter((x) => !!x.missionType).length;
      if (!goals) return `${E}.GOAL_REQUIRED`;
      return goals > 1 ? `${E}.GOAL_SINGLE` : undefined;
    },
  },
  OrderValueGoal: {
    label: `${G}.ORDER_VALUE`,
    group: GOAL,
    parts: [`${G}.ORDER_VALUE_TEXT`, num("value"), { kind: "lookup", path: "currencyCode", source: "currencies" }, `${G}.PER_PERIOD`],
    validate: (node) => (node.currencyCode ? undefined : `${E}.GOAL_CURRENCY`),
  },
  OrderCountGoal: { label: `${G}.ORDER_COUNT`, group: GOAL, parts: [`${G}.REACH`, int("count"), `${G}.ORDERS_PER_PERIOD`] },
  PerSkuGoal: { label: `${G}.PER_SKU`, group: GOAL, parts: [`${G}.PURCHASE`, allAny, `${G}.OF_TARGET_SKUS`, { kind: "goalItems" }] },
};

const SKIP = new Set(["id", "availableChildren", "children"]);
const humanize = (id: string) => id.replace(/([a-z])([A-Z])/g, "$1 $2");

// Any node the table doesn't know (e.g. registered by another module): its id plus an editor per primitive property
// (the id and property names aren't keys: $t shows them as they are)
export function defOf(node: TreeNode): Def {
  const id = String(node.id ?? "");
  const known = defs[id];
  if (known) return known;
  const parts: Part[] = [humanize(id)];
  for (const [key, value] of Object.entries(node)) {
    if (SKIP.has(key)) continue;
    if (typeof value === "number") parts.push(key, { kind: "number", path: key });
    else if (typeof value === "boolean") parts.push(key, { kind: "choice", path: key, bool: true, options: opts(["true", "LOYALTY.EXPRESSIONS.YES"], ["false", "LOYALTY.EXPRESSIONS.NO"]) });
    else if (typeof value === "string" || value === null) parts.push({ kind: "text", path: key, placeholder: key });
  }
  return { label: humanize(id), parts };
}

// every validation message (key) of the tree, as the AngularJS mission blade checked before saving
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
