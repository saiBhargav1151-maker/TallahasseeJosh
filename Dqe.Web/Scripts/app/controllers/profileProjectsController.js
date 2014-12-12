dqeControllers.controller('ProfileProjectsController', ['$scope', '$rootScope', '$http', function ($scope, $rootScope, $http) {
    $rootScope.$broadcast('initializeNavigation');
    $http.get('./profile/GetRecentProjects').success(function (result) {
        if (!containsDqeError(result)) {
            $scope.projects = getDqeData(result);
        }
    });
}]);