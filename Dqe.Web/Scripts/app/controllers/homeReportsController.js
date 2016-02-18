dqeControllers.controller('HomeReportsController', ['$scope', '$rootScope', '$http', function ($scope, $rootScope, $http) {
    $rootScope.$broadcast('initializeNavigation');
    $scope.testParm = "Kevin";
    $http.get('./marketarea/GetMarketAreas').success(function (result) {
        if (!containsDqeError(result)) {
            var r = getDqeData(result);
            $scope.marketAreas = r.marketAreas;
        }
    });
    //$scope.getProposals = function (val) {
    //    return $http.get('./projectproposal/GetProposals', { params: { number: val } })
    //        .then(function (response) {
    //            var proposals = [];
    //            angular.forEach(response.data, function (item) {
    //                proposals.push(item);
    //            });
    //            return proposals;
    //        });
    //};
}]);