import { ref, watch } from "vue";

// Saved loyalty program filters of the AngularJS list ($localStorage.loyaltyProgramSearchFilters), shared by the list
// and the filter blade. Stored in this browser only, as the AngularJS filters were.
export interface SavedProgramFilter {
  id: string;
  name: string;
  onlyActive?: boolean;
  storeIds?: string[];
}

const KEY = "vc-loyalty.programFilters";
const CURRENT_KEY = "vc-loyalty.programFilterId";

function read<T>(key: string, fallback: T): T {
  try {
    return (JSON.parse(localStorage.getItem(key) ?? "") as T) ?? fallback;
  } catch {
    return fallback;
  }
}

const filters = ref<SavedProgramFilter[]>(read(KEY, []));
const currentId = ref<string | undefined>(read<string | undefined>(CURRENT_KEY, undefined));
watch(filters, (v) => localStorage.setItem(KEY, JSON.stringify(v)), { deep: true });
watch(currentId, (v) => (v ? localStorage.setItem(CURRENT_KEY, JSON.stringify(v)) : localStorage.removeItem(CURRENT_KEY)));

export function useSavedFilters() {
  const find = (id?: string) => (id ? filters.value.find((f) => f.id === id) : undefined);
  return { filters, currentId, find };
}
