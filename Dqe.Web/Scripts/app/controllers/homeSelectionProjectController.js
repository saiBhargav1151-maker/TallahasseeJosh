dqeControllers.controller('HomeSelectionProjectController', ['$scope', '$rootScope', '$http', function ($scope, $rootScope, $http) {
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
            $scope.project = r.project;
            $scope.workingEstimate = r.workingEstimate;
            $scope.versions = r.versions;
        }
    }
    checkMasterFileCopyInProcess();
    $scope.$on('checkMasterFileCopyInProcess', function () {
        checkMasterFileCopyInProcess();
    });
    $http.get('./projectproposal/GetRecentProject').success(function (result) {
        processResult(result);
    });
    $scope.loadProject = function() {
        $http.get('./projectproposal/GetProject', { params: { number: $scope.selectedProject.number } }).success(function(result) {
            processResult(result);
        });
    };
    $scope.createProjectVersionFromWt = function(project) {
        $http.post('./projectproposal/CreateProjectVersionFromWt', project).success(function (result) {
            processResult(result);
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
        });
    }
    $scope.createProjectVersionFromSnapshot = function(snapshot) {
        $http.post('./projectproposal/CreateProjectVersionFromSnapshot', snapshot).success(function (result) {
            processResult(result);
        });
    }
    $scope.saveComment = function (project) {
        $http.post('./projectproposal/SaveComment', project).success(function (result) {
            processResult(result);
        });
    }
    $scope.getProjects = function(val) {
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