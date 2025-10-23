dqeControllers.controller('HomeSelectionProposalController', ['$scope', '$rootScope', '$http', '$location', '$route', 'stateService', function ($scope, $rootScope, $http, $location, $route, stateService) {
    $rootScope.$broadcast('initializeNavigation');
    function processResult(result) {
        if (!containsDqeError(result)) {
            var r = getDqeData(result);
            stateService.currentProposalId = r.proposal == undefined ? '0' : r.proposal.id;
            $scope.proposal = r.proposal;
            $scope.projects = r.projects;
            $scope.security = r.security;
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
    $scope.deleteProposal = function (proposal) {
        $http.post('./projectproposal/DeleteProposal', proposal).success(function (result) {
            if (!containsDqeError(result)) {
                $scope.security = undefined;
                $scope.proposal = undefined;
                $scope.projects = undefined;
            }
        });
    }
    $scope.deleteSnapshot = function (proposal) {
        $http.post('./projectproposal/DeleteProposalSnapshot', proposal).success(function (result) {
            if (!containsDqeError(result)) {
                processResult(result);
            }
        });
    }
    function hasCustodyAndEstimate() {
        $scope.hasCustodyAndEstimate = true;
        if ($scope.projects == undefined) return;
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
            $http.get('./projectproposal/GetProposal', { params: { number: $route.current.params.proposal } }).success(function (result) {
                stateService.currentProposal = $route.current.params.proposal;
                processResult(result);
            });
        } else {
            $http.get('./projectproposal/GetRecentProposal').success(function (result) {
                processResult(result);
            });
        }
    }
    //Previously this was just reloading the data on the page, but decided on refreshing the page altogether because of hard to isolate occasional non syncronous loading of data. MB. 
    $scope.loadProposal = function () {
        $http.get('./projectproposal/GetProposal', { params: { number: $scope.selectedProposal.number } }).success(function (result) {
            $location.url('/home_proposal/' + $scope.selectedProposal.number);

            // //unreached older way
            //stateService.currentProposal = $scope.selectedProposal.number;
            //processResult(result);
        });
    }
    $scope.getProposals = function (val) {
        return $http.get('./projectproposal/GetProposals', { params: { number: val } })
            .then(function (response) {
                var proposals = [];
                angular.forEach(response.data, function (item) {
                    item.displayName = item.number;
                    if ((item.contractType[0] == 'M')){
                        item.displayName = '(M) ' + item.displayName;
                    }
                    else {
                        item.displayName = '(C) ' + item.displayName;
                    }
                    proposals.push(item);
                });
                return proposals;
           });
    };
    $scope.snapshotWorkingEstimate = function (proposal) {
        if (proposal.takeLabeledSnapshot == undefined) {
            proposal.takeLabeledSnapshot = false;
        }

        $http.post('./estimate/WriteProposalPrices', proposal).success(function (res) {
            if (!containsDqeError(res)) {
                var data = getDqeData(res);
                if (data != null && data.isOfficial) {
                    proposal.isOfficial = true;
                }

                for (var i = 0; i < $scope.projects.length; i++) {
                    var project = $scope.projects[i];
                    project.snapshotTaken = false;
                    project.takeLabeledSnapshot = proposal.takeLabeledSnapshot;
                }
                takeSnapshot(proposal);

                if (data != null) {
                    proposal.authorizationTotal = data.authorizationTotal;
                    proposal.officialTotal = data.officialTotal;
                }

                if (proposal.nextEstimate === "Authorization" && proposal.takeLabeledSnapshot) {
                    $http.post('./report/SendAuthorizationReport', proposal);
                }
            }
        });
    }
    function takeSnapshot(proposal) {
        var project = null;
        for (var i = 0; i < $scope.projects.length; i++) {
            if (!$scope.projects[i].snapshotTaken) {
                project = $scope.projects[i];
                project.snapshotTaken = true;
                project.comment = proposal.comment;
                break;
            }
        }
        if (project != null) {
            $http.post('./projectproposal/SnapshotProposalWorkingEstimate', project).success(function (result) {
                if (!containsDqeError(result)) {
                    project.label = getDqeData(result).label;
                }
                takeSnapshot(proposal);
            });
        } else {
            $http.post('./projectproposal/GetProposalNextSnapshot', proposal).success(function (result) {
                if (!containsDqeError(result)) {
                    var data = getDqeData(result);
                    proposal.nextEstimate = data.label;
                    proposal.isOfficial = data.isOfficial;
                    proposal.takeLabeledSnapshot = false;
                    proposal.comment = '';
                }
            });
        }
    }
    $scope.canSnapshotProposal = function () {
        if ($scope.proposal == undefined || $scope.proposal == null) return false;
        if (!$scope.proposal.hasCustody) return false;
        if (!$scope.proposal.canSnapshot) return false;
        if ($scope.projects == undefined || $scope.projects == null || $scope.projects.length == 0) return false;
        for (var i = 0; i < $scope.projects.length; i++) {
            if (!$scope.projects[i].isSynced) return false;
        }
        return true;
    }
}]);