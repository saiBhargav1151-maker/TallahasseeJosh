dqeControllers.controller('HomeSelectionProposalController', ['$scope', '$rootScope', '$http', '$route', 'stateService', function ($scope, $rootScope, $http, $route, stateService) {
    $rootScope.$broadcast('initializeNavigation');
    function processResult(result) {
        if (!containsDqeError(result)) {
            var r = getDqeData(result);
            stateService.currentProposalId = r.proposal == undefined ? '0' : r.proposal.id;
            $scope.proposal = r.proposal;
            $scope.projects = r.projects;
            hasCustodyAndEstimate();
            //if ($scope.hasCustodyAndEstimate) {
            //    //check proposal for initial structure creation
            //}
        }
    }
    $scope.checkProjectSync = function () {
        $scope.isSyncCheckInProgress = true;
        for (var i = 0; i < $scope.projects.length; i++) {
            $scope.projects[i].hasCheckedSync = false;
            $scope.projects[i].isSynced = null;
        }
        $http.post('./estimate/DoProposalSync', { estimateId: stateService.currentProposalId }).success(function (result) {
            if (!containsDqeError(result)) {
                syncProjects();        
            }
        });
    }
    function hasCustodyAndEstimate() {
        $scope.hasCustodyAndEstimate = true;
        for (var i = 0; i < $scope.projects.length; i++) {
            if (!$scope.projects[i].hasCustody || !$scope.projects[i].hasWorkingEstimate) {
                $scope.hasCustodyAndEstimate = false;
                break;
            }
        }
    }
    function syncProjects() {
        var project = null;
        for (var i = 0; i < $scope.projects.length; i++) {
            if (!$scope.projects[i].hasCheckedSync) {
                project = $scope.projects[i];
                break;
            }
        }
        if (project == null) {
            $scope.isSyncCheckInProgress = false;
        } else {
            $http.get('./projectproposal/IsProjectSyncedForProposal', { params: { projectId: project.id } }).success(function (result) {
                if (!containsDqeError(result)) {
                    var r = getDqeData(result);
                    project.hasCheckedSync = true;
                    project.isSynced = r.isSynced;
                    syncProjects();
                }
            });
        }
    }
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
        if (proposal.takeLabeledSnapshot == undefined) {
            proposal.takeLabeledSnapshot = false;
        }
        $http.post('./projectproposal/BuildProposalReportData', proposal).success(function (result) {
            if (!containsDqeError(result)) {
                $http.post('./estimate/WriteProposalPrices', proposal).success(function (res) {
                    if (!containsDqeError(res)) {
                        for (var i = 0; i < $scope.projects.length; i++) {
                            var project = $scope.projects[i];
                            project.snapshotTaken = false;
                            project.takeLabeledSnapshot = proposal.takeLabeledSnapshot;
                        }
                        takeSnapshot(proposal);
                    }
                });
            }
        });
    }
    function takeSnapshot(proposal) {
        var project = null;
        for (var i = 0; i < $scope.projects.length; i++) {
            if (!$scope.projects[i].snapshotTaken) {
                project = $scope.projects[i];
                project.snapshotTaken = true;
                break;
            }
        }
        if (project != null) {
            $http.post('./projectproposal/SnapshotProposalWorkingEstimate', project).success(function(result) {
                if (!containsDqeError(result)) {
                    project.label = getDqeData(result).label;
                }
                takeSnapshot(proposal);
            });
        } else {
            $http.post('./projectproposal/GetProposalNextSnapshot', proposal).success(function (result) {
                if (!containsDqeError(result)) {
                    proposal.nextEstimate = getDqeData(result).label;
                }
            });
        }
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