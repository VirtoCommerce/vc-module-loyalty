import * as blades from "./pages";
import * as locales from "./locales";
import { defineAppModule, registerExternalWidget, usePermissions } from "@vc-shell/framework";
import { markRaw } from "vue";
import "./styles.scss";
import BalanceWidget from "./widgets/BalanceWidget.vue";
import ProductFactorsWidget from "./widgets/ProductFactorsWidget.vue";
import StoreSettingsWidget from "./widgets/StoreSettingsWidget.vue";

// The Loyalty module extends other modules' blades, as its AngularJS widgets do (none on unsaved entities):
// the points balance (customerDetail1 / organizationDetail2), the store's loyalty settings (storeDetail)
// and the product's loyalty factors (itemDetail). All of them read through loyalty:read endpoints.
const saved = (blade?: { param?: unknown }) => !!blade?.param && usePermissions().hasAccess("loyalty:read");

registerExternalWidget({
  id: "LoyaltyBalance",
  title: "Loyalty balance",
  component: markRaw(BalanceWidget),
  targetBlades: ["CustomerContactDetails", "CustomerOrganizationDetails"],
  isVisible: saved,
});
registerExternalWidget({
  id: "LoyaltyStoreSettings",
  title: "Loyalty settings",
  component: markRaw(StoreSettingsWidget),
  targetBlades: ["StoreDetails"],
  isVisible: saved,
});
registerExternalWidget({
  id: "LoyaltyProductFactors",
  title: "Loyalty factors",
  component: markRaw(ProductFactorsWidget),
  targetBlades: ["CatalogProductDetails"],
  isVisible: saved,
});

export default defineAppModule({
  blades,
  locales,
});
