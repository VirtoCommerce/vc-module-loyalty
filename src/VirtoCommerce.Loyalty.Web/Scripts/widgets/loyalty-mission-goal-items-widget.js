angular.module('VirtoCommerce.Loyalty')
    .controller('VirtoCommerce.Loyalty.loyaltyMissionGoalItemsWidgetController',
        ['$scope', 'platformWebApp.bladeNavigationService', 'VirtoCommerce.Loyalty.loyaltyMissionGoalItems',
            function ($scope, bladeNavigationService, loyaltyMissionGoalItems) {
                var blade = $scope.widget.blade;

                function refresh() {
                    if (!blade.currentEntityId) {
                        return;
                    }

                    loyaltyMissionGoalItems.search({
                        missionId: blade.currentEntityId,
                        take: 0
                    }, function (data) {
                        $scope.count = data.totalCount;
                    });
                }

                $scope.openBlade = function () {
                    var newBlade = {
                        id: 'loyaltyMissionGoalItemList',
                        title: 'Loyalty.blades.loyalty-mission-goal-item-list.title',
                        missionId: blade.currentEntityId,
                        storeId: blade.currentEntity.storeId,
                        controller: 'VirtoCommerce.Loyalty.loyaltyMissionGoalItemListController',
                        template: 'Modules/$(VirtoCommerce.Loyalty)/Scripts/blades/loyalty-mission-goal-item-list.html',
                        parentWidgetRefresh: refresh
                    };
                    bladeNavigationService.showBlade(newBlade, blade);
                };

                refresh();
            }]);
