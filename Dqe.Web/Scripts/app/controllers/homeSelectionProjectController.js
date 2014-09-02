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
    $http.get('./projectproposal/GetRecentProject').success(function(result) {
        if (!containsDqeError(result)) {
            var project = getDqeData(result);
            $scope.id = project.id;
            $scope.number = project.number;
            $scope.county = project.county;
            $scope.description = project.description;
        }
    });
    $scope.loadProject = function() {
        $http.get('./projectproposal/GetProject', { params: { number: $scope.selectedProject.number } }).success(function (result) {
            if (!containsDqeError(result)) {
                var project = getDqeData(result);
                $scope.id = project.id;
                $scope.number = project.number;
                $scope.county = project.county;
                $scope.description = project.description;
            }
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
    }
}]);