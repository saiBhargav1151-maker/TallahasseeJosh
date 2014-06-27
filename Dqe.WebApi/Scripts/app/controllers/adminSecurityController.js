dqeControllers.controller('AdminSecurityController', ['$scope', '$location', '$resource', '$http', '$rootScope', function ($scope, $location, $resource, $http, $rootScope) {
    //user = {id, fullName, district, role, roleAsString, selected}
    $rootScope.$broadcast('initializeNavigation');
    $scope.users = [];
    getAllUsers();
    $scope.order = 'FullName';
    $scope.initializeEditForm = function () {
        $scope.selected = undefined;
        $scope.role = undefined;
        $scope.district = undefined;
    }
    $scope.getUsers = function(val) {
        return $http.get('./api/staff', { params: { id: val } })
            .then(function (response) {
            var users = [];
            angular.forEach(response.data, function(item) {
                users.push(item);
            });
            return users;
        });
    }
    $scope.postUser = function() {
        if ($scope.selected != undefined) {
            var user = {};
            user.id = $scope.selected.id;
            user.role = $scope.role;
            user.district = $scope.district;
            $http.post('./securityadministration/UpdateUser', user).success(function () {
                getAllUsers();
                $scope.initializeEditForm();
            });
        }
    }
    $scope.isRemoveUserDisabled = function () {
        for (var i = 0; i < $scope.users.length; i++) {
            if ($scope.users[i].selected == true) return false;
        }
        $scope.showConfirmRemoval = false;
        return true;
    }
    $scope.isSubmitUserDisabled = function() {
        return ($scope.selected == undefined || $scope.role == undefined || $scope.district == undefined);
    }
    $scope.setDistrict = function() {
        $scope.district = $scope.selected.district;
    }
    $scope.editUser = function(user) {
        $scope.selected = user;
        $scope.role = user.role;
        $scope.district = user.district;
    }
    $scope.showConfirmRemoval = false;
    $scope.removeSelectedUsers = function () {
        $http.post('./securityadministration/RemoveUsers', $scope.users).success(function () {
            getAllUsers();
            $scope.initializeEditForm();
        });
    }
    $scope.initializeEditForm();
    function getAllUsers() {
        $http.get('./securityadministration/GetAllUsers').success(function (data) {
            $scope.users = data;
        });
    }
}]);