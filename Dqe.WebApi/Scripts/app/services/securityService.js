dqeServices.factory('securityService', ['$resource', function ($resource) {
    return {
        roles: [
            { name: 'admin' },
            { name: 'estimator' },
            { name: 'read' }
        ],
        getMainNavigation : function(scope) {
            $resource('./api/navigation/:id').query({ id: scope.role }, function (data) {
                if (data.length > 0) {
                    scope.menuItems = data.splice(0, data.length - 1);
                    scope.signInMenuItem = data[data.length - 1];
                }
            });
        }
    };
}]);