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
        $scope.selectedContractTypes = [];
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
        $scope.allWorkMixes = [];
        $scope.workMixSelected = [];
        $scope.workMixSearch = '';
        $scope.filteredWorkMixes = [];
        $scope.workMixSelected = [];
        $scope.isWorkMixDropdownOpen = false;
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
            $scope.workMixSelected = [];
            $scope.workMixSearch = '';
            $scope.filterWorkMixes();
            $scope.isWorkMixDropdownOpen = false;
            $scope.selectedContractTypes = [];
            $scope.selectedWorkTypeCodes = [];
        };
        $scope.loadWorkMixes = function () {
            $http.get('/UnitPriceSearch/GetWorkMixes').then(function (response) {
                $scope.allWorkMixes = Array.from(new Set(response.data)).sort();
                $scope.filteredWorkMixes = angular.copy($scope.allWorkMixes);
            }, function (error) {
                console.error("Failed to load work mixes:", error);
            });
        };
        $scope.searchBids = function () {
            if (!$scope.searchProjectNumber || $scope.searchProjectNumber.trim() === '') {
                alert("Please enter a valid Proposal Number before searching.");
                return;
            }
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
        $scope.loadWorkMixes();
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
        $scope.filterWorkMixes = function () {
            const query = ($scope.workMixSearch || '').toLowerCase();
            $scope.filteredWorkMixes = $scope.allWorkMixes.filter(item =>
                item.toLowerCase().includes(query)
            );
            $scope.isWorkMixDropdownOpen = true;
        };

        $scope.toggleWorkMixSelection = function (item) {
            const index = $scope.workMixSelected.indexOf(item);
            if (index === -1) {
                $scope.workMixSelected.push(item);
            } else {
                $scope.workMixSelected.splice(index, 1);
            }
            $scope.isWorkMixDropdownOpen = false;
        };

        $scope.removeWorkMix = function (item) {
            const index = $scope.workMixSelected.indexOf(item);
            if (index !== -1) {
                $scope.workMixSelected.splice(index, 1);
            }
        };

        $scope.clearWorkMixSelection = function () {
            $scope.workMixSelected = [];
            $scope.workMixSearch = '';
            $scope.filterWorkMixes();
            $scope.isWorkMixDropdownOpen = false;
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
                    workTypeNames: $scope.workMixSelected.map(x => typeof x === 'string' ? x : x.label),
                    projectNumber: $scope.searchProjectNumber || null
                },
                traditional: true
            }).success(function (data) {
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

                $scope.bidHistoryData = data;
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

            }).error(function (err) {
                console.error("Error fetching bid data:", err);
            }).finally(function () {
                $scope.isLoading = false;
            });
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
            if (newVal.includes("ALL")) {
                $scope.selectedContractTypes = angular.copy($scope.contractTypes);
            }
        }, true);

        $scope.$watch('selectedWorkTypeCodes', function (newVal) {
            if (newVal.includes("ALL")) {
                $scope.selectedWorkTypeCodes = angular.copy($scope.workTypeCodes);
            }
        }, true);
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
            return prices
                .map(function (p, i) {
                    return { p: p, q: quantities[i] };
                })
                .filter(function (d) {
                    return Math.abs(d.p - weightedMean) <= weightedStd;
                });
        }

        /*function loessSmooth(x, y, bandwidth, xvals) {
            if (!Array.isArray(x) || !Array.isArray(y) || !Array.isArray(xvals) ||
                x.length === 0 || y.length === 0 || x.length !== y.length) {
                console.warn("Invalid input to loessSmooth");
                return xvals.map(() => NaN);
            }

            try {
                const data = x.map((xi, i) => [xi, y[i]]);

                if (typeof science === "undefined" || typeof science.stats.loess !== "function") {
                    throw new Error("Science.js loess function not available");
                }

                const loess = science.stats.loess().bandwidth(bandwidth);
                const smoothingFunction = loess(x,y); // returns function(x) => y

                return xvals.map(xval => {
                    try {
                        return smoothingFunction(xval);
                    } catch {
                        return interpolateValue(data, xval);
                    }
                });

            } catch (error) {
                console.error("Science.js LOESS error:", error);

                if (x && y && xvals && x.length === y.length && x.length > 0) {
                    return manualLoessSmooth(x, y, bandwidth, xvals);
                } else {
                    return xvals.map(() => NaN);
                }
            }
        }*/

        function loessSmooth(x, y, bandwidth, xvals) {
            if (!Array.isArray(x) || !Array.isArray(y) || !Array.isArray(xvals) ||
                x.length === 0 || y.length === 0 || x.length !== y.length) {
                console.warn("Invalid input to loessSmooth");
                return xvals.map(() => NaN);
            }

            try {
                // Step 1: Handle multiple y-values per x by aggregating
                const aggregatedData = aggregateDataByX(x, y);
                const uniqueX = aggregatedData.map(d => d.x);
                const uniqueY = aggregatedData.map(d => d.y);

                console.log("Aggregated data:", aggregatedData);
                console.log("Unique X:", uniqueX);
                console.log("Unique Y:", uniqueY);

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

                    console.log("LOESS smoothed values:", smoothedValues);

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


        function interpolateValue(data, xval) {
            // Sort data by x values
            var sortedData = data.slice().sort((a, b) => a[0] - b[0]);

            // Find surrounding points
            for (var i = 0; i < sortedData.length - 1; i++) {
                if (xval >= sortedData[i][0] && xval <= sortedData[i + 1][0]) {
                    var x1 = sortedData[i][0], y1 = sortedData[i][1];
                    var x2 = sortedData[i + 1][0], y2 = sortedData[i + 1][1];

                    // Linear interpolation
                    return y1 + (y2 - y1) * (xval - x1) / (x2 - x1);
                }
            }

            // If outside range, return nearest value
            if (xval < sortedData[0][0]) return sortedData[0][1];
            if (xval > sortedData[sortedData.length - 1][0]) return sortedData[sortedData.length - 1][1];

            return NaN;
        }

        // Manual LOESS implementation as fallback
        function manualLoessSmooth(x, y, bandwidth, xvals) {
            const n = x.length;
            const result = [];

            for (let i = 0; i < xvals.length; i++) {
                const xi = xvals[i];
                const distances = x.map((xj, idx) => ({ dist: Math.abs(xi - xj), idx }))
                    .sort((a, b) => a.dist - b.dist);

                const windowSize = Math.max(1, Math.floor(bandwidth * n));
                const neighbors = distances.slice(0, windowSize);

                if (neighbors.length === 0) {
                    result.push(NaN);
                    continue;
                }

                const maxDist = neighbors[neighbors.length - 1].dist;
                const weights = neighbors.map(n => {
                    const u = n.dist / maxDist;
                    return u >= 1 ? 0 : Math.pow(1 - Math.pow(u, 3), 3);
                });

                let sumW = 0, sumWX = 0, sumWY = 0, sumWXX = 0, sumWXY = 0;

                for (let j = 0; j < neighbors.length; j++) {
                    const idx = neighbors[j].idx;
                    const w = weights[j];
                    const xj = x[idx], yj = y[idx];
                    sumW += w;
                    sumWX += w * xj;
                    sumWY += w * yj;
                    sumWXX += w * xj * xj;
                    sumWXY += w * xj * yj;
                }

                if (sumW > 0) {
                    const meanX = sumWX / sumW;
                    const meanY = sumWY / sumW;
                    const denom = sumWXX - sumWX * meanX;

                    if (Math.abs(denom) > 1e-10) {
                        const slope = (sumWXY - sumWX * meanY) / denom;
                        const intercept = meanY - slope * meanX;
                        result.push(slope * xi + intercept);
                    } else {
                        result.push(meanY);
                    }
                } else {
                    result.push(NaN);
                }
            }

            return result;
        }


        // Corrected Bootstrap CI function
        function bootstrapCI(x, y, xvals, frac, nBoot) {
            if (frac === undefined) frac = 0.3;
            if (nBoot === undefined) nBoot = 200;

            if (!x.length || !y.length || x.length !== y.length) {
                return {
                    lower: xvals.map(() => null),
                    upper: xvals.map(() => null),
                };
            }

            var preds = [];

            for (var b = 0; b < nBoot; b++) {
                // Fixed lodash reference
                var indices = _.sampleSize(_.range(x.length), x.length);
                var xBoot = indices.map(function (i) { return x[i]; });
                var yBoot = indices.map(function (i) { return y[i]; });

                // Get smoothed values
                var smoothed = loessSmooth(xBoot, yBoot, frac, xvals);

                // Handle different return formats
                var smoothedY;
                if (Array.isArray(smoothed)) {
                    // If smoothed is already an array of numbers
                    smoothedY = smoothed.map(function (val) {
                        return (val !== null && !isNaN(val)) ? val : null;
                    });
                } else {
                    // If smoothed returns objects with .y property
                    smoothedY = smoothed.map(function (pt) {
                        return (pt && pt.y !== undefined) ? pt.y : null;
                    });
                }

                preds.push(smoothedY);
            }

            var lower = [];
            var upper = [];

            for (var i = 0; i < xvals.length; i++) {
                var valuesAtPoint = preds.map(function (row) {
                    return row[i];
                }).filter(function (v) {
                    return v !== null && !isNaN(v);
                });

                if (valuesAtPoint.length > 0) {
                    // Sort values for quantile calculation
                    valuesAtPoint.sort(function (a, b) { return a - b; });

                    // Use d3.quantile if available, otherwise manual calculation
                    if (typeof d3 !== 'undefined' && d3.quantile) {
                        lower[i] = d3.quantile(valuesAtPoint, 0.025);
                        upper[i] = d3.quantile(valuesAtPoint, 0.975);
                    } else {
                        // Manual quantile calculation
                        var lowerIdx = Math.floor(valuesAtPoint.length * 0.025);
                        var upperIdx = Math.floor(valuesAtPoint.length * 0.975);
                        lower[i] = valuesAtPoint[lowerIdx];
                        upper[i] = valuesAtPoint[upperIdx];
                    }
                } else {
                    lower[i] = null;
                    upper[i] = null;
                }
            }

            return { lower: lower, upper: upper };
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

                    const ctx = canvas.getContext("2d");

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
                    const ciUnfiltered = bootstrapCI(quantities, prices, quantityRange, 0.3);
                    const ciFiltered = bootstrapCI(QuantityFiltered, PriceFiltered, quantityRange, 0.9);

                    // Convert data to Chart.js format
                    const originalPoints = quantities.map((q, i) => ({ x: q, y: prices[i] }));
                    const filteredPoints = QuantityFiltered.map((q, i) => ({ x: q, y: PriceFiltered[i] }));
                    const loessLineUnfiltered = quantityRange.map((q, i) => ({ x: q, y: loessUnfiltered[i] }));
                    const loessLineFiltered = quantityRange.map((q, i) => ({ x: q, y: loessFiltered[i] }));
                    const ciBandUnfiltered = [...quantityRange.map((q, i) => ({ x: q, y: ciUnfiltered.lower[i] })),
                    ...quantityRange.slice().reverse().map((q, i) => ({ x: quantityRange[quantityRange.length - 1 - i], y: ciUnfiltered.upper[i] }))];
                    const ciBandFiltered = [...quantityRange.map((q, i) => ({ x: q, y: ciFiltered.lower[i] })),
                    ...quantityRange.slice().reverse().map((q, i) => ({ x: quantityRange[quantityRange.length - 1 - i], y: ciFiltered.upper[i] }))];
                    console.log("Ci Band Filtered value", ciBandFiltered);
                    if (quantities.length === 0 || prices.length === 0) {
                        $scope.isChartLoading = false;
                        return;
                    }

                    const outlierPoints = [];
                    const normalPoints = [];
                    const bidPoints = [];

                    for (let i = 0; i < quantities.length; i++) {
                        bidPoints.push({ x: quantities[i], y: prices[i] });
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
                        const formattedPoint = { x: point.x, y: point.y };

                        if (isOutlier) {
                            outlierPoints.push(formattedPoint);
                        } else {
                            normalPoints.push(formattedPoint);
                        }
                    });
                    console.log({
                        originalPoints,
                        filteredPoints,
                        loessLineFiltered,
                        loessLineUnfiltered,
                        ciBandFiltered,
                        ciBandUnfiltered,
                    });
                    const numPoints = quantities.length;
                    const meanX = quantities.reduce((a, b) => a + b, 0) / numPoints;
                    const meanY = prices.reduce((a, b) => a + b, 0) / numPoints;

                    const numerator = quantities.map((x, i) => (x - meanX) * (prices[i] - meanY)).reduce((a, b) => a + b, 0);
                    const denominator = quantities.map(x => Math.pow(x - meanX, 2)).reduce((a, b) => a + b, 0);

                    const slope = denominator !== 0 ? numerator / denominator : 0;
                    const intercept = meanY - slope * meanX;

                    const uniqueQuantities = [...new Set(quantities)].sort((a, b) => a - b);
                    const regressionLine = uniqueQuantities.map(q => ({ x: q, y: slope * q + intercept }));

                    const minQty = Math.min(...quantities);
                    const maxQty = Math.max(...quantities);
                    const weightedAvgLine = [
                        { x: minQty, y: weightedAvg },
                        { x: maxQty, y: weightedAvg }
                    ];

                    // Destroy existing chart
                    if ($scope.chartInstance) {
                        $scope.chartInstance.destroy();
                        $scope.chartInstance = null;
                    }

                    $scope.chartInstance = new Chart(ctx, {
                        type: 'scatter',
                        data: {
                            datasets: [
                                {
                                    label: 'Outliers',
                                    data: originalPoints,
                                    backgroundColor: 'rgba(128, 128, 128, 0.3)',
                                    pointRadius: 5,
                                },
                                {
                                    label: 'No Outliers',
                                    data: filteredPoints,
                                    backgroundColor: 'rgba(0, 128, 0, 0.5)',
                                    pointRadius: 5,
                                },
                                {
                                    label: 'LOESS (w/ Outliers)',
                                    data: loessLineUnfiltered,
                                    type: 'line',
                                    borderColor: 'red',
                                    borderWidth: 2,
                                    fill: false,
                                },
                                {
                                   /* label: '95% CI Unfiltered',
                                    data: ciBandUnfiltered,
                                    type: 'line',
                                    showLine: true,
                                    backgroundColor: 'rgba(255, 0, 0, 0.1)',
                                    fill: true,
                                    pointRadius: 0,
                                    borderWidth: 0,
                                },*/
                                label: '95% CI Unfiltered (Upper)',
                                data: ciBandUnfiltered,
                                type: 'line',
                                showLine: true,
                                backgroundColor: 'rgba(255, 0, 0, 0.1)',
                                borderColor: 'transparent', // Hide the border line
                                fill: '+1', // Fill to the next dataset (lower bound)
                                pointRadius: 0,
                                borderWidth: 0,
                                tension: 0.4, // Smooth curves
                                order: 10 // Render behind other lines
                                },
                                {
                                    label: 'LOESS (w/o Outliers)',
                                    data: loessLineFiltered,
                                    type: 'line',
                                    borderColor: 'blue',
                                    borderWidth: 2,
                                    fill: false,
                                },

                                /*{
                                    label: '95% CI Filtered',
                                    data: ciBandFiltered,
                                    type: 'line',
                                    showLine: false,
                                    backgroundColor: 'rgba(0, 0, 255, 0.1)',
                                    fill: true,
                                    pointRadius: 0,
                                    borderWidth: 0,
                                },*/
                                {
                                    label: '95% CI Filtered',
                                    data: ciBandFiltered,
                                    type: 'line',
                                    showLine: true,
                                    backgroundColor: 'rgba(0, 0, 255, 0.1)',  // Blue with 0.1 alpha (same as Python)
                                    borderColor: 'transparent',
                                    fill: true,
                                    pointRadius: 0,
                                    borderWidth: 0,
                                    tension: 0.4,
                                    order: 10
                                },
                                {
                                    label: `Weighted Avg: $${weightedMean.toFixed(2)}`,
                                    data: [
                                        { x: d3.min(quantities), y: weightedMean },
                                        { x: d3.max(quantities), y: weightedMean }
                                    ],
                                    type: 'line',
                                    borderColor: 'black',
                                    borderDash: [5, 5],
                                    fill: false,
                                    borderWidth: 1
                                }
                            ]

                        },
                        options: {
                            responsive: true,
                            maintainAspectRatio: false,
                            scales: {
                                x: {
                                    title: { display: true, text: 'Quantity' },
                                    beginAtZero: true
                                },
                                y: {
                                    title: { display: true, text: 'Unit Price ($)' },
                                    beginAtZero: true
                                }
                            },
                            plugins: {
                                tooltip: {
                                    callbacks: {
                                        label: function (context) {
                                            const label = context.dataset.label || '';
                                            return `${label} - Qty: ${context.parsed.x}, Price: $${context.parsed.y.toFixed(2)}`;
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
                    $scope.$apply();
                });
            }, 0);
        }

        $scope.getBidTypeLabel = function (code) {
            return code ? ($scope.bidTypeMap[code] || "Unknown") : "Unknown";
        };

        $scope.getBidStatusLabel = function (code) {
            return code ? ($scope.bidStatusMap[code] || "Unknown") : "Unknown";
        };
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