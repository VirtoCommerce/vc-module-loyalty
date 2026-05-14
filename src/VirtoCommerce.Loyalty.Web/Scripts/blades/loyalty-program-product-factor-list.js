angular.module('VirtoCommerce.Loyalty')
    .controller('VirtoCommerce.Loyalty.loyaltyProgramProductFactorListController', [
        '$scope', 'platformWebApp.bladeUtils', 'platformWebApp.uiGridHelper', 'platformWebApp.ui-grid.extension',
        'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService', 'VirtoCommerce.Loyalty.loyaltyProgramProductFactors',
        'virtoCommerce.storeModule.stores',
        function ($scope, bladeUtils, uiGridHelper, gridOptionExtension, bladeNavigationService, dialogService, loyaltyProgramProductFactors, stores) {
            var blade = $scope.blade;
            blade.headIcon = 'fa fa-star';
            blade.updatePermission = 'loyalty:update';

            blade.refresh = function () {
                blade.isLoading = true;

                var criteria = {
                    loyaltyProgramId: blade.loyaltyProgramId,
                    productIds: blade.productId ? [blade.productId] : undefined,
                    sort: uiGridHelper.getSortExpression($scope),
                    skip: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
                    take: $scope.pageSettings.itemsPerPageCount
                };

                loyaltyProgramProductFactors.search(criteria, function (data) {
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
                    // new (unsaved) entries have no id yet — always treat as modified
                    if (!entry.id) {
                        return true;
                    }
                    var original = _.findWhere(blade.originalEntries, { id: entry.id });
                    // factor input is bound to a string via ng-model; compare numerically to avoid false positives
                    return original && (+entry.factor !== +original.factor || !angular.equals(_.omit(entry, 'factor'), _.omit(original, 'factor')));
                });
            }

            function isDirty() {
                return getModifiedEntries().length > 0;
            }

            function saveChanges() {
                var modified = getModifiedEntries();
                if (!modified.length) {
                    return;
                }

                // normalize factor to a number before sending
                var payload = _.map(modified, function (entry) {
                    return angular.extend({}, entry, { factor: +entry.factor });
                });

                blade.isLoading = true;
                loyaltyProgramProductFactors.updateFactors(payload, function () {
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
                    title: "Loyalty.dialogs.loyalty-program-product-factor-delete.title",
                    message: "Loyalty.dialogs.loyalty-program-product-factor-delete.message",
                    callback: function (remove) {
                        if (remove) {
                            blade.isLoading = true;

                            var itemIds = _.pluck(list, 'id');
                            loyaltyProgramProductFactors.delete({ ids: itemIds }, function () {
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
                    canExecuteMethod: isDirty,
                    permission: blade.updatePermission
                },
                {
                    name: "platform.commands.refresh",
                    icon: 'fa fa-refresh',
                    executeMethod: blade.refresh,
                    canExecuteMethod: function () {
                        return true;
                    }
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

            // Add command only makes sense when scoped to a specific loyalty program
            if (blade.loyaltyProgramId) {
                blade.toolbarCommands.splice(2, 0, {
                    name: "platform.commands.add", icon: 'fas fa-plus',
                    executeMethod: openCatalogItemsSelect,
                    canExecuteMethod: function () {
                        return blade.catalogId;
                    },
                    permission: blade.updatePermission
                });
            }

            blade.onClose = function (closeCallback) {
                bladeNavigationService.showConfirmationIfNeeded(isDirty(), true, blade, saveChanges, closeCallback,
                    "platform.dialogs.unsaved-changes.title", "platform.dialogs.unsaved-changes.message");
            };

            function openCatalogItemsSelect() {
                $scope.selectedNodeId = null;
                var selectedProducts = [];
                var newBlade = {
                    id: 'CatalogItemsSelect',
                    title: 'Loyalty.blades.select-loyalty-program-products-list.title',
                    controller: 'virtoCommerce.catalogModule.catalogItemSelectController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/common/catalog-items-select.tpl.html',
                    breadcrumbs: [],
                    catalogId: blade.catalogId,
                    toolbarCommands: [
                        {
                            name: 'Loyalty.blades.select-loyalty-program-products-list.commands.add',
                            icon: 'fas fa-plus',
                            executeMethod: function (catalogSelectBlade) {
                                addProductsToLoyaltyProgram(selectedProducts, catalogSelectBlade);
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
                            if (_.all(selectedProducts, function (x) {
                                return x.id !== listItem.id;
                            })) {
                                selectedProducts.push(listItem);
                            }
                        }
                        else {
                            selectedProducts = _.reject(selectedProducts,
                                function (x) {
                                    return x.id === listItem.id;
                                });
                        }
                        newBlade.error = undefined;
                    }
                };

                bladeNavigationService.showBlade(newBlade, blade);
            }

            function addProductsToLoyaltyProgram(products, currentBlade) {
                currentBlade.isLoading = true;

                // Skip products that already have a saved factor in DB, or are already staged locally
                loyaltyProgramProductFactors.search({
                    loyaltyProgramId: blade.loyaltyProgramId,
                    productIds: _.pluck(products, 'id')
                }, function (data) {
                    var alreadyPresentIds = _.uniq(
                        _.pluck(data.results, 'productId')
                            .concat(_.pluck($scope.listEntries || [], 'productId'))
                    );

                    var newItems = _.filter(products, function (product) {
                        return !_.contains(alreadyPresentIds, product.id);
                    });

                    var productFactors = _.map(newItems, function (x) {
                        return {
                            // no id — getModifiedEntries() will pick it up as a new entry to save
                            productId: x.id,
                            productCode: x.code,
                            productName: x.name,
                            factor: 1,
                            loyaltyProgramId: blade.loyaltyProgramId
                        };
                    });

                    $scope.listEntries = ($scope.listEntries || []).concat(productFactors);
                    $scope.pageSettings.totalItems += productFactors.length;

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
