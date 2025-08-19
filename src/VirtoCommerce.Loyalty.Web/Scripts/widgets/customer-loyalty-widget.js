angular.module('VirtoCommerce.Loyalty')
    .controller('VirtoCommerce.Loyalty.customerLoyaltyWidgetController',
        ['$scope', 'platformWebApp.bladeNavigationService', 'VirtoCommerce.Loyalty.loyaltyProgramOperationLogs',
            function ($scope, bladeNavigationService, loyaltyProgramOperationLogs) {
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
                    loyaltyProgramOperationLogs.getBalance({ userId: searchCriteria.userId }, function (data) {
                        $scope.balance = data.balance;
                    });
                }

                $scope.openBlade = function () {
                    if (!searchCriteria.userId) {
                        return;
                    }

                    var newBlade = {
                        id: 'customerLoyaltyBlade',
                        title: 'Loyalty.blades.loyalty-program-operation-log-list.title',
                        searchCriteria: searchCriteria,
                        controller: 'VirtoCommerce.Loyalty.loyaltyProgramOperationLogListController',
                        template: 'Modules/$(VirtoCommerce.Loyalty)/Scripts/blades/loyalty-program-operation-log-list.html'
                    };
                    bladeNavigationService.showBlade(newBlade, blade);
                };

                refresh()
            }]);
