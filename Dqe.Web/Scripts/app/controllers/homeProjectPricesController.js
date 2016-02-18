dqeControllers.controller('HomeProjectPricesController', ['$scope', '$rootScope', '$http', '$filter', function ($scope, $rootScope, $http, $filter) {
    $rootScope.$broadcast('initializeNavigation');
    $scope.pricingLevel = 'project';
}]);