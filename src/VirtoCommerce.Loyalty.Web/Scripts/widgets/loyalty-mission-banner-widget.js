angular.module('VirtoCommerce.Loyalty')
    .controller('VirtoCommerce.Loyalty.loyaltyMissionBannerWidgetController',
        ['$scope', 'platformWebApp.bladeNavigationService',
            function ($scope, bladeNavigationService) {
                var blade = $scope.widget.blade;

                $scope.openBlade = function () {
                    var newBlade = {
                        id: 'loyaltyMissionBanner',
                        currentEntity: blade.currentEntity,
                        controller: 'VirtoCommerce.Loyalty.loyaltyMissionBannerController',
                        template: 'Modules/$(VirtoCommerce.Loyalty)/Scripts/blades/loyalty-mission-banner.html'
                    };
                    bladeNavigationService.showBlade(newBlade, blade);
                };
            }]);
