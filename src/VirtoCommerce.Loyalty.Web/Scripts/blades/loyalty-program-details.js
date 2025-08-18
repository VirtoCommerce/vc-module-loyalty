angular.module('VirtoCommerce.Loyalty')
    .controller('VirtoCommerce.Loyalty.loyaltyProgramDetailsController',
        ['$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.settings', 'platformWebApp.metaFormsService',
            'VirtoCommerce.Loyalty.loyaltyPrograms', 'virtoCommerce.storeModule.stores', 'virtoCommerce.coreModule.common.dynamicExpressionService',
            function ($scope, bladeNavigationService, settings, metaFormsService, loyaltyPrograms, stores, dynamicExpressionService) {
                var blade = $scope.blade;
                blade.headIcon = 'fa fa-area-chart'; // find better icon  
                blade.updatePermission = 'loyalty:update';
                blade.metaFields = metaFormsService.getMetaFields("loyaltyProgramDetail");
                blade.expressionTreeTemplateUrl = dynamicExpressionService.expressionTreeTemplateUrl;
                var languagesPromise = settings.getValues({ id: 'VirtoCommerce.Core.General.Languages' }).$promise;
                blade.languages = [];

                blade.refresh = function (parentRefresh) {
                    if (blade.isNew) {
                        loyaltyPrograms.getNew(initializeBlade);
                    }
                    else {
                        loyaltyPrograms.get({ id: blade.currentEntityId }, initializeBlade);

                        if (parentRefresh && angular.isFunction(blade.parentBlade.refresh)) {
                            blade.parentBlade.refresh(true);
                        }
                    }
                };

                function initializeBlade(data) {
                    if (data.dynamicExpression) {
                        _.each(data.dynamicExpression.children, extendElementBlock);
                        groupAvailableChildren(data.dynamicExpression.children[0]);
                    }

                    blade.currentEntity = angular.copy(data);
                    blade.originalEntity = data;

                    if (blade.currentEntity.name) {
                        blade.title = blade.currentEntity.name;
                    }

                    languagesPromise.then(function (languagesData) {
                        blade.languages = languagesData;
                    });

                    blade.isLoading = false;
                }

                $scope.saveChanges = function () {
                    bladeNavigationService.setError(null, blade);
                    blade.isLoading = true;

                    if (blade.currentEntity.dynamicExpression) {
                        _.each(blade.currentEntity.dynamicExpression.children, stripOffUiInformation);
                    }

                    if (blade.isNew) {
                        loyaltyPrograms.save({}, blade.currentEntity, function (data) {
                            blade.isNew = false;
                            blade.currentEntity = data;
                            blade.currentEntityId = data.id;
                            initializeToolbar();
                            blade.refresh(true);
                        });
                    } else {
                        loyaltyPrograms.update({}, blade.currentEntity, function (data) {
                            blade.refresh(true);
                        });
                    }
                };

                blade.searchStores = function (criteria) {
                    return stores.search(criteria);
                }

                $scope.setForm = function (form) {
                    $scope.formScope = form;
                };

                function canSave() {
                    return isDirty() && $scope.formScope && $scope.formScope.$valid;
                }

                function isDirty() {
                    return !angular.equals(blade.currentEntity, blade.originalEntity) && blade.hasUpdatePermission();
                }

                blade.onClose = function (closeCallback) {
                    bladeNavigationService.showConfirmationIfNeeded(isDirty() && !blade.isNew,
                        canSave(),
                        blade,
                        $scope.saveChanges,
                        closeCallback,
                        "Loyalty.dialogs.loyalty-program-save.title", "Loyalty.dialogs.loyalty-program-save.message");
                };

                function groupAvailableChildren(expressionBlock) {
                    results = _.groupBy(expressionBlock.availableChildren, 'groupName');
                    expressionBlock.availableChildren = _.map(results, function (items, key) {
                        return {
                            displayName: key,
                            subitems: items
                        };
                    });
                }

                function initializeToolbar() {
                    blade.toolbarCommands = [{
                        name: "platform.commands.save",
                        icon: 'fas fa-save',
                        executeMethod: $scope.saveChanges,
                        canExecuteMethod: canSave,
                        permission: blade.updatePermission
                    }];

                    if (!blade.isNew) {
                        blade.toolbarCommands.push({
                            name: "platform.commands.reset",
                            icon: 'fa fa-undo',
                            executeMethod: function () {
                                angular.copy(blade.originalEntity, blade.currentEntity);
                            },
                            canExecuteMethod: isDirty,
                            permission: blade.updatePermission
                        });
                    }
                }

                // Dynamic ExpressionBlock
                function extendElementBlock(expressionBlock) {
                    var retVal = dynamicExpressionService.expressions[expressionBlock.id];
                    if (!retVal) {
                        retVal = { displayName: 'unknown element: ' + expressionBlock.id };
                    }

                    _.extend(expressionBlock, retVal);

                    if (!expressionBlock.children) {
                        expressionBlock.children = [];
                    }

                    _.each(expressionBlock.children, extendElementBlock);
                    _.each(expressionBlock.availableChildren, extendElementBlock);

                    return expressionBlock;
                }

                function stripOffUiInformation(expressionElement) {
                    expressionElement.availableChildren = undefined;
                    expressionElement.displayName = undefined;
                    expressionElement.getValidationError = undefined;
                    expressionElement.newChildLabel = undefined;
                    expressionElement.templateURL = undefined;

                    _.each(expressionElement.children, stripOffUiInformation);
                }

                initializeToolbar();
                blade.refresh(false);
            }]);
