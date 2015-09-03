dqeDirectives.directive('fileModel', ['$parse', function ($parse) {
    return {
        restrict: 'A',
        link: function (scope, element, attrs) {
            var model = $parse(attrs.fileModel);
            var modelSetter = model.assign;

            element.bind('change', function () {
                scope.$apply(function () {
                    modelSetter(scope, element[0].files[0]);
                });
            });
        }
    };
}]);

dqeServices.service('fileUpload', ['$http', function ($http) {
    this.uploadFileToUrl = function (file, uploadUrl,formData,success) {
        if (formData != null) {
            fd = formData;
        } else {
            var fd = new FormData();
        }
        fd.append('file', file);
        $http.post(uploadUrl, fd, {
            transformRequest: angular.identity,
            headers: { 'Content-Type': undefined }
        })
        .success(function (result) {
            if (success != undefined) {
                success(result);
            }
        })
        .error(function () {
        });
    }
}]);