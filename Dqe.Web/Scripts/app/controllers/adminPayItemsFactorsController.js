dqeControllers.controller('AdminPayItemsFactorsController', ['$scope', '$rootScope','$http', function ($scope, $rootScope, $http) {
    $rootScope.$broadcast('initializeNavigation');

    $scope.payItems = [];

    $scope.submitFactors = function() {
        $http.post('./PayItemAdministration/UpdateFactors',$scope.payItems).success(function(result) {

        });
    }

    var loadData = function() {
        $http.get('./PayItemAdministration/GetAllPayItems').success(function(result) {
            $scope.payItems = result;
        });
    }

    loadData();

}]);