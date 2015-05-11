dqeControllers.controller('HomeReportsBidToleranceController', ['$scope', '$rootScope', '$http', function ($scope, $rootScope, $http) {
    $rootScope.$broadcast('initializeNavigation');

    $scope.reportFormat = {
        type: "PDF"
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

    $scope.viewBidToleranceReport = function () {
        var lettingNumber = $scope.selectedLetting.number;
        return $http.get('./report/SaveLettingAndVendorDataByLetting', { params: { lettingNumber: lettingNumber } }).then(function(result) {
            if (!containsDqeError(result)) {
                $.download('./report/ViewBidToleranceReport', $('form#ViewBidToleranceReport').serialize());
            }
        });
    };

    jQuery.download = function (url, data, method) {
        //url and data options required
        if (url && data) {
            //data can be string of parameters or array/object
            data = typeof data == 'string' ? data : jQuery.param(data);
            //split params into form inputs
            var inputs = '';
            jQuery.each(data.split('&'), function () {
                var pair = this.split('=');
                inputs += '<input type="hidden" name="' + pair[0] + '" value="' + pair[1] + '" />';
            });
            //send request
            jQuery('<form action="' + url + '" method="' + (method || 'post') + '">' + inputs + '</form>')
            .appendTo('body').submit().remove();
        };
    };
}]);