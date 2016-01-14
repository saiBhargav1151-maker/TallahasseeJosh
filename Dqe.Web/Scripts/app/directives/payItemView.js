dqeDirectives.directive('payItemView', function() {
    return {
        restrict: 'E',
        scope: {},
        templateUrl: './Views/directives/payItemView.html',
        controller: ['$scope', '$http', '$filter', function($scope, $http, $filter) {
            $scope.searchPayItems = {
                specBook: "",
                masterFile: new Object(),
                structure: false,
                showCurrentItems: false
            };

            $scope.removedItemsWithoutStructures = new Array();
            $scope.removedItemsNotCurrent = new Array();
            $scope.currentPage = 0;
            $scope.pageSize = 75;
            $scope.setCurrentPage = function (currentPage) {
                $scope.currentPage = currentPage;
            }
            $scope.getNumberAsArray = function (num) {
                return new Array(num);
            };
            $scope.numberOfPages = function () {
                if ($scope.filteredItems != undefined) {
                    return Math.ceil($scope.filteredItems.length / $scope.pageSize);
                }
                return 0;
            };

            $scope.reportFormat = {
                type: "PDF"
            };

            function getSpecBooks() {
                $http.get('./PayItemStructureAdministration/GetSpecBooks').success(function (result) {
                    if (!containsDqeError(result)) {
                        $scope.specBooks = getDqeData(result);
                    }
                });
            };

            $scope.getPayItems = function (specBook) {
                $http.get('./PayItemStructureAdministration/GetPayItemsBySpecBook', { params: { specBook: specBook } }).success(function (result) {
                    if (!containsDqeError(result)) {
                        $scope.searchPayItems.structure = false;
                        $scope.searchPayItems.showCurrentItems = false;
                        //$scope.payItems = getDqeData(result);

                        var data = getDqeData(result);
                        $scope.payItems = data.payItems;
                        $scope.security = data.security;

                        angular.forEach($scope.specBooks, function (item) {
                            if (item.name === specBook) {
                                $scope.searchPayItems.masterFile = item;
                                document.getElementById("hiddenMasterFile").value = item.name;
                            }
                        });

                        $scope.filter = {
                            id: '',
                            name: '',
                            description: '',
                            unit: '',
                            itemClass: '',
                            specTech: '',
                            combFlag: ''
                            //obsoleteFlag: ''
                        };
                        $scope.filterItems();
                        $scope.filteredItems = $filter('orderBy')($scope.filteredItems, 'name');
                    }
                });
            };

            $http.get('./PayItemStructureAdministration/GetCurrentSpecBook').success(function (result) {
                if (!containsDqeError(result)) {
                    var specBook = getDqeData(result);
                    $scope.searchPayItems.specBook = specBook.specBook;
                    getSpecBooks();
                    $scope.getPayItems(specBook.specBook);
                }
            });

            $scope.filterStructures = function () {
                if ($scope.searchPayItems.structure) {
                    angular.forEach($scope.payItems, function (item) {
                        if (!item.hasStructure) {
                            $scope.removedItemsWithoutStructures.push(item);
                        }
                    });
                    angular.forEach($scope.removedItemsWithoutStructures, function (item) {
                        var index = $scope.payItems.indexOf(item);
                        $scope.payItems.splice(index, 1);
                    });
                } else {
                    angular.forEach($scope.removedItemsWithoutStructures, function (item) {
                        if (item.isObsolete && $scope.searchPayItems.showCurrentItems) {
                            $scope.removedItemsNotCurrent.push(item);
                        } else {
                            $scope.payItems.push(item);
                        }
                    });
                    $scope.removedItemsWithoutStructures = new Array();
                }
                $scope.filterItems();
            };

            $scope.filterCurrent = function () {
                if ($scope.searchPayItems.showCurrentItems) {
                    angular.forEach($scope.payItems, function (item) {
                        if (item.isObsolete) {
                            $scope.removedItemsNotCurrent.push(item);
                        }
                    });
                    angular.forEach($scope.removedItemsNotCurrent, function (item) {
                        var index = $scope.payItems.indexOf(item);
                        $scope.payItems.splice(index, 1);
                    });
                } else {
                    angular.forEach($scope.removedItemsNotCurrent, function (item) {
                        if (!item.hasStructure && $scope.searchPayItems.structure) {
                            $scope.removedItemsWithoutStructures.push(item);
                        } else {
                            $scope.payItems.push(item);
                        }
                    });
                    $scope.removedItemsNotCurrent = new Array();
                }
                $scope.filterItems();
            };

            $scope.filterItems = function () {
                $scope.currentPage = 0;
                $scope.filteredItems = $filter('filter')($scope.payItems,
                {
                    id: $scope.filter.id,
                    name: $scope.filter.name,
                    description: $scope.filter.description,
                    unit: $scope.filter.unit,
                    itemClass: $scope.filter.itemClass,
                    specTech: $scope.filter.specTech,
                    combFlag: $scope.filter.combFlag
                    //obsoleteFlag: $scope.filter.obsoleteFlag
                });
            };

            $scope.clearAllFilters = function () {
                $scope.filteredItems = $filter('filter')($scope.payItems,
                {
                    id: '',
                    name: '',
                    description: '',
                    unit: '',
                    itemClass: '',
                    specTech: '',
                    combFlag: ''
                    //obsoleteFlag: ''
                });
                $scope.filter.id = '';
                $scope.filter.name = '';
                $scope.filter.description = '';
                $scope.filter.unit = '';
                $scope.filter.itemClass = '';
                $scope.filter.specTech = '';
                $scope.filter.combFlag = '';
                //$scope.filter.obsoleteFlag = '';
                $scope.filterItems();
            };

            //begin bid history directive support
            $http.get('./marketarea/GetMarketAreas').success(function (res) {
                if (!containsDqeError(res)) {
                    $scope.marketAreas = getDqeData(res);
                    if ($scope.marketAreas != null && $scope.marketAreas != undefined && $scope.marketAreas.marketAreas != null && $scope.marketAreas.marketAreas != undefined) {
                        var marketAreas = $scope.marketAreas.marketAreas;
                        for (var i = 0; i < marketAreas.length; i++) {
                            marketAreas[i].include = true;
                            for (var ii = 0; ii < marketAreas[i].counties.length; ii++) {
                                marketAreas[i].counties[ii].include = true;
                            }
                        }
                    }
                }
            });
            $http.get('./PayItemStructureAdministration/GetWorkTypes').success(function (res) {
                if (!containsDqeError(res)) {
                    $scope.workTypes = getDqeData(res);
                    if ($scope.workTypes != null && $scope.workTypes != undefined) {
                        var workTypes = $scope.workTypes;
                        for (var i = 0; i < workTypes.length; i++) {
                            workTypes[i].include = true;
                        }
                    }
                }
            });
            $scope.contractType = 'all';
            $scope.closePricingParameters = function (itemGroup) {
                itemGroup.showPricingParameters = !itemGroup.showPricingParameters;
                itemGroup.history = null;
            }
            $scope.showPricingParameters = function (itemGroup) {
                if (!itemGroup.showPricingParameters) {
                    itemGroup.itemNumber = itemGroup.name;
                    var itemToPrice = {
                        itemGroup: itemGroup,
                        county: ''
                    }

                    $http.post('./estimate/GetBidHistory', itemToPrice).success(function (result) {
                        if (!containsDqeError(result)) {
                            itemGroup.history = getDqeData(result);
                            itemGroup.history.contractType = 'all';
                            itemGroup.history.omitOutliers = true;
                            itemGroup.history.includeEstimates = false;
                            itemGroup.history.bidMonths = 36;
                            itemGroup.history.workTypes = [];
                            for (var i = 0; i < $scope.workTypes.length; i++) {
                                itemGroup.history.workTypes.push({
                                    name: $scope.workTypes[i].name,
                                    code: $scope.workTypes[i].code,
                                    include: true
                                });
                            }
                            itemGroup.history.marketAreas = [];
                            for (i = 0; i < $scope.marketAreas.marketAreas.length; i++) {
                                var ma = {
                                    id: $scope.marketAreas.marketAreas[i].id,
                                    name: $scope.marketAreas.marketAreas[i].name,
                                    include: true,
                                    counties: []
                                };
                                itemGroup.history.marketAreas.push(ma);
                                for (var ii = 0; ii < $scope.marketAreas.marketAreas[i].counties.length; ii++) {
                                    var c = {
                                        id: $scope.marketAreas.marketAreas[i].counties[ii].id,
                                        name: $scope.marketAreas.marketAreas[i].counties[ii].name,
                                        code: $scope.marketAreas.marketAreas[i].counties[ii].code,
                                        include: true
                                    };
                                    ma.counties.push(c);
                                }
                            }
                        }
                    });
                }
                itemGroup.showPricingParameters = !itemGroup.showPricingParameters;
            }
            //end bid history directive support

            $scope.viewMasterFileReport = function() {
                $.download('./report/ViewMasterFileReport', $('form#ViewMasterFileReport').serialize());
            };

            jQuery.download = function (url, data, method) {
                //url and data options required
                if (url && data) {
                    //data can be string of parameters or array/object
                    data = typeof data == 'string' ? data : jQuery.param(data);
                    //split params into form inputs
                    var inputs = '';
                    jQuery.each(data.split('&'), function () {
                        var pair = this.split('=');
                        inputs += '<input type="hidden" name="' + pair[0] + '" value="' + pair[1] + '" />';
                    });
                    //send request
                    jQuery('<form action="' + url + '" method="' + (method || 'post') + '">' + inputs + '</form>')
                    .appendTo('body').submit().remove();
                };
            };
        }
    ]}
});