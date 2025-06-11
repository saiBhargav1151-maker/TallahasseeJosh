dqeControllers.controller('UnitPriceSearchController', ['$scope', '$rootScope', '$http', '$timeout',
    function ($scope, $rootScope, $http, $timeout) {
        $rootScope.$broadcast('initializeNavigation');
        $scope.searchText = "";
        $scope.items = [];
        $scope.selectedPayItemNumber = null;
        $scope.bidHistoryData = [];
        /*$scope.selectedMinRank = '';
        $scope.selectedMaxRank = '';*/
        $scope.lastSearchedPayItem = $scope.searchText;
        $scope.isLoading = false;
        $scope.draggingThumb = null;
        $scope.monthsOfHistory = 36;
        $scope.selectedBidStatus = "";
        $scope.searchAttempted = false;
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
            "L": "Loss",
            "W": "Wins",
            "I": "Irregular"
        };
        $scope.isInvalidDateRange = function () {
            return $scope.startDate && $scope.endDate && new Date($scope.startDate) > new Date($scope.endDate);
        };
        $scope.contractTypes = Object.keys($scope.contractTypeMap);
        $scope.districtCountyMap = {
            'District 1 (Southwest Florida)': [
                '01 - CHARLOTTE', '03 - COLLIER', '04 - DESOTO', '05 - GLADES', '06 - HARDEE', '07 - HENDRY',
                '09 - HIGHLANDS', '12 - LEE', '13 - MANATEE', '16 - POLK', '17 - SARASOTA', '91 - OKEECHOBEE', '99 - DIST/ST-WIDE'
            ],
            'District 2 (Northeast Florida)': [
                '26 - ALACHUA', '27 - BAKER', '28 - BRADFORD', '29 - COLUMBIA', '30 - DIXIE', '31 - GILCHRIST',
                '32 - HAMILTON', '33 - LAFAYETTE', '34 - LEVY', '35 - MADISON', '37 - SUWANNEE', '38 - TAYLOR',
                '39 - UNION', '71 - CLAY', '72 - DUVAL', '74 - NASSAU', '76 - PUTNAM', '78 - ST JOHNS', '99 - DIST/ST-WIDE'
            ],
            'District 3 (Northwest Florida)': [
                '46 - BAY', '47 - CALHOUN', '48 - ESCAMBIA', '49 - FRANKLIN', '50 - GADSDEN', '51 - GULF',
                '52 - HOLMES', '53 - JACKSON', '54 - JEFFERSON', '55 - LEON', '56 - LIBERTY', '57 - OKALOOSA',
                '58 - SANTA ROSA', '59 - WAKULLA', '60 - WALTON', '61 - WASHINGTON', '99 - DIST/ST-WIDE'
            ],
            'District 4 (Southeast Florida)': [
                '86 - BROWARD', '88 - INDIAN RIVER', '89 - MARTIN', '93 - PALM BEACH', '94 - ST LUCIE', '99 - DIST/ST-WIDE'
            ],
            'District 5 (Central Florida)': [
                '11 - LAKE', '18 - SUMTER', '36 - MARION', '70 - BREVARD', '73 - FLAGLER',
                '75 - ORANGE', '77 - SEMINOLE', '79 - VOLUSIA', '92 - OSCEOLA', '99 - DIST/ST-WIDE'
            ],
            'District 6 (South Florida)': [
                '87 - MIAMI-DADE', '90 - MONROE', '99 - DIST/ST-WIDE'
            ],
            'District 7 (West Central Florida)': [
                '02 - CITRUS', '08 - HERNANDO', '10 - HILLSBOROUGH', '14 - PASCO', '15 - PINELLAS', '99 - DIST/ST-WIDE'
            ],
            "Turnpike": [
                'TURNPIKE'
            ]
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
            "Area 99": ["DIST/ST-WIDE", "DISTRICT WIDE", "TURNPIKE"]
        };
        $scope.selectedMarketArea = "";
        $scope.selectedMarketCounties = [];
        $scope.clearFilters = function () {
            $scope.searchText = "";
            $scope.selectedPayItemNumber = null;
            $scope.selectedMinQuantity = null;
            $scope.selectedMaxQuantity = null;
            $scope.selectedMinRank = null;
            $scope.selectedMaxRank = null;
            $scope.monthsOfHistory = 36;
            $scope.selectedBidStatus = "";
            $scope.selectedContractType = null;
            $scope.selectedWorkTypeCode = null;
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
        };
        $scope.onMarketAreaChange = function () {
            if ($scope.selectedMarketArea && $scope.marketAreaToCountiesMap[$scope.selectedMarketArea]) {
                $scope.selectedMarketCounties = angular.copy($scope.marketAreaToCountiesMap[$scope.selectedMarketArea]);
            } else {
                $scope.selectedMarketCounties = [];
            }
        };
        $scope.startDragging = function (thumb) {
            $scope.draggingThumb = thumb;
            document.addEventListener('mousemove', onThumbDrag);
            document.addEventListener('mouseup', stopDragging);
        };
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
            $scope.availableCounties = rawCounties.map(c => {
                const clean = c.includes(" - ") ? c.split(" - ")[1].trim() : c.trim();
                return {
                    name: clean,
                    selected: $scope.selectedCounties.includes(clean)
                };
            });
        };
        $scope.validateQuantity = function () {
            const min = $scope.selectedMinQuantity;
            const max = $scope.selectedMaxQuantity;
            // Reset error
            $scope.hasError = false;
            $scope.errorMessage = '';

            if (min === undefined || min === null || min < 1) {
                $scope.hasError = true;
                $scope.errorMessage = 'Minimum quantity must be at least 1.';
                return;
            }

            if (max === undefined || max === null || max < 1) {
                $scope.hasError = true;
                $scope.errorMessage = 'Maximum quantity must be at least 1.';
                return;
            }

            if (min !== null && max !== null && min > max) {
                $scope.hasError = true;
                $scope.errorMessage = 'Minimum quantity cannot be greater than maximum quantity.';
                return;
            }
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
            const index = $scope.selectedCounties.indexOf(countyName);
            if (index > -1) $scope.selectedCounties.splice(index, 1);

            const match = $scope.availableCounties.find(c => c.name === countyName);
            if (match) match.selected = false;
        };
        // Fetch Pay Item Suggestions
        $scope.fetchPayItemSuggestions = function () {
            if (debounceTimer) $timeout.cancel(debounceTimer);

            if (!$scope.searchText || $scope.searchText.length < 2) {
                $scope.items = [];
                $scope.selectedPayItemNumber = null;
                return;
            }
            $scope.getBidTypeLabel = function (code) {
                return code ? ($scope.bidTypeMap[code] || "Unknown") : "Unknown";
            };
            $scope.getBidStatusLabel = function (code) {
                return code ? ($scope.bidStatusMap[code] || "Unknown") : "Unknown";
            };
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
        // clear the text box input
        $scope.clearSearchText = function () {
            $scope.searchText = "";
            $scope.items = [];
            $scope.selectedPayItemNumber = null;
        };

        // Select Pay Item
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
            if (!$scope.selectedPayItemNumber) {
                alert("Please enter a valid Pay Item before searching.");
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
                    months: $scope.monthsOfHistory || 36,
                    contractWorkType: $scope.selectedWorkTypeCode || null,
                    startDate: $scope.startDate || null,
                    endDate: $scope.endDate || null,
                    counties: $scope.selectedCounties,
                    bidStatus: $scope.selectedBidStatus || null,
                    contractType: $scope.selectedContractType || null,
                    marketCounties: $scope.selectedMarketCounties,
                    minRank: $scope.selectedMinQuantity || null,
                    maxRank: $scope.selectedMaxQuantity || null,
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

                data.forEach(item => {
                    item.WeightedAvgNoOutliers = weightedAvgNoOutliers;
                });

                $scope.bidHistoryData = data;

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
            return date.toLocaleDateString('en-US'); // Format: MM/DD/YYYY
        }
       
        // CSV Export Functionlity
        $scope.exportClick = function () {
            let headers = [
                "Contract Number", "Duration", "Project Number", "Letting Date", "Pay Item",
                "Description", "Supplemental Description", "Units", "Quantity",
                "Unit Price Bid", "Bid Amount", "Bid Status", "Bid Type", "Weighted Avg", "Weighted Avg No Outliers", "Outlier", "Primary County", "District",
                "Contract Type", "Work Type", "Proposal Type", " Executed Date", "Bidder Rank", "Bidder Name"
            ].join(",") + "\n";

            let rows = $scope.bidHistoryData.map(item => [
                `"${item.p}"`, `"${item.Duration}"`, `"${item.ProjectNumber}"`, `"${formatDotNetDate(item.l)}"`, `"${item.ri}"`,
                `"${item.Description.replace(/"/g, '""')}"`, `"${item.SupplementalDescription}"`, `"${item.CalculatedUnit}"`, `"${item.Quantity}"`,
                `"${item.b}"`, `"${item.PvBidTotal}"`, `"${$scope.getBidStatusLabel(item.BidStatus)}"`, `"${$scope.getBidTypeLabel(item.BidType)}"`, `"${item.WeightedAvg}"`, `"${item.WeightedAvgNoOutliers}"`, `"${item.IsOutlier ? 'Yes' : 'No'}"`, `"${item.c}"`, `"${item.d}"`,
                `"${$scope.contractTypeMap[item.ContractType] || item.ContractType}"`, `"${$scope.workTypeMap[item.ContractWorkType] || item.ContractWorkType}"`,
                `"${$scope.proposalTypeMap[item.ProposalType] || item.ProposalType}"`, `"${formatDotNetDate(item.ExecutedDate)}"`, `"${item.VendorRanking}"`, `"${item.VendorName}"`
            ].join(",")).join("\n");

            let csvContent = "data:text/csv;charset=utf-8," + headers + rows;
            let encodedUri = encodeURI(csvContent);
            let link = document.createElement("a");
            link.setAttribute("href", encodedUri);
            link.setAttribute("download", "bid_history.csv");
            document.body.appendChild(link);
            link.click();
        };
        // Watch for data update
        $scope.$watch('bidHistoryData', function (newVal) {
            if (newVal && newVal.length > 0) {
                waitForCanvasAndRender();
            }
        });
        // Line Graph
        function waitForCanvasAndRender() {
            $scope.isChartLoading = true;

            $timeout(function () {
                requestAnimationFrame(function () {
                    const canvas = document.getElementById("priceChart");
                    /*if (!canvas) {
                        if (retryCount > 0) waitForCanvasAndRender(retryCount - 1);
                        return; retryCount = 10
                    }*/

                    const ctx = canvas.getContext("2d");
                    const quantities = $scope.bidHistoryData.map(item => item.Quantity || 0);
                    const prices = $scope.bidHistoryData.map(item => item.b || 0);
                    const outlierPoints = [], normalPoints = [];

                    const bidPoints = [];
                    for (let i = 0; i < quantities.length; i++) {
                        bidPoints.push({ x: quantities[i], y: prices[i] });
                    }

                    const totalQty = quantities.reduce((sum, q) => sum + q, 0);
                    const weightedAvg = quantities.reduce((sum, q, i) => sum + (q * prices[i]), 0) / totalQty;

                    const weightedStdDev = Math.sqrt(
                        quantities.reduce((sum, q, i) => sum + q * Math.pow(prices[i] - weightedAvg, 2), 0) / totalQty
                    );

                    bidPoints.forEach(point => {
                        const isOutlier = Math.abs(point.y - weightedAvg) > weightedStdDev;
                        const formattedPoint = { x: point.x, y: point.y };

                        if (isOutlier) outlierPoints.push(formattedPoint);
                        else normalPoints.push(formattedPoint);
                    });

                    const n = quantities.length;
                    const meanX = quantities.reduce((a, b) => a + b, 0) / n;
                    const meanY = prices.reduce((a, b) => a + b, 0) / n;
                    const slope = quantities.map((x, i) => (x - meanX) * (prices[i] - meanY)).reduce((a, b) => a + b, 0) /
                        quantities.map(x => Math.pow(x - meanX, 2)).reduce((a, b) => a + b, 0) || 0;
                    const intercept = meanY - slope * meanX;

                    const uniqueQuantities = [...new Set(quantities)].sort((a, b) => a - b);
                    const regressionLine = uniqueQuantities.map(q => ({ x: q, y: slope * q + intercept }));

                    const minQty = Math.min(...quantities);
                    const maxQty = Math.max(...quantities);
                    const weightedAvgLine = [
                        { x: minQty, y: weightedAvg },
                        { x: maxQty, y: weightedAvg }
                    ];

                    if ($scope.chartInstance) $scope.chartInstance.destroy();

                    $scope.chartInstance = new Chart(ctx, {
                        type: 'scatter',
                        data: {
                            datasets: [
                                {
                                    label: 'Non-Outlier Bid Points',
                                    data: normalPoints,
                                    backgroundColor: 'rgba(54, 162, 235, 0.8)',
                                    pointRadius: 8,
                                    pointHoverRadius: 10,
                                    showLine: false
                                },
                                {
                                    label: 'Outlier Bid Points',
                                    data: outlierPoints,
                                    backgroundColor: 'red',
                                    pointRadius: 5,
                                    pointHoverRadius: 8,
                                    showLine: false
                                },
                                {
                                    label: 'Regression Line',
                                    data: regressionLine,
                                    type: 'line',
                                    borderColor: '#dc3545',
                                    fill: false,
                                    tension: 0.5
                                },
                                {
                                    label: `Weighted Avg: $${weightedAvg.toFixed(2)}`,
                                    data: weightedAvgLine,
                                    type: 'line',
                                    borderColor: 'green',
                                    borderWidth: 1,
                                    fill: false,
                                    borderDash: [5, 5]
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
                                }
                            }
                        }
                    });

                    $scope.chartStats = {
                        avg: weightedAvg,
                        totalContracts: new Set($scope.bidHistoryData.map(item => item.p)).size,
                        totalBidAmount: $scope.bidHistoryData.reduce((sum, item) => sum + (item.PvBidTotal || 0), 0),
                        totalQuantity: $scope.bidHistoryData.reduce((sum, item) => sum + (item.Quantity || 0), 0),
                        count: $scope.bidHistoryData.length
                    };

                    $scope.isChartLoading = false;
                    $scope.$apply();
                });
            }, 0);
        }
    }
]);

// calling from HTML Dynamically, Filter to convert MS JSON date to JavaScript Date
angular.module('dqeControllers')
    .filter('msDateToJS', function () {
        return function (input) {
            if (!input) return '';
            var match = /\/Date\((\d+)\)\//.exec(input);
            return match ? new Date(parseInt(match[1])) : input;
        };
    });