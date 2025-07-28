dqeControllers.controller('UnitPriceSearchController', ['$scope', '$rootScope', '$http', '$timeout',
    function ($scope, $rootScope, $http, $timeout) {
        $rootScope.$broadcast('initializeNavigation');
        $scope.searchText = "";
        $scope.items = [];
        $scope.selectedPayItemNumber = null;
        $scope.bidHistoryData = [];
        $scope.lastSearchedPayItem = $scope.searchText;
        $scope.isLoading = false;
        $scope.draggingThumb = null;
        $scope.monthsOfHistory = 12;
        $scope.regionType = '';
        $scope.regionOptions = [];
        $scope.selectedRegions = [];
        $scope.relatedCounties = [];
        $scope.selectedRegionCounties = [];
        $scope.isRegionDropdownOpen = false;
        $scope.selectedBidStatus = "FMV";
        $scope.searchAttempted = false;
        $scope.showNormal = true;
        $scope.showOutliers = true;
        $scope.showTrendLine = true;
        $scope.showWeightedAvg = true;
        $scope.sortColumn = 'p'; // Default sort by Contract
        $scope.reverseSort = false;
        let debounceTimer;
        $scope.isChartLoading = false;
        const today = new Date();
        const pastLimit = new Date();
        pastLimit.setMonth(pastLimit.getMonth() - 120);
        $scope.today = today;
        $scope.minAllowedDate = pastLimit;
        
        // Column selection functionality
        $scope.availableColumns = [
            { key: 'p', label: 'Contract', visible: true, sortable: true },
            { key: 'ProjectNumber', label: 'Project Number', visible: true, sortable: true },
            { key: 'ri', label: 'Pay Item', visible: true, sortable: true },
            { key: 'Description', label: 'Description', visible: false, sortable: true },
            { key: 'SupplementalDescription', label: 'Supp Desc', visible: false, sortable: false },
            { key: 'CalculatedUnit', label: 'Units', visible: false, sortable: false },
            { key: 'Quantity', label: 'Quantity', visible: true, sortable: true },
            { key: 'b', label: 'Unit Price Bid', visible: true, sortable: true },
            { key: 'IsOutlier', label: 'Outlier', visible: true, sortable: true },
            { key: 'PvBidTotal', label: 'Bid Amount', visible: true, sortable: true },
            { key: 'd', label: 'District', visible: true, sortable: true },
            { key: 'MarketArea', label: 'Market Area', visible: true, sortable: true },
            { key: 'c', label: 'County', visible: true, sortable: true },
            { key: 'VendorName', label: 'Bidder Name', visible: false, sortable: false },
            { key: 'BidStatus', label: 'Bid Status', visible: true, sortable: true },
            { key: 'VendorRanking', label: 'Bidder Rank', visible: true, sortable: true },
            { key: 'ContractType', label: 'Contract Type', visible: false, sortable: false },
            { key: 'ContractWorkType', label: 'Work Type', visible: false, sortable: false },
            { key: 'WorkMixDescription', label: 'Work Mix', visible: false, sortable: false },
            { key: 'CategoryDescription', label: 'Project Category', visible: false, sortable: false },
            { key: 'l', label: 'Letting Date', visible: true, sortable: true },
            { key: 'ExecutedDate', label: 'Executed Date', visible: false, sortable: false },
            { key: 'Duration', label: 'Awarded Days', visible: false, sortable: false },
            { key: 'ProposalType', label: 'Proposal Type', visible: false, sortable: false },
            { key: 'BidType', label: 'Bid Type', visible: false, sortable: false }
        ];
        
        $scope.visibleColumns = function() {
            return $scope.availableColumns.filter(col => col.visible);
        };
        
        $scope.showColumnSelector = false;
        
        $scope.toggleColumnSelector = function() {
            $scope.showColumnSelector = !$scope.showColumnSelector;
        };
        
        $scope.selectAllColumns = function() {
            $scope.availableColumns.forEach(col => col.visible = true);
        };
        
        $scope.deselectAllColumns = function() {
            $scope.availableColumns.forEach(col => col.visible = false);
        };
        
        $scope.resetToDefaultColumns = function() {
            $scope.availableColumns.forEach(col => {
                col.visible = col.key === 'p' || col.key === 'ProjectNumber' || col.key === 'ri' || 
                             col.key === 'Quantity' || col.key === 'b' || col.key === 'PvBidTotal' || 
                             col.key === 'd' || col.key === 'MarketArea' || col.key === 'c';
            });
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
        $timeout(function() {
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
        $scope.selectedMarketArea = "";
        $scope.selectedMarketCounties = [];
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
            $scope.selectedMinRank = null;
            $scope.selectedMaxRank = null;
            $scope.selectedBidStatus = "FMV";
            $scope.startDate = null;
            $scope.endDate = null;
            $scope.selectedDistrict = "";
            $scope.selectedCounties = [];
            $scope.availableCounties = [];
            $scope.selectedMarketArea = "";
            $scope.selectedMarketCounties = [];
            $scope.items = [];
            $scope.hasError = false;
            $scope.errorMessage = '';
            $scope.selectedContractTypes = ["CC"];
            $scope.selectedWorkTypeCodes = [];
        };
       
        $scope.searchBids = function () {
            if ((!$scope.searchProjectNumber || $scope.searchProjectNumber.trim() === '') &&
                (!$scope.selectedPayItemNumber || $scope.selectedPayItemNumber.trim() === '')) {
                alert("Please enter a valid Proposal Number before searching.");
                return;
            }
            const months = $scope.monthsOfHistory;
            if (!months || months < 1 || months > 120) {
                alert("Please enter a valid Months of Bid History between 1 and 120.");
                return;
            }
            $scope.bidHistoryData = [];
            $scope.chartStats = null;
            $scope.isLoading = true;
            $scope.searchAttempted = true;
            $scope.isLargeDataset = false;
            $scope.largeDatasetMessage = '';
            
            if ($scope.chartInstance) {
                $scope.chartInstance.destroy();
                $scope.chartInstance = null;
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
                    marketCounties: $scope.selectedMarketCounties,
                    minRank: $scope.selectedMinQuantity || null,
                    maxRank: $scope.selectedMaxQuantity || $scope.maxQuantity || Infinity,
                    projectNumber: $scope.searchProjectNumber || null
                },
                traditional: true
            }).success(function (data) {
                // Check response size (1.7MB = 1,785,728 bytes)
                const responseSize = JSON.stringify(data).length;
                const maxSize = 1.7 * 1024 * 1024; // 1.7MB in bytes
                
                $scope.bidHistoryData = data;
                const quantities = data.map(item => item.Quantity || 0);
                const prices = data.map(item => item.b || 0);
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

                // Check if dataset is too large for display
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

        $scope.getSortClass = function (column) {
            if ($scope.sortColumn === column) {
                return $scope.reverseSort ? 'fa-sort-down' : 'fa-sort-up';
            }
            return 'fa-sort';
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
                scope.$apply(() => {
                    scope.isRegionDropdownOpen = false;
                });
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
        $scope.hasError = false;
        $scope.errorMessage = '';
        $scope.districts = Object.keys($scope.districtCountyMap);
        $scope.availableCounties = [];
        $scope.selectedDistrict = "";
        $scope.selectedCounties = [];
        $scope.getCountiesForSelectedDistrict = function () {
            return $scope.districtCountyMap[$scope.selectedDistrict] || [];
        };
        $scope.updateCounties = function () {
            const rawCounties = $scope.districtCountyMap[$scope.selectedDistrict] || [];
            const cleaned = rawCounties.map(c => c.includes(" - ") ? c.split(" - ")[1].trim() : c.trim());

            $scope.availableCounties = cleaned.map(c => ({ name: c, selected: true }));
            $scope.selectedCounties = cleaned;
        };

        $scope.validateQuantity = function () {
            const min = parseFloat($scope.selectedMinQuantity);
            const max = parseFloat($scope.selectedMaxQuantity);

            $scope.hasError = false;
            $scope.errorMessage = '';

            if (!isNaN(min) && !isNaN(max) && min > max) {
                $scope.hasError = true;
                $scope.errorMessage = 'Minimum quantity cannot be greater than maximum quantity.';
            }
        };
        $scope.getQuantityRange = function () {
            const min = parseFloat($scope.selectedMinQuantity) || 0;
            const max = parseFloat($scope.selectedMaxQuantity) || $scope.maxQuantity || Infinity;
            return max - min;
        };
        $scope.toggleCounty = function (county) {
            const index = $scope.selectedCounties.indexOf(county.name);
            if (county.selected && index === -1) {
                $scope.selectedCounties.push(county.name);
            } else if (!county.selected && index > -1) {
                $scope.selectedCounties.splice(index, 1);
            }
        };

        $scope.selectAllCounties = function () {
            $scope.availableCounties.forEach(c => c.selected = true);
            $scope.selectedCounties = $scope.availableCounties.map(c => c.name);
        };

        $scope.clearAllCounties = function () {
            $scope.availableCounties.forEach(c => c.selected = false);
            $scope.selectedCounties = [];
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
            if (debounceTimer) $timeout.cancel(debounceTimer);

            if (!$scope.searchText || $scope.searchText.length < 2) {
                $scope.items = [];
                $scope.selectedPayItemNumber = null;
                return;
            }

            debounceTimer = $timeout(function () {
                $http.get('/UnitPriceSearch/GetPayItemSuggestions', {
                    params: { input: $scope.searchText }
                }).success(function (data) {
                    $scope.items = data;
                }).error(function (err) {
                    console.error("Error fetching pay item suggestions:", err);
                });
            }, 300);
        };
        $scope.clearSearchText = function () {
            $scope.searchText = "";
            $scope.items = [];
            $scope.selectedPayItemNumber = null;
        };
        $scope.selectPayItem = function (item) {
            $scope.searchText = item.Description;
            $scope.selectedPayItemNumber = item.Name;
            $scope.items = [];
        };
        $scope.shouldHideGraphForLumpSum = function () {
            if (!$scope.bidHistoryData || $scope.bidHistoryData.length === 0) return false;

            const totalQty = $scope.bidHistoryData.reduce((sum, item) => sum + (item.Quantity || 0), 0);
            const allLumpSum = $scope.bidHistoryData.every(item => item.CalculatedUnit === 'LS - Lump Sum');

            return totalQty === $scope.bidHistoryData.length && allLumpSum;
        };
        // Search Bids
        // $scope.searchBids = function () {
        //     if ((!$scope.searchProjectNumber || $scope.searchProjectNumber.trim() === '') &&
        //         (!$scope.selectedPayItemNumber || $scope.selectedPayItemNumber.trim() === '')) {
        //         alert("Please enter a valid Proposal Number before searching.");
        //         return;
        //     }
        //     const months = $scope.monthsOfHistory;
        //     if (!months || months < 1 || months > 120) {
        //         alert("Please enter a valid Months of Bid History between 1 and 120.");
        //         return;
        //     }
        //     $scope.bidHistoryData = [];
        //     $scope.chartStats = null;
        //     $scope.isLoading = true;
        //     $scope.searchAttempted = true;
        //     if ($scope.chartInstance) {
        //         $scope.chartInstance.destroy();
        //         $scope.chartInstance = null;
        //     }
        //     $http.get('/UnitPriceSearch/GetPayItemDetails', {
        //         params: {
        //             number: $scope.selectedPayItemNumber,
        //             months: $scope.monthsOfHistory || 12,
        //             contractWorkType: Array.isArray($scope.selectedWorkTypeCodes) && $scope.selectedWorkTypeCodes.length
        //                 ? $scope.selectedWorkTypeCodes
        //                 : null,
        //             startDate: $scope.startDate || null,
        //             endDate: $scope.endDate || null,
        //             counties: $scope.selectedRegionCounties,
        //             bidStatus: $scope.selectedBidStatus || null,
        //             contractType: Array.isArray($scope.selectedContractTypes) && $scope.selectedContractTypes.length
        //                 ? $scope.selectedContractTypes
        //                 : null,
        //             marketCounties: $scope.selectedMarketCounties,
        //             minRank: $scope.selectedMinQuantity || null,
        //             maxRank: $scope.selectedMaxQuantity || $scope.maxQuantity || Infinity,
        //            
        //             projectNumber: $scope.searchProjectNumber || null
        //         },
        //         traditional: true
        //     }).success(function (data) {
        //         $scope.bidHistoryData = data;
        //         const quantities = data.map(item => item.Quantity || 0);
        //         const prices = data.map(item => item.b || 0);
        //         const totalQty = quantities.reduce((sum, q) => sum + q, 0);
        //         const weightedAvg = quantities.reduce((sum, q, i) => sum + (q * prices[i]), 0) / totalQty;

        //         const weightedStdDev = Math.sqrt(
        //             quantities.reduce((sum, q, i) => sum + q * Math.pow(prices[i] - weightedAvg, 2), 0) / totalQty
        //         );

        //         let cleanQty = [], cleanPrices = [];

        //         data.forEach((item, i) => {
        //             const price = prices[i];
        //             const qty = quantities[i];
        //             const isOutlier = Math.abs(price - weightedAvg) > weightedStdDev;

        //             item.IsOutlier = isOutlier;
        //             item.WeightedAvg = weightedAvg;

        //             if (!isOutlier) {
        //                 cleanQty.push(qty);
        //                 cleanPrices.push(price);
        //             }
        //         });

        //         const cleanTotalQty = cleanQty.reduce((sum, q) => sum + q, 0);
        //         const weightedAvgNoOutliers = cleanQty.reduce((sum, q, i) => sum + (q * cleanPrices[i]), 0) / cleanTotalQty;
        //         $scope.weightedAvgNoOutliers = weightedAvgNoOutliers;
        //         data.forEach(item => {
        //             item.WeightedAvgNoOutliers = weightedAvgNoOutliers;
        //         });

        //         $scope.bidHistoryData.forEach(function (bidItem) {
        //             var itemCounty = bidItem.c;
        //             var normalizedItemCounty = itemCounty ? itemCounty.trim().toUpperCase() : '';
        //             var foundMarketArea = '';

        //             if (normalizedItemCounty) {
        //                 var marketAreaKeys = Object.keys($scope.marketAreaToCountiesMap);

        //                 for (var keyIndex = 0; keyIndex < marketAreaKeys.length; keyIndex++) {
        //                     var currentMarketArea = marketAreaKeys[keyIndex];
        //                     var countyList = $scope.marketAreaToCountiesMap[currentMarketArea];

        //                     for (var countyIndex = 0; countyIndex < countyList.length; countyIndex++) {
        //                         var currentCounty = countyList[countyIndex].trim().toUpperCase();
        //                         if (currentCounty === normalizedItemCounty) {
        //                             foundMarketArea = currentMarketArea;
        //                             break;
        //                         }
        //                     }

        //                     if (foundMarketArea) {
        //                         break;
        //                     }
        //                 }
        //             }

        //             bidItem.MarketArea = foundMarketArea || "Unknown";
        //         });

        //     }).error(function (err) {
        //         console.error("Error fetching bid data:", err);
        //     }).finally(function () {
        //         $scope.isLoading = false;
        //     });
        //     $scope.isChartStale = false;
        // };
        function formatDotNetDate(msDateString) {
            if (!msDateString) return '';
            const match = /\/Date\((\d+)\)\//.exec(msDateString);
            if (!match) return '';
            const date = new Date(parseInt(match[1]));
            return date.toLocaleDateString('en-US');
        }

        //CSV Export
        $scope.exportClick = function () {
            let headers = [
                "Contract Number", "Project Number", "Pay Item", "Description", "Supplemental Description",
                "Units", "Quantity", "Unit Price Bid", "Weighted Avg", "Weighted Avg No Outliers", "Outlier", "Bid Amount", "District", "Market Area",
                "Primary County", "Bidder Name", "Bid Status", "Bidder Rank"
                , "Contract Type", "Work Type", "Work Mix", "Project Category",
                "Letting Date", "Executed Date", "Awarded Days", "Proposal Type", "Bid Type"
            ].join(",") + "\n";

            let rows = $scope.bidHistoryData.map(item => [
                `"${item.p}"`, `"${item.ProjectNumber}"`, `"${item.ri}"`, `"${item.Description.replace(/"/g, '""')}"`,
                `"${item.SupplementalDescription}"`,
                `"${item.CalculatedUnit}"`, `"${item.Quantity}"`, `"${item.b}"`, `"${item.WeightedAvg}"`, `"${item.WeightedAvgNoOutliers}"`, `"${item.IsOutlier ? 'Yes' : 'No'}"`,
                `"${item.PvBidTotal}"`, `"${item.d}"`, `"${item.MarketArea}"`, `"${item.c}"`, `"${item.VendorName}"`,
                `"${$scope.getBidTypeLabel(item.BidStatus)}"`, `"${item.VendorRanking}"`, `"${$scope.contractTypeMap[item.ContractType] || item.ContractType}"`,
                `"${$scope.workTypeMap[item.ContractWorkType] || item.ContractWorkType}"`, `"${(item.WorkMixDescription)}"`, `"${(item.CategoryDescription)}"`,
                `"${formatDotNetDate(item.l)}"`, `"${formatDotNetDate(item.ExecutedDate)}"`,
                `"${item.Duration}"`, `"${$scope.proposalTypeMap[item.ProposalType] || item.ProposalType}"`, `"${$scope.getBidTypeLabel(item.BidType)}"`
            ].join(",")).join("\n");

            let csvContent = "data:text/csv;charset=utf-8," + headers + rows;
            let encodedUri = encodeURI(csvContent);
            let link = document.createElement("a");
            link.setAttribute("href", encodedUri);
            link.setAttribute("download", "bid_history.csv");
            document.body.appendChild(link);
            link.click();
        };
        $scope.downloadPDF = function() {
            $scope.isGeneratingPDF = true;
            setTimeout(function() {
                var jsPDF = window.jspdf && window.jspdf.jsPDF;
                if (typeof jsPDF !== 'function') {
                    alert('PDF generation libraries are still loading. Please try again shortly. jsPDF is not loaded!');
                    $scope.isGeneratingPDF = false;
                    if(!$scope.$$phase) $scope.$apply();
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
                doc.text('Months of History: ' + ($scope.monthsOfHistory || '12'), 40, y);
                y += 15;
                doc.text('Date Range: ' + ($scope.startDate ? $scope.startDate.toLocaleDateString() : 'All') + ' to ' + ($scope.endDate ? $scope.endDate.toLocaleDateString() : 'All'), 40, y);
                y += 15;
                doc.text('Selected Counties: ' + ($scope.selectedRegionCounties && $scope.selectedRegionCounties.length > 0 ? $scope.selectedRegionCounties.join(', ') : 'All'), 40, y);
                y += 25;
                
                // Summary statistics
                if ($scope.chartStats) {
                    doc.setFontSize(12);
                    doc.setFont('helvetica', 'bold');
                    doc.text('Summary Statistics:', 40, y);
                    y += 20;
                    doc.setFontSize(10);
                    doc.setFont('helvetica', 'normal');
                    
                    // Create a more detailed statistics section
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
                    doc.text('Weighted Average Unit Price: $' + ($scope.chartStats.avg || 0).toFixed(2), 40, y);
                    y += 15;
                    doc.text('Weighted Average (No Outliers): $' + ($scope.chartStats.weightedAvgNoOutliers || 0).toFixed(2), 40, y);
                    y += 25;
                }
                
                // Add chart image if available
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
                
                // Data summary section
                if ($scope.bidHistoryData && $scope.bidHistoryData.length > 0) {
                    doc.setFontSize(12);
                    doc.setFont('helvetica', 'bold');
                    doc.text('Data Summary:', 40, y);
                    y += 20;
                    doc.setFontSize(10);
                    doc.setFont('helvetica', 'normal');
                    doc.text('Total Records: ' + $scope.bidHistoryData.length, 40, y);
                    y += 15;
                    
                    // Show sample of data (first 5 records)
                    const sampleSize = Math.min(5, $scope.bidHistoryData.length);
                    doc.text('Sample Records (showing first ' + sampleSize + '):', 40, y);
                    y += 15;
                    
                    for (let i = 0; i < sampleSize; i++) {
                        const item = $scope.bidHistoryData[i];
                        const line = `${i + 1}. Contract: ${item.p}, Pay Item: ${item.ri}, Qty: ${item.Quantity}, Price: $${item.b}`;
                        doc.text(line, 50, y);
                        y += 12;
                    }
                    
                    if ($scope.bidHistoryData.length > sampleSize) {
                        doc.text(`... and ${$scope.bidHistoryData.length - sampleSize} more records`, 50, y);
                        y += 15;
                    }
                }
                
                // Footer
                const pageHeight = doc.internal.pageSize.height;
                doc.setFontSize(9);
                doc.setTextColor(100);
                doc.text('For complete data, please use the CSV export option.', 40, pageHeight - 40);
                doc.text('Report generated by DQE System', 40, pageHeight - 25);
                
                // Save the PDF
                doc.save('UnitPriceSearch_Report.pdf');
                
                setTimeout(function() {
                    $scope.isGeneratingPDF = false;
                    if(!$scope.$$phase) $scope.$apply();
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
            'selectedMarketArea',
            'selectedMarketCounties',
            'selectedDistrict',
            'selectedCounties',
            'selectedMinQuantity',
            'selectedMaxQuantity'
        ];
        angular.forEach(filterWatchList, function (filter) {
            $scope.$watch(filter, function (newVal, oldVal) {
                if (newVal !== oldVal && !$scope.isLoading && $scope.bidHistoryData && $scope.bidHistoryData.length > 0 && $scope.searchAttempted && $scope.chartStats && $scope.chartInstance) {
                    $scope.isChartStale = true;
                }
            }, true);
        });
        $scope.$watch('bidHistoryData', function (newVal) {
            if (newVal && newVal.length > 0) {
                waitForCanvasAndRender();
            }
        });

        function computeWeightedStats(prices, quantities) {
            var totalQty = d3.sum(quantities);
            var weightedMean = d3.sum(prices.map(function (p, i) {
                return p * quantities[i];
            })) / totalQty;
            var weightedStd = Math.sqrt(d3.sum(prices.map(function (p, i) {
                return quantities[i] * Math.pow(p - weightedMean, 2);
            })) / totalQty);
            return { weightedMean: weightedMean, weightedStd: weightedStd };
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
            // y-values by x
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

        // linear interpolation with extrapolation
        function interpolateLinear(xArr, yArr, targetX) {
            if (xArr.length === 0 || yArr.length === 0) return NaN;
            if (xArr.length !== yArr.length) return NaN;
            if (xArr.length === 1) return yArr[0];
            // exact matches
            const exactIndex = xArr.indexOf(targetX);
            if (exactIndex !== -1) return yArr[exactIndex];
            // interpolation points
            let leftIndex = -1;
            let rightIndex = -1;
            for (let i = 0; i < xArr.length - 1; i++) {
                if (targetX >= xArr[i] && targetX <= xArr[i + 1]) {
                    leftIndex = i;
                    rightIndex = i + 1;
                    break;
                }
            }
            // Extrapolation cases
            if (leftIndex === -1) {
                if (targetX < xArr[0]) {
                    
                    leftIndex = 0;
                    rightIndex = 1;
                } else if (targetX > xArr[xArr.length - 1]) {
                  
                    leftIndex = xArr.length - 2;
                    rightIndex = xArr.length - 1;
                } else {
                    return NaN;
                }
            }
            // Linear interpolation/extrapolation
            const x1 = xArr[leftIndex];
            const x2 = xArr[rightIndex];
            const y1 = yArr[leftIndex];
            const y2 = yArr[rightIndex];
            if (x2 === x1) return y1;
            const slope = (y2 - y1) / (x2 - x1);
            const result = y1 + slope * (targetX - x1);

            return result;
        }
        function loessSmooth(x, y, bandwidth, xvals) {
            if (!Array.isArray(x) || !Array.isArray(y) || !Array.isArray(xvals) ||
                x.length === 0 || y.length === 0 || x.length !== y.length) {
               
                return xvals.map(() => NaN);
            }
            try {
                // Step 1: Handle multiple y-values per x by aggregating
                const aggregatedData = aggregateDataByX(x, y);
                const uniqueX = aggregatedData.map(d => d.x);
                const uniqueY = aggregatedData.map(d => d.y);
                // Step 2: Check if we have enough data points
                if (uniqueX.length < 3) {
                    console.warn("Not enough unique x-values for LOESS (need at least 3)");
                    return xvals.map(xval => interpolateLinear(uniqueX, uniqueY, xval));
                }
                // Step 3: Sort data by x-values (LOESS requires sorted data)
                const sortedIndices = uniqueX.map((_, i) => i)
                    .sort((a, b) => uniqueX[a] - uniqueX[b]);
                const sortedX = sortedIndices.map(i => uniqueX[i]);
                const sortedY = sortedIndices.map(i => uniqueY[i]);

                // Step 4: Adjust bandwidth for small datasets
                const adjustedBandwidth = Math.max(bandwidth, 2 / sortedX.length);

                // Step 5: Try Science.js LOESS
                if (typeof science !== "undefined" && typeof science.stats.loess === "function") {
                    const loess = science.stats.loess().bandwidth(adjustedBandwidth);

                    // IMPORTANT: Science.js returns an ARRAY of smoothed values, not a function!
                    const smoothedValues = loess(sortedX, sortedY);
                    // Step 6: Interpolate smoothed values for requested x-values
                    return xvals.map(xval => {
                        return interpolateLinear(sortedX, smoothedValues, xval);
                    });
                } else {
                    throw new Error("Science.js not available");
                }

            } catch (error) {
                console.error("Science.js LOESS error:", error);

                // Fallback to manual smoothing
                return manualLoessSmooth(x, y, bandwidth, xvals);
            }
        }
        // adding the manualLoessSmooth function if you don't have it
        function manualLoessSmooth(x, y, bandwidth, xvals) {

            // Aggregate data first
            const aggregatedData = aggregateDataByX(x, y);
            const uniqueX = aggregatedData.map(d => d.x);
            const uniqueY = aggregatedData.map(d => d.y);

            if (uniqueX.length < 3) {
                
                return xvals.map(xval => interpolateLinear(uniqueX, uniqueY, xval));
            }
            const windowSize = Math.max(2, Math.floor(bandwidth * uniqueX.length));

            return xvals.map(xval => {
                // nearest points
                const distances = uniqueX.map((xi, i) => ({ dist: Math.abs(xi - xval), index: i }));
                distances.sort((a, b) => a.dist - b.dist);

                const nearestPoints = distances.slice(0, Math.min(windowSize, distances.length));

                // Weighted average based on inverse distance
                let weightedSum = 0;
                let totalWeight = 0;

                for (const point of nearestPoints) {
                    const weight = point.dist === 0 ? 1 : 1 / (1 + point.dist);
                    weightedSum += uniqueY[point.index] * weight;
                    totalWeight += weight;
                }

                return totalWeight > 0 ? weightedSum / totalWeight : NaN;
            });
        }
        //Bootstrap CI function
        function bootstrapCI(x, y, xvals, frac, nBoot = 200) {
            if (!x.length || !y.length || x.length !== y.length) {
                return {
                    lower: xvals.map(() => null),
                    upper: xvals.map(() => null),
                };
            }
            let preds = [];
            for (let b = 0; b < nBoot; b++) {
                let indices = [];
                for (let i = 0; i < x.length; i++) {
                    indices.push(Math.floor(Math.random() * x.length));
                }
                let xBoot = indices.map(i => x[i]);
                let yBoot = indices.map(i => y[i]);
                let smoothed = loessSmooth(xBoot, yBoot, frac, xvals);
                preds.push(smoothed);
            }
            let lower = [];
            let upper = [];
            for (let i = 0; i < xvals.length; i++) {
                let valuesAtPoint = preds.map(row => row[i]).filter(v => v !== null && !isNaN(v));
                valuesAtPoint.sort((a, b) => a - b);
                if (valuesAtPoint.length > 0) {
                    let lowerIdx = Math.floor(valuesAtPoint.length * 0.025);
                    let upperIdx = Math.floor(valuesAtPoint.length * 0.975);
                    lower[i] = valuesAtPoint[lowerIdx];
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
                requestAnimationFrame(function () {
                    const canvas = document.getElementById("priceChart");
                    if (!canvas) {
                        $scope.isChartLoading = false;
                        return;
                    }

                    /*const ctx = canvas.getContext("2d");*/

                    if (!$scope.bidHistoryData || $scope.bidHistoryData.length === 0) {
                        $scope.isChartLoading = false;
                        return;
                    }

                    const quantities = $scope.bidHistoryData.map(item => item.Quantity || 0);
                    const prices = $scope.bidHistoryData.map(item => item.b || 0);

                    const { weightedMean, weightedStd } = computeWeightedStats(prices, quantities);
                    const filtered = filterOutliers(prices, quantities, weightedMean, weightedStd);
                    const QuantityFiltered = filtered.map(d => d.q);
                    const PriceFiltered = filtered.map(d => d.p);

                    // Common range
                    const quantityRange = Array.from(new Set(quantities)).sort((a, b) => a - b);
                    // LOESS fits
                    const loessUnfiltered = loessSmooth(quantities, prices, 0.3, quantityRange);
                    const loessFiltered = loessSmooth(QuantityFiltered, PriceFiltered, 0.9, quantityRange);

                    // Bootstrap CI
                    const ciUnfiltered = bootstrapCI(quantities, prices, quantityRange, 0.3, 200);
                    const ciFiltered = bootstrapCI(QuantityFiltered, PriceFiltered, quantityRange, 0.9, 200);
                    // Split into lower and upper arrays for each CI
                    const lowerUnfiltered = quantityRange.map((q, i) => ({ x: q, y: ciUnfiltered.lower[i] })).filter(pt => pt.x > 0 && pt.y > 0 && isFinite(pt.x) && isFinite(pt.y));
                    const upperUnfiltered = quantityRange.map((q, i) => ({ x: q, y: ciUnfiltered.upper[i] })).filter(pt => pt.x > 0 && pt.y > 0 && isFinite(pt.x) && isFinite(pt.y));
                    const lowerFiltered = quantityRange.map((q, i) => ({ x: q, y: ciFiltered.lower[i] })).filter(pt => pt.x > 0 && pt.y > 0 && isFinite(pt.x) && isFinite(pt.y));
                    const upperFiltered = quantityRange.map((q, i) => ({ x: q, y: ciFiltered.upper[i] })).filter(pt => pt.x > 0 && pt.y > 0 && isFinite(pt.x) && isFinite(pt.y));
                    // logging
                    /*console.log('lowerUnfiltered', lowerUnfiltered);
                    console.log('upperUnfiltered', upperUnfiltered);
                    console.log('lowerFiltered', lowerFiltered);
                    console.log('upperFiltered', upperFiltered);*/
                    const filteredPoints = filtered.map(d => ({
                        x: d.q,
                        y: d.p,
                        l: d.l,  
                        p: d.pn
                    }));
                    const loessLineUnfiltered = quantityRange.map((q, i) => ({ x: q, y: loessUnfiltered[i] }));
                    const loessLineFiltered = quantityRange.map((q, i) => ({ x: q, y: loessFiltered[i] }));
                    
                    if (quantities.length === 0 || prices.length === 0) {
                        $scope.isChartLoading = false;
                        return;
                    }
                    console.log(ciUnfiltered.lower, ciUnfiltered.upper);
                    const outlierPoints = [];
                    const normalPoints = [];
                    const bidPoints = [];

                    for (let i = 0; i < $scope.bidHistoryData.length; i++) {
                        const item = $scope.bidHistoryData[i];
                        const quantity = item.Quantity || 0;
                        const price = item.b || 0;
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
                    // Destroy existing chart
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

                    $scope.chartInstance = new Chart(newCtx, {
                        type: 'scatter',
                        data: {
                            datasets: [

                                {
                                    label: 'LOESS',
                                    data: loessLineUnfiltered,
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
                                    data: loessLineFiltered,
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
                                        { x: d3.min(quantities), y: weightedMean },
                                        { x: d3.max(quantities), y: weightedMean }
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
                                        { x: d3.min(QuantityFiltered), y: $scope.weightedAvgNoOutliers },
                                        { x: d3.max(QuantityFiltered), y: $scope.weightedAvgNoOutliers }
                                    ],
                                    type: 'line',
                                    borderColor: '#6366f1',
                                    borderDash: [8, 8],
                                    fill: false,
                                    borderWidth: 1
                                },
                                // Unfiltered CI band (red)
                                {
                                    label: '95% CI Outliers (Lower)',
                                    data: lowerUnfiltered,
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
                                    data: upperUnfiltered,
                                    type: 'line',
                                    borderColor: 'rgba(255,0,0,0.5)',
                                    backgroundColor: 'rgba(255,0,0,0.1)',
                                    fill: false,
                                    pointRadius: 0,
                                    borderWidth: 2,
                                    tension: 0.4,
                                    order: 0
                                },
                                // Filtered CI band (blue)
                                {
                                    label: '95% CI No Outliers (Lower)',
                                    data: lowerFiltered,
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
                                    data: upperFiltered,
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
                                        callback: function (value) {
                                            // Format currency nicely for log scale
                                            if (value >= 1000) {
                                                return '$' + (value / 1000).toFixed(1) + 'K';
                                            } else if (value >= 1) {
                                                return '$' + value.toFixed(0);
                                            } else {
                                                return '$' + value.toFixed(2);
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
                                            
                                            // Group similar datasets under common headers
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
                                            lines.push(`💰 Price: ${"$" + price.toFixed(2).replace(/\B(?=(\d{3})+(?!\d))/g, ',')}`);

                                          
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
                        outlierCount: $scope.bidHistoryData.filter(item => item.IsOutlier).length
                    };

                    $scope.isChartLoading = false;
                    $scope.isChartStale = false;
                    $scope.$apply();
                });
            }, 0);
        }

        /*function processBidData() {
            const quantities = Array.isArray($scope.bidHistoryData) ? $scope.bidHistoryData.map(item => item.Quantity || 0) : [];
            const prices = Array.isArray($scope.bidHistoryData) ? $scope.bidHistoryData.map(item => item.b || 0) : [];
            const totalQty = quantities.reduce((sum, q) => sum + q, 0);
            const weightedAvg = quantities.reduce((sum, q, i) => sum + (q * prices[i]), 0) / (totalQty || 1);

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
        }*/

        $scope.getBidTypeLabel = function (code) {
            return code ? ($scope.bidTypeMap[code] || "Unknown") : "Unknown";
        };

        $scope.getBidStatusLabel = function (code) {
            return code ? ($scope.bidStatusMap[code] || "Unknown") : "Unknown";
        };

        // Remove pagination variables
        // $scope.currentPage = 1;
        // $scope.pageSize = 10;
        // $scope.isTruncated = false;
        // $scope.totalCount = 0;

        // $scope.getPagedData = function(page) {
        //     var p = page || $scope.currentPage;
        //     var start = (p - 1) * $scope.pageSize;
        //     return $scope.bidHistoryData.slice(start, start + $scope.pageSize);
        // };
        // console.log($scope.getPagedData());
    }
]);

angular.module('dqeControllers')
    .filter('msDateToJS', function () {
        return function (input) {
            if (!input) return '';
            var match = /\/Date\((\d+)\)\//.exec(input);
            return match ? new Date(parseInt(match[1])) : input;
        };
    });