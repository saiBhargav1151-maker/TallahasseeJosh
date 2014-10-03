dqeControllers.controller('HomeSelectionProposalController', ['$scope', '$rootScope', function ($scope, $rootScope) {
    $rootScope.$broadcast('initializeNavigation');
    function checkMasterFileCopyInProcess() {
        $http.get('./masterfileadministration/IsCopyInProcess').success(function (result) {
            if (!containsDqeError(result)) {
                $scope.copyMasterFileInProcess = getDqeData(result);
                if (!$scope.copyMasterFileInProcess) {
                    initialize();
                }
            }
        });
    }
    function processResult(result) {
        if (!containsDqeError(result)) {
            var r = getDqeData(result);
            //$scope.project = r.project;
            //$scope.workingEstimate = r.workingEstimate;
            //$scope.versions = r.versions;
        }
    }
    checkMasterFileCopyInProcess();
    $scope.$on('checkMasterFileCopyInProcess', function () {
        checkMasterFileCopyInProcess();
    });
    $scope.getProposals = function (val) {
        return $http.get('./projectproposal/GetProposals', { params: { number: val } })
            .then(function (response) {
                var proposals = [];
                angular.forEach(response.data, function (item) {
                    proposals.push(item);
                });
                return proposals;
            });
    };
}]);