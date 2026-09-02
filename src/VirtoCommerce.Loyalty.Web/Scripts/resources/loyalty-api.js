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
            getBalance: { method: 'GET', url: 'api/loyalty-program-operation-log/balance/user/:userId' },
            getOrganizationBalance: { method: 'GET', url: 'api/loyalty-program-operation-log/balance/organization/:organizationId' },
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
    }])
    .factory('VirtoCommerce.Loyalty.loyaltyMissions', ['$resource', function ($resource) {
        return $resource('api/loyalty-missions/', {}, {
            get: { method: 'GET', url: 'api/loyalty-missions/:id' },
            getNew: { url: 'api/loyalty-missions/new' },
            search: { url: 'api/loyalty-missions/search', method: 'POST' },
            save: { method: 'POST' },
            update: { method: 'PUT' },
            delete: { method: 'DELETE' },
        });
    }])
    .factory('VirtoCommerce.Loyalty.loyaltyMissionGoalItems', ['$resource', function ($resource) {
        return $resource('api/loyalty-mission-goal-items/', {}, {
            get: { method: 'GET', url: 'api/loyalty-mission-goal-items/:id' },
            search: { url: 'api/loyalty-mission-goal-items/search', method: 'POST' },
            save: { method: 'POST' },
            update: { method: 'PUT' },
            updateItems: { url: 'api/loyalty-mission-goal-items/items', method: 'PUT' },
            delete: { method: 'DELETE' },
        });
    }])
    .factory('VirtoCommerce.Loyalty.loyaltyMissionProgress', ['$resource', function ($resource) {
        return $resource('api/loyalty-mission-progress/', {}, {
            get: { method: 'GET', url: 'api/loyalty-mission-progress/:id' },
            search: { url: 'api/loyalty-mission-progress/search', method: 'POST' },
        });
    }])
    .factory('VirtoCommerce.Loyalty.loyaltySetting', ['$resource', function ($resource) {
        return $resource('api/loyalty-setting', {}, {
            getByStore: { method: 'GET', url: 'api/loyalty-setting/store/:storeId' },
            updateSetting: { method: 'PUT' }
        });
    }]);
