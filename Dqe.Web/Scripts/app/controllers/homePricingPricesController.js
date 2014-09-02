dqeControllers.controller('HomePricingPricesController', ['$scope', '$rootScope', '$http', function ($scope, $rootScope, $http) {
    $rootScope.$broadcast('initializeNavigation');
    $http.get('./estimate/LoadEstimate').success(function (result) {
        if (!containsDqeError(result)) {
            $scope.estimate = getDqeData(result);
        }
    });
    $scope.getTotal = function() {
        if ($scope.estimate == undefined || $scope.estimate == null || $scope.estimate.groups == undefined || $scope.estimate.groups == null || $scope.estimate.groups.length == 0) return 0;
        var total = 0;
        for (var i = 0; i < $scope.estimate.groups.length; i++) {
            if ($scope.estimate.groups[i].payItems == undefined || $scope.estimate.groups[i].payItems == null || $scope.estimate.groups[i].payItems.length == 0) continue;
            for (var ii = 0; ii < $scope.estimate.groups[i].payItems.length; ii++) {
                total += $scope.estimate.groups[i].payItems[ii].quantity * $scope.estimate.groups[i].payItems[ii].price;
            }
        }
        return total;
    };
    $scope.saveEstimate = function() {
        if ($scope.estimate == undefined || $scope.estimate == null || $scope.estimate.groups == undefined || $scope.estimate.groups == null || $scope.estimate.groups.length == 0) return;
        $http.post('./estimate/SaveEstimate', $scope.estimate);
    }
}]);