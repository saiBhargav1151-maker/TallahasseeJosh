dqeControllers.controller('HomePricingPricesController', ['$scope', '$rootScope', '$http', function ($scope, $rootScope, $http) {
    $rootScope.$broadcast('initializeNavigation');
    $http.get('./estimate/LoadEstimate').success(function (result) {
        if (!containsDqeError(result)) {
            $scope.estimate = getDqeData(result);
        }
    });
    $scope.loadProject = function() {
        $http.get('./estimate/LoadProject').success(function (result) {
            if (!containsDqeError(result)) {
                $scope.estimate = getDqeData(result);
            }
        });
    }
    $scope.loadProposal = function() {
        $http.get('./estimate/LoadProposal').success(function(result) {
            if (!containsDqeError(result)) {
                $scope.estimate = getDqeData(result);
            }
        });
    };
    $scope.getSetTotal = function (set) {
        if ($scope.totals == undefined) {
            $scope.calculateTotals();
        }
        for (var i = 0; i < $scope.totals.sets.length; i++) {
            if ($scope.totals.sets[i].set == set) {
                return $scope.totals.sets[i].total;
            }
        }
        return 0;
    };
    $scope.getGroupTotal = function (set, group) {
        if ($scope.totals == undefined) {
            $scope.calculateTotals();
        }
        for (var i = 0; i < $scope.totals.sets.length; i++) {
            if ($scope.totals.sets[i].set == set) {
                for (var ii = 0; ii < $scope.totals.sets[i].set.groups.length; ii++) {
                    if ($scope.totals.sets[i].groups[ii].group == group) {
                        return $scope.totals.sets[i].groups[ii].total;
                    }
                }
            }
        }
        return 0;
    };
    $scope.getProjectTotal = function () {
        if ($scope.totals == undefined) {
            $scope.calculateTotals();
        }
        var projectTotal = 0;
        var likeSets = {
            ids: []
        }
        for (var i = 0; i < $scope.totals.sets.length; i++) {
            if ($scope.totals.sets[i].set.id != '') {
                var added = false;
                for (var ii = 0; ii < likeSets.ids.length; ii++) {
                    if (likeSets.ids[ii].id == $scope.totals.sets[i].set.id) {
                        likeSets.ids[ii].totals.push($scope.totals.sets[i].total);
                        added = true;
                    }
                }
                if (!added) {
                    likeSets.ids.push({
                        id: $scope.totals.sets[i].set.id,
                        totals: [$scope.totals.sets[i].total]
                    });
                }
            }
        }
        for (i = 0; i < likeSets.ids.length; i++) {
            projectTotal += Math.min.apply(null, likeSets.ids[i].totals);
        }
        for (i = 0; i < $scope.totals.sets.length; i++) {
            if ($scope.totals.sets[i].set.id == '') {
                for (ii = 0; ii < $scope.totals.sets[i].set.groups.length; ii++) {
                    projectTotal += $scope.totals.sets[i].groups[ii].total;
                }
            } 
        }
        return projectTotal;
    }
    $scope.calculateTotals = function () {
        $scope.totals = {
            sets: []
        }
        if ($scope.estimate == undefined || $scope.estimate == null || $scope.estimate.sets == undefined || $scope.estimate.sets == null || $scope.estimate.sets.length == 0) return;
        for (var i = 0; i < $scope.estimate.sets.length; i++) {
            $scope.totals.sets.push({
                set: $scope.estimate.sets[i],
                groups: [],
                total: 0
            });
            if ($scope.estimate.sets[i].groups == undefined || $scope.estimate.sets[i].groups == null || $scope.estimate.sets[i].groups.length == 0) continue;
            for (var ii = 0; ii < $scope.estimate.sets[i].groups.length; ii++) {
                $scope.totals.sets[i].groups.push({
                    group: $scope.estimate.sets[i].groups[ii],
                    total: 0
                });
                if ($scope.estimate.sets[i].groups[ii].payItems == undefined || $scope.estimate.sets[i].groups[ii].payItems == null || $scope.estimate.sets[i].groups[ii].payItems.length == 0) continue;
                for (var iii = 0; iii < $scope.estimate.sets[i].groups[ii].payItems.length; iii++) {

                    $scope.totals.sets[i].total += $scope.estimate.sets[i].groups[ii].payItems[iii].quantity * $scope.estimate.sets[i].groups[ii].payItems[iii].price;
                    $scope.totals.sets[i].groups[ii].total += $scope.estimate.sets[i].groups[ii].payItems[iii].quantity * $scope.estimate.sets[i].groups[ii].payItems[iii].price;
                }
            }
        }
    };
    $scope.saveEstimate = function() {
        if ($scope.estimate == undefined
            || $scope.estimate == null
            || $scope.estimate.sets == undefined
            || $scope.estimate.sets == null
            || $scope.estimate.sets.length == 0
            ) return;
        $http.post('./estimate/SaveEstimate', $scope.estimate);
    }
}]);