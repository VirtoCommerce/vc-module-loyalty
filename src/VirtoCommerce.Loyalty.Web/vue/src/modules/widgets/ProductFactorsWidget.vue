<template>
  <VcWidget
    icon="lucide-star"
    :title="$t('LOYALTY.WIDGETS.PRODUCT_FACTORS')"
    :value="count ?? '…'"
    :disabled="!productId"
    @click="open"
  />
</template>

<script setup lang="ts">
// product-details-loyalty-factors-widget on the Catalog module's product blade (itemDetail):
// the loyalty programs that have a factor for the product
import { computed, ref, watch } from "vue";
import { injectBladeContext, useBlade, useWidgetTrigger } from "@vc-shell/framework";
import { VcWidget } from "@vc-shell/framework/ui";
import { factorsApi } from "../api";

const ctx = injectBladeContext();
const productId = computed(() => (ctx.value.item as { id?: string } | undefined)?.id);
const { openBlade } = useBlade();
const count = ref<number | string>();

async function load() {
  if (!productId.value) return (count.value = undefined);
  try {
    count.value = (await factorsApi.search({ productIds: [productId.value], take: 0 })).totalCount ?? 0;
  } catch {
    count.value = ""; // a failed lookup leaves the badge empty, not an error on the product's blade
  }
}
watch(productId, load, { immediate: true });
useWidgetTrigger({ onRefresh: load });

const open = () => openBlade({ name: "LoyaltyProductItems", options: { kind: "factors", productId: productId.value }, onClose: load });
</script>
