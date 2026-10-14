<template>
  <VcBlade
    :title="title"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="70%"
  >
    <VcContainer>
      <VcForm class="ly-form tw-space-y-4">
        <VcBanner
          v-if="isNew && item.programType === 'ProductPoints'"
          variant="info"
          icon="lucide-info"
          >{{ $t("LOYALTY.PROGRAM.SAVE_FIRST") }}</VcBanner
        >
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcInput
              v-model="item.name"
              :label="$t('LOYALTY.FIELDS.NAME')"
              :placeholder="$t('LOYALTY.PROGRAM.NAME_PLACEHOLDER')"
              required
              :error-message="nameError"
              :disabled="readonly"
            />
          </VcCol>
          <VcCol>
            <VcSelect
              v-model="item.storeId"
              :label="$t('LOYALTY.FIELDS.STORE')"
              :placeholder="$t('LOYALTY.FIELDS.STORE_PLACEHOLDER')"
              :options="searchStores"
              option-value="id"
              option-label="name"
              searchable
              required
              :error-message="storeError"
              :disabled="readonly"
            />
          </VcCol>
        </VcRow>
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcDatePicker
              v-model="item.startDate"
              :label="$t('LOYALTY.FIELDS.START_DATE')"
              clearable
              :disabled="readonly"
            />
          </VcCol>
          <VcCol>
            <VcDatePicker
              v-model="item.endDate"
              :label="$t('LOYALTY.FIELDS.END_DATE')"
              clearable
              :disabled="readonly"
            />
          </VcCol>
        </VcRow>
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcInput
              v-model="item.priority"
              type="integer"
              :label="$t('LOYALTY.PROGRAM.PRIORITY')"
              :disabled="readonly"
            />
          </VcCol>
          <VcCol class="ly-form__switch">
            <VcSwitch
              v-model="item.isActive"
              :label="$t('LOYALTY.STATUS.ACTIVE')"
              :disabled="readonly"
            />
          </VcCol>
        </VcRow>

        <LocalizedFields
          :fields="[{ field: 'localizedName', label: $t('LOYALTY.FIELDS.LOCALIZED_NAME'), placeholder: $t('LOYALTY.PROGRAM.NAME_PLACEHOLDER') }]"
          :item="item"
          :disabled="readonly"
        />

        <VcCard
          v-if="item.dynamicExpression"
          :header="$t('LOYALTY.PROGRAM.CONDITIONS')"
          class="ly-conditions"
        >
          <ExpressionNode
            v-for="(block, i) in item.dynamicExpression.children ?? []"
            :key="i"
            :node="block as TreeNode"
            :disabled="readonly"
          />
        </VcCard>
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { defineBladeContext, useAsync, useBlade, useBladeForm, useBladeWidgets, useLoading, usePermissions, type IBladeToolbar } from "@vc-shell/framework";
import { VcBanner, VcBlade, VcCard, VcCol, VcContainer, VcDatePicker, VcForm, VcInput, VcRow, VcSelect, VcSwitch } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import type { LoyaltyProgram } from "../../api_client/virtocommerce.loyalty";
import { factorsApi, programsApi, searchStores } from "../api";
import ExpressionNode from "../expressions/ExpressionNode.vue";
import { withoutPrototypes, type TreeNode } from "../expressions/defs";
import LocalizedFields from "../components/LocalizedFields.vue";

defineBlade({ url: "/loyalty-program", name: "LoyaltyProgramDetails", permissions: ["loyalty:access"] });

// param: the program id; options.programType: the type of a new program (from the type picker)
const { t } = useI18n({ useScope: "global" });
const { param, options, openBlade, callParent, exposeToChildren, replaceWith } = useBlade<{ programType?: string }>();
const { hasAccess } = usePermissions();

const item = ref<LoyaltyProgram>({});
const id = ref<string | undefined>(param.value);
const isNew = computed(() => !id.value);
const readonly = computed(() => !hasAccess(isNew.value ? "loyalty:create" : "loyalty:update"));
const factorCount = ref(0);

const { loading: itemLoading, action: load } = useAsync(async () => {
  item.value = id.value ? await programsApi.get(id.value) : await programsApi.getNewLoyaltyProgram(options.value?.programType ?? "Default");
});

const { loading: saveLoading, action: save } = useAsync(async () => {
  if (id.value) await programsApi.update(withoutPrototypes(item.value));
  else id.value = (await programsApi.create(withoutPrototypes(item.value))).id;
  item.value = await programsApi.get(id.value!);
});
const loading = useLoading(itemLoading, saveLoading);

const form = useBladeForm({
  data: item,
  canSaveOverride: computed(() => !!item.value.name?.trim() && !!item.value.storeId),
  closeConfirmMessage: computed(() => t("LOYALTY.PROGRAM.CLOSE_CONFIRM")),
});
const nameError = computed(() => (form.isReady.value && !item.value.name?.trim() ? t("LOYALTY.VALIDATION.REQUIRED") : undefined));
const storeError = computed(() => (form.isReady.value && !item.value.storeId ? t("LOYALTY.VALIDATION.REQUIRED") : undefined));

const title = computed(() => item.value.name || t(isNew.value ? "LOYALTY.PROGRAM.NEW_TITLE" : "LOYALTY.PROGRAM.TITLE"));

async function refreshCount() {
  if (!id.value || item.value.programType !== "ProductPoints") return;
  factorCount.value = (await factorsApi.search({ loyaltyProgramId: id.value, take: 0 })).totalCount ?? 0;
}

const bladeToolbar = ref<IBladeToolbar[]>([
  {
    id: "save",
    title: computed(() => t(isNew.value ? "LOYALTY.ACTIONS.CREATE" : "LOYALTY.ACTIONS.SAVE")),
    icon: "lucide-save",
    disabled: computed(() => !form.canSave.value || readonly.value),
    async clickHandler() {
      const created = isNew.value;
      await save();
      form.setBaseline();
      callParent("reload");
      // a created program reopens by its id, so the URL (F5) and the next save address it
      if (created) return replaceWith({ name: "LoyaltyProgramDetails", param: id.value });
      await refreshCount();
    },
  },
  {
    id: "reset",
    title: t("LOYALTY.ACTIONS.RESET"),
    icon: "lucide-undo-2",
    isVisible: computed(() => !isNew.value),
    disabled: computed(() => !form.isModified.value),
    clickHandler: () => form.revert(),
  },
]);

// the AngularJS loyaltyDetail widget: product factors of a saved product-points program
useBladeWidgets([
  {
    id: "LoyaltyProductFactors",
    icon: "lucide-package",
    title: "LOYALTY.WIDGETS.PRODUCT_FACTORS",
    badge: factorCount,
    isVisible: computed(() => !isNew.value && item.value.programType === "ProductPoints"),
    onClick: () =>
      openBlade({
        name: "LoyaltyProductItems",
        options: { kind: "factors", parentId: id.value, storeId: item.value.storeId },
        onClose: refreshCount,
      }),
  },
]);

// the program, for widgets other modules may add (registerExternalWidget with targetBlades: ["LoyaltyProgramDetails"])
defineBladeContext({ item });

// the factors blade updates the widget badge after it saves
exposeToChildren({ reloadWidgets: refreshCount });

onMounted(async () => {
  await load();
  form.setBaseline();
  await refreshCount();
});
</script>
