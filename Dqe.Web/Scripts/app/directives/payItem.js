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
        controller: function ($scope, $http) {
            if ($scope.item.id == 0) {
                $http.get('./PayItemStructureAdministration/GetSpecBooks').success(function (result) {
                    if (!containsDqeError(result)) {
                        $scope.specBooks = getDqeData(result);
                        $http.get('./PayItemStructureAdministration/GetCurrentSpecBook').success(function (res) {
                            if (!containsDqeError(res)) {
                                $scope.item.specBook = getDqeData(res).specBook;
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
                if (item.id == 0) {
                    item.structureId = $scope.structure.id;
                    item.showCurrent = $scope.showCurrent;
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
            }
        }
    }
});