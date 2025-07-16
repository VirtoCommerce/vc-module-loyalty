angular.module('VirtoCommerce.Loyalty')
    .factory('VirtoCommerce.Loyalty.webApi', ['$resource', function ($resource) {
        return $resource('api/loyalty', {}, {
            create: { method: 'POST', url: 'api/loyalty' },
            update: { method: 'PUT', url: 'api/loyalty' },
            delete: { method: 'DELETE', url: 'api/loyalty' },
            get: { method: 'GET', url: 'api/loyalty/:id' },
            search: { method: 'POST', url: 'api/loyalty/search' },
            getNew: { url: 'api/loyalty/new' },
            getPointsByCustomerId: { method: 'GET', url: 'api/loyalty/points/:id' },
            trasactionsSearch: { method: 'POST', url: 'api/loyalty/transactions/search' },
        });
    }]);
