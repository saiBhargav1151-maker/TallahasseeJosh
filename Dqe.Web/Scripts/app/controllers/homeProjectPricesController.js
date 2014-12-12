dqeControllers.controller('HomeProjectPricesController', ['$scope', '$rootScope', '$http', function ($scope, $rootScope, $http) {
    $rootScope.$broadcast('initializeNavigation');
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
    $http.get('./estimate/LoadProjectEstimate').success(function (result) {
        if (!containsDqeError(result)) {
            $scope.estimate = getDqeData(result);
            $scope.estimate.locked = false;
            buildSummary();
        }
    });
    $scope.toggleGroup = function(itemGroup) {
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
        $http.post('./estimate/SaveEstimate', $scope.estimate).success(function(result) {
            if (!containsDqeError(result)) {

                for (var x = 0; x < $scope.estimate.itemGroups.length; x++) {
                    var itemGroup = $scope.estimate.itemGroups[x];
                    itemGroup.holdPriceType = itemGroup.priceType;
                    itemGroup.holdPrice = itemGroup.price;
                    if (itemGroup.group) {
                        angular.forEach($scope.estimate.itemGroups, function (i) {
                            if (i.key == itemGroup.key && !i.group) {
                                i.priceType = itemGroup.priceType;
                                i.holdPrice = itemGroup.price;
                            }
                        });
                    }
                }

                $http.get('./estimate/LoadProjectEstimateSummary').success(function(r) {
                    if (!containsDqeError(r)) {
                        $scope.estimate.total = getDqeData(r).total;
                        buildSummary();
                    }
                });
            }
        });
    }
    $scope.showPriceOptions = function (itemGroup) {
        angular.forEach($scope.estimate.itemGroups, function (i) {
            if (itemGroup != i) {
                i.showPriceOptions = false;
            }
        });
        itemGroup.showPriceOptions = !itemGroup.showPriceOptions;
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
    $scope.savePricingParameters = function (itemGroup) {
        //save parameters and bids and purge binding
        itemGroup.priceType = "P";
        itemGroup.holdPrice = itemGroup.price;
        itemGroup.holdPriceType = itemGroup.priceType;
        if (itemGroup.group) {
            angular.forEach($scope.estimate.itemGroups, function (i) {
                if (i.key == itemGroup.key && !i.group) {
                    i.priceType = itemGroup.priceType;
                    i.holdPrice = itemGroup.price;
                    i.holdPriceType = itemGroup.priceType;
                }
            });
        }
        $scope.calculateBidHistoryPrice(itemGroup);
        $http.post('./estimate/SaveBidHistory', itemGroup).success(function (result) {
            if (!containsDqeError(result)) {
                itemGroup.history = null;
                $http.get('./estimate/LoadProjectEstimateSummary').success(function (r) {
                    if (!containsDqeError(r)) {
                        $scope.estimate.total = getDqeData(r).total;
                        buildSummary();
                    }
                });
            }
        });
        itemGroup.showPricingParameters = !itemGroup.showPricingParameters;
    }
    $scope.cancelPricingParameters = function (itemGroup) {
        //purge binding
        //if (itemGroup.priceType == "P" || itemGroup.priceType == "N") {
            
            itemGroup.price = itemGroup.holdPrice;
            itemGroup.priceType = itemGroup.holdPriceType;
            if (itemGroup.group) {
                angular.forEach($scope.estimate.itemGroups, function (i) {
                    if (i.key == itemGroup.key && !i.group) {
                        i.price = itemGroup.holdPrice;
                        i.priceType = itemGroup.holdPriceType;
                    }
                });
            }
        //}
        itemGroup.history = null;
        itemGroup.showPricingParameters = !itemGroup.showPricingParameters;
    }
    $scope.showPricingParameters = function (itemGroup) {
        if (!itemGroup.showPricingParameters/* && itemGroup.priceType == "P"*/) {
            //get parameters and bids
            $http.post('./estimate/GetBidHistory', itemGroup).success(function (result) {
                if (!containsDqeError(result)) {
                    itemGroup.history = getDqeData(result);
                }
            });
        }
        itemGroup.showPricingParameters = !itemGroup.showPricingParameters;
    }
    $scope.getBidHistory = function(itemGroup) {
        $http.post('./pricingengine/CalculateBidHistory', itemGroup).success(function (result) {
            if (!containsDqeError(result)) {
                itemGroup.history = getDqeData(result);
                $scope.calculateBidHistoryPrice(itemGroup);
            }
        });
    }
    $scope.getProjectBidHistory = function () {
        for (var i = 0; i < $scope.estimate.itemGroups.length; i++) {
            var itemGroup = $scope.estimate.itemGroups[i];
            itemGroup.asyncPricing = itemGroup.group && !itemGroup.locked;
        }
        rollCalculation();
    }
    function rollCalculation() {
        var itemGroup = null;
        for (var i = 0; i < $scope.estimate.itemGroups.length; i++) {
            itemGroup = $scope.estimate.itemGroups[i];
            if (itemGroup.asyncPricing) {
                break;
            } else {
                itemGroup = null;
            }
        }
        if (itemGroup != null) {
            getItemHistory(itemGroup);
        } else {
            $http.get('./estimate/LoadProjectEstimateSummary').success(function (r) {
                if (!containsDqeError(r)) {
                    $scope.estimate.total = getDqeData(r).total;
                    buildSummary();
                }
            });
        }
    }
    function getItemHistory(itemGroup) {
        $http.post('./pricingengine/AsyncCalculateBidHistory', itemGroup).success(function (result) {
            if (!containsDqeError(result)) {
                itemGroup.history = getDqeData(result);
                itemGroup.priceType = "P";
                if (itemGroup.group) {
                    angular.forEach($scope.estimate.itemGroups, function (i) {
                        if (i.key == itemGroup.key && !i.group) {
                            i.priceType = "P";           
                        }
                    });
                }
                $scope.calculateBidHistoryPrice(itemGroup);
                itemGroup.holdPrice = itemGroup.price;
                itemGroup.holdPriceType = itemGroup.priceType;
                if (itemGroup.group) {
                    angular.forEach($scope.estimate.itemGroups, function (i) {
                        if (i.key == itemGroup.key && !i.group) {
                            i.holdPrice = itemGroup.price;
                            i.holdPriceType = itemGroup.priceType;
                        }
                    });
                }
                $http.post('./estimate/AsyncSaveBidHistory', itemGroup).success(function (res) {
                    if (!containsDqeError(res)) {
                        itemGroup.history = null;
                        itemGroup.asyncPricing = false;
                        rollCalculation();
                    }
                });
            }
        });
    }
    $scope.calculateBidHistoryPrice = function (itemGroup) {
        itemGroup.history.averagePrice = 0;
        var totalBids = 0;
        var totalPrice = 0;
        for (var i = 0; i < itemGroup.history.proposals.length; i++) {
            if (itemGroup.history.proposals[i].include) {
                for (var ii = 0; ii < itemGroup.history.proposals[i].bids.length; ii++) {
                    if (itemGroup.history.proposals[i].bids[ii].include) {
                        totalBids += 1;
                        totalPrice += itemGroup.history.proposals[i].bids[ii].price;
                    }
                }
            }
        }
        itemGroup.history.averagePrice = totalBids == 0 ? 0 : totalPrice / totalBids;
        itemGroup.parameterPrice = (itemGroup.history.averagePrice).toFixed(2);
        //if (itemGroup.priceType == "P") {
        //    itemGroup.price = itemGroup.parameterPrice;
        //    if (itemGroup.group) {
        //        angular.forEach($scope.estimate.itemGroups, function (x) {
        //            if (x.key == itemGroup.key && !x.group) {
        //                x.price = itemGroup.parameterPrice;
        //            }
        //        });
        //    }
        //}
        itemGroup.price = itemGroup.parameterPrice;
        if (itemGroup.group) {
            angular.forEach($scope.estimate.itemGroups, function (x) {
                if (x.key == itemGroup.key && !x.group) {
                    x.price = itemGroup.parameterPrice;
                }
            });
        }
    }
    $scope.omitOrIncludeBidders = function(proposal, itemGroup) {
        if (!proposal.include) {
            for (var i = 0; i < proposal.bids.length; i++) {
                proposal.bids[i].include = false;
            }
        } else {
            for (i = 0; i < proposal.bids.length; i++) {
                proposal.bids[i].include = true;
            }
        }
        $scope.calculateBidHistoryPrice(itemGroup);
    }
    $scope.toggleEstimateLock = function() {
        $scope.estimate.locked = !$scope.estimate.locked;
        for (var i = 0; i < $scope.estimate.itemGroups.length; i++) {
            $scope.estimate.itemGroups[i].locked = $scope.estimate.locked;
        }
    }
}]);