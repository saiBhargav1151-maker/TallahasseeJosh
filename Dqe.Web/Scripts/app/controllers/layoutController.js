dqeControllers.controller('LayoutController', ['$scope', '$location', '$cookieStore', '$window', 'securityService', 'navigationService', function ($scope, $location, $cookieStore, $window, securityService, navigationService) {
    var cb = function configureForUser(user) {
        $scope.currentUser = user;
        $scope.navs = navigationService.getNavs($scope.currentUser);
        $scope.topTabs = navigationService.getTopTabs($scope.currentUser);
        $scope.subTabs = navigationService.getSubTabs($scope.currentUser);
        if (!$scope.currentUser.isAuthenticated) $location.url('/signin');
    };
    $scope.$on('initializeNavigation', function () {
        securityService.getCurrentUser(cb);
    });
    $scope.navigate = function(url) {
        $location.url(url);
    }
    $scope.signout = function () {
        $cookieStore.remove('DQE_AUTH_TICKET');
        $window.location.href = './';
    }
}]);