dqeControllers.controller('HomeReportsBidToleranceController', ['$scope', '$rootScope', '$http', function ($scope, $rootScope, $http) {
    $rootScope.$broadcast('initializeNavigation');

    $scope.getLettings = function (val) {
        return $http.get('./report/GetLettings', { params: { number: val } })
            .then(function (response) {
                var letting = [];
                angular.forEach(response.data, function (item) {
                    letting.push(item);
                });
                return letting;
            });
    };
}]);