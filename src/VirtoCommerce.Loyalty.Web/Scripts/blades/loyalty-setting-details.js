angular.module('VirtoCommerce.Loyalty')
    .controller('VirtoCommerce.Loyalty.loyaltySettingDetailsController',
        ['$scope', '$q', '$timeout', 'platformWebApp.bladeNavigationService', 'VirtoCommerce.Loyalty.loyaltySetting', 'platformWebApp.settings', 'virtoCommerce.coreModule.currency.currencyApi',
            function ($scope, $q, $timeout, bladeNavigationService, loyaltySetting, settings, currency) {
                const blade = $scope.blade;
                blade.title = 'Loyalty.blades.loyalty-setting-details.title';
                blade.updatePermission = 'loyalty:update';
                blade.properties = [];
                $scope.loyaltySettingLoaded = false;

                blade.refresh = function () {
                    blade.isLoading = true;

                    if (blade.store) {
                        var getStorePromise = loyaltySetting.getByStore({ storeId: blade.store.id }).$promise.then(function (data) {
                            return data;
                        });
                        var getSettingPromise = settings.get({ id: 'Loyalty.Mode' }).$promise.then(function (data) {
                            return data;
                        });
                        var currenciesPromise = currency.query({}).$promise.then(function (data) {
                            return data;
                        });

                        $q.all([
                            getStorePromise,
                            getSettingPromise,
                            currenciesPromise
                        ]).then(function (results) {
                            if (results) {
                                initialize(results[0], results[1], results[2]);
                            }
                        });
                    }
                };

                function initialize(data, settings, currencies) {
                    blade.currentEntity = angular.copy(data);
                    blade.originalEntity = data;

                    if (settings && settings.allowedValues) {
                        blade.settings = _.map(settings.allowedValues, function (settingValue) {
                            return {
                                id: settingValue,
                                name: settingValue
                            };
                        });
                    }

                    if (currencies) {
                        blade.currencies = _.map(currencies, function (currency) {
                            return {
                                id: currency.code,
                                name: currency.code
                            }
                        });
                    }

                    $timeout(reset, 0);
                    blade.isLoading = false;
                }

                blade.saveChanges = function () {
                    blade.isLoading = true;

                    loyaltySetting.updateSetting(blade.currentEntity, function () {
                        blade.refresh();
                    }, function (error) {
                        blade.isLoading = false;
                        var errorText = (error && (error.message || error.statusText || error.status))
                            ? ('Error ' + (error.message || error.statusText || error.status))
                            : 'An error occurred while saving';
                        bladeNavigationService.setError(errorText, blade);
                    })
                };

                $scope.setForm = function (form) {
                    $scope.formScope = form;
                };

                function canSave() {
                    return isDirty() && $scope.formScope && $scope.formScope.$valid;
                }

                function isDirty() {
                    return (currentEntityIsDirty()) && blade.hasUpdatePermission();
                }

                function currentEntityIsDirty() {
                    return !angular.equals(blade.currentEntity, blade.originalEntity);
                }

                blade.fetchLoyaltyModes = function () {
                    return blade.settings;
                }

                blade.fetchCurrencies = function () {
                    return blade.currencies;
                }

                function reset() {
                    $scope.loyaltySettingLoaded = true;
                }

                blade.toolbarCommands = [
                    {
                        name: "platform.commands.save",
                        icon: 'fas fa-save',
                        executeMethod: blade.saveChanges,
                        canExecuteMethod: canSave,
                        permission: blade.updatePermission
                    }];

                blade.refresh();
            }]);
