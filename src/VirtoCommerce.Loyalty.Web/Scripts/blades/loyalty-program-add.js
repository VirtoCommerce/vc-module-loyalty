angular.module('VirtoCommerce.Loyalty')
    .controller('VirtoCommerce.Loyalty.loyaltyProgramAddController',
        ['$scope', 'platformWebApp.bladeNavigationService',
            function ($scope, bladeNavigationService) {
                var blade = $scope.blade;
                blade.headIcon = 'fa fa-star';
                blade.isLoading = false;

                blade.programTypes = [
                    {
                        type: 'Default',
                        name: 'Loyalty.blades.loyalty-program-add.types.order-loyalty.name',
                        description: 'Loyalty.blades.loyalty-program-add.types.order-loyalty.description'
                    },
                    {
                        type: 'ProductPoints',
                        name: 'Loyalty.blades.loyalty-program-add.types.product-points-loyalty.name',
                        description: 'Loyalty.blades.loyalty-program-add.types.product-points-loyalty.description'
                    }
                ];

                $scope.selectType = function (programType) {
                    var newBlade = {
                        id: 'listItemChild',
                        title: 'Loyalty.blades.loyalty-program-details.new-program',
                        subtitle: blade.subtitle,
                        isNew: true,
                        programType: programType.type,
                        currentEntity: { programType: programType.type },
                        controller: 'VirtoCommerce.Loyalty.loyaltyProgramDetailsController',
                        template: 'Modules/$(VirtoCommerce.Loyalty)/Scripts/blades/loyalty-program-details.html'
                    };
                    bladeNavigationService.showBlade(newBlade, blade);
                };
            }]);
