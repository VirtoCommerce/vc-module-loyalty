<template>
  <!-- The product lists with an editable number per product (the AngularJS loyalty-program-product-factor-list,
       product-loyalty-program-list and loyalty-mission-goal-item-list blades):
       factors of a product-points program, the programs of a product, or the target SKUs of a PerSku mission. -->
  <VcBlade
    :title="title"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="50%"
  >
    <VcBanner
      v-if="error"
      class="ly-items-error"
      variant="danger"
      icon="lucide-circle-alert"
      >{{ error }}</VcBanner
    >
    <VcDataTable
      v-model:selection="selection"
      selection-mode="multiple"
      :items="rows"
      :total-count="totalCount"
      :pagination="pagination"
      edit-mode="inline"
      :state-key="stateKey"
      :empty-state="{ icon: 'lucide-package', title: $t('LOYALTY.ITEMS.EMPTY') }"
      @cell-edit-complete="onEdit"
      @pagination-click="pagination.goToPage"
    >
      <VcColumn
        v-if="byProduct"
        id="programName"
        :title="$t('LOYALTY.ITEMS.PROGRAM')"
        always-visible
        mobile-role="title"
      />
      <template v-else>
        <VcColumn
          id="productName"
          :title="$t('LOYALTY.ITEMS.PRODUCT')"
          always-visible
          mobile-role="title"
        >
          <template #body="{ data }">
            <div class="ly-items__name">
              <span>{{ data.productName }}</span>
              <div
                v-if="!data.id"
                class="tw-inline-flex"
              >
                <VcStatus variant="info">{{ $t("LOYALTY.ITEMS.NEW") }}</VcStatus>
              </div>
            </div>
          </template>
        </VcColumn>
        <VcColumn
          id="productCode"
          :title="$t('LOYALTY.ITEMS.SKU')"
          width="110px"
          mobile-role="field"
        />
      </template>
      <VcColumn
        :id="valueField"
        :title="$t(goals ? 'LOYALTY.ITEMS.QUANTITY' : 'LOYALTY.ITEMS.FACTOR')"
        type="number"
        width="110px"
        :editable="!readonly"
        always-visible
        mobile-role="field"
      />
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import { useAsync, useBlade, useBladeForm, useDataTablePagination, useLoading, usePermissions, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { VcBanner, VcBlade, VcColumn, VcDataTable, VcStatus } from "@vc-shell/framework/ui";
import { computed, onMounted, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { factorsApi, goalItemsApi, storeCatalogId, type ProductEntry } from "../api";

defineBlade({ url: "/loyalty-items", name: "LoyaltyProductItems", permissions: ["loyalty:read"] });

interface Options {
  kind: "factors" | "goals";
  /** the program (factors) or mission (goals) */
  parentId?: string;
  /** whose catalog products are added from */
  storeId?: string;
  /** factors of one product, across programs (the product blade's widget) */
  productId?: string;
}
type Row = {
  id?: string;
  productId?: string;
  productName?: string;
  productCode?: string;
  programName?: string;
  factor?: number;
  quantity?: number;
  loyaltyProgramId?: string;
  missionId?: string;
};

const { t } = useI18n({ useScope: "global" });
const { options, openBlade, callParent, exposeToChildren } = useBlade<Options>();
const { showConfirmation } = usePopup();
const { hasAccess } = usePermissions();

const opts = options.value!;
const goals = opts.kind === "goals";
const byProduct = !!opts.productId;
const valueField = goals ? "quantity" : "factor";
const parentKey = goals ? "missionId" : "loyaltyProgramId";
// the blade lists a mission's goals, a product's programs or a program's factors
function perList<T>(goalsValue: T, programsValue: T, factorsValue: T) {
  if (goals) return goalsValue;
  return byProduct ? programsValue : factorsValue;
}
const stateKey = perList("LOYALTY_GOAL_ITEMS", "LOYALTY_PRODUCT_PROGRAMS", "LOYALTY_PRODUCT_FACTORS");
const title = computed(() => t(perList("LOYALTY.ITEMS.GOALS_TITLE", "LOYALTY.ITEMS.PROGRAMS_TITLE", "LOYALTY.ITEMS.FACTORS_TITLE")));
const readonly = computed(() => !hasAccess("loyalty:update"));

const search = (criteria: { productIds?: string[]; skip?: number; take?: number }) =>
  goals
    ? goalItemsApi.search({ missionId: opts.parentId, ...criteria })
    : factorsApi.search({ loyaltyProgramId: opts.parentId, ...criteria, productIds: criteria.productIds ?? (byProduct ? [opts.productId!] : undefined) });

const PAGE_SIZE = 20;
const rows = ref<Row[]>([]);
const totalCount = ref(0);
const selection = ref<Row[]>([]);
const pagination = useDataTablePagination({ stateKey, pageSize: PAGE_SIZE, totalCount });
// saved values, to send only the changed rows (and every new one)
let saved = new Map<string, number>();

const form = useBladeForm({
  data: rows,
  canSaveOverride: computed(() => !error.value),
  closeConfirmMessage: computed(() => t("LOYALTY.ITEMS.CLOSE_CONFIRM")),
});

// factors can't be negative (the server rejects them); a goal needs at least one unit, as the AngularJS input required
const error = computed(() => {
  const bad = rows.value.find((r) => {
    const v = r[valueField];
    return v === undefined || v === null || Number.isNaN(v) || (goals ? !Number.isInteger(v) || v < 1 : v < 0);
  });
  if (!bad) return "";
  return t(goals ? "LOYALTY.ITEMS.INVALID_QUANTITY" : "LOYALTY.ITEMS.INVALID_FACTOR", { name: bad.productName ?? bad.programName });
});

const { loading: listLoading, action: load } = useAsync(async () => {
  const r = await search({ skip: pagination.skip, take: PAGE_SIZE });
  rows.value = (r.results ?? []) as Row[];
  totalCount.value = r.totalCount ?? 0;
  saved = new Map(rows.value.map((x) => [x.id!, Number(x[valueField])]));
  selection.value = [];
  form.setBaseline();
});

const modified = () => rows.value.filter((r) => !r.id || saved.get(r.id) !== Number(r[valueField]));

const { loading: saveLoading, action: save } = useAsync(async () => {
  const changed = modified().map((r) => ({ ...r, [valueField]: Number(r[valueField]) }));
  if (changed.length) await (goals ? goalItemsApi.updateItems(changed) : factorsApi.updateFactors(changed));
});

const { loading: deleteLoading, action: removeSaved } = useAsync<string[]>((ids) => (goals ? goalItemsApi.delete(ids) : factorsApi.delete(ids)));
const loading = useLoading(listLoading, saveLoading, deleteLoading);

function onEdit(e: { field: string; newValue: unknown; index: number }) {
  (rows.value[e.index] as Record<string, unknown>)[e.field] = e.newValue === "" || e.newValue === null ? undefined : Number(e.newValue);
}

// products are added from the store's catalog, as the AngularJS catalog item picker did
const catalogId = ref<string>();
onMounted(async () => {
  [catalogId.value] = await Promise.all([opts.storeId && !byProduct ? storeCatalogId(opts.storeId) : undefined, load()]);
});
watch(() => pagination.skip, () => load());

async function addProducts(products: ProductEntry[]) {
  // skip products already in the list: saved (on any page) or staged here
  const existing = await search({ productIds: products.map((p) => p.id), take: products.length });
  const present = new Set([...(existing.results ?? []).map((x) => x.productId), ...rows.value.map((x) => x.productId)]);
  const added = products
    .filter((p) => !present.has(p.id))
    .map((p) => ({ productId: p.id, productName: p.name, productCode: p.code, [valueField]: 1, [parentKey]: opts.parentId }) as Row);
  rows.value.push(...added);
  totalCount.value += added.length;
}

async function deleteSelected() {
  const list = selection.value;
  if (!(await showConfirmation(t(goals ? "LOYALTY.ITEMS.DELETE_GOALS_CONFIRM" : "LOYALTY.ITEMS.DELETE_FACTORS_CONFIRM", { count: list.length })))) return;
  const ids = list.map((x) => x.id).filter((x): x is string => !!x);
  if (!ids.length) {
    // only unsaved rows: just drop them
    rows.value = rows.value.filter((r) => !list.includes(r));
    totalCount.value -= list.length;
    selection.value = [];
    return;
  }
  await removeSaved(ids);
  await load();
  await callParent("reloadWidgets");
}

const bladeToolbar = ref<IBladeToolbar[]>([
  {
    id: "save",
    title: t("LOYALTY.ACTIONS.SAVE"),
    icon: "lucide-save",
    permissions: ["loyalty:update"],
    disabled: computed(() => !form.canSave.value || !modified().length),
    async clickHandler() {
      await save();
      await load();
      await callParent("reloadWidgets");
    },
  },
  { id: "refresh", title: t("LOYALTY.ACTIONS.REFRESH"), icon: "lucide-refresh-cw", clickHandler: () => load() },
  {
    id: "add",
    title: t("LOYALTY.ACTIONS.ADD"),
    icon: "lucide-plus",
    permissions: ["loyalty:update"],
    isVisible: !byProduct,
    disabled: computed(() => !catalogId.value),
    clickHandler: () => openBlade({ name: "LoyaltyProductPicker", options: { catalogId: catalogId.value } }),
  },
  {
    id: "delete",
    title: t("LOYALTY.ACTIONS.DELETE"),
    icon: "lucide-trash-2",
    permissions: ["loyalty:delete"],
    disabled: computed(() => !selection.value.length),
    clickHandler: deleteSelected,
  },
]);

exposeToChildren({ addProducts });
</script>

<style lang="scss">
.ly-items-error {
  margin: 12px 16px 0;
}

.ly-items__name {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 4px;
}
</style>
