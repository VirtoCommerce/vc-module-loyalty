<template>
  <VcWidget
    icon="lucide-star"
    :title="$t('LOYALTY.WIDGETS.BALANCE')"
    :value="balance ?? '…'"
    :disabled="!criteria"
    @click="open"
  />
</template>

<script setup lang="ts">
// customer-loyalty-widget / organization-loyalty-widget on the Customer module's contact and organization blades:
// the points balance of the contact's (first) account, or of the organization
import { computed, ref, watch } from "vue";
import { injectBladeContext, useBlade, useWidgetTrigger } from "@vc-shell/framework";
import { VcWidget } from "@vc-shell/framework/ui";
import { balanceApi } from "../api";

interface Member {
  id?: string;
  memberType?: string;
  securityAccounts?: { id?: string }[];
}

const ctx = injectBladeContext();
const { openBlade } = useBlade();
const balance = ref<number | string>();

const criteria = computed(() => {
  const member = ctx.value.item as Member | undefined;
  if (!member?.id) return undefined;
  if (member.memberType === "Organization") return { organizationId: member.id };
  const userId = member.securityAccounts?.[0]?.id;
  return userId ? { userId } : undefined;
});

async function load() {
  const c = criteria.value;
  balance.value = 0;
  if (!c) return;
  try {
    const r = c.organizationId ? await balanceApi.getOrganizationBalance(c.organizationId) : await balanceApi.getUserBalance(c.userId!);
    balance.value = r?.balance ?? 0;
  } catch {
    balance.value = ""; // a failed lookup leaves the badge empty, not an error on the customer's blade
  }
}
watch(criteria, load, { immediate: true });
useWidgetTrigger({ onRefresh: load });

const open = () => criteria.value && openBlade({ name: "LoyaltyOperationLog", options: { ...criteria.value }, onClose: load });
</script>
