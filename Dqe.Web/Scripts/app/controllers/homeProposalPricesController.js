dqeControllers.controller('HomeProposalPricesController', ['$scope', '$rootScope', '$http', function ($scope, $rootScope, $http) {
    $rootScope.$broadcast('initializeNavigation');
    $scope.pricingLevel = 'proposal';
}]);