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
                });
        }
    ])
    .run(['$state', 'platformWebApp.mainMenuService', 'platformWebApp.dynamicTemplateService', 'virtoCommerce.coreModule.common.dynamicExpressionService', 'platformWebApp.metaFormsService',
        function ($state, mainMenuService, dynamicTemplateService, dynamicExpressionService, metaFormsService) {
            // Register module in main menu
            var menuItem = {
                path: 'browse/loyalty',
                icon: 'fa fa-cube',
                title: 'Loyalty',
                priority: 100,
                action: function () { $state.go('workspace.LoyaltyState'); },
                permission: 'loyalty:access',
            };
            mainMenuService.addMenuItem(menuItem);

            // Register meta fields
            metaFormsService.registerMetaFields("loyaltyProgramDetail", [
                {
                    name: 'isActive',
                    title: "Loyalty.blades.loyalty-program-details.labels.is-active",
                    colSpan: 3,
                    valueType: "Boolean"
                },
                {
                    name: 'priority',
                    title: "Loyalty.blades.loyalty-program-details.labels.priority",
                    colSpan: 3,
                    valueType: "Integer"
                },
                {
                    name: 'name',
                    title: "Loyalty.blades.loyalty-program-details.labels.name",
                    colSpan: 6,
                    valueType: "ShortText"
                },
                {
                    name: 'startDate',
                    title: "Loyalty.blades.loyalty-program-details.labels.start-date",
                    colSpan: 3,
                    valueType: "DateTime"
                },
                {
                    name: 'endDate',
                    title: "Loyalty.blades.loyalty-program-details.labels.end-date",
                    colSpan: 3,
                    valueType: "DateTime"
                },
            ]);

            // Register dynamic expression tree templates
            // Conditions
            dynamicExpressionService.registerExpression({
                id: 'BlockLoyaltyCondition',
                newChildLabel: 'Add condition',
                getValidationError: function () {
                    return (this.children && this.children.length) ? undefined : 'Your loyalty program must have at least one condition';
                },
            });

            dynamicExpressionService.registerExpression({
                id: 'OrderStatusCondition',
                displayName: 'Order status is...',
            });

            dynamicExpressionService.registerExpression({
                id: 'OrderTotalCondition',
                displayName: 'Order total is...',
            });

            dynamicExpressionService.registerExpression({
                id: 'IsFirstOrderCondition',
                displayName: 'Is first order',
            });

            dynamicExpressionService.registerExpression({
                id: 'IsRecurringOrderCondition',
                displayName: 'Is recurring order',
            });

            dynamicExpressionService.registerExpression({
                id: 'IsRegistrationCondition',
                displayName: 'Registration',
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
                id: 'FixedPointsReward',
                displayName: 'earn fixed amount of points per order',
            });

            dynamicExpressionService.registerExpression({
                id: 'RelativeOrderValueReward',
                displayName: 'earn % of order value as points',
            });

            dynamicTemplateService.ensureTemplateLoaded('Modules/$(VirtoCommerce.Loyalty)/Scripts/dynamicConditions/templates.html');
        }
    ]);
