angular.module('VirtoCommerce.Loyalty')
    .controller('VirtoCommerce.Loyalty.loyaltyMissionBannerController',
        ['$scope', 'FileUploader', 'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService',
            function ($scope, FileUploader, bladeNavigationService, dialogService) {
                const blade = $scope.blade;
                blade.title = 'Loyalty.blades.loyalty-mission-banner.title';
                blade.updatePermission = 'loyalty:update';

                if (!$scope.bannerUploader) {
                    const bannerUploader = $scope.bannerUploader = new FileUploader({
                        scope: $scope,
                        headers: { Accept: 'application/json' },
                        autoUpload: true,
                        removeAfterUpload: true,
                        filters: [{
                            name: 'imageFilter',
                            fn: function (item) {
                                const approval = /\.(png|gif|jpg|jpeg|svg|webp)$/i.test(item.name);
                                if (!approval) {
                                    dialogService.showErrorDialog({
                                        title: 'Loyalty.dialogs.loyalty-mission-banner-upload-filter.title',
                                        message: 'Loyalty.dialogs.loyalty-mission-banner-upload-filter.message',
                                    });
                                }
                                return approval;
                            }
                        }]
                    });

                    bannerUploader.url = 'api/assets?folderUrl=loyalty-missions';

                    bannerUploader.onAfterAddingFile = function (item) {
                        const fileExtension = '.' + item.file.name.split('.').pop();
                        const entityId = blade.currentEntity.id;
                        item.file.name = `banner_${entityId}_${Date.now().toString()}${fileExtension}`;
                    };

                    bannerUploader.onSuccessItem = function (_, uploadedImages) {
                        blade.currentEntity.bannerUrl = uploadedImages[0].url;
                    };

                    bannerUploader.onErrorItem = function (element, response, status) {
                        bladeNavigationService.setError(`${element._file.name} failed: ${response.message ? response.message : status}`, blade);
                    };
                }

                let formScope;
                $scope.setForm = function (form) {
                    formScope = form;
                };

                $scope.browseFiles = function (id) {
                    window.document.querySelector(`#${id}`).click();
                };

                function isDirty() {
                    return !angular.equals(blade.currentEntity, blade.origEntity) && blade.hasUpdatePermission();
                }

                function canSave() {
                    return isDirty() && formScope && formScope.$valid;
                }

                blade.saveChanges = function () {
                    blade.parentBlade.currentEntity.bannerUrl = blade.currentEntity.bannerUrl;

                    angular.copy(blade.currentEntity, blade.origEntity);

                    $scope.bladeClose();
                };

                blade.refresh = function () {
                    blade.origEntity = blade.currentEntity;
                    blade.currentEntity = angular.copy(blade.currentEntity);

                    blade.isLoading = false;
                };

                blade.toolbarCommands = [
                    {
                        name: "platform.commands.save",
                        icon: 'fas fa-save',
                        executeMethod: blade.saveChanges,
                        canExecuteMethod: canSave
                    },
                    {
                        name: "platform.commands.reset",
                        icon: 'fa fa-undo',
                        executeMethod: function () {
                            blade.currentEntity = angular.copy(blade.origEntity);
                        },
                        canExecuteMethod: isDirty
                    },
                    {
                        name: "platform.commands.clear",
                        icon: 'fa fa-eraser',
                        executeMethod: function () {
                            blade.currentEntity.bannerUrl = null;
                        },
                        canExecuteMethod: function () {
                            return blade.currentEntity.bannerUrl;
                        }
                    }
                ];

                blade.refresh();
            }]);
