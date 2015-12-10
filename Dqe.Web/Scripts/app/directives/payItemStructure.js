dqeDirectives.directive('payItemStructure', function () {
    return {
        restrict: 'E',
        scope: {
            structure: '=',
            cancelCallback: '=',
            insertStructureCallback: '=',
            removeStructureCallback: '=',
            unlinkedItems: '=',
            loadItems: '=',
            primaryUnits: '=',
            hybridUnits: '=',
            itemTypes: '=',
            costBasedTemplates: '=',
            itemClassifications: '=',
            contractClassifications: '=',
            fuelTypes: '=',
            precisionCodes: '=',
            showCurrent: '='
        },
        templateUrl: './Views/directives/payItemStructure.html',
        controller: ['$scope', '$http', function ($scope, $http) {
            $scope.saveStructure = function (structure) {
                structure.safeName = '~' + structure.name;
                var spliceIn = structure.id == 0;
                $http.post('./PayItemStructureAdministration/SaveStructure', structure).success(function (result) {
                    if (!containsDqeError(result)) {
                        structure = getDqeData(result);
                        if ($scope.cancelCallback != undefined) {
                            $scope.cancelCallback();
                        }
                        if (spliceIn && $scope.insertStructureCallback != undefined) {
                            $scope.insertStructureCallback(structure);
                        }
                    }
                });
            }
            $scope.cancel = function() {
                $scope.cancelCallback();
            }
            $scope.toggleItemDetail = function (structure, item) {
                if (item.showDetail) {
                    item.showDetail = false;
                    return;
                } 
                $http.get('./PayItemStructureAdministration/GetProtectedItemDetail', { params: { itemId: item.id } }).success(function (result) {
                    if (!containsDqeError(result)) {
                        item = getDqeData(result);
                        item.showDetail = true;
                        for (var i = 0; i < structure.items.length; i++) {
                            if (structure.items[i].id == item.id) {
                                structure.items[i] = item;
                                break;
                            }
                        }
                    }
                });
            }
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
            $scope.setStructureMonitor = function (structure) {
                structure.srsId = structure.monitor.id;
            }
            $scope.getUnlinkedItems = function(val) {
                return $http.get('./PayItemStructureAdministration/GetUnlinkedItems', { params: { val: val } })
                    .then(function (response) {
                        var items = [];
                        angular.forEach(response.data.data, function (item) {
                            items.push(item);
                        });
                        return items;
                    });
            }
            
            $scope.removeLink = function(link, arr) {
                var index = arr.indexOf(link);
                if (index > -1) {
                    arr.splice(index, 1);
                }
            }
            $scope.addLink = function (link, arr) {
                arr.push(link);
            }
            $scope.getLinks = function (val, linkType, arr) {
                return $http.get('./weblinkadministration/SearchWebLinks', { params: { linkType: linkType, val: val } }).then(function (result) {
                    var data = getDqeData(result);
                    var links = [];
                    var addItem = true;
                    angular.forEach(data.data, function (item) {
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
            $scope.boeRecentChangeDateOpen = function ($event, structure) {
                $event.preventDefault();
                $event.stopPropagation();
                structure.boeRecentChangeDateOpened = true;
            };
            $scope.removeStructure = function(structure) {
                $http.post('./PayItemStructureAdministration/RemoveStructure', structure).success(function (result) {
                    if (!containsDqeError(result)) {
                        if ($scope.removeStructureCallback != undefined) {
                            $scope.removeStructureCallback(structure);
                        }
                    }
                });
            }
            $scope.addUnlinkedItem = function(item, structure) {
                structure.itemToAdd = item.id;
                $http.post('./PayItemStructureAdministration/AddItemToStructure', structure).success(function (result) {
                    if (!containsDqeError(result)) {
                        $scope.loadItems(structure);
                        structure.itemToAdd = null;
                    }
                });
            }
            $scope.removeItem = function(structure, item) {
                $http.post('./PayItemStructureAdministration/RemoveItemToStructure', item).success(function (result) {
                    if (!containsDqeError(result)) {
                        //$scope.unlinkedItems.push(item);
                        $scope.loadItems(structure);
                    }
                });
            }
            $scope.newItem = {
                specBook: '',
                isObsolete: false,
                unit: '',
                administrative: false,
                alternateItemName: '',
                asphaltFactor: 0,
                autoPaidPercentSchedule: false,
                bidAsLumpSum: false,
                bidRequirementCode: '',
                calculatedUnit: '',
                coApprovalRequired: false,
                combineWithLikeItems: true,
                commonUnit: '',
                concreteFactor: 0,
                contractClass: '',
                conversionFactorToCommonUnit: '',
                description: '',
                effectiveDate: '',
                exemptFromMaa: false,
                isFrontLoadedItem: false,
                exemptFromRetainage: false,
                factorNotes: '',
                fuelAdjustment: false,
                fuelAdjustmentType: '',
                id: 0,
                ildt2: '',
                ilflg1: '',
                illst1: '',
                ilnum1: '',
                ilsst1: '',
                isFederalFunded: false,
                isFixedPrice: false,
                itemClass: '',
                itemType: '',
                itmqtyprecsn: '',
                lumpSum: false,
                majorItem: false,
                nonBid: false,
                obsoleteDate: '',
                validDate: '',
                payPlan: false,
                percentScheduleItem: false,
                recordSource: '',
                name: '',
                refPrice: 0,
                regressionInclusion: false,
                shortDescription: '',
                specialtyItem: false,
                stateReferencePrice: 0,
                suppDescriptionRequired: false,
                itemUnit: '',
                unitSystem: '',
                primaryUnit: '',
                hybridUnit: '',
                isNonPart: false,
                costBasedTemplate: 0,
                specType: ''
            };
            $scope.addNewPayItem = false;
            $scope.addItem = function () {
                $scope.newItem = {
                    specBook: $scope.specBook,
                    isObsolete: false,
                    unit: '',
                    administrative: false,
                    alternateItemName: '',
                    asphaltFactor: 0,
                    autoPaidPercentSchedule: false,
                    bidAsLumpSum: false,
                    bidRequirementCode: '',
                    calculatedUnit: '',
                    coApprovalRequired: false,
                    combineWithLikeItems: true,
                    commonUnit: '',
                    concreteFactor: 0,
                    contractClass: '',
                    conversionFactorToCommonUnit: '',
                    description: '',
                    effectiveDate: '',
                    exemptFromMaa: false,
                    isFrontLoadedItem: false,
                    exemptFromRetainage: false,
                    factorNotes: '',
                    fuelAdjustment: false,
                    fuelAdjustmentType: '',
                    id: 0,
                    ildt2: '',
                    ilflg1: '',
                    illst1: '',
                    ilnum1: '',
                    ilsst1: '',
                    isFederalFunded: false,
                    isFixedPrice: false,
                    itemClass: '',
                    itemType: '',
                    itmqtyprecsn: '',
                    lumpSum: false,
                    majorItem: false,
                    nonBid: false,
                    obsoleteDate: '',
                    validDate: '',
                    payPlan: false,
                    percentScheduleItem: false,
                    recordSource: '',
                    name: '',
                    refPrice: 0,
                    regressionInclusion: false,
                    shortDescription: '',
                    specialtyItem: false,
                    stateReferencePrice: 0,
                    suppDescriptionRequired: false,
                    itemUnit: '',
                    unitSystem: '',
                    primaryUnit: '',
                    hybridUnit: '',
                    isNonPart: false,
                    costBasedTemplate: 0,
                    specType: '',
                    lrePickLists: null
                };
                $scope.addNewPayItem = true;
            }
            $scope.cancelAddItem = function() {
                $scope.addNewPayItem = false;
            }
            $scope.copyItemDetail = function (structure, item) {
                $http.get('./PayItemStructureAdministration/GetProtectedItemDetail', { params: { itemId: item.id } }).success(function (result) {
                    if (!containsDqeError(result)) {
                        item = getDqeData(result);
                        for (var i = 0; i < structure.items.length; i++) {
                            if (structure.items[i].id == item.id) {
                                structure.items[i] = item;
                                $scope.newItem = {
                                    isCopy: true,
                                    specBook: item.specBook,
                                    isObsolete: item.isObsolete,
                                    unit: item.unit,
                                    administrative: item.administrative,
                                    alternateItemName: item.alternateItemName,
                                    asphaltFactor: item.asphaltFactor,
                                    autoPaidPercentSchedule: item.autoPaidPercentSchedule,
                                    bidAsLumpSum: item.bidAsLumpSum,
                                    bidRequirementCode: item.bidRequirementCode,
                                    calculatedUnit: item.calculatedUnit,
                                    coApprovalRequired: item.coApprovalRequired,
                                    combineWithLikeItems: item.combineWithLikeItems,
                                    commonUnit: item.commonUnit,
                                    concreteFactor: item.concreteFactor,
                                    contractClass: item.contractClass,
                                    conversionFactorToCommonUnit: item.conversionFactorToCommonUnit,
                                    description: item.description,
                                    effectiveDate: item.effectiveDate,
                                    exemptFromMaa: item.exemptFromMaa,
                                    isFrontLoadedItem: item.isFrontLoadedItem,
                                    exemptFromRetainage: item.exemptFromRetainage,
                                    factorNotes: item.factorNotes,
                                    fuelAdjustment: item.fuelAdjustment,
                                    fuelAdjustmentType: item.fuelAdjustmentType,
                                    id: 0,
                                    ildt2: item.ildt2,
                                    ilflg1: item.ilflg1,
                                    illst1: item.illst1,
                                    ilnum1: item.ilnum1,
                                    ilsst1: item.ilsst1,
                                    isFederalFunded: item.isFederalFunded,
                                    isFixedPrice: item.isFixedPrice,
                                    itemClass: item.itemClass,
                                    itemType: item.itemType,
                                    itmqtyprecsn: item.itmqtyprecsn,
                                    lumpSum: item.lumpSum,
                                    majorItem: item.majorItem,
                                    nonBid: item.nonBid,
                                    obsoleteDate: item.obsoleteDate,
                                    validDate: item.validDate,
                                    payPlan: item.payPlan,
                                    percentScheduleItem: item.percentScheduleItem,
                                    recordSource: item.recordSource,
                                    name: '',
                                    refPrice: item.refPrice,
                                    regressionInclusion: item.regressionInclusion,
                                    shortDescription: item.shortDescription,
                                    specialtyItem: item.specialtyItem,
                                    stateReferencePrice: item.stateReferencePrice,
                                    suppDescriptionRequired: item.suppDescriptionRequired,
                                    itemUnit: item.itemUnit,
                                    unitSystem: item.unitSystem,
                                    primaryUnit: item.primaryUnit,
                                    hybridUnit: item.hybridUnit,
                                    isNonPart: item.isNonPart,
                                    costBasedTemplate: item.costBasedTemplate,
                                    specType: item.specType,
                                    lrePickLists: item.lrePickLists
                                };
                                $scope.addNewPayItem = true;
                                break;
                            }
                        }
                    }
                });
            }

                $http.get('./PayItemStructureAdministration/GetCurrentSpecBook').success(function(res) {
                    if (!containsDqeError(res)) {
                        $scope.specBook = getDqeData(res).specBook;
                    }
                });
            }
   ] }
});