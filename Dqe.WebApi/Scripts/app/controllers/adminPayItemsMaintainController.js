dqeControllers.controller('AdminPayItemsMaintainController', ['$scope', '$rootScope', function ($scope, $rootScope) {
    $rootScope.$broadcast('initializeNavigation');
    $scope.show101 = false;
    $scope.toggle101 = function() {
        if ($scope.show101 == true) {
            $scope.show101 = false;
        } else {
            $scope.show101 = true;
        }

    }
}]);