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
        controller: function ($scope, $http) {
            $scope.saveStructure = function (structure) {
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
                combineWithLikeItems: false,
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
            };
            $scope.addNewPayItem = false;
            $scope.addItem = function () {
                //$scope.newItem.isObsolete = false;
                //$scope.newItem.unit = '';
                //$scope.newItem.administrative = false;
                //$scope.newItem.alternateItemName = '';
                //$scope.newItem.asphaltFactor = 0;
                //$scope.newItem.autoPaidPercentSchedule = false;
                //$scope.newItem.bidAsLumpSum = false;
                //$scope.newItem.bidRequirementCode = '';
                //$scope.newItem.calculatedUnit = '';
                //$scope.newItem.coApprovalRequired = false;
                //$scope.newItem.combineWithLikeItems = false;
                //$scope.newItem.commonUnit = '';
                //$scope.newItem.concreteFactor = 0;
                //$scope.newItem.contractClass = '';
                //$scope.newItem.conversionFactorToCommonUnit = '';
                //$scope.newItem.description = '';
                //$scope.newItem.effectiveDate = '';
                //$scope.newItem.exemptFromMaa = false;
                //$scope.newItem.exemptFromRetainage = false;
                //$scope.newItem.factorNotes = '';
                //$scope.newItem.fuelAdjustment = false;
                //$scope.newItem.fuelAdjustmentType = '';
                //$scope.newItem.id = 0;
                //$scope.newItem.ildt2 = '';
                //$scope.newItem.ilflg1 = '';
                //$scope.newItem.illst1 = '';
                //$scope.newItem.ilnum1 = '';
                //$scope.newItem.ilsst1 = '';
                //$scope.newItem.isFederalFunded = false;
                //$scope.newItem.isFixedPrice = false;
                //$scope.newItem.itemClass = '';
                //$scope.newItem.itemType = '';
                //$scope.newItem.itmqtyprecsn = '';
                //$scope.newItem.lumpSum = false;
                //$scope.newItem.majorItem = false;
                //$scope.newItem.nonBid = false;
                //$scope.newItem.obsoleteDate = '';
                //$scope.newItem.validDate = '';
                //$scope.newItem.payPlan = false;
                //$scope.newItem.percentScheduleItem = false;
                //$scope.newItem.recordSource = '';
                //$scope.newItem.name = '';
                //$scope.newItem.refPrice = 0;
                //$scope.newItem.regressionInclusion = false;
                //$scope.newItem.shortDescription = '';
                //$scope.newItem.specialtyItem = false;
                //$scope.newItem.stateReferencePrice = 0;
                //$scope.newItem.suppDescriptionRequired = false;
                //$scope.newItem.itemUnit = '';
                //$scope.newItem.unitSystem = '';
                //$scope.newItem.primaryUnit = '';
                //$scope.newItem.hybridUnit = '';
                //$scope.newItem.isNonPart = false;
                $scope.addNewPayItem = true;
            }
            $scope.cancelAddItem = function() {
                $scope.addNewPayItem = false;
            }
        }
    }
});