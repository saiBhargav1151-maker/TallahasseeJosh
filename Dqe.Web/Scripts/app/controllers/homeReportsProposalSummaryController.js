dqeControllers.controller('HomeReportsProposalSummaryController', ['$scope', '$rootScope', '$http', function ($scope, $rootScope, $http) {
    $rootScope.$broadcast('initializeNavigation');

    $scope.reportFormat = {
        type: "PDF"
    };

    $scope.getProposals = function (val) {
        var estimateType = $scope.order;
        return $http.get('./projectproposal/GetDqeProposals', { params: { proposalNumber: val, estimateType: estimateType } })
            .then(function (response) {
                var proposals = [];
                angular.forEach(response.data, function (item) {
                    proposals.push(item);
                });
                return proposals;
            });
    };

    $scope.viewProposalSummary = function () {
        $.download('./report/ViewProposalSummaryReport', $('form#ViewProposalSummaryReport').serialize());
    }

    $scope.estimateTypeChange = function () {
        $scope.selectedProposal = null;
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