dqeControllers.controller('AdminWebLinksController', ['$scope', '$rootScope', '$http', function ($scope, $rootScope, $http) {
    $rootScope.$broadcast('initializeNavigation');
    $scope.linkType = '--';
    $scope.links = [];
    $scope.getLinks = function () {
        if ($scope.linkType != undefined && $scope.linkType != '--') {
            $http.get('./weblinkadministration/GetWebLinks', { params: { linkType: $scope.linkType } }).success(function (result) {
                $scope.links = [];
                $scope.links = getDqeData(result);
            });
        }
    };
    $scope.newLink = {id: 0,  name: '', webLink: '' };
    $scope.updateLinks = function() {
        if ($scope.links.length > 0 || $scope.newLink.name != '' && $scope.newLink.webLink != '') {
            var arr = $scope.links.slice();
            if ($scope.newLink.name != '' && $scope.newLink.webLink != '') arr.push($scope.newLink);
            var linkSet = { linkType: $scope.linkType, links: arr };
            $http.post('./weblinkadministration/UpdateWebLinks', linkSet).success(function(result) {
                if (!containsDqeError(result)) {
                    $scope.links = getDqeData(result);
                    $scope.newLink = { id: 0, name: '', webLink: '' };
                }
            });
        }
    };
    $scope.showConfirmRemoval = false;
    $scope.isRemoveLinkDisabled = function() {
        for (var i = 0; i < $scope.links.length; i++) {
            if ($scope.links[i].selected == true) return false;
        }
        $scope.showConfirmRemoval = false;
        return true;
    };
    $scope.removeSelectedLinks = function() {
        $http.post('./weblinkadministration/RemoveLinks', $scope.links).success(function() {
            $scope.getLinks();
        });
    };
}]);