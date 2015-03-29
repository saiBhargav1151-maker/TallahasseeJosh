dqeControllers.controller('HomeBoeController', ['$scope', '$rootScope', '$http', '$route', '$filter', '$location', function ($scope, $rootScope, $http, $route, $filter, $location) {
    $rootScope.$broadcast('initializeNavigation');
    if ($route.current.params != 'undefined' && $route.current.params != null) {
        if ($route.current.params.index != 'undefined' && $route.current.params.index != null) {
            var arr = $route.current.params.index.split('-');
            if (arr.length == 2) {
                var specYear = arr[0];
                var itemName = arr[1];
                $http.get('./PayItemStructureAdministration/GetStructureForItem', { params: { specYear: specYear, itemName: itemName } }).success(function (result) {
                    if (!containsDqeError(result)) {
                        $scope.structures = getDqeData(result);
                        $scope.showList = true;
                        $scope.showChapters = false;
                        $scope.filter = {
                            name: '',
                            title: '',
                            unit: ''
                        };
                        $scope.filterItems();
                    }
                });
            } else {
                var initialSet = ' 1';
                if ($route.current.params.index == '2') {
                    initialSet = ' 2';
                }
                if ($route.current.params.index == '3') {
                    initialSet = ' 3';
                }
                if ($route.current.params.index == '4') {
                    initialSet = ' 4';
                }
                if ($route.current.params.index == '5') {
                    initialSet = ' 5';
                }
                if ($route.current.params.index == '6') {
                    initialSet = ' 6';
                }
                if ($route.current.params.index == '7') {
                    initialSet = ' 7';
                }
                if ($route.current.params.index == '8') {
                    initialSet = ' 8';
                }
                if ($route.current.params.index == '9') {
                    initialSet = ' 9';
                }
                if ($route.current.params.index == '10') {
                    initialSet = ' 10';
                }
                $http.get('./PayItemStructureAdministration/GetStructuresRange', { params: { set: initialSet } }).success(function (result) {
                    if (!containsDqeError(result)) {
                        $scope.structures = getDqeData(result);
                        $scope.showList = true;
                        $scope.showChapters = false;
                        $scope.filter = {
                            name: '',
                            title: '',
                            unit: ''
                        };
                        $scope.filterItems();
                    }
                });
            }
        } else {
            $scope.showChapters = true;
        }
    }
    $scope.listItems = function (set) {
        $scope.filteredItems = undefined;
        $http.get('./PayItemStructureAdministration/GetStructuresRange', {params: { set: set }}).success(function (result) {
            if (!containsDqeError(result)) {
                $scope.structures = getDqeData(result);
                $scope.filter = {
                    name: '',
                    title: '',
                    unit: ''
                };
                $scope.filterItems();
            }
        });
        $scope.showList = true;
        $scope.showChapters = false;
    }
    $scope.navigate = function() {
        $location.url('/boe');
        $scope.showChapters = true;
        $scope.showList = false;
    }
    $scope.toggleStructureDetail = function (structure) {
        if (structure.showDetail) {
            structure.showDetail = false;
            return;
        }
        $http.get('./PayItemStructureAdministration/GetStructureDetail', { params: { structureId: structure.id } }).success(function (result) {
            if (!containsDqeError(result)) {
                structure = getDqeData(result);
                $scope.getItemsForStructure(structure);
                for (var i = 0; i < $scope.filteredItems.length; i++) {
                    if ($scope.filteredItems[i].id == structure.id) {
                        $scope.filteredItems[i] = structure;
                        structure.showDetail = true;
                        break;
                    }
                }
            }
        });
    }
    $scope.filterItems = function () {
        $scope.currentPage = 0;
        $scope.filteredItems = $filter('filter')($scope.structures,
        {
            name: $scope.filter.name,
            title: $scope.filter.title,
            unit: $scope.filter.unit
            //primaryUnit: $scope.filter.unit.split('/')[0],
            //hybridUnit: $scope.filter.unit.split('/').length == 2 ? $scope.filter.unit.split('/')[1] : '',
        });
    }
    $scope.clearAllFilters = function () {
        $scope.filteredItems = $filter('filter')($scope.structures,
        {
            name: '',
            title: '',
            unit: ''
        });
        $scope.filter.name = '';
        $scope.filter.title = '';
        $scope.filter.unit = '';
        $scope.filterItems();
    }
    $scope.getItemsForStructure = function (structure) {
        $http.get('./PayItemStructureAdministration/GetItemHeadersForStructure', { params: { structureId: structure.id, currentItemsOnly: true } }).success(function (result) {
            if (!containsDqeError(result)) {
                structure.items = getDqeData(result);
                var unit = '';
                for (var i = 0; i < structure.items.length; i++) {
                    if (structure.items[i].isObsolete) continue;
                    if (unit == '') {
                        unit = structure.items[i].unit;
                    } else if (unit != structure.items[i].unit) {
                        unit = "MIXED";
                        break;
                    }
                }
                structure.unit = unit;
            }
        });
    }
}]);