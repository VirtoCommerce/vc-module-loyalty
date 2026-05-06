angular.module('VirtoCommerce.Loyalty')
    .controller('VirtoCommerce.Loyalty.loyaltyProgramProductFactorsWidgetController',
        ['$scope', 'platformWebApp.bladeNavigationService',
            function ($scope, bladeNavigationService) {
                var blade = $scope.widget.blade;

                $scope.openBlade = function () {
                    var newBlade = {
                        id: 'loyaltyProgramProductFactorList',
                        title: 'Loyalty.blades.loyalty-program-product-factor-list.title',
                        loyaltyProgramId: blade.currentEntityId,
                        controller: 'VirtoCommerce.Loyalty.loyaltyProgramProductFactorListController',
                        template: 'Modules/$(VirtoCommerce.Loyalty)/Scripts/blades/loyalty-program-product-factor-list.html'
                    };
                    bladeNavigationService.showBlade(newBlade, blade);
                };
            }]);
