dqeControllers.controller('AdminCostGroupsController', [
    '$scope', '$rootScope', '$http', '$filter', function($scope, $rootScope, $http, $filter) {
        $rootScope.$broadcast('initializeNavigation');

        function shouldShowPayItems(payItems) {
            if (payItems.length > 0) {
                return true;
            } else {
                return false;
            }
        };

        function checkNumeric(arr) {
            var val = "";
            var decimalLocation = "";
            var countBeforeDecimal = 0;
            var countAfterDecimal = 0;
            for (var i = 0; i < arr.length; i++) {
                if (isNumber(arr[i]) || (arr[i] === "." && decimalLocation === "")) {
                    if (arr[i] === ".") {
                        decimalLocation = i;
                    }
                    if (isNumber(arr[i])) {
                        if (decimalLocation === "") {
                            countBeforeDecimal++;
                        } else {
                            countAfterDecimal++;
                        }
                    }
                    if (countBeforeDecimal <= 12 && countAfterDecimal <= 2) {
                        val = val + arr[i];
                    }
                }
            }
            return val;
        };

        $http.get('./PayItemStructureAdministration/GetUnitCodes').success(function (result) {
            if (!containsDqeError(result)) {
                var units = getDqeData(result);
                $scope.primaryUnits = units.slice();
                $scope.hybridUnits = units.slice();
            }
        });

        $http.get('./CostGroup/GetCostGroups').success(function(result) {
            if (!containsDqeError(result)) {
                $scope.costGroups = getDqeData(result);
                angular.forEach($scope.costGroups, function(i) {
                    i.showDetail = false;
                });
                $scope.filter = {
                    id: '',
                    name: '',
                    description: '',
                    unit: ''
                };
                $scope.filterItems();
            }
        });

        $scope.filterItems = function() {
            $scope.currentPage = 0;
            $scope.filteredItems = $filter('filter')($scope.costGroups,
            {
                id: $scope.filter.id,
                name: $scope.filter.name,
                description: $scope.filter.description,
                unit: $scope.filter.unit
            });
        };

        $scope.clearAllFilters = function() {
            $scope.filteredItems = $filter('filter')($scope.costGroups,
            {
                id: '',
                name: '',
                description: '',
                unit: ''
            });
            $scope.filter.id = '';
            $scope.filter.name = '';
            $scope.filter.description = '';
            $scope.filter.unit = '';
            $scope.filterItems();
        };

        $scope.addNewCostGroup = function() {
            $scope.costGroup =
            {
                id: '',
                name: '',
                description: '',
                unit: ''
            };
            $scope.showNewCostGroup = true;
        };

        $scope.cancelCostGroup = function () {
            $scope.showNewCostGroup = false;
        };

        $scope.cancelEdit = function(costGroup) {
            costGroup.showDetail = false;
        };

        $scope.saveCostGroup = function(costGroup) {
            $http.post('./CostGroup/SaveCostGroup', costGroup).success(function(result) {
                if (!containsDqeError(result)) {
                    costGroup = getDqeData(result);
                    $scope.costGroups.push(costGroup);
                    $scope.filterItems();
                    $scope.showNewCostGroup = false;
                }
            });
        };

        $scope.editCostGroup = function (costGroup) {
            $http.post('./CostGroup/EditCostGroup', costGroup).success(function (result) {
                if (!containsDqeError(result)) {
                    costGroup.showDetail = false;
                }
            });
        };

        $scope.removeCostGroup = function(costGroup) {
            $http.post('./CostGroup/DeleteCostGroup', costGroup).success(function(result) {
                if (!containsDqeError(result)) {
                    var index = $scope.costGroups.indexOf(costGroup);
                    $scope.costGroups.splice(index, 1);
                    $scope.filterItems();
                }
            });
        };

        $scope.toggleGroupDetail = function(costGroup) {
            if (costGroup.showDetail) {
                costGroup.showDetail = false;
            } else {
                costGroup.showDetail = true;
                costGroup.showPayItems = shouldShowPayItems(costGroup.payItems);
            }
        };

        $scope.removeItem = function(costGroup, payItem) {
            $http.post('./CostGroup/RemovePayItem', payItem).success(function(result) {
                if (!containsDqeError(result)) {
                    var index = costGroup.payItems.indexOf(payItem);
                    costGroup.payItems.splice(index, 1);
                    //$scope.showPayItems(costGroup.payItems);
                    costGroup.showPayItems = shouldShowPayItems(costGroup.payItems);
                };
            });
        };

        $scope.getPayItems = function (costGroup, val) {
            return $http.get('./costgroup/GetPayItems', { params: { payItemName: val, costGroupId: costGroup.id } })
                .then(function (response) {
                    var links = [];
                    angular.forEach(response.data.data, function(item) {
                        links.push(item);
                    });
                    return links;
            });
        };

        $scope.newItem = {
            linkItem: "",
            conversionFactor: ""
        };

        $scope.saveCostGroupPayItem = function (costGroup, newItem) {
            var costGroupPayItem = {
                costGroupId: costGroup.id,
                payItemId: newItem.linkItem.id,
                conversionFactor: newItem.conversionFactor
            };
            $http.post('./CostGroup/SaveCostGroupPayItem', costGroupPayItem).success(function (result) {
                if (!containsDqeError(result)) {
                    $scope.linkItem = null;
                    $scope.conversionFactor = null;
                    var cgpi = getDqeData(result);
                    costGroup.payItems.push(cgpi);
                    $scope.newItem = {
                        linkItem: "",
                        conversionFactor: ""
                    };
                    //$scope.showPayItems(costGroup.payItems);
                    costGroup.showPayItems = shouldShowPayItems(costGroup.payItems);
                };
            });
        };

        $scope.checkConversionFactorNumeric = function (newItem) {
            var arr = newItem.conversionFactor.split("");
            newItem.conversionFactor = checkNumeric(arr);
        }


}]);