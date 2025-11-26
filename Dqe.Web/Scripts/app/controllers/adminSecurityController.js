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
    var systemAdminRole = { name: 'CO Administrator', value: 'A' };
    var payItemAdminRole = { name: 'Pay Item Administrator', value: 'P' };
    var costBasedTemplateAdminRole = { name: 'Cost-Based Template Administrator', value: 'T' };
    //var coderRole = { name: 'Coder', value: 'C' };

    //District Only Roles
    var districtReviewerRole = { name: 'District Reviewer', value: 'R' };
    var districtAdminRole = { name: 'District Administrator', value: 'D' };

    //Both CO and District Roles
    var estimatorRole = { name: 'Estimator', value: 'E' };
    //var maintenanceDistrictAdminRole = { name: 'Maintenance District Admin Role', value: 'F' };
    //var maintenanceEstimatorRole = { name: 'Maintenance Estimator Role', value: 'M' };
    var stateReviewerRole = { name: 'State Reviewer', value: 'B' };
    var adminReadOnlyRole = { name: 'Admin Read-Only', value: 'O' };

    //CoderRole
    var coRoles = [systemAdminRole, payItemAdminRole, costBasedTemplateAdminRole,
        estimatorRole, stateReviewerRole, adminReadOnlyRole]; 

    var districtRoles = [districtAdminRole, estimatorRole];

    //We are only giving co system admin the right to assign any Reviewer roles
   
        securityService.getCurrentUser(function (thisUser) {
            $scope.thisUser = thisUser;
            if ($scope.thisUser.district == "CO") {
                districtRoles.push(districtReviewerRole);
                districtRoles.push(stateReviewerRole);
                districtRoles.push(adminReadOnlyRole);
            }            
        });
        
    

    $scope.sysRoles = function() {
        if ($scope.thisUser == undefined) {
            return emptyRoles();
        } else if ($scope.thisUser.role == 'A' || ( $scope.thisUser.district == 'CO')) {
            if ($scope.district == undefined) {
                return emptyRoles();
            } else if (isCo()) {
                return validCoRoles();
            } else if (isDistrict()) {
                return validDistrictRoles();
            } else {
                return emptyRoles();
            }
        } else if ($scope.thisUser.role == 'D' || $scope.thisUser.role == 'F') {
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
        if ($scope.role != systemAdminRole.value && $scope.role != payItemAdminRole.value && $scope.role != costBasedTemplateAdminRole.value && $scope.role != estimatorRole.value && $scope.role != adminReadOnlyRole.value && $scope.role != stateReviewerRole.value   ) {
            $scope.role = undefined;
        }
        return coRoles;
    }
    function validDistrictRoles() {
        if ($scope.role != districtAdminRole.value && $scope.role != estimatorRole.value && $scope.role != districtReviewerRole.value && $scope.role != stateReviewerRole.value && $scope.role != adminReadOnlyRole.value   ) {
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