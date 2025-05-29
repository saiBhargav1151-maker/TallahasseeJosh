dqeControllers.controller('UnitPriceSearchController', [
    '$scope', '$http', '$timeout',
    function ($scope, $http, $timeout) {
        $scope.searchText = "";
        $scope.items = [];
        $scope.selectedPayItemNumber = null;
        $scope.bidHistoryData = [];
        $scope.isLoading = false;
        $scope.searchAttempted = false;
        let debounceTimer;
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
        // Fetch Pay Item Suggestions
        $scope.fetchPayItemSuggestions = function () {
            if (debounceTimer) $timeout.cancel(debounceTimer);

            if (!$scope.searchText || $scope.searchText.length < 2) {
                $scope.items = [];
                $scope.selectedPayItemNumber = null;
                return;
            }
            $scope.getWorkTypeLabel = function (code) {
                return code ? `${code} - ${$scope.workTypeMap[code] || "Unknown"}` : "";
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

        // Search Bids
        $scope.searchBids = function () {
            if (!$scope.selectedPayItemNumber) {
                alert("Please select a Pay Item from the list.");
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
                params: { number: $scope.selectedPayItemNumber }
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
                    item.PvAwardedLabel = item.PvAwarded ? "Winning Bid" : "Non Winning Bid";

                    if (!isOutlier) {
                        cleanQty.push(qty);
                        cleanPrices.push(price); //Date and time for csv file
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
        // Export Functionlity
        $scope.exportClick = function () {
            let headers = [
                "Proposal Number", "Project Number", "Letting Date", "Pay Item",
                "Description", "Supplemental Description", "Units", "Quantity",
                "Unit Price Bid", "Bid Amount", "Awarded", "Weighted Avg", "Weighted Avg No Outliers", "Outlier", "County", "District",
                "Contract Type", "Work Type", "Proposal Type", "Bidder Name"
            ].join(",") + "\n";

            let rows = $scope.bidHistoryData.map(item => [
                `"${item.p}"`, `"${item.ProjectNumber}"`, `"${new Date(item.l).toLocaleDateString()}"`, `"${item.ri}"`,
                `"${item.Description}"`, `"${item.SupplementalDescription}"`, `"${item.CalculatedUnit}"`, `"${item.Quantity}"`,
                `"${item.b}"`, `"${item.PvBidTotal}"`, `"${item.PvAwardedLabel}"`, `"${item.WeightedAvg}"`, `"${item.WeightedAvgNoOutliers}"`, `"${item.IsOutlier ? 'Yes' : 'No'}"` , `"${item.c}"`, `"${item.d}"`,
                `"${item.ContractType}"`, `"${item.ContractWorkType}"`, `"${item.ProposalType}"`, `"${item.VendorName}"`
            ].join(",")).join("\n");

            let csvContent = "data:text/csv;charset=utf-8," + headers + rows;
            let encodedUri = encodeURI(csvContent);
            let link = document.createElement("a");
            link.setAttribute("href", encodedUri);
            link.setAttribute("download", "bid_history.csv");
            document.body.appendChild(link);
            link.click();
        };



       /* $scope.convertArrayToCSV = function (dataArray) {
            debugger
            const headers = Object.keys(dataArray[0]).join(","); // Extract headers
            const rows = dataArray.map(row => Object.values(row).join(",")).join("\n"); // Convert rows
            return `${headers}\n${rows}`; // Combine headers and rows
        }
        $scope.downloadCSV = function (dataArray, filename = "data.csv") {
            debugger
            const csvContent = $scope.convertArrayToCSV(dataArray);
            const blob = new Blob([csvContent], { type: "text/csv" });
            const link = document.createElement("a");
            link.href = URL.createObjectURL(blob);
            link.download = filename;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        }*/

        // Watch for data update
        $scope.$watch('bidHistoryData', function (newVal) {
            if (newVal && newVal.length > 0) {
                waitForCanvasAndRender();
            }
        });

        // Render Chart
        /*function waitForCanvasAndRender(retryCount = 10) {
            $timeout(function () {
                requestAnimationFrame(function () {
                    const canvas = document.getElementById("priceChart");
                    if (!canvas) {
                        console.warn("Chart canvas not yet rendered. Retrying...");
                        if (retryCount > 0) {
                            waitForCanvasAndRender(retryCount - 1);
                        }
                        return;
                    }

                    const ctx = canvas.getContext("2d");
                    const labels = $scope.bidHistoryData.map(item => item.VendorName || item.p);
                    const prices = $scope.bidHistoryData.map(item => item.b || 0);

                    const avg = prices.reduce((a, b) => a + b, 0) / prices.length;
                    const stdDev = Math.sqrt(
                        prices.reduce((sum, val) => sum + Math.pow(val - avg, 2), 0) / prices.length
                    );

                    if ($scope.chartInstance) {
                        $scope.chartInstance.destroy();
                    }

                    $scope.chartInstance = new Chart(ctx, {
                        type: 'bar',
                        data: {
                            labels: labels,
                            datasets: [
                                {
                                    label: 'Unit Price Bid ($)',
                                    data: prices,
                                    backgroundColor: 'rgba(54, 162, 235, 0.6)'
                                },
                                {
                                    label: `Average ($${avg.toFixed(2)})`,
                                    data: Array(prices.length).fill(avg),
                                    type: 'line',
                                    borderColor: 'green',
                                    borderWidth: 2,
                                    fill: false
                                },
                                {
                                    label: 'Std Dev Range',
                                    data: Array(prices.length).fill(avg + stdDev),
                                    type: 'line',
                                    borderColor: 'orange',
                                    borderWidth: 1,
                                    borderDash: [4, 4],
                                    fill: false
                                }
                            ]
                        },
                        options: {
                            responsive: true,
                            maintainAspectRatio: false,
                            plugins: {
                                tooltip: {
                                    callbacks: {
                                        label: function (context) {
                                            return `$${context.raw.toFixed(2)}`;
                                        }
                                    }
                                }
                            },
                            scales: {
                                y: {
                                    beginAtZero: true,
                                    title: {
                                        display: true,
                                        text: 'Dollars ($)'
                                    }
                                },
                                x: {
                                    ticks: {
                                        autoSkip: false,
                                        maxRotation: 60,
                                        minRotation: 30
                                    }
                                }
                            }
                        }
                    });

                    console.log("✅ Chart rendered.");
                });
            }, 100);
        }*/
        // Line Graph
        function waitForCanvasAndRender(retryCount = 10) {
            $timeout(function () {
                requestAnimationFrame(function () {
                    const canvas = document.getElementById("priceChart");
                    if (!canvas) {
                        if (retryCount > 0) waitForCanvasAndRender(retryCount - 1);
                        return;
                    }

                    const ctx = canvas.getContext("2d");
                    const quantities = $scope.bidHistoryData.map(item => item.Quantity || 0);
                    const prices = $scope.bidHistoryData.map(item => item.b || 0);
                    const outlierPoints = [];
                    const normalPoints = [];

                    // Remove duplicate points
                    /*const bidPoints = [];
                    const seen = new Set();
                    for (let i = 0; i < quantities.length; i++) {
                        const key = `${quantities[i]}-${prices[i]}`;
                        if (!seen.has(key)) {
                            seen.add(key);
                            bidPoints.push({ x: quantities[i], y: prices[i] });
                        }
                    }*/
                    const bidPoints = [];
                    for (let i = 0; i < quantities.length; i++) {
                        bidPoints.push({ x: quantities[i], y: prices[i] });
                    }
                    // Weighted Average Calculation
                    const totalQty = quantities.reduce((sum, q) => sum + q, 0);
                    const weightedAvg = quantities.reduce((sum, q, i) => sum + (q * prices[i]), 0) / totalQty;

                    // Weighted Standard Deviation Calculation
                    const weightedStdDev = Math.sqrt(
                        quantities.reduce((sum, q, i) => sum + q * Math.pow(prices[i] - weightedAvg, 2), 0) / totalQty
                    );

                    // Outlier Detection
                    bidPoints.forEach(point => {
                        const isOutlier = Math.abs(point.y - weightedAvg) > weightedStdDev;
                        point.label = isOutlier ? "Outlier" : "Not Outlier";

                        const formattedPoint = {
                            x: point.x,
                            y: point.y
                        };

                        if (isOutlier) {
                            outlierPoints.push(formattedPoint);
                        } else {
                            normalPoints.push(formattedPoint);
                        }
                    });

                    // Regression Line Calculation
                    const n = quantities.length;
                    const meanX = quantities.reduce((a, b) => a + b, 0) / n;
                    const meanY = prices.reduce((a, b) => a + b, 0) / n;
                    const numerator = quantities.map((x, i) => (x - meanX) * (prices[i] - meanY)).reduce((a, b) => a + b, 0);
                    const denominator = quantities.map(x => Math.pow(x - meanX, 2)).reduce((a, b) => a + b, 0);
                    const slope = numerator / denominator || 0;
                    const intercept = meanY - slope * meanX;

                    const uniqueQuantities = [...new Set(quantities)].sort((a, b) => a - b);
                    const regressionLine = uniqueQuantities.map(q => ({ x: q, y: slope * q + intercept }));

                    // Weighted Avg horizontal line - only 2 points
                    const minQty = Math.min(...quantities);
                    const maxQty = Math.max(...quantities);
                    const weightedAvgLine = [
                        { x: minQty, y: weightedAvg },
                        { x: maxQty, y: weightedAvg }
                    ];
                    if ($scope.chartInstance) {
                        $scope.chartInstance.destroy();
                        $scope.chartInstance = null;
                    }
                   
                    // Update summary stats
                    
                    $scope.chartInstance = new Chart(ctx, {
                        type: 'scatter',
                        data: {
                            datasets: [
                                {
                                    label: 'Non-Outlier Bid Points',
                                    data: normalPoints,
                                    backgroundColor: 'rgba(54, 162, 235, 0.8)',
                                    pointRadius: 5,
                                    pointHoverRadius: 8,
                                    type: 'scatter',
                                    showLine: false
                                },
                                {
                                    label: 'Outlier Bid Points',
                                    data: outlierPoints,
                                    backgroundColor: 'red',
                                    pointRadius: 6,
                                    pointHoverRadius: 10,
                                    type: 'scatter',
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
                                    title: {
                                        display: true,
                                        text: 'Quantity'
                                    },
                                    beginAtZero: true
                                },
                                y: {
                                    title: {
                                        display: true,
                                        text: 'Unit Price ($)'
                                    },
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
                        stdDev: weightedStdDev,
                        slope: slope,
                        intercept: intercept,
                        count: bidPoints.length
                    };
                    // Clear previous chart

                });
            }, 0);
        }




    }
]);

// Filter to convert MS JSON date to JavaScript Date
angular.module('dqeControllers')
    .filter('msDateToJS', function () {
        return function (input) {
            if (!input) return '';
            var match = /\/Date\((\d+)\)\//.exec(input);
            return match ? new Date(parseInt(match[1])) : input;
        };
    });