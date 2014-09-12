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
    checkMasterFileCopyInProcess();
    $scope.$on('checkMasterFileCopyInProcess', function () {
        checkMasterFileCopyInProcess();
    });
    $http.get('./projectproposal/GetRecentProject').success(function (result) {
        if (!containsDqeError(result)) {
            var r = getDqeData(result);
            $scope.snapshot = r.snapshot;
            $scope.wtProject = r.wtProject;
            $scope.workingVersion = r.workingVersion;
            $scope.userVersions = r.userVersions;
            $scope.otherVersions = r.otherVersions;
        }
    });
    $scope.loadProject = function() {
        $http.get('./projectproposal/GetProjectList', { params: { number: $scope.selectedProject.number } }).success(function(result) {
            if (!containsDqeError(result)) {
                var r = getDqeData(result);
                $scope.snapshot = r.snapshot;
                $scope.wtProject = r.wtProject;
                $scope.workingVersion = r.workingVersion;
                $scope.userVersions = r.userVersions;
                $scope.otherVersions = r.otherVersions;
            }
        });
    };
    $scope.copyWtProject = function() {
        $http.post('./projectproposal/CreateProjectFromWt', $scope.wtProject).success(function (result) {
            if (!containsDqeError(result)) {
                var r = getDqeData(result);
                $scope.snapshot = r.snapshot;
                $scope.wtProject = r.wtProject;
                $scope.workingVersion = r.workingVersion;
                $scope.userVersions = r.userVersions;
                $scope.otherVersions = r.otherVersions;
            }
        });
    };
    $scope.copyProjectVersion = function (sourceProject) {
        $http.post('./projectproposal/CreateProjectFromVersion', sourceProject).success(function (result) {
            if (!containsDqeError(result)) {
                var r = getDqeData(result);
                $scope.snapshot = r.snapshot;
                $scope.wtProject = r.wtProject;
                $scope.workingVersion = r.workingVersion;
                $scope.userVersions = r.userVersions;
                $scope.otherVersions = r.otherVersions;
            }
        });
    };
    $scope.takeSnapshot = function(sourceProject) {
        $http.post('./projectproposal/SnapshotProjectVersion', sourceProject).success(function (result) {
            if (!containsDqeError(result)) {
                var r = getDqeData(result);
                $scope.snapshot = r.snapshot;
                $scope.wtProject = r.wtProject;
                $scope.workingVersion = r.workingVersion;
                $scope.userVersions = r.userVersions;
                $scope.otherVersions = r.otherVersions;
            }
        });
    };
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