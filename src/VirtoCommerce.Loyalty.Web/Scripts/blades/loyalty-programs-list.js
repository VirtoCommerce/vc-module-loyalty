angular.module('VirtoCommerce.Loyalty')
    .controller('VirtoCommerce.Loyalty.helloWorldController', ['$scope', 'VirtoCommerce.Loyalty.webApi', function ($scope, api) {
        var blade = $scope.blade;
        blade.title = 'Loyalty';

        blade.refresh = function () {
            api.get(function (data) {
                blade.title = 'Loyalty.blades.loyalti-programs-list.title';
                blade.data = data.result;
                blade.isLoading = false;
            });
        };

        blade.refresh();
    }]);
