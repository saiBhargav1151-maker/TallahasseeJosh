dqeControllers.controller('HomeReportsSummaryLettingController', ['$scope', '$rootScope', '$http', function ($scope, $rootScope, $http) {
    $rootScope.$broadcast('initializeNavigation');

    $scope.viewSummaryOfLettingReport = function () {
        var lettingNumber = $scope.selectedLetting.number;
        return $http.get('./report/SaveLettingAndVendorDataByLetting', { params: { lettingNumber: lettingNumber } })
            .then(function (response) {
                $.download('./report/ViewSummaryOfLettingReport', $('form#ViewSummaryOfLettingReport').serialize());
            });
    };

    $scope.getLettings = function (val) {
        return $http.get('./report/GetLettings', { params: { lettingNumber: val } })
            .then(function (response) {
                var lettings = [];
                angular.forEach(response.data, function (item) {
                    lettings.push(item);
                });
                return lettings;
            });
    };
}]);