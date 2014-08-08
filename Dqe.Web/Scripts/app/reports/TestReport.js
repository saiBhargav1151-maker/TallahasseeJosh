//instance application
var dqeApp = angular.module('dqeApp', [
    'ui.bootstrap'
]).controller('TestReport',['$scope',function($scope) {
    $scope.TestValue = "Test 1";
}]);