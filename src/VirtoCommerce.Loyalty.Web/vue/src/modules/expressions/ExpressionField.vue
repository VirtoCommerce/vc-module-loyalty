<template>
  <!-- number -->
  <VcInput
    v-if="f.kind === 'number'"
    class="ly-field ly-field--num"
    size="small"
    :type="f.int ? 'integer' : 'number'"
    :aria-label="f.path"
    :model-value="node[f.path] as number"
    :disabled="disabled"
    @update:model-value="(v) => (node[f.path] = toNumber(v))"
  />

  <!-- text -->
  <VcInput
    v-else-if="f.kind === 'text'"
    class="ly-field ly-field--text"
    size="small"
    :aria-label="f.placeholder ? $t(f.placeholder) : f.path"
    :placeholder="f.placeholder ? $t(f.placeholder) : undefined"
    :model-value="(node[f.path] as string) ?? ''"
    :disabled="disabled"
    @update:model-value="(v) => (node[f.path] = v)"
  />

  <!-- one of a fixed list -->
  <VcSelect
    v-else-if="f.kind === 'choice'"
    class="ly-field ly-field--choice"
    size="small"
    :options="choices"
    option-value="value"
    option-label="label"
    :clearable="false"
    :aria-label="f.path"
    :model-value="f.bool ? String(!!node[f.path]) : (node[f.path] as string)"
    :disabled="disabled"
    @update:model-value="(v) => (node[f.path] = f.bool ? v === 'true' : v)"
  />

  <!-- compare condition + value (+ second value for "between") -->
  <template v-else-if="f.kind === 'range'">
    <VcSelect
      class="ly-field ly-field--choice"
      size="small"
      :options="compares"
      option-value="value"
      option-label="label"
      :clearable="false"
      aria-label="compareCondition"
      :model-value="node.compareCondition as string"
      :disabled="disabled"
      @update:model-value="onCompare"
    />
    <VcInput
      class="ly-field ly-field--num"
      size="small"
      type="number"
      :aria-label="f.path"
      :model-value="node[f.path] as number"
      :disabled="disabled"
      @update:model-value="(v) => (node[f.path] = toNumber(v))"
    />
    <template v-if="node.compareCondition === 'Between'">
      <span>{{ $t("LOYALTY.EXPRESSIONS.AND") }}</span>
      <VcInput
        class="ly-field ly-field--num"
        size="small"
        type="number"
        :aria-label="f.second"
        :model-value="node[f.second] as number"
        :disabled="disabled"
        @update:model-value="(v) => (node[f.second] = toNumber(v))"
      />
    </template>
  </template>

  <!-- dictionaries: currencies, order statuses, user groups -->
  <VcSelect
    v-else-if="f.kind === 'lookup'"
    class="ly-field"
    :class="f.multiple ? 'ly-field--wide' : 'ly-field--lookup'"
    size="small"
    searchable
    :multiple="f.multiple"
    :options="lookupOptions"
    option-value="id"
    option-label="name"
    :aria-label="f.path"
    :placeholder="$t(`LOYALTY.EXPRESSIONS.SELECT.${f.source.toUpperCase()}`)"
    :model-value="(node[f.path] as string | string[] | null) ?? (f.multiple ? [] : null)"
    :disabled="disabled"
    @update:model-value="(v) => (node[f.path] = f.multiple && !(v as string[])?.length ? null : (v ?? null))"
  />

  <!-- the PerSku goal's SKUs: a list of their own, editable once the mission is saved -->
  <template v-else-if="f.kind === 'goalItems'">
    <VcButton
      v-if="goalItems?.canOpen.value"
      variant="link"
      size="sm"
      @click="goalItems.open()"
      >{{ $t("LOYALTY.EXPRESSIONS.SELECT_SKUS") }}</VcButton
    >
    <VcHint
      v-else
      class="ly-field--row"
      >{{ $t("LOYALTY.EXPRESSIONS.SKUS_AFTER_SAVE") }}</VcHint
    >
  </template>
</template>

<script setup lang="ts">
import { computed, inject } from "vue";
import { useI18n } from "vue-i18n";
import { VcButton, VcHint, VcInput, VcSelect } from "@vc-shell/framework/ui";
import { compareOptions, GoalItemsKey, type ChoiceOption, type Part, type TreeNode } from "./defs";
import { lookups, type LookupSource, type Option } from "../api";

type FieldPart = Exclude<Part, string>;

const props = defineProps<{ node: TreeNode; part: FieldPart; disabled?: boolean }>();
const goalItems = inject(GoalItemsKey, undefined);
const { t } = useI18n({ useScope: "global" });

// the template reads the part through one loose shape (each v-if branch only touches its own kind's keys)
type Loose = {
  kind: FieldPart["kind"];
  path: string;
  second: string;
  source: LookupSource;
  int?: boolean;
  bool?: boolean;
  multiple?: boolean;
  placeholder?: string;
  options?: ChoiceOption[];
};
const f = computed(() => props.part as unknown as Loose);

// option labels are i18n keys
const translate = (options: ChoiceOption[]) => options.map((x) => ({ ...x, label: t(x.label) }));
const choices = computed(() => translate(f.value.options ?? []));
const compares = computed(() => translate(compareOptions));

// the API stores non-nullable numbers: an emptied field is 0, as the AngularJS inputs required a value
const toNumber = (v: unknown) => (v === null || v === undefined || v === "" ? 0 : Number(v));

function onCompare(value: unknown) {
  props.node.compareCondition = value;
  // the AngularJS "between" started the range at the current value
  if (value === "Between") props.node[f.value.second] = props.node[f.value.path];
}

async function lookupOptions(keyword?: string) {
  const all = await lookups[f.value.source]();
  const k = keyword?.toLowerCase();
  const results = k ? all.filter((x: Option) => x.name.toLowerCase().includes(k)) : all;
  return { results, totalCount: results.length };
}
</script>

<style lang="scss">
.ly-field {
  flex: 0 0 auto;
  /* phones: a wide field never overflows the sentence */
  max-width: 100%;
  &--num {
    width: 96px;
  }
  &--text {
    width: 180px;
  }
  &--choice {
    width: 168px;
  }
  &--lookup {
    width: 160px;
  }
  &--wide {
    width: 320px;
  }
  &--row {
    flex-basis: 100%;
  }
}
</style>
