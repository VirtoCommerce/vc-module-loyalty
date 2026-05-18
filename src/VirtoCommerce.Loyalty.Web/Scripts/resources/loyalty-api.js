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
    .factory('VirtoCommerce.Loyalty.loyaltyProgramOperationLogs', ['$resource', function ($resource) {
        return $resource('api/loyalty-program-operation-log/', {}, {
            getBalance: { method: 'GET', url: 'api/loyalty-program-operation-log/balance/:userId' },
            search: { url: 'api/loyalty-program-operation-log/search', method: 'POST' },
        });
    }])
    .factory('VirtoCommerce.Loyalty.loyaltySetting', ['$resource', function ($resource) {
        return $resource('api/loyalty-setting', {}, {
            getByStore: { method: 'GET', url: 'api/loyalty-setting/store/:storeId' },
            updateSetting: { method: 'PUT' }
        });
    }]);
