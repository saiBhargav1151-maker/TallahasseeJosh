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
        $scope.costGroupAuthorization = "N";
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
            user.costGroupAuthorization = $scope.costGroupAuthorization;
            user.district = $scope.thisUser.role == 'A' ? $scope.district : $scope.thisUser.district;
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
        $scope.costGroupAuthorization = user.costGroupAuthorization;
    }
    $scope.showConfirmRemoval = false;
    $scope.removeSelectedUsers = function () {
        $http.post('./securityadministration/RemoveUsers', $scope.users).success(function () {
            getAllUsers();
            $scope.initializeEditForm();
        });
    }
    $scope.initializeEditForm();
    //CO only Roles
    var systemAdminRole = { name: 'System Administrator', value: 'A' };
    var payItemAdminRole = { name: 'Pay Item Administrator', value: 'P' };
    var costBasedTemplateAdminRole = { name: 'Cost-Based Template Administrator', value: 'T' };
    var CoderRole = { name: 'Coder', value: 'C' };
    var AdminReadOnlyRole = { name: 'Admin Read Only', value: 'O' };

    //District Only Roles
    var DistrictReviewerRole = { name: 'District Reviewer', value: 'R' };
    var districtAdminRole = { name: 'District Administrator', value: 'D' };

    //Both CO and District Roles
    var MaintenanceDistrictAdminRole = { name: 'Maintenance District Admin Role', value: '2' };
    var MaintenanceEstimatorRole = { name: 'Maintenance Estimator Role', value: 'M' };
    var StateReviewerRole = { name: 'State Reviewer', value: '1' };
    var estimatorRole = { name: 'Estimator', value: 'E' };

    //CoderRole
    var coRoles = [systemAdminRole, payItemAdminRole, costBasedTemplateAdminRole,
        estimatorRole, StateReviewerRole, MaintenanceDistrictAdminRole, MaintenanceEstimatorRole, AdminReadOnlyRole]; 

    var districtRoles = [districtAdminRole, estimatorRole];

    //, DistrictReviewerRole, StateReviewerRole, MaintenanceDistrictAdminRole, MaintenanceEstimatorRole


    $scope.sysRoles = function() {
        if ($scope.thisUser == undefined) {
            return emptyRoles();
        } else if ($scope.thisUser.role == 'A' || ($scope.thisUser.role == '2' && $scope.thisUser.district == 'CO')) {
            if ($scope.district == undefined) {
                return emptyRoles();
            } else if (isCo()) {
                return validCoRoles();
            } else if (isDistrict()) {
                return validDistrictRoles();
            } else {
                return emptyRoles();
            }
        } else if ($scope.thisUser.role == 'D' || $scope.thisUser.role == '2') {
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
        if ($scope.role != systemAdminRole.value && $scope.role != payItemAdminRole.value && $scope.role != costBasedTemplateAdminRole.value && $scope.role != estimatorRole.value && $scope.role != CoderRole.value && $scope.role != AdminReadOnlyRole.value
            && $scope.role != MaintenanceDistrictAdminRole.value && $scope.role != MaintenanceEstimatorRole.value && $scope.role != StateReviewerRole.value  ) {
            $scope.role = undefined;
        }
        return coRoles;
    }
    function validDistrictRoles() {
        if ($scope.role != districtAdminRole.value && $scope.role != estimatorRole.value && $scope.role != DistrictReviewerRole.value
            && $scope.role != MaintenanceDistrictAdminRole.value && $scope.role != MaintenanceEstimatorRole.value && $scope.role != StateReviewerRole.value  ) {
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