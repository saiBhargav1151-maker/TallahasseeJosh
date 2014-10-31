dqeControllers.controller('HomeProposalPricesController', ['$scope', '$rootScope', '$http', function ($scope, $rootScope, $http) {
    $rootScope.$broadcast('initializeNavigation');
    $http.get('./estimate/LoadProposalEstimate').success(function (result) {
        if (!containsDqeError(result)) {
            $scope.estimate = getDqeData(result);
        }
    });
    $scope.loadProposal = function() {
        $http.get('./estimate/LoadProposal').success(function (result) {
            if (!containsDqeError(result)) {
                $scope.estimate = getDqeData(result);
            }
        });
    };
}]);