import {
  ApiException,
  LoyaltyBalanceOperationLogClient,
  LoyaltyMissionClient,
  LoyaltyMissionGoalItemClient,
  LoyaltyProgramClient,
  LoyaltyProgramProductFactorClient,
  LoyaltySettingClient,
} from "../api_client/virtocommerce.loyalty";
import { ref } from "vue";

// the module's own API (generated clients; same-origin cookie auth, which is all useApiClient adds)
export const programsApi = new LoyaltyProgramClient();
export const missionsApi = new LoyaltyMissionClient();
export const factorsApi = new LoyaltyProgramProductFactorClient();
export const goalItemsApi = new LoyaltyMissionGoalItemClient();
export const balanceApi = new LoyaltyBalanceOperationLogClient();
export const settingApi = new LoyaltySettingClient();

// a rejected mission save returns FluentValidation failures: [{ propertyName, errorMessage }]
export function validationMessages(e: unknown): string[] | undefined {
  if (!ApiException.isApiException(e) || e.status !== 400) return undefined;
  try {
    const failures = JSON.parse(e.response) as { errorMessage?: string }[];
    const messages = (Array.isArray(failures) ? failures : []).map((x) => x.errorMessage).filter((x): x is string => !!x);
    return messages.length ? messages : undefined;
  } catch {
    return undefined;
  }
}

// Other modules' / platform endpoints these screens read (stores, products, currencies, settings):
// a few calls, so plain same-origin fetches (cookie auth) instead of generating their clients.
export interface Option {
  id: string;
  name: string;
  code?: string;
}
export interface Page<T = Option> {
  results: T[];
  totalCount: number;
}

export async function http<T>(url: string, body?: unknown, method = body === undefined ? "GET" : "POST"): Promise<T> {
  const res = await fetch(url, {
    method,
    headers: body === undefined ? { Accept: "application/json" } : { "Content-Type": "application/json", Accept: "application/json" },
    body: body === undefined ? undefined : JSON.stringify(body),
  });
  if (!res.ok) throw new Error(`${res.status} ${res.statusText}: ${url}`);
  const text = await res.text();
  return (text ? JSON.parse(text) : undefined) as T;
}

const PAGE = 20;

// caches a parameterless loader for the session
function once<T>(load: () => Promise<T>): () => Promise<T> {
  let p: Promise<T> | undefined;
  return () => (p ??= load().catch((e) => ((p = undefined), Promise.reject(e))));
}

// VcSelect option source: (keyword, skip, ids) => page
export async function searchStores(keyword?: string, skip = 0, ids?: string[]): Promise<Page> {
  const r = await http<{ results?: Option[]; totalCount?: number }>("/api/stores/search", {
    keyword: keyword || undefined,
    objectIds: ids?.length ? ids : undefined,
    skip,
    take: ids?.length ? ids.length : PAGE,
    responseGroup: "None",
  });
  return { results: (r.results ?? []).map((s) => ({ id: s.id, name: s.name || s.id })), totalCount: r.totalCount ?? 0 };
}

// the catalog of the store a program / mission belongs to (the legacy product picker browsed it)
export const storeCatalogId = (storeId: string) => http<{ catalog?: string }>(`/api/stores/${encodeURIComponent(storeId)}`).then((s) => s?.catalog);

export interface ProductEntry {
  id: string;
  name: string;
  code?: string;
  imageUrl?: string;
}

export async function searchProducts(catalogId: string | undefined, keyword: string | undefined, skip: number, take: number): Promise<Page<ProductEntry>> {
  const r = await http<{ listEntries?: ProductEntry[]; totalCount?: number }>("/api/catalog/listentries", {
    catalogId,
    keyword: keyword || undefined,
    objectTypes: ["CatalogProduct"],
    searchInChildren: true,
    searchInVariations: true,
    skip,
    take,
  });
  return { results: r.listEntries ?? [], totalCount: r.totalCount ?? 0 };
}

export const settingValues = (name: string) => http<unknown[]>(`/api/platform/settings/values/${encodeURIComponent(name)}`);
const asOptions = (values: unknown[] | undefined): Option[] => (values ?? []).map((x) => ({ id: String(x), name: String(x) }));

// dictionaries of the forms and the expression editor
export const lookups = {
  currencies: once(() => http<{ code?: string }[]>("/api/currencies").then((v) => asOptions((v ?? []).map((c) => c.code).filter(Boolean)))),
  languages: once(() => settingValues("VirtoCommerce.Core.General.Languages").then(asOptions)),
  memberGroups: once(() => settingValues("Customer.MemberGroups").then(asOptions)),
  orderStatuses: once(() => settingValues("Order.Status").then(asOptions)),
  loyaltyModes: once(() =>
    http<{ allowedValues?: unknown[] }>("/api/platform/settings/Loyalty.Mode").then((s) => asOptions(s?.allowedValues)),
  ),
};
export type LookupSource = keyof typeof lookups;

// locale key of a mission status label
export const missionStatusKey = (status?: string) => `LOYALTY.MISSION_STATUS.${(status ?? "Draft").toUpperCase()}`;

// a list's "a child blade is open" flag, which collapses its columns; track() gives the child's onClose.
// A blade replaced by a newer one must not clear the flag on its close.
export function useChildOpen() {
  const childOpen = ref(false);
  let opened = 0;
  function track() {
    const mine = ++opened;
    childOpen.value = true;
    return () => {
      if (mine === opened) childOpen.value = false;
    };
  }
  return { childOpen, track };
}
