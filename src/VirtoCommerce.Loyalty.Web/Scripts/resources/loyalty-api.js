angular.module('VirtoCommerce.Loyalty')
    .factory('VirtoCommerce.Loyalty.loyaltyPrograms', ['$resource', function ($resource) {
        return $resource('api/loyalty-programs/', {}, {
            get: { method: 'GET', url: 'api/loyalty-programs/:id' },
            search: { url: 'api/loyalty-programs/search', method: 'POST' },
            getNew: { url: 'api/loyalty-programs/new' },
            save: { method: 'POST' },
            update: { method: 'PUT' },
            delete: { method: 'DELETE' },
        });
    }])
    .factory('VirtoCommerce.Loyalty.loyaltyProgramUsages', ['$resource', function ($resource) {
        return $resource('api/loyalty-program-usages/', {}, {
            getBalance: { method: 'GET', url: 'api/loyalty-program-usages/balance/:userId' },
            search: { url: 'api/loyalty-program-usages/search', method: 'POST' },
        });
    }]);
