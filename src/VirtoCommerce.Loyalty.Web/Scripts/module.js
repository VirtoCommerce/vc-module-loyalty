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
                                id: 'blade1',
                                controller: 'VirtoCommerce.Loyalty.helloWorldController',
                                template: 'Modules/$(VirtoCommerce.Loyalty)/Scripts/blades/loyalty-programs-list.html',
                                isClosingDisabled: true,
                            };
                            bladeNavigationService.showBlade(newBlade);
                        }
                    ]
                });
        }
    ])
    .run(['platformWebApp.mainMenuService', '$state',
        function (mainMenuService, $state) {
            //Register module in main menu
            var menuItem = {
                path: 'browse/loyalty',
                icon: 'fa fa-cube',
                title: 'Loyalty.main-menu-title',
                priority: 100,
                action: function () { $state.go('workspace.LoyaltyState'); },
                permission: 'loyalty:access',
            };
            mainMenuService.addMenuItem(menuItem);
        }
    ]);
