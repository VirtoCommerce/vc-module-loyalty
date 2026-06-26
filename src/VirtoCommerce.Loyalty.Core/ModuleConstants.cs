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
        public const string EarnedOperationType = "Earned";
        public const string RedeemedOperationType = "Redeemed";

        public const string ProductProgramType = "ProductPoints";
        public const string DefaultProgramType = "Default";
    }

    public static class LoyaltyModes
    {
        public const string LoyaltyStore = "Loyalty Store";
        public const string MixedCart = "Mixed Cart";
        public const string CouponRedemption = "Coupon Redemption";
        public const string PaymentMethod = "Payment Method";
    }

    public const string LoyaltyPaymentMethodGatewayCode = "LoyaltyPaymentMethod";

    public const string FallbackLoyaltyCurrencyCode = "XPT";

    public static class Settings
    {
        public static class General
        {
            public static SettingDescriptor Enable { get; } = new()
            {
                Name = "Loyalty.Enable",
                GroupName = "Loyalty|General",
                ValueType = SettingValueType.Boolean,
                DefaultValue = false,
                IsPublic = true,
            };

            public static SettingDescriptor LoyaltyMode { get; } = new()
            {
                Name = "Loyalty.Mode",
                GroupName = "Loyalty|General",
                ValueType = SettingValueType.ShortText,
                IsPublic = true,
                AllowedValues = [LoyaltyModes.LoyaltyStore, LoyaltyModes.MixedCart, LoyaltyModes.CouponRedemption, LoyaltyModes.PaymentMethod]
            };

            public static SettingDescriptor LoyaltyCurrency { get; } = new()
            {
                Name = "Loyalty.Currency",
                GroupName = "Loyalty|General",
                ValueType = SettingValueType.ShortText,
                IsPublic = true,
            };

            public static SettingDescriptor DefaultProductMultiplyFactor { get; } = new()
            {
                Name = "Loyalty.DefaultProductMultiplyFactor",
                GroupName = "Loyalty|General",
                ValueType = SettingValueType.Decimal,
                DefaultValue = 1m,
                IsPublic = true,
            };

            public static IEnumerable<SettingDescriptor> AllGeneralSettings
            {
                get
                {
                    yield return Enable;
                    yield return LoyaltyMode;
                    yield return LoyaltyCurrency;
                    yield return DefaultProductMultiplyFactor;
                }
            }
        }

        public static IEnumerable<SettingDescriptor> StoreSettings
        {
            get
            {
                yield return General.Enable;
                yield return General.LoyaltyMode;
                yield return General.LoyaltyCurrency;
                yield return General.DefaultProductMultiplyFactor;
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
