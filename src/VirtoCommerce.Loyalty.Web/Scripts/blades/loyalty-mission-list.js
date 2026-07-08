angular.module('VirtoCommerce.Loyalty')
    .controller('VirtoCommerce.Loyalty.loyaltyMissionListController', [
        '$scope', 'platformWebApp.dialogService', 'platformWebApp.bladeUtils', 'platformWebApp.uiGridHelper', 'platformWebApp.ui-grid.extension',
        'VirtoCommerce.Loyalty.loyaltyMissions',
        function ($scope, dialogService, bladeUtils, uiGridHelper, gridOptionExtension, loyaltyMissions) {
            var blade = $scope.blade;
            var bladeNavigationService = bladeUtils.bladeNavigationService;

            var filter = blade.filter = $scope.filter = {};

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

                loyaltyMissions.search(getSearchCriteria(), function (data) {
                    blade.isLoading = false;
                    $scope.pageSettings.totalItems = data.totalCount;
                    $scope.listEntries = data.results;
                });
            };

            $scope.selectNode = function (node) {
                $scope.selectedNodeId = node.id;
                openDetailsBlade({
                    currentEntity: node,
                    currentEntityId: node.id,
                    title: node.name
                });
            };

            function openDetailsBlade(bladeData) {
                var newBlade = {
                    id: 'loyaltyMissionBlade',
                    controller: 'VirtoCommerce.Loyalty.loyaltyMissionDetailsController',
                    template: 'Modules/$(VirtoCommerce.Loyalty)/Scripts/blades/loyalty-mission-details.html'
                };
                angular.extend(newBlade, bladeData);
                bladeNavigationService.showBlade(newBlade, blade);
            }

            $scope.deleteList = function (list) {
                var dialog = {
                    id: "confirmDeleteItem",
                    title: "Loyalty.dialogs.loyalty-mission-delete.title",
                    message: "Loyalty.dialogs.loyalty-mission-delete.message",
                    callback: function (remove) {
                        if (remove) {
                            bladeNavigationService.closeChildrenBlades(blade, function () {
                                blade.isLoading = true;
                                var itemIds = _.pluck(list, 'id');
                                loyaltyMissions.remove({ ids: itemIds }, function () {
                                    blade.refresh();
                                });
                            });
                        }
                    }
                };
                dialogService.showConfirmationDialog(dialog);
            };

            blade.headIcon = 'fa fa-flag-checkered';

            blade.toolbarCommands = [
                {
                    name: "platform.commands.refresh",
                    icon: 'fa fa-refresh',
                    executeMethod: blade.refresh,
                    canExecuteMethod: function () { return true; }
                },
                {
                    name: "platform.commands.add", icon: 'fas fa-plus',
                    executeMethod: function () {
                        bladeNavigationService.closeChildrenBlades(blade, function () {
                            openDetailsBlade({ isNew: true, title: 'Loyalty.blades.loyalty-mission-details.new-mission' });
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
