dqeControllers.controller('DefaultValuesController', ['$scope', '$rootScope', '$http',function ($scope, $rootScope,$http) {
    $rootScope.$broadcast('initializeNavigation');

    $scope.defaultPricingParameter = {};

    var loadDefaultPricingParameter = function() {
        $http.get('./DefaultPricingParameter/GetUsersDefaultPricingParameter').success(function(result) {
            if (result != null && result != "") {
                $scope.defaultPricingParameter = result;
            }
        });
    };

    loadDefaultPricingParameter();

    $scope.save = function() {
        $http.post('./DefaultPricingParameter/Save', $scope.defaultPricingParameter).success(function (result) {
            $scope.defaultPricingParameter = result.data;
        });
    };

    $scope.isSubmitDisabled = function() {
        if ($scope.defaultPricingParameter.contractType != null &&
            $scope.defaultPricingParameter.quantities != null &&
            $scope.defaultPricingParameter.pricingModel != null &&
            $scope.defaultPricingParameter.bidders != null) {
            return false;
        }

        return true;
    };
}]);