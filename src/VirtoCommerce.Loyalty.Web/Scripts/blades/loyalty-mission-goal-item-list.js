angular.module('VirtoCommerce.Loyalty')
    .controller('VirtoCommerce.Loyalty.loyaltyMissionGoalItemListController', [
        '$scope', 'platformWebApp.bladeUtils', 'platformWebApp.uiGridHelper', 'platformWebApp.ui-grid.extension',
        'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService', 'VirtoCommerce.Loyalty.loyaltyMissionGoalItems',
        'virtoCommerce.storeModule.stores',
        function ($scope, bladeUtils, uiGridHelper, gridOptionExtension, bladeNavigationService, dialogService, loyaltyMissionGoalItems, stores) {
            var blade = $scope.blade;
            blade.headIcon = 'fa fa-flag-checkered';
            blade.updatePermission = 'loyalty:update';

            blade.refresh = function () {
                blade.isLoading = true;

                var criteria = {
                    missionId: blade.missionId,
                    sort: uiGridHelper.getSortExpression($scope),
                    skip: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
                    take: $scope.pageSettings.itemsPerPageCount
                };

                loyaltyMissionGoalItems.search(criteria, function (data) {
                    blade.isLoading = false;
                    $scope.pageSettings.totalItems = data.totalCount;
                    $scope.listEntries = data.results;
                    blade.originalEntries = angular.copy(data.results);

                    if (blade.storeId) {
                        stores.get({ id: blade.storeId }, (storeData) => {
                            blade.catalogId = storeData.catalog;
                        });
                    }
                });
            };

            function getModifiedEntries() {
                if (!$scope.listEntries || !blade.originalEntries) {
                    return [];
                }
                return _.filter($scope.listEntries, function (entry) {
                    if (!entry.id) {
                        return true;
                    }
                    var original = _.findWhere(blade.originalEntries, { id: entry.id });
                    return original && (+entry.quantity !== +original.quantity || !angular.equals(_.omit(entry, 'quantity'), _.omit(original, 'quantity')));
                });
            }

            function isDirty() {
                return getModifiedEntries().length > 0;
            }

            var formScope;
            $scope.setForm = function (form) {
                formScope = form;
            };

            function saveChanges() {
                var modified = getModifiedEntries();
                if (!modified.length) {
                    return;
                }

                var payload = _.map(modified, function (entry) {
                    return angular.extend({}, entry, { quantity: +entry.quantity });
                });

                blade.isLoading = true;
                loyaltyMissionGoalItems.updateItems(payload, function () {
                    blade.refresh();
                    if (angular.isFunction(blade.parentWidgetRefresh)) {
                        blade.parentWidgetRefresh();
                    }
                }, function (error) {
                    blade.isLoading = false;
                    bladeNavigationService.setError('Error ' + error.status, blade);
                });
            }

            $scope.deleteList = function (list) {
                var dialog = {
                    id: "confirmDeleteItem",
                    title: "Loyalty.dialogs.loyalty-mission-goal-item-delete.title",
                    message: "Loyalty.dialogs.loyalty-mission-goal-item-delete.message",
                    callback: function (remove) {
                        if (remove) {
                            blade.isLoading = true;
                            var itemIds = _.pluck(_.filter(list, function (x) { return x.id; }), 'id');
                            if (!itemIds.length) {
                                // only unsaved rows selected — just drop them locally
                                $scope.listEntries = _.difference($scope.listEntries, list);
                                blade.isLoading = false;
                                return;
                            }
                            loyaltyMissionGoalItems.delete({ ids: itemIds }, function () {
                                blade.refresh();
                                if (angular.isFunction(blade.parentWidgetRefresh)) {
                                    blade.parentWidgetRefresh();
                                }
                            }, function (error) {
                                blade.isLoading = false;
                                bladeNavigationService.setError('Error ' + error.status, blade);
                            });
                        }
                    }
                };
                dialogService.showConfirmationDialog(dialog);
            };

            blade.toolbarCommands = [
                {
                    name: "platform.commands.save",
                    icon: 'fas fa-save',
                    executeMethod: saveChanges,
                    canExecuteMethod: function () {
                        return isDirty() && formScope && formScope.$valid;
                    },
                    permission: blade.updatePermission
                },
                {
                    name: "platform.commands.refresh",
                    icon: 'fa fa-refresh',
                    executeMethod: blade.refresh,
                    canExecuteMethod: function () { return true; }
                },
                {
                    name: "platform.commands.add", icon: 'fas fa-plus',
                    executeMethod: openCatalogItemsSelect,
                    canExecuteMethod: function () { return blade.catalogId; },
                    permission: blade.updatePermission
                },
                {
                    name: "platform.commands.delete", icon: 'fas fa-trash-alt',
                    executeMethod: function () {
                        $scope.deleteList($scope.gridApi.selection.getSelectedRows());
                    },
                    canExecuteMethod: function () {
                        return $scope.gridApi && _.any($scope.gridApi.selection.getSelectedRows());
                    },
                    permission: 'loyalty:delete'
                },
            ];

            blade.onClose = function (closeCallback) {
                bladeNavigationService.showConfirmationIfNeeded(isDirty(), true, blade, saveChanges, closeCallback,
                    "Loyalty.dialogs.loyalty-mission-save.title", "Loyalty.dialogs.loyalty-mission-save.message");
            };

            function openCatalogItemsSelect() {
                $scope.selectedNodeId = null;
                var selectedProducts = [];
                var newBlade = {
                    id: 'CatalogItemsSelect',
                    title: 'Loyalty.blades.loyalty-mission-goal-item-list.select-products',
                    controller: 'virtoCommerce.catalogModule.catalogItemSelectController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/common/catalog-items-select.tpl.html',
                    breadcrumbs: [],
                    catalogId: blade.catalogId,
                    toolbarCommands: [
                        {
                            name: 'platform.commands.add',
                            icon: 'fas fa-plus',
                            executeMethod: function (catalogSelectBlade) {
                                addProductsToMission(selectedProducts, catalogSelectBlade);
                            },
                            canExecuteMethod: function () {
                                return selectedProducts.length > 0;
                            }
                        }]
                };

                newBlade.options = {
                    allowCheckingCategory: false,
                    checkItemFn: function (listItem, isSelected) {
                        if (isSelected) {
                            if (_.all(selectedProducts, function (x) { return x.id !== listItem.id; })) {
                                selectedProducts.push(listItem);
                            }
                        } else {
                            selectedProducts = _.reject(selectedProducts, function (x) { return x.id === listItem.id; });
                        }
                        newBlade.error = undefined;
                    }
                };

                bladeNavigationService.showBlade(newBlade, blade);
            }

            function addProductsToMission(products, currentBlade) {
                currentBlade.isLoading = true;

                loyaltyMissionGoalItems.search({
                    missionId: blade.missionId,
                    productIds: _.pluck(products, 'id')
                }, function (data) {
                    var alreadyPresentIds = _.uniq(
                        _.pluck(data.results, 'productId')
                            .concat(_.pluck($scope.listEntries || [], 'productId'))
                    );

                    var newItems = _.filter(products, function (product) {
                        return !_.contains(alreadyPresentIds, product.id);
                    });

                    var goalItems = _.map(newItems, function (x) {
                        return {
                            productId: x.id,
                            productCode: x.code,
                            productName: x.name,
                            quantity: 1,
                            missionId: blade.missionId
                        };
                    });

                    $scope.listEntries = ($scope.listEntries || []).concat(goalItems);
                    $scope.pageSettings.totalItems += goalItems.length;

                    currentBlade.isLoading = false;
                    bladeNavigationService.closeBlade(currentBlade);
                }, function (error) {
                    currentBlade.isLoading = false;
                    bladeNavigationService.setError('Error ' + error.status, currentBlade);
                });
            }

            $scope.setGridOptions = function (gridId, gridOptions) {
                $scope.gridOptions = gridOptions;
                gridOptionExtension.tryExtendGridOptions(gridId, gridOptions);

                uiGridHelper.initialize($scope, gridOptions, function (gridApi) {
                    uiGridHelper.bindRefreshOnSortChanged($scope);
                });

                gridOptions.onRegisterApi = function (gridApi) {
                    $scope.gridApi = gridApi;
                };

                bladeUtils.initializePagination($scope);

                return gridOptions;
            };
        }]);
