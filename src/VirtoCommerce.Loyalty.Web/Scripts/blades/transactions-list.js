angular.module('VirtoCommerce.Loyalty')
    .controller(
        'VirtoCommerce.Loyalty.transactionListController',
        [
            '$scope',
            'VirtoCommerce.Loyalty.webApi',
            'platformWebApp.authService', 'platformWebApp.bladeNavigationService', 'platformWebApp.uiGridHelper',
            'platformWebApp.bladeUtils', 'platformWebApp.dialogService',
            function (
                $scope,
                loyaltyApi,
                authService, bladeNavigationService, uiGridHelper,
                bladeUtils, dialogService) {
                const blade = $scope.blade;

                //blade functions
                blade.refresh = function () {
                    loyaltyApi.trasactionsSearch(getSearchCriteria(), function (searchResult) {
                        blade.data = searchResult.results;
                        $scope.pageSettings.totalItems = searchResult.totalCount;
                        blade.isLoading = false;
                    });
                };

                //local functions
                function getSearchCriteria() {
                    return {
                        customerId: blade.customerId,
                        searchPhrase: filter.keyword ? filter.keyword : undefined,
                        sort: uiGridHelper.getSortExpression($scope),
                        skip: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
                        take: $scope.pageSettings.itemsPerPageCount
                    };
                }

                // ui-grid
                $scope.setGridOptions = function (gridOptions) {
                    uiGridHelper.initialize($scope, gridOptions, function (gridApi) {
                        $scope.gridApi = gridApi;
                        uiGridHelper.bindRefreshOnSortChanged($scope);
                    });
                    bladeUtils.initializePagination($scope);
                };

                //other functions
                const filter = $scope.filter = blade.filter || {};
                filter.criteriaChanged = function () {
                    if ($scope.pageSettings.currentPage > 1) {
                        $scope.pageSettings.currentPage = 1;
                    }
                    blade.refresh();
                };
            }
    ]);
