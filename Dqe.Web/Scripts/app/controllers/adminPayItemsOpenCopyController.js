dqeControllers.controller('AdminPayItemsOpenCopyController', ['$scope', '$rootScope', '$http', function ($scope, $rootScope, $http) {
    $rootScope.$broadcast('initializeNavigation');
    $scope.masterFiles = [];
    loadMasterFiles();
    $scope.addMasterFile = function () {
        var o = {
            copy: $scope.fileNumberCopy == undefined ? 0 : $scope.fileNumberCopy.id,
            add: $scope.fileNumberNew == undefined ? 0 : $scope.fileNumberNew
        }
        $http.post('./masterfileadministration/AddMasterFile', o).success(function () {
            loadMasterFiles();
        });
    }
    function loadMasterFiles() {
        $http.get('./masterfileadministration/GetMasterFiles').success(function (result) {
            if (!containsDqeError(result)) {
                var data = getDqeData(result);
                if (data.length > 0) {
                    $scope.masterFiles = data;
                }
            }
        });
    }
}]);