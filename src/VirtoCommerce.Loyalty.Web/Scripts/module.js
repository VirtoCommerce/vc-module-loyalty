// Call this to register your module to main application
var moduleName = 'VirtoCommerce.Loyalty';

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])
    .config(['$stateProvider',
        function ($stateProvider) {
            $stateProvider
                .state('workspace.LoyaltyState', {
                    url: '/loyalty',
                    templateUrl: '$(Platform)/Scripts/common/templates/home.tpl.html',
                    controller: [
                        'platformWebApp.bladeNavigationService',
                        function (bladeNavigationService) {
                            var newBlade = {
                                id: 'loyaltyProgramList',
                                controller: 'VirtoCommerce.Loyalty.loyaltyProgramListController',
                                title: 'Loyalty.blades.loyalty-program-list.title',
                                template: 'Modules/$(VirtoCommerce.Loyalty)/Scripts/blades/loyalty-program-list.html',
                                isClosingDisabled: true,
                            };
                            bladeNavigationService.showBlade(newBlade);
                        }
                    ]
                })
                .state('workspace.LoyaltyMissionState', {
                    url: '/loyalty-missions',
                    templateUrl: '$(Platform)/Scripts/common/templates/home.tpl.html',
                    controller: [
                        'platformWebApp.bladeNavigationService',
                        function (bladeNavigationService) {
                            var newBlade = {
                                id: 'loyaltyMissionList',
                                controller: 'VirtoCommerce.Loyalty.loyaltyMissionListController',
                                title: 'Loyalty.blades.loyalty-mission-list.title',
                                template: 'Modules/$(VirtoCommerce.Loyalty)/Scripts/blades/loyalty-mission-list.html',
                                isClosingDisabled: true,
                            };
                            bladeNavigationService.showBlade(newBlade);
                        }
                    ]
                });
        }
    ])
    .run(['$state',
        'platformWebApp.mainMenuService',
        'platformWebApp.dynamicTemplateService',
        'virtoCommerce.coreModule.common.dynamicExpressionService',
        'platformWebApp.metaFormsService',
        'platformWebApp.widgetService',
        function ($state, mainMenuService, dynamicTemplateService, dynamicExpressionService, metaFormsService, widgetService) {
            // Register module in main menu
            var menuItem = {
                path: 'browse/loyalty',
                icon: 'fa fa-star',
                title: 'Loyalty',
                priority: 100,
                action: function () { $state.go('workspace.LoyaltyState'); },
                permission: 'loyalty:access',
            };
            mainMenuService.addMenuItem(menuItem);

            var missionsMenuItem = {
                path: 'browse/loyalty-missions',
                icon: 'fa fa-flag-checkered',
                title: 'Loyalty.blades.loyalty-mission-list.title',
                priority: 101,
                action: function () { $state.go('workspace.LoyaltyMissionState'); },
                permission: 'loyalty:access',
            };
            mainMenuService.addMenuItem(missionsMenuItem);

            // widgets
            var customerLoyaltyWidget = {
                controller: 'VirtoCommerce.Loyalty.customerLoyaltyWidgetController',
                template: 'Modules/$(VirtoCommerce.Loyalty)/Scripts/widgets/customer-loyalty-widget.html',
                size: [2, 1],
                isVisible: function (blade) {
                    return !blade.isNew;
                }
            };

            widgetService.registerWidget(customerLoyaltyWidget, 'customerDetail1');

            var loyaltySettingWidget = {
                controller: 'VirtoCommerce.Loyalty.loyaltySettingWidgetController',
                template: 'Modules/$(VirtoCommerce.Loyalty)/Scripts/widgets/loyalty-setting-widget.html',
                isVisible: function (blade) {
                    return !blade.isNew;
                }
            };

            widgetService.registerWidget(loyaltySettingWidget, 'storeDetail');

            var loyaltyProductFactorsWidget = {
                controller: 'VirtoCommerce.Loyalty.loyaltyProgramProductFactorsWidgetController',
                template: 'Modules/$(VirtoCommerce.Loyalty)/Scripts/widgets/loyalty-product-factors-widget.html'
            };

            widgetService.registerWidget(loyaltyProductFactorsWidget, 'loyaltyDetail');

            var loyaltyMissionGoalItemsWidget = {
                controller: 'VirtoCommerce.Loyalty.loyaltyMissionGoalItemsWidgetController',
                template: 'Modules/$(VirtoCommerce.Loyalty)/Scripts/widgets/loyalty-mission-goal-items-widget.html',
                isVisible: function (blade) {
                    return !blade.isNew;
                }
            };

            widgetService.registerWidget(loyaltyMissionGoalItemsWidget, 'loyaltyMissionDetail');

            var productDetailsloyaltyFactorsWidget = {
                controller: 'VirtoCommerce.Loyalty.productDetailsLoyaltyFactorsWidgetController',
                template: 'Modules/$(VirtoCommerce.Loyalty)/Scripts/widgets/product-details-loyalty-factors-widget.html'
            };

            widgetService.registerWidget(productDetailsloyaltyFactorsWidget, 'itemDetail');

            // Register meta fields
            metaFormsService.registerMetaFields('loyaltyProgramDetail', [
                {
                    name: 'isActive',
                    title: 'Loyalty.blades.loyalty-program-details.labels.is-active',
                    colSpan: 3,
                    valueType: 'Boolean'
                },
                {
                    name: 'priority',
                    title: 'Loyalty.blades.loyalty-program-details.labels.priority',
                    colSpan: 3,
                    valueType: 'Integer'
                },
                {
                    name: 'name',
                    title: 'Loyalty.blades.loyalty-program-details.labels.name',
                    placeholder: 'Loyalty.blades.loyalty-program-details.placeholders.name',
                    colSpan: 6,
                    valueType: 'ShortText',
                    isRequired: true
                },
                {
                    title: 'Loyalty.blades.loyalty-program-details.labels.localized-names',
                    placeholder: 'Loyalty.blades.loyalty-program-details.placeholders.name',
                    colSpan: 6,
                    templateUrl: 'loyaltyLocalizedName.html'
                },
                {
                    name: 'storeId',
                    colSpan: 6,
                    title: 'Loyalty.blades.loyalty-program-details.labels.store',
                    templateUrl: 'loyaltyStoreSelector.html'
                },
                {
                    name: 'startDate',
                    title: 'Loyalty.blades.loyalty-program-details.labels.start-date',
                    colSpan: 3,
                    valueType: 'DateTime'
                },
                {
                    name: 'endDate',
                    title: 'Loyalty.blades.loyalty-program-details.labels.end-date',
                    colSpan: 3,
                    valueType: 'DateTime'
                },
            ]);

            metaFormsService.registerMetaFields("loyaltyProgramFilterDetail", [
                {
                    name: 'onlyActive',
                    title: "Loyalty.blades.filter-detail.labels.is-active",
                    valueType: "Boolean"
                },
                {
                    title: 'Loyalty.blades.filter-detail.labels.store',
                    templateUrl: "loyalty-filter-store-selector.html"
                }
            ]);

            metaFormsService.registerMetaFields('loyaltyMissionDetail', [
                {
                    name: 'status',
                    title: 'Loyalty.blades.loyalty-mission-details.labels.status',
                    colSpan: 3,
                    templateUrl: 'loyaltyMissionStatusSelector.html'
                },
                {
                    name: 'public',
                    title: 'Loyalty.blades.loyalty-mission-details.labels.public',
                    colSpan: 3,
                    valueType: 'Boolean'
                },
                {
                    name: 'name',
                    title: 'Loyalty.blades.loyalty-mission-details.labels.name',
                    placeholder: 'Loyalty.blades.loyalty-mission-details.placeholders.name',
                    colSpan: 6,
                    valueType: 'ShortText',
                    isRequired: true
                },
                {
                    title: 'Loyalty.blades.loyalty-mission-details.labels.localized-names',
                    placeholder: 'Loyalty.blades.loyalty-mission-details.placeholders.name',
                    colSpan: 6,
                    templateUrl: 'loyaltyMissionLocalizedName.html'
                },
                {
                    title: 'Loyalty.blades.loyalty-mission-details.labels.localized-descriptions',
                    colSpan: 6,
                    templateUrl: 'loyaltyMissionLocalizedDescription.html'
                },
                {
                    name: 'storeId',
                    colSpan: 6,
                    title: 'Loyalty.blades.loyalty-mission-details.labels.store',
                    templateUrl: 'loyaltyMissionStoreSelector.html'
                },
                {
                    name: 'startDate',
                    title: 'Loyalty.blades.loyalty-mission-details.labels.start-date',
                    colSpan: 3,
                    valueType: 'DateTime'
                },
                {
                    name: 'endDate',
                    title: 'Loyalty.blades.loyalty-mission-details.labels.end-date',
                    colSpan: 3,
                    valueType: 'DateTime'
                },
            ]);

            // Register dynamic expression tree templates
            // Conditions
            const order = 'Order conditions';
            const special = 'Special conditions';

            dynamicExpressionService.registerExpression({
                id: 'BlockLoyaltyCondition',
                newChildLabel: 'Add condition',
                getValidationError: function () {
                    var errorMessage = (this.children && this.children.length) ? undefined : 'Your loyalty program must have at least one condition';
                    return errorMessage;
                },
            });

            dynamicExpressionService.registerExpression({
                id: 'BlockLoyaltyMissionCondition',
                newChildLabel: 'Add condition',
                getValidationError: function () {
                    var goals = _.filter(this.children, function (child) { return !!child.missionType; });
                    if (goals.length === 0) {
                        return 'Your mission must have exactly one goal: order value, order count or SKU-based';
                    }
                    if (goals.length > 1) {
                        return 'Your mission must have only one goal';
                    }
                    return undefined;
                },
            });

            dynamicExpressionService.registerExpression({
                groupName: order,
                id: 'OrderStatusCondition',
                displayName: 'Order status is...',
            });

            dynamicExpressionService.registerExpression({
                groupName: order,
                id: 'OrderTotalCondition',
                displayName: 'Order total is...',
            });

            dynamicExpressionService.registerExpression({
                groupName: order,
                id: 'IsFirstOrderCondition',
                displayName: 'Is first order',
            });

            dynamicExpressionService.registerExpression({
                groupName: order,
                id: 'IsRecurringOrderCondition',
                displayName: 'Is recurring order',
            });

            dynamicExpressionService.registerExpression({
                groupName: special,
                id: 'IsRegistrationCondition',
                displayName: 'Registration',
            });

            dynamicExpressionService.registerExpression({
                groupName: 'Shopper profile',
                id: 'AnyUserGroupCondition',
                displayName: 'Any User Group',
            });

            // Rewards
            dynamicExpressionService.registerExpression({
                id: 'BlockLoyaltyReward',
                newChildLabel: 'Add reward',
                getValidationError: function () {
                    return (this.children && this.children.length) ? undefined : 'Your loyalty program must have at least one reward';
                },
            });

            dynamicExpressionService.registerExpression({
                id: 'FixedAmountReward',
                displayName: 'Earn fixed amount of points per order',
            });

            dynamicExpressionService.registerExpression({
                id: 'RelativeAmountReward',
                displayName: 'Earn % of order value as points',
            });

            // Mission goals
            const missionGoal = 'Mission goal';

            dynamicExpressionService.registerExpression({
                groupName: missionGoal,
                id: 'OrderValueGoal',
                displayName: 'Reach target order value',
            });

            dynamicExpressionService.registerExpression({
                groupName: missionGoal,
                id: 'OrderCountGoal',
                displayName: 'Reach target number of orders',
            });

            dynamicExpressionService.registerExpression({
                groupName: missionGoal,
                id: 'PerSkuGoal',
                displayName: 'Purchase target quantity of SKUs',
            });

            dynamicTemplateService.ensureTemplateLoaded('Modules/$(VirtoCommerce.Loyalty)/Scripts/dynamicConditions/templates.html');
            dynamicTemplateService.ensureTemplateLoaded('Modules/$(VirtoCommerce.Loyalty)/Scripts/blades/templates.html');
        }
    ]);
