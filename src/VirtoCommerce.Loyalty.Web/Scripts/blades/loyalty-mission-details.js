angular.module('VirtoCommerce.Loyalty')
    .controller('VirtoCommerce.Loyalty.loyaltyMissionDetailsController',
        ['$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.settings', 'platformWebApp.metaFormsService',
            'VirtoCommerce.Loyalty.loyaltyMissions', 'virtoCommerce.storeModule.stores', 'virtoCommerce.coreModule.common.dynamicExpressionService',
            'platformWebApp.dialogService',
            function ($scope, bladeNavigationService, settings, metaFormsService, loyaltyMissions, stores, dynamicExpressionService, dialogService) {
                var blade = $scope.blade;
                blade.headIcon = 'fa fa-flag-checkered';
                blade.updatePermission = 'loyalty:update';
                blade.metaFields = metaFormsService.getMetaFields("loyaltyMissionDetail");
                blade.expressionTreeTemplateUrl = dynamicExpressionService.expressionTreeTemplateUrl;
                blade.missionStatuses = ['Draft', 'Published', 'Archived'];
                var languagesPromise = settings.getValues({ id: 'VirtoCommerce.Core.General.Languages' }).$promise;
                blade.languages = [];

                blade.refresh = function (parentRefresh) {
                    if (blade.isNew) {
                        loyaltyMissions.getNew({}, initializeBlade);
                    }
                    else {
                        loyaltyMissions.get({ id: blade.currentEntityId }, initializeBlade);

                        if (parentRefresh && angular.isFunction(blade.parentBlade.refresh)) {
                            blade.parentBlade.refresh(true);
                        }
                    }
                };

                blade.showGoalItemsBlade = function () {
                    var newBlade = {
                        id: 'loyaltyMissionGoalItemList',
                        title: 'Loyalty.blades.loyalty-mission-goal-item-list.title',
                        missionId: blade.currentEntityId,
                        storeId: blade.currentEntity.storeId,
                        controller: 'VirtoCommerce.Loyalty.loyaltyMissionGoalItemListController',
                        template: 'Modules/$(VirtoCommerce.Loyalty)/Scripts/blades/loyalty-mission-goal-item-list.html'
                    };
                    bladeNavigationService.showBlade(newBlade, blade);
                }

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

                function getExpressionValidationError(element) {
                    if (!element) {
                        return undefined;
                    }
                    if (angular.isFunction(element.getValidationError)) {
                        var error = element.getValidationError();
                        if (error) {
                            return error;
                        }
                    }
                    for (var i = 0; i < (element.children || []).length; i++) {
                        var childError = getExpressionValidationError(element.children[i]);
                        if (childError) {
                            return childError;
                        }
                    }
                    return undefined;
                }

                $scope.saveChanges = function () {
                    bladeNavigationService.setError(null, blade);

                    if (blade.currentEntity.dynamicExpression) {
                        var validationError = getExpressionValidationError(blade.currentEntity.dynamicExpression);
                        if (validationError) {
                            bladeNavigationService.setError(validationError, blade);
                            return;
                        }
                    }

                    blade.isLoading = true;

                    if (blade.currentEntity.dynamicExpression) {
                        _.each(blade.currentEntity.dynamicExpression.children, stripOffUiInformation);
                    }

                    if (blade.isNew) {
                        loyaltyMissions.save({}, blade.currentEntity, function (data) {
                            blade.isNew = false;
                            blade.currentEntity = data;
                            blade.currentEntityId = data.id;
                            initializeToolbar();
                            blade.refresh(true);
                        });
                    } else {
                        loyaltyMissions.update({}, blade.currentEntity, function () {
                            blade.refresh(true);
                        }, function (error) {
                            blade.isLoading = false;
                            bladeNavigationService.setError('Error ' + error.status, blade);
                        });
                    }
                };

                blade.searchStores = function (criteria) {
                    return stores.search(criteria);
                }

                $scope.setForm = function (form) {
                    $scope.formScope = form;
                };

                function isDraft() {
                    return blade.isNew || (blade.currentEntity && blade.currentEntity.status === 'Draft');
                }

                function canSave() {
                    return isDirty() && isDraft() && $scope.formScope && $scope.formScope.$valid;
                }

                function isDirty() {
                    return !angular.equals(blade.currentEntity, blade.originalEntity) && blade.hasUpdatePermission();
                }

                blade.onClose = function (closeCallback) {
                    bladeNavigationService.showConfirmationIfNeeded(isDraft() && isDirty() && !blade.isNew,
                        canSave(),
                        blade,
                        $scope.saveChanges,
                        closeCallback,
                        "Loyalty.dialogs.loyalty-mission-save.title", "Loyalty.dialogs.loyalty-mission-save.message");
                };

                function groupAvailableChildren(expressionBlock) {
                    const results = _.groupBy(expressionBlock.availableChildren, 'groupName');
                    expressionBlock.availableChildren = _.map(results, function (items, key) {
                        return {
                            displayName: key,
                            subitems: items
                        };
                    });
                }

                function publish() {
                    var dialog = {
                        id: 'confirmPublishMission',
                        title: 'Loyalty.dialogs.loyalty-mission-publish.title',
                        message: 'Loyalty.dialogs.loyalty-mission-publish.message',
                        callback: function (confirmed) {
                            if (confirmed) {
                                blade.currentEntity.status = 'Published';
                                $scope.saveChanges();
                            }
                        }
                    };
                    dialogService.showConfirmationDialog(dialog);
                }

                function archive() {
                    var dialog = {
                        id: 'confirmArchiveMission',
                        title: 'Loyalty.dialogs.loyalty-mission-archive.title',
                        message: 'Loyalty.dialogs.loyalty-mission-archive.message',
                        callback: function (confirmed) {
                            if (confirmed) {
                                blade.currentEntity.status = 'Archived';
                                $scope.saveChanges();
                            }
                        }
                    };
                    dialogService.showConfirmationDialog(dialog);
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
                            canExecuteMethod: function () {
                                return isDraft() && isDirty();
                            },
                            permission: blade.updatePermission
                        });

                        blade.toolbarCommands.push({
                            name: "Loyalty.blades.loyalty-mission-details.commands.publish",
                            icon: 'fa fa-rocket',
                            executeMethod: publish,
                            canExecuteMethod: function () {
                                return blade.currentEntity && blade.currentEntity.status === 'Draft';
                            },
                            permission: blade.updatePermission
                        });

                        blade.toolbarCommands.push({
                            name: "Loyalty.blades.loyalty-mission-details.commands.archive",
                            icon: 'fa fa-archive',
                            executeMethod: archive,
                            canExecuteMethod: function () {
                                return blade.currentEntity && blade.currentEntity.status !== 'Archived';
                            },
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
