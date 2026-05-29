angular.module('VirtoCommerce.Loyalty')
    .controller('VirtoCommerce.Loyalty.loyaltySettingWidgetController', ['$scope', 'platformWebApp.bladeNavigationService',
        function ($scope, bladeNavigationService) {
            $scope.openBlade = function () {
                var newBlade = {
                    id: 'loyaltySettingDetails',
                    controller: 'VirtoCommerce.Loyalty.loyaltySettingDetailsController',
                    template: 'Modules/$(VirtoCommerce.Loyalty)/Scripts/blades/loyalty-setting-details.html',
                    store: $scope.blade.currentEntity,
                };

                bladeNavigationService.showBlade(newBlade, $scope.blade);
            };
        }]);
