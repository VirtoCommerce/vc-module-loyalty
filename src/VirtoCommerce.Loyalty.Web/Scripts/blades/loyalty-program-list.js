angular.module('VirtoCommerce.Loyalty')
    .controller('VirtoCommerce.Loyalty.loyaltyProgramListController', [
        '$scope', '$localStorage', 'platformWebApp.dialogService', 'platformWebApp.bladeUtils', 'platformWebApp.uiGridHelper', 'platformWebApp.ui-grid.extension',
        'VirtoCommerce.Loyalty.loyaltyPrograms',
        function ($scope, $localStorage, dialogService, bladeUtils, uiGridHelper, gridOptionExtension, loyaltyPrograms) {
            $scope.$localStorage = $localStorage;
            var blade = $scope.blade;
            var bladeNavigationService = bladeUtils.bladeNavigationService;

            // simple and advanced filtering
            var filter = blade.filter = $scope.filter = {};

            if (!$localStorage.loyaltyProgramSearchFilters) {
                $localStorage.loyaltyProgramSearchFilters = [{ name: 'Loyalty.blades.loyalty-program-list.labels.new-filter' }];
            }
            if ($localStorage.loyaltyProgramFilterId) {
                filter.current = _.findWhere($localStorage.loyaltyProgramSearchFilters, { id: $localStorage.loyaltyProgramFilterId });
            }

            filter.change = function () {
                $localStorage.loyaltyProgramFilterId = filter.current ? filter.current.id : null;
                if (filter.current && !filter.current.id) {
                    filter.current = null;
                    showFilterDetailBlade({ isNew: true });
                } else {
                    bladeNavigationService.closeBlade({ id: 'filterDetail' });
                    filter.criteriaChanged();
                }
            };

            filter.edit = function () {
                if (filter.current) {
                    showFilterDetailBlade({ data: filter.current });
                }
            };

            function showFilterDetailBlade(bladeData) {
                var newBlade = {
                    id: 'filterDetail',
                    controller: 'VirtoCommerce.Loyalty.filterDetailController',
                    template: 'Modules/$(VirtoCommerce.Loyalty)/Scripts/blades/loyalty-program-filter-detail.html'
                };
                angular.extend(newBlade, bladeData);
                bladeNavigationService.showBlade(newBlade, blade);
            }

            function getSearchCriteria() {
                return {
                    keyword: filter.keyword ? filter.keyword : undefined,
                    sort: uiGridHelper.getSortExpression($scope),
                    skip: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
                    take: $scope.pageSettings.itemsPerPageCount
                };
            }

            blade.refresh = function () {
                blade.isLoading = true;

                var criteria = getSearchCriteria();

                if (filter.current) {
                    angular.extend(criteria, filter.current);
                }

                loyaltyPrograms.search(criteria, function (data) {
                    blade.isLoading = false;

                    $scope.pageSettings.totalItems = data.totalCount;
                    $scope.listEntries = data.results;
                });
            };

            $scope.selectNode = function (node) {
                $scope.selectedNodeId = node.id;

                var newBlade = {
                    id: 'loyaltyProgramBlade',
                    currentEntity: node,
                    currentEntityId: node.id,
                    title: node.name,
                    controller: 'VirtoCommerce.Loyalty.loyaltyProgramDetailsController',
                    template: 'Modules/$(VirtoCommerce.Loyalty)/Scripts/blades/loyalty-program-details.html'
                };

                bladeNavigationService.showBlade(newBlade, blade);
            };

            $scope.deleteList = function (list) {
                var dialog = {
                    id: "confirmDeleteItem",
                    title: "Loyalty.dialogs.loyalty-program-delete.title",
                    message: "Loyalty.dialogs.loyalty-program-delete.message",
                    callback: function (remove) {
                        if (remove) {
                            bladeNavigationService.closeChildrenBlades(blade, function () {
                                blade.isLoading = true;

                                var itemIds = _.pluck(list, 'id');
                                loyaltyPrograms.remove({ ids: itemIds }, function () {
                                    blade.refresh();
                                });
                            });
                        }
                    }
                };
                dialogService.showConfirmationDialog(dialog);
            };

            blade.headIcon = 'fa fa-area-chart';

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
                    executeMethod: function () {
                        bladeNavigationService.closeChildrenBlades(blade, function () {
                            var newBlade = {
                                id: 'listItemChild',
                                title: 'Loyalty.blades.loyalty-program-details.new-program',
                                subtitle: blade.subtitle,
                                isNew: true,
                                currentEntity: {},
                                controller: 'VirtoCommerce.Loyalty.loyaltyProgramDetailsController',
                                template: 'Modules/$(VirtoCommerce.Loyalty)/Scripts/blades/loyalty-program-details.html'
                            };
                            bladeNavigationService.showBlade(newBlade, blade);
                        });
                    },
                    canExecuteMethod: function () { return true; },
                    permission: 'loyalty:create'
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
                }
            ];

            filter.criteriaChanged = function () {
                if ($scope.pageSettings.currentPage > 1) {
                    $scope.pageSettings.currentPage = 1;
                } else {
                    blade.refresh();
                }
            };

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
