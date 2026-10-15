import path from "node:path";
import { getDynamicModuleConfiguration } from "@vc-shell/mf-module";

// Loyalty contributions to the Admin Shell host (Module Federation remote),
// served by the platform at /modules/$(VirtoCommerce.Loyalty)/plugins/admin-shell/
const base = "/modules/$(VirtoCommerce.Loyalty)/plugins/admin-shell/";
process.env.APP_BASE_PATH = base;

export default getDynamicModuleConfiguration({
  appId: "admin-shell",
  moduleRoot: path.resolve(__dirname, ".."),
  entry: "./src/modules/index.ts",
  remoteName: "loyalty_admin",
  base,
});
