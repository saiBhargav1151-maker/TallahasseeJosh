dqeControllers.controller('HomeReportsUnbalancedItemsController', [
    '$scope', '$rootScope', '$http', function($scope, $rootScope, $http) {
        $rootScope.$broadcast('initializeNavigation');
        $scope.getProposals = function(val) {
            return $http.get('./report/GetDqeReportProposals', { params: { proposalNumber: val, estimateType: 2 } })
                .then(function(response) {
                    var proposals = [];
                    angular.forEach(response.data, function(item) {
                        proposals.push(item);
                    });
                    return proposals;
                });
        };

        $scope.viewUnbalancedItems = function() {
            var proposalNumber = $scope.selectedProposal.number;
            return $http.get('./report/SaveLettingAndVendorDataByProposal', { params: { proposalNumber: proposalNumber } })
                .then(function(response) {
                    $.download('./report/ViewUnbalancedItemsReport', $('form#ViewUnbalancedItemsReport').serialize());
                });
        };
    }
]);