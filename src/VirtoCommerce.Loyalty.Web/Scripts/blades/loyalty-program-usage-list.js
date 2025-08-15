angular.module('VirtoCommerce.Loyalty')
    .controller('VirtoCommerce.Loyalty.loyaltyProgramUsageListController', [
        '$scope', 'platformWebApp.bladeUtils', 'platformWebApp.uiGridHelper', 'platformWebApp.ui-grid.extension',
        'VirtoCommerce.Loyalty.loyaltyProgramUsages',
        function ($scope, bladeUtils, uiGridHelper, gridOptionExtension, loyaltyProgramUsages) {
            var blade = $scope.blade;
            var bladeNavigationService = bladeUtils.bladeNavigationService;
            blade.headIcon = 'fa fa-area-chart'; //todo: find better icon

            // simple and advanced filtering
            var filter = blade.filter = $scope.filter = {};

            blade.refresh = function () {
                blade.isLoading = true;

                var criteria = {
                    //keyword: filter.keyword,
                    sort: uiGridHelper.getSortExpression($scope),
                    skip: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
                    take: $scope.pageSettings.itemsPerPageCount
                };

                if (filter.current) {
                    angular.extend(criteria, filter.current);
                }

                loyaltyProgramUsages.search(criteria, function (data) {
                    blade.isLoading = false;

                    $scope.pageSettings.totalItems = data.totalCount;
                    $scope.listEntries = data.results;
                });
            };

            $scope.selectNode = function (node) {
                $scope.selectedNodeId = node.id;

                var newBlade = {
                    id: 'loyaltyProgramBlade',
                    currentEntity: {
                        id: node.loyaltyProgramId
                    },
                    currentEntityId: node.loyaltyProgramId,
                    controller: 'VirtoCommerce.Loyalty.loyaltyProgramDetailsController',
                    template: 'Modules/$(VirtoCommerce.Loyalty)/Scripts/blades/loyalty-program-details.html'
                };

                bladeNavigationService.showBlade(newBlade, blade);
            };

            blade.toolbarCommands = [
                {
                    name: "platform.commands.refresh",
                    icon: 'fa fa-refresh',
                    executeMethod: blade.refresh,
                    canExecuteMethod: function () {
                        return true;
                    }
                }
            ];

            $scope.openLoyaltyProgram = function (node) {
                $scope.selectNode(node);
            };

            $scope.getUsageType = function (usageType) {
                if (usageType === 'Awarded') {
                    return '1';
                }
                else if (usageType === 'Redeemed') {
                    return '2';
                }
            }

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
