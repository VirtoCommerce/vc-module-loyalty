angular.module('VirtoCommerce.Loyalty')
    .factory('VirtoCommerce.Loyalty.loyaltyPrograms', ['$resource', function ($resource) {
        return $resource('api/loyalty-programs/', {}, {
            get: { method: 'GET', url: 'api/loyalty-programs/:id' },
            getNew: { url: 'api/loyalty-programs/new/:programType' },
            search: { url: 'api/loyalty-programs/search', method: 'POST' },
            save: { method: 'POST' },
            update: { method: 'PUT' },
            delete: { method: 'DELETE' },
        });
    }])
    .factory('VirtoCommerce.Loyalty.loyaltyProgramOperationLogs', ['$resource', function ($resource) {
        return $resource('api/loyalty-program-operation-log/', {}, {
            getBalance: { method: 'GET', url: 'api/loyalty-program-operation-log/balance/:userId' },
            search: { url: 'api/loyalty-program-operation-log/search', method: 'POST' },
        });
    }])
    .factory('VirtoCommerce.Loyalty.loyaltyProgramProductFactors', ['$resource', function ($resource) {
        return $resource('api/loyalty-program-product-factors/', {}, {
            get: { method: 'GET', url: 'api/loyalty-program-product-factors/:id' },
            search: { url: 'api/loyalty-program-product-factors/search', method: 'POST' },
            save: { method: 'POST' },
            update: { method: 'PUT' },
            updateFactors: { url: 'api/loyalty-program-product-factors/factors', method: 'PUT' },
            delete: { method: 'DELETE' },
        });
    }]);
