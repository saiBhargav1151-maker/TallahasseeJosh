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
        }
    };
}]);