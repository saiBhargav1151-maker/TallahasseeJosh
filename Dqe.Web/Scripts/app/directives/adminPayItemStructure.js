dqeDirectives.directive('adminPayItemStructure', function() {
    return {
        restrict: 'E',
        scope: {
            payItemStructure: '=',
            prefix: '='
        },
        templateUrl: './Views/directives/admin-payitem-structure.html',
        controller: function ($scope, $http) {
            if ($scope.payItemStructure == undefined) {
                $scope.payItemStructure = getNewPayItemStructure();
            }
            $scope.showPayItemContent = function () {
                if ($scope.payItemStructure.showItems) {
                    $scope.payItemStructure.showItems = false;
                } else {
                    $scope.payItemStructure.showItems = true;
                    $http.get('./payitemadministration/GetPayItems', { params: { payItemStructureId: $scope.payItemStructure.id } }).success(function (result) {
                        if (!containsDqeError(result)) {
                            $scope.payItemStructure.items = getDqeData(result);
                        }
                    });
                }
            };
            $scope.effectiveOpen = function($event) {
                $event.preventDefault();
                $event.stopPropagation();
                $scope.effectiveOpened = true;
                $scope.obsoleteOpened = false;
                $scope.boeRecentChangeDateOpened = false;
            };
            $scope.obsoleteOpen = function($event) {
                $event.preventDefault();
                $event.stopPropagation();
                $scope.obsoleteOpened = true;
                $scope.effectiveOpened = false;
                $scope.boeRecentChangeDateOpened = false;
            };
            $scope.boeRecentChangeDateOpen = function($event) {
                $event.preventDefault();
                $event.stopPropagation();
                $scope.boeRecentChangeDateOpened = true;
                $scope.obsoleteOpened = false;
                $scope.effectiveOpened = false;
            };
            $scope.addPayItemStructure = function () {
                var resetPayItemStructure = false;
                if ($scope.payItemStructure.id == 0) {
                    resetPayItemStructure = true;
                }
                $scope.payItemStructure.structureId = $scope.prefix + $scope.payItemStructure.filteredStructureId;
                $http.post('./payitemstructureadministration/UpdatePayItemStructure', $scope.payItemStructure).success(function (result) {
                    if (resetPayItemStructure && !containsDqeError(result)) {
                        $scope.payItemStructure = getNewPayItemStructure();
                    }
                });
            };
            $scope.toggleDetail = function () {
                if ($scope.payItemStructure.showSummary) {
                    $http.get('./payitemstructureadministration/GetPayItemStructure', { params: { id: $scope.payItemStructure.id } }).success(function(result) {
                        if (!containsDqeError(result)) {
                            $scope.payItemStructure = getDqeData(result);
                            $scope.payItemStructure.filteredStructureId = $scope.payItemStructure.structureId.substr(2);
                        }
                    });
                } else {
                    $scope.payItemStructure.showSummary = !$scope.payItemStructure.showSummary;
                }
            };
            $scope.getLinks = function(val, linkType, arr) {
                return $http.get('./weblinkadministration/SearchWebLinks', { params: { linkType: linkType, val: val } }).then(function(result) {
                    var data = getDqeData(result);
                    var links = [];
                    var addItem = true;
                    angular.forEach(data, function (item) {
                        for (var i = 0; i < arr.length; i++) {
                            if (arr[i].id == item.id) {
                                addItem = false;
                                break;
                            }
                            addItem = true;
                        }
                        if (addItem) links.push(item);
                    });
                    return links;
                });
            }
            //otherReference
            $scope.addOtherReference = function () {
                addLink($scope.newOtherReference, $scope.payItemStructure.otherReferences);
            };
            $scope.removeOtherReference = function (otherReference) {
                removeLink(otherReference, $scope.payItemStructure.otherReferences);
            };
            //ppmChapter
            $scope.addPpmChapter = function () {
                addLink($scope.newPpmChapter, $scope.payItemStructure.ppmChapters);
            };
            $scope.removePpmChapter = function (ppmChapter) {
                removeLink(ppmChapter, $scope.payItemStructure.ppmChapters);
            };
            //prepAndDocChapter
            $scope.addPrepAndDocChapter = function () {
                addLink($scope.newPrepAndDocChapter, $scope.payItemStructure.prepAndDocChapters);
            };
            $scope.removePrepAndDocChapter = function (prepAndDocChapter) {
                removeLink(prepAndDocChapter, $scope.payItemStructure.prepAndDocChapters);
            };
            //standard
            $scope.addStandard = function () {
                addLink($scope.newStandard, $scope.payItemStructure.standards);
            };
            $scope.removeStandard = function (standard) {
                removeLink(standard, $scope.payItemStructure.standards);
            };
            //specification
            $scope.addSpecification = function () {
                addLink($scope.newSpecification, $scope.payItemStructure.specifications);
            };
            $scope.removeSpecification = function (specification) {
                removeLink(specification, $scope.payItemStructure.specifications);
            };
            $scope.tabs = [
                { title: 'Structure'},
                { title: 'Items' }
            ];
            $scope.showStructureContent = true;
            $scope.showItemsContent = false;
            $scope.showTabContent = function(tab) {
                if (tab.title == 'Structure') {
                    $scope.showStructureContent = true;
                    $scope.showItemsContent = false;
                } else {
                    $scope.showStructureContent = false;
                    $scope.showItemsContent = true;
                    $http.get('./payitemadministration/GetPayItems', { params: { payItemStructureId: $scope.payItemStructure.id } }).success(function (result) {
                        if (!containsDqeError(result)) {
                            $scope.payItemStructure.items = getDqeData(result);
                        }
                    });
                }
            };
            function addLink(newLink, arr) {
                if (newLink == undefined || newLink == '') return;
                arr.push(newLink);
            }
            function removeLink(link, arr) {
                var index = arr.indexOf(link);
                if (index > -1) {
                    arr.splice(index, 1);
                }
            }
            function getNewPayItemStructure() {
                return {
                    id: 0,
                    structureId: '',
                    title: '',
                    effectiveDate: '',
                    obsoleteDate: '',
                    primaryUnit: 0,
                    secondaryUnit: 0,
                    accuracy: 0,
                    isPlanQuantity: 'False',
                    isDoNotBid: 'False',
                    isFixedPrice: 'False',
                    fixedAmount: 0,
                    notes: '',
                    details: '',
                    essHistory: '',
                    boeRecentChangeDate: '',
                    boeRecentChangeDescription: '',
                    structureDescription: '',
                    showSummary: false,
                    showItems: true,
                    otherReferences: [],
                    prepAndDocChapters: [],
                    specifications: [],
                    ppmChapters: [],
                    standards: [],
                    items: []
                };
            }
        }
    }
});