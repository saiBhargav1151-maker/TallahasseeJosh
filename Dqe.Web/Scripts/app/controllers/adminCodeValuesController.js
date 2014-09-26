dqeControllers.controller('AdminCodeValuesController', ['$scope', '$rootScope', '$http', function ($scope, $rootScope, $http) {
    $rootScope.$broadcast('initializeNavigation');
    $scope.codeType = '--';
    $scope.codes = [];
    $scope.getCodes = function () {
        if ($scope.codeType != undefined && $scope.codeType != '--') {
            $http.get('./codeadministration/GetCodes', { params: { codeType: $scope.codeType } }).success(function (result) {
                $scope.codes = [];
                $scope.codes = getDqeData(result);
            });
        }
    };
    $scope.newCode = {id: 0,  name: '', isActive: true };
    $scope.updateCodes = function () {
        if ($scope.codes.length > 0 || $scope.newCode.name != '') {
            var arr = $scope.codes.slice();
            if ($scope.newCode.name != '') arr.push($scope.newCode);
            var codeSet = { codeType: $scope.codeType, codes: arr };
            $http.post('./codeadministration/UpdateCodes', codeSet).success(function (result) {
                if (!containsDqeError(result)) {
                    $scope.codes = getDqeData(result);
                    $scope.newCode = { id: 0, name: '', isActive: true };
                }
            });
        }
    }
}]);