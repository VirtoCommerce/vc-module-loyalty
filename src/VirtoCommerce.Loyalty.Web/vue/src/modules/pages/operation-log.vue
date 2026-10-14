<template>
  <!-- Points earned and redeemed by a customer or an organization (the AngularJS loyalty-program-operation-log-list) -->
  <VcBlade
    :title="$t('LOYALTY.LOG.TITLE')"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="50%"
  >
    <VcDataTable
      v-model:sort-field="sortField"
      v-model:sort-order="sortOrder"
      :items="items"
      :total-count="totalCount"
      :pagination="pagination"
      state-key="LOYALTY_OPERATION_LOG"
      :active-item-id="activeItemId"
      :empty-state="{ icon: 'lucide-star', title: $t('LOYALTY.LOG.EMPTY') }"
      @row-click="(e: { data: LoyaltyBalanceOperationLog }) => open(e.data)"
      @pagination-click="pagination.goToPage"
    >
      <VcColumn
        id="operationType"
        :title="$t('LOYALTY.LOG.OPERATION')"
        sortable
        always-visible
        mobile-role="status"
      >
        <template #body="{ data }">
          <div class="tw-inline-flex">
            <VcStatus :variant="data.operationType === 'Redeemed' ? 'warning' : 'success'">{{
              $t(data.operationType === "Redeemed" ? "LOYALTY.LOG.REDEEMED" : "LOYALTY.LOG.EARNED")
            }}</VcStatus>
          </div>
        </template>
      </VcColumn>
      <VcColumn
        id="amount"
        :title="$t('LOYALTY.LOG.AMOUNT')"
        type="number"
        sortable
        always-visible
        mobile-role="title"
      />
      <VcColumn
        id="balance"
        :title="$t('LOYALTY.LOG.BALANCE')"
        type="number"
        sortable
        always-visible
        mobile-role="field"
      />
      <VcColumn
        id="createdDate"
        :title="$t('LOYALTY.LOG.CREATED')"
        type="datetime"
        sortable
        mobile-role="field"
      />
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import { useAsync, useBlade, useDataTablePagination, useDataTableSort, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable, VcStatus } from "@vc-shell/framework/ui";
import { onMounted, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import type { LoyaltyBalanceOperationLog } from "../../api_client/virtocommerce.loyalty";
import { balanceApi } from "../api";

defineBlade({ url: "/loyalty-log", name: "LoyaltyOperationLog", permissions: ["loyalty:read"] });

// options: whose operations — a customer's account (userId) or an organization
const { t } = useI18n({ useScope: "global" });
const { options, openBlade } = useBlade<{ userId?: string; organizationId?: string }>();

const PAGE_SIZE = 20;
const STATE = "LOYALTY_OPERATION_LOG";
const items = ref<LoyaltyBalanceOperationLog[]>([]);
const totalCount = ref(0);
const activeItemId = ref<string>();
const { sortField, sortOrder, sortExpression } = useDataTableSort({ stateKey: STATE, initialField: "createdDate", initialDirection: "DESC" });
const pagination = useDataTablePagination({ stateKey: STATE, pageSize: PAGE_SIZE, totalCount });

const { loading, action: load } = useAsync(async () => {
  const r = await balanceApi.search({
    userId: options.value?.userId,
    organizationId: options.value?.organizationId,
    sort: sortExpression.value,
    skip: pagination.skip,
    take: PAGE_SIZE,
  });
  items.value = r.results ?? [];
  totalCount.value = r.totalCount ?? 0;
});

onMounted(load);
watch([sortExpression, () => pagination.skip], () => load());

// the operation's source: the mission or program that awarded the points, otherwise the order
function sourceOf(log: LoyaltyBalanceOperationLog) {
  if (log.sourceType === "LoyaltyMission") return { name: "LoyaltyMissionDetails", param: log.sourceId };
  if (log.sourceType === "LoyaltyProgram") return { name: "LoyaltyProgramDetails", param: log.sourceId };
  if (log.objectType === "CustomerOrder") return { name: "OrdersDetails", param: log.objectId };
  return undefined;
}

function open(log: LoyaltyBalanceOperationLog) {
  const target = sourceOf(log);
  if (!target?.param) return;
  activeItemId.value = log.id;
  openBlade({ ...target, onClose: () => (activeItemId.value = undefined) });
}

const bladeToolbar = ref<IBladeToolbar[]>([{ id: "refresh", title: t("LOYALTY.ACTIONS.REFRESH"), icon: "lucide-refresh-cw", clickHandler: () => load() }]);
</script>
