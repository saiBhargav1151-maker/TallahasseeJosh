dqeControllers.controller('HomeSelectionProjectController', ['$scope', '$rootScope', '$http', function ($scope, $rootScope, $http) {
    /*
    project 
    {
        id
        description
        district
        county
        lettingDate
        isAvailable
        userHasCustody
    },
    workingEstimate 
    {
        projectVersionId
        projectSnapshotId
        projectVersion
        projectSnapshot
        label
        created
        lastUpdated
        estimate
        comment
        owner
        source 
    } 
    versions[]
    {
        projectVersionId
        owner
        projectVersion
        source
        snapshots[]
        {
            projectSnapshotId
            projectSnapshot
            created
            lastUpdated
            comment
            estimate
            label
        }
    }
    */
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
            $scope.project = r.project;
            $scope.workingEstimate = r.workingEstimate;
            $scope.versions = r.versions;
        }
    });
    $scope.loadProject = function() {
        $http.get('./projectproposal/GetProject', { params: { number: $scope.selectedProject.number } }).success(function(result) {
            if (!containsDqeError(result)) {
                var r = getDqeData(result);
                $scope.project = r.project;
                $scope.workingEstimate = r.workingEstimate;
                $scope.versions = r.versions;
            }
        });
    };
    $scope.createProjectVersionFromWt = function(project) {
        $http.post('./projectproposal/CreateProjectVersionFromWt', project).success(function (result) {
            if (!containsDqeError(result)) {
                var r = getDqeData(result);
                $scope.project = r.project;
                $scope.workingEstimate = r.workingEstimate;
                $scope.versions = r.versions;
            }
        });
    }
    //$scope.copyWtProject = function() {
    //    $http.post('./projectproposal/CreateProjectFromWt', $scope.wtProject).success(function (result) {
    //        if (!containsDqeError(result)) {
    //            var r = getDqeData(result);
    //            $scope.label = r.label;
    //            $scope.wtProject = r.wtProject;
    //            $scope.workingEstimate = r.workingEstimate;
    //            $scope.otherSnapshots = r.otherSnapshots;
    //        }
    //    });
    //};
    //$scope.copyProjectVersion = function (sourceProject) {
    //    $http.post('./projectproposal/CreateProjectFromVersion', sourceProject).success(function (result) {
    //        if (!containsDqeError(result)) {
    //            var r = getDqeData(result);
    //            $scope.label = r.label;
    //            $scope.wtProject = r.wtProject;
    //            $scope.workingEstimate = r.workingEstimate;
    //            $scope.otherSnapshots = r.otherSnapshots;
    //        }
    //    });
    //};
    //$scope.takeSnapshot = function(sourceProject) {
    //    $http.post('./projectproposal/SnapshotProjectVersion', sourceProject).success(function (result) {
    //        if (!containsDqeError(result)) {
    //            var r = getDqeData(result);
    //            $scope.label = r.label;
    //            $scope.wtProject = r.wtProject;
    //            $scope.workingEstimate = r.workingEstimate;
    //            $scope.otherSnapshots = r.otherSnapshots;
    //        }
    //    });
    //};
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