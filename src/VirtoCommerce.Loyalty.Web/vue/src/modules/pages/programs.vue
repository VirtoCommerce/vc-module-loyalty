<template>
  <VcBlade
    :title="$t('LOYALTY.PROGRAMS.TITLE')"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="50%"
  >
    <VcDataTable
      v-model:search-value="searchValue"
      v-model:sort-field="sortField"
      v-model:sort-order="sortOrder"
      v-model:selection="selection"
      selection-mode="multiple"
      :items="items"
      :total-count="totalCount"
      :pagination="pagination"
      :searchable="true"
      state-key="LOYALTY_PROGRAMS"
      :active-item-id="activeChildParam"
      :show-all-columns="!childOpen"
      :row-actions="rowActions"
      row-actions-mode="dropdown"
      :empty-state="{ icon: 'lucide-star', title: $t('LOYALTY.PROGRAMS.EMPTY') }"
      @row-click="(e: { data: LoyaltyProgram }) => openDetails(e.data.id)"
      @pagination-click="pagination.goToPage"
    >
      <!-- saved filters (the AngularJS filter dropdown): the user's named filters and "new filter" -->
      <template #search-header-actions>
        <div class="ly-saved-filter">
          <VcSelect
            :model-value="currentId"
            :options="savedFilterOptions"
            :placeholder="$t('LOYALTY.FILTER.SELECT')"
            :aria-label="$t('LOYALTY.FILTER.SELECT')"
            size="small"
            @update:model-value="onSavedFilterPicked"
          />
          <VcButton
            v-if="currentFilter"
            icon="lucide-pencil"
            variant="ghost"
            size="sm"
            :aria-label="$t('LOYALTY.FILTER.EDIT')"
            @click="openFilter(currentFilter.id)"
          />
        </div>
      </template>
      <VcColumn
        id="name"
        :title="$t('LOYALTY.FIELDS.NAME')"
        sortable
        always-visible
        mobile-role="title"
      />
      <VcColumn
        id="programType"
        :title="$t('LOYALTY.FIELDS.TYPE')"
        sortable
        mobile-role="field"
      >
        <template #body="{ data }">{{ typeName(data.programType) }}</template>
      </VcColumn>
      <VcColumn
        id="storeId"
        :title="$t('LOYALTY.FIELDS.STORE')"
        sortable
        mobile-role="field"
      />
      <VcColumn
        id="isActive"
        :title="$t('LOYALTY.FIELDS.STATUS')"
        sortable
        always-visible
        mobile-role="status"
      >
        <template #body="{ data }">
          <div class="tw-inline-flex">
            <VcStatus :variant="data.isActive ? 'success' : 'info'">{{
              $t(data.isActive ? "LOYALTY.STATUS.ACTIVE" : "LOYALTY.STATUS.INACTIVE")
            }}</VcStatus>
          </div>
        </template>
      </VcColumn>
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import {
  useAsync,
  useBlade,
  useDataTablePagination,
  useDataTableSort,
  useFunctions,
  useLatestRequest,
  useLoading,
  usePermissions,
  usePopup,
  useTableSearch,
  type IBladeToolbar,
} from "@vc-shell/framework";
import { VcBlade, VcButton, VcColumn, VcDataTable, VcSelect, VcStatus } from "@vc-shell/framework/ui";
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import type { LoyaltyProgram } from "../../api_client/virtocommerce.loyalty";
import { programsApi, useChildOpen } from "../api";
import { useSavedFilters } from "../savedFilters";

defineBlade({
  url: "/loyalty",
  name: "LoyaltyPrograms",
  isWorkspace: true,
  permissions: ["loyalty:access"],
  menuItem: {
    title: "LOYALTY.MENU.PROGRAMS",
    icon: "lucide-star",
    priority: 40,
  },
});

const { t } = useI18n({ useScope: "global" });
const { activeChildParam, openBlade, exposeToChildren, closeChildren } = useBlade();
const { debounce } = useFunctions();
const { showConfirmation } = usePopup();
const { hasAccess } = usePermissions();

const PAGE_SIZE = 20;
const STATE = "LOYALTY_PROGRAMS";
const items = ref<LoyaltyProgram[]>([]);
const totalCount = ref(0);
const selection = ref<LoyaltyProgram[]>([]);
// columns collapse while a child blade (details, new program, filter) is open
const { childOpen, track } = useChildOpen();

