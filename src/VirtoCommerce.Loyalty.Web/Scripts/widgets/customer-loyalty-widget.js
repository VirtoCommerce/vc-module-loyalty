angular.module('VirtoCommerce.Loyalty')
    .controller('VirtoCommerce.Loyalty.customerLoyaltyWidgetController',
        ['$scope', '$filter', 'platformWebApp.bladeNavigationService', 'VirtoCommerce.Loyalty.loyaltyProgramUsages',
            function ($scope, $filter, bladeNavigationService, loyaltyProgramUsages) {
                var blade = $scope.widget.blade;

                var searchCriteria = {};
                if (blade.currentEntity) {
                    var account = _.first(blade.currentEntity.securityAccounts)
                    if (account) {
                        searchCriteria.userId = account.id;
                    }
                }

                function refresh() {
                    $scope.balance = 0;

                    if (!searchCriteria.userId) {
                        return;
                    }

                    loyaltyProgramUsages.getBalance({ userId: searchCriteria.userId }, function (data) {
                        $scope.balance = data.balance;
                    });
                }

                $scope.openBlade = function () {
                    if (!searchCriteria.userId) {
                        return;
                    }

                    var newBlade = {
                        id: 'customerLoyaltyBlade',
                        title: 'Loyalty.blades.loyalty-program-customer-usage-list.title',
                        searchCriteria: searchCriteria,
                        controller: 'VirtoCommerce.Loyalty.loyaltyProgramUsageListController',
                        template: 'Modules/$(VirtoCommerce.Loyalty)/Scripts/blades/loyalty-program-usage-list.html'
                    };
                    bladeNavigationService.showBlade(newBlade, blade);
                };

                refresh()
            }]);
