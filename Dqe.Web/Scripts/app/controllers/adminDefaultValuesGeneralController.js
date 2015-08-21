dqeControllers.controller('AdminDefaultValuesGeneralController', ['$scope', '$rootScope', '$http', function ($scope, $rootScope, $http) {
    $rootScope.$broadcast('initializeNavigation');
    $http.get('./profile/InitializeParametersFromWt').success(function (result) {
        if (!containsDqeError(result)) {
            var r = getDqeData(result);
            $scope.parms = {
                loadWtPrices: r.loadPrices
            };
        }
    });
    $scope.save = function () {
        var parms = {
            loadPrices: $scope.parms.loadWtPrices
        };
        $http.post('./profile/SetParameters', parms);
    }
}]);