<template>
  <VcBlade
    :title="$t('LOYALTY.PROGRAM_ADD.TITLE')"
    width="30%"
  >
    <VcDataTable
      :items="choices"
      data-key="type"
      state-key="LOYALTY_PROGRAM_ADD"
      :column-switcher="false"
      @row-click="(e: { data: Choice }) => callParent('openNew', e.data.type)"
    >
      <VcColumn
        id="name"
        :title="$t('LOYALTY.PROGRAM_ADD.TYPE')"
        mobile-role="title"
      >
        <template #body="{ data: choice }">
          <div class="ly-choice">
            <span class="ly-choice__name">{{ choice.name }}</span>
            <span class="ly-choice__descr">{{ choice.description }}</span>
          </div>
        </template>
      </VcColumn>
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
// the AngularJS loyalty-program-add blade: pick the type of the new program
import { useBlade } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable } from "@vc-shell/framework/ui";
import { computed } from "vue";
import { useI18n } from "vue-i18n";

defineBlade({ name: "LoyaltyProgramAdd", permissions: ["loyalty:create"] });

interface Choice {
  type: string;
  name: string;
  description: string;
}

const { t } = useI18n({ useScope: "global" });
const { callParent } = useBlade();

const choices = computed<Choice[]>(() => [
  { type: "Default", name: t("LOYALTY.PROGRAM_TYPES.DEFAULT"), description: t("LOYALTY.PROGRAM_TYPES.DEFAULT_DESCR") },
  { type: "ProductPoints", name: t("LOYALTY.PROGRAM_TYPES.PRODUCT_POINTS"), description: t("LOYALTY.PROGRAM_TYPES.PRODUCT_POINTS_DESCR") },
]);
</script>

<style lang="scss">
.ly-choice {
  display: flex;
  flex-direction: column;
  gap: 2px;

  &__name {
    color: var(--color-neutral-900);
    font-weight: 500;
  }

  &__descr {
    color: var(--color-neutral-500);
    font-size: 13px;
  }
}
</style>
