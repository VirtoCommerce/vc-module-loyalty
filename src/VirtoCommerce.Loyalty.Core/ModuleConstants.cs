using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.Loyalty.Core;

public static class ModuleConstants
{
    public static class Security
    {
        public static class Permissions
        {
            public const string Read = "loyalty:read";
            public const string Access = "loyalty:access";
            public const string Create = "loyalty:create";
            public const string Update = "loyalty:update";
            public const string Delete = "loyalty:delete";

            public static string[] AllPermissions { get; } =
            [
                Read,
                Access,
                Create,
                Update,
                Delete,
            ];
        }
    }

    public static class LoyaltyPrograms
    {
        public const string AwardedUsageType = "Awarded";
        public const string RedeemedUsageType = "Redeemed";
    }

    public static class Settings
    {
        public static class General
        {
            public static SettingDescriptor LoyaltyEnabled { get; } = new()
            {
                Name = "Loyalty.Enabled",
                GroupName = "Loyalty|General",
                ValueType = SettingValueType.Boolean,
                DefaultValue = false,
            };

            public static IEnumerable<SettingDescriptor> AllGeneralSettings
            {
                get
                {
                    yield return LoyaltyEnabled;
                }
            }
        }

        public static IEnumerable<SettingDescriptor> AllSettings
        {
            get
            {
                return General.AllGeneralSettings;
            }
        }
    }
}
