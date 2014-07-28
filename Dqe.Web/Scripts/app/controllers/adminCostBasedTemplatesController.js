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
            $scope.templates = [];
            angular.forEach(result, function (item) {
                $scope.templates.push(item);
            });
        });
    };

    $scope.resetForm = function() {
        $scope.currentTemplate = getNewTemplate();
        document.forms["CostTemplateForm"].reset();
    }

    getAllCostTemplates();

    $scope.editTemplate = function (template) {
        $scope.currentTemplate = template;
    };

    var successUploadFile = function (result) {
        if (result.data == null)
            return;

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
        return "./CostBasedTemplateAdministration/DownloadTemplate/" + template.documentId;
    };

    //$scope.removeSelectedTemplates = function () {
    //    var toBeRemoved = $scope.templates.filter(function(template) {
    //        return template.selected != undefined && template.selected == true;
    //    });

    //    $http.post("./CostBasedTemplateAdministration/RemoveCostBasedTemplates", toBeRemoved).success(function () {
    //        $scope.showConfirmRemoval = false;
    //        getAllCostTemplates();
    //    });
    //};
}]);