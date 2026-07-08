angular.module('VirtoCommerce.Loyalty')
    .controller('VirtoCommerce.Loyalty.loyaltyProgramOperationLogListController', [
        '$scope', 'platformWebApp.bladeUtils', 'platformWebApp.uiGridHelper', 'platformWebApp.ui-grid.extension',
        'VirtoCommerce.Loyalty.loyaltyProgramOperationLogs', 'virtoCommerce.orderModule.knownOperations',
        function ($scope, bladeUtils, uiGridHelper, gridOptionExtension, loyaltyProgramOperationLogs, knownOperations) {
            var blade = $scope.blade;
            var bladeNavigationService = bladeUtils.bladeNavigationService;
            blade.headIcon = 'fa fa-star';

            blade.refresh = function () {
                blade.isLoading = true;

                var criteria = {
                    sort: uiGridHelper.getSortExpression($scope),
                    skip: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
                    take: $scope.pageSettings.itemsPerPageCount
                };

                if (blade.searchCriteria) {
                    angular.extend(criteria, blade.searchCriteria);
                }

                loyaltyProgramOperationLogs.search(criteria, function (data) {
                    blade.isLoading = false;

                    $scope.pageSettings.totalItems = data.totalCount;
                    $scope.listEntries = data.results;
                });
            };

            $scope.selectNode = function (node) {
                $scope.selectedNodeId = node.id;

                if (node.sourceType === 'LoyaltyMission') {
                    var newBlade = {
                        id: 'loyaltyMissionBlade',
                        currentEntity: {
                            id: node.sourceId
                        },
                        currentEntityId: node.sourceId,
                        controller: 'VirtoCommerce.Loyalty.loyaltyMissionDetailsController',
                        template: 'Modules/$(VirtoCommerce.Loyalty)/Scripts/blades/loyalty-mission-details.html'
                    };

                    bladeNavigationService.showBlade(newBlade, blade);
                }
                else if (node.sourceType === 'LoyaltyProgram') {
                    var newBlade = {
                        id: 'loyaltyProgramBlade',
                        currentEntity: {
                            id: node.sourceId
                        },
                        currentEntityId: node.sourceId,
                        controller: 'VirtoCommerce.Loyalty.loyaltyProgramDetailsController',
                        template: 'Modules/$(VirtoCommerce.Loyalty)/Scripts/blades/loyalty-program-details.html'
                    };

                    bladeNavigationService.showBlade(newBlade, blade);
                }
                else if (node.objectType === 'CustomerOrder') {
                    var foundTemplate = knownOperations.getOperation(node.objectType);
                    if (foundTemplate) {
                        var newBlade = angular.copy(foundTemplate.detailBlade);
                        //if (blade.preloadedOrders || angular.isFunction(blade.refreshCallback)) {
                        //    newBlade.id = 'preloadedOrderDetails';
                        //}
                        newBlade.customerOrder = { id: node.objectId };
                        bladeNavigationService.showBlade(newBlade, blade);
                    }
                }
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

            $scope.openDetails = function (node) {
                $scope.selectNode(node);
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
