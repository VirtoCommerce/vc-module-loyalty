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
          v-if="!isNew && !isDraft"
          variant="info"
          icon="lucide-info"
          >{{ $t("LOYALTY.MISSION.IMMUTABLE") }}</VcBanner
        >
        <VcBanner
          v-if="serverError"
          variant="danger"
          icon="lucide-circle-alert"
          >{{ serverError }}</VcBanner
        >
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcInput
              v-model="item.name"
              :label="$t('LOYALTY.FIELDS.NAME')"
              :placeholder="$t('LOYALTY.MISSION.NAME_PLACEHOLDER')"
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
            <VcInput
              :model-value="statusName"
              :label="$t('LOYALTY.FIELDS.STATUS')"
              disabled
            />
          </VcCol>
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

        <LocalizedFields
          :fields="[
            { field: 'localizedName', label: $t('LOYALTY.FIELDS.LOCALIZED_NAME'), placeholder: $t('LOYALTY.MISSION.NAME_PLACEHOLDER') },
            { field: 'description', label: $t('LOYALTY.FIELDS.LOCALIZED_DESCRIPTION'), multiline: true },
          ]"
          :item="item"
          :disabled="readonly"
        />

        <VcCard
          v-if="item.dynamicExpression"
          :header="$t('LOYALTY.MISSION.CONDITIONS')"
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
import { defineBladeContext, useAsync, useBlade, useBladeForm, useBladeWidgets, useLoading, usePermissions, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { VcBanner, VcBlade, VcCard, VcCol, VcContainer, VcDatePicker, VcForm, VcInput, VcRow, VcSelect } from "@vc-shell/framework/ui";
import { computed, onMounted, provide, ref } from "vue";
import { useI18n } from "vue-i18n";
import type { LoyaltyMission } from "../../api_client/virtocommerce.loyalty";
import { missionStatusKey, missionsApi, searchStores, validationMessages } from "../api";
import ExpressionNode from "../expressions/ExpressionNode.vue";
import { GoalItemsKey, treeErrors, withoutPrototypes, type TreeNode } from "../expressions/defs";
import LocalizedFields from "../components/LocalizedFields.vue";

defineBlade({ url: "/loyalty-mission", name: "LoyaltyMissionDetails", permissions: ["loyalty:access"] });

// param: the mission id
const { t } = useI18n({ useScope: "global" });
const { param, expanded, openBlade, callParent, exposeToChildren, closeChildren, replaceWith } = useBlade();
const { hasAccess } = usePermissions();
const { showConfirmation } = usePopup();

const item = ref<LoyaltyMission>({});
const id = ref<string | undefined>(param.value);
const isNew = computed(() => !id.value);
// a published or archived mission is immutable
const isDraft = computed(() => isNew.value || item.value.status === "Draft");
const readonly = computed(() => !isDraft.value || !hasAccess(isNew.value ? "loyalty:create" : "loyalty:update"));
const serverError = ref("");

const { loading: itemLoading, action: load } = useAsync(async () => {
  item.value = id.value ? await missionsApi.get(id.value) : await missionsApi.getNewLoyaltyMission();
});

// false when the server rejected the mission (its validation messages are shown in the blade)
const { loading: saveLoading, action: persist } = useAsync<void, boolean>(async () => {
  serverError.value = "";
  try {
    if (id.value) await missionsApi.update(withoutPrototypes(item.value));
    else id.value = (await missionsApi.create(withoutPrototypes(item.value))).id;
  } catch (e) {
    const messages = validationMessages(e);
    if (!messages) throw e;
    serverError.value = messages.join("; ");
    return false;
  }
  item.value = await missionsApi.get(id.value!);
  return true;
});
const loading = useLoading(itemLoading, saveLoading);

const errors = computed(() => treeErrors(item.value.dynamicExpression as TreeNode));
const form = useBladeForm({
  data: item,
  canSaveOverride: computed(() => !!item.value.name?.trim() && !!item.value.storeId && !errors.value.length),
  closeConfirmMessage: computed(() => t("LOYALTY.MISSION.CLOSE_CONFIRM")),
});
const nameError = computed(() => (form.isReady.value && !item.value.name?.trim() ? t("LOYALTY.VALIDATION.REQUIRED") : undefined));
const storeError = computed(() => (form.isReady.value && !item.value.storeId ? t("LOYALTY.VALIDATION.REQUIRED") : undefined));

const title = computed(() => item.value.name || t(isNew.value ? "LOYALTY.MISSION.NEW_TITLE" : "LOYALTY.MISSION.TITLE"));
const statusName = computed(() => t(missionStatusKey(item.value.status)));

async function save() {
  const created = isNew.value;
  if (!(await persist())) return;
  form.setBaseline();
  callParent("reload");
  // a created mission reopens by its id, so the URL (F5) and the next save address it
  if (created) {
    await closeChildren(); // replaceWith replaces the active blade: the banner's, if it stays open
    if (expanded.value) await replaceWith({ name: "LoyaltyMissionDetails", param: id.value });
  }
}

// publish / archive: the status change is saved with the mission, after a confirmation
async function changeStatus(status: "Published" | "Archived", message: string) {
  if (!(await showConfirmation(t(message)))) return;
  if (errors.value.length) {
    serverError.value = errors.value[0];
    return;
  }
  const previous = item.value.status;
  item.value.status = status;
  // any failure rolls the status back (useAsync has already shown an error that isn't a validation one)
  if (!(await persist().catch(() => false))) {
    item.value.status = previous;
    return;
  }
  form.setBaseline();
  callParent("reload");
}

const bladeToolbar = ref<IBladeToolbar[]>([
  {
    id: "save",
    title: computed(() => t(isNew.value ? "LOYALTY.ACTIONS.CREATE" : "LOYALTY.ACTIONS.SAVE")),
    icon: "lucide-save",
    disabled: computed(() => !form.canSave.value || readonly.value),
    clickHandler: save,
  },
  {
    id: "reset",
    title: t("LOYALTY.ACTIONS.RESET"),
    icon: "lucide-undo-2",
    isVisible: computed(() => !isNew.value),
    disabled: computed(() => !form.isModified.value || !isDraft.value),
    clickHandler: () => {
      serverError.value = "";
      return form.revert();
    },
  },
  {
    id: "publish",
    title: t("LOYALTY.MISSION.PUBLISH"),
    icon: "lucide-rocket",
    permissions: ["loyalty:update"],
    isVisible: computed(() => !isNew.value),
    disabled: computed(() => item.value.status !== "Draft"),
    clickHandler: () => changeStatus("Published", "LOYALTY.MISSION.PUBLISH_CONFIRM"),
  },
  {
    id: "archive",
    title: t("LOYALTY.MISSION.ARCHIVE"),
    icon: "lucide-archive",
    permissions: ["loyalty:update"],
    isVisible: computed(() => !isNew.value),
    disabled: computed(() => item.value.status === "Archived"),
    clickHandler: () => changeStatus("Archived", "LOYALTY.MISSION.ARCHIVE_CONFIRM"),
  },
]);

// the PerSku goal's "select" link: the mission's target SKUs, once it is saved
provide(GoalItemsKey, {
  canOpen: computed(() => !isNew.value),
  open: () => openBlade({ name: "LoyaltyProductItems", options: { kind: "goals", parentId: id.value, storeId: item.value.storeId } }),
});

// the AngularJS loyaltyMissionDetail widget: the banner, edited in a child blade and saved with the mission
useBladeWidgets([
  {
    id: "LoyaltyMissionBanner",
    icon: "lucide-image",
    title: "LOYALTY.WIDGETS.BANNER",
    onClick: () =>
      openBlade({
        name: "LoyaltyMissionBanner",
        options: { bannerUrl: item.value.bannerUrl ?? undefined, missionId: id.value, readonly: readonly.value },
      }),
  },
]);

exposeToChildren({ setBanner: (url: string | null) => (item.value.bannerUrl = url ?? undefined) });

// the mission, for widgets other modules may add (registerExternalWidget with targetBlades: ["LoyaltyMissionDetails"])
defineBladeContext({ item });

onMounted(async () => {
  await load();
  form.setBaseline();
});
</script>
