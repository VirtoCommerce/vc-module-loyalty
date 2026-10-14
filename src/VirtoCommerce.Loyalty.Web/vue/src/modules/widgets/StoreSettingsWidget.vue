<template>
  <VcWidget
    icon="lucide-star"
    :title="$t('LOYALTY.WIDGETS.SETTINGS')"
    :disabled="!storeId"
    @click="open"
  />
</template>

<script setup lang="ts">
// loyalty-setting-widget on the Store module's blade (storeDetail); the store comes from defineBladeContext({ item })
import { computed } from "vue";
import { injectBladeContext, useBlade } from "@vc-shell/framework";
import { VcWidget } from "@vc-shell/framework/ui";

const ctx = injectBladeContext();
const storeId = computed(() => (ctx.value.item as { id?: string } | undefined)?.id);
const { openBlade } = useBlade();

const open = () => openBlade({ name: "LoyaltyStoreSettings", param: storeId.value });
</script>
