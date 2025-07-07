angular.module('VirtoCommerce.Loyalty')
    .factory('VirtoCommerce.Loyalty.webApi', ['$resource', function ($resource) {
        return $resource('api/loyalty');
    }]);