const { sortField, sortOrder, sortExpression } = useDataTableSort({ stateKey: STATE, initialField: "createdDate", initialDirection: "DESC" });
const { searchValue } = useTableSearch({ stateKey: STATE });
const pagination = useDataTablePagination({ stateKey: STATE, pageSize: PAGE_SIZE, totalCount });

const typeName = (type?: string) => t(type === "ProductPoints" ? "LOYALTY.PROGRAM_TYPES.PRODUCT_POINTS" : "LOYALTY.PROGRAM_TYPES.DEFAULT");

// saved filters
const { filters: savedFilters, currentId, find } = useSavedFilters();
const NEW_FILTER = "__new";
const currentFilter = computed(() => find(currentId.value));
const savedFilterOptions = computed(() => [
  ...savedFilters.value.map((f) => ({ id: f.id, title: f.name })),
  { id: NEW_FILTER, title: t("LOYALTY.FILTER.NEW") },
]);

function onSavedFilterPicked(id: unknown) {
  if (id === NEW_FILTER) return openFilter();
  currentId.value = (id as string) || undefined;
  reloadFirstPage();
}

const openFilter = (filterId?: string) => openBlade({ name: "LoyaltyProgramFilter", options: { filterId }, onClose: track() });
const openDetails = (id?: string) => openBlade({ name: "LoyaltyProgramDetails", param: id, onClose: track() });

function reloadFirstPage() {
  if (pagination.skip) pagination.setPage(1);
  else load();
}

const search = useLatestRequest();
const { loading: listLoading, action: load } = useAsync(async () => {
  const filter = currentFilter.value;
  const result = await search.latest(
    programsApi.search({
      keyword: searchValue.value || undefined,
      sort: sortExpression.value,
      skip: pagination.skip,
      take: PAGE_SIZE,
      onlyActive: filter?.onlyActive || undefined,
      storeIds: filter?.storeIds?.length ? filter.storeIds : undefined,
    }),
  );
  if (!result) return;
  items.value = result.results ?? [];
  totalCount.value = result.totalCount ?? 0;
});

const { loading: deleteLoading, action: remove } = useAsync<string[]>((ids) => programsApi.delete(ids));
const loading = useLoading(listLoading, deleteLoading);

load();
watch(searchValue, () => pagination.setPage(1));
watch([sortExpression, searchValue, () => pagination.skip], debounce(load, 300));

async function confirmDelete(list: LoyaltyProgram[]) {
  if (!(await showConfirmation(t("LOYALTY.PROGRAMS.DELETE_CONFIRM", { count: list.length })))) return;
  await closeChildren();
  await remove(list.map((x) => x.id!));
  selection.value = [];
  await load();
}

// the AngularJS row context menu: delete
const rowActions = (item: LoyaltyProgram) => [
  {
    id: "delete",
    icon: "lucide-trash-2",
    title: t("LOYALTY.ACTIONS.DELETE"),
    type: "danger" as const,
    variant: "danger" as const,
    hidden: !hasAccess("loyalty:delete"),
    clickHandler: () => confirmDelete([item]),
  },
];

const bladeToolbar = ref<IBladeToolbar[]>([
  { id: "refresh", title: t("LOYALTY.ACTIONS.REFRESH"), icon: "lucide-refresh-cw", clickHandler: () => load() },
  {
    id: "add",
    title: t("LOYALTY.ACTIONS.ADD"),
    icon: "lucide-plus",
    permissions: ["loyalty:create"],
    clickHandler: () => openBlade({ name: "LoyaltyProgramAdd", onClose: track() }),
  },
  {
    id: "delete",
    title: t("LOYALTY.ACTIONS.DELETE"),
    icon: "lucide-trash-2",
    permissions: ["loyalty:delete"],
    disabled: computed(() => !selection.value.length),
    clickHandler: () => confirmDelete(selection.value),
  },
]);

// the type picker opens the new program here, in its place
const openNew = (programType: string) => openBlade({ name: "LoyaltyProgramDetails", options: { programType }, onClose: track() });

exposeToChildren({ reload: () => load(), reloadFirstPage, openNew });
</script>

<style lang="scss">
.ly-saved-filter {
  display: flex;
  align-items: center;
  gap: 4px;

  .vc-select {
    width: 200px;
  }
}
</style>
