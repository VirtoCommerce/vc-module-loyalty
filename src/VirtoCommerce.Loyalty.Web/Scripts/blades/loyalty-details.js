angular.module('VirtoCommerce.Loyalty')
    .controller(
        'VirtoCommerce.Loyalty.loyaltyDetailsController',
        [
            '$scope',
            'VirtoCommerce.Loyalty.webApi',
            'platformWebApp.bladeNavigationService',
            'virtoCommerce.storeModule.stores',
            '$q',
            'platformWebApp.settings',
            'platformWebApp.bladeNavigationService',
            function (
                $scope,
                loyaltyApi,
                bladeNavigationService,
                stores,
                $q,
                settings,
                bladeNavigationService) {
                const blade = $scope.blade;
                blade.updatePermission = 'loyalty:update';
                var promise = settings.getValues({ id: 'VirtoCommerce.Core.General.Languages' }).$promise;
                $scope.languages = [];

                blade.showErrorStoreStateMessage = null;

                function initializeLanguages() {
                    promise.then(function (promiseData) {
                        $scope.languages = promiseData;
                    });
                }

                blade.refresh = function () {
                    if (!blade.isNew) {
                        loyaltyApi.get({ id: [blade.itemId] }, function (getResult) {
                            blade.originalEntity = angular.copy(getResult);
                            blade.currentEntity = angular.copy(getResult);
                            initializeLanguages();
                        });
                    }
                    else {
                        loyaltyApi.getNew((getResult) => {
                            blade.currentEntity = getResult;
                            initializeToolbar();
                            initializeTitle();
                            initializeLanguages();
                        });
                    }
                    blade.isLoading = false;
                }

                blade.onClose = function (closeCallback) {
                    bladeNavigationService.showConfirmationIfNeeded(
                        isDirty() && !blade.isNew,
                        $scope.isValid(),
                        blade,
                        $scope.saveChanges,
                        closeCallback,
                        'loyalty.dialogs.loyalty-program-save.title',
                        'loyalty.dialogs.loyalty-program-save.message'
                    );
                };

                $scope.isValid = function () {
                    return isDirty()
                        && $scope.formScope
                        && $scope.formScope.$valid
                        && $scope.validateStores();
                };

                /// TODO: refactor to new va-marketing-stores-selector directive.
                // PageSize amount must be enough to show scrollbar in dropdown list container.
                // If scrollbar doesn't appear auto loading won't work.
                $scope.pageSize = 25;
                $scope.stores = [];
                var lastSearchPhrase = '';
                var totalCount = 0;

                $scope.fetchStores = function ($select) {
                    $q.all([loadLoyaltyStores(), $scope.fetchNextStores($select)]);
                };

                function loadLoyaltyStores() {
                    if (_.any(blade.currentEntity?.storeIds) && !_.any($scope.stores)) {
                        return stores.search({
                            storeIds: blade.currentEntity.storeIds,
                            take: blade.currentEntity.storeIds.length,
                            responseGroup: 'none'
                        }, (data) => {
                            joinStores(data.results);
                        }).$promise;
                    }

                    return $q.resolve();
                }

                $scope.fetchNextStores = ($select) => {
                    $select.page = $select.page || 0;

                    if (lastSearchPhrase !== $select.search) {
                        lastSearchPhrase = $select.search;
                        $select.page = 0;
                    }

                    if ($select.page === 0 || totalCount > $scope.stores.length) {
                        return stores.search(
                            {
                                searchPhrase: $select.search,
                                take: $scope.pageSize,
                                skip: $select.page * $scope.pageSize,
                                responseGroup: 'none'
                            }, (data) => {
                                joinStores(data.results);
                                $select.page++;

                                if ($select.page * $scope.pageSize < data.totalCount) {
                                    $scope.$broadcast('scrollCompleted');
                                }

                                totalCount = Math.max(totalCount, data.totalCount);
                            }).$promise;
                    }

                    return $q.resolve();
                };

                $scope.validateStores = function () {
                    if (!$scope.stores.length) {
                        return true;
                    }
                    const unknownStores = _.difference(blade.currentEntity?.storeIds, _.pluck($scope.stores, 'id'));
                    if (unknownStores.length) {
                        $("#storesContainer .ui-select-container").addClass("ng-invalid");
                        if (blade.showErrorStoreStateMessage === null) {
                            blade.showErrorStoreStateMessage = true;
                        }
                    } else {
                        $("#storesContainer .ui-select-container").removeClass("ng-invalid");
                    }
                    return !unknownStores.length && !blade.showErrorStoreStateMessage;
                };

                function joinStores(newItems) {
                    newItems = _.reject(newItems, x => _.any($scope.stores, y => y.id === x.id));
                                       
                    $scope.stores = $scope.stores.concat(newItems);
                    initStoreStateErrorMessage();
                    $scope.validateStores();
                }

                function initStoreStateErrorMessage() {
                    blade.showErrorStoreStateMessage = bladeNavigationService.checkPermission() // isAdmin
                        ? false
                        : null;
                }

                //scope functions
                // datepicker
                $scope.datepickers = {
                    str: false,
                    end: false
                };

                $scope.open = function ($event, which) {
                    $event.preventDefault();
                    $event.stopPropagation();

                    $scope.datepickers[which] = true;
                };

                let formScope;
                $scope.setForm = function (form) {
                    formScope = form;
                };

                $scope.saveChanges = function () {
                    bladeNavigationService.setError(null, blade);
                    blade.isLoading = true;

                    if (blade.isNew) {
                        loyaltyApi.create(blade.currentEntity, function (createResult) {
                            blade.parentBlade.refresh(true);
                            blade.isNew = false;
                            blade.itemId = createResult.id;
                            initializeToolbar();
                            initializeTitle();
                            blade.refresh();
                            blade.isLoading = false;
                        }, function (error) {
                            bladeNavigationService.setError('Error ' + error.status, blade);
                            blade.isLoading = false;
                        });
                    }
                    else {
                        loyaltyApi.update(blade.currentEntity, function (updateResult) {
                            blade.parentBlade.refresh(true);
                            blade.originalEntity = angular.copy(blade.currentEntity);
                            blade.isLoading = false;
                        }, function (error) {
                            bladeNavigationService.setError('Error ' + error.status, blade);
                            blade.isLoading = false;
                        });
                    }
                }

                function initializeTitle() {
                    blade.title = blade.isNew ? 'loyalty.blades.loyalty-program-details.title-add' : 'loyalty.blades.loyalty-program-details.title-edit';
                }

                function isDirty() {
                    return !angular.equals(blade.currentEntity, blade.originalEntity) && blade.hasUpdatePermission();
                }

                $scope.cancelChanges = function () {
                    $scope.bladeClose();
                };

                function canSave() {
                    return isDirty() && formScope && formScope.$valid;
                }

                function reset() {
                    angular.copy(blade.originalEntity, blade.currentEntity);
                }

                function initializeToolbar() {
                    blade.toolbarCommands = [
                        {
                            name: 'platform.commands.save',
                            icon: 'fas fa-save',
                            executeMethod: function () {
                                $scope.saveChanges();
                            },
                            canExecuteMethod: canSave,
                            permission: getSavePermission()
                        }
                    ];

                    if (!blade.isNew) {
                        blade.toolbarCommands.push({
                            name: "platform.commands.reset",
                            icon: 'fa fa-undo',
                            executeMethod: reset,
                            canExecuteMethod: isDirty
                        });
                    }
                }

                function getSavePermission() {
                    return blade.isNew ? 'loyalty:create' : 'loyalty:update';
                }

                // initial calls
                initializeToolbar();
                initializeTitle();
                blade.refresh();
        }]);
