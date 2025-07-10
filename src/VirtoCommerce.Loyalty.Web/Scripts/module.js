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
                                controller: 'VirtoCommerce.Loyalty.loyaltyController',
                                template: 'Modules/$(VirtoCommerce.Loyalty)/Scripts/blades/loyalty-programs-list.html',
                                isClosingDisabled: true,
                            };
                            bladeNavigationService.showBlade(newBlade);
                        }
                    ]
                });
        }
    ])
    .run(['platformWebApp.mainMenuService', '$state', 'virtoCommerce.marketingModule.marketingMenuItemService',
        function (mainMenuService, $state, marketingMenuItemService) {
            marketingMenuItemService.register({
                id: 'loayltyProgramItemService',
                name: 'loyalty.main-menu-title',
                entityName: 'loayaltyProgram',
                icon: 'fa fa-a',
                controller: 'VirtoCommerce.Loyalty.loyaltyController',
                template: 'Modules/$(virtoCommerce.Loyalty)/Scripts/blades/loyalty-programs-list.html',
                permission: 'loyalty:access:access'
            });
        }
    ]);
