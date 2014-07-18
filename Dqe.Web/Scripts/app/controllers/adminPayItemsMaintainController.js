dqeControllers.controller('AdminPayItemsMaintainController', ['$scope', '$rootScope', '$http', function ($scope, $rootScope, $http) {
    $rootScope.$broadcast('initializeNavigation');
    $http.get('./payitemstructureadministration/GetPayItemStructures').success(function (result) {
        var data = getDqeData(result);
        $scope.payItemStructureGroups = [
            {
                heading: '000-099 Items',
                payItemStructures: data.items00
            },
            {
                heading: '100-199 Items',
                payItemStructures: data.items01
            },
            {
                heading: '200-299 Items',
                payItemStructures: data.items02
            },
            {
                heading: '300-399 Items',
                payItemStructures: data.items03
            },
            {
                heading: '400-499 Items',
                payItemStructures: data.items04
            },
            {
                heading: '500-599 Items',
                payItemStructures: data.items05
            },
            {
                heading: '600-699 Items',
                payItemStructures: data.items06
            },
            {
                heading: '700-799 Items',
                payItemStructures: data.items07
            },
            {
                heading: '800-899 Items',
                payItemStructures: data.items08
            },
            {
                heading: '900-999 Items',
                payItemStructures: data.items09
            },
            {
                heading: '1000-9999 Items',
                payItemStructures: data.items10
            }
        ];
    });
}]);