dqeControllers.controller('AdminPayItemsMaintainController', ['$scope', '$rootScope', '$http', function ($scope, $rootScope, $http) {
    $rootScope.$broadcast('initializeNavigation');
    function addLink(newLink, arr) {
        if (newLink == undefined || newLink == '') return;
        arr.push(newLink);
    }
    function removeLink(link, arr) {
        var index = arr.indexOf(link);
        if (index > -1) {
            arr.splice(index, 1);
        }
    }
    function loadPayItems(viewAll, range) {
        $http.get('./payitemstructureadministration/GetPayItemStructures', { params: { viewAll: viewAll, range: range } }).success(function (result) {
            if (!containsDqeError(result)) {
                $scope.payItemStructures = getDqeData(result);
            }
        });
    }
    function getPayItemPrefix() {
        var piArr = $scope.payItemDetail.structureNumber.split('');
        var payItemPrefix = '';
        for (var i = 0; i < piArr.length ; i++) {
            if (piArr[i] == '0'
                    || piArr[i] == '1'
                    || piArr[i] == '2'
                    || piArr[i] == '3'
                    || piArr[i] == '4'
                    || piArr[i] == '5'
                    || piArr[i] == '6'
                    || piArr[i] == '7'
                    || piArr[i] == '8'
                    || piArr[i] == '9'
                    || piArr[i] == '-'
                    || piArr[i] == ' '
            ) {
                payItemPrefix += piArr[i];
            } else {
                break;
            }
        }
        return payItemPrefix;
    }
    function resetPayItemDetail() {
        $scope.showPayItemDetail = true;
        $scope.payItemDetail = {
            id: 0,
            payItemId: '',
            shortDescription: '',
            primaryUnit: 0,
            secondaryUnit: 0,
            primaryUnitCode: '',
            secondaryUnitCode: '',
            masterFileId: 0,
            structureId: '',
            structureNumber: '',
            isMixedUnit: '',
            description: '',
            isSupplementalDescriptionRequired: 'False',
            isLikeItemsCombined: 'False',
            isFederalFunded: 'False',
            lreReferencePrice: '',
            concreteFactor: '',
            dqeReferencePrice: '',
            asphaltFactor: '',
            effectiveDate: '',
            obsoleteDate: '',
            factorNotes: '',
            masterFile: null,
            srsId: 0,
            costBasedTemplateId: 0
        };
    }
    function initialize() {
        $http.get('./CostBasedTemplateAdministration/GetAll').success(function (result) {
            $scope.costBasedTemplates = [];
            angular.forEach(result, function (item) {
                $scope.costBasedTemplates.push(item);
            });
        });
        $http.get('./masterfileadministration/GetMasterFiles').success(function (result) {
            if (!containsDqeError(result)) {
                var data = getDqeData(result);
                if (data.length > 0) {
                    $scope.masterFiles = data;
                }
            }
        });
        $scope.$watch('viewItems', function (newValue, oldValue) {
            if (newValue != oldValue) {
                loadPayItems(newValue != 'current', $scope.structureRange);
            }
        });
        $scope.$watch('structureRange', function (newValue, oldValue) {
            if (newValue != oldValue) {
                loadPayItems($scope.viewItems != 'current', newValue);
            }
        });
        $scope.viewItems = 'current';
        $scope.structureRange = 0;
        loadPayItems(false, $scope.structureRange);
        $scope.getUsers = function (val) {
            return $http.get('./staff/GetStaffByName', { params: { id: val } })
                .then(function (response) {
                    var users = [];
                    angular.forEach(response.data, function (item) {
                        users.push(item);
                    });
                    return users;
                });
        }
        $scope.payItemNumberIsValid = function () {
            return $scope.payItemDetail.payItemId.startsWith(getPayItemPrefix());
        }
        $scope.setStructureMonitor = function () {
            $scope.payItemStructureDetail.srsId = $scope.payItemStructureDetail.monitor.id;
        }
        $scope.setItemMonitor = function () {
            $scope.payItemDetail.srsId = $scope.payItemDetail.monitor.id;
        }
        $scope.addNewPayItemStructure = function () {
            $scope.showStructureDetail = true;
            $scope.payItemStructureDetail = {
                id: 0,
                structureId: '',
                title: '',
                effectiveDate: '',
                obsoleteDate: '',
                primaryUnit: 0,
                secondaryUnit: 0,
                accuracy: 0,
                isPlanQuantity: 'False',
                isDoNotBid: 'False',
                isFixedPrice: 'False',
                fixedAmount: 0,
                notes: '',
                details: '',
                pendingInformation: '',
                essHistory: '',
                boeRecentChangeDate: '',
                boeRecentChangeDescription: '',
                structureDescription: '',
                otherReferences: [],
                prepAndDocChapters: [],
                specifications: [],
                ppmChapters: [],
                standards: [],
                srsId: 0
            }
        }
        $scope.loadPayItemStructure = function (payItemStructure) {
            $scope.addNewPayItemStructure();
            $http.get('./payitemstructureadministration/GetPayItemStructure', { params: { id: payItemStructure.id } }).success(function (result) {
                if (!containsDqeError(result)) {
                    $scope.payItemStructureDetail = getDqeData(result);
                    if ($scope.payItemStructureDetail.costBasedTemplateId != 0) {
                        for (var i = 0; i < $scope.costBasedTemplates.length; i++) {
                            if ($scope.payItemStructureDetail.costBasedTemplateId == $scope.costBasedTemplates[i].id) {
                                $scope.payItemStructureDetail.costBasedTemplate = $scope.costBasedTemplates[i];
                                break;
                            }
                        }
                    }
                }
            });
        }
        $scope.savePayItemStructure = function () {
            var push = $scope.payItemStructureDetail.id == 0;
            if ($scope.payItemStructureDetail.costBasedTemplate != null && $scope.payItemStructureDetail.costBasedTemplate != 'undefined') {
                $scope.payItemStructureDetail.costBasedTemplateId = $scope.payItemStructureDetail.costBasedTemplate.id;
            } else {
                $scope.payItemStructureDetail.costBasedTemplateId = 0;
            }
            $http.post('./payitemstructureadministration/UpdatePayItemStructure', $scope.payItemStructureDetail).success(function (result) {
                if (!containsDqeError(result)) {
                    $scope.showStructureDetail = false;
                    var o = getDqeData(result);
                    if (push) {
                        $scope.payItemStructures.push(o);
                    } else {
                        for (var i = 0; i < $scope.payItemStructures.length; i++) {
                            if ($scope.payItemStructures[i].id == o.id) {
                                o.showItems = $scope.payItemStructures[i].showItems;
                                $scope.payItemStructures[i] = o;
                                break;
                            }
                        }
                    }
                }
            });
        }
        $scope.addNewPayItem = function (payItemStructure) {
            $scope.showPayItemDetail = true;
            $scope.payItemDetail = {
                id: 0,
                payItemId: '',
                shortDescription: '',
                primaryUnit: payItemStructure.isMixedUnit ? 0 : payItemStructure.primaryUnit,
                secondaryUnit: payItemStructure.isMixedUnit ? 0 : payItemStructure.secondaryUnit,
                primaryUnitCode: payItemStructure.primaryUnitCode,
                secondaryUnitCode: payItemStructure.secondaryUnitCode,
                masterFileId: 0,
                structureId: payItemStructure.id,
                structureNumber: payItemStructure.structureId,
                isMixedUnit: payItemStructure.isMixedUnit,
                description: '',
                isSupplementalDescriptionRequired: 'False',
                isLikeItemsCombined: 'False',
                isFederalFunded: 'False',
                lreReferencePrice: '',
                concreteFactor: '',
                dqeReferencePrice: '',
                asphaltFactor: '',
                effectiveDate: '',
                obsoleteDate: '',
                factorNotes: '',
                masterFile: $scope.masterFiles.length > 0 ? $scope.masterFiles[0] : null,
                srsId: payItemStructure.srsId,
                costBasedTemplateId: payItemStructure.costBasedTemplateId
            };
            $scope.payItemDetail.payItemId = getPayItemPrefix();
            if (payItemStructure.srsId != 0) {
                $http.get('./payitemadministration/GetMonitor', { params: { srsId: $scope.payItemDetail.srsId } }).success(function (result) {
                    if (!containsDqeError(result)) {
                        $scope.payItemDetail.monitor = getDqeData(result);
                    }
                });
            }
            if ($scope.payItemDetail.costBasedTemplateId != 0) {
                for (var i = 0; i < $scope.costBasedTemplates.length; i++) {
                    if ($scope.payItemDetail.costBasedTemplateId == $scope.costBasedTemplates[i].id) {
                        $scope.payItemDetail.costBasedTemplate = $scope.costBasedTemplates[i];
                        break;
                    }
                }
            }
        }
        $scope.copyPayItem = function (payItem) {
            resetPayItemDetail();
            $http.get('./payitemadministration/GetPayItem', { params: { id: payItem.id } }).success(function (result) {
                if (!containsDqeError(result)) {
                    payItem = getDqeData(result);
                    payItem.id = 0;
                    payItem.masterFileId = 0;
                    $scope.payItemDetail = payItem;
                    if ($scope.payItemDetail.costBasedTemplateId != 0) {
                        for (var i = 0; i < $scope.costBasedTemplates.length; i++) {
                            if ($scope.payItemDetail.costBasedTemplateId == $scope.costBasedTemplates[i].id) {
                                $scope.payItemDetail.costBasedTemplate = $scope.costBasedTemplates[i];
                                break;
                            }
                        }
                    }
                }
            });
        }
        $scope.loadPayItem = function (payItem) {
            resetPayItemDetail();
            $http.get('./payitemadministration/GetPayItem', { params: { id: payItem.id } }).success(function (result) {
                if (!containsDqeError(result)) {
                    $scope.payItemDetail = getDqeData(result);
                    if ($scope.payItemDetail.costBasedTemplateId != 0) {
                        for (var i = 0; i < $scope.costBasedTemplates.length; i++) {
                            if ($scope.payItemDetail.costBasedTemplateId == $scope.costBasedTemplates[i].id) {
                                $scope.payItemDetail.costBasedTemplate = $scope.costBasedTemplates[i];
                                break;
                            }
                        }
                    }
                }
            });
        }
        $scope.savePayItem = function () {
            var payItemStructure = undefined;
            for (var i = 0; i < $scope.payItemStructures.length; i++) {
                if ($scope.payItemStructures[i].id == $scope.payItemDetail.structureId) {
                    payItemStructure = $scope.payItemStructures[i];
                    break;
                }
            }
            if (payItemStructure == undefined) {
                return;
            }
            if ($scope.payItemDetail.masterFile != undefined && $scope.payItemDetail.masterFile != null) {
                $scope.payItemDetail.masterFileId = $scope.payItemDetail.masterFile.id;
            }
            if ($scope.payItemDetail.costBasedTemplate != null && $scope.payItemDetail.costBasedTemplate != 'undefined') {
                $scope.payItemDetail.costBasedTemplateId = $scope.payItemDetail.costBasedTemplate.id;
            } else {
                $scope.payItemDetail.costBasedTemplateId = 0;
            }
            var push = $scope.payItemDetail.id == 0;
            $http.post('./payitemadministration/UpdatePayItem', $scope.payItemDetail).success(function (result) {
                if (!containsDqeError(result)) {
                    $scope.showPayItemDetail = false;
                    var o = getDqeData(result);
                    if (push) {
                        payItemStructure.items.push(o);
                    } else {
                        for (var ii = 0; ii < payItemStructure.items.length; ii++) {
                            if (payItemStructure.items[ii].id == o.id) {
                                payItemStructure.items[ii] = o;
                                break;
                            }
                        }
                    }
                }
            });
        }
        $scope.effectiveOpen = function ($event) {
            $event.preventDefault();
            $event.stopPropagation();
            $scope.effectiveOpened = true;
            $scope.obsoleteOpened = false;
            $scope.boeRecentChangeDateOpened = false;
        };
        $scope.obsoleteOpen = function ($event) {
            $event.preventDefault();
            $event.stopPropagation();
            $scope.obsoleteOpened = true;
            $scope.effectiveOpened = false;
            $scope.boeRecentChangeDateOpened = false;
        };
        $scope.boeRecentChangeDateOpen = function ($event) {
            $event.preventDefault();
            $event.stopPropagation();
            $scope.boeRecentChangeDateOpened = true;
            $scope.obsoleteOpened = false;
            $scope.effectiveOpened = false;
        };
        $scope.getLinks = function (val, linkType, arr) {
            return $http.get('./weblinkadministration/SearchWebLinks', { params: { linkType: linkType, val: val } }).then(function (result) {
                var data = getDqeData(result);
                var links = [];
                var addItem = true;
                angular.forEach(data, function (item) {
                    for (var i = 0; i < arr.length; i++) {
                        if (arr[i].id == item.id) {
                            addItem = false;
                            break;
                        }
                        addItem = true;
                    }
                    if (addItem) links.push(item);
                });
                return links;
            });
        }
        //otherReference
        $scope.addOtherReference = function () {
            addLink($scope.newOtherReference, $scope.payItemStructureDetail.otherReferences);
        };
        $scope.removeOtherReference = function (otherReference) {
            removeLink(otherReference, $scope.payItemStructureDetail.otherReferences);
        };
        //ppmChapter
        $scope.addPpmChapter = function () {
            addLink($scope.newPpmChapter, $scope.payItemStructureDetail.ppmChapters);
        };
        $scope.removePpmChapter = function (ppmChapter) {
            removeLink(ppmChapter, $scope.payItemStructureDetail.ppmChapters);
        };
        //prepAndDocChapter
        $scope.addPrepAndDocChapter = function () {
            addLink($scope.newPrepAndDocChapter, $scope.payItemStructureDetail.prepAndDocChapters);
        };
        $scope.removePrepAndDocChapter = function (prepAndDocChapter) {
            removeLink(prepAndDocChapter, $scope.payItemStructureDetail.prepAndDocChapters);
        };
        //standard
        $scope.addStandard = function () {
            addLink($scope.newStandard, $scope.payItemStructureDetail.standards);
        };
        $scope.removeStandard = function (standard) {
            removeLink(standard, $scope.payItemStructureDetail.standards);
        };
        //specification
        $scope.addSpecification = function () {
            addLink($scope.newSpecification, $scope.payItemStructureDetail.specifications);
        };
        $scope.removeSpecification = function (specification) {
            removeLink(specification, $scope.payItemStructureDetail.specifications);
        };
    }
}]);