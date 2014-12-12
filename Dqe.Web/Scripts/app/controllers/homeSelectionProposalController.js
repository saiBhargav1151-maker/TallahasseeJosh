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
            //check if this async is executed in the proper context
            $http.get('./estimate/LoadProposalEstimateSummary').success(function (res) {
                if (!containsDqeError(res)) {
                    $scope.total = getDqeData(res).total;
                }
            });
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
    $scope.snapshotWorkingEstimate = function (proposal) {
        $http.post('./projectproposal/SnapshotProposalWorkingEstimate', proposal).success(function (result) {
            processResult(result);
        });
    }
    $scope.canSnapshotProposal = function () {
        if ($scope.proposal == undefined || $scope.proposal == null) return false;
        if (!$scope.proposal.hasCustody) return false;
        if (!$scope.proposal.canSnapshot) return false;
        if ($scope.projects == undefined || $scope.projects == null || $scope.projects.length ==0) return false;
        for (var i = 0; i < $scope.projects.length; i++) {
            if (!$scope.projects[i].isSynced) return false;
        }
        return true;
    }
}]);