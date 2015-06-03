dqeControllers.controller('AdminCostBasedTemplatesController', ['$scope', '$rootScope', '$http', 'fileUpload', function ($scope, $rootScope, $http, fileUpload) {
    "use strict";
    $rootScope.$broadcast('initializeNavigation');
    var getNewTemplate = function () {
        return {
            resultCell: '',
            name: '',
            file: '',
            id: 0,
            selected : false
        };
    }
    $scope.currentTemplate = getNewTemplate();
    $scope.templates = [];
    var getAllCostTemplates = function () {
        $http.get('./CostBasedTemplateAdministration/GetAll').success(function (result) {
            if (!containsDqeError(result)) {
                var data = getDqeData(result);
                $scope.templates = [];
                angular.forEach(data, function (item) {
                    $scope.templates.push(item);
                });
            }
        });
    };
    $scope.resetForm = function() {
        $scope.currentTemplate = getNewTemplate();
        document.forms["CostTemplateForm"].reset();
    }
    $scope.isSaveDisabled = function() {
        if ($scope.currentTemplate.id == 0) {
            if ($scope.currentTemplate.name == '') return true;
            if ($scope.currentTemplate.resultCell == '') return true;
            if ($scope.currentTemplate.file == '') return true;
        } else {
            if ($scope.currentTemplate.name == '') return true;
            if ($scope.currentTemplate.resultCell == '') return true;
        }
        return false;
    }
    getAllCostTemplates();
    $scope.editTemplate = function (template) {
        $scope.currentTemplate = template;
    };
    var successUploadFile = function (result) {
        if (result.data == null) return;
        if ($scope.currentTemplate.id == 0) {
            $scope.templates.push(result.data);
        }
        $scope.resetForm();
    };
    $scope.upload = function () {
        var uploadUrl = "./CostBasedTemplateAdministration/SaveCostBasedTemplate";

        var fd = new FormData();
        fd.append("name", $scope.currentTemplate.name);
        fd.append("resultCell", $scope.currentTemplate.resultCell);
        fd.append("id", $scope.currentTemplate.id);

        fileUpload.uploadFileToUrl($scope.currentTemplate.file, uploadUrl, fd, successUploadFile);
    };
    $scope.downloadUrl = function(template) {
        return "./CostBasedTemplateAdministration/DownloadTemplate/" + template.id;
    };
    $scope.removeTemplate = function(template) {
        $http.post('./CostBasedTemplateAdministration/RemoveCostBasedTemplates', template).success(function (result) {
            if (!containsDqeError(result)) {
                for (var i = 0; i < $scope.templates.length; i++) {
                    if ($scope.templates[i].id == template.id) {
                        $scope.templates.splice(i, 1);
                    }   
                }
            }
        });
    }
}]);