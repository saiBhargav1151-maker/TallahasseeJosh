dqeControllers.controller('LayoutController', ['$scope', '$rootScope', '$location', '$cookieStore', '$window', 'securityService', 'navigationService', 'stateService', function ($scope, $rootScope, $location, $cookieStore, $window, securityService, navigationService, stateService) {
    var cb = function configureForUser(user) {
        $scope.currentUser = user;
        $scope.subTabs = navigationService.getSubTabs($scope.currentUser);
        $scope.topTabs = navigationService.getTopTabs($scope.currentUser);
        $scope.navs = navigationService.getNavs($scope.currentUser);
        if (!$scope.currentUser.isAuthenticated) {
            if ($location.url().startsWith('/boe')) {
                //$location.url('/boe');
            } else {
                $location.url('/signin');
            }
        }
    };
    $scope.$on('initializeNavigation', function () {
        securityService.getCurrentUser(cb);
    });
    $scope.navigate = function(url) {
        $location.url(url);
    }
    $scope.signout = function () {
        $cookieStore.remove('DQE_AUTH_TICKET');
        $window.location.href = '.';
    }
    //signalR messaging implementation
    //$scope.showStatusMessage = false;
    //$scope.hideStatusMessage = function() {
    //    $scope.showStatusMessage = false;
    //}
    //$scope.deferredTaskHub = $.connection.deferredTaskHub;
    //$scope.deferredTaskHub.client.showMessage = function (task, message) {
    //    if (message != undefined && message.length > 0) {
    //        $scope.statusMessageText = message;
    //        $scope.showStatusMessage = true;
    //    }
    //    $rootScope.$apply();
    //};
    //$.connection.hub.start().done(function () {
    //    //$scope.deferredTaskHub.server.sendMessage('GlobalTask', 'Deferred task messages will display here.  You can dismiss the message by clicking the X on the right.');
    //});
}]);