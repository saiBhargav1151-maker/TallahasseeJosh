dqeControllers.controller('HomeGamingController', ['$scope', '$rootScope', function ($scope, $rootScope) {
    $rootScope.$broadcast('initializeNavigation');
}]);