dqeControllers.controller('HomeSelectionProjectController', ['$scope', '$rootScope', '$http', '$route', 'stateService', function ($scope, $rootScope, $http, $route, stateService) {
    $rootScope.$broadcast('initializeNavigation');
    function processResult(result) {
        if (!containsDqeError(result)) {
            var r = getDqeData(result);
            stateService.currentEstimateId = r.workingEstimate == undefined ? 0 : r.workingEstimate.projectSnapshotId;
            $scope.security = r.security;
            $scope.project = r.project;
            $scope.proposals = r.proposals;
            $scope.workingEstimate = r.workingEstimate;
            $scope.versions = r.versions;
            $scope.authorizedUsers = r.authorizedUsers;
        }
    }
    $scope.isSynced = null;
    function checkSync(result) {
        if (!containsDqeError(result)) {
            $scope.isSynced = null;
            if ($scope.project == undefined || $scope.project.id == undefined) return;
            $http.get('./projectproposal/IsProjectSynced', { params: { projectId: $scope.project.id } }).success(function (res) {
                if (!containsDqeError(res)) {
                    var r = getDqeData(res);
                    $scope.isSynced = r.isSynced;
                }
            });
        }
    }
    function getRecentProject() {
        $http.get('./projectproposal/GetRecentProject').success(function (result) {
            processResult(result);
            checkSync(result);
        });
    }
    if ($route.current.params != 'undefined' && $route.current.params != null) {
        if ($route.current.params.project != 'undefined' && $route.current.params.project != null) {
            $http.get('./projectproposal/GetProject', { params: { number: $route.current.params.project } }).success(function (result) {
                processResult(result);
                checkSync(result);
            });
        } else {
            getRecentProject();
        }
    }
    $scope.synchronizeWorkingEstimate = function(estimate) {
        $http.post('./projectproposal/SyncWorkingEstimate', estimate).success(function (result) {
            processResult(result);
            getRecentProject();
        });
    }
    $scope.loadProject = function() {
        $http.get('./projectproposal/GetProject', { params: { number: $scope.selectedProject.number } }).success(function(result) {
            processResult(result);
        });
    };
    $scope.createProjectVersionFromWt = function(project) {
        $http.post('./projectproposal/CreateProjectVersionFromWt', project).success(function (result) {
            processResult(result);
            checkSync(result);
        });
    }
    $scope.createProjectVersionFromLre = function(project) {
        alert("Not Implemented");
    }
    $scope.releaseCustody = function(project) {
        $http.post('./projectproposal/ReleaseCustody', project).success(function (result) {
            processResult(result);
        });
    }
    $scope.aquireCustody = function(project) {
        $http.post('./projectproposal/AquireCustody', project).success(function (result) {
            processResult(result);
        });
    }
    $scope.snapshotWorkingEstimate = function(project) {
        $http.post('./projectproposal/SnapshotWorkingEstimate', project).success(function (result) {
            processResult(result);
        });
    }
    $scope.assignWorkingEstimate = function(version) {
        $http.post('./projectproposal/AssignWorkingEstimate', version).success(function (result) {
            processResult(result);
            checkSync(result);
        });
    }
    $scope.createProjectVersionFromEstimate = function(snapshot) {
        $http.post('./projectproposal/CreateProjectVersionFromEstimate', snapshot).success(function (result) {
            processResult(result);
        });
    }
    $scope.saveComment = function (project) {
        $http.post('./projectproposal/SaveComment', project).success(function (result) {
            processResult(result);
        });
    }
    $scope.getPotentialAuthorizedUsers = function (val) {
        return $http.get('./staff/GetDqeStaffByName', { params: { id: val } })
            .then(function (response) {
                var users = [];
                angular.forEach(response.data, function (item) {
                    users.push(item);
                });
                return users;
            });
    }
    $scope.authorizeUser = function () {
        if ($scope.selectedUser == undefined) return;
        $scope.selectedUser.project = $scope.project; 
        $http.post('./projectproposal/AuthorizeUser', $scope.selectedUser).success(function (result) {
            if (!containsDqeError(result)) {
                var r = getDqeData(result);
                $scope.authorizedUsers = r;
            }
            $scope.selectedUser = '';
        });
    }
    $scope.removeAuthorization = function(authorizedUser) {
        authorizedUser.project = $scope.project;
        $http.post('./projectproposal/DeauthorizeUser', authorizedUser).success(function (result) {
            if (!containsDqeError(result)) {
                var r = getDqeData(result);
                $scope.authorizedUsers = r;
            }
        });
    }
    $scope.getProjects = function (val) {
        return $http.get('./projectproposal/GetProjects', { params: { number: val } })
            .then(function(response) {
                var projects = [];
                angular.forEach(response.data, function(item) {
                    projects.push(item);
                });
                return projects;
            });
    };
}]);