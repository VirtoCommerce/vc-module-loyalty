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

    public static class MissionTypes
    {
        public const string OrderValue = "OrderValue";
        public const string OrderCount = "OrderCount";
        public const string PerSku = "PerSku";
        public const string PerSkuAll = "PerSkuAll";
        public const string PerSkuAny = "PerSkuAny";
    }

    public static class MissionStatuses
    {
        public const string Draft = "Draft";
        public const string Published = "Published";
        public const string Archived = "Archived";
    }

    public static class MissionProgressStatuses
    {
        public const string InProgress = "InProgress";
        public const string Completed = "Completed";
        public const string Expired = "Expired";
    }

    public static class MissionPeriodicities
    {
        public const string None = "None";
    }

    public static class LoyaltyBalanceCalculationModes
    {
        public const string Customer = "Customer";
        public const string Organization = "Organization";
    }

    /// <summary>
    /// Values for <see cref="Models.LoyaltyBalanceOperationLog.SourceType"/>.
    /// </summary>
    public static class LoyaltySourceTypes
    {
        public const string LoyaltyProgram = nameof(Models.LoyaltyProgram);
        public const string LoyaltyMission = nameof(Models.LoyaltyMission);
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

            public static SettingDescriptor MissionsEnable { get; } = new()
            {
                Name = "Loyalty.Missions.Enable",
                GroupName = "Loyalty|Missions",
                ValueType = SettingValueType.Boolean,
                DefaultValue = false,
                IsPublic = true,
            };

            public static SettingDescriptor LoyaltyBalanceCalculationMode { get; } = new()
            {
                Name = "Loyalty.LoyaltyBalanceCalculationMode",
                GroupName = "Loyalty|General",
                ValueType = SettingValueType.ShortText,
                IsPublic = false,
                DefaultValue = LoyaltyBalanceCalculationModes.Customer,
                AllowedValues = [LoyaltyBalanceCalculationModes.Customer, LoyaltyBalanceCalculationModes.Organization],
            };

            public static IEnumerable<SettingDescriptor> AllGeneralSettings
            {
                get
                {
                    yield return Enable;
                    yield return LoyaltyMode;
                    yield return LoyaltyCurrency;
                    yield return DefaultProductMultiplyFactor;
                    yield return MissionsEnable;
                    yield return LoyaltyBalanceCalculationMode;
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
                yield return General.MissionsEnable;
                yield return General.LoyaltyBalanceCalculationMode;
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
