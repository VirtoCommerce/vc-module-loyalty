<template>
  <!-- LocalizedString fields edited one language at a time (the AngularJS per-language inputs) -->
  <VcCard :header="$t('LOYALTY.FIELDS.LOCALIZATION')">
    <template #actions>
      <VcSelect
        v-model="language"
        class="ly-lang"
        size="small"
        :options="languages"
        option-value="id"
        option-label="name"
        :clearable="false"
        :aria-label="$t('LOYALTY.FIELDS.LANGUAGE')"
      />
    </template>
    <div class="tw-p-4 tw-space-y-4">
      <template
        v-for="f in fields"
        :key="f.field"
      >
        <VcTextarea
          v-if="f.multiline"
          :model-value="value(f.field)"
          :label="f.label"
          :placeholder="f.placeholder"
          multilanguage
          :current-language="language"
          maxlength="4000"
          :disabled="disabled"
          @update:model-value="(v) => set(f.field, v)"
        />
        <VcInput
          v-else
          :model-value="value(f.field)"
          :label="f.label"
          :placeholder="f.placeholder"
          multilanguage
          :current-language="language"
          maxlength="1024"
          :disabled="disabled"
          @update:model-value="(v) => set(f.field, v)"
        />
      </template>
    </div>
  </VcCard>
</template>

<script setup lang="ts">
import { VcCard, VcInput, VcSelect, VcTextarea } from "@vc-shell/framework/ui";
import { ref } from "vue";
import { lookups, type Option } from "../api";

type Localized = { values?: Record<string, string> } | null | undefined;

const props = defineProps<{
  item: object;
  fields: { field: string; label: string; placeholder?: string; multiline?: boolean }[];
  disabled?: boolean;
}>();

const languages = ref<Option[]>([]);
const language = ref("en-US");
lookups.languages().then((l) => {
  languages.value = l;
  if (l.length && !l.some((x) => x.id === language.value)) language.value = l[0].id;
});

const fieldsOf = () => props.item as Record<string, unknown>;
const value = (field: string) => (fieldsOf()[field] as Localized)?.values?.[language.value] ?? "";

function set(field: string, v: unknown) {
  const target = (fieldsOf()[field] ??= { values: {} }) as { values?: Record<string, string> };
  (target.values ??= {})[language.value] = String(v ?? "");
}
</script>
