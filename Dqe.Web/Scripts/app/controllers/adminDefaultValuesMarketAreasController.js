dqeControllers.controller('AdminDefaultValuesMarketAreasController', ['$scope', '$rootScope', '$http', function ($scope, $rootScope, $http) {
    $rootScope.$broadcast('initializeNavigation');
    function getAvailableCounties() {
        $http.get('./marketarea/GetUnassignedCounties').success(function (result) {
            if (!containsDqeError(result)) {
                var r = getDqeData(result);
                $scope.unassignedCounties = r.counties;
            }
        });
    }
    $http.get('./marketarea/GetMarketAreas').success(function (result) {
        if (!containsDqeError(result)) {
            var r = getDqeData(result);
            $scope.marketAreas = r.marketAreas;
        }
    });
    getAvailableCounties();
    $scope.addNewMarketArea = function () {
        var marketArea = { name: $scope.newMarketArea }
        $http.post('./marketarea/AddMarketArea', marketArea).success(function (result) {
            if (!containsDqeError(result)) {
                var r = getDqeData(result);
                $scope.marketAreas = r.marketAreas;
            }
        });
    }
    $scope.addCounty = function (marketArea) {
        $http.post('./marketarea/AddCounty', marketArea).success(function (result) {
            if (!containsDqeError(result)) {
                var r = getDqeData(result);
                $scope.marketAreas = r.marketAreas;
                getAvailableCounties();
            }
        });
    }
    $scope.removeCounty = function(county) {
        $http.post('./marketarea/RemoveCounty', county).success(function (result) {
            if (!containsDqeError(result)) {
                var r = getDqeData(result);
                $scope.marketAreas = r.marketAreas;
                getAvailableCounties();
            }
        });
    }
    $scope.removeMarketArea = function (marketArea) {
        $http.post('./marketarea/RemoveMarketArea', marketArea).success(function (result) {
            if (!containsDqeError(result)) {
                var r = getDqeData(result);
                $scope.marketAreas = r.marketAreas;
                getAvailableCounties();
            }
        });
    }
}]);