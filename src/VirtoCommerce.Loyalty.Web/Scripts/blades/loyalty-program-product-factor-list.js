angular.module('VirtoCommerce.Loyalty')
    .controller('VirtoCommerce.Loyalty.loyaltyProgramProductFactorListController', [
        '$scope', 'platformWebApp.bladeUtils', 'platformWebApp.uiGridHelper', 'platformWebApp.ui-grid.extension', 'platformWebApp.bladeNavigationService',
        'VirtoCommerce.Loyalty.loyaltyProgramProductFactors',
        function ($scope, bladeUtils, uiGridHelper, gridOptionExtension, bladeNavigationService, loyaltyProgramProductFactors) {
            var blade = $scope.blade;
            blade.headIcon = 'fa fa-star';
            blade.updatePermission = 'loyalty:update';

            blade.refresh = function () {
                blade.isLoading = true;

                var criteria = {
                    loyaltyProgramId: blade.loyaltyProgramId,
                    sort: uiGridHelper.getSortExpression($scope),
                    skip: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
                    take: $scope.pageSettings.itemsPerPageCount
                };

                loyaltyProgramProductFactors.search(criteria, function (data) {
                    blade.isLoading = false;

                    $scope.pageSettings.totalItems = data.totalCount;
                    $scope.listEntries = data.results;
                });
            };

            blade.toolbarCommands = [
                {
                    name: "platform.commands.refresh",
                    icon: 'fa fa-refresh',
                    executeMethod: blade.refresh,
                    canExecuteMethod: function () {
                        return true;
                    }
                },
                {
                    name: "platform.commands.add", icon: 'fas fa-plus',
                    executeMethod: openCatalogItemsSelect,
                    canExecuteMethod: function () {
                        return true;
                    },
                    permission: blade.updatePermission
                },
            ];

            function openCatalogItemsSelect() {
                $scope.selectedNodeId = null;
                var selectedProducts = [];
                var newBlade = {
                    id: 'CatalogItemsSelect',
                    title: 'Loyalty.blades.select-loyalty-program-products-list.title',
                    controller: 'virtoCommerce.catalogModule.catalogItemSelectController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/common/catalog-items-select.tpl.html',
                    breadcrumbs: [],
                    toolbarCommands: [
                        {
                            name: 'Loyalty.blades.select-loyalty-program-products-list.commands.add',
                            icon: 'fas fa-plus',
                            executeMethod: function (blade) {
                                addProductsToLoyaltyProgram(selectedProducts, blade);
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

                loyaltyProgramProductFactors.search({
                    loyaltyProgramId: blade.loyaltyProgramId,
                    productIds: _.pluck(products, 'id')
                }, function (data) {
                    var newItems = _.filter(products, function (product) {
                        return _.all(data.results, function (x) {
                            return x.productId !== product.id;
                        })
                    });

                    var productFactors = _.map(newItems, function (x) {
                        return {
                            productId: x.id,
                            factor: 1,
                            loyaltyProgramId: blade.loyaltyProgramId
                        };
                    });

                    loyaltyProgramProductFactors.updateFactors(productFactors, function () {
                        bladeNavigationService.closeBlade(currentBlade);
                        blade.refresh();
                        if (blade.parentRefresh) {
                            blade.parentRefresh();
                        }
                    }, function (error) {
                        bladeNavigationService.setError('Error ' + error.status, blade);
                    });
                }, function (error) {
                    bladeNavigationService.setError('Error ' + error.status, blade);
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
