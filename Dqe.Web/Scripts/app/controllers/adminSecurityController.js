dqeControllers.controller('AdminSecurityController', ['$scope', '$location', '$http', '$rootScope', 'securityService', function ($scope, $location, $http, $rootScope, securityService) {
    //user = {id, fullName, district, role, roleAsString, selected}
    $rootScope.$broadcast('initializeNavigation');
    securityService.getCurrentUser(function(thisUser) {
        $scope.thisUser = thisUser;
    });
    $scope.users = [];
    getAllUsers();
    $scope.order = 'fullName';
    $scope.initializeEditForm = function () {
        $scope.selected = undefined;
        $scope.role = undefined;
        $scope.district = undefined;
    }
    $scope.getUsers = function (val) {
        return $http.get('./staff/GetStaffByName', { params: { id: val } })
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
            user.district = $scope.thisUser.role == 2 ? $scope.district : $scope.thisUser.district;
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
    var systemAdminRole = { name: 'System Administrator', value: 2 };
    var districtAdminRole = { name: 'District Administrator', value: 3 };
    var payItemAdminRole = { name: 'Pay Item Administrator', value: 4 };
    var costBasedTemplateAdminRole = { name: 'Cost-Based Template Administrator', value: 5 };
    var estimatorRole = { name: 'Estimator', value: 6 };
    var coRoles = [systemAdminRole, payItemAdminRole, costBasedTemplateAdminRole, estimatorRole];
    var districtRoles = [districtAdminRole, estimatorRole];
    $scope.sysRoles = function() {
        if ($scope.thisUser == undefined) {
            return emptyRoles();
        } else if ($scope.thisUser.role == 2) {
            if ($scope.district == undefined) {
                return emptyRoles();
            } else if (isCo()) {
                return validCoRoles();
            } else if (isDistrict()) {
                return validDistrictRoles();
            } else {
                return emptyRoles();
            }
        } else if ($scope.thisUser.role == 3) {
            return validDistrictRoles();
        } else {
            return emptyRoles();
        }
    };
    function isCo() {
        return $scope.district == 'CO';
    }
    function isDistrict() {
        return ($scope.district.startsWith('D') || $scope.district == 'TP');
    }
    function validCoRoles() {
        if ($scope.role != systemAdminRole.value && $scope.role != payItemAdminRole.value && $scope.role != costBasedTemplateAdminRole.value && $scope.role != estimatorRole.value) {
            $scope.role = undefined;
        }
        return coRoles;
    }
    function validDistrictRoles() {
        if ($scope.role != districtAdminRole.value && $scope.role != estimatorRole.value) {
            $scope.role = undefined;
        }
        return districtRoles;
    }
    function emptyRoles() {
        $scope.role = undefined;
        return [];
    }
    function getAllUsers() {
        $http.get('./securityadministration/GetAllUsers').success(function (result) {
            $scope.users = getDqeData(result);
        });
    }
}]);