dqeControllers.controller('ProfileProjectsController', ['$scope', '$rootScope', '$http', '$filter', function ($scope, $rootScope, $http, $filter) {
    $rootScope.$broadcast('initializeNavigation');

    $scope.filterItems = function () {
        $scope.currentPage = 0;
        $scope.filteredItems = $filter('filter')($scope.projects,
        {
            id: $scope.filter.id,
            number: $scope.filter.number,
            proposal: $scope.filter.proposal
        });
    };

    $scope.clearAllFilters = function () {
        $scope.filteredItems = $filter('filter')($scope.projects,
        {
            id: "",
            number: "",
            proposal: ""
        });
        $scope.filter.number = '';
        $scope.filter.proposal = '';
        $scope.filterItems();
    };

    $scope.sortItems = function (sort) {
        var sortArray = [];
        sortArray.push(sort);
        if ($scope.lastItemSort === sort) {
            $scope.sortItemReverse = !$scope.sortItemReverse;
        } else {
            $scope.lastItemSort = sort;
            $scope.sortItemReverse = false;
        }
        $scope.filteredItems = $filter('orderBy')($scope.filteredItems, sortArray, $scope.sortItemReverse);
    }

    $http.get('./profile/GetRecentProjects').success(function (result) {
        if (!containsDqeError(result)) {
            $scope.projects = getDqeData(result);

            $scope.filter = {
                id: "",
                number: "",
                proposal: ""
            };
            $scope.filterItems();
        }
    });
}]);