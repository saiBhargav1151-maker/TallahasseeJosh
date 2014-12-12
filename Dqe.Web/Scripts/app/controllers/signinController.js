dqeControllers.controller('SigninController', ['$scope', '$http', '$location', 'securityService', function ($scope, $http, $location, securityService) {
    $scope.initializeEditForm = function () {
        $scope.selected = undefined;
        $scope.role = undefined;
        $scope.district = undefined;
    }
    $scope.getUsers = function (val) {
        return $http.get('./staff/GetStaffByName', { params: { id: val } })
            .then(function (response) {
                var users = [];
                angular.forEach(response.data, function (item) {
                    users.push(item);
                });
                return users;
            });
    }
    $scope.postUser = function () {
        if ($scope.selected != undefined) {
            var user = {};
            user.id = $scope.selected.id;
            user.role = $scope.role;
            user.district = $scope.district;
            securityService.impersonateUser(user, function () {
                securityService.getCurrentUser(function(u) {
                    if (u.role == 2 || u.role == 3 || u.role == 6) {
                        $location.url('/home_project');
                    }else if (u.role == 4) {
                        $location.url('/admin_payitems_maintain');
                    } else if (u.role == 5) {
                        $location.url('/admin_costbasedtemplates');
                    } else {
                        $location.url('/signin');
                    }
                });
                
            });
        }
    }
    $scope.isSubmitUserDisabled = function () {
        return ($scope.selected == undefined || $scope.role == undefined || $scope.district == undefined);
    }
    $scope.setDistrict = function () {
        $scope.district = $scope.selected.district;
    }
}]);