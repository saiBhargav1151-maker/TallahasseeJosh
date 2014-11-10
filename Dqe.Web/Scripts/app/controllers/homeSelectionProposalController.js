dqeControllers.controller('HomeSelectionProposalController', ['$scope', '$rootScope', '$http', '$route', function ($scope, $rootScope, $http, $route) {
    $rootScope.$broadcast('initializeNavigation');
    function checkMasterFileCopyInProcess() {
        $http.get('./masterfileadministration/IsCopyInProcess').success(function (result) {
            if (!containsDqeError(result)) {
                $scope.copyMasterFileInProcess = getDqeData(result);
            }
        });
    }
    function processResult(result) {
        if (!containsDqeError(result)) {
            var r = getDqeData(result);
            $scope.proposal = r.proposal;
            $scope.projects = r.projects;
            for (var i = 0; i < $scope.projects.length; i++) {
                syncProject($scope.projects[i]);
            }
        }
    }
    function syncProject(project) {
        $http.get('./projectproposal/IsProjectSynced', { params: { projectId: project.id } }).success(function (result) {
            if (!containsDqeError(result)) {
                var r = getDqeData(result);
                project.isSynced = r.isSynced;
            }
        });
    }
    checkMasterFileCopyInProcess();
    if ($route.current.params != 'undefined' && $route.current.params != null) {
        if ($route.current.params.proposal != 'undefined' && $route.current.params.proposal != null) {
            $http.get('./projectproposal/GetProposal', { params: { number: $route.current.params.proposal } }).success(function(result) {
                processResult(result);
            });
        } else {
            $http.get('./projectproposal/GetRecentProposal').success(function (result) {
                processResult(result);
            });
        }
    }
    $scope.$on('checkMasterFileCopyInProcess', function () {
        checkMasterFileCopyInProcess();
    });
    $scope.loadProposal = function() {
        $http.get('./projectproposal/GetProposal', { params: { number: $scope.selectedProposal.number } }).success(function (result) {
            processResult(result);
        });
    }
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