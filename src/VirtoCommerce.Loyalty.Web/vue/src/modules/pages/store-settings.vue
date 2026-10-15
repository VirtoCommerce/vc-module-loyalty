<template>
  <!-- The AngularJS loyalty-setting-details blade: a store's loyalty switch, mode and currency -->
  <VcBlade
    :title="$t('LOYALTY.SETTINGS.TITLE')"
    :toolbar-items="toolbar"
    :loading="loading"
    width="40%"
  >
    <VcContainer>
      <VcForm class="ly-form tw-space-y-4">
        <VcSwitch
          v-model="setting.loyaltyEnabled"
          :label="$t('LOYALTY.SETTINGS.ENABLED')"
          :disabled="readonly"
        />
        <VcSelect
          v-model="setting.loyaltyMode"
          :label="$t('LOYALTY.SETTINGS.MODE')"
          :placeholder="$t('LOYALTY.SETTINGS.MODE_PLACEHOLDER')"
          :options="modes"
          option-value="id"
          option-label="name"
          :disabled="readonly"
        />
        <VcSelect
          v-model="setting.loyaltyCurrency"
          :label="$t('LOYALTY.SETTINGS.CURRENCY')"
          :placeholder="$t('LOYALTY.SETTINGS.CURRENCY_PLACEHOLDER')"
          :options="currencies"
          option-value="id"
          option-label="name"
          :disabled="readonly"
        />
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useAsync, useBlade, useBladeForm, useLoading, usePermissions, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcContainer, VcForm, VcSelect, VcSwitch } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import type { LoyaltyStoreSetting } from "../../api_client/virtocommerce.loyalty";
import { lookups, settingApi, type Option } from "../api";

defineBlade({ url: "/loyalty-store-settings", name: "LoyaltyStoreSettings", permissions: ["loyalty:read"] });

// param: the store id
const { t } = useI18n({ useScope: "global" });
const { param, callParent } = useBlade();
const { hasAccess } = usePermissions();
const readonly = computed(() => !hasAccess("loyalty:update"));

const setting = ref<LoyaltyStoreSetting>({});
const form = useBladeForm({ data: setting, closeConfirmMessage: computed(() => t("LOYALTY.SETTINGS.CLOSE_CONFIRM")) });

const { loading: loadLoading, action: load } = useAsync(async () => {
  setting.value = await settingApi.getByStoreId(param.value!);
  form.setBaseline();
});
const { loading: saveLoading, action: save } = useAsync(async () => {
  await settingApi.update(setting.value);
  // the setting lives in the store's settings: refresh them in the store blade (keeping its unsaved edits),
  // or its next save would write the old ones back
  await callParent("refreshPart", "settings").catch(() => undefined);
  await load();
});
const loading = useLoading(loadLoading, saveLoading);

const modes = ref<Option[]>([]);
const currencies = ref<Option[]>([]);
onMounted(async () => {
  [modes.value, currencies.value] = await Promise.all([lookups.loyaltyModes(), lookups.currencies(), load()]);
});

const toolbar = ref<IBladeToolbar[]>([
  {
    id: "save",
    title: t("LOYALTY.ACTIONS.SAVE"),
    icon: "lucide-save",
    disabled: computed(() => !form.canSave.value || readonly.value),
    clickHandler: () => save(),
  },
  {
    id: "reset",
    title: t("LOYALTY.ACTIONS.RESET"),
    icon: "lucide-undo-2",
    disabled: computed(() => !form.isModified.value),
    clickHandler: () => form.revert(),
  },
]);
</script>
