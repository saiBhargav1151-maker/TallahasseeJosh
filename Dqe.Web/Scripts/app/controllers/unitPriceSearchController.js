dqeControllers.controller('UnitPriceSearchController', ['$scope', '$rootScope', '$http', '$timeout',
    function ($scope, $rootScope, $http, $timeout) {
        $rootScope.$broadcast('initializeNavigation');
        $rootScope.showStatisticsDetails = true;
        $scope.searchText = "";
        $scope.items = [];
        $scope.selectedPayItemNumber = null;
        $scope.bidHistoryData = [];
        $scope.lastSearchedPayItem = $scope.searchText;
        $scope.isLoading = false;
        $scope.monthsOfHistory = 36;
        $scope.regionType = '';
        $scope.regionOptions = [];
        $scope.selectedRegions = [];
        $scope.relatedCounties = [];
        $scope.selectedRegionCounties = [];
        $scope.selectedMinBidAmount = null;
        $scope.selectedMaxBidAmount = null;
        $scope.isRegionDropdownOpen = false;
        $scope.selectedBidStatus = "FMV";
        $scope.searchAttempted = false;
        $scope.showNormal = true;
        $scope.showOutliers = true;
        $scope.showTrendLine = true;
        $scope.showWeightedAvg = true;
        $scope.sortColumn = 'p'; // Default sort by Contract
        $scope.reverseSort = false;
        let exportDebounceTimer;
        $scope.isChartLoading = false;
        $scope.isSearching = false; // New variable for search loading state
        $scope.showSuggestions = false; // New variable to control suggestions visibility
        const today = new Date();
        const pastLimit = new Date();
        pastLimit.setMonth(pastLimit.getMonth() - 120);
        $scope.today = today;
        $scope.minAllowedDate = pastLimit;
        $scope.trendAnalysisData = {
            trendTimeGrouping: "year"
        };
        $scope.trendData = [];
        $scope.trendChartInstance = null;
        $scope.isTrendChartLoading = false;
        $scope.showTrendChart = false;
        $scope.trendWarning = '';
        $scope.useInflationAdjustedPrices = true;
        $scope.isExporting = false;
        $scope.customQuantityData = {
            userQuantity: null
        };
        $scope.customQuantityPrediction = null;
        $scope.isCalculatingPrediction = false;

        // Column selection
        $scope.availableColumns = [
            { key: 'p', label: 'Contract', visible: true, sortable: true, selectionOrder: 1 },
            { key: 'ProjectNumber', label: 'Project Number', visible: true, sortable: true, selectionOrder: 2 },
            { key: 'ri', label: 'Pay Item', visible: true, sortable: true, selectionOrder: 3 },
            { key: 'Description', label: 'Description', visible: false, sortable: true, selectionOrder: 0 },
            { key: 'SupplementalDescription', label: 'Supp Desc', visible: false, sortable: false, selectionOrder: 0 },
            { key: 'CalculatedUnit', label: 'Units', visible: false, sortable: false, selectionOrder: 0 },
            { key: 'Quantity', label: 'Quantity', visible: true, sortable: true, selectionOrder: 4 },
            { key: 'b', label: 'Unit Price Bid', visible: true, sortable: true, selectionOrder: 5 },
            { key: 'InflationAdjustedPrice', label: 'Inflation-Adjusted Unit Price', visible: true, sortable: true, selectionOrder: 6 },
            { key: 'IsOutlier', label: 'Outlier', visible: true, sortable: true, selectionOrder: 7 },
            { key: 'PvBidTotal', label: 'Bid Amount', visible: true, sortable: true, selectionOrder: 8 },
            { key: 'd', label: 'District', visible: true, sortable: true, selectionOrder: 9 },
            { key: 'MarketArea', label: 'Market Area', visible: true, sortable: true, selectionOrder: 10 },
            { key: 'c', label: 'County', visible: true, sortable: true, selectionOrder: 11 },
            { key: 'VendorName', label: 'Bidder Name', visible: false, sortable: false, selectionOrder: 0 },
            { key: 'BidStatus', label: 'Bid Status', visible: true, sortable: true, selectionOrder: 12 },
            { key: 'VendorRanking', label: 'Bidder Rank', visible: true, sortable: true, selectionOrder: 13 },
            { key: 'NumberOfBidders', label: 'Number of Bidders', visible: true, sortable: true, selectionOrder: 14 },
            { key: 'ContractType', label: 'Contract Type', visible: false, sortable: false, selectionOrder: 0 },
            { key: 'ContractWorkType', label: 'Work Type', visible: false, sortable: false, selectionOrder: 0 },
            { key: 'WorkMixDescription', label: 'Work Mix', visible: false, sortable: false, selectionOrder: 0 },
            { key: 'CategoryDescription', label: 'Project Category', visible: false, sortable: false, selectionOrder: 0 },
            { key: 'l', label: 'Letting Date', visible: true, sortable: true, selectionOrder: 15 },
            { key: 'ExecutedDate', label: 'Executed Date', visible: false, sortable: false, selectionOrder: 0 },
            { key: 'Duration', label: 'Awarded Days', visible: false, sortable: false, selectionOrder: 0 },
            { key: 'ProposalType', label: 'Proposal Type', visible: false, sortable: false, selectionOrder: 0 },
            { key: 'BidType', label: 'Bid Type', visible: false, sortable: false, selectionOrder: 0 }
        ];
        $scope.nextSelectionOrder = 16;
        $scope.visibleColumns = function () {
            return $scope.availableColumns
                .filter(col => col.visible)
                .sort((a, b) => a.selectionOrder - b.selectionOrder);
        };
        $scope.showColumnSelector = false;
        $scope.toggleColumnSelector = function () {
            $scope.showColumnSelector = !$scope.showColumnSelector;
        };
        $scope.selectAllColumns = function () {
            $scope.nextSelectionOrder = 1;
            $scope.availableColumns.forEach(col => {
                col.visible = true;
                col.selectionOrder = $scope.nextSelectionOrder++;
            });
        };
        $scope.deselectAllColumns = function () {
            $scope.availableColumns.forEach(col => {
                col.visible = false;
                col.selectionOrder = 0;
            });
        };
        $scope.resetToDefaultColumns = function () {
            $scope.availableColumns.forEach(col => {
                col.visible = col.key === 'p' || col.key === 'ProjectNumber' || col.key === 'ri' ||
                    col.key === 'Quantity' || col.key === 'b' || col.key === 'InflationAdjustedPrice' || col.key === 'PvBidTotal' ||
                    col.key === 'd' || col.key === 'MarketArea' || col.key === 'c' || col.key === 'NumberOfBidders';
            });
            $scope.nextSelectionOrder = 1;
            $scope.availableColumns.forEach(col => {
                if (col.visible) {
                    col.selectionOrder = $scope.nextSelectionOrder++;
                } else {
                    col.selectionOrder = 0;
                }
            });
        };
        $scope.toggleColumn = function (column) {
            if (!column.visible) {
                column.selectionOrder = $scope.nextSelectionOrder++;
            } else {
                column.selectionOrder = 0;
            }
            column.visible = !column.visible;
        };
        $scope.workTypeMap = {
            "I": "Maintenance Other",
            "X0": "Interstate Construction (new)",
            "X1": "New Construction",
            "X2": "Reconstruction",
            "X3": "Resurfacing",
            "X4": "Widening & Resurfacing",
            "X5": "Bridge Construction",
            "X6": "Bridge Repair",
            "X7": "Traffic Operations",
            "X8": "Miscellaneous Construction",
            "X9": "Interstate Rehabilition",
            "Z": "Other"
        };
        $scope.contractTypeMap = {
            "CC": "Const Contract",
            "CEC": "Const Emergency Contract",
            "CFR": "Const Fast Response",
            "CPB": "Const Push Button",
            "CSL": "Construction Streamline",
            "MC": "Maint Contract",
            "MEC": "Maint Emergency Contract",
            "MFR": "Maint Fast Response",
            "MLC": "MT Landscape Install Establish",
            "TO": "Traffic Operations",
            "TOPB": "Traffic Operations Push Button"
        };
        $scope.bidTypeMap = {
            "RESP": "Responsive",
            "NONR": "Non-Responsive",
            "IRR": "Irregular",
            "OTH": "Other"
        };
        $scope.proposalTypeMap = {
            "DIST": "District",
            "CENT": "Central Office"
        };
        $scope.bidStatusMap = {
            "W": "Won",
            "L": "Loss",
            "I": "Irregular",
            "FMV": "Fair Market Value (Bidder Rank 1, 2, 3)"
        };
        $scope.isInvalidDateRange = function () {
            return $scope.startDate && $scope.endDate && new Date($scope.startDate) > new Date($scope.endDate);
        };
        $scope.contractTypes = Object.keys($scope.contractTypeMap);
        $timeout(function () {
            $scope.selectedContractTypes = ['CC'];
        });
        $scope.workTypeCodes = Object.keys($scope.workTypeMap);
        $scope.selectedWorkTypeCodes = [];
        $scope.districtCountyMap = {
            'District 1 (Southwest Florida)': [
                '01 - CHARLOTTE', '03 - COLLIER', '04 - DESOTO', '05 - GLADES', '06 - HARDEE', '07 - HENDRY',
                '09 - HIGHLANDS', '12 - LEE', '13 - MANATEE', '16 - POLK', '17 - SARASOTA', '91 - OKEECHOBEE'
            ],
            'District 2 (Northeast Florida)': [
                '26 - ALACHUA', '27 - BAKER', '28 - BRADFORD', '29 - COLUMBIA', '30 - DIXIE', '31 - GILCHRIST',
                '32 - HAMILTON', '33 - LAFAYETTE', '34 - LEVY', '35 - MADISON', '37 - SUWANNEE', '38 - TAYLOR',
                '39 - UNION', '71 - CLAY', '72 - DUVAL', '74 - NASSAU', '76 - PUTNAM', '78 - ST JOHNS'
            ],
            'District 3 (Northwest Florida)': [
                '46 - BAY', '47 - CALHOUN', '48 - ESCAMBIA', '49 - FRANKLIN', '50 - GADSDEN', '51 - GULF',
                '52 - HOLMES', '53 - JACKSON', '54 - JEFFERSON', '55 - LEON', '56 - LIBERTY', '57 - OKALOOSA',
                '58 - SANTA ROSA', '59 - WAKULLA', '60 - WALTON', '61 - WASHINGTON'
            ],
            'District 4 (Southeast Florida)': [
                '86 - BROWARD', '88 - INDIAN RIVER', '89 - MARTIN', '93 - PALM BEACH', '94 - ST LUCIE'
            ],
            'District 5 (Central Florida)': [
                '11 - LAKE', '18 - SUMTER', '36 - MARION', '70 - BREVARD', '73 - FLAGLER',
                '75 - ORANGE', '77 - SEMINOLE', '79 - VOLUSIA', '92 - OSCEOLA'
            ],
            'District 6 (South Florida)': [
                '87 - MIAMI-DADE', '90 - MONROE'
            ],
            'District 7 (West Central Florida)': [
                '02 - CITRUS', '08 - HERNANDO', '10 - HILLSBOROUGH', '14 - PASCO', '15 - PINELLAS'
            ],
            "Turnpike ": [
                'TURNPIKE',
            ], "DIST/ST-WIDE": ['99 - DIST/ST-WIDE']
        };
        $scope.marketAreaToCountiesMap = {
            "Area 01": ["BAY", "ESCAMBIA", "OKALOOSA", "SANTA ROSA", "WALTON"],
            "Area 02": ["CALHOUN", "FRANKLIN", "GULF", "HOLMES", "JACKSON", "LIBERTY", "WASHINGTON"],
            "Area 03": ["GADSDEN", "JEFFERSON", "LEON", "WAKULLA"],
            "Area 04": [
                "BAKER", "BRADFORD", "COLUMBIA", "DIXIE", "GILCHRIST", "HAMILTON",
                "LAFAYETTE", "LEVY", "MADISON", "PUTNAM", "SUWANNEE", "TAYLOR", "UNION"
            ],
            "Area 05": ["CLAY", "DUVAL", "NASSAU", "ST JOHNS"],
            "Area 06": ["ALACHUA", "MARION", "VOLUSIA"],
            "Area 07": ["CITRUS", "FLAGLER", "HERNANDO", "LAKE", "PASCO", "SUMTER"],
            "Area 08": ["BREVARD", "HILLSBOROUGH", "ORANGE", "OSCEOLA", "PINELLAS", "POLK", "SEMINOLE"],
            "Area 09": ["DESOTO", "GLADES", "HARDEE", "HENDRY", "HIGHLANDS", "OKEECHOBEE"],
            "Area 10": ["CHARLOTTE", "COLLIER", "LEE", "MANATEE", "SARASOTA"],
            "Area 11": ["INDIAN RIVER", "MARTIN", "ST LUCIE"],
            "Area 12": ["BROWARD", "PALM BEACH"],
            "Area 13": ["MIAMI-DADE"],
            "Area 14": ["MONROE"],
            "Area 99": ["DIST/ST-WIDE", "TURNPIKE"]
        };
        $scope.searchProjectNumber = '';
        $scope.clearFilters = function () {
            $scope.regionType = '';
            $scope.regionOptions = [];
            $scope.selectedRegions = [];
            $scope.relatedCounties = [];
            $scope.selectedRegionCounties = [];
            $scope.isRegionDropdownOpen = false;
            $scope.searchText = "";
            $scope.selectedPayItemNumber = null;
            $scope.selectedMinQuantity = null;
            $scope.selectedMaxQuantity = null;
            $scope.monthsOfHistory = 36;
            $scope.selectedMinBidAmount = null;
            $scope.selectedMaxBidAmount = null;
            $scope.selectedBidStatus = "FMV";
            $scope.startDate = null;
            $scope.endDate = null;

            $scope.items = [];
            $scope.showSuggestions = false;
            $scope.isSearching = false;

            $scope.selectedContractTypes = ["CC"];
            $scope.selectedWorkTypeCodes = [];
            $scope.showTrendChart = false;
            $scope.trendAnalysisData.trendTimeGrouping = 'year';
            $scope.trendData = [];
            $scope.trendWarning = '';
            if ($scope.trendChartInstance) {
                $scope.trendChartInstance.destroy();
                $scope.trendChartInstance = null;
            }
        };

        // Function to recalculate weighted averages based on current toggle state
        $scope.recalculateWeightedAverages = function () {
            if (!$scope.bidHistoryData || $scope.bidHistoryData.length === 0) {
                return;
            }

            const quantities = $scope.bidHistoryData.map(item => item.Quantity || 0);
            const prices = $scope.bidHistoryData.map(item => $scope.getPriceField(item) || 0);
            const totalQty = quantities.reduce((sum, q) => sum + q, 0);
            const weightedAvg = quantities.reduce((sum, q, i) => sum + (q * prices[i]), 0) / totalQty;

            const weightedStdDev = Math.sqrt(
                quantities.reduce((sum, q, i) => sum + q * Math.pow(prices[i] - weightedAvg, 2), 0) / totalQty
            );

            let cleanQty = [], cleanPrices = [];

            $scope.bidHistoryData.forEach((item, i) => {
                const price = prices[i];
                const qty = quantities[i];
                const isOutlier = Math.abs(price - weightedAvg) > weightedStdDev;

                item.IsOutlier = isOutlier;
                item.WeightedAvg = weightedAvg;

                if (!isOutlier) {
                    cleanQty.push(qty);
                    cleanPrices.push(price);
                }
            });

            const cleanTotalQty = cleanQty.reduce((sum, q) => sum + q, 0);
            const weightedAvgNoOutliers = cleanQty.reduce((sum, q, i) => sum + (q * cleanPrices[i]), 0) / cleanTotalQty;
            $scope.weightedAvgNoOutliers = weightedAvgNoOutliers;

            $scope.bidHistoryData.forEach(item => {
                item.WeightedAvgNoOutliers = weightedAvgNoOutliers;
            });
        };

        // Inflation toggle change handler
        $scope.onInflationToggleChange = function () {
            if ($scope.bidHistoryData && $scope.bidHistoryData.length > 0) {
                $scope.recalculateWeightedAverages();
                waitForCanvasAndRender();

                // Re-render trend chart if it's currently shown
                if ($scope.showTrendChart) {
                    $timeout(function () {
                        renderTrendChart();
                    }, 0);
                }
                $scope.isChartStale = false;
                if ($scope.customQuantityData.userQuantity && $scope.customQuantityData.userQuantity > 0) {
                    $timeout(function () {
                        $scope.calculateCustomQuantityStats();
                    }, 100);
                }
            }
        };

        // Helper function to get the appropriate price field based on toggle state and item type
        $scope.getPriceField = function (item) {
            if (item.CalculatedUnit === 'LS - Lump Sum') {
                const calculatedUnitPrice = item.Quantity > 0 ? item.b / item.Quantity : 0;
                if ($scope.useInflationAdjustedPrices && item.InflationAdjustedPrice) {
                    return item.Quantity > 0 ? item.InflationAdjustedPrice / item.Quantity : calculatedUnitPrice;
                }
                return calculatedUnitPrice;
            }

            return $scope.useInflationAdjustedPrices && item.InflationAdjustedPrice ?
                item.InflationAdjustedPrice : item.b;
        };

        $scope.searchBids = function () {
            if ((!$scope.searchProjectNumber || $scope.searchProjectNumber.trim() === '') &&
                (!$scope.selectedPayItemNumber || $scope.selectedPayItemNumber.trim() === '')) {
                alert("Please enter and select a valid Pay Item Number before searching.");
                return;
            }
            const months = $scope.monthsOfHistory;
            if (!months || months < 1 || months > 120) {
                alert("Please enter a valid Months of Bid History between 1 and 120.");
                return;
            }
            // Reset Custom Quantity Analysis state
            $scope.customQuantityData.userQuantity = null;
            $scope.customQuantityPrediction = null;
            $scope.isCalculatingPrediction = false;
            $scope.bidHistoryData = [];
            $scope.chartStats = null;
            $scope.isLoading = true;
            $scope.searchAttempted = true;
            $scope.isLargeDataset = false;
            $scope.largeDatasetMessage = '';
            $scope.isChartStale = false;
            if ($scope.chartInstance) {
                $scope.chartInstance.destroy();
                $scope.chartInstance = null;
            }
            // Reset trend chart
            if ($scope.trendChartInstance) {
                $scope.trendChartInstance.destroy();
                $scope.trendChartInstance = null;
            }

            $http.get('/UnitPriceSearch/GetPayItemDetails', {
                params: {
                    number: $scope.selectedPayItemNumber,
                    months: $scope.monthsOfHistory || 12,
                    contractWorkType: Array.isArray($scope.selectedWorkTypeCodes) && $scope.selectedWorkTypeCodes.length
                        ? $scope.selectedWorkTypeCodes
                        : null,
                    startDate: $scope.startDate || null,
                    endDate: $scope.endDate || null,
                    counties: $scope.selectedRegionCounties,
                    bidStatus: $scope.selectedBidStatus || null,
                    contractType: Array.isArray($scope.selectedContractTypes) && $scope.selectedContractTypes.length
                        ? $scope.selectedContractTypes
                        : null,

                    minRank: $scope.selectedMinQuantity || null,
                    maxRank: $scope.selectedMaxQuantity || Infinity,
                    projectNumber: $scope.searchProjectNumber || null,
                    minBidAmount: $scope.selectedMinBidAmount ? parseFloat($scope.selectedMinBidAmount.toString().replace(/,/g, '')) : null,
                    maxBidAmount: $scope.selectedMaxBidAmount ? parseFloat($scope.selectedMaxBidAmount.toString().replace(/,/g, '')) : null,
                },
                traditional: true
            }).success(function (data) {
                // Check response size (1.7MB = 1,785,728 bytes)
                const responseSize = JSON.stringify(data).length;
                const maxSize = 1.7 * 1024 * 1024;
                
                // Clear bandwidth cache when new data is loaded
                clearBandwidthCache();
                
                $scope.bidHistoryData = data;

                const quantities = data.map(item => item.Quantity || 0);
                const prices = data.map(item => $scope.getPriceField(item) || 0);
                const totalQty = quantities.reduce((sum, q) => sum + q, 0);
                const weightedAvg = quantities.reduce((sum, q, i) => sum + (q * prices[i]), 0) / totalQty;

                const weightedStdDev = Math.sqrt(
                    quantities.reduce((sum, q, i) => sum + q * Math.pow(prices[i] - weightedAvg, 2), 0) / totalQty
                );

                let cleanQty = [], cleanPrices = [];

                data.forEach((item, i) => {
                    const price = prices[i];
                    const qty = quantities[i];
                    const isOutlier = Math.abs(price - weightedAvg) > weightedStdDev;

                    item.IsOutlier = isOutlier;
                    item.WeightedAvg = weightedAvg;

                    if (!isOutlier) {
                        cleanQty.push(qty);
                        cleanPrices.push(price);
                    }
                });

                const cleanTotalQty = cleanQty.reduce((sum, q) => sum + q, 0);
                const weightedAvgNoOutliers = cleanQty.reduce((sum, q, i) => sum + (q * cleanPrices[i]), 0) / cleanTotalQty;
                $scope.weightedAvgNoOutliers = weightedAvgNoOutliers;
                data.forEach(item => {
                    item.WeightedAvgNoOutliers = weightedAvgNoOutliers;
                });

                $scope.bidHistoryData.forEach(function (bidItem) {
                    var itemCounty = bidItem.c;
                    var normalizedItemCounty = itemCounty ? itemCounty.trim().toUpperCase() : '';
                    var foundMarketArea = '';

                    if (normalizedItemCounty) {
                        var marketAreaKeys = Object.keys($scope.marketAreaToCountiesMap);

                        for (var keyIndex = 0; keyIndex < marketAreaKeys.length; keyIndex++) {
                            var currentMarketArea = marketAreaKeys[keyIndex];
                            var countyList = $scope.marketAreaToCountiesMap[currentMarketArea];

                            for (var countyIndex = 0; countyIndex < countyList.length; countyIndex++) {
                                var currentCounty = countyList[countyIndex].trim().toUpperCase();
                                if (currentCounty === normalizedItemCounty) {
                                    foundMarketArea = currentMarketArea;
                                    break;
                                }
                            }

                            if (foundMarketArea) {
                                break;
                            }
                        }
                    }

                    bidItem.MarketArea = foundMarketArea || "Unknown";
                });

                // Calculate Number of Bidders per Contract
                const contractBidderCounts = {};
                $scope.bidHistoryData.forEach(function (bidItem) {
                    const contract = bidItem.p;
                    const vendorName = bidItem.VendorName;

                    if (!contractBidderCounts[contract]) {
                        contractBidderCounts[contract] = new Set();
                    }
                    if (vendorName && vendorName.trim() && vendorName.trim().length > 0) {
                        contractBidderCounts[contract].add(vendorName.trim());
                    }
                });
                $scope.bidHistoryData.forEach(function (bidItem) {
                    const contract = bidItem.p;
                    const bidderSet = contractBidderCounts[contract];
                    bidItem.NumberOfBidders = bidderSet ? bidderSet.size : 0;
                });
                if (responseSize > maxSize) {
                    $scope.isLargeDataset = true;
                    $scope.largeDatasetMessage = `The dataset is too large (${(responseSize / (1024 * 1024)).toFixed(2)} MB) to fully display the table in the browser, as it may impact performance. However, summary statistics and charts are still available for review, and you can download the complete data as a CSV file. Please consider refining your filters for a more responsive experience.`;
                } else {
                    $scope.isLargeDataset = false;
                }

            }).error(function (err) {
                console.error("Error fetching bid data:", err);
            }).finally(function () {
                $scope.isLoading = false;
            });
        };
        $scope.setSort = function (column) {
            if ($scope.sortColumn === column) {
                $scope.reverseSort = !$scope.reverseSort;
            } else {
                $scope.sortColumn = column;
                $scope.reverseSort = false;
            }
        };

        // Custom sorting function to handle filtered columns
        $scope.customSort = function (item) {
            if (!$scope.sortColumn) return 0;

            let value = item[$scope.sortColumn];

            switch ($scope.sortColumn) {
                case 'b':
                    if (item.CalculatedUnit === 'LS - Lump Sum') {
                        value = item.Quantity > 0 ? item.b / item.Quantity : 0;
                    } else {
                        value = item.b;
                    }
                    break;
                case 'InflationAdjustedPrice':
                    if (item.CalculatedUnit === 'LS - Lump Sum' && item.InflationAdjustedPrice) {
                        value = item.Quantity > 0 ? item.InflationAdjustedPrice / item.Quantity : 0;
                    } else {
                        value = item.InflationAdjustedPrice || 0;
                    }
                    break;
                case 'PvBidTotal':
                    value = item.PvBidTotal || 0;
                    break;
                case 'l':
                    if (item.l) {
                        value = new Date(parseInt(item.l.replace(/\/Date\((\d+)\)\//, '$1')));
                    } else {
                        value = new Date(0);
                    }
                    break;
                case 'ExecutedDate':
                    if (item.ExecutedDate) {
                        value = new Date(parseInt(item.ExecutedDate.replace(/\/Date\((\d+)\)\//, '$1')));
                    } else {
                        value = new Date(0);
                    }
                    break;
                default:
                    value = item[$scope.sortColumn] || '';
            }

            return value;
        };
        $scope.getSortClass = function (column) {
            if ($scope.sortColumn === column) {
                return $scope.reverseSort ? 'fa-sort-down' : 'fa-sort-up';
            }
            return ''; // Return empty string for unsorted columns
        };
        $scope.clearProjectNumberSearch = function () {
            $scope.searchProjectNumber = '';
        };

        $scope.onRegionTypeChange = function () {
            $scope.selectedRegions = [];
            $scope.relatedCounties = [];
            $scope.selectedRegionCounties = [];
            $scope.isRegionDropdownOpen = false;

            if ($scope.regionType === 'district') {
                $scope.regionOptions = Object.keys($scope.districtCountyMap);
            } else if ($scope.regionType === 'market') {
                $scope.regionOptions = Object.keys($scope.marketAreaToCountiesMap);
            } else if ($scope.regionType === 'county') {
                const allCounties = new Set();

                Object.values($scope.districtCountyMap).forEach(countyList =>
                    countyList.forEach(c => {
                        const cleaned = c.includes(" - ") ? c.split(" - ")[1].trim() : c.trim();
                        allCounties.add(cleaned);
                    })
                );

                Object.values($scope.marketAreaToCountiesMap).forEach(countyList =>
                    countyList.forEach(c => allCounties.add(c.trim()))
                );

                $scope.regionOptions = Array.from(allCounties).sort();
            } else {
                $scope.regionOptions = [];
                $scope.relatedCounties = [];
                $scope.selectedRegionCounties = null;
            }
        };
        $scope.toggleRegionSelection = function (option) {
            const idx = $scope.selectedRegions.indexOf(option);
            if (idx > -1) {
                $scope.selectedRegions.splice(idx, 1);
            } else {
                $scope.selectedRegions.push(option);
            }
            $scope.onMultiRegionChange();
        };

        $scope.onMultiRegionChange = function () {
            let combined = [];

            $scope.selectedRegions.forEach(region => {
                let rawList = [];

                if ($scope.regionType === 'district') {
                    rawList = $scope.districtCountyMap[region] || [];
                } else if ($scope.regionType === 'market') {
                    rawList = $scope.marketAreaToCountiesMap[region] || [];
                } else if ($scope.regionType === 'county') {
                    rawList = [region];
                }

                rawList.forEach(c => {
                    const cleaned = c.includes(" - ") ? c.split(" - ")[1].trim() : c.trim();
                    if (!combined.includes(cleaned)) combined.push(cleaned);
                });
            });

            $scope.relatedCounties = combined.map(c => ({ name: c, selected: true }));
            $scope.selectedRegionCounties = combined;
        };

        $scope.toggleMultiSelectDropdown = function () {
            $scope.isRegionDropdownOpen = !$scope.isRegionDropdownOpen;
        };

        document.addEventListener('click', function (event) {
            const dropdown = document.querySelector('.multi-select-dropdown');
            if (dropdown && !dropdown.contains(event.target)) {
                const scope = angular.element(dropdown).scope();
                if (scope) {
                    $timeout(function () {
                        scope.isRegionDropdownOpen = false;
                    }, 0);
                }
            }
        });
        $scope.toggleRegionCounty = function (county) {
            const index = $scope.selectedRegionCounties.indexOf(county.name);
            if (county.selected && index === -1) {
                $scope.selectedRegionCounties.push(county.name);
            } else if (!county.selected && index > -1) {
                $scope.selectedRegionCounties.splice(index, 1);
            }
        };

        $scope.selectAllRegionCounties = function () {
            $scope.relatedCounties.forEach(c => c.selected = true);
            $scope.selectedRegionCounties = $scope.relatedCounties.map(c => c.name);
        };

        $scope.clearAllRegionCounties = function () {
            $scope.relatedCounties.forEach(c => c.selected = false);
            $scope.selectedRegionCounties = [];
        };

        $scope.removeCounty = function (countyName) {
            const idx = $scope.selectedRegionCounties.indexOf(countyName);
            if (idx > -1) $scope.selectedRegionCounties.splice(idx, 1);

            const match = $scope.relatedCounties.find(c => c.name === countyName);
            if (match) match.selected = false;
        };

        // Call on load
        $scope.onRegionTypeChange();









        $scope.formatBidAmount = function (type) {
            if (type === 'min' && $scope.selectedMinBidAmount) {
                const value = $scope.selectedMinBidAmount.toString().replace(/,/g, '');
                if (!isNaN(value) && value !== '') {
                    const numValue = parseFloat(value);
                    $scope.selectedMinBidAmount = numValue.toLocaleString('en-US', {
                        minimumFractionDigits: 2,
                        maximumFractionDigits: 2
                    });
                }
            } else if (type === 'max' && $scope.selectedMaxBidAmount) {
                const value = $scope.selectedMaxBidAmount.toString().replace(/,/g, '');
                if (!isNaN(value) && value !== '') {
                    const numValue = parseFloat(value);
                    $scope.selectedMaxBidAmount = numValue.toLocaleString('en-US', {
                        minimumFractionDigits: 2,
                        maximumFractionDigits: 2
                    });
                }
            }
        };

        $scope.unformatBidAmount = function (type) {
            if (type === 'min' && $scope.selectedMinBidAmount) {
                $scope.selectedMinBidAmount = $scope.selectedMinBidAmount.toString().replace(/,/g, '');
            } else if (type === 'max' && $scope.selectedMaxBidAmount) {
                $scope.selectedMaxBidAmount = $scope.selectedMaxBidAmount.toString().replace(/,/g, '');
            }
        };

        $scope.getBidAmountRange = function () {
            const min = parseFloat($scope.selectedMinBidAmount ? $scope.selectedMinBidAmount.toString().replace(/,/g, '') : 0);
            const max = parseFloat($scope.selectedMaxBidAmount ? $scope.selectedMaxBidAmount.toString().replace(/,/g, '') : 0);
            return max - min;
        };

        $scope.getQuantityRange = function () {
            const min = parseFloat($scope.selectedMinQuantity) || 0;
            const max = parseFloat($scope.selectedMaxQuantity) || Infinity;
            return max - min;
        };



        $scope.removeCounty = function (countyName) {
            const idx = $scope.selectedRegionCounties.indexOf(countyName);
            if (idx > -1) {
                $scope.selectedRegionCounties.splice(idx, 1);
            }
            const match = $scope.relatedCounties.find(c => c.name === countyName);
            if (match) {
                match.selected = false;
            }
            $scope.selectedRegionCounties = angular.copy($scope.selectedRegionCounties);
        };
        // Fetch Pay Item Suggestions
        $scope.fetchPayItemSuggestions = function () {
            if (!$scope.searchText || $scope.searchText.length < 3) {
                $scope.items = [];
                $scope.selectedPayItemNumber = null;
                return;
            }
            
            $scope.isSearching = true;
            $http.get('/UnitPriceSearch/GetPayItemSuggestions', {
                params: { input: $scope.searchText }
            }).success(function (data) {
                $scope.items = data;
                $scope.showSuggestions = true; // Show dropdown after search
            }).error(function (err) {
                console.error("Error fetching pay item suggestions:", err);
            }).finally(function () {
                $scope.isSearching = false;
            });
        };

        // New function to handle Enter key press
        $scope.onSearchKeyPress = function (event) {
            if (event.keyCode === 13) { // Enter key
                $scope.fetchPayItemSuggestions();
            }
        };
        $scope.clearSearchText = function () {
            $scope.searchText = "";
            $scope.items = [];
            $scope.selectedPayItemNumber = null;
            $scope.showSuggestions = false;
        };
        $scope.selectPayItem = function (item) {
            $scope.searchText = item.Description;
            $scope.selectedPayItemNumber = item.Name;
            $scope.items = [];
            $scope.showSuggestions = false;
        };

        // Get the latest bid date from the search results
        $scope.getLatestBidDate = function () {
            if (!$scope.bidHistoryData || $scope.bidHistoryData.length === 0) {
                return null;
            }

            let latestDate = null;
            $scope.bidHistoryData.forEach(function (item) {
                if (item.l) {
                    const bidDate = new Date(parseInt(item.l.replace(/\/Date\((\d+)\)\//, '$1')));
                    if (!latestDate || bidDate > latestDate) {
                        latestDate = bidDate;
                    }
                }
            });

            return latestDate;
        };
        $scope.shouldHideGraphForLumpSum = function () {
            if (!$scope.bidHistoryData || $scope.bidHistoryData.length === 0) return false;

            const allLumpSum = $scope.bidHistoryData.every(item => item.CalculatedUnit === 'LS - Lump Sum');
            if (allLumpSum) {
                const allQuantityOne = $scope.bidHistoryData.every(item => (item.Quantity || 0) === 1);
                return allQuantityOne;
            }

            return false;
        };


        function formatDotNetDate(msDateString) {
            if (!msDateString) return '';
            const match = /\/Date\((\d+)\)\//.exec(msDateString);
            if (!match) return '';
            const date = new Date(parseInt(match[1]));
            return date.toLocaleDateString('en-US');
        }

        //CSV Export
        $scope.exportClick = function () {
            // Prevents multiple simultaneous exports
            if ($scope.isExporting) {
                return;
            }
            if (exportDebounceTimer) {
                $timeout.cancel(exportDebounceTimer);
            }

            $scope.isExporting = true;

            try {
                let headers = [
                    "Contract Number", "Project Number", "Pay Item", "Description", "Supplemental Description",
                    "Units", "Quantity", "Unit Price Bid", "Inflation-Adjusted Unit Price", "Weighted Avg No Outliers", "Outlier", "Bid Amount", "District",
                    "Primary County", "Bidder Name", "Bid Status", "Bidder Rank", "Number of Bidders"
                    , "Contract Type", "Work Type", "Work Mix", "Project Category",
                    "Letting Date", "Executed Date", "Awarded Days", "Proposal Type", "Bid Type"
                ].join(",") + "\n";

                let rows = $scope.bidHistoryData.map(item => {
                    // Handle LS items for CSV export
                    const unitPrice = item.CalculatedUnit === 'LS - Lump Sum' ?
                        (item.Quantity > 0 ? item.b / item.Quantity : 0) : item.b;

                    const inflationAdjustedUnitPrice = item.CalculatedUnit === 'LS - Lump Sum' && item.InflationAdjustedPrice ?
                        (item.Quantity > 0 ? item.InflationAdjustedPrice / item.Quantity : 0) :
                        (item.InflationAdjustedPrice || item.b);

                    return [
                        `"${item.p}"`, `"${item.ProjectNumber}"`, `"${item.ri}"`, `"${item.Description.replace(/"/g, '""')}"`,
                        `"${item.SupplementalDescription}"`,
                        `"${item.CalculatedUnit}"`, `"${item.Quantity}"`, `"${unitPrice}"`, `"${inflationAdjustedUnitPrice}"`, `"${item.WeightedAvgNoOutliers}"`, `"${item.IsOutlier ? 'Yes' : 'No'}"`,
                        `"${item.PvBidTotal}"`, `"${item.d}"`, `"${item.c}"`, `"${item.VendorName}"`,
                        `"${(item.BidStatus)}"`, `"${item.VendorRanking}"`, `"${item.NumberOfBidders}"`, `"${$scope.contractTypeMap[item.ContractType] || item.ContractType}"`,
                        `"${$scope.workTypeMap[item.ContractWorkType] || item.ContractWorkType}"`, `"${(item.WorkMixDescription)}"`, `"${(item.CategoryDescription)}"`,
                        `"${formatDotNetDate(item.l)}"`, `"${formatDotNetDate(item.ExecutedDate)}"`,
                        `"${item.Duration}"`, `"${$scope.proposalTypeMap[item.ProposalType] || item.ProposalType}"`, `"${$scope.getBidTypeLabel(item.BidType)}"`
                    ].join(",");
                }).join("\n");

                let csvContent = "data:text/csv;charset=utf-8," + headers + rows;
                let encodedUri = encodeURI(csvContent);
                let link = document.createElement("a");
                link.setAttribute("href", encodedUri);
                link.setAttribute("download", "bid_history.csv");
                link.style.display = "none";
                document.body.appendChild(link);
                link.click();

                // Clean up the DOM element after a short delay
                $timeout(function () {
                    if (link && link.parentNode) {
                        link.parentNode.removeChild(link);
                    }
                }, 100);

            } catch (error) {
                console.error("Error exporting CSV:", error);
            } finally {
                // Reset the exporting flag after a delay to prevent rapid successive clicks
                exportDebounceTimer = $timeout(function () {
                    $scope.isExporting = false;
                }, 500);
            }
        };
        // Helper function to wrap text in PDF
        function wrapText(doc, text, x, y, maxWidth, lineHeight) {
            const words = text.split(' ');
            let line = '';
            let currentY = y;

            for (let i = 0; i < words.length; i++) {
                const testLine = line + words[i] + ' ';
                const testWidth = doc.getTextWidth(testLine);

                if (testWidth > maxWidth && i > 0) {
                    doc.text(line, x, currentY);
                    line = words[i] + ' ';
                    currentY += lineHeight;
                } else {
                    line = testLine;
                }
            }

            // Add the last line
            if (line.trim()) {
                doc.text(line, x, currentY);
                currentY += lineHeight;
            }

            return currentY;
        }

        $scope.downloadPDF = function () {
            setTimeout(function () {
                var jsPDF = window.jspdf && window.jspdf.jsPDF;
                if (typeof jsPDF !== 'function') {
                    console.error('PDF generation libraries are still loading. Please try again shortly. jsPDF is not loaded!');
                    return;
                }

                var doc = new jsPDF({ unit: 'pt', format: 'a4' });
                let y = 40;

                // Header
                doc.setTextColor('#1F4288');
                doc.setFontSize(18);
                doc.setFont('helvetica', 'bold');
                doc.text('Florida Department of Transportation', 40, y);
                y += 30;
                doc.setFontSize(14);
                doc.setFont('helvetica', 'normal');
                doc.text('Unit Price Search Report', 40, y);
                y += 30;

                // Report timestamp
                var reportTimestamp = new Date();
                var timestampStr = 'Report generated: ' + reportTimestamp.toLocaleString();
                doc.setTextColor('#666666');
                doc.setFontSize(10);
                doc.text(timestampStr, 40, y);
                y += 25;

                // Search filters
                doc.setTextColor('#000000');
                doc.setFontSize(12);
                doc.setFont('helvetica', 'bold');
                doc.text('Search Filters:', 40, y);
                y += 20;
                doc.setFontSize(10);
                doc.setFont('helvetica', 'normal');
                doc.text('Contract Number: ' + ($scope.searchProjectNumber || 'All'), 40, y);
                y += 15;
                doc.text('Pay Item: ' + ($scope.searchText || 'All'), 40, y);
                y += 15;
                doc.text('Bid Status: ' + ($scope.bidStatusMap[$scope.selectedBidStatus] || 'All'), 40, y);
                y += 15;
                doc.text('Months of History: ' + ($scope.monthsOfHistory || '36'), 40, y);
                y += 15;
                doc.text('Inflation Adjustment: ' + ($scope.useInflationAdjustedPrices ? 'Enabled (NHCCI-based adjustment to 2024 Q4 levels)' : 'Disabled (using raw prices)'), 40, y);
                y += 15;
                doc.text(
                    'Date Range: ' +
                    ($scope.startDate ? new Date($scope.startDate).toLocaleDateString() : 'All') +
                    ' to ' +
                    ($scope.endDate ? new Date($scope.endDate).toLocaleDateString() : 'All'),
                    40,
                    y
                );
                y += 15;

                // Handle counties with text wrapping
                const countiesText = 'Selected Counties: ' + ($scope.selectedRegionCounties && $scope.selectedRegionCounties.length > 0 ? $scope.selectedRegionCounties.join(', ') : 'All');
                y = wrapText(doc, countiesText, 40, y, 500, 15);
                y += 10;

                // Summary statistics
                if ($scope.chartStats) {
                    doc.setFontSize(12);
                    doc.setFont('helvetica', 'bold');
                    doc.text('Summary Statistics:', 40, y);
                    y += 20;
                    doc.setFontSize(10);
                    doc.setFont('helvetica', 'normal');
                    const stats = [
                        { label: 'Total Bids', value: $scope.chartStats.count || 0 },
                        { label: 'Total Contracts', value: $scope.chartStats.totalContracts || 0 },
                        { label: 'Total Bid Amount', value: '$' + ($scope.chartStats.totalBidAmount || 0).toLocaleString() },
                        { label: 'Total Quantity', value: ($scope.chartStats.totalQuantity || 0).toLocaleString() },
                        { label: 'Average Quantity', value: ($scope.chartStats.avgQty || 0).toFixed(2) },
                        { label: 'Outlier Count', value: $scope.chartStats.outlierCount || 0 }
                    ];

                    // Display statistics in two columns
                    const col1X = 40;
                    const col2X = 250;
                    const lineHeight = 18;

                    for (let i = 0; i < stats.length; i += 2) {
                        const stat1 = stats[i];
                        const stat2 = stats[i + 1];

                        doc.text(stat1.label + ': ' + stat1.value, col1X, y);
                        if (stat2) {
                            doc.text(stat2.label + ': ' + stat2.value, col2X, y);
                        }
                        y += lineHeight;
                    }
                    y += 10;

                    // Weighted averages section
                    doc.setFontSize(11);
                    doc.setFont('helvetica', 'bold');
                    doc.text('Weighted Averages:', 40, y);
                    y += 15;
                    doc.setFontSize(10);
                    doc.setFont('helvetica', 'normal');
                    doc.text('Weighted Average Unit Price (' + ($scope.useInflationAdjustedPrices ? 'Inflation-Adjusted' : 'Raw') + '): $' + ($scope.chartStats.avg || 0).toFixed(2), 40, y);
                    y += 15;
                    doc.text('Weighted Average (No Outliers) (' + ($scope.useInflationAdjustedPrices ? 'Inflation-Adjusted' : 'Raw') + '): $' + ($scope.chartStats.weightedAvgNoOutliers || 0).toFixed(2), 40, y);
                    y += 15;
                    doc.text('Average Inflation-Adjusted Price: $' + ($scope.chartStats.avgInflationAdjustedPrice || 0).toFixed(2), 40, y);
                    y += 15;
                    doc.text('Average Inflation Increase: ' + ($scope.chartStats.avgInflationIncrease || 0).toFixed(1) + '%', 40, y);
                    y += 25;
                }

                if ($scope.chartInstance && typeof $scope.chartInstance.toBase64Image === 'function') {
                    try {
                        const chartImg = $scope.chartInstance.toBase64Image();
                        doc.setFontSize(12);
                        doc.setFont('helvetica', 'bold');
                        doc.text('Price Analysis Chart:', 40, y);
                        y += 20;
                        doc.addImage(chartImg, 'PNG', 40, y, 500, 250);
                        y += 270;
                    } catch (error) {
                        console.error('Error adding chart to PDF:', error);
                        doc.setFontSize(10);
                        doc.setFont('helvetica', 'normal');
                        doc.text('Chart could not be included in PDF', 40, y);
                        y += 20;
                    }
                }
                // Footer
                const pageHeight = doc.internal.pageSize.height;
                doc.setFontSize(9);
                doc.setTextColor(100);
                doc.text('For complete data, please use the CSV export option.', 40, pageHeight - 40);
                doc.text('Report generated by DQE System', 40, pageHeight - 25);
                doc.save('UnitPriceSearch_Report.pdf');
                setTimeout(function () {
                    if (!$scope.$$phase) $scope.$apply();
                }, 500);
            }, 10);
        };

        $scope.toggleLegend = function (type) {
            switch (type) {
                case 'normal':
                    $scope.showNormal = !$scope.showNormal;
                    break;
                case 'outlier':
                    $scope.showOutliers = !$scope.showOutliers;
                    break;
                case 'trend':
                    $scope.showTrendLine = !$scope.showTrendLine;
                    break;
                case 'avg':
                    $scope.showWeightedAvg = !$scope.showWeightedAvg;
                    break;
            }
            if ($scope.bidHistoryData && $scope.bidHistoryData.length > 0) {
                waitForCanvasAndRender();
            }
        };
        $scope.$watch('selectedContractTypes', function (newVal) {
            if (Array.isArray(newVal) && newVal.includes("ALL")) {
                $scope.selectedContractTypes = angular.copy($scope.contractTypes);
            }
        }, true);

        $scope.$watch('selectedWorkTypeCodes', function (newVal) {
            if (Array.isArray(newVal) && newVal.includes("ALL")) {
                $scope.selectedWorkTypeCodes = angular.copy($scope.workTypeCodes);
            }
        }, true);
        var filterWatchList = [
            'searchText',
            'searchProjectNumber',
            'selectedContractTypes',
            'selectedWorkTypeCodes',
            'selectedBidStatus',
            'monthsOfHistory',
            'startDate',
            'endDate',
            'regionType',
            'selectedRegions',
            'selectedRegionCounties',


            'selectedMinQuantity',
            'selectedMaxQuantity',
            'selectedMinBidAmount',
            'selectedMaxBidAmount'
        ];
        angular.forEach(filterWatchList, function (filter) {
            $scope.$watch(filter, function (newVal, oldVal) {
                if (newVal !== oldVal &&
                    !$scope.isLoading &&
                    $scope.bidHistoryData &&
                    $scope.bidHistoryData.length > 0 &&
                    $scope.searchAttempted &&
                    $scope.chartStats &&
                    $scope.chartInstance &&
                    filter !== 'useInflationAdjustedPrices') {
                    $scope.isChartStale = true;
                }
            }, true);
        });
        $scope.$watch('bidHistoryData', function (newVal) {
            if (newVal && newVal.length > 0) {
                waitForCanvasAndRender();
                if ($scope.showTrendChart) {
                    $timeout(function () {
                        renderTrendChart();
                    }, 0);
                }
            }
        });
        function computeWeightedStats(prices, quantities) {
            if (typeof d3 !== 'undefined') {
                var totalQty = d3.sum(quantities);
                var weightedMean = d3.sum(prices.map(function (p, i) {
                    return p * quantities[i];
                })) / totalQty;
                var weightedStd = Math.sqrt(d3.sum(prices.map(function (p, i) {
                    return quantities[i] * Math.pow(p - weightedMean, 2);
                })) / totalQty);
                return { weightedMean: weightedMean, weightedStd: weightedStd };
            } else {
                var totalQty = quantities.reduce((sum, q) => sum + q, 0);
                var weightedMean = prices.reduce((sum, p, i) => sum + (p * quantities[i]), 0) / totalQty;
                var weightedStd = Math.sqrt(prices.reduce((sum, p, i) => sum + (quantities[i] * Math.pow(p - weightedMean, 2)), 0) / totalQty);
                return { weightedMean: weightedMean, weightedStd: weightedStd };
            }
        }

        function filterOutliers(prices, quantities, weightedMean, weightedStd) {
            return $scope.bidHistoryData.map((item, i) => {
                const isOutlier = Math.abs(prices[i] - weightedMean) > weightedStd;
                return !isOutlier ? {
                    q: quantities[i],
                    p: prices[i],
                    l: item.l,
                    pn: item.p
                } : null;
            }).filter(d => d !== null);
        }

        // Helper function to aggregate multiple y-values per x
        function aggregateDataByX(x, y, method = 'mean') {
            const groups = {};
            for (let i = 0; i < x.length; i++) {
                const xVal = x[i];
                if (!groups[xVal]) {
                    groups[xVal] = [];
                }
                groups[xVal].push(y[i]);
            }
            const result = [];
            for (const [xVal, yVals] of Object.entries(groups)) {
                let aggregatedY;

                switch (method) {
                    case 'mean':
                        aggregatedY = yVals.reduce((sum, val) => sum + val, 0) / yVals.length;
                        break;
                    case 'median':
                        yVals.sort((a, b) => a - b);
                        const mid = Math.floor(yVals.length / 2);
                        aggregatedY = yVals.length % 2 === 0
                            ? (yVals[mid - 1] + yVals[mid]) / 2
                            : yVals[mid];
                        break;
                    case 'first':
                        aggregatedY = yVals[0];
                        break;
                    case 'last':
                        aggregatedY = yVals[yVals.length - 1];
                        break;
                    default:
                        aggregatedY = yVals.reduce((sum, val) => sum + val, 0) / yVals.length;
                }

                result.push({ x: parseFloat(xVal), y: aggregatedY });
            }

            return result.sort((a, b) => a.x - b.x);
        }

        const bandwidthCache = new Map();
        
       
        let cachedUnfilteredBandwidth = null;
        let cachedFilteredBandwidth = null;
        let lastUnfilteredDataKey = null;
        let lastFilteredDataKey = null;
        
        function calculateAdaptiveBandwidth(x, y, targetQuantity = null) {
           
            const xSum = x.reduce((sum, val) => sum + val, 0);
            const ySum = y.reduce((sum, val) => sum + val, 0);
            const xMin = Math.min(...x);
            const xMax = Math.max(...x);
            const yMin = Math.min(...y);
            const yMax = Math.max(...y);
            const cacheKey = `${x.length}_${xSum.toFixed(2)}_${ySum.toFixed(2)}_${xMin.toFixed(2)}_${xMax.toFixed(2)}_${yMin.toFixed(2)}_${yMax.toFixed(2)}_${targetQuantity || 'null'}`;
            
            if (bandwidthCache.has(cacheKey)) {
                console.log('Using cached bandwidth for dataset:', { length: x.length, cacheKey: cacheKey.substring(0, 50) + '...' });
                return bandwidthCache.get(cacheKey);
            }
            
            if (!Array.isArray(x) || !Array.isArray(y) || x.length < 3 || y.length < 3 || x.length !== y.length) {
                const result = 0.8; // Default fallback
                bandwidthCache.set(cacheKey, result);
                return result;
            }
            const validData = x.map((xi, i) => ({ x: xi, y: y[i] }))
                .filter(point => isFinite(point.x) && isFinite(point.y) && point.x > 0 && point.y > 0);

            if (validData.length < 3) {
                const result = 0.8;
                bandwidthCache.set(cacheKey, result);
                return result;
            }

            const sortedData = validData.sort((a, b) => a.x - b.x);
            const xValues = sortedData.map(d => d.x);
            const yValues = sortedData.map(d => d.y);

            // Check if we're predicting near existing data points (local prediction)
            let isLocalPrediction = false;
            if (targetQuantity !== null) {
                // Find the closest existing data point to the target
                const distances = xValues.map(x => Math.abs(x - targetQuantity));
                const minDistance = Math.min(...distances);
                const closestIndex = distances.indexOf(minDistance);
                const closestQuantity = xValues[closestIndex];
                
                // Calculate relative distance (percentage of data range)
                const xRange = Math.max(...xValues) - Math.min(...xValues);
                const relativeDistance = minDistance / xRange;
                
                // If target is within 5% of existing data, treat as local prediction
                isLocalPrediction = relativeDistance < 0.05;
                
                console.log('Local prediction check:', {
                    targetQuantity: targetQuantity,
                    closestQuantity: closestQuantity,
                    minDistance: minDistance,
                    relativeDistance: relativeDistance,
                    isLocalPrediction: isLocalPrediction
                });
            }

            const xRange = Math.max(...xValues) - Math.min(...xValues);
            const xMean = xValues.reduce((sum, val) => sum + val, 0) / xValues.length;
            
            const windowSize = Math.min(5, Math.floor(xValues.length / 4));
            let localVariance = 0;
            let varianceCount = 0;
            
            for (let i = windowSize; i < xValues.length - windowSize; i++) {
                const windowY = yValues.slice(i - windowSize, i + windowSize + 1);
                const windowMean = windowY.reduce((sum, val) => sum + val, 0) / windowY.length;
                const variance = windowY.reduce((sum, val) => sum + Math.pow(val - windowMean, 2), 0) / windowY.length;
                localVariance += variance;
                varianceCount++;
            }
            
            const avgLocalVariance = varianceCount > 0 ? localVariance / varianceCount : 0;
            
            const dataDensity = xValues.length / xRange;
     
            let adaptiveBandwidth = 0.9; 
            
            if (dataDensity > 20) {
                adaptiveBandwidth *= 0.6;
            } else if (dataDensity > 10) {
                adaptiveBandwidth *= 0.7; 
            } else if (dataDensity < 1) {
                adaptiveBandwidth *= 1.4; 
            } else if (dataDensity < 2) {
                adaptiveBandwidth *= 1.2;
            }
            
            const globalVariance = yValues.reduce((sum, val) => sum + Math.pow(val - yValues.reduce((s, v) => s + v, 0) / yValues.length, 2), 0) / yValues.length;
            if (avgLocalVariance > globalVariance * 0.3) {
                adaptiveBandwidth *= 0.7; 
            } else if (avgLocalVariance < globalVariance * 0.05) {
                adaptiveBandwidth *= 1.3; 
            }
            
            // Adjust based on data range - more conservative for small ranges
            if (xRange > 1000) {
                adaptiveBandwidth *= 1.1; 
            } else if (xRange < 50) {
                adaptiveBandwidth *= 0.8;
            } else if (xRange < 100) {
                adaptiveBandwidth *= 0.9; 
            }
            
            // Apply local bandwidth reduction for predictions near existing data
            if (isLocalPrediction) {
                const originalBandwidth = adaptiveBandwidth;
                // Use much smaller bandwidth for local predictions to focus on nearby points
                adaptiveBandwidth = Math.min(adaptiveBandwidth, 0.3);
                
                console.log('Local bandwidth reduction applied:', {
                    originalBandwidth: originalBandwidth,
                    reducedBandwidth: adaptiveBandwidth,
                    reason: 'Target quantity is near existing data points'
                });
            }
            
            adaptiveBandwidth = Math.max(0.2, Math.min(0.9, adaptiveBandwidth));
            
            
            const minPoints = Math.max(3, Math.floor(adaptiveBandwidth * xValues.length));
            if (minPoints < 3) {
                adaptiveBandwidth = Math.max(0.2, 3 / xValues.length);
            }
            
            if (bandwidthCache.size > 100) {
                const firstKey = bandwidthCache.keys().next().value;
                bandwidthCache.delete(firstKey);
            }
            
            console.log('Adaptive bandwidth calculation:', {
                originalBandwidth: 0.8,
                adaptiveBandwidth: adaptiveBandwidth,
                dataDensity: dataDensity,
                avgLocalVariance: avgLocalVariance,
                globalVariance: globalVariance,
                xRange: xRange,
                dataPoints: xValues.length
            });
            
            bandwidthCache.set(cacheKey, adaptiveBandwidth);
            return adaptiveBandwidth;
        }
        
        function getCachedBandwidth(x, y, isFiltered = false, targetQuantity = null) {
            // Create a data key for comparison
            const xSum = x.reduce((sum, val) => sum + val, 0);
            const ySum = y.reduce((sum, val) => sum + val, 0);
            const dataKey = `${x.length}_${xSum.toFixed(2)}_${ySum.toFixed(2)}`;
            
            // Check if we have cached bandwidth for this exact dataset
            if (isFiltered) {
                if (lastFilteredDataKey === dataKey && cachedFilteredBandwidth !== null) {
                    console.log('Using cached filtered bandwidth for dataset:', { length: x.length });
                    return cachedFilteredBandwidth;
                }
                lastFilteredDataKey = dataKey;
                cachedFilteredBandwidth = calculateAdaptiveBandwidth(x, y, targetQuantity);
                return cachedFilteredBandwidth;
            } else {
                if (lastUnfilteredDataKey === dataKey && cachedUnfilteredBandwidth !== null) {
                    console.log('Using cached unfiltered bandwidth for dataset:', { length: x.length });
                    return cachedUnfilteredBandwidth;
                }
                lastUnfilteredDataKey = dataKey;
                cachedUnfilteredBandwidth = calculateAdaptiveBandwidth(x, y, targetQuantity);
                return cachedUnfilteredBandwidth;
            }
        }
        
        function clearBandwidthCache() {
            cachedUnfilteredBandwidth = null;
            cachedFilteredBandwidth = null;
            lastUnfilteredDataKey = null;
            lastFilteredDataKey = null;
            bandwidthCache.clear();
            console.log('Bandwidth cache cleared');
        }

        // linear interpolation with extrapolation
        function interpolateLinear(xArr, yArr, targetX) {
            if (xArr.length === 0 || yArr.length === 0) return NaN;
            if (xArr.length !== yArr.length) return NaN;
            if (xArr.length === 1) return yArr[0];
            
            // Filter out any NaN or invalid values
            const validIndices = [];
            for (let i = 0; i < xArr.length; i++) {
                if (isFinite(xArr[i]) && isFinite(yArr[i]) && xArr[i] > 0 && yArr[i] > 0) {
                    validIndices.push(i);
                }
            }
            
            if (validIndices.length === 0) return NaN;
            if (validIndices.length === 1) return yArr[validIndices[0]];
            
            const validX = validIndices.map(i => xArr[i]);
            const validY = validIndices.map(i => yArr[i]);
            
            const exactIndex = validX.indexOf(targetX);
            if (exactIndex !== -1) return validY[exactIndex];
            
            let leftIndex = -1;
            let rightIndex = -1;
            for (let i = 0; i < validX.length - 1; i++) {
                if (targetX >= validX[i] && targetX <= validX[i + 1]) {
                    leftIndex = i;
                    rightIndex = i + 1;
                    break;
                }
            }
            
            // Extrapolation cases
            if (leftIndex === -1) {
                if (targetX < validX[0]) {
                    leftIndex = 0;
                    rightIndex = 1;
                } else if (targetX > validX[validX.length - 1]) {
                    leftIndex = validX.length - 2;
                    rightIndex = validX.length - 1;
                } else {
                    return NaN;
                }
            }
            
            // Linear interpolation/extrapolation
            const x1 = validX[leftIndex];
            const x2 = validX[rightIndex];
            const y1 = validY[leftIndex];
            const y2 = validY[rightIndex];
            
            if (x2 === x1) return y1;
            const slope = (y2 - y1) / (x2 - x1);
            const result = y1 + slope * (targetX - x1);

            return isFinite(result) && result > 0 ? result : NaN;
        }
        function loessSmooth(x, y, bandwidth, xvals) {
            if (!Array.isArray(x) || !Array.isArray(y) || !Array.isArray(xvals) ||
                x.length === 0 || y.length === 0 || x.length !== y.length) {

                return xvals.map(() => NaN);
            }
            
            // Debug logging for specific cases
            const isCustomQuantityCase = xvals.length === 1 && xvals[0] === $scope.customQuantityData?.userQuantity;
            if (isCustomQuantityCase) {
                console.log('LOESS calculation for custom quantity:', {
                    targetQuantity: xvals[0],
                    dataPoints: x.length,
                    bandwidth: bandwidth,
                    sampleData: x.slice(0, 5).map((xi, i) => ({ x: xi, y: y[i] }))
                });
            }
            
            try {
                const aggregatedData = aggregateDataByX(x, y, 'median'); // Use median to reduce outlier influence
                const uniqueX = aggregatedData.map(d => d.x);
                const uniqueY = aggregatedData.map(d => d.y);
                
                if (isCustomQuantityCase) {
                    console.log('After aggregation:', {
                        originalPoints: x.length,
                        uniquePoints: uniqueX.length,
                        aggregatedData: aggregatedData.slice(0, 10)
                    });
                }
                
                if (uniqueX.length < 3) {
                    console.warn("Not enough unique x-values for LOESS (need at least 3)");
                    return xvals.map(xval => interpolateLinear(uniqueX, uniqueY, xval));
                }
                
                const sortedIndices = uniqueX.map((_, i) => i)
                    .sort((a, b) => uniqueX[a] - uniqueX[b]);
                const sortedX = sortedIndices.map(i => uniqueX[i]);
                const sortedY = sortedIndices.map(i => uniqueY[i]);
                
                // More conservative bandwidth adjustment
                const adjustedBandwidth = Math.max(bandwidth, 3 / sortedX.length);
                
                if (isCustomQuantityCase) {
                    console.log('LOESS parameters:', {
                        sortedX: sortedX.slice(0, 10),
                        sortedY: sortedY.slice(0, 10),
                        adjustedBandwidth: adjustedBandwidth,
                        windowSize: Math.floor(adjustedBandwidth * sortedX.length)
                    });
                }
                
                if (typeof science !== "undefined" && typeof science.stats !== "undefined" && typeof science.stats.loess === "function") {
                    const loess = science.stats.loess().bandwidth(adjustedBandwidth);
                    const smoothedValues = loess(sortedX, sortedY);
                    
                    if (isCustomQuantityCase) {
                        console.log('Science.js LOESS results:', {
                            smoothedValues: smoothedValues.slice(0, 10),
                            targetIndex: sortedX.findIndex(x => Math.abs(x - xvals[0]) < 0.01)
                        });
                    }
                    
                    return xvals.map(xval => {
                        const result = interpolateLinear(sortedX, smoothedValues, xval);
                        // Ensure we don't return NaN values
                        return isFinite(result) && result > 0 ? result : interpolateLinear(sortedX, sortedY, xval);
                    });
                } else {
                    console.warn("Science.js not available, using manual LOESS");
                    return manualLoessSmooth(x, y, bandwidth, xvals);
                }

            } catch (error) {
                console.error("Science.js LOESS error:", error);
                return manualLoessSmooth(x, y, bandwidth, xvals);
            }
        }
        function manualLoessSmooth(x, y, bandwidth, xvals) {

            const aggregatedData = aggregateDataByX(x, y, 'median'); // Use median to reduce outlier influence
            const uniqueX = aggregatedData.map(d => d.x);
            const uniqueY = aggregatedData.map(d => d.y);

            if (uniqueX.length < 3) {
                return xvals.map(xval => interpolateLinear(uniqueX, uniqueY, xval));
            }
            
            // More conservative window size calculation
            const windowSize = Math.max(3, Math.floor(bandwidth * uniqueX.length));
            
            // Debug logging for custom quantity case
            const isCustomQuantityCase = xvals.length === 1 && xvals[0] === $scope.customQuantityData?.userQuantity;
            if (isCustomQuantityCase) {
                console.log('Manual LOESS parameters:', {
                    windowSize: windowSize,
                    bandwidth: bandwidth,
                    uniquePoints: uniqueX.length
                });
            }

            return xvals.map(xval => {
                // nearest points
                const distances = uniqueX.map((xi, i) => ({ dist: Math.abs(xi - xval), index: i }));
                distances.sort((a, b) => a.dist - b.dist);

                const nearestPoints = distances.slice(0, Math.min(windowSize, distances.length));
                
                if (isCustomQuantityCase) {
                    console.log('Manual LOESS for target:', {
                        targetX: xval,
                        nearestPoints: nearestPoints.slice(0, 5).map(p => ({
                            x: uniqueX[p.index],
                            y: uniqueY[p.index],
                            distance: p.dist
                        }))
                    });
                }

                let weightedSum = 0;
                let totalWeight = 0;

                for (const point of nearestPoints) {
                    // Improved weighting function - more emphasis on closer points
                    const weight = point.dist === 0 ? 1 : 1 / (1 + Math.pow(point.dist, 1.5));
                    weightedSum += uniqueY[point.index] * weight;
                    totalWeight += weight;
                }

                // If no valid weighted calculation, fall back to linear interpolation
                if (totalWeight <= 0) {
                    return interpolateLinear(uniqueX, uniqueY, xval);
                }
                
                const result = weightedSum / totalWeight;
                
                if (isCustomQuantityCase) {
                    console.log('Manual LOESS result:', {
                        targetX: xval,
                        result: result,
                        weightedSum: weightedSum,
                        totalWeight: totalWeight
                    });
                }
                
                return result;
            });
        }
        //Bootstrap CI function
        function bootstrapCI(x, y, xvals, frac, nBoot = 500) {
            if (!x.length || !y.length || x.length !== y.length) {
                return {
                    lower: xvals.map(() => null),
                    upper: xvals.map(() => null),
                };
            }
            
            // Create a seeded random number generator for consistent results
            let seed = 12345; // Fixed seed for consistent results
            function seededRandom() {
                seed = (seed * 9301 + 49297) % 233280;
                return seed / 233280;
            }
            
            let preds = [];
            for (let b = 0; b < nBoot; b++) {
                let indices = [];
                for (let i = 0; i < x.length; i++) {
                    indices.push(Math.floor(seededRandom() * x.length));
                }
                let xBoot = indices.map(i => x[i]);
                let yBoot = indices.map(i => y[i]);
                let smoothed = loessSmooth(xBoot, yBoot, frac, xvals);
                preds.push(smoothed);
            }
            let lower = [];
            let upper = [];
            for (let i = 0; i < xvals.length; i++) {
                // Filter out null, NaN, and negative values for unit prices
                let valuesAtPoint = preds.map(row => row[i]).filter(v => v !== null && !isNaN(v) && v >= 0);
                valuesAtPoint.sort((a, b) => a - b);
                if (valuesAtPoint.length > 0) {
                    let lowerIdx = Math.floor(valuesAtPoint.length * 0.025);
                    let upperIdx = Math.floor(valuesAtPoint.length * 0.975);

                    // Ensure lower bound is non-negative (minimum 0)
                    lower[i] = Math.max(0, valuesAtPoint[lowerIdx]);
                    upper[i] = valuesAtPoint[upperIdx];
                } else {
                    lower[i] = null;
                    upper[i] = null;
                }
            }
            return { lower, upper };
        }

        // Line Graph rendering
        function waitForCanvasAndRender() {
            $scope.isChartLoading = true;

            $timeout(function () {
                if (typeof requestAnimationFrame === 'function') {
                    requestAnimationFrame(function () {
                        const canvas = document.getElementById("priceChart");
                        if (!canvas) {
                            $scope.isChartLoading = false;
                            return;
                        }
                        if (!$scope.bidHistoryData || $scope.bidHistoryData.length === 0) {
                            $scope.isChartLoading = false;
                            return;
                        }

                        // Check if we should hide the graph for all LS items
                        if ($scope.shouldHideGraphForLumpSum()) {
                            $scope.isChartLoading = false;
                            return;
                        }

                        const quantities = $scope.bidHistoryData.map(item => item.Quantity || 0);
                        const prices = $scope.bidHistoryData.map(item => $scope.getPriceField(item) || 0);

                        const { weightedMean, weightedStd } = computeWeightedStats(prices, quantities);
                        const filtered = filterOutliers(prices, quantities, weightedMean, weightedStd);
                        const QuantityFiltered = filtered.map(d => d.q);
                        const PriceFiltered = filtered.map(d => d.p);

                        // Common range for unfiltered data
                        const quantityRange = Array.from(new Set(quantities)).sort((a, b) => a - b);
                        // Range for filtered data (no outliers)
                        const quantityRangeFiltered = Array.from(new Set(QuantityFiltered)).sort((a, b) => a - b);
                        
                        // Debug logging to verify data consistency
                        console.log('Data ranges:', {
                            unfiltered: {
                                count: quantities.length,
                                min: Math.min(...quantities),
                                max: Math.max(...quantities),
                                rangeLength: quantityRange.length
                            },
                            filtered: {
                                count: QuantityFiltered.length,
                                min: Math.min(...QuantityFiltered),
                                max: Math.max(...QuantityFiltered),
                                rangeLength: quantityRangeFiltered.length
                            }
                        });
                        
                        // Get cached adaptive bandwidth for both datasets
                        const adaptiveBandwidthUnfiltered = getCachedBandwidth(quantities, prices, false);
                        const adaptiveBandwidthFiltered = getCachedBandwidth(QuantityFiltered, PriceFiltered, true);

                        // LOESS fits with adaptive bandwidth
                        const loessUnfiltered = loessSmooth(quantities, prices, adaptiveBandwidthUnfiltered, quantityRange);
                        const loessFiltered = loessSmooth(QuantityFiltered, PriceFiltered, adaptiveBandwidthFiltered, quantityRangeFiltered);

                        // Bootstrap CI with adaptive bandwidth
                        const ciUnfiltered = bootstrapCI(quantities, prices, quantityRange, adaptiveBandwidthUnfiltered, 500);
                        const ciFiltered = bootstrapCI(QuantityFiltered, PriceFiltered, quantityRangeFiltered, adaptiveBandwidthFiltered, 500);
                        const lowerUnfiltered = quantityRange.map((q, i) => ({ x: q, y: ciUnfiltered.lower[i] })).filter(pt => pt.x > 0 && pt.y > 0 && isFinite(pt.x) && isFinite(pt.y));
                        const upperUnfiltered = quantityRange.map((q, i) => ({ x: q, y: ciUnfiltered.upper[i] })).filter(pt => pt.x > 0 && pt.y > 0 && isFinite(pt.x) && isFinite(pt.y));
                        const lowerFiltered = quantityRangeFiltered.map((q, i) => ({ x: q, y: ciFiltered.lower[i] })).filter(pt => pt.x > 0 && pt.y > 0 && isFinite(pt.x) && isFinite(pt.y));
                        const upperFiltered = quantityRangeFiltered.map((q, i) => ({ x: q, y: ciFiltered.upper[i] })).filter(pt => pt.x > 0 && pt.y > 0 && isFinite(pt.x) && isFinite(pt.y));
                        const filteredPoints = filtered.map(d => {
                            let price = d.p;
                            if (price > 0 && price < 0.01) {
                                price = 0.01;
                            }
                            return {
                                x: d.q,
                                y: price,
                                l: d.l,
                                p: d.pn
                            };
                        });
                        const loessLineUnfiltered = quantityRange.map((q, i) => {
                            let price = loessUnfiltered[i];
                            // Force values less than $0.01 (but greater than 0) to be $0.01 for chart display
                            if (price > 0 && price < 0.01) {
                                price = 0.01;
                            }
                            return { x: q, y: price };
                        }).filter(pt => pt.x > 0 && pt.y > 0 && isFinite(pt.x) && isFinite(pt.y));
                        
                        const loessLineFiltered = quantityRangeFiltered.map((q, i) => {
                            let price = loessFiltered[i];

                            if (price > 0 && price < 0.01) {
                                price = 0.01;
                            }
                            return { x: q, y: price };
                        }).filter(pt => pt.x > 0 && pt.y > 0 && isFinite(pt.x) && isFinite(pt.y));
                        
                        // Fill small gaps in LOESS lines by interpolating between valid points
                        const fillGapsInLine = (lineData) => {
                            if (lineData.length < 2) return lineData;
                            
                            const filled = [];
                            for (let i = 0; i < lineData.length; i++) {
                                filled.push(lineData[i]);
                                
                                // If there's a gap to the next point, add interpolated points
                                if (i < lineData.length - 1) {
                                    const current = lineData[i];
                                    const next = lineData[i + 1];
                                    const gap = next.x - current.x;
                                    
                                    // If gap is significant (more than 1.5x the average gap), add interpolated points
                                    const avgGap = (lineData[lineData.length - 1].x - lineData[0].x) / (lineData.length - 1);
                                    if (gap > avgGap * 1.5) {
                                        const numInterpolated = Math.min(Math.floor(gap / avgGap), 3); // Max 3 interpolated points
                                        for (let j = 1; j <= numInterpolated; j++) {
                                            const t = j / (numInterpolated + 1);
                                            const interpolatedX = current.x + t * gap;
                                            const interpolatedY = current.y + t * (next.y - current.y);
                                            filled.push({ x: interpolatedX, y: interpolatedY });
                                        }
                                    }
                                }
                            }
                            return filled.sort((a, b) => a.x - b.x);
                        };
                        
                        const filledLoessUnfiltered = fillGapsInLine(loessLineUnfiltered);
                        const filledLoessFiltered = fillGapsInLine(loessLineFiltered);
                        
                        // Add gap filling for confidence intervals as well
                        const fillGapsInConfidenceIntervals = (lowerData, upperData) => {
                            if (lowerData.length < 2 || upperData.length < 2) return { lower: lowerData, upper: upperData };
                            
                            const allX = [...new Set([...lowerData.map(d => d.x), ...upperData.map(d => d.x)])].sort((a, b) => a - b);
                            const filledLower = [];
                            const filledUpper = [];
                            
                            for (let i = 0; i < allX.length; i++) {
                                const x = allX[i];
                                const lowerPoint = lowerData.find(d => d.x === x);
                                const upperPoint = upperData.find(d => d.x === x);
                                
                                if (lowerPoint && upperPoint) {
                                    filledLower.push(lowerPoint);
                                    filledUpper.push(upperPoint);
                                } else if (i > 0 && i < allX.length - 1) {
                                    // Interpolate missing points
                                    const prevLower = lowerData.find(d => d.x < x);
                                    const nextLower = lowerData.find(d => d.x > x);
                                    const prevUpper = upperData.find(d => d.x < x);
                                    const nextUpper = upperData.find(d => d.x > x);
                                    
                                    if (prevLower && nextLower && prevUpper && nextUpper) {
                                        const t = (x - prevLower.x) / (nextLower.x - prevLower.x);
                                        const interpolatedLower = prevLower.y + t * (nextLower.y - prevLower.y);
                                        const interpolatedUpper = prevUpper.y + t * (nextUpper.y - prevUpper.y);
                                        
                                        filledLower.push({ x, y: interpolatedLower });
                                        filledUpper.push({ x, y: interpolatedUpper });
                                    }
                                }
                            }
                            
                            return { lower: filledLower, upper: filledUpper };
                        };
                        
                        const filledConfidenceUnfiltered = fillGapsInConfidenceIntervals(lowerUnfiltered, upperUnfiltered);
                        const filledConfidenceFiltered = fillGapsInConfidenceIntervals(lowerFiltered, upperFiltered);

                        if (quantities.length === 0 || prices.length === 0) {
                            $scope.isChartLoading = false;
                            return;
                        }

                        const outlierPoints = [];
                        const normalPoints = [];
                        const bidPoints = [];

                        for (let i = 0; i < $scope.bidHistoryData.length; i++) {
                            const item = $scope.bidHistoryData[i];
                            const quantity = item.Quantity || 0;
                            let price = $scope.getPriceField(item) || 0;

                            if (price > 0 && price < 0.01) {
                                price = 0.01;
                            }

                            bidPoints.push({
                                x: quantity, // Quantity
                                y: price, // Price
                                l: item.l || "N/A", // Letting Date 
                                p: item.p || "N/A"  // Contract Number
                            });
                        }

                        const totalQty = quantities.reduce((sum, q) => sum + q, 0);

                        if (totalQty === 0) {
                            $scope.isChartLoading = false;
                            return;
                        }

                        const weightedAvg = quantities.reduce((sum, q, i) => sum + (q * prices[i]), 0) / totalQty;

                        const weightedStdDev = Math.sqrt(
                            quantities.reduce((sum, q, i) => sum + q * Math.pow(prices[i] - weightedAvg, 2), 0) / totalQty
                        );

                        bidPoints.forEach(point => {
                            const isOutlier = Math.abs(point.y - weightedAvg) > weightedStdDev;
                            if (isOutlier) {
                                outlierPoints.push({
                                    x: point.x,
                                    y: point.y,
                                    l: point.l,
                                    p: point.p
                                });
                            } else {
                                normalPoints.push({
                                    x: point.x,
                                    y: point.y,
                                    l: point.l,
                                    p: point.p
                                });
                            }
                        });
                        if ($scope.chartInstance) {
                            $scope.chartInstance.destroy();
                            $scope.chartInstance = null;
                        }
                        const chartContainer = document.getElementById('priceChart').parentNode;
                        const oldCanvas = document.getElementById('priceChart');
                        if (oldCanvas) {
                            chartContainer.removeChild(oldCanvas);
                        }
                        const newCanvas = document.createElement('canvas');
                        newCanvas.id = 'priceChart';
                        newCanvas.style.width = '100%';
                        newCanvas.style.height = '400px';
                        newCanvas.style.background = 'white';
                        chartContainer.appendChild(newCanvas);
                        const newCtx = newCanvas.getContext('2d');

                        if (typeof Chart === 'undefined') {
                            console.error('Chart.js is not loaded');
                            $scope.isChartLoading = false;
                            return;
                        }
                        $scope.chartInstance = new Chart(newCtx, {
                            type: 'scatter',
                            data: {
                                datasets: [

                                    {
                                        label: 'LOESS',
                                        data: filledLoessUnfiltered,
                                        type: 'line',
                                        borderColor: 'red',
                                        borderWidth: 2,
                                        fill: false,
                                        tension: 0.4,
                                        cubicInterpolationMode: 'monotone',
                                        stepped: false
                                    },
                                    {
                                        label: 'LOESS (No Outliers)',
                                        data: filledLoessFiltered,
                                        type: 'line',
                                        borderColor: 'blue',
                                        borderWidth: 2,
                                        fill: false,
                                        tension: 0.4,
                                        cubicInterpolationMode: 'monotone',
                                        stepped: false

                                    },
                                    {
                                        label: 'Weighted Avg',
                                        data: [
                                            { x: Math.min(...quantities), y: weightedMean > 0 && weightedMean < 0.01 ? 0.01 : weightedMean },
                                            { x: Math.max(...quantities), y: weightedMean > 0 && weightedMean < 0.01 ? 0.01 : weightedMean }
                                        ],
                                        type: 'line',
                                        borderColor: 'black',
                                        borderDash: [5, 5],
                                        fill: false,
                                        borderWidth: 1
                                    },
                                    {
                                        label: 'Weighted Avg (No Outliers)',
                                        data: [
                                            { x: Math.min(...QuantityFiltered), y: $scope.weightedAvgNoOutliers > 0 && $scope.weightedAvgNoOutliers < 0.01 ? 0.01 : $scope.weightedAvgNoOutliers },
                                            { x: Math.max(...QuantityFiltered), y: $scope.weightedAvgNoOutliers > 0 && $scope.weightedAvgNoOutliers < 0.01 ? 0.01 : $scope.weightedAvgNoOutliers }
                                        ],
                                        type: 'line',
                                        borderColor: '#6366f1',
                                        borderDash: [8, 8],
                                        fill: false,
                                        borderWidth: 1
                                    },
                                    {
                                        label: '95% CI Outliers (Lower)',
                                        data: filledConfidenceUnfiltered.lower,
                                        type: 'line',
                                        borderColor: 'rgba(255,0,0,0.5)',
                                        backgroundColor: 'rgba(255,0,0,0.1)',
                                        fill: '+1', // fill to next dataset
                                        pointRadius: 0,
                                        borderWidth: 2,
                                        tension: 0.4,
                                        order: 0
                                    },
                                    {
                                        label: '95% CI Outliers (Upper)',
                                        data: filledConfidenceUnfiltered.upper,
                                        type: 'line',
                                        borderColor: 'rgba(255,0,0,0.5)',
                                        backgroundColor: 'rgba(255,0,0,0.1)',
                                        fill: false,
                                        pointRadius: 0,
                                        borderWidth: 2,
                                        tension: 0.4,
                                        order: 0
                                    },

                                    {
                                        label: '95% CI No Outliers (Lower)',
                                        data: filledConfidenceFiltered.lower,
                                        type: 'line',
                                        borderColor: 'rgba(0,0,255,0.5)',
                                        backgroundColor: 'rgba(0,0,255,0.1)',
                                        fill: '+1', // fill to next dataset
                                        pointRadius: 0,
                                        borderWidth: 2,
                                        tension: 0.4,
                                        order: 0
                                    },
                                    {
                                        label: '95% CI No Outliers (Upper)',
                                        data: filledConfidenceFiltered.upper,
                                        type: 'line',
                                        borderColor: 'rgba(0,0,255,0.5)',
                                        backgroundColor: 'rgba(0,0,255,0.1)',
                                        fill: false,
                                        pointRadius: 0,
                                        borderWidth: 2,
                                        tension: 0.4,
                                        order: 0
                                    },
                                    {
                                        label: 'Bid Point Outlier',
                                        data: outlierPoints,
                                        backgroundColor: 'rgba(128, 128, 128, 0.3)',
                                        pointRadius: 5,
                                    },
                                    {
                                        label: 'Bid Point',
                                        data: filteredPoints,
                                        backgroundColor: 'rgba(0, 128, 0, 0.5)',
                                        pointRadius: 5,
                                    },

                                ]

                            },
                            options: {
                                responsive: true,
                                maintainAspectRatio: false,
                                scales: {
                                    x: {
                                        type: 'logarithmic',
                                        title: { display: true, text: 'Quantity (log scale)' },
                                        grid: {
                                            display: true,
                                            drawTicks: true,
                                            tickLength: 8,
                                            color: 'rgba(0,0,0,0.1)',
                                            maxTicksLimit: 6
                                        },
                                        ticks: {
                                            maxTicksLimit: 6,
                                            callback: function (value) {

                                                if (value >= 1000000) {
                                                    return (value / 1000000).toFixed(1) + 'M';
                                                } else if (value >= 1000) {
                                                    return (value / 1000).toFixed(1) + 'K';
                                                } else {
                                                    return Number(value.toString());
                                                }
                                            }
                                        }
                                    },
                                    y: {
                                        type: 'logarithmic',
                                        title: { display: true, text: 'Unit Price (log scale)' },
                                        grid: {
                                            display: true,
                                            drawTicks: true,
                                            tickLength: 8,
                                            color: 'rgba(0,0,0,0.1)',
                                            maxTicksLimit: 6
                                        },
                                        ticks: {
                                            maxTicksLimit: 6,
                                            callback: function (value, index, values) {
                                                // Format currency nicely for log scale
                                                if (value >= 1000) {
                                                    return '$' + (value / 1000).toFixed(1) + 'K';
                                                } else if (value >= 1) {
                                                    return '$' + value.toFixed(0);
                                                } else if (value > 0) {
                                                    return '$' + value.toFixed(2);
                                                } else {
                                                    return '$0.00';
                                                }
                                            }
                                        }
                                    }
                                },
                                plugins: {
                                    tooltip: {
                                        backgroundColor: 'rgba(15, 23, 42, 0.95)',
                                        titleColor: '#ffffff',
                                        bodyColor: '#ffffff',
                                        borderColor: '#3b82f6',
                                        borderWidth: 2,
                                        cornerRadius: 10,
                                        displayColors: false,
                                        titleFont: {
                                            size: 14,
                                            weight: 'bold',
                                            family: '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif'
                                        },
                                        bodyFont: {
                                            size: 12,
                                            weight: 'normal',
                                            family: '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif'
                                        },
                                        padding: {
                                            top: 14,
                                            right: 18,
                                            bottom: 14,
                                            left: 18
                                        },
                                        titleMarginBottom: 10,
                                        bodySpacing: 6,
                                        callbacks: {
                                            title: function (context) {
                                                const datasetLabel = context[0].dataset.label || 'Data Point';
                                                if (datasetLabel.includes('LOESS')) {
                                                    return '📈 Trend Analysis';
                                                } else if (datasetLabel.includes('Weighted Avg')) {
                                                    return '📊 Statistical Averages';
                                                } else if (datasetLabel.includes('95% CI')) {
                                                    return '📉 Confidence Intervals';
                                                } else if (datasetLabel.includes('Bid Point')) {
                                                    return '🎯 Actual Bid Data';
                                                } else {
                                                    return datasetLabel;
                                                }
                                            },
                                            label: function (context) {
                                                const label = context.dataset.label || '';
                                                const point = context.raw;
                                                const qty = point.x;
                                                const price = point.y;
                                                const lines = [];
                                                if (label.includes('LOESS')) {
                                                    lines.push(`📈 ${label}`);
                                                } else if (label.includes('Weighted Avg')) {
                                                    lines.push(`📊 ${label}`);
                                                } else if (label.includes('95% CI')) {
                                                    lines.push(`📉 ${label}`);
                                                } else if (label.includes('Bid Point')) {
                                                    lines.push(`🎯 ${label}`);
                                                } else {
                                                    lines.push(`📋 ${label}`);
                                                }
                                                lines.push(`📊 Quantity: ${qty.toLocaleString()}`);
                                                // Format price appropriately based on its size
                                                let formattedPrice;
                                                if (price >= 1000) {
                                                    formattedPrice = "$" + (price / 1000).toFixed(1) + "K";
                                                } else if (price >= 1) {
                                                    formattedPrice = "$" + price.toFixed(2);
                                                } else if (price >= 0.01) {
                                                    formattedPrice = "$" + price.toFixed(2);
                                                } else if (price > 0 && price < 0.01) {

                                                    formattedPrice = "$" + price.toFixed(4);
                                                } else {

                                                    formattedPrice = "$" + price.toFixed(2);
                                                }
                                                lines.push(`💰 Price: ${formattedPrice}`);
                                                if (label === 'Bid Point Outlier' || label === 'Bid Point') {
                                                    const lettingDate = point.l || 'N/A';
                                                    const contract = point.p || 'N/A';
                                                    lines.push(`📅 Letting Date: ${formatDotNetDate(lettingDate)}`);
                                                    lines.push(`📄 Contract #: ${contract}`);
                                                }
                                                if (label.includes('LOESS') || label.includes('Weighted Avg')) {
                                                    lines.push(`📈 Trend Line Value`);
                                                }
                                                if (label.includes('95% CI')) {
                                                    if (label.includes('Lower')) {
                                                        lines.push(`📉 Lower Confidence Bound`);
                                                    } else if (label.includes('Upper')) {
                                                        lines.push(`📉 Upper Confidence Bound`);
                                                    }
                                                }

                                                return lines;
                                            },
                                            afterLabel: function (context) {
                                                const label = context.dataset.label || '';
                                                if (label.includes('Bid Point Outlier')) {
                                                    return ['⚠️ This point is identified as an outlier', 'based on statistical analysis'];
                                                } else if (label.includes('Bid Point')) {
                                                    return ['✅ This is a normal bid point', 'within expected statistical range'];
                                                } else if (label.includes('LOESS')) {
                                                    return ['📈 Smoothed trend line', 'showing price patterns'];
                                                } else if (label.includes('Weighted Avg')) {
                                                    return ['📊 Statistical average', 'weighted by quantity'];
                                                } else if (label.includes('95% CI')) {
                                                    return ['📉 Confidence interval', 'showing statistical uncertainty'];
                                                }
                                                return [];
                                            }
                                        }
                                    },
                                    legend: {
                                        display: true
                                    }
                                }
                            }
                        });
                        // Calculate stats
                        $scope.chartStats = {
                            avg: weightedAvg,
                            weightedAvgNoOutliers: $scope.weightedAvgNoOutliers,
                            totalContracts: new Set($scope.bidHistoryData.map(item => item.p)).size,
                            totalBidAmount: $scope.bidHistoryData.reduce((sum, item) => sum + (item.PvBidTotal || 0), 0),
                            totalQuantity: $scope.bidHistoryData.reduce((sum, item) => sum + (item.Quantity || 0), 0),
                            count: $scope.bidHistoryData.length,
                            avgQty: $scope.bidHistoryData.reduce((sum, item) => sum + (item.Quantity || 0), 0) / $scope.bidHistoryData.length,
                            outlierCount: $scope.bidHistoryData.filter(item => item.IsOutlier).length,

                            avgCurrentPrice: $scope.bidHistoryData.reduce((sum, item) => sum + ($scope.getPriceField(item) || 0), 0) / $scope.bidHistoryData.length,
                            avgInflationAdjustedPrice: $scope.bidHistoryData.reduce((sum, item) => sum + (item.InflationAdjustedPrice || item.b || 0), 0) / $scope.bidHistoryData.length,
                            maxInflationIncrease: Math.max(...$scope.bidHistoryData.map(item => item.InflationPercentIncrease || 0)),
                            minInflationIncrease: Math.min(...$scope.bidHistoryData.map(item => item.InflationPercentIncrease || 0)),
                            avgInflationIncrease: $scope.bidHistoryData.reduce((sum, item) => sum + (item.InflationPercentIncrease || 0), 0) / $scope.bidHistoryData.length,

                            currentPriceField: $scope.useInflationAdjustedPrices ? 'Inflation-Adjusted' : 'Raw'
                        };
                        $scope.isChartLoading = false;
                        $scope.isChartStale = false;
                        $scope.$apply();
                    });
                } else {
                    // Fallback if requestAnimationFrame is not available
                    console.warn('requestAnimationFrame not available, using setTimeout');
                    setTimeout(function () {

                        $scope.isChartLoading = false;
                        $scope.isChartStale = false;
                    }, 16);
                }
            }, 0);
        }
        // Trend Analysis Functions
        function processTrendData() {
            if (!$scope.bidHistoryData || $scope.bidHistoryData.length === 0) {
                return [];
            }
            const validData = $scope.bidHistoryData.filter(item =>
                item.l && item.Quantity && $scope.getPriceField(item)
            );
            if (validData.length === 0) {
                return [];
            }
            const groupedData = {};
            validData.forEach(item => {
                const lettingDate = new Date(parseInt(item.l.replace(/\/Date\((\d+)\)\//, '$1')));
                let timeKey;
                console.log($scope.trendAnalysisData.trendTimeGrouping);
                switch ($scope.trendAnalysisData.trendTimeGrouping) {
                    case 'year':
                        timeKey = lettingDate.getFullYear();
                        break;
                    case 'quarter':
                        const quarter = Math.floor(lettingDate.getMonth() / 3) + 1;
                        timeKey = `${lettingDate.getFullYear()}-Q${quarter}`;
                        break;
                    case 'month':
                        timeKey = `${lettingDate.getFullYear()}-${String(lettingDate.getMonth() + 1).padStart(2, '0')}`;
                        break;
                    default:
                        timeKey = lettingDate.getFullYear();
                }
                if (!groupedData[timeKey]) {
                    groupedData[timeKey] = {
                        quantities: [],
                        prices: [],
                        totalQuantity: 0,
                        totalAmount: 0,
                        count: 0
                    };
                }
                groupedData[timeKey].quantities.push(item.Quantity);
                groupedData[timeKey].prices.push($scope.getPriceField(item));
                groupedData[timeKey].totalQuantity += item.Quantity;
                groupedData[timeKey].totalAmount += item.PvBidTotal || 0;
                groupedData[timeKey].count++;
            });
            const trendData = Object.keys(groupedData).map(timeKey => {
                const data = groupedData[timeKey];
                const weightedAvg = data.totalQuantity > 0 ?
                    data.quantities.reduce((sum, qty, idx) => sum + (qty * data.prices[idx]), 0) / data.totalQuantity : 0;

                return {
                    timeKey: timeKey,
                    weightedAvg: weightedAvg,
                    totalQuantity: data.totalQuantity,
                    totalAmount: data.totalAmount,
                    count: data.count,
                    date: parseTimeKeyToDate(timeKey)
                };
            });
            trendData.sort((a, b) => a.date - b.date);
            // Check for sparse data
            const sparseIntervals = trendData.filter(item => item.count < 5);
            if (sparseIntervals.length > 0) {
                const timeUnit = $scope.trendAnalysisData.trendTimeGrouping === 'year' ? 'years' :
                    $scope.trendAnalysisData.trendTimeGrouping === 'quarter' ? 'quarters' : 'months';
                $scope.trendWarning = `Warning: Some of your time intervals (${timeUnit}) include fewer than 5 contracts, which may affect the accuracy of the calculated average. Consider selecting a broader time range for a more stable trend.`;
            } else {
                $scope.trendWarning = '';
            }

            let limit;
            switch ($scope.trendAnalysisData.trendTimeGrouping) {
                case 'year':
                    limit = 10;
                    break;
                case 'quarter':
                    limit = 12;
                    break;
                case 'month':
                    limit = 12;
                    break;
                default:
                    limit = 10;
            }

            return trendData.slice(-limit);
        }

        function parseTimeKeyToDate(timeKey) {
            if (timeKey.includes('-Q')) {
                const [year, quarter] = timeKey.split('-Q');
                const month = (parseInt(quarter) - 1) * 3;
                return new Date(parseInt(year), month, 1);
            } else if (timeKey.includes('-')) {
                const [year, month] = timeKey.split('-');
                return new Date(parseInt(year), parseInt(month) - 1, 1);
            } else {
                return new Date(parseInt(timeKey), 0, 1);
            }
        }
        function formatTimeKey(timeKey) {
            if (timeKey.includes('-Q')) {
                const [year, quarter] = timeKey.split('-Q');
                return `Q${quarter} ${year}`;
            } else if (timeKey.includes('-') && timeKey.length === 7) {
                const [year, month] = timeKey.split('-');
                const monthNames = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun',
                    'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
                return `${monthNames[parseInt(month) - 1]} ${year}`;
            } else {
                return timeKey;
            }
        }
        function renderTrendChart() {
            $scope.isTrendChartLoading = true;

            $timeout(function () {
                const canvas = document.getElementById("trendChart");
                if (!canvas) {
                    $scope.isTrendChartLoading = false;
                    return;
                }

                $scope.trendData = processTrendData();

                if ($scope.trendData.length === 0) {
                    $scope.isTrendChartLoading = false;
                    return;
                }

                if ($scope.trendChartInstance) {
                    $scope.trendChartInstance.destroy();
                    $scope.trendChartInstance = null;
                }

                const chartContainer = document.getElementById('trendChart').parentNode;
                const oldCanvas = document.getElementById('trendChart');
                if (oldCanvas) {
                    chartContainer.removeChild(oldCanvas);
                }

                const newCanvas = document.createElement('canvas');
                newCanvas.id = 'trendChart';
                newCanvas.style.width = '100%';
                newCanvas.style.height = '400px';
                newCanvas.style.background = 'white';
                chartContainer.appendChild(newCanvas);

                const newCtx = newCanvas.getContext('2d');

                const chartData = $scope.trendData.map(item => ({
                    x: item.timeKey,
                    y: item.weightedAvg,
                    totalQuantity: item.totalQuantity,
                    totalAmount: item.totalAmount,
                    count: item.count
                }));

                const labels = chartData.map(item => formatTimeKey(item.x));

                if (typeof Chart === 'undefined') {
                    console.error('Chart.js is not loaded');
                    $scope.isTrendChartLoading = false;
                    return;
                }
                $scope.trendChartInstance = new Chart(newCtx, {
                    type: 'line',
                    data: {
                        labels: labels,
                        datasets: [{
                            label: 'Weighted Average Unit Price (' + ($scope.useInflationAdjustedPrices ? 'Inflation-Adjusted' : 'Raw') + ')',
                            data: chartData.map(item => item.y),
                            borderColor: '#1F4283',
                            backgroundColor: 'rgba(31, 66, 131, 0.1)',
                            borderWidth: 3,
                            fill: true,
                            tension: 0.4,
                            pointBackgroundColor: '#1F4283',
                            pointBorderColor: '#ffffff',
                            pointBorderWidth: 2,
                            pointRadius: 6,
                            pointHoverRadius: 8,
                            pointHoverBackgroundColor: '#152C57',
                            pointHoverBorderColor: '#ffffff'
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        scales: {
                            x: {
                                title: {
                                    display: true,
                                    text: $scope.trendAnalysisData.trendTimeGrouping === 'year' ? 'Year' :
                                        $scope.trendAnalysisData.trendTimeGrouping === 'quarter' ? 'Quarter' : 'Month',
                                    font: { size: 14, weight: 'bold' }
                                },
                                grid: {
                                    display: true,
                                    color: 'rgba(0,0,0,0.1)'
                                }
                            },
                            y: {
                                title: {
                                    display: true,
                                    text: 'Weighted Average Unit Price (' + ($scope.useInflationAdjustedPrices ? 'Inflation-Adjusted' : 'Raw') + ') ($)',
                                    font: { size: 14, weight: 'bold' }
                                },
                                grid: {
                                    display: true,
                                    color: 'rgba(0,0,0,0.1)'
                                },
                                ticks: {
                                    callback: function (value, index, values) {
                                        if (value >= 1000) {
                                            return '$' + (value / 1000).toFixed(1) + 'K';
                                        } else if (value >= 1) {
                                            return '$' + value.toFixed(2);
                                        } else if (value > 0) {
                                            return '$' + value.toFixed(2);
                                        } else {
                                            return '$0.00';
                                        }
                                    }
                                }
                            }
                        },
                        plugins: {
                            tooltip: {
                                backgroundColor: 'rgba(15, 23, 42, 0.95)',
                                titleColor: '#ffffff',
                                bodyColor: '#ffffff',
                                borderColor: '#3b82f6',
                                borderWidth: 2,
                                cornerRadius: 10,
                                displayColors: false,
                                titleFont: {
                                    size: 14,
                                    weight: 'bold'
                                },
                                bodyFont: {
                                    size: 12,
                                    weight: 'normal'
                                },
                                padding: {
                                    top: 14,
                                    right: 18,
                                    bottom: 14,
                                    left: 18
                                },
                                callbacks: {
                                    title: function (context) {
                                        return '📈 Price Trend Analysis';
                                    },
                                    label: function (context) {
                                        const dataPoint = $scope.trendData[context.dataIndex];
                                        const lines = [];
                                        lines.push(`📅 Period: ${formatTimeKey(dataPoint.timeKey)}`);
                                        lines.push(`💰 Weighted Avg Price: $${dataPoint.weightedAvg.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`);
                                        lines.push(`📊 Total Quantity: ${dataPoint.totalQuantity.toLocaleString()}`);
                                        lines.push(`💵 Total Amount: $${dataPoint.totalAmount.toLocaleString()}`);
                                        lines.push(`📋 Number of Bids: ${dataPoint.count}`);
                                        return lines;
                                    }
                                }
                            },
                            legend: {
                                display: false,

                            }
                        }
                    }
                });

                $scope.isTrendChartLoading = false;
            }, 0);
        }
        $scope.onTrendTimeGroupingChange = function () {
            if ($scope.showTrendChart) {
                $timeout(function () {
                    renderTrendChart();
                }, 0);
            }
        };

        $scope.toggleTrendChart = function () {
            $scope.showTrendChart = !$scope.showTrendChart;
            if ($scope.showTrendChart) {
                renderTrendChart();
            } else if ($scope.trendChartInstance) {
                $scope.trendChartInstance.destroy();
                $scope.trendChartInstance = null;
            }
        };
        $scope.getBidTypeLabel = function (code) {
            return code ? ($scope.bidTypeMap[code] || "Unknown") : "Unknown";
        };

        $scope.getBidStatusLabel = function (code) {
            return code ? ($scope.bidStatusMap[code] || "Unknown") : "Unknown";
        };

        $scope.getInflationInfo = function (item) {
            if (!item.InflationAdjustedPrice || !item.InflationPercentIncrease) {
                return "No inflation data available";
            }
            return `Adjusted to 2024 Q4 (${item.InflationPercentIncrease.toFixed(1)}% increase)`;
        };

        // Custom Quantity Analysis - LOESS Prediction
        $scope.calculateCustomQuantityStats = function () {
            if (!$scope.customQuantityData.userQuantity || $scope.customQuantityData.userQuantity <= 0 || !$scope.bidHistoryData || $scope.bidHistoryData.length === 0) {
                $scope.customQuantityPrediction = null;
                return;
            }

            $scope.isCalculatingPrediction = true;

            try {

                const quantities = $scope.bidHistoryData.map(item => item.Quantity || 0);
                const prices = $scope.bidHistoryData.map(item => $scope.getPriceField(item) || 0);

                const validData = quantities.map((qty, i) => ({ qty, price: prices[i] }))
                    .filter(item => item.qty > 0 && item.price > 0 && isFinite(item.qty) && isFinite(item.price));

                if (validData.length < 3) {
                    $scope.customQuantityPrediction = {
                        success: false,
                        message: "Insufficient data points for LOESS prediction (need at least 3 valid data points)"
                    };
                    return;
                }

                const x = validData.map(item => item.qty);
                const y = validData.map(item => item.price);

                // Calculate weighted statistics using validData only
                const totalQty = x.reduce((sum, q) => sum + q, 0);
                const weightedAvg = x.reduce((sum, q, i) => sum + (q * y[i]), 0) / totalQty;

                const weightedStdDev = Math.sqrt(
                    x.reduce((sum, q, i) => sum + q * Math.pow(y[i] - weightedAvg, 2), 0) / totalQty
                );

                // Filter outliers from validData (not from original arrays)
                const cleanData = validData.filter(item => Math.abs(item.price - weightedAvg) <= weightedStdDev);

                const cleanTotalQty = cleanData.reduce((sum, item) => sum + item.qty, 0);
                const weightedAvgNoOutliers = cleanTotalQty > 0 ?
                    cleanData.reduce((sum, item) => sum + (item.qty * item.price), 0) / cleanTotalQty : 0;

                // Get cached adaptive bandwidth for both datasets
                const adaptiveBandwidthUnfiltered = getCachedBandwidth(x, y, false, $scope.customQuantityData.userQuantity);
                const adaptiveBandwidthFiltered = getCachedBandwidth(cleanData.map(item => item.qty), cleanData.map(item => item.price), true, $scope.customQuantityData.userQuantity);

                const prediction = loessSmooth(x, y, adaptiveBandwidthUnfiltered, [$scope.customQuantityData.userQuantity]);

                const loessUnfiltered = loessSmooth(x, y, adaptiveBandwidthUnfiltered, [$scope.customQuantityData.userQuantity]);
                const loessFiltered = loessSmooth(cleanData.map(item => item.qty), cleanData.map(item => item.price), adaptiveBandwidthFiltered, [$scope.customQuantityData.userQuantity]);

                // Calculate confidence intervals using bootstrap with adaptive bandwidth
                const ciUnfiltered = bootstrapCI(x, y, [$scope.customQuantityData.userQuantity], adaptiveBandwidthUnfiltered, 500);
                const ciFiltered = bootstrapCI(cleanData.map(item => item.qty), cleanData.map(item => item.price), [$scope.customQuantityData.userQuantity], adaptiveBandwidthFiltered, 500);

                if (prediction && prediction.length > 0 && isFinite(prediction[0]) && prediction[0] > 0) {
                    const predictedPrice = prediction[0];
                    const totalCost = $scope.customQuantityData.userQuantity * predictedPrice;

                    // Check for counterintuitive LOESS results
                    const loessUnfilteredValue = loessUnfiltered[0] || 0;
                    const loessFilteredValue = loessFiltered[0] || 0;
                    const hasCounterintuitiveResult = loessFilteredValue > loessUnfilteredValue;

                    // Determine explanation for counterintuitive results
                    let counterintuitiveExplanation = '';
                    if (hasCounterintuitiveResult) {
                        const userQty = $scope.customQuantityData.userQuantity;
                        const filteredQuantities = cleanData.map(item => item.qty);
                        const allQuantities = x;

                        const minFiltered = Math.min(...filteredQuantities);
                        const maxFiltered = Math.max(...filteredQuantities);
                        const minAll = Math.min(...allQuantities);
                        const maxAll = Math.max(...allQuantities);

                        if (userQty < minFiltered || userQty > maxFiltered) {
                            counterintuitiveExplanation = 'This result occurs because your quantity is outside the range of non-outlier data, causing different extrapolation behavior.';
                        } else {
                            // Calculate the difference to provide more specific explanation
                            const difference = loessFilteredValue - loessUnfilteredValue;
                            const percentChange = ((difference / loessUnfilteredValue) * 100).toFixed(1);

                            counterintuitiveExplanation = `This result occurs because removing outliers changed the local data patterns around your quantity (${userQty.toLocaleString()}). The LOESS algorithm detected a different local trend, resulting in a ${percentChange}% change in prediction. This is normal for local regression methods when data distribution changes.`;
                        }
                    }

                    $scope.customQuantityPrediction = {
                        success: true,
                        userQuantity: $scope.customQuantityData.userQuantity,
                        predictedUnitPrice: parseFloat(predictedPrice.toFixed(4)),
                        totalCost: parseFloat(totalCost.toFixed(2)),
                        priceType: $scope.useInflationAdjustedPrices ? 'Inflation-Adjusted' : 'Raw',
                        loessUnfiltered: parseFloat(loessUnfilteredValue.toFixed(4)),
                        loessFiltered: parseFloat(loessFilteredValue.toFixed(4)),
                        weightedAvg: parseFloat(weightedAvg.toFixed(4)),
                        weightedAvgNoOutliers: parseFloat(weightedAvgNoOutliers.toFixed(4)),
                        dataPoints: validData.length,
                        dataPointsNoOutliers: cleanData.length,
                        loessUnfilteredTotal: parseFloat((loessUnfilteredValue * $scope.customQuantityData.userQuantity).toFixed(2)),
                        loessFilteredTotal: parseFloat((loessFilteredValue * $scope.customQuantityData.userQuantity).toFixed(2)),
                        weightedAvgTotal: parseFloat((weightedAvg * $scope.customQuantityData.userQuantity).toFixed(2)),
                        weightedAvgNoOutliersTotal: parseFloat((weightedAvgNoOutliers * $scope.customQuantityData.userQuantity).toFixed(2)),
                        // Confidence intervals for LOESS predictions
                        loessUnfilteredLower: ciUnfiltered.lower[0] ? parseFloat(ciUnfiltered.lower[0].toFixed(4)) : 0,
                        loessUnfilteredUpper: ciUnfiltered.upper[0] ? parseFloat(ciUnfiltered.upper[0].toFixed(4)) : 0,
                        loessFilteredLower: ciFiltered.lower[0] ? parseFloat(ciFiltered.lower[0].toFixed(4)) : 0,
                        loessFilteredUpper: ciFiltered.upper[0] ? parseFloat(ciFiltered.upper[0].toFixed(4)) : 0,
                        // Counterintuitive result detection
                        hasCounterintuitiveResult: hasCounterintuitiveResult,
                        counterintuitiveExplanation: counterintuitiveExplanation
                    };
                } else {
                    $scope.customQuantityPrediction = {
                        success: false,
                        message: "Unable to generate prediction for this quantity. Try a different value."
                    };
                }

            } catch (error) {
                console.error("Error calculating LOESS prediction:", error);
                $scope.customQuantityPrediction = {
                    success: false,
                    message: "Error calculating prediction: " + error.message
                };
            } finally {
                $scope.isCalculatingPrediction = false;
            }
        };

        // Check if user quantity is within valid range for LOESS prediction
        $scope.isUserQuantityInRange = function () {
            if (!$scope.customQuantityData.userQuantity || $scope.customQuantityData.userQuantity <= 0) {
                return false;
            }

            if (!$scope.bidHistoryData || $scope.bidHistoryData.length === 0) {
                return false;
            }
            // Get the range of historical quantities
            const quantities = $scope.bidHistoryData.map(item => item.Quantity || 0).filter(qty => qty > 0);
            if (quantities.length === 0) {
                return false;
            }

            const minQuantity = Math.min(...quantities);
            const maxQuantity = Math.max(...quantities);
            const userQty = $scope.customQuantityData.userQuantity;
            return userQty >= minQuantity * 0.1 && userQty <= maxQuantity * 10;
        };

        // Get the valid range for LOESS prediction
        $scope.getValidQuantityRange = function () {
            if (!$scope.bidHistoryData || $scope.bidHistoryData.length === 0) {
                return { min: 0, max: 0 };
            }



            const quantities = $scope.bidHistoryData.map(item => item.Quantity || 0).filter(qty => qty > 0);
            if (quantities.length === 0) {
                return { min: 0, max: 0 };
            }

            const minQuantity = Math.min(...quantities);
            const maxQuantity = Math.max(...quantities);

            return {
                min: minQuantity * 0.1,
                max: maxQuantity * 10
            };
        };
        // Cleanup when controller is destroyed
        $scope.$on('$destroy', function () {
            $rootScope.showStatisticsDetails = false;
        });



    }
]);
angular.module('dqeControllers')
    .filter('msDateToJS', function () {
        return function (input) {
            if (!input) return '';
            var match = /\/Date\((\d+)\)\//.exec(input);
            return match ? new Date(parseInt(match[1])) : input;
        };
    })
    .filter('smartCurrency', function () {
        return function (input) {
            if (input === null || input === undefined || isNaN(input)) {
                return '$0.00';
            }

            var value = parseFloat(input);

            if (value > 0 && value < 0.01) {
                return '$0.01';
            }
            else if (value >= 0.01 && value < 1) {
                return '$' + value.toFixed(2);
            }
            else if (value >= 1) {
                return '$' + value.toLocaleString('en-US', {
                    minimumFractionDigits: 2,
                    maximumFractionDigits: 2
                });
            }
            else {
                return '$' + value.toFixed(2);
            }
        };
    });