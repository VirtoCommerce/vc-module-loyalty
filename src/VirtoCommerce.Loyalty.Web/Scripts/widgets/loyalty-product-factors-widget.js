angular.module('VirtoCommerce.Loyalty')
    .controller('VirtoCommerce.Loyalty.loyaltyProgramProductFactorsWidgetController',
        ['$scope', 'platformWebApp.bladeNavigationService', 'VirtoCommerce.Loyalty.loyaltyProgramProductFactors',
            function ($scope, bladeNavigationService, loyaltyProgramProductFactors) {
                var blade = $scope.widget.blade;

                function refresh() {
                    if (!blade.currentEntityId) {
                        return;
                    }

                    loyaltyProgramProductFactors.search({
                        loyaltyProgramId: blade.currentEntityId,
                        take: 0
                    }, function (data) {
                        $scope.count = data.totalCount;
                    });
                }

                $scope.openBlade = function () {
                    var newBlade = {
                        id: 'loyaltyProgramProductFactorList',
                        title: 'Loyalty.blades.loyalty-program-product-factor-list.title',
                        loyaltyProgramId: blade.currentEntityId,
                        controller: 'VirtoCommerce.Loyalty.loyaltyProgramProductFactorListController',
                        template: 'Modules/$(VirtoCommerce.Loyalty)/Scripts/blades/loyalty-program-product-factor-list.html',
                        parentWidgetRefresh: refresh
                    };
                    bladeNavigationService.showBlade(newBlade, blade);
                };

                refresh();
            }]);
