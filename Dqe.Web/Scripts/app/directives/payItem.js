dqeDirectives.directive('payItem', function () {
    return {
        restrict: 'E',
        scope: {
            item: '=',
            structure: '=',
            primaryUnits: '=',
            hybridUnits: '=',
            itemTypes: '=',
            costBasedTemplates: '=',
            itemClassifications: '=',
            contractClassifications: '=',
            fuelTypes: '=',
            precisionCodes: '=',
            cancel: '=',
            showCurrent: '='
        },
        templateUrl: './Views/directives/payItem.html',
        controller: ['$scope', '$http', function ($scope, $http) {
            if ($scope.item.id == 0) {
                $http.get('./PayItemStructureAdministration/GetSpecBooks').success(function (result) {
                    if (!containsDqeError(result)) {
                        $scope.specBooks = getDqeData(result);
                        $http.get('./PayItemStructureAdministration/GetCurrentSpecBook').success(function (res) {
                            if (!containsDqeError(res)) {
                                $scope.item.specBook = getDqeData(res).specBook;
                                $http.get('./PayItemStructureAdministration/GetLrePickLists').success(function (res2) {
                                    if (!containsDqeError(res2)) {
                                        $scope.lrePickLists = getDqeData(res2);
                                    }
                                });
                            }
                        });
                    }
                });
            }
            $scope.validDateOpen = function ($event, item) {
                $event.preventDefault();
                $event.stopPropagation();
                item.validDateOpened = true;
                item.effectiveDateOpened = false;
                item.obsoleteDateOpened = false;
            };
            $scope.effectiveDateOpen = function ($event, item) {
                $event.preventDefault();
                $event.stopPropagation();
                item.validDateOpened = false;
                item.effectiveDateOpened = true;
                item.obsoleteDateOpened = false;
            };
            $scope.obsoleteDateOpen = function ($event, item) {
                $event.preventDefault();
                $event.stopPropagation();
                item.validDateOpened = false;
                item.effectiveDateOpened = false;
                item.obsoleteDateOpened = true;
            };
            function checkNumeric(arr) {
                var val = '';
                var hasDecimal = false;
                for (var i = 0; i < arr.length; i++) {
                    if (isNumber(arr[i]) || (arr[i] == '.' && !hasDecimal)) {
                        if (arr[i] == '.') hasDecimal = true;
                        val = val + arr[i];
                    }
                }
                return val;
            }
            $scope.checkRefPriceNumeric = function(item) {
                var arr = item.refPrice.split('');
                item.refPrice = checkNumeric(arr);
            }
            $scope.checkConcreteFactorNumeric = function (item) {
                var arr = item.concreteFactor.split('');
                item.concreteFactor = checkNumeric(arr);
            }
            $scope.checkAsphaltFactorNumeric = function (item) {
                var arr = item.asphaltFactor.split('');
                item.asphaltFactor = checkNumeric(arr);
            }
            $scope.checkConversionFactorNumeric = function(item) {
                var arr = item.conversionFactorToCommonUnit.split('');
                item.conversionFactorToCommonUnit = checkNumeric(arr);
            }
            $scope.saveItem = function (item) {
                var name = item.name;
                item.name = '~' + item.name;
                if (item.id == 0) {
                    item.structureId = $scope.structure.id;
                    item.showCurrent = $scope.showCurrent;
                    item.lrePickLists = $scope.lrePickLists;
                }
                $http.post('./PayItemStructureAdministration/SaveItem', item).success(function (result) {
                    if (!containsDqeError(result)) {
                        if (item.primaryUnit == 'LS') {
                            item.unit = item.primaryUnit + '/' + item.hybridUnit;
                        } else {
                            item.unit = item.primaryUnit;
                        }
                        var unit = '';
                        if ($scope.structure.items != undefined) {
                            for (var i = 0; i < $scope.structure.items.length; i++) {
                                if ($scope.structure.items[i].isObsolete) continue;
                                if (unit == '') {
                                    unit = $scope.structure.items[i].unit;
                                } else if (unit != $scope.structure.items[i].unit) {
                                    unit = "MIXED";
                                    break;
                                }
                            }
                        }
                        $scope.structure.unit = unit;
                        if (item.id == 0) {
                            $scope.structure.items = getDqeData(result);
                            $scope.cancel();
                        }
                    }
                });
                item.name = name;
            }
            //begin bid history directive support
            $http.get('./marketarea/GetMarketAreas').success(function (res) {
                if (!containsDqeError(res)) {
                    $scope.marketAreas = getDqeData(res);
                    if ($scope.marketAreas != null && $scope.marketAreas != undefined && $scope.marketAreas.marketAreas != null && $scope.marketAreas.marketAreas != undefined) {
                        var marketAreas = $scope.marketAreas.marketAreas;
                        for (var i = 0; i < marketAreas.length; i++) {
                            marketAreas[i].include = true;
                            for (var ii = 0; ii < marketAreas[i].counties.length; ii++) {
                                marketAreas[i].counties[ii].include = true;
                            }
                        }
                    }
                }
            });
            $http.get('./estimate/GetWorkTypes').success(function (res) {
                if (!containsDqeError(res)) {
                    $scope.workTypes = getDqeData(res);
                    if ($scope.workTypes != null && $scope.workTypes != undefined) {
                        var workTypes = $scope.workTypes;
                        for (var i = 0; i < workTypes.length; i++) {
                            workTypes[i].include = true;
                        }
                    }
                }
            });
            $scope.contractType = 'all';
            $scope.closePricingParameters = function (itemGroup) {
                itemGroup.showPricingParameters = !itemGroup.showPricingParameters;
                itemGroup.history = null;
            }
            $scope.showPricingParameters = function (itemGroup) {
                if (!itemGroup.showPricingParameters) {
                    itemGroup.itemNumber = itemGroup.name;
                    var itemToPrice = {
                        itemGroup: itemGroup,
                        county: ''
                    }
                    
                    $http.post('./estimate/GetBidHistory', itemToPrice).success(function (result) {
                        if (!containsDqeError(result)) {
                            itemGroup.history = getDqeData(result);
                            itemGroup.history.contractType = 'all';
                            itemGroup.history.omitOutliers = true;
                            itemGroup.history.bidMonths = 36;
                            itemGroup.history.workTypes = [];
                            for (var i = 0; i < $scope.workTypes.length; i++) {
                                itemGroup.history.workTypes.push({
                                    name: $scope.workTypes[i].name,
                                    code: $scope.workTypes[i].code,
                                    include: true
                                });
                            }
                            itemGroup.history.marketAreas = [];
                            for (i = 0; i < $scope.marketAreas.marketAreas.length; i++) {
                                var ma = {
                                    id: $scope.marketAreas.marketAreas[i].id,
                                    name: $scope.marketAreas.marketAreas[i].name,
                                    include: true,
                                    counties: []
                                };
                                itemGroup.history.marketAreas.push(ma);
                                for (var ii = 0; ii < $scope.marketAreas.marketAreas[i].counties.length; ii++) {
                                    var c = {
                                        id: $scope.marketAreas.marketAreas[i].counties[ii].id,
                                        name: $scope.marketAreas.marketAreas[i].counties[ii].name,
                                        code: $scope.marketAreas.marketAreas[i].counties[ii].code,
                                        include: true
                                    };
                                    ma.counties.push(c);
                                }
                            }
                        }
                    });
                }
                itemGroup.showPricingParameters = !itemGroup.showPricingParameters;
            }
            //end bid history directive support
        }
    ]}
});