dqeDirectives.directive('pricingParameter', function () {
    return {
        restrict: 'E',
        scope: {
            pricingParameter: '=bind'
        },
        templateUrl: './Views/directives/pricing-parameter.html',
        controller: function ($scope, $http) {
            var getNewPricingParameter = function () {
                return {
                    months: 3,
                    contractType: null,
                    quantities: null,
                    workTypes: [],
                    pricingModel: null,
                    bidders: null,
                    id:0
                };
            };

            $scope.pricingParameter = getNewPricingParameter();

            $scope.toggleSelection = function toggleSelection(value) {
                var idx = $scope.pricingParameter.workTypes.indexOf(value);

                if (idx > -1) {
                    $scope.pricingParameter.workTypes.splice(idx, 1);
                }
                else {
                    $scope.pricingParameter.workTypes.push(value);
                }
            };
        }
    }
});