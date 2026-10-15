<template>
  <VcBlade
    :title="isNew ? $t('LOYALTY.FILTER.TITLE') : filter.name"
    :toolbar-items="bladeToolbar"
    width="40%"
  >
    <VcContainer>
      <VcForm class="ly-form tw-space-y-4">
        <VcInput
          v-model="filter.name"
          :label="$t('LOYALTY.FILTER.NAME')"
          :error-message="filter.name?.trim() ? undefined : $t('LOYALTY.VALIDATION.REQUIRED')"
          required
        />
        <VcSwitch
          v-model="filter.onlyActive"
          :label="$t('LOYALTY.FILTER.ONLY_ACTIVE')"
        />
        <VcSelect
          v-model="filter.storeIds"
          :label="$t('LOYALTY.FILTER.STORES')"
          :placeholder="$t('LOYALTY.FILTER.ALL_STORES')"
          :options="searchStores"
          option-value="id"
          option-label="name"
          multiple
          searchable
        />
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
// filter-detail: a named, saved loyalty program filter (stored in this browser, like the AngularJS $localStorage filters)
import { useBlade, useBladeForm, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcContainer, VcForm, VcInput, VcSelect, VcSwitch } from "@vc-shell/framework/ui";
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import { searchStores } from "../api";
import { useSavedFilters, type SavedProgramFilter } from "../savedFilters";

defineBlade({ url: "/loyalty-filter", name: "LoyaltyProgramFilter", permissions: ["loyalty:access"] });

const { t } = useI18n({ useScope: "global" });
const { options, callParent, closeSelf } = useBlade<{ filterId?: string }>();
const { filters, currentId, find } = useSavedFilters();

const copy = (f: SavedProgramFilter) => JSON.parse(JSON.stringify(f)) as SavedProgramFilter;
const initial = find(options.value?.filterId);
const filter = ref<SavedProgramFilter>(initial ? copy(initial) : { id: String(Date.now()), name: t("LOYALTY.FILTER.UNNAMED") });
const isNew = computed(() => !find(filter.value.id));

const form = useBladeForm({ data: filter });
form.setBaseline();

const bladeToolbar = ref<IBladeToolbar[]>([
  {
    id: "apply",
    title: t("LOYALTY.FILTER.APPLY"),
    icon: "lucide-filter",
    disabled: computed(() => !filter.value.name?.trim()),
    async clickHandler() {
      const value = copy({ ...filter.value, onlyActive: filter.value.onlyActive || undefined, storeIds: filter.value.storeIds?.length ? filter.value.storeIds : undefined });
      const index = filters.value.findIndex((f) => f.id === value.id);
      if (index >= 0) filters.value.splice(index, 1, value);
      else filters.value.push(value);
      currentId.value = value.id;
      form.setBaseline();
      await callParent("reloadFirstPage");
    },
  },
  {
    id: "reset",
    title: t("LOYALTY.ACTIONS.RESET"),
    icon: "lucide-undo-2",
    disabled: computed(() => !form.isModified.value),
    clickHandler: () => form.revert(),
  },
  {
    id: "delete",
    title: t("LOYALTY.ACTIONS.DELETE"),
    icon: "lucide-trash-2",
    isVisible: computed(() => !isNew.value),
    async clickHandler() {
      filters.value = filters.value.filter((f) => f.id !== filter.value.id);
      if (currentId.value === filter.value.id) currentId.value = undefined;
      form.setBaseline();
      await callParent("reloadFirstPage");
      await closeSelf();
    },
  },
]);
</script>
