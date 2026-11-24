import type { GlobalFilterConfig } from "@vc-shell/framework";
import { computed, ref } from "vue";
import { searchStores, type Option } from "./api";

// The search bar of the program and mission lists. A marketer of a multi-store shop works on one store's programs
// or missions (the API takes a list of stores) and checks what runs now (onlyActive=true: active and within the dates;
// false isn't a useful filter, so it is a toggle).
export interface ListFilters {
  storeIds?: string[];
  onlyActive?: true;
}

interface FilterField {
  id: string;
  label: string;
  filter: { multiple?: boolean; toggle?: boolean; options?: { value: string; label: string }[] };
}

const stores = ref<Option[]>([]);
let storesLoaded = false;

const byLabel = (a: { label: string }, b: { label: string }) => a.label.localeCompare(b.label);
const picked = (value: unknown) => {
  const list = ([] as string[]).concat((value as string | string[] | undefined) ?? []);
  return list.length ? list : undefined;
};

// extra: the list's own fields, placed between the store and the toggle
export function useListFilters(t: (key: string) => string, extra: () => FilterField[] = () => []) {
  if (!storesLoaded) {
    storesLoaded = true;
    searchStores(undefined, 0, undefined, 100)
      .then((r) => (stores.value = r.results))
      .catch(() => (storesLoaded = false));
  }

  const filters = ref<ListFilters & Record<string, unknown>>({});

  // VcDataTable types global-filters as GlobalFilterConfig[], which lacks the search bar's toggle field
  const globalFilters = computed(
    () =>
      [
        {
          id: "storeIds",
          label: t("LOYALTY.FILTER.STORES"),
          filter: { multiple: true, options: stores.value.map((s) => ({ value: s.id, label: s.name })).sort(byLabel) },
        },
        ...extra(),
        { id: "onlyActive", label: t("LOYALTY.FILTER.ONLY_ACTIVE"), filter: { toggle: true } },
      ] as unknown as GlobalFilterConfig[],
  );

  function onFilter(event: { filters: Record<string, unknown> }) {
    const { storeIds, onlyActive, ...rest } = event.filters;
    const single = Object.fromEntries(Object.entries(rest).map(([k, v]) => [k, (v as string | undefined) || undefined]));
    filters.value = { ...single, storeIds: picked(storeIds), onlyActive: onlyActive ? true : undefined };
  }

  return { filters, globalFilters, onFilter };
}
