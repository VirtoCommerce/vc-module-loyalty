<template>
  <VcBlade
    :title="$t('LOYALTY.BANNER.TITLE')"
    :toolbar-items="toolbar"
    width="30%"
  >
    <div class="ly-form tw-space-y-2">
      <VcImageUpload
        :image="banner.url ? { url: banner.url } : undefined"
        :loading="uploading"
        :disabled="readonly"
        accept=".png,.gif,.jpg,.jpeg,.svg,.webp"
        @upload="upload"
        @remove="banner.url = null"
      />
      <VcHint>{{ $t("LOYALTY.BANNER.HINT") }}</VcHint>
    </div>
  </VcBlade>
</template>

<script setup lang="ts">
// the AngularJS mission banner blade: uploads the image to the loyalty-missions asset folder;
// the URL goes back to the mission blade and is saved with the mission
import { useBlade, useBladeForm, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcHint, VcImageUpload } from "@vc-shell/framework/ui";
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";

defineBlade({ url: "/loyalty-mission-banner", name: "LoyaltyMissionBanner", permissions: ["loyalty:access"] });

const { t } = useI18n({ useScope: "global" });
const { options, callParent, closeSelf, setError, clearError } = useBlade<{ bannerUrl?: string; missionId?: string; readonly?: boolean }>();
const readonly = computed(() => !!options.value?.readonly);

const banner = ref<{ url: string | null }>({ url: options.value?.bannerUrl ?? null });
const uploading = ref(false);
const form = useBladeForm({ data: banner });
form.setBaseline();

async function upload(files: FileList) {
  const file = files[0];
  if (!file) return;
  uploading.value = true;
  clearError();
  try {
    const body = new FormData();
    body.append("file", file, `banner_${options.value?.missionId ?? "new"}_${Date.now()}.${file.name.split(".").pop()}`);
    const res = await fetch("/api/assets?folderUrl=loyalty-missions", { method: "POST", body, headers: { Accept: "application/json" } });
    if (!res.ok) throw new Error(`${file.name} failed: ${res.status}`);
    const [uploaded] = (await res.json()) as { url: string }[];
    banner.value.url = uploaded.url;
  } catch (e) {
    setError(e);
  } finally {
    uploading.value = false;
  }
}

const toolbar = ref<IBladeToolbar[]>([
  {
    id: "save",
    title: t("LOYALTY.ACTIONS.APPLY"),
    icon: "lucide-check",
    disabled: computed(() => readonly.value || !form.isModified.value),
    async clickHandler() {
      await callParent("setBanner", banner.value.url);
      form.setBaseline();
      await closeSelf();
    },
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
