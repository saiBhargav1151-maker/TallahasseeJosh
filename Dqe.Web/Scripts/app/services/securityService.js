dqeServices.factory('securityService', ['$http', function ($http) {
    return {
        getCurrentUser: function(cb) {
            return $http.get('./security/GetCurrentUser').success(function(user) {
                cb(user);
            });
        },
        impersonateUser: function(user, cb) {
            $http.post('./security/ImpersonateUser', user).success(function() {
                cb();
            });
        },
        authenticateUser: function(user, cb) {
            $http.post('./security/AuthenticateUser', user).success(function () {
                cb();
            });
        },
        canImpersonate: function(user, cb) {
            $http.post('./security/CanImpersonate', user).success(function (result) {
                cb(result);
            });
        }
    };
}]);