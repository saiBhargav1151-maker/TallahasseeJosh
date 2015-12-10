dqeControllers.controller('HomeReportsUnbalancedItemsController', [
    '$scope', '$rootScope', '$http', function($scope, $rootScope, $http) {
        $rootScope.$broadcast('initializeNavigation');

        $scope.reportFormat = {
            type: "PDF",
            sort: "1"
        };

        $scope.showEstimate = {
            flag: "Yes"
        };

        $scope.getProposals = function (val) {
            return $http.get('./report/GetDqeReportProposals', { params: { proposalNumber: val, estimateType: "O" } })
                .then(function(response) {
                    var proposals = [];
                    angular.forEach(response.data, function(item) {
                        proposals.push(item);
                    });
                    return proposals;
                });
        };

        $scope.viewUnbalancedItems = function () {
            var s = document.getElementById("proposalNumber");
            s.value = s.value.trim();
            var proposalNumber = $scope.selectedProposal.number;
            return $http.get('./report/SaveLettingAndVendorDataByProposal', { params: { proposalNumber: proposalNumber } })
                .success(function (result) {
                if (!containsDqeError(result)) {
                    $.download('./report/ViewUnbalancedItemsReport', $('form#ViewUnbalancedItemsReport').serialize());
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
    }
]);