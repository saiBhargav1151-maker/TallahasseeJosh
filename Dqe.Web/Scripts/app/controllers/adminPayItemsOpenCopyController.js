dqeControllers.controller('AdminPayItemsOpenCopyController', ['$scope', '$rootScope', '$http', function ($scope, $rootScope, $http) {
    $rootScope.$broadcast('initializeNavigation');

    $scope.masterFile = {
        copyMasterFile: false,
        validDateOpened: false,
        specBook: "",
        effectiveDate: "",
        id: ""
    };

    $scope.createNewMasterFile = {
        shouldCreate: false
    };

    $scope.changeMasterFileNumber = function () {
        $scope.masterFile.specBook = $scope.masterFile.effectiveDate.getFullYear().toString().substring(2);
    };

    $http.get('./PayItemStructureAdministration/GetCurrentSpecBook').success(function (result) {
        if (!containsDqeError(result)) {
            var currentSpecBook = getDqeData(result);
            $scope.masterFile.id = currentSpecBook.id;
            $scope.masterFile.copyMasterFile = currentSpecBook.copyMasterFile;
            if (currentSpecBook.effectiveDate != null) {
                var myDate = new Date(parseInt(currentSpecBook.effectiveDate.substr(6)));
                $scope.masterFile.effectiveDate = myDate.getMonth() + "/" + myDate.getDate() + "/" + myDate.getFullYear();
            }


            $scope.createNewMasterFile.shouldCreate = currentSpecBook.copyMasterFile;
        }
    });

    $scope.validDateOpen = function ($event, item) {
        $event.preventDefault();
        $event.stopPropagation();
        item.validDateOpened = true;
    };

    $scope.saveMasterFile = function (masterFile) {
        masterFile.copyMasterFile = true;
        $http.post('./masterfileadministration/UpdateMasterFile', masterFile).success(function (result) {
            if (!containsDqeError(result)) {
                $scope.masterFile.effectiveDate = $scope.masterFile.effectiveDate.getMonth() + "/" +
                                                  $scope.masterFile.effectiveDate.getDate() + "/" +
                                                  $scope.masterFile.effectiveDate.getFullYear();
                $scope.createNewMasterFile.shouldCreate = true;
            }
        });
    };

    $scope.cancelMasterFile = function(masterFile) {
        $http.post('./masterfileadministration/CancelMasterFile', masterFile).success(function (result) {
            if (!containsDqeError(result)) {
                $scope.masterFile.effectiveDate = "";
                $scope.masterFile.specBook = "";
                $scope.createNewMasterFile.shouldCreate = false;
            }
        });
    };
}]);