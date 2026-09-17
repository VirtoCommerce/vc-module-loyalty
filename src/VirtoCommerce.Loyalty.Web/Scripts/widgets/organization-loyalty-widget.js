angular.module('VirtoCommerce.Loyalty')
    .controller('VirtoCommerce.Loyalty.organizationLoyaltyWidgetController',
        ['$scope', 'platformWebApp.bladeNavigationService', 'VirtoCommerce.Loyalty.loyaltyProgramOperationLogs',
            function ($scope, bladeNavigationService, loyaltyProgramOperationLogs) {
                var blade = $scope.widget.blade;

                var searchCriteria = {};
                if (blade.currentEntity) {
                    searchCriteria.organizationId = blade.currentEntity.id;
                }

                function refresh() {
                    $scope.balance = 0;

                    if (!searchCriteria.organizationId) {
                        return;
                    }

                    loyaltyProgramOperationLogs.getOrganizationBalance({ organizationId: searchCriteria.organizationId }, function (data) {
                        $scope.balance = data.balance;
                    });
                }

                $scope.openBlade = function () {
                    if (!searchCriteria.organizationId) {
                        return;
                    }

                    var newBlade = {
                        id: 'organizationLoyaltyBlade',
                        title: 'Loyalty.blades.loyalty-program-operation-log-list.title',
                        searchCriteria: searchCriteria,
                        controller: 'VirtoCommerce.Loyalty.loyaltyProgramOperationLogListController',
                        template: 'Modules/$(VirtoCommerce.Loyalty)/Scripts/blades/loyalty-program-operation-log-list.html'
                    };
                    bladeNavigationService.showBlade(newBlade, blade);
                };

                refresh()
            }]);
