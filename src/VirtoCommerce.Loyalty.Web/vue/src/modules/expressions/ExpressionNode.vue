<template>
  <div
    class="ly-expr"
    :class="depth === 0 ? 'ly-expr--block' : 'ly-expr--item'"
    :data-expression="node.id"
  >
    <div class="ly-expr__head">
      <div class="ly-expr__line">
        <template
          v-for="(part, i) in def.parts"
          :key="i"
        >
          <span
            v-if="typeof part === 'string'"
            class="ly-expr__text"
            >{{ $t(part) }}</span
          >
          <ExpressionField
            v-else
            :node="node"
            :part="part"
            :disabled="disabled"
          />
        </template>
      </div>
      <VcButton
        v-if="removable && !disabled"
        class="ly-expr__remove"
        variant="ghost"
        size="icon-sm"
        icon="lucide-x"
        :aria-label="$t('LOYALTY.EXPRESSIONS.REMOVE')"
        @click="$emit('remove')"
      />
    </div>

    <div
      v-if="children.length"
      class="ly-expr__children"
    >
      <ExpressionNode
        v-for="(child, i) in children"
        :key="keyOf(child)"
        :node="child"
        :depth="depth + 1"
        :disabled="disabled"
        removable
        @remove="node.children!.splice(i, 1)"
      />
    </div>

    <VcHint
      v-if="error"
      class="ly-expr__error"
      error
      >{{ $t(error) }}</VcHint
    >

    <VcDropdown
      v-if="addable.length && !disabled"
      v-model="menuOpen"
      :items="addable"
      :item-text="(x: AddItem) => x.label"
      floating
      close-on-select
      max-height="360"
      @item-click="add"
    >
      <template #trigger="{ toggle }">
        <VcButton
          class="ly-expr__add"
          variant="link"
          size="sm"
          icon="lucide-plus"
          @click="toggle"
          >{{ $t(def.addLabel || "LOYALTY.EXPRESSIONS.ADD") }}</VcButton
        >
      </template>
    </VcDropdown>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import { VcButton, VcDropdown, VcHint } from "@vc-shell/framework/ui";
import ExpressionField from "./ExpressionField.vue";
import { defOf, type TreeNode } from "./defs";

interface AddItem {
  label: string;
  prototype: TreeNode;
}

defineOptions({ name: "ExpressionNode" });
const props = defineProps<{ node: TreeNode; depth?: number; removable?: boolean; disabled?: boolean }>();
defineEmits<{ remove: [] }>();
const { t } = useI18n({ useScope: "global" });

const depth = computed(() => props.depth ?? 0);
const def = computed(() => defOf(props.node));
const children = computed(() => props.node.children ?? []);
const error = computed(() => def.value.validate?.(props.node));

// the "Add ..." menu lists the prototypes the API sends with the node (availableChildren), grouped as the AngularJS menu
const addable = computed<AddItem[]>(() =>
  (props.node.availableChildren ?? [])
    .map((prototype) => {
      const d = defOf(prototype);
      const label = d.label ? t(d.label) : String(prototype.id);
      return { label: d.group ? `${t(d.group)}: ${label}` : label, prototype };
    })
    .sort((a, b) => a.label.localeCompare(b.label)),
);
const menuOpen = ref(false);

// a new child is a copy of the server's prototype, so it carries exactly the server's field set
function add(item: AddItem) {
  props.node.children ??= [];
  props.node.children.push(JSON.parse(JSON.stringify(item.prototype)) as TreeNode);
}

// stable keys without writing anything onto the API objects
const keys = new WeakMap<object, number>();
let seq = 0;
const keyOf = (o: object) => keys.get(o) ?? (keys.set(o, ++seq), seq);
</script>

<style lang="scss">
.ly-expr {
  font-size: 14px;
  line-height: 20px;
  color: var(--color-neutral-700);

  &--block {
    padding: 14px 16px;
    & + & {
      border-top: 1px solid var(--color-neutral-200);
    }
    > .ly-expr__head .ly-expr__text {
      font-weight: 600;
      color: var(--color-neutral-900);
    }
  }

  /* the sentence wraps; the remove button stays at the end of its first line */
  &__head {
    display: flex;
    align-items: flex-start;
    gap: 8px;
  }

  &__line {
    flex: 1 1 auto;
    min-width: 0;
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 6px 8px;
    min-height: 32px;
  }

  &__children {
    margin: 8px 0 4px 6px;
    padding-left: 14px;
    border-left: 2px solid var(--color-neutral-200);
    display: flex;
    flex-direction: column;
    gap: 8px;
  }

  &__remove {
    flex: none;
    margin-top: 4px;
  }

  &__error {
    margin-top: 6px;
  }

  &__add {
    margin-top: 6px;
  }
}
</style>
