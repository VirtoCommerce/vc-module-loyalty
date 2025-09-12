angular.module('VirtoCommerce.Loyalty')
    .controller('VirtoCommerce.Loyalty.filterDetailController',
        ['$scope', '$localStorage', '$translate', 'platformWebApp.metaFormsService',
        function ($scope, $localStorage, $translate, metaFormsService) {
            var blade = $scope.blade;

            blade.metaFields = blade.metaFields ? blade.metaFields : metaFormsService.getMetaFields('loyaltyProgramFilterDetail');

            $scope.applyCriteria = function () {
                angular.copy(blade.currentEntity, blade.originalEntity);

                if (blade.isNew) {
                    $localStorage.loyaltyProgramSearchFilters.push(blade.originalEntity);
                    $localStorage.loyaltyProgramFilterId = blade.originalEntity.id;
                    blade.parentBlade.filter.current = blade.originalEntity;
                    blade.isNew = false;
                }

                initializeBlade(blade.originalEntity);
                blade.parentBlade.filter.criteriaChanged();
            };

            $scope.saveChanges = function () {
                $scope.applyCriteria();
            };

            function initializeBlade(data) {
                blade.currentEntity = angular.copy(data);
                blade.originalEntity = data;
                blade.isLoading = false;

                blade.title = blade.isNew ? 'Loyalty.blades.filter-detail.title' : data.name;
            }

            var formScope;
            $scope.setForm = function (form) {
                formScope = form;
            };

            function isDirty() {
                return !angular.equals(blade.currentEntity, blade.originalEntity);
            }

            blade.headIcon = 'fa fa-filter';

            blade.toolbarCommands = [
                {
                    name: "core.commands.apply-filter", icon: 'fa fa-filter',
                    executeMethod: function () {
                        $scope.saveChanges();
                    },
                    canExecuteMethod: function () {
                        return formScope && formScope.$valid;
                    }
                },
                {
                    name: "platform.commands.reset", icon: 'fa fa-undo',
                    executeMethod: function () {
                        angular.copy(blade.originalEntity, blade.currentEntity);
                    },
                    canExecuteMethod: isDirty
                },
                {
                    name: "platform.commands.delete", icon: 'fas fa-trash-alt',
                    executeMethod: deleteEntry,
                    canExecuteMethod: function () {
                        return !blade.isNew;
                    }
                }];

            function deleteEntry() {
                blade.parentBlade.filter.current = null;
                $localStorage.loyaltyProgramSearchFilters.splice($localStorage.loyaltyProgramSearchFilters.indexOf(blade.originalEntity), 1);
                delete $localStorage.loyaltyProgramFilterId;
                blade.parentBlade.refresh();
                $scope.bladeClose();
            }

            // actions on load
            if (blade.isNew) {
                $translate('Loyalty.blades.filter-detail.labels.unnamed-filter').then(function (result) {
                    initializeBlade({ id: new Date().getTime(), name: result });
                });
            } else {
                initializeBlade(blade.data);
            }
        }]);
