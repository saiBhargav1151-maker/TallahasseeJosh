dqeControllers.controller('LayoutController', ['$scope', '$location', 'navigationService', function ($scope, $location, navigationService) {
    $scope.$on('initializeNavigation', function () {
        $scope.navs = navigationService.getNavs();
        $scope.topTabs = navigationService.getTopTabs();
        $scope.subTabs = navigationService.getSubTabs();
    });
    $scope.navigate = function(url) {
        $location.url(url);
    }
}]);