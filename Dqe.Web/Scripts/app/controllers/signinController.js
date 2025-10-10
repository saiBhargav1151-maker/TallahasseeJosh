dqeControllers.controller('SigninController', ['$scope', '$rootScope', '$http', '$location', 'securityService', function ($scope, $rootScope, $http, $location, securityService) {
    $rootScope.$broadcast('initializeNavigation');
    securityService.canImpersonate(null, function(result) {
        if (!containsDqeError(result)) {
            var data = getDqeData(result);
            $scope.canImpersonate = data.canImpersonate;
        }
    });
    $scope.impersonate = false;
    $scope.showImpersonation = function() {
        $scope.impersonate = true;
    }
    $scope.showLogin = function () {
        $scope.impersonate = false;
    }
    $scope.initializeEditForm = function () {
        $scope.impersonateUser = {
            selected: '',
            role: '',
            district: ''
        }
        //$scope.selected = undefined;
        //$scope.role = undefined;
        //$scope.district = undefined;
    }
    $scope.initializeEditForm();
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
        if ($scope.impersonateUser.selected != '') {
            var user = {};
            user.id = $scope.impersonateUser.selected.id;
            user.role = $scope.impersonateUser.role;
            user.district = $scope.impersonateUser.district;
            securityService.impersonateUser(user, function () {
                securityService.getCurrentUser(function(u) {
                    if (u.role == 'A'
                        || u.role == 'D'
                        || u.role == 'E'
                        || u.role == 'R'
                        || u.role == 'B'
                        || u.role == 'C'
                        || u.role == 'F'
                        || u.role == 'M'
                        || u.role == 'O'
                    ) {
                        $location.url('/home_estimates');
                    }else if (u.role == 'P') {
                        $location.url('/admin_payitems_maintain');
                    } else if (u.role == 'T') {
                        $location.url('/admin_costbasedtemplates');
                    } else if (u.role == 'R') {
                        $location.url('/home_estimates');
                    } else {
                        $location.url('/signin');
                    }
                });
            });
        }
    }
    $scope.isSubmitUserDisabled = function () {
        return ($scope.impersonateUser.selected == '' || $scope.impersonateUser.role == '' || $scope.impersonateUser.district == '');
    }
    $scope.setDistrict = function () {
        $scope.impersonateUser.district = $scope.impersonateUser.selected.district;
    }
    $scope.canAttemptLogin = function (userName, password) {
        if (userName == undefined || password == undefined) return false;
        if (userName != '' && password != '') return true;
        return false;
    }
    $scope.login = function (userName, password, isChangePassword, newPassword, confirmPassword) {
        var user = {};
        user.id = userName;
        user.password = password;
        user.isChangePassword = isChangePassword == undefined ? false : isChangePassword;
        user.newPassword = newPassword == undefined ? '' : newPassword;
        user.confirmPassword = confirmPassword == undefined ? '' : confirmPassword;
        securityService.authenticateUser(user, function () {
            securityService.getCurrentUser(function (u) {
                if (u.role == 'A' || u.role == 'D' || u.role == 'E' || u.role == 'R' || u.role == 'B' || u.role == 'C' || u.role == 'F' ||  u.role == 'M' || u.role == 'O' ) {
                    $location.url('/home_estimates');
                } else if (u.role == 'P') {
                    $location.url('/admin_payitems_maintain');
                } else if (u.role == 'T') {
                    $location.url('/admin_costbasedtemplates');
                } else {
                    $location.url('/signin');
                }
            });
        });
    }
}]);