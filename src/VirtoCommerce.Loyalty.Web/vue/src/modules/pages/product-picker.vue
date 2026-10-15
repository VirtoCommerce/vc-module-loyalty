<template>
  <!-- Replaces the legacy Catalog "select items" blade: keyword search over the products of the store's catalog -->
  <VcBlade
    :title="$t('LOYALTY.PICKER.TITLE')"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="50%"
  >
    <VcDataTable
      v-model:search-value="searchValue"
      v-model:selection="selection"
      selection-mode="multiple"
      :items="items"
      :total-count="totalCount"
      :pagination="pagination"
      :searchable="true"
      state-key="LOYALTY_PRODUCT_PICKER"
      :empty-state="{ icon: 'lucide-package', title: $t('LOYALTY.PICKER.EMPTY') }"
      @pagination-click="pagination.goToPage"
    >
      <VcColumn
        id="imageUrl"
        :title="$t('LOYALTY.PICKER.IMAGE')"
        type="image"
        width="80px"
        mobile-role="image"
      />
      <VcColumn
        id="name"
        :title="$t('LOYALTY.ITEMS.PRODUCT')"
        always-visible
        mobile-role="title"
      />
      <VcColumn
        id="code"
        :title="$t('LOYALTY.ITEMS.SKU')"
        always-visible
        mobile-role="field"
      />
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import { useAsync, useBlade, useDataTablePagination, useFunctions, useTableSearch, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable } from "@vc-shell/framework/ui";
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { searchProducts, type ProductEntry } from "../api";

defineBlade({ url: "/loyalty-product-picker", name: "LoyaltyProductPicker", permissions: ["loyalty:update"] });

const PAGE_SIZE = 20;
const { t } = useI18n({ useScope: "global" });
const { options, callParent, closeSelf } = useBlade<{ catalogId?: string }>();
const { debounce } = useFunctions();

const items = ref<ProductEntry[]>([]);
const totalCount = ref(0);
const selection = ref<ProductEntry[]>([]);
const { searchValue } = useTableSearch({ stateKey: "LOYALTY_PRODUCT_PICKER" });
const pagination = useDataTablePagination({ stateKey: "LOYALTY_PRODUCT_PICKER", pageSize: PAGE_SIZE, totalCount });

const { loading, action: load } = useAsync(async () => {
  const r = await searchProducts(options.value?.catalogId, searchValue.value || undefined, pagination.skip, PAGE_SIZE);
  items.value = r.results;
  totalCount.value = r.totalCount;
});

load();
watch(searchValue, () => pagination.setPage(1));
watch([searchValue, () => pagination.skip], debounce(load, 300));

const bladeToolbar = ref<IBladeToolbar[]>([
  {
    id: "add",
    title: t("LOYALTY.PICKER.ADD_SELECTED"),
    icon: "lucide-plus",
    disabled: computed(() => !selection.value.length),
    async clickHandler() {
      await callParent("addProducts", selection.value);
      await closeSelf();
    },
  },
]);
</script>
