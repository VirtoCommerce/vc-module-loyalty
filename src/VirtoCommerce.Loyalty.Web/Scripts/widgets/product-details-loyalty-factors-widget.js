angular.module('VirtoCommerce.Loyalty')
    .controller('VirtoCommerce.Loyalty.productDetailsLoyaltyFactorsWidgetController',
        ['$scope', 'platformWebApp.bladeNavigationService', 'VirtoCommerce.Loyalty.loyaltyProgramProductFactors',
            function ($scope, bladeNavigationService, loyaltyProgramProductFactors) {
                var blade = $scope.widget.blade;

                function refresh() {
                    if (!blade.currentEntityId) {
                        return;
                    }

                    loyaltyProgramProductFactors.search({
                        productIds: [blade.currentEntityId],
                        take: 0
                    }, function (data) {
                        $scope.count = data.totalCount;
                    });
                }

                $scope.openBlade = function () {
                    var newBlade = {
                        id: 'productLoyaltyProgramList',
                        title: 'Loyalty.blades.product-loyalty-program-list.title',
                        productId: blade.currentEntityId,
                        controller: 'VirtoCommerce.Loyalty.productLoyaltyProgramListController',
                        template: 'Modules/$(VirtoCommerce.Loyalty)/Scripts/blades/product-loyalty-program-list.html',
                        parentWidgetRefresh: refresh
                    };
                    bladeNavigationService.showBlade(newBlade, blade);
                };

                refresh();
            }]);
