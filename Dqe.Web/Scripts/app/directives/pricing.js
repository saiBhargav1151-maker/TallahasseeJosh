dqeDirectives.directive('pricing', function () {
    return {
        restrict: 'E',
        scope: {
            pricingLevel: '='
        },
        templateUrl: './Views/directives/pricing.html',
        controller: function ($scope, $rootScope, $http, $filter, $location, stateService) {
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
            var loadLink = $scope.pricingLevel == 'project' ? './estimate/LoadProjectEstimate' : './estimate/LoadProposalEstimate';
            var loadSummaryLink = $scope.pricingLevel == 'project' ? './estimate/LoadProjectEstimateSummary' : './estimate/LoadProposalEstimateSummary';
            var loadId = $scope.pricingLevel == 'project' ? stateService.currentEstimateId : stateService.currentProposalId;
            if (loadId == '') {
                $location.url($scope.pricingLevel == 'project' ? '/home_project' : '/home_proposal');
            } else {

                //test
                //$http.post('./estimate/TestReport').success(function (result) {
                //    var rpt = getDqeData(result);
                //});
                //end test


                $http.post(loadLink, { loadId: loadId }).success(function (result) {
                    if (!containsDqeError(result)) {
                        $scope.estimate = getDqeData(result);
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
                        $http.get('./estimate/GetWorkTypes').success(function (res) {
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
                        $scope.saveEstimate();
                        //$http.post(loadSummaryLink, { estimateId: $scope.estimate.estimateId }).success(function (r) {
                        //    if (!containsDqeError(r)) {
                        //        $scope.estimate.total = getDqeData(r).total;
                        //        buildSummary();
                        //    }
                        //});
                        $scope.contractType = 'all';
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
                            priceType: ''
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
                                priceType: ''
                            };
                        $scope.filterItems();
                        buildSummary();
                        $scope.canSaveEstimate = true;
                    }
                });
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
                        combineCategories: $scope.filter.combineCategories,
                        combineItems: $scope.filter.combineItems,
                        categoryAlternateSet: $scope.filter.categoryAlternateSet,
                        categoryAlternateMember: $scope.filter.categoryAlternateMember,
                        itemAlternateSet: $scope.filter.itemAlternateSet,
                        itemAlternateMember: $scope.filter.itemAlternateMember,
                        itemDescription: $scope.filter.itemDescription,
                        fund: $scope.filter.fund,
                        supplementalDescription: $scope.filter.supplementalDescription,
                        unit: $scope.filter.unit,
                        priceType: $scope.filter.priceType
                    } :
                    {
                        federalConstructionClass: $scope.filter.federalConstructionClass,
                        projectNumber: $scope.filter.projectNumber,
                        itemNumber: $scope.filter.itemNumber,
                        combineCategories: $scope.filter.combineCategories,
                        combineItems: $scope.filter.combineItems,
                        categoryAlternateSet: $scope.filter.categoryAlternateSet,
                        categoryAlternateMember: $scope.filter.categoryAlternateMember,
                        itemAlternateSet: $scope.filter.itemAlternateSet,
                        itemAlternateMember: $scope.filter.itemAlternateMember,
                        itemDescription: $scope.filter.itemDescription,
                        fund: $scope.filter.fund,
                        supplementalDescription: $scope.filter.supplementalDescription,
                        unit: $scope.filter.unit,
                        priceType: $scope.filter.priceType
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
                $scope.filteredItems = $filter('orderBy')($scope.filteredItems, sortArray, $scope.sortItemReverse);
                for (var i = 0; i < $scope.filteredItems.length; i++) {
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
            $scope.calculateTotals = function (itemGroup, overridePriceType) {
                if (!isNumber(itemGroup.price)) {
                    itemGroup.price = 0;
                }
                if (itemGroup.price < 0) {
                    itemGroup.price = 0;
                }
                if (itemGroup.price == 0) {
                    itemGroup.priceType = "N";
                } else {
                    itemGroup.priceType = overridePriceType;
                }
                if (itemGroup.group) {
                    angular.forEach($scope.estimate.itemGroups, function (i) {
                        if (i.key == itemGroup.key && !i.group) {
                            i.price = itemGroup.price;
                            if (i.price == 0) {
                                i.priceType = "N";
                            } else {
                                i.priceType = overridePriceType;
                            }
                        }
                    });
                }
            }
            $scope.saveEstimate = function () {
                if ($scope.estimate == undefined
                    || $scope.estimate == null
                    ) return;
                $scope.estimate.isSystemSync = false;
                var saveLink = $scope.pricingLevel == 'project' ? './estimate/SaveEstimate' : './estimate/SaveProposalEstimate';
                


                var est = {
                    isSystemSync: $scope.estimate.isSystemSync,
                    itemGroups: []
                }
                for (var i = 0; i < $scope.estimate.itemGroups.length; i++) {
                    est.itemGroups.push({
                        itemId: $scope.estimate.itemGroups[i].itemId,
                        price: $scope.estimate.itemGroups[i].price,
                        isSystemOverride: $scope.estimate.itemGroups[i].isSystemOverride,
                        priceType: $scope.estimate.itemGroups[i].priceType
                    });
                }
                $http.post(saveLink, est).success(function (result) {
                    if (!containsDqeError(result)) {
                        for (var x = 0; x < $scope.estimate.itemGroups.length; x++) {
                            var itemGroup = $scope.estimate.itemGroups[x];
                            itemGroup.holdPriceType = itemGroup.priceType;
                            itemGroup.holdPrice = itemGroup.price;
                            if (itemGroup.group) {
                                angular.forEach($scope.estimate.itemGroups, function (ii) {
                                    if (ii.key == itemGroup.key && !ii.group) {
                                        ii.priceType = itemGroup.priceType;
                                        ii.holdPrice = itemGroup.price;
                                    }
                                });
                            }
                        }
                        $http.post(loadSummaryLink, { estimateId: $scope.estimate.estimateId }).success(function (r) {
                            if (!containsDqeError(r)) {
                                $scope.estimate.total = getDqeData(r).total;
                                buildSummary();
                            }
                        });
                    }
                });
            }
            $scope.setBestPrices = function () {
                $scope.canSaveEstimate = false;
                angular.forEach($scope.estimate.itemGroups, function (i) {
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
            $scope.setStatewidePrice = function (itemGroup) {
                itemGroup.showPriceOptions = false;
                if (itemGroup.statewidePrice == 0) return;
                itemGroup.priceType = "S";
                itemGroup.price = itemGroup.statewidePrice;
                $scope.calculateTotals(itemGroup, "S");
            }
            $scope.setMarketAreaPrice = function (itemGroup) {
                itemGroup.showPriceOptions = false;
                if (itemGroup.marketAreaPrice == 0) return;
                itemGroup.priceType = "M";
                itemGroup.price = itemGroup.marketAreaPrice;
                $scope.calculateTotals(itemGroup, "M");
            }
            $scope.setCountyPrice = function (itemGroup) {
                itemGroup.showPriceOptions = false;
                if (itemGroup.countyPrice == 0) return;
                itemGroup.priceType = "C";
                itemGroup.price = itemGroup.countyPrice;
                $scope.calculateTotals(itemGroup, "C");
            }
            $scope.setParameterPrice = function (itemGroup) {
                itemGroup.showPriceOptions = false;
                if (itemGroup.parameterPrice == 0) return;
                itemGroup.priceType = "P";
                itemGroup.price = itemGroup.parameterPrice;
                $scope.calculateTotals(itemGroup, "P");
            }
            $scope.applyParameterPrice = function (itemGroup) {
                itemGroup.parameterPrice = itemGroup.history.average;
                $scope.closePricingParameters(itemGroup);
                itemGroup.price = itemGroup.parameterPrice;
                $scope.calculateTotals(itemGroup, "P");
            }
            $scope.closePricingParameters = function (itemGroup) {
                itemGroup.showPricingParameters = !itemGroup.showPricingParameters;
                itemGroup.history = null;
            }
            $scope.generateParameterPrices = function () {
                var parms = {
                    contractType: $scope.contractType,
                    workTypes: $scope.workTypes,
                    marketAreas: $scope.marketAreas.marketAreas,
                    county: $scope.pricingLevel == 'project' ? $scope.estimate.project.county : $scope.estimate.proposal.county
                }
                if ($scope.estimate != null && $scope.estimate != undefined && $scope.estimate.itemGroups != null && $scope.estimate.itemGroups != undefined) {
                    var itemGroups = $scope.estimate.itemGroups;
                    for (var i = 0; i < itemGroups.length; i++) {
                        if (!itemGroups[i].group || !itemGroups[i].selected || itemGroups[i].isFixedPrice) {
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
                            itemGroup.history.contractType = 'all';
                            itemGroup.history.workTypes = [];
                            for (i = 0; i < $scope.workTypes.length; i++) {
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
            $scope.applyFilters = function (history) {
                for (var i = 0; i < history.proposals.length; i++) {
                    var includeBids = true;
                    //contract type
                    if (history.contractType != 'all') {
                        includeBids = !history.proposals[i].contractType.startsWith('M');
                    }
                    //work type
                    if (includeBids) {
                        for (var ii = 0; ii < history.workTypes.length; ii++) {
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
                    //check or uncheck bids
                    for (ii = 0; ii < history.proposals[i].bids.length; ii++) {
                        if (!history.proposals[i].bids[ii].blank) {
                            history.proposals[i].bids[ii].include = includeBids;
                        }
                    }
                }
            }
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
            $scope.updateBidHistory = function (itemGroup, county) {
                if (!itemGroup.history.omitOutliers) {
                    itemGroup.history.omitOutliers = false;
                }
                var itemToPrice = {
                    itemGroup: itemGroup,
                    county: county,
                    specYear: $scope.pricingLevel == 'project' ? $scope.estimate.project.specYear : $scope.estimate.proposal.specYear
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
            $scope.omitOrIncludeProposal = function (history, proposal) {
                var proposalReached = false;
                var l = $filter('orderBy')(history.proposals, history.sort, history.sortReverse);
                for (var i = 0; i < l.length; i++) {
                    if (l[i] == proposal) {
                        proposalReached = true;
                    }
                    if (proposalReached && l[i] != proposal) {
                        for (var ii = 0; ii < l[i].bids.length; ii++) {
                            if (!l[i].bids[ii].blank) {
                                l[i].bids[ii].include = false;
                            }
                        }
                    }
                }
            }
            $scope.omitOrIncludeBidder = function (history, bidder) {
                var bidderReached = false;
                var targetBidder = 0;
                for (var x = 0; x < history.maxBiddersProposal.bids.length; x++) {
                    if (history.maxBiddersProposal.bids[x] == bidder && !bidderReached) {
                        targetBidder = x;
                        bidderReached = true;
                    }
                }
                for (var i = 0; i < history.proposals.length; i++) {
                    for (var ii = 0; ii < history.proposals[i].bids.length; ii++) {
                        if (!history.proposals[i].bids[ii].blank) {
                            if (ii > targetBidder && history.proposals[i].bids[ii].include) {
                                history.proposals[i].bids[ii].include = false;
                            }
                        }
                    }
                }
            }
            $scope.sortHistory = function (history, prop) {
                history.sort = prop;
                history.sortReverse = history.lastSort == prop ? !history.sortReverse : false;
                history.lastSort = prop;
            }
            $scope.selectAll = function (history) {
                for (var i = 0; i < history.proposals.length; i++) {
                    for (var ii = 0; ii < history.proposals[i].bids.length; ii++) {
                        if (!history.proposals[i].bids[ii].blank) {
                            history.proposals[i].bids[ii].include = true;
                        }
                    }
                }
            }
        }
    }
});