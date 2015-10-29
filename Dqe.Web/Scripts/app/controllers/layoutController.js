dqeControllers.controller('LayoutController', ['$scope', '$rootScope', '$http', '$location', '$cookieStore', '$window', 'securityService', 'navigationService', 'stateService', '$interval', '$cookies', function ($scope, $rootScope, $http, $location, $cookieStore, $window, securityService, navigationService, stateService, $interval, $cookies) {
    var cb = function configureForUser(user) {
        $scope.currentUser = user;
        $scope.subTabs = navigationService.getSubTabs($scope.currentUser);
        $scope.topTabs = navigationService.getTopTabs($scope.currentUser);
        $scope.navs = navigationService.getNavs($scope.currentUser);
        if (!$scope.currentUser.isAuthenticated) {
            if ($location.url().startsWith('/boe') || $location.url().startsWith('/payitems')) {
                //$location.url('/boe');
            } else {
                $location.url('/signin');
            }
        }
    };
    $http.get('./security/GetEnvironment').success(function (result) {
        if (!containsDqeError(result)) {
            var r = getDqeData(result);
            $scope.showEnvironmentWarning = r.showEnvironmentWarning;
            $scope.environment = r.environment;
        }
    });
    $scope.$on('initializeNavigation', function () {
        securityService.getCurrentUser(cb);
    });
    $scope.navigate = function (url) {
        if (url.startsWith('/home_project_prices')) {
            url = stateService.currentProject == '/home_project_prices' ? '' : '/home_project_prices/' + stateService.currentProject;
        }
        else if (url.startsWith('/home_project')) {
            url = stateService.currentProject == '' ? '/home_project' : '/home_project/' + stateService.currentProject;
        }
        else if (url.startsWith('/home_proposal_prices')) {
            url = stateService.currentProposal == '' ? '/home_proposal_prices' : '/home_proposal_prices/' + stateService.currentProposal;
        }
        else if (url.startsWith('/home_proposal')) {
            url = stateService.currentProposal == '' ? '/home_proposal' : '/home_proposal/' + stateService.currentProposal;
        }
        $location.url(url);
    }
    $scope.signout = function () {
        $http.post('./security/SignOut').success(function() {
            $cookieStore.remove('DQE_AUTH_TICKET');
            $location.replace();
            $window.location.href = '.';
        });
        //$location.url('/signin');
    }
    $scope.heartBeat = function() {
        $http.post('./security/GetTimeout').success(function (result) {
            if (!containsDqeError(result)) {
                var r = getDqeData(result);
                if (r.redirect) {
                    $cookieStore.remove('DQE_AUTH_TICKET');
                    $location.replace();
                    $window.location.href = '.';
                } else {
                    $scope.session = {
                        hours: r.hours,
                        minutes: r.minutes
                    }
                }
            }
        }).error(function (result) {
            $interval.cancel(stop);
            $cookieStore.remove('DQE_AUTH_TICKET');
            $location.replace();
            $window.location.href = '.';
        });
    }
    var stop = $interval($scope.heartBeat, 60000);
    $scope.dumpSql = function() {
        $http.post('./security/DumpSql');
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