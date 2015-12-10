dqeControllers.controller('HomeProposalPricesController', ['$scope', '$rootScope', '$http', function ($scope, $rootScope, $http) {
    $rootScope.$broadcast('initializeNavigation');
    function buildSummary() {
        $scope.estimateSummary = [];
        for (var i = 0; i < $scope.estimate.total.CategorySets.length; i++) {
            for (var ii = 0; ii < $scope.estimate.total.CategorySets[i].ItemSets.length; ii++) {
                $scope.estimateSummary.push({
                    included: $scope.estimate.total.CategorySets[i].Included && $scope.estimate.total.CategorySets[i].ItemSets[ii].Included,
                    categorySet: $scope.estimate.total.CategorySets[i].Set,
                    categoryMember: $scope.estimate.total.CategorySets[i].Member,
                    itemSet: $scope.estimate.total.CategorySets[i].ItemSets[ii].Set,
                    itemMember: $scope.estimate.total.CategorySets[i].ItemSets[ii].Member,
                    total: $scope.estimate.total.CategorySets[i].ItemSets[ii].Total,
                });
            }
        }
    }
    $http.get('./estimate/LoadProposalEstimate').success(function (result) {
        if (!containsDqeError(result)) {
            $scope.estimate = getDqeData(result);
            buildSummary();
        }
    });
    $scope.toggleGroup = function (itemGroup) {
        if (itemGroup.isExpanded) {
            itemGroup.isExpanded = false;
            angular.forEach($scope.estimate.itemGroups, function (i) {
                if (i.key == itemGroup.key && !i.group) {
                    i.isExpanded = false;
                }
            });
        } else {
            itemGroup.isExpanded = true;
            angular.forEach($scope.estimate.itemGroups, function (i) {
                if (i.key == itemGroup.key && !i.group) {
                    i.isExpanded = true;
                }
            });
        }
    }
    $scope.calculateTotals = function (itemGroup) {
        if (!isNumber(itemGroup.price)) {
            itemGroup.price = 0;
        }
        if (itemGroup.price < 0) {
            itemGroup.price = 0;
        }
        if (itemGroup.price == 0) {
            itemGroup.priceType = "N";
        } else {
            itemGroup.priceType = "O";
        }
        if (itemGroup.group) {
            angular.forEach($scope.estimate.itemGroups, function (i) {
                if (i.key == itemGroup.key && !i.group) {
                    i.price = itemGroup.price;
                    if (i.price == 0) {
                        i.priceType = "N";
                    } else {
                        i.priceType = "O";
                    }
                }
            });
        }
    }
    $scope.saveProposalEstimate = function () {
        if ($scope.estimate == undefined
            || $scope.estimate == null
            ) return;
        $scope.estimate.isSystemSync = false;
        $http.post('./estimate/SaveProposalEstimate', $scope.estimate).success(function (result) {
            if (!containsDqeError(result)) {
                $http.get('./estimate/LoadProposalEstimateSummary').success(function (r) {
                    if (!containsDqeError(r)) {
                        $scope.estimate.total = getDqeData(r).total;
                        buildSummary();
                    }
                });
            }
        });
    }
}]);