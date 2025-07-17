angular.module('VirtoCommerce.Loyalty')
    .controller('virtoCommerce.Loyalty.customerLoyaltyProgramWidgetController', ['$scope', 'platformWebApp.bladeNavigationService', 'VirtoCommerce.Loyalty.webApi', function ($scope, bladeNavigationService, loyaltyApi) {
        const blade = $scope.widget.blade;

        if (blade.currentEntity) {
            var account = _.first(blade.currentEntity.securityAccounts)
            if (account) {
                $scope.customerId = account.id;
                loyaltyApi.getPointsByCustomerId({ id: $scope.customerId }, function (data) {
                    $scope.loyaltyPoints = data.points; 
                });
            }
        }

        $scope.openLoyaltyProgramsBlade = function () {
            var newBlade = {
                id: 'customerLoyaltyPrograms',
                title: 'loyalty.widgets.loyalty-programs.blade-title',
                controller: 'VirtoCommerce.Loyalty.transactionListController',
                template: 'Modules/$(VirtoCommerce.Loyalty)/Scripts/blades/transactions-list.html',
                customerId: $scope.customerId,
            };
            bladeNavigationService.showBlade(newBlade, $scope.blade);
        };
    }]);
