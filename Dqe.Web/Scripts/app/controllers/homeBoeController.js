dqeControllers.controller('HomeBoeController', ['$scope', '$rootScope', function ($scope, $rootScope) {
    $rootScope.$broadcast('initializeNavigation');
}]);