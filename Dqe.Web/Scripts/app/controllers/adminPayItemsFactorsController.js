dqeControllers.controller('AdminPayItemsFactorsController', ['$scope', '$rootScope','$http', function ($scope, $rootScope, $http) {
    $rootScope.$broadcast('initializeNavigation');
    function loadPayItems(range) {
        $http.get('./PayItemAdministration/GetAllPayItems', { params: { range: range } }).success(function (result) {
            if (!containsDqeError(result)) {
                $scope.payItems = getDqeData(result);
            }
        });
    }
    function initialize() {
        $scope.payItems = [];
        $scope.submitFactors = function () {
            $http.post('./PayItemAdministration/UpdateFactors', $scope.payItems);
        }
        $scope.$watch('structureRange', function (newValue, oldValue) {
            if (newValue != oldValue) {
                loadPayItems(newValue);
            }
        });
        $scope.structureRange = 0;
        loadPayItems($scope.structureRange);
    }
}]);