dqeDirectives.directive('pricing', function () {
    return {
        restrict: 'E',
        scope: {
            pricingLevel: '='
        },
        templateUrl: './Views/directives/pricing.html',
        controller: ['$scope', '$rootScope', '$http', '$filter', '$location', 'stateService', 'fileUpload', '$localStorage', '$route', 'securityService', '$timeout'
            , function ($scope, $rootScope, $http, $filter, $location, stateService, fileUpload, $localStorage, $route, securityService, $timeout) {
                $scope.prp = {
                    syncPrices: true
            };

            $scope.canSyncPrices = true;    
            //if is on list, then make can checkout var as true.
            $scope.user = null;
            securityService.getCurrentUser(function (user) {
                let role = user.role.toString()[0];
                if (role === 'C') {
                    $scope.canSyncPrices = false;
                    $scope.prp = {
                        syncPrices: false
                    };
                    $scope.user = user;
                }
            });


                //$scope.getContracts = function (val) {
                //    var v = (val == null ? '' : String(val)).trim();
                //    if (v.length < 3) return $q.when([]);
                //    return $http.get('./projectproposal/GetProposals', { params: { number: v } })
                //        .then(function (response) {
                //            var proposals = [];
                //            angular.forEach(response.data, function (item) {
                //                proposals.push(item.number);
                //                /* item.displayName = item.number;*/
                //                //if ((item.contractType[0] == 'M')) {
                //                //    item.displayName = '(M) ' + item.displayName;
                //                //}
                //                //else {
                //                //    item.displayName = '(C) ' + item.displayName;
                //                //}
                //                //proposals.push(item);
                //            });
                //            return proposals;
                //        });
                //};

                //// Bidders filter typeahead-wait-ms debounce, multi-select via tags
                //$scope.getBidders = function (val) {
                //    var v = (val == null ? '' : String(val)).trim();
                //    if (v.length < 3) return $q.when([]);
                //    return $http.get('/UnitPriceSearch/GetBidderSuggestions', { params: { input: v } })
                //        .then(function (r) { return r.data || []; }, function () { return []; });
                //};
                //$scope.addBidderFromTypeahead = function (item) {
                //    var name = item;
                //    if (!name || $scope.parmSet.selectedBidders.indexOf(name) !== -1) {
                //        $scope.parmSet.bidderTypeahead = null;
                //        return;
                //    }
                //    $scope.parmSet.selectedBidders = $scope.parmSet.selectedBidders.concat([name]);
                //    $scope.parmSet.selectedBidders.sort();
                //    $timeout(function () { $scope.parmSet.bidderTypeahead = null; }, 0);
                //    $scope.parmSet.bidderTypeahead = null;
                //};
                //$scope.removeBidder = function (name) {
                //    var idx = $scope.parmSet.selectedBidders.indexOf(name);
                //    if (idx !== -1) {
                //        $scope.parmSet.selectedBidders = $scope.parmSet.selectedBidders.slice(0, idx).concat($scope.parmSet.selectedBidders.slice(idx + 1));
                //    }
                //};

            $scope.transferLsDbEstimate = function (estimate) {
                var lsdbLoad = {
                    number: estimate.project.number,
                    estimateId : estimate.estimateId
                };
                $http.post("./estimate/TransferLsDbEstimate", lsdbLoad).success(function (result) {
                    if (!containsDqeError(result)) {
                        var summary = getDqeData(result);
                        for (var i = 0; i < $scope.estimate.itemGroups.length; i++) {
                            if ($scope.estimate.itemGroups[i].itemNumber == summary.summaryBucketItem.payItemName) {
                                $scope.estimate.itemGroups[i].price = summary.summaryBucketItem.total;
                                $scope.estimate.itemGroups[i].priceType = "X";
                            } else {
                                for (var ii = 0; ii < summary.matchedItemList.length; ii++) {
                                    if ($scope.estimate.itemGroups[i].itemNumber == summary.matchedItemList[ii].payItemName) {
                                        $scope.estimate.itemGroups[i].price = summary.matchedItemList[ii].total;
                                        $scope.estimate.itemGroups[i].priceType = "X";
                                    }
                                }
                            }
                        }
                    }
                });
            }
            $scope.$storage = $localStorage.$default({
                visibleColumns: {
                    showCategoryColumn: true,
                    showProjectColumn: true,
                    showCombineCategoriesColumn: true,
                    showCombineItemsColumn: true,
                    showCategoryAlternateSetColumn: true,
                    showCategoryAlternateMemberColumn: true,
                    showItemAlternateSetColumn: true,
                    showItemAlternateMemberColumn: true,
                    showItemDescriptionColumn: true,
                    showSupplementalDescriptionColumn: true,
                    showFundColumn: true,
                    showPreviousPriceColumn: true,
                    showObsoleteColumn: true
                },
                contractType: 'construction',
                //worktypes: $scope.workTypes,
                //marketAreas: $scope.marketAreas,
                bidMonths: 36,
                bidFilter: 'ALL',
                numberOfBids: 0,
                includeLsDbEstimates: false,
                includeSvEstimates: false,
                contractNumberFilters: "",
                contractNumbersListed: "",
                //bidderTypeahead: "",
                //selectedBidders: [],
                useStraightAverage: false,
                fontSize: 'S',
                showNonVisibleColumns: true
            });
            $scope.saveMasterParameters = function () {
                $scope.$storage.contractType = $scope.parmSet.contractType;
                $scope.$storage.bidMonths = $scope.parmSet.bidMonths;
                $scope.$storage.bidFilter = $scope.parmSet.bidFilter,
                $scope.$storage.numberOfBids = $scope.parmSet.numberOfBids,
                $scope.$storage.includeLsDbEstimates = $scope.parmSet.includeLsDbEstimates,
                $scope.$storage.includeSvEstimates = $scope.parmSet.includeSvEstimates,

                //$scope.$storage.selectedBidders = $scope.parmSet.selectedBidders,
                $scope.$storage.useStraightAverage = $scope.parmSet.useStraightAverage,
                delete $scope.$storage.workTypes;
                $scope.$storage.workTypes = [];
                if ($scope.workTypes != undefined && $scope.workTypes.length > 0) {
                    for (var i = 0; i < $scope.workTypes.length; i++) {
                        $scope.$storage.workTypes.push({
                            code: $scope.workTypes[i].code,
                            include: $scope.workTypes[i].include
                        });
                    }
                }
                delete $scope.$storage.marketAreas;
                $scope.$storage.marketAreas = [];
                if ($scope.marketAreas != undefined && $scope.marketAreas.marketAreas != undefined && $scope.marketAreas.marketAreas.length > 0) {
                    for (i = 0; i < $scope.marketAreas.marketAreas.length; i++) {
                        var ma = {
                            name: $scope.marketAreas.marketAreas[i].name,
                            include: $scope.marketAreas.marketAreas[i].include,
                            counties: []
                        };
                        if ($scope.marketAreas.marketAreas[i].counties != undefined && $scope.marketAreas.marketAreas[i].counties.length > 0) {
                            for (var ii = 0; ii < $scope.marketAreas.marketAreas[i].counties.length; ii++) {
                                ma.counties.push({
                                    name: $scope.marketAreas.marketAreas[i].counties[ii].name,
                                    include: $scope.marketAreas.marketAreas[i].counties[ii].include
                                });
                            }
                        }
                        $scope.$storage.marketAreas.push(ma);
                    }
                }
            }
            //scroll management
            $scope.scrollPosition = null;
            $scope.rememberScroll = function() {
                $scope.scrollPosition = $(window).scrollTop();
            }
            $scope.goToLastScrollPosition = function () {
                if ($scope.scrollPosition != undefined && $scope.scrollPosition != null && $scope.scrollPosition > 0) {
                    var position = $scope.scrollPosition;
                    $("body, html").animate({ scrollTop: position }, "slow");
                    $scope.scrollPosition = null;
                } 
            }
            //cost-based template process
            $scope.toggleCostBasedTemplate = function(itemGroup) {
                if (!itemGroup.showTemplate) {
                    itemGroup.showTemplate = true;
                } else {
                    itemGroup.showTemplate = false;
                }
            }
            var successUploadFile = function (result) {
                if (!containsDqeError(result)) {
                    var data = getDqeData(result);
                    for (var i = 0; i < $scope.filteredItems.length; i++) {
                        if (!$scope.filteredItems[i].group) continue;
                        if ($scope.filteredItems[i].itemId == data.itemId) {
                            var itemGroup = $scope.filteredItems[i];
                            itemGroup.currentTemplate.file = '';
                            itemGroup.templatePrice = data.templatePrice;
                            document.forms["templateForm" + itemGroup.itemId].reset();
                            $scope.setTemplatePrice(itemGroup);
                        }
                    }
                }
            };
            $scope.isTemplateUploadVisible = function (itemGroup) {
                if (itemGroup == undefined) return false;
                if (itemGroup.currentTemplate == undefined) return false;
                if (itemGroup.currentTemplate.file == undefined) return false;
                if (itemGroup.currentTemplate.file == '') return false;
                return true;
            }
            $scope.uploadTemplate = function (itemGroup) {
                var baseTemplateId = itemGroup.costBasedTemplateId;
                var uploadUrl = "./estimate/SaveCostBasedTemplate";
                var fd = new FormData();
                fd.append("baseTemplateId", baseTemplateId);
                fd.append("itemId", itemGroup.itemId);
                fileUpload.uploadFileToUrl(itemGroup.currentTemplate.file, uploadUrl, fd, successUploadFile);
            };
            //end cost-based template process
            $scope.filterItem = function (itemNumber) {
                $scope.filter.itemNumber = itemNumber;
                $scope.filterItems();
            }
            $scope.monthRange = [];
            for (var r = 0; r < 36; r++) {
                $scope.monthRange.push(r + 1);
            }
            function buildSummary() {
                $scope.estimateSummary = [];
                for (var i = 0; i < $scope.estimate.total.CategorySets.length; i++) {
                    for (var ii = 0; ii < $scope.estimate.total.CategorySets[i].ItemSets.length; ii++) {
                        $scope.estimateSummary.push({
                            included: $scope.estimate.total.CategorySets[i].Included && $scope.estimate.total.CategorySets[i].ItemSets[ii].Included,
                            categorySet: $scope.estimate.total.CategorySets[i].Set,
                            categoryMember: $scope.estimate.total.CategorySets[i].Member,
                            itemSet: $scope.estimate.total.CategorySets[i].ItemSets[ii].Set,
                            itemMember: $scope.estimate.total.CategorySets[i].ItemSets[ii].Member,
                            total: $scope.estimate.total.CategorySets[i].ItemSets[ii].Total,
                        });
                    }
                }
            }
            $scope.visibleColumns = {
                showCategoryColumn: $scope.$storage.visibleColumns.showCategoryColumn,
                showProjectColumn: $scope.$storage.visibleColumns.showProjectColumn,
                showCombineCategoriesColumn: $scope.$storage.visibleColumns.showCombineCategoriesColumn,
                showCombineItemsColumn: $scope.$storage.visibleColumns.showCombineItemsColumn,
                showCategoryAlternateSetColumn: $scope.$storage.visibleColumns.showCategoryAlternateSetColumn,
                showCategoryAlternateMemberColumn: $scope.$storage.visibleColumns.showCategoryAlternateMemberColumn,
                showItemAlternateSetColumn: $scope.$storage.visibleColumns.showItemAlternateSetColumn,
                showItemAlternateMemberColumn: $scope.$storage.visibleColumns.showItemAlternateMemberColumn,
                showItemDescriptionColumn: $scope.$storage.visibleColumns.showItemDescriptionColumn,
                showSupplementalDescriptionColumn: $scope.$storage.visibleColumns.showSupplementalDescriptionColumn,
                showFundColumn: $scope.$storage.visibleColumns.showFundColumn,
                showPreviousPriceColumn: $scope.$storage.visibleColumns.showPreviousPriceColumn,
                showObsoleteColumn: $scope.$storage.visibleColumns.showObsoleteColumn
            };
            var loadLink = $scope.pricingLevel == 'project' ? './estimate/LoadProjectEstimate' : './estimate/LoadProposalEstimate';
            var loadSummaryLink = $scope.pricingLevel == 'project' ? './estimate/LoadProjectEstimateSummary' : './estimate/LoadProposalEstimateSummary';
            //Make sure the currentEstimateId/currentProposalId is loaded properly to the stateService from previous pages. MB.
            var loadId = $scope.pricingLevel == 'project' ? stateService.currentEstimateId : stateService.currentProposalId;
            if (loadId == '' || loadId == 0) {
                if ($scope.pricingLevel == 'project') {
                    if ($route.current.params != 'undefined' && $route.current.params != null) {
                        if ($route.current.params.project != 'undefined' && $route.current.params.project != null) {
                            $location.url('/home_project/' + $route.current.params.project);
                        } else {
                            $location.url('/home_project');
                        }
                    } else {
                        $location.url('/home_project');
                    }
                } else {
                    if ($route.current.params != 'undefined' && $route.current.params != null) {
                        if ($route.current.params.proposal != 'undefined' && $route.current.params.proposal != null) {
                            $location.url('/home_proposal/' + $route.current.params.proposal);
                        } else {
                            $location.url('/home_proposal');
                        }
                    } else {
                        $location.url('/home_proposal');
                    }
                }
            } else {

                //test
                //$http.post('./estimate/TestReport').success(function (result) {
                //    var rpt = getDqeData(result);
                //});
                //end test


                $http.post(loadLink, { loadId: loadId }).success(function (result) {
                    if (!containsDqeError(result)) {
                        $scope.estimate = getDqeData(result);
                        $scope.confidentialNotice = $scope.estimate.proposal.confidentialData;

                        //set confidential data flag here.MB.
                        if ($scope.pricingLevel == 'project') {
                            $http.get('./projectproposal/GetLsDbProject', { params: { number: $scope.estimate.project.number } }).success(function (rlt) {
                                if (!containsDqeError(rlt)) {
                                    $scope.estimate.hasDetailProject = getDqeData(rlt).hasDetailProject;                                  
                                }
                            });                      
                        }
                        //document.getElementById("hiddenProposalId").value = $scope.estimate.proposal.id;
                        //document.getElementById("hiddenProposalNumber").value = $scope.estimate.proposal.number;
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
                                if ($scope.$storage.marketAreas != undefined && $scope.$storage.marketAreas.length > 0) {
                                    for (i = 0; i < $scope.$storage.marketAreas.length; i++) {
                                        for (ii = 0; ii < $scope.marketAreas.marketAreas.length; ii++) {
                                            if ($scope.marketAreas.marketAreas[ii].name == $scope.$storage.marketAreas[i].name) {
                                                $scope.marketAreas.marketAreas[ii].include = $scope.$storage.marketAreas[i].include;
                                                for (var iii = 0; iii < $scope.$storage.marketAreas[i].counties.length; iii++) {
                                                    for (var iiii = 0; iiii < $scope.marketAreas.marketAreas[ii].counties.length; iiii++) {
                                                        if ($scope.marketAreas.marketAreas[ii].counties[iiii].name == $scope.$storage.marketAreas[i].counties[iii].name) {
                                                            $scope.marketAreas.marketAreas[ii].counties[iiii].include = $scope.$storage.marketAreas[i].counties[iii].include;
                                                            break;
                                                        }
                                                    }
                                                }
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        });
                        $http.get('./estimate/GetWorkTypes').success(function (res) {
                            if (!containsDqeError(res)) {
                                $scope.workTypes = getDqeData(res);
                                if ($scope.workTypes != null && $scope.workTypes != undefined) {
                                    var workTypes = $scope.workTypes;
                                    for (var i = 0; i < workTypes.length; i++) {
                                        workTypes[i].include = true;
                                    }
                                }
                                if ($scope.$storage.workTypes != undefined && $scope.$storage.workTypes.length > 0) {
                                    for (i = 0; i < $scope.$storage.workTypes.length; i++) {
                                        for (var ii = 0; ii < $scope.workTypes.length; ii++) {
                                            if ($scope.workTypes[ii].code == $scope.$storage.workTypes[i].code) {
                                                $scope.workTypes[ii].include = $scope.$storage.workTypes[i].include;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        });
                        $scope.saveEstimate(false, false);
                        //$http.post(loadSummaryLink, { estimateId: $scope.estimate.estimateId }).success(function (r) {
                        //    if (!containsDqeError(r)) {
                        //        $scope.estimate.total = getDqeData(r).total;
                        //        buildSummary();
                        //    }
                        //});
                        $scope.parmSet.contractType = $scope.$storage.contractType;
                        $scope.parmSet.bidMonths = $scope.$storage.bidMonths;
                        $scope.parmSet.bidFilter = $scope.$storage.bidFilter;
                        $scope.parmSet.numberOfBids = $scope.$storage.numberOfBids;
                        $scope.parmSet.includeLsDbEstimates = $scope.$storage.includeLsDbEstimates;
                        $scope.parmSet.includeSvEstimates = $scope.$storage.includeSvEstimates;
                        $scope.parmSet.contractNumberFilters = $scope.$storage.contractNumberFilters;
                        //$scope.parmSet.selectedBidders = $scope.$storage.selectedBidders;
                        //$scope.parmSet.contractNumbersListed = parseCommaSeparatedText($scope.parmSet.contractNumberFilters);
                        $scope.parmSet.useStraightAverage = $scope.$storage.useStraightAverage;
                        //$scope.parmSet.bidderTypeahead = $scope.$storage.bidderTypeahead;

                        angular.forEach($scope.estimate.itemGroups, function (i) {
                            if (i.group) {
                                i.selected = false;
                            }
                        });
                        $scope.filter = $scope.pricingLevel == 'project' ? {
                            federalConstructionClass: '',
                            itemNumber: '',
                            combineCategories: '',
                            combineItems: '',
                            categoryAlternateSet: '',
                            categoryAlternateMember: '',
                            itemAlternateSet: '',
                            itemAlternateMember: '',
                            itemDescription: '',
                            fund: '',
                            supplementalDescription: '',
                            unit: '',
                            priceType: '',
                            isObsolete: ''
                        }
                            : {
                                federalConstructionClass: '',
                                projectNumber: '',
                                itemNumber: '',
                                combineCategories: '',
                                combineItems: '',
                                categoryAlternateSet: '',
                                categoryAlternateMember: '',
                                itemAlternateSet: '',
                                itemAlternateMember: '',
                                itemDescription: '',
                                fund: '',
                                supplementalDescription: '',
                                unit: '',
                                priceType: '',
                                isObsolete: ''
                            };
                        $scope.filterItems();
                        buildSummary();
                        $scope.canSaveEstimate = true;
                    }
                });
            }


            $scope.setPendingChanges = function () {
                $scope.estimate.hasPendingChanges = true;
            }
            $scope.clearPendingChanges = function () {
                $scope.estimate.hasPendingChanges = false;
            }


            // Pagination in controller
            $scope.currentPage = 0;
            $scope.pageSize = 75;
            $scope.setCurrentPage = function (currentPage) {
                $scope.currentPage = currentPage;
            }
            $scope.getNumberAsArray = function (num) {
                return new Array(num);
            };
            $scope.numberOfPages = function () {
                return Math.ceil($scope.filteredItems.length / $scope.pageSize);
            };
            $scope.filterItems = function () {
                $scope.currentPage = 0;
                $scope.filteredItems = $filter('filter')($scope.estimate.itemGroups,
                    $scope.pricingLevel == 'project' ?
                    {
                        federalConstructionClass: $scope.filter.federalConstructionClass,
                        itemNumber: $scope.filter.itemNumber,
                        combineCategories: $scope.filter.combineCategories == '*' ? true : '',
                        combineItems: $scope.filter.combineItems == '*' ? true : '',
                        categoryAlternateSet: $scope.filter.categoryAlternateSet,
                        categoryAlternateMember: $scope.filter.categoryAlternateMember,
                        itemAlternateSet: $scope.filter.itemAlternateSet,
                        itemAlternateMember: $scope.filter.itemAlternateMember,
                        itemDescription: $scope.filter.itemDescription,
                        fund: $scope.filter.fund,
                        supplementalDescription: $scope.filter.supplementalDescription,
                        unit: $scope.filter.unit,
                        priceType: $scope.filter.priceType,
                        isObsolete: $scope.filter.isObsolete == '*' ? true : ''
                    } :
                    {
                        federalConstructionClass: $scope.filter.federalConstructionClass,
                        projectNumber: $scope.filter.projectNumber,
                        itemNumber: $scope.filter.itemNumber,
                        combineCategories: $scope.filter.combineCategories == '*' ? true : '',
                        combineItems: $scope.filter.combineItems == '*' ? true : '',
                        categoryAlternateSet: $scope.filter.categoryAlternateSet,
                        categoryAlternateMember: $scope.filter.categoryAlternateMember,
                        itemAlternateSet: $scope.filter.itemAlternateSet,
                        itemAlternateMember: $scope.filter.itemAlternateMember,
                        itemDescription: $scope.filter.itemDescription,
                        fund: $scope.filter.fund,
                        supplementalDescription: $scope.filter.supplementalDescription,
                        unit: $scope.filter.unit,
                        priceType: $scope.filter.priceType,
                        isObsolete: $scope.filter.isObsolete == '*' ? true : ''
                    }
                );
            }
            $scope.clearAllFilters = function () {
                if ($scope.pricingLevel == 'project') {

                    $scope.filter.federalConstructionClass = '';
                    $scope.filter.itemNumber = '';
                    $scope.filter.combineCategories = '';
                    $scope.filter.combineItems = '';
                    $scope.filter.categoryAlternateSet = '';
                    $scope.filter.categoryAlternateMember = '';
                    $scope.filter.itemAlternateSet = '';
                    $scope.filter.itemAlternateMember = '';
                    $scope.filter.itemDescription = '';
                    $scope.filter.fund = '';
                    $scope.filter.supplementalDescription = '';
                    $scope.filter.unit = '';
                    $scope.filter.priceType = '';
                    $scope.filter.isObsolete = '';
                } else
                {
                    $scope.filter.federalConstructionClass = '';
                    $scope.filter.projectNumber = '';
                    $scope.filter.itemNumber = '';
                    $scope.filter.combineCategories = '';
                    $scope.filter.combineItems = '';
                    $scope.filter.categoryAlternateSet = '';
                    $scope.filter.categoryAlternateMember = '';
                    $scope.filter.itemAlternateSet = '';
                    $scope.filter.itemAlternateMember = '';
                    $scope.filter.itemDescription = '';
                    $scope.filter.fund = '';
                    $scope.filter.supplementalDescription = '';
                    $scope.filter.unit = '';
                    $scope.filter.priceType = '';
                    $scope.filter.isObsolete = '';
                };
                $scope.filterItems();
            }
            $scope.sortItems = function (sort) {
                $scope.filteredItems = $filter('filter')($scope.filteredItems, { group: true });
                var sortArray = [];
                sortArray.push(sort);
                if ($scope.lastItemSort == sort) {
                    $scope.sortItemReverse = !$scope.sortItemReverse;
                } else {
                    $scope.lastItemSort = sort;
                    $scope.sortItemReverse = false;
                }
                //bug fix for price sort that accounts for the currency mask
                if (sort == 'price' || sort == 'price * quantity') {
                    for (var i = 0; i < $scope.filteredItems.length; i++) {
                        $scope.filteredItems[i].price = Number($scope.filteredItems[i].price.toString().replace(/[^0-9\.]+/g, ""));
                    }
                }
                //end bug fix
                $scope.filteredItems = $filter('orderBy')($scope.filteredItems, sortArray, $scope.sortItemReverse);
                for (i = 0; i < $scope.filteredItems.length; i++) {
                    if (!$scope.filteredItems[i].group) continue;
                    var x = i + 1;
                    for (var ii = 0; ii < $scope.estimate.itemGroups.length; ii++) {
                        if ($scope.estimate.itemGroups[ii].group) continue;
                        if ($scope.estimate.itemGroups[ii].key != $scope.filteredItems[i].key) continue;
                        $scope.filteredItems.splice(x, 0, $scope.estimate.itemGroups[ii]);
                        x = x + 1;
                    }
                }
            }
            $scope.selectItems = function () {
                angular.forEach($scope.filteredItems, function (i) {
                    if (i.group) {
                        i.selected = true;
                    }
                });
            }
            $scope.deselectItems = function () {
                angular.forEach($scope.filteredItems, function (i) {
                    if (i.group) {
                        i.selected = false;
                    }
                });
            }
            $scope.toggleGroup = function (itemGroup) {
                if (itemGroup.isExpanded) {
                    itemGroup.isExpanded = false;
                    angular.forEach($scope.estimate.itemGroups, function (i) {
                        if (i.key == itemGroup.key && !i.group) {
                            i.isExpanded = false;
                        }
                    });
                } else {
                    itemGroup.isExpanded = true;
                    angular.forEach($scope.estimate.itemGroups, function (i) {
                        if (i.key == itemGroup.key && !i.group) {
                            i.isExpanded = true;
                        }
                    });
                }
            }
            //$scope.calculateTotals = function (itemGroup, overridePriceType) {
            //    if (!isNumber(itemGroup.price)) {
            //        itemGroup.price = 0;
            //    }
            //    if (itemGroup.price < 0) {
            //        itemGroup.price = 0;
            //    }
            //    if (itemGroup.price == 0) {
            //        itemGroup.priceType = "N";
            //    } else {
            //        itemGroup.priceType = overridePriceType;
            //    }
            //    if (itemGroup.group) {
            //        angular.forEach($scope.estimate.itemGroups, function (i) {
            //            if (i.key == itemGroup.key && !i.group) {
            //                i.price = itemGroup.price;
            //                if (i.price == 0) {
            //                    i.priceType = "N";
            //                } else {
            //                    i.priceType = overridePriceType;
            //                }
            //            }
            //        });
            //    }
            //}
            $scope.calculateTotals = function (itemGroup, overridePriceType) {
                var price = Number(itemGroup.price.toString().replace(/[^0-9\.]+/g, ""));
                if (!isNumber(price)) {
                    itemGroup.price = "$0.00";
                }
                if (price < 0) {
                    itemGroup.price = "$0.00";
                }
                if (price == 0) {
                    itemGroup.priceType = "N";
                } else {
                    itemGroup.priceType = overridePriceType;
                }
                if (itemGroup.group) {
                    angular.forEach($scope.estimate.itemGroups, function (i) {
                        if (i.key == itemGroup.key && !i.group) {
                            price = Number(itemGroup.price.toString().replace(/[^0-9\.]+/g, ""));
                            i.price = price;
                            if (price == 0) {
                                i.priceType = "N";
                            } else {
                                i.priceType = overridePriceType;
                            }
                        }
                    });
                }
            }
            $scope.getExtendedAmount = function(price, quantity)
            {
               
                price = Number(price.toString().replace(/[^0-9\.]+/g, ""));
                return Math.round(price * 100) / 100 * quantity;
                
            }
            //the name of this is misleading as it is being multipurposed for loading Estimate data also
            $scope.saveEstimate = function (updatePreviousPrices, pushPrices) {             
                var siteManagerDssMaximum = false;                
                angular.forEach($scope.estimate.itemGroups, function (itemGroup) {
           
                    if ($scope.getExtendedAmount(itemGroup.price, itemGroup.quantity) > 99999999.99) {
                        siteManagerDssMaximum = true;
                    }
                });

                if (siteManagerDssMaximum === true) {
                    $http.post('./estimate/ThrowSiteManagerExtendedAmountError');
                } else {
                    $scope.clearPendingChanges();
                    if ($scope.estimate == undefined
                            || $scope.estimate == null
                    ) return;
                    $scope.estimate.isSystemSync = false;
                    var saveLink = $scope.pricingLevel == 'project' ? './estimate/SaveEstimate' : './estimate/SaveProposalEstimate';
                    var est = {
                        isSystemSync: $scope.estimate.isSystemSync,
                        itemGroups: [],
                        pushPrices: pushPrices
                    }           
                    for (var i = 0; i < $scope.estimate.itemGroups.length; i++) {
                        est.itemGroups.push({
                            itemId: $scope.estimate.itemGroups[i].itemId,
                            //price: $scope.estimate.itemGroups[i].price,
                            price: Number($scope.estimate.itemGroups[i].price.toString().replace(/[^0-9\.]+/g, "")),
                            isSystemOverride: $scope.estimate.itemGroups[i].isSystemOverride,
                            priceType: $scope.estimate.itemGroups[i].priceType
                        });
                    }
                    $http.post(saveLink, est).success(function(result) {
                        if (!containsDqeError(result)) {
                            for (var x = 0; x < $scope.estimate.itemGroups.length; x++) {
                                var itemGroup = $scope.estimate.itemGroups[x];

                                var lastPrice = Number(itemGroup.holdPrice.toString().replace(/[^0-9\.]+/g, ""));
                                var currentPrice = Number(itemGroup.price.toString().replace(/[^0-9\.]+/g, ""));
                                if (updatePreviousPrices && !itemGroup.canExpand) {

                                    if (currentPrice != lastPrice) {
                                        itemGroup.previousPrice = lastPrice;
                                    }

                                    //itemGroup.previousPrice = Number(itemGroup.holdPrice.toString().replace(/[^0-9\.]+/g, ""));
                                }
                                itemGroup.holdPriceType = itemGroup.priceType;
                                itemGroup.holdPrice = currentPrice;
                                if (itemGroup.group) {

                                    for (var ii = 0; ii < $scope.estimate.itemGroups.length; ii++) {
                                        if ($scope.estimate.itemGroups[ii].key == itemGroup.key && !$scope.estimate.itemGroups[ii].group) {
                                            if (updatePreviousPrices) {
                                                if (currentPrice != lastPrice) {
                                                    $scope.estimate.itemGroups[ii].previousPrice = lastPrice;
                                                }
                                            }
                                            $scope.estimate.itemGroups[ii].priceType = itemGroup.priceType;
                                            $scope.estimate.itemGroups[ii].holdPrice = itemGroup.price;
                                        }
                                    }

                                    //angular.forEach($scope.estimate.itemGroups, function (ii) {
                                    //    if (ii.key == itemGroup.key && !ii.group) {
                                    //        if (updatePreviousPrices) {
                                    //            ii.previousPrice = lastPrice;
                                    //        }
                                    //        ii.priceType = itemGroup.priceType;
                                    //        ii.holdPrice = itemGroup.price;
                                    //    }
                                    //});
                                }
                            }
                            $http.post(loadSummaryLink, { estimateId: $scope.estimate.estimateId }).success(function(res) {
                                if (!containsDqeError(res)) {
                                    $scope.estimate.total = getDqeData(res).total;
                                    buildSummary();
                                }
                            });
                        }
                    });
                }
            }
            $scope.setBestPrices = function () {
                $scope.canSaveEstimate = false;
                $scope.setPendingChanges();
                angular.forEach($scope.estimate.itemGroups, function (i) {
                    if (i.unit == 'LS - Lump Sum' && i.quantity == 1) return;
                    if (i.selected && !i.isFixedPrice) {
                        $scope.setFixedPrice(i);
                        if (i.fixedPrice == 0) {
                            $scope.setStatewidePrice(i);
                            $scope.setMarketAreaPrice(i);
                            $scope.setCountyPrice(i);
                            if (i.price == 0) {
                                $scope.setReferencePrice(i);
                            }
                        }
                    }
                });
                $scope.canSaveEstimate = true;
            }
            $scope.showPriceOptions = function (itemGroup) {
                angular.forEach($scope.estimate.itemGroups, function (i) {
                    if (itemGroup != i) {
                        i.showPriceOptions = false;
                    }
                });
                itemGroup.showPriceOptions = !itemGroup.showPriceOptions;
            }
            $scope.setReferencePrice = function (itemGroup) {
                $scope.setPendingChanges();
                itemGroup.showPriceOptions = false;
                if (itemGroup.referencePrice == 0) return;
                itemGroup.priceType = "R";
                itemGroup.price = itemGroup.referencePrice;
                $scope.calculateTotals(itemGroup, "R");
            }
            $scope.setFixedPrice = function (itemGroup) {
                itemGroup.showPriceOptions = false;
                if (itemGroup.fixedPrice == 0) return;
                itemGroup.priceType = "F";
                itemGroup.price = itemGroup.fixedPrice;
                $scope.calculateTotals(itemGroup, "F");
            }
            $scope.setTemplatePrice = function (itemGroup) {
                $scope.setPendingChanges();
                itemGroup.showPriceOptions = false;
                if (itemGroup.templatePrice == 0) return;
                itemGroup.priceType = "T";
                itemGroup.price = itemGroup.templatePrice;
                $scope.calculateTotals(itemGroup, "T");
            }
            $scope.setStatewidePrice = function (itemGroup) {
                $scope.setPendingChanges();
                itemGroup.showPriceOptions = false;
                if (itemGroup.statewidePrice == 0) return;
                itemGroup.priceType = "S";
                itemGroup.price = itemGroup.statewidePrice;
                $scope.calculateTotals(itemGroup, "S");
            }
            $scope.setMarketAreaPrice = function (itemGroup) {
                $scope.setPendingChanges();
                itemGroup.showPriceOptions = false;
                if (itemGroup.marketAreaPrice == 0) return;
                itemGroup.priceType = "M";
                itemGroup.price = itemGroup.marketAreaPrice;
                $scope.calculateTotals(itemGroup, "M");
            }
            $scope.setCountyPrice = function (itemGroup) {
                $scope.setPendingChanges();
                itemGroup.showPriceOptions = false;
                if (itemGroup.countyPrice == 0) return;
                itemGroup.priceType = "C";
                itemGroup.price = itemGroup.countyPrice;
                $scope.calculateTotals(itemGroup, "C");
            }
            $scope.setParameterPrice = function (itemGroup) {
                $scope.setPendingChanges();
                itemGroup.showPriceOptions = false;
                if (itemGroup.parameterPrice == 0) return;
                itemGroup.priceType = "P";
                itemGroup.price = itemGroup.parameterPrice;
                $scope.calculateTotals(itemGroup, "P");
            }
            //$scope.applyParameterPrice = function (itemGroup) {
            //    itemGroup.parameterPrice = itemGroup.history.average;
            //    $scope.closePricingParameters(itemGroup);
            //    itemGroup.price = itemGroup.parameterPrice;
            //    $scope.calculateTotals(itemGroup, "P");
            //}
            $scope.closePricingParameters = function (itemGroup) {
                itemGroup.showPricingParameters = !itemGroup.showPricingParameters;
                itemGroup.history = null;
            }
            $scope.parmSet = {
                bidMonths: 36,
                useStraightAverage: false,
                includeLsDbEstimates: false,
                includeSvEstimates: false,
                contractNumberFilters: '',
                bidFilter: 'ALL',
                numberOfBids: 0
                //selectedBidders: [],
                //bidderTypeahead: null
            }
            $scope.generateParameterPrices = function () {
                $scope.setPendingChanges();
                var parms = {
                    contractType: $scope.parmSet.contractType,
                    workTypes: $scope.workTypes,
                    marketAreas: $scope.marketAreas.marketAreas,
                    county: $scope.pricingLevel == 'project' ? $scope.estimate.project.county : $scope.estimate.proposal.county,
                    bidMonths: $scope.parmSet.bidMonths,
                    useStraightAverage: $scope.parmSet.useStraightAverage,
                    includeLsDbEstimates: $scope.parmSet.includeLsDbEstimates,
                    includeSvEstimates: $scope.parmSet.includeSvEstimates,
                    contractNumberFilters: $scope.parmSet.contractNumberFilters,
                    //selectedBidders: $scope.parmSet.selectedBidders,
                    //bidderTypeahead: $scope.parmSet.bidderTypeahead,
                    bidFilter: $scope.parmSet.bidFilter,
                    numberOfBids: $scope.parmSet.numberOfBids
                }
                if ($scope.estimate != null && $scope.estimate != undefined && $scope.estimate.itemGroups != null && $scope.estimate.itemGroups != undefined) {
                    var itemGroups = $scope.estimate.itemGroups;
                    for (var i = 0; i < itemGroups.length; i++) {
                        if (itemGroups[i].unit == 'LS - Lump Sum' && itemGroups[i].quantity == 1) {
                            itemGroups[i].processed = true;
                        } else if (!itemGroups[i].group || !itemGroups[i].selected || itemGroups[i].isFixedPrice) {
                            itemGroups[i].processed = true;
                        } else {
                            itemGroups[i].processed = false;
                            itemGroups[i].asyncPricing = true;
                        }
                    }
                    $scope.canSaveEstimate = false;
                    processItemParameterPricing(itemGroups, parms);
                }
            }
            function processItemParameterPricing(itemGroups, parms) {
                var ig = null;
                //$scope.parmSet.contractNumbersListed = parseCommaSeparatedText($scope.parmSet.contractNumberFilters);
                for (var i = 0; i < itemGroups.length; i++) {
                    if (!itemGroups[i].processed) {
                        itemGroups[i].processed = true;
                        ig = itemGroups[i];
                        break;
                    }
                }
                if (ig != null) {
                    parms.itemGroup = ig;
                    parms.specYear = $scope.pricingLevel == 'project' ? $scope.estimate.project.specYear : $scope.estimate.proposal.specYear;
                    $http.post('./estimate/GenerateParameterPrices', parms).success(function (result) {
                        if (!containsDqeError(result)) {
                            ig.parameterPrice = getDqeData(result);
                            ig.price = ig.parameterPrice;
                            $scope.calculateTotals(ig, "P");
                            ig.asyncPricing = false;
                            processItemParameterPricing(itemGroups, parms);
                        } else {
                            for (var ii = 0; ii < itemGroups.length; ii++) {
                                itemGroups[ii].asyncPricing = false;
                            }
                            $scope.canSaveEstimate = true;
                        }
                    });
                } else {
                    $scope.canSaveEstimate = true;
                }          
            }
            function monthDiff(d1, d2) {
                return (d2.getDate() >= d1.getDate() ? 0 : -1) + ((d2.getFullYear() - d1.getFullYear()) * 12) + ((d2.getMonth() + 1) - (d1.getMonth() + 1));
            }
            function removeSuffix(str) {
                if (str.length >= 2 &&
                    (str.substring(str.length - 2).toUpperCase() === 'LS' ||
                        str.substring(str.length - 2).toUpperCase() === 'DB' ||
                        str.substring(str.length - 2).toUpperCase() === 'SV')) {
                    return str.substring(0, str.length - 2);
                }
                return str;
            }

            function containsSuffix(str, suffix) {
                if (str.length >= 2 &&
                    (str.substring(str.length - 2).toUpperCase() === suffix.toUpperCase())) {
                   /* console.log("proposal " + str + " has a " + suffix + " .");*/
                    return true;
                }
                return false;
            }
            function applyFilters(history) {
                if (history.proposals == null) {
                    return;
                }
                for (var i = 0; i < history.proposals.length; i++) {
                    for (var ii = 0; ii < history.proposals[i].bids.length; ii++) {
                        if (!history.proposals[i].bids[ii].blank) {
                            history.proposals[i].bids[ii].include = true;
                        }
                    }
                }

                for (i = 0; i < history.proposals.length; i++) {
                    var includeBids = true;
                    //contract type
                    if (history.contractType != 'all') {
                        includeBids = !history.proposals[i].contractType.startsWith('M');
                    }
                    //work type
                    if (includeBids) {
                        for (ii = 0; ii < history.workTypes.length; ii++) {
                            if (history.proposals[i].workType == history.workTypes[ii].code) {
                                includeBids = history.workTypes[ii].include;
                            }
                        }
                    }
                    //location
                    if (includeBids) {
                        includeBids = false;
                        for (ii = 0; ii < history.marketAreas.length; ii++) {
                            for (var iii = 0; iii < history.marketAreas[ii].counties.length; iii++) {
                                if (history.proposals[i].county == history.marketAreas[ii].counties[iii].name) {
                                    includeBids = history.marketAreas[ii].counties[iii].include;
                                }
                            }
                        }
                    }
                    //range
                    if (includeBids) {
                        var months = history.bidMonths == undefined ? 36 : history.bidMonths;
                        var today = new Date();
                        var diff = monthDiff(new Date(history.proposals[i].lettingAsDate + 'T10:20:30Z'), today);
                        includeBids = (diff < months);
                    }
                  
                    //check or uncheck Special Bids
                    if (containsSuffix(history.proposals[i].proposal, "LS") ||
                        containsSuffix(history.proposals[i].proposal, "DB")) {
                        for (ii = 0; ii < history.proposals[i].bids.length; ii++) {
                            if (!history.proposals[i].bids[ii].blank) {
                                history.proposals[i].bids[ii].include = history.includeLsDbEstimates && includeBids;
                            }

                        }
                    }
                    else if (containsSuffix(history.proposals[i].proposal, "SV")) {
                        for (ii = 0; ii < history.proposals[i].bids.length; ii++) {
                            if (!history.proposals[i].bids[ii].blank) {
                                history.proposals[i].bids[ii].include = history.includeSvEstimates && includeBids;
                            }
                        }
                    }
                }
            }

            /////This function takes a string of text, turns it into a list of strings (delimited by commas)
            ////Ignores whitespace
            //function parseCommaSeparatedText(input) {
            //    if (!input || typeof input !== 'string' || input.trim() === '') {
            //        return [];
            //    }

            //    var items = input.split(',');
            //    var result = [];

            //    for (var i = 0; i < items.length; i++) {
            //        var trimmed = items[i].trim();
            //        if (trimmed !== '') {
            //            var trimmed = items[i].trim().toUpperCase();
            //        }
            //    }

            //    return result;
            //}
            function updateBidHistory(itemGroup, county) {
                itemGroup.history.omitOutliers = true;
                if (itemGroup.history.useStraightAverage == undefined) itemGroup.history.useStraightAverage = false;
                var itemToPrice = $scope.isPricingInterface ? {
                    itemGroup: itemGroup,
                    county: county,
                    specYear: $scope.pricingLevel == 'project' ? $scope.estimate.project.specYear : $scope.estimate.proposal.specYear
                } : {
                    itemGroup: itemGroup,
                    county: county
                }
                $http.post('./estimate/UpdateBidHistory', itemToPrice).success(function (result) {
                    if (!containsDqeError(result)) {
                        if (result.data == null) {
                            itemGroup.history.average = 0;
                        } else {
                            var hist = getDqeData(result);
                            itemGroup.history.average = hist.average;
                            for (var i = 0; i < hist.proposals.length; i++) {
                                for (var x = 0; x < itemGroup.history.proposals.length; x++) {
                                    if (hist.proposals[i].id == itemGroup.history.proposals[x].id) {
                                        itemGroup.history.proposals[x] = hist.proposals[i];
                                        break;
                                    }
                                }
                            }
                        }
                    }
                });
            }

            $scope.showPricingParameters = function (itemGroup, county) {
                for (var i = 0; i < $scope.estimate.itemGroups.length; i++) {
                    $scope.estimate.itemGroups[i].showPricingParameters = false;
                    $scope.estimate.itemGroups[i].history = null;
                }
                if (!itemGroup.showPricingParameters) {
                    var itemToPrice = {
                        itemGroup: itemGroup,
                        county: county,
                        specYear: $scope.pricingLevel == 'project' ? $scope.estimate.project.specYear : $scope.estimate.proposal.specYear
                    }

                    $http.post('./estimate/GetBidHistory', itemToPrice).success(function (result) {
                        if (!containsDqeError(result)) {
                            itemGroup.history = getDqeData(result);
                            //itemGroup.history.contractType = 'all';

                            itemGroup.history.contractType = $scope.parmSet.contractType;
                            //itemGroup.history.bidderTypeahead = $scope.parmSet.bidderTypeahead;

                            itemGroup.history.bidMonths = $scope.parmSet.bidMonths;

                            itemGroup.history.includeLsDbEstimates = $scope.parmSet.includeLsDbEstimates;
                            itemGroup.history.includeSvEstimates = $scope.parmSet.includeSvEstimates;
                            itemGroup.history.useStraightAverage = $scope.parmSet.useStraightAverage;
                            itemGroup.history.compoundingOutlierMultiplier = 1;


                            itemGroup.history.workTypes = [];
                            for (i = 0; i < $scope.workTypes.length; i++) {
                                itemGroup.history.workTypes.push({
                                    name: $scope.workTypes[i].name,
                                    code: $scope.workTypes[i].code,
                                    include: $scope.workTypes[i].include
                            });
                            }
                            itemGroup.history.marketAreas = [];
                            for (i = 0; i < $scope.marketAreas.marketAreas.length; i++) {
                                var ma = {
                                    id: $scope.marketAreas.marketAreas[i].id,
                                    name: $scope.marketAreas.marketAreas[i].name,
                                    include: $scope.marketAreas.marketAreas[i].include,
                                    counties: []
                                };
                                itemGroup.history.marketAreas.push(ma);
                                for (var ii = 0; ii < $scope.marketAreas.marketAreas[i].counties.length; ii++) {
                                    var c = {
                                        id: $scope.marketAreas.marketAreas[i].counties[ii].id,
                                        name: $scope.marketAreas.marketAreas[i].counties[ii].name,
                                        code: $scope.marketAreas.marketAreas[i].counties[ii].code,
                                        include: $scope.marketAreas.marketAreas[i].counties[ii].include
                                    };
                                    ma.counties.push(c);
                                }
                            }

                            applyFilters(itemGroup.history);
                            updateBidHistory(itemGroup, county);


                        }
                    });
                }
                itemGroup.showPricingParameters = !itemGroup.showPricingParameters;
            }

            $scope.viewCurrentProposalSummaryReport = function() {
                //document.getElementById("hiddenProposalId").value = $scope.estimate.proposal.id;
                document.getElementById("hiddenProposalNumber").value = $scope.estimate.proposal.number;
                var params = {
                    proposalNumber: $scope.estimate.proposal.number,
                    proposalId: $scope.estimate.proposal.id
                };
                $http.post('./report/SetupWorkingProposalSummaryReport', params).success(function (result) {
                    if (!containsDqeError(result)) {
                        $.download('./report/ViewWorkingProposalSummaryReport', $('form#ViewWorkingProposalSummaryReport').serialize());
                    };
                });
            };

            $scope.reportFormat = {
                type: "PDF"
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
            //$scope.applyFilters = function (history) {
            //    for (var i = 0; i < history.proposals.length; i++) {
            //        var includeBids = true;
            //        //contract type
            //        if (history.contractType != 'all') {
            //            includeBids = !history.proposals[i].contractType.startsWith('M');
            //        }
            //        //work type
            //        if (includeBids) {
            //            for (var ii = 0; ii < history.workTypes.length; ii++) {
            //                if (history.proposals[i].workType == history.workTypes[ii].code) {
            //                    includeBids = history.workTypes[ii].include;
            //                }
            //            }
            //        }
            //        //location
            //        if (includeBids) {
            //            includeBids = false;
            //            for (ii = 0; ii < history.marketAreas.length; ii++) {
            //                for (var iii = 0; iii < history.marketAreas[ii].counties.length; iii++) {
            //                    if (history.proposals[i].county == history.marketAreas[ii].counties[iii].name) {
            //                        includeBids = history.marketAreas[ii].counties[iii].include;
            //                    }
            //                }
            //            }
            //        }
            //        //check or uncheck bids
            //        for (ii = 0; ii < history.proposals[i].bids.length; ii++) {
            //            if (!history.proposals[i].bids[ii].blank) {
            //                history.proposals[i].bids[ii].include = includeBids;
            //            }
            //        }
            //    }
            //}
            $scope.selectMarketArea = function (marketArea, history) {
                if (marketArea.include) {
                    for (var i = 0; i < marketArea.counties.length; i++) {
                        marketArea.counties[i].include = true;
                    }
                } else {
                    for (i = 0; i < marketArea.counties.length; i++) {
                        marketArea.counties[i].include = false;
                    }
                }
                if (history == null) return;
                $scope.applyFilters(history);
            }
            $scope.setMarketAreaSelection = function (marketArea, history) {
                marketArea.include = false;
                for (var i = 0; i < marketArea.counties.length; i++) {
                    if (marketArea.counties[i].include) {
                        marketArea.include = true;
                        break;
                    }
                }
                if (history == null) return;
                $scope.applyFilters(history);
            }
            $scope.partialMarketAreaSelected = function (marketArea) {
                var hasIncluded = false;
                var hasExcluded = false;
                for (var i = 0; i < marketArea.counties.length; i++) {
                    if (!hasIncluded && marketArea.counties[i].include) {
                        hasIncluded = true;
                    }
                    if (!hasExcluded && !marketArea.counties[i].include) {
                        hasExcluded = true;
                    }
                    if (hasIncluded && hasExcluded) return true;
                }
                return false;
            }
            //$scope.updateBidHistory = function (itemGroup, county) {
            //    if (!itemGroup.history.omitOutliers) {
            //        itemGroup.history.omitOutliers = false;
            //    }
            //    var itemToPrice = {
            //        itemGroup: itemGroup,
            //        county: county,
            //        specYear: $scope.pricingLevel == 'project' ? $scope.estimate.project.specYear : $scope.estimate.proposal.specYear
            //    }
            //    $http.post('./estimate/UpdateBidHistory', itemToPrice).success(function (result) {
            //        if (!containsDqeError(result)) {
            //            if (result.data == null) {
            //                itemGroup.history.average = 0;
            //            } else {
            //                var hist = getDqeData(result);
            //                itemGroup.history.average = hist.average;
            //                for (var i = 0; i < hist.proposals.length; i++) {
            //                    for (var x = 0; x < itemGroup.history.proposals.length; x++) {
            //                        if (hist.proposals[i].id == itemGroup.history.proposals[x].id) {
            //                            itemGroup.history.proposals[x] = hist.proposals[i];
            //                            break;
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    });
            //}
            //$scope.omitOrIncludeProposal = function (history, proposal) {
            //    var proposalReached = false;
            //    var l = $filter('orderBy')(history.proposals, history.sort, history.sortReverse);
            //    for (var i = 0; i < l.length; i++) {
            //        if (l[i] == proposal) {
            //            proposalReached = true;
            //        }
            //        if (proposalReached && l[i] != proposal) {
            //            for (var ii = 0; ii < l[i].bids.length; ii++) {
            //                if (!l[i].bids[ii].blank) {
            //                    l[i].bids[ii].include = false;
            //                }
            //            }
            //        }
            //    }
            //}
            //$scope.omitOrIncludeBidder = function (history, bidder) {
            //    var bidderReached = false;
            //    var targetBidder = 0;
            //    for (var x = 0; x < history.maxBiddersProposal.bids.length; x++) {
            //        if (history.maxBiddersProposal.bids[x] == bidder && !bidderReached) {
            //            targetBidder = x;
            //            bidderReached = true;
            //        }
            //    }
            //    for (var i = 0; i < history.proposals.length; i++) {
            //        for (var ii = 0; ii < history.proposals[i].bids.length; ii++) {
            //            if (!history.proposals[i].bids[ii].blank) {
            //                if (ii > targetBidder && history.proposals[i].bids[ii].include) {
            //                    history.proposals[i].bids[ii].include = false;
            //                }
            //            }
            //        }
            //    }
            //}
            //$scope.sortHistory = function (history, prop) {
            //    history.sort = prop;
            //    history.sortReverse = history.lastSort == prop ? !history.sortReverse : false;
            //    history.lastSort = prop;
            //}
            //$scope.selectAll = function (history) {
            //    for (var i = 0; i < history.proposals.length; i++) {
            //        for (var ii = 0; ii < history.proposals[i].bids.length; ii++) {
            //            if (!history.proposals[i].bids[ii].blank) {
            //                history.proposals[i].bids[ii].include = true;
            //            }
            //        }
            //    }
            //}
        }
    ]}
});