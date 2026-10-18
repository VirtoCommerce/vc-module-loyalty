<template>
  <VcBlade
    :title="$t('LOYALTY.MENU.MISSIONS')"
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
      state-key="LOYALTY_MISSIONS"
      :active-item-id="activeChildParam"
      :show-all-columns="!childOpen"
      :row-actions="rowActions"
      row-actions-mode="dropdown"
      :empty-state="{ icon: 'lucide-trophy', title: $t('LOYALTY.MISSIONS.EMPTY') }"
      @row-click="(e: { data: LoyaltyMission }) => openDetails(e.data.id)"
      @pagination-click="pagination.goToPage"
    >
      <VcColumn
        id="name"
        :title="$t('LOYALTY.FIELDS.NAME')"
        sortable
        always-visible
        mobile-role="title"
      />
      <VcColumn
        id="storeId"
        :title="$t('LOYALTY.FIELDS.STORE')"
        sortable
        mobile-role="field"
      />
      <VcColumn
        id="startDate"
        :title="$t('LOYALTY.FIELDS.START_DATE')"
        type="date"
        sortable
      />
      <VcColumn
        id="endDate"
        :title="$t('LOYALTY.FIELDS.END_DATE')"
        type="date"
        sortable
      />
      <VcColumn
        id="status"
        :title="$t('LOYALTY.FIELDS.STATUS')"
        sortable
        always-visible
        mobile-role="status"
      >
        <template #body="{ data }">
          <div class="tw-inline-flex">
            <VcStatus :variant="missionStatusVariant(data.status)">{{ $t(missionStatusKey(data.status)) }}</VcStatus>
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
import { VcBlade, VcColumn, VcDataTable, VcStatus } from "@vc-shell/framework/ui";
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import type { LoyaltyMission } from "../../api_client/virtocommerce.loyalty";
import { missionStatusKey, missionsApi, useChildOpen } from "../api";

defineBlade({
  url: "/loyalty-missions",
  name: "LoyaltyMissions",
  isWorkspace: true,
  permissions: ["loyalty:access"],
  menuItem: {
    title: "LOYALTY.MENU.MISSIONS",
    icon: "lucide-trophy",
    priority: 41,
  },
});

const { t } = useI18n({ useScope: "global" });
const { activeChildParam, openBlade, exposeToChildren, closeChildren } = useBlade();
const { debounce } = useFunctions();
const { showConfirmation } = usePopup();
const { hasAccess } = usePermissions();

const PAGE_SIZE = 20;
const STATE = "LOYALTY_MISSIONS";
const items = ref<LoyaltyMission[]>([]);
const totalCount = ref(0);
const selection = ref<LoyaltyMission[]>([]);
// columns collapse while a mission (or a new one) is open
const { childOpen, track } = useChildOpen();

const { sortField, sortOrder, sortExpression } = useDataTableSort({ stateKey: STATE, initialField: "createdDate", initialDirection: "DESC" });
const { searchValue } = useTableSearch({ stateKey: STATE });
const pagination = useDataTablePagination({ stateKey: STATE, pageSize: PAGE_SIZE, totalCount });

function missionStatusVariant(status?: string) {
  if (status === "Published") return "success";
  if (status === "Archived") return "danger";
  return "info";
}

const openDetails = (id?: string) => openBlade({ name: "LoyaltyMissionDetails", param: id, onClose: track() });

const search = useLatestRequest();
const { loading: listLoading, action: load } = useAsync(async () => {
  const result = await search.latest(
    missionsApi.search({ keyword: searchValue.value || undefined, sort: sortExpression.value, skip: pagination.skip, take: PAGE_SIZE }),
  );
  if (!result) return;
  items.value = result.results ?? [];
  totalCount.value = result.totalCount ?? 0;
});

const { loading: deleteLoading, action: remove } = useAsync<string[]>((ids) => missionsApi.delete(ids));
const loading = useLoading(listLoading, deleteLoading);

load();
watch(searchValue, () => pagination.setPage(1));
watch([sortExpression, searchValue, () => pagination.skip], debounce(load, 300));

async function confirmDelete(list: LoyaltyMission[]) {
  if (!(await showConfirmation(t("LOYALTY.MISSIONS.DELETE_CONFIRM", { count: list.length })))) return;
  await closeChildren();
  await remove(list.map((x) => x.id!));
  selection.value = [];
  await load();
}

const rowActions = (item: LoyaltyMission) => [
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
  { id: "add", title: t("LOYALTY.ACTIONS.ADD"), icon: "lucide-plus", permissions: ["loyalty:create"], clickHandler: () => openDetails() },
  {
    id: "delete",
    title: t("LOYALTY.ACTIONS.DELETE"),
    icon: "lucide-trash-2",
    permissions: ["loyalty:delete"],
    disabled: computed(() => !selection.value.length),
    clickHandler: () => confirmDelete(selection.value),
  },
]);

exposeToChildren({ reload: () => load() });
</script>
