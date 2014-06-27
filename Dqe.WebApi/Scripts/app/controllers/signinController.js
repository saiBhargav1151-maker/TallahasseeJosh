dqeControllers.controller('SigninController', ['$scope', '$rootScope', '$location', 'securityService', function ($scope, $rootScope, $location, securityService) {
    $rootScope.$broadcast('initializeNavigation');
    $scope.roles = securityService.roles;
    $scope.selectedRole = $scope.roles[0];
    $scope.setRole = function(role) {
        $rootScope.$broadcast('setRole', role);
    };
}]);