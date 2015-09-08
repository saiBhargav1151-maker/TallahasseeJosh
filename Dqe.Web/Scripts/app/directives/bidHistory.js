dqeDirectives.directive('bidHistory', function () {
    return {
        restrict: 'E',
        scope: {
            pricingLevel: '=',
            calculateTotals: '=',
            closePricingParameters: '=',
            itemGroup: '=',
            estimate: '=',
            isPricingInterface: '='
        },
        templateUrl: './Views/directives/bidHistory.html',
        controller: ['$scope', '$rootScope', '$http', '$filter', function ($scope, $rootScope, $http, $filter) {

            $scope.currentPage = 0;
            $scope.pageSize = 10;
            $scope.setCurrentPage = function (currentPage) {
                $scope.currentPage = currentPage;
            }
            $scope.getNumberAsArray = function (num) {
                return new Array(num);
            };
            $scope.numberOfPages = function () {
                if ($scope.itemGroup == undefined || $scope.itemGroup == null) return 0;
                if ($scope.itemGroup.history == undefined || $scope.itemGroup.history == null) return 0;
                if ($scope.itemGroup.history.proposals == undefined || $scope.itemGroup.history.proposals == null) return 0;
                return Math.ceil($scope.itemGroup.history.proposals.length / $scope.pageSize);
            };

            $scope.monthRange = [];
            for (var r = 0; r < 36; r++) {
                $scope.monthRange.push(r + 1);
            }
            $scope.setParameterPrice = function (itemGroup) {
                itemGroup.showPriceOptions = false;
                if (itemGroup.parameterPrice == 0) return;
                itemGroup.priceType = "P";
                itemGroup.price = itemGroup.parameterPrice;
                $scope.calculateTotals(itemGroup, "P");
            }
            $scope.applyParameterPrice = function (itemGroup) {
                $scope.estimate.hasPendingChanges = true;
                itemGroup.parameterPrice = itemGroup.history.average;
                $scope.closePricingParameters(itemGroup);
                itemGroup.price = itemGroup.parameterPrice;
                $scope.calculateTotals(itemGroup, "P");
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
            function monthDiff(d1, d2) {
                return (d2.getDate() >= d1.getDate() ? 0 : -1) + ((d2.getFullYear() - d1.getFullYear()) * 12) + ((d2.getMonth() + 1) - (d1.getMonth() + 1));
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
                    //range
                    if (includeBids) {
                        var months = history.bidMonths == undefined ? 36 : history.bidMonths;
                        var today = new Date();
                        var diff = monthDiff(new Date(history.proposals[i].lettingAsDate + 'T10:20:30Z'), today);
                        includeBids = (diff < months);
                    }
                    //check or uncheck bids
                    for (ii = 0; ii < history.proposals[i].bids.length; ii++) {
                        if (!history.proposals[i].bids[ii].blank) {
                            history.proposals[i].bids[ii].include = includeBids;
                            if (history.proposals[i].bids[ii].include) {
                                if (history.proposals[i].bids[ii].estimate) {
                                    history.proposals[i].bids[ii].include = history.includeEstimates;
                                }
                            }
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
            $scope.setHistoryRange = function (history) {
                if (history == null) return;
                $scope.applyFilters(history);
            }
            $scope.toggleIncludeEstimates = function(history) {
                if (history == null) return;
                $scope.applyFilters(history);
            }
            $scope.updateBidHistory = function (itemGroup, county) {
                if (!itemGroup.history.omitOutliers) {
                    itemGroup.history.omitOutliers = false;
                }
                if (!itemGroup.history.useStraightAverage) {
                    itemGroup.history.useStraightAverage = false;
                }
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
                //estimates
                history.includeEstimates = true;
                //contract type
                history.contractType = 'all';
                //work type
                for (var i = 0; i < history.workTypes.length; i++) {
                    history.workTypes[i].include = true;
                }
                //location
                for (i = 0; i < history.marketAreas.length; i++) {
                    history.marketAreas[i].include = true;
                    for (ii = 0; ii < history.marketAreas[i].counties.length; ii++) {
                        history.marketAreas[i].counties[ii].include = true;
                    }
                }
                //range
                history.bidMonths = 36;
                for (i = 0; i < history.proposals.length; i++) {
                    for (var ii = 0; ii < history.proposals[i].bids.length; ii++) {
                        if (!history.proposals[i].bids[ii].blank) {
                            history.proposals[i].bids[ii].include = true;
                        }
                    }
                }
            }
        }
    ]}
});