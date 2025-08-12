angular.module('VirtoCommerce.Loyalty')
    .controller('virtoCommerce.Loyalty.orderStatusConditionController', ['$scope', 'platformWebApp.settings', function ($scope, settings) {
        $scope.orderStatuses = settings.getValues({ id: 'Order.Status' });
    }])

