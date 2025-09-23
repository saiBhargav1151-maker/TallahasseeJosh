dqeControllers.controller('UnitPriceSearchController', ['$scope', '$rootScope', '$http', '$timeout', function ($scope, $rootScope, $http, $timeout) {
    $rootScope.$broadcast('initializeNavigation');
    $rootScope.showStatisticsDetails = true;
    $scope.searchText = '';
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
    $scope.selectedBidStatus = 'FMV';
    $scope.searchAttempted = false;
    $scope.showNormal = true;
    $scope.showOutliers = true;
    $scope.showTrendLine = true;
    $scope.showWeightedAvg = true;
    $scope.sortColumn = 'p';
    $scope.reverseSort = false;
    let exportDebounceTimer;
    $scope.isChartLoading = false;
    $scope.isSearching = false;
    $scope.showSuggestions = false;
    const today = new Date();
    const pastLimit = new Date();
    pastLimit.setMonth(pastLimit.getMonth() - 120);
    $scope.today = today;
    $scope.minAllowedDate = pastLimit;
    $scope.trendAnalysisData = { trendTimeGrouping: 'year' };
    $scope.trendData = [];
    $scope.trendChartInstance = null;
    $scope.isTrendChartLoading = false;
    $scope.showTrendChart = false;
    $scope.trendWarning = '';
    $scope.useInflationAdjustedPrices = true;
    $scope.isExporting = false;
    $scope.customQuantityData = { userQuantity: null };
    $scope.customQuantityPrediction = null;
    $scope.isCalculatingPrediction = false;
    $scope.chartSettings = { loessBandwidth: 0.3 };  

    $scope.availableColumns = [
      { key: 'p', label: 'Contract', visible: true, sortable: true, selectionOrder: 1 },
      { key: 'ProjectNumber', label: 'Project Number', visible: true, sortable: true, selectionOrder: 2 },
      { key: 'ri', label: 'Pay Item', visible: true, sortable: false, selectionOrder: 3 },
      { key: 'Description', label: 'Description', visible: false, sortable: false, selectionOrder: 0 },
      { key: 'SupplementalDescription', label: 'Supp Desc', visible: false, sortable: false, selectionOrder: 0 },
      { key: 'CalculatedUnit', label: 'Units', visible: false, sortable: false, selectionOrder: 0 },
      { key: 'Quantity', label: 'Quantity', visible: true, sortable: true, selectionOrder: 4 },
      { key: 'b', label: 'Unit Price Bid', visible: true, sortable: true, selectionOrder: 5 },
      { key: 'InflationAdjustedPrice', label: 'Adj. Unit Price', visible: true, sortable: true, selectionOrder: 6 },
      { key: 'IsOutlier', label: 'Outlier', visible: false, sortable: true, selectionOrder: 7 },
      { key: 'PvBidTotal', label: 'Bid Amount', visible: true, sortable: true, selectionOrder: 8 },
      { key: 'd', label: 'District', visible: true, sortable: true, selectionOrder: 11 },
      { key: 'MarketArea', label: 'Market Area', visible: true, sortable: true, selectionOrder: 12 },
      { key: 'c', label: 'County', visible: true, sortable: true, selectionOrder: 13 },
      { key: 'VendorName', label: 'Bidder Name', visible: true, sortable: true, selectionOrder: 14 },
      { key: 'BidStatus', label: 'Bid Status', visible: true, sortable: true, selectionOrder: 15 },
      { key: 'VendorRanking', label: 'Bidder Rank', visible: false, sortable: true, selectionOrder: 16 },
      { key: 'ContractType', label: 'Contract Type', visible: true, sortable: true, selectionOrder: 9 },
      { key: 'ContractWorkType', label: 'Work Type', visible: true, sortable: true, selectionOrder: 10 },
      { key: 'WorkMixDescription', label: 'Work Mix', visible: false, sortable: true, selectionOrder: 0 },
      { key: 'CategoryDescription', label: 'Project Category', visible: false, sortable: true, selectionOrder: 0 },
      { key: 'l', label: 'Letting Date', visible: true, sortable: true, selectionOrder: 17 },
      { key: 'ExecutedDate', label: 'Executed Date', visible: false, sortable: false, selectionOrder: 0 },
      { key: 'Duration', label: 'Awarded Days', visible: false, sortable: false, selectionOrder: 0 },
      { key: 'ProposalType', label: 'Proposal Type', visible: false, sortable: false, selectionOrder: 0 },
      { key: 'BidType', label: 'Bid Type', visible: false, sortable: false, selectionOrder: 0 }
    ];
    $scope.nextSelectionOrder = 16;
    $scope.visibleColumns = function () { return $scope.availableColumns.filter((col) => col.visible).sort((a, b) => a.selectionOrder - b.selectionOrder); };
    $scope.showColumnSelector = false;
    $scope.toggleColumnSelector = function () { $scope.showColumnSelector = !$scope.showColumnSelector; };
    $scope.selectAllColumns = function () {
      $scope.nextSelectionOrder = 1;
      $scope.availableColumns.forEach((col) => { col.visible = true; col.selectionOrder = $scope.nextSelectionOrder++; });
    };
    $scope.deselectAllColumns = function () { $scope.availableColumns.forEach((col) => { col.visible = false; col.selectionOrder = 0; }); };
    $scope.resetToDefaultColumns = function () {
      $scope.availableColumns.forEach((col) => { col.visible = ['p', 'ProjectNumber', 'ri', 'Quantity', 'b', 'InflationAdjustedPrice', 'PvBidTotal', 'ContractType', 'ContractWorkType', 'd', 'MarketArea', 'c', 'VendorName', 'BidStatus', 'l'].includes(col.key); });
      $scope.nextSelectionOrder = 1;
      $scope.availableColumns.forEach((col) => { if (col.visible) { col.selectionOrder = $scope.nextSelectionOrder++; } else { col.selectionOrder = 0; } });
    };
    $scope.toggleColumn = function (column) { if (!column.visible) { column.selectionOrder = $scope.nextSelectionOrder++; } else { column.selectionOrder = 0; } column.visible = !column.visible; };
    $scope.workTypeMap = { I: 'Maintenance Other', X0: 'Interstate Construction (new)', X1: 'New Construction', X2: 'Reconstruction', X3: 'Resurfacing', X4: 'Widening & Resurfacing', X5: 'Bridge Construction', X6: 'Bridge Repair', X7: 'Traffic Operations', X8: 'Miscellaneous Construction', X9: 'Interstate Rehabilition', Z: 'Other' };
    $scope.contractTypeMap = { CC: 'Const Contract', CEC: 'Const Emergency Contract', CFR: 'Const Fast Response', CPB: 'Const Push Button', CSL: 'Construction Streamline', MC: 'Maint Contract', MEC: 'Maint Emergency Contract', MFR: 'Maint Fast Response', MLC: 'MT Landscape Install Establish', TO: 'Traffic Operations', TOPB: 'Traffic Operations Push Button' };
    $scope.bidTypeMap = { RESP: 'Responsive', NONR: 'Non-Responsive', IRR: 'Irregular', OTH: 'Other' };
    $scope.proposalTypeMap = { DIST: 'District', CENT: 'Central Office' };
    $scope.bidStatusMap = { W: 'Won', L: 'Loss', I: 'Irregular', FMV: 'Fair Market Value (Bidder Rank 1, 2, 3)' };
    $scope.isInvalidDateRange = function () {
      if (!$scope.startDate || !$scope.endDate) return false;
      const startDate = parseDateWithoutTimezone($scope.startDate);
      const endDate = parseDateWithoutTimezone($scope.endDate);
      if (startDate > endDate) return true;
      const today = new Date();
      const pastLimit = new Date();
      pastLimit.setMonth(pastLimit.getMonth() - 120);
      if (startDate < pastLimit || startDate > today || endDate < pastLimit || endDate > today) return true;
      return false;
    };

    $scope.validationErrors = { bidAmountMin: '', bidAmountMax: '', bidAmountRange: '', quantityMin: '', quantityMax: '', quantityRange: '', monthsOfHistory: '', dateRange: '', regionSelection: '' };
    
    // Helper function to clear validation errors for a specific field
    $scope.clearValidationError = function(fieldName) {
      if ($scope.validationErrors[fieldName]) {
        $scope.validationErrors[fieldName] = '';
      }
    };

    // Helper functions to clear individual fields and their validation errors
    $scope.clearBidAmountMin = function() {
      $scope.selectedMinBidAmount = null;
      $scope.clearValidationError('bidAmountMin');
      $scope.validateBidAmountRange();
    };

    $scope.clearBidAmountMax = function() {
      $scope.selectedMaxBidAmount = null;
      $scope.clearValidationError('bidAmountMax');
      $scope.validateBidAmountRange();
    };

    $scope.clearQuantityMin = function() {
      $scope.selectedMinQuantity = null;
      $scope.clearValidationError('quantityMin');
      $scope.validateQuantityRange();
    };

    $scope.clearQuantityMax = function() {
      $scope.selectedMaxQuantity = null;
      $scope.clearValidationError('quantityMax');
      $scope.validateQuantityRange();
    };

    $scope.clearMonthsOfHistory = function() {
      $scope.monthsOfHistory = null;
      $scope.clearValidationError('monthsOfHistory');
    };

    $scope.validateBidAmount = function(value, type) {
      if (!value || value === '') {
        $scope.validationErrors[`bidAmount${type}`] = '';
        return true;
      }
      const cleanValue = value.toString().replace(/,/g, '');
      if (isNaN(cleanValue)) {
        $scope.validationErrors[`bidAmount${type}`] = 'Please enter a valid number.';
        return false;
      }
      const numValue = parseFloat(cleanValue);
      const strValue = cleanValue.toString();
      const parts = strValue.split('.');
      const wholeDigits = parts[0].length;
      const decimalDigits = parts[1] ? parts[1].length : 0;
      if (wholeDigits > 11 || decimalDigits > 2 || (wholeDigits + decimalDigits) > 16) {
        $scope.validationErrors[`bidAmount${type}`] = 'Bid amount cannot exceed 11 whole digits and 2 decimal places (13 total digits).';
        return false;
      }
      if (numValue < 0) {
        $scope.validationErrors[`bidAmount${type}`] = 'Bid amount cannot be negative.';
        return false;
      }
      $scope.validationErrors[`bidAmount${type}`] = '';
      return true;
    };
    $scope.validateBidAmountRange = function() {
      if (!$scope.selectedMinBidAmount || !$scope.selectedMaxBidAmount) {
        $scope.validationErrors.bidAmountRange = '';
        return true;
      }
      const minValue = parseFloat($scope.selectedMinBidAmount.toString().replace(/,/g, ''));
      const maxValue = parseFloat($scope.selectedMaxBidAmount.toString().replace(/,/g, ''));

      if (minValue > maxValue) {
        $scope.validationErrors.bidAmountRange = 'Minimum bid amount cannot be greater than maximum bid amount.';
        return false;
      }

      $scope.validationErrors.bidAmountRange = '';
      return true;
    };

    $scope.validateQuantity = function(value, type) {
      if (!value || value === '') {
        $scope.validationErrors[`quantity${type}`] = '';
        return true;
      }
      
      if (isNaN(value)) {
        $scope.validationErrors[`quantity${type}`] = 'Please enter a valid number.';
        return false;
      }

      const numValue = parseFloat(value);
      const strValue = value.toString();
      const parts = strValue.split('.');
      const wholeDigits = parts[0].length;
      const decimalDigits = parts[1] ? parts[1].length : 0;
      
      if (wholeDigits > 9 || decimalDigits > 2 || (wholeDigits + decimalDigits) > 12) {
        $scope.validationErrors[`quantity${type}`] = 'Quantity cannot exceed 9 whole digits and 2 decimal places (11 total digits).';
        return false;
      }

      if (numValue < 0) {
        $scope.validationErrors[`quantity${type}`] = 'Quantity cannot be negative.';
        return false;
      }

      $scope.validationErrors[`quantity${type}`] = '';
      return true;
    };

    $scope.validateQuantityRange = function() {
      if (!$scope.selectedMinQuantity || !$scope.selectedMaxQuantity) {
        $scope.validationErrors.quantityRange = '';
        return true;
      }

      const minValue = parseFloat($scope.selectedMinQuantity);
      const maxValue = parseFloat($scope.selectedMaxQuantity);

      if (minValue > maxValue) {
        $scope.validationErrors.quantityRange = 'Minimum quantity cannot be greater than maximum quantity.';
        return false;
      }

      $scope.validationErrors.quantityRange = '';
      return true;
    };

    $scope.validateMonthsOfHistory = function() {
      if (!$scope.monthsOfHistory) {
        if ($scope.startDate || $scope.endDate) {
          $scope.validationErrors.monthsOfHistory = '';
          return true;
        } else {
          $scope.validationErrors.monthsOfHistory = 'Enter valid months of bid history value.';
          return false;
        }
      }

      const months = parseInt($scope.monthsOfHistory);   
      
      if (isNaN(months) || months < 1 || months > 120) {
        $scope.validationErrors.monthsOfHistory = 'Please enter a valid number of months (1-120).';
        return false;
      }

      $scope.validationErrors.monthsOfHistory = '';
      return true;
    };

    $scope.validateDateRange = function() {
      const today = new Date();
      const pastLimit = new Date();
      pastLimit.setMonth(pastLimit.getMonth() - 120); 
      let hasError = false;
      let errorMessage = '';

      if ($scope.startDate) {
      const startDate = parseDateWithoutTimezone($scope.startDate);
        
        if (startDate < pastLimit || startDate > today) {
          hasError = true;
          errorMessage = `Start date must be within the last 10 years (after ${pastLimit.toLocaleDateString()}).`;
        }
      }

      if ($scope.endDate) {
      const endDate = parseDateWithoutTimezone($scope.endDate);

        if (endDate < pastLimit || endDate > today) {
          hasError = true;
          errorMessage = `End date must be within the last 10 years (after ${pastLimit.toLocaleDateString()}).`;
        }
      }

      if ($scope.startDate && $scope.endDate && !hasError) {
        const startDate = parseDateWithoutTimezone($scope.startDate);
        const endDate = parseDateWithoutTimezone($scope.endDate);

        if (startDate > endDate) {
          hasError = true;
          errorMessage = 'Start date cannot be later than end date.';
        }
      }

      if (hasError) {
        $scope.validationErrors.dateRange = errorMessage;
        return false;
      }
      $scope.validationErrors.dateRange = '';
      return true;
    };

    $scope.validateRegionSelection = function() {
      if (!$scope.regionType || $scope.regionType === '') {
        $scope.validationErrors.regionSelection = '';
        return true;
      }

      if ($scope.regionType === 'district' || $scope.regionType === 'market' || $scope.regionType === 'county') {
        if (!$scope.selectedRegions || $scope.selectedRegions.length === 0) {
          const regionName = $scope.regionType === 'district' ? 'District' : 
                           $scope.regionType === 'market' ? 'Market Area' : 'County';
          $scope.validationErrors.regionSelection = `Please select at least one ${regionName} to search.`;
          return false;
        }
      }
      $scope.validationErrors.regionSelection = '';
      return true;
    };

    $scope.validateAllFilters = function() {
      let isValid = true;

      if (!validateBidAmount($scope.selectedMinBidAmount, 'Min')) isValid = false;
      if (!validateBidAmount($scope.selectedMaxBidAmount, 'Max')) isValid = false;
      if (!validateBidAmountRange()) isValid = false;

      if (!validateQuantity($scope.selectedMinQuantity, 'Min')) isValid = false;
      if (!validateQuantity($scope.selectedMaxQuantity, 'Max')) isValid = false;
      if (!validateQuantityRange()) isValid = false;

      if (!validateMonthsOfHistory()) isValid = false;

      if (!validateDateRange()) isValid = false;

      const regionValid = validateRegionSelection();
      if (!regionValid) isValid = false;

      return isValid;
    };

    // Helper functions for validation
    function validateBidAmount(value, type) { return $scope.validateBidAmount(value, type); }
    function validateBidAmountRange() { return $scope.validateBidAmountRange(); }
    function validateQuantity(value, type) { return $scope.validateQuantity(value, type); }
    function validateQuantityRange() { return $scope.validateQuantityRange(); }
    function validateMonthsOfHistory() { return $scope.validateMonthsOfHistory(); }
    function validateDateRange() { return $scope.validateDateRange(); }
    function validateRegionSelection() { return $scope.validateRegionSelection(); }

    function parseDateWithoutTimezone(dateValue) {
      if (!dateValue) return null;
      if (typeof dateValue === 'string') {
        const parts = dateValue.split('/');
        if (parts.length === 3) {
          const month = parseInt(parts[0]) - 1;
          const day = parseInt(parts[1]);
          const year = parseInt(parts[2]);
          return new Date(year, month, day);
        }
      }
      if (dateValue instanceof Date) {
        return dateValue;
      }
      return new Date(dateValue);
    }
    $scope.contractTypes = Object.keys($scope.contractTypeMap);
    $timeout(function () {
      $scope.selectedContractTypes = ['CC'];
    });
    $scope.workTypeCodes = Object.keys($scope.workTypeMap);
    $scope.selectedWorkTypeCodes = [];
    $scope.districtCountyMap = {
      'District 1 (Southwest Florida)': ['01 - CHARLOTTE', '03 - COLLIER', '04 - DESOTO', '05 - GLADES', '06 - HARDEE', '07 - HENDRY', '09 - HIGHLANDS', '12 - LEE', '13 - MANATEE', '16 - POLK', '17 - SARASOTA', '91 - OKEECHOBEE'],
      'District 2 (Northeast Florida)': ['26 - ALACHUA', '27 - BAKER', '28 - BRADFORD', '29 - COLUMBIA', '30 - DIXIE', '31 - GILCHRIST', '32 - HAMILTON', '33 - LAFAYETTE', '34 - LEVY', '35 - MADISON', '37 - SUWANNEE', '38 - TAYLOR', '39 - UNION', '71 - CLAY', '72 - DUVAL', '74 - NASSAU', '76 - PUTNAM', '78 - ST JOHNS'],
      'District 3 (Northwest Florida)': ['46 - BAY', '47 - CALHOUN', '48 - ESCAMBIA', '49 - FRANKLIN', '50 - GADSDEN', '51 - GULF', '52 - HOLMES', '53 - JACKSON', '54 - JEFFERSON', '55 - LEON', '56 - LIBERTY', '57 - OKALOOSA', '58 - SANTA ROSA', '59 - WAKULLA', '60 - WALTON', '61 - WASHINGTON'],
      'District 4 (Southeast Florida)': ['86 - BROWARD', '88 - INDIAN RIVER', '89 - MARTIN', '93 - PALM BEACH', '94 - ST LUCIE'],
      'District 5 (Central Florida)': ['11 - LAKE', '18 - SUMTER', '36 - MARION', '70 - BREVARD', '73 - FLAGLER', '75 - ORANGE', '77 - SEMINOLE', '79 - VOLUSIA', '92 - OSCEOLA'],
      'District 6 (South Florida)': ['87 - MIAMI-DADE', '90 - MONROE'],
      'District 7 (West Central Florida)': ['02 - CITRUS', '08 - HERNANDO', '10 - HILLSBOROUGH', '14 - PASCO', '15 - PINELLAS'],
      'Turnpike ': ['TURNPIKE']
    };
    $scope.marketAreaToCountiesMap = {
      'Area 01': ['BAY', 'ESCAMBIA', 'OKALOOSA', 'SANTA ROSA', 'WALTON'],
      'Area 02': ['CALHOUN', 'FRANKLIN', 'GULF', 'HOLMES', 'JACKSON', 'LIBERTY', 'WASHINGTON'],
      'Area 03': ['GADSDEN', 'JEFFERSON', 'LEON', 'WAKULLA'],
      'Area 04': ['BAKER', 'BRADFORD', 'COLUMBIA', 'DIXIE', 'GILCHRIST', 'HAMILTON', 'LAFAYETTE', 'LEVY', 'MADISON', 'PUTNAM', 'SUWANNEE', 'TAYLOR', 'UNION'],
      'Area 05': ['CLAY', 'DUVAL', 'NASSAU', 'ST JOHNS'],
      'Area 06': ['ALACHUA', 'MARION', 'VOLUSIA'],
      'Area 07': ['CITRUS', 'FLAGLER', 'HERNANDO', 'LAKE', 'PASCO', 'SUMTER'],
      'Area 08': ['BREVARD', 'HILLSBOROUGH', 'ORANGE', 'OSCEOLA', 'PINELLAS', 'POLK', 'SEMINOLE'],
      'Area 09': ['DESOTO', 'GLADES', 'HARDEE', 'HENDRY', 'HIGHLANDS', 'OKEECHOBEE'],
      'Area 10': ['CHARLOTTE', 'COLLIER', 'LEE', 'MANATEE', 'SARASOTA'],
      'Area 11': ['INDIAN RIVER', 'MARTIN', 'ST LUCIE'],
      'Area 12': ['BROWARD', 'PALM BEACH'],
      'Area 13': ['MIAMI-DADE'],
      'Area 14': ['MONROE'],
      'Area 99': ['DIST/ST-WIDE', 'TURNPIKE']
    };
    $scope.searchProjectNumber = '';
    $scope.clearFilters = function () {
      $scope.regionType = '';
      $scope.regionOptions = [];
      $scope.selectedRegions = [];
      $scope.relatedCounties = [];
      $scope.selectedRegionCounties = [];
      $scope.isRegionDropdownOpen = false;
      $scope.searchText = '';
      $scope.selectedPayItemNumber = null;
      $scope.selectedMinQuantity = null;
      $scope.selectedMaxQuantity = null;
      $scope.monthsOfHistory = 36;
      $scope.selectedMinBidAmount = null;
      $scope.selectedMaxBidAmount = null;
      $scope.selectedBidStatus = 'FMV';
      $scope.startDate = null;
      $scope.endDate = null;
      $scope.items = [];
      $scope.showSuggestions = false;
      $scope.isSearching = false;
      $scope.selectedContractTypes = ['CC'];
      $scope.selectedWorkTypeCodes = [];
      $scope.showTrendChart = false;
      $scope.trendAnalysisData.trendTimeGrouping = 'year';
      $scope.trendData = [];
      $scope.trendWarning = '';
      if ($scope.trendChartInstance) {
        $scope.trendChartInstance.destroy();
        $scope.trendChartInstance = null;
      }
      $scope.validationErrors = { bidAmountMin: '', bidAmountMax: '', bidAmountRange: '', quantityMin: '', quantityMax: '', quantityRange: '', monthsOfHistory: '', dateRange: '', regionSelection: '' };
    };
    $scope.recalculateWeightedAverages = function () {
      if (!$scope.bidHistoryData || $scope.bidHistoryData.length === 0) {
        return;
      }
      const quantities = $scope.bidHistoryData.map(
        (item) => item.Quantity || 0
      );
      const prices = $scope.bidHistoryData.map(
        (item) => $scope.getPriceField(item) || 0
      );
      const totalQty = quantities.reduce((sum, q) => sum + q, 0);
      const weightedAvg =
        quantities.reduce((sum, q, i) => sum + q * prices[i], 0) / totalQty;
      const weightedStdDev = Math.sqrt(
        quantities.reduce(
          (sum, q, i) => sum + q * Math.pow(prices[i] - weightedAvg, 2),
          0
        ) / totalQty
      );
      let cleanQty = [],
        cleanPrices = [];
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
      const weightedAvgNoOutliers =
        cleanQty.reduce((sum, q, i) => sum + q * cleanPrices[i], 0) /
        cleanTotalQty;
      $scope.weightedAvgNoOutliers = weightedAvgNoOutliers;
      $scope.bidHistoryData.forEach((item) => {
        item.WeightedAvgNoOutliers = weightedAvgNoOutliers;
      });
    };
    $scope.onInflationToggleChange = function () {
      if ($scope.bidHistoryData && $scope.bidHistoryData.length > 0) {
        $scope.recalculateWeightedAverages();
        waitForCanvasAndRender();
        if ($scope.showTrendChart) {
          $timeout(function () {
            renderTrendChart();
          }, 0);
        }
        $scope.isChartStale = false;
        if (
          $scope.customQuantityData.userQuantity &&
          $scope.customQuantityData.userQuantity > 0
        ) {
          $timeout(function () {
            $scope.calculateCustomQuantityStats();
          }, 100);
        }
      }
    };
    $scope.getPriceField = function (item) {
      if (item.CalculatedUnit === 'LS - Lump Sum') {
        const calculatedUnitPrice =
          item.Quantity > 0 ? item.b / item.Quantity : 0;
        if ($scope.useInflationAdjustedPrices && item.InflationAdjustedPrice) {
          return item.Quantity > 0
            ? item.InflationAdjustedPrice / item.Quantity
            : calculatedUnitPrice;
        }
        return calculatedUnitPrice;
      }

      return $scope.useInflationAdjustedPrices && item.InflationAdjustedPrice
        ? item.InflationAdjustedPrice
        : item.b;
    };
    $scope.searchBids = function () {
      if (
        (!$scope.searchProjectNumber ||
          $scope.searchProjectNumber.trim() === '') &&
        (!$scope.selectedPayItemNumber ||
          $scope.selectedPayItemNumber.trim() === '')
      ) {
        alert(
          'Please enter and select a valid Pay Item Number before searching.'
        );
        return;
      }

      if (!$scope.validateAllFilters()) {
        alert('Please correct the validation errors before searching.');
        return;
      }
      const months = $scope.monthsOfHistory;
      if (!months && !$scope.startDate && !$scope.endDate) {
        alert('Please enter a valid Months of Bid History between 1 and 120, or provide a Letting Date Range.');
        return;
      }
      if (months && (months < 1 || months > 120)) {
        alert('Please enter a valid Months of Bid History between 1 and 120.');
        return;
      }
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
      if ($scope.trendChartInstance) {
        $scope.trendChartInstance.destroy();
        $scope.trendChartInstance = null;
      }

        let districtParam = null;
      if ($scope.regionType === 'district' && $scope.selectedRegions && $scope.selectedRegions.length > 0) {
          districtParam = $scope.selectedRegions.map(region => {
          if (region === 'District 1 (Southwest Florida)') return 'District 1';
          if (region === 'District 2 (Northeast Florida)') return 'District 2';
          if (region === 'District 3 (Northwest Florida)') return 'District 3';
          if (region === 'District 4 (Southeast Florida)') return 'District 4';
          if (region === 'District 5 (Central Florida)') return 'District 5';
          if (region === 'District 6 (South Florida)') return 'District 6';
          if (region === 'District 7 (West Central Florida)') return 'District 7';
          if (region === 'Turnpike ') return 'Turnpike';
          return region;
        });
      }

      $http
        .get('/UnitPriceSearch/GetPayItemDetails', {
          params: {
            number: $scope.selectedPayItemNumber,
            months: $scope.monthsOfHistory || ($scope.startDate || $scope.endDate ? null : 36),
            contractWorkType:
              Array.isArray($scope.selectedWorkTypeCodes) &&
              $scope.selectedWorkTypeCodes.length
                ? $scope.selectedWorkTypeCodes
                : null,
            startDate: $scope.startDate || null,
            endDate: $scope.endDate || null,
            // Pass counties only for market and county region types, district for district type
            counties: ($scope.regionType === 'market' || $scope.regionType === 'county') ? $scope.selectedRegionCounties : null,
            district: districtParam,
            bidStatus: $scope.selectedBidStatus || null,
            contractType:
              Array.isArray($scope.selectedContractTypes) &&
              $scope.selectedContractTypes.length
                ? $scope.selectedContractTypes
                : null,
            minRank: $scope.selectedMinQuantity || null,
            maxRank: $scope.selectedMaxQuantity || Infinity,
            projectNumber: $scope.searchProjectNumber || null,
            minBidAmount: $scope.selectedMinBidAmount
              ? parseFloat(
                  $scope.selectedMinBidAmount.toString().replace(/,/g, '')
                )
              : null,
            maxBidAmount: $scope.selectedMaxBidAmount
              ? parseFloat(
                  $scope.selectedMaxBidAmount.toString().replace(/,/g, '')
                )
              : null,
          },
          traditional: true,
        })
        .success(function (data) {
          const responseSize = JSON.stringify(data).length;
          const maxSize = 1.7 * 1024 * 1024;
          clearBandwidthCache();
          $scope.bidHistoryData = data;
          const quantities = data.map((item) => item.Quantity || 0);
          const prices = data.map((item) => $scope.getPriceField(item) || 0);
          const totalQty = quantities.reduce((sum, q) => sum + q, 0);
          const weightedAvg =
            quantities.reduce((sum, q, i) => sum + q * prices[i], 0) / totalQty;
          const weightedStdDev = Math.sqrt(
            quantities.reduce(
              (sum, q, i) => sum + q * Math.pow(prices[i] - weightedAvg, 2),
              0
            ) / totalQty
          );
          let cleanQty = [],
            cleanPrices = [];
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
          const weightedAvgNoOutliers =
            cleanQty.reduce((sum, q, i) => sum + q * cleanPrices[i], 0) /
            cleanTotalQty;
          $scope.weightedAvgNoOutliers = weightedAvgNoOutliers;
          data.forEach((item) => {
            item.WeightedAvgNoOutliers = weightedAvgNoOutliers;
          });
          $scope.bidHistoryData.forEach(function (bidItem) {
            var itemCounty = bidItem.c;
            var normalizedItemCounty = itemCounty
              ? itemCounty.trim().toUpperCase()
              : '';
            var foundMarketArea = '';
            if (normalizedItemCounty) {
              var marketAreaKeys = Object.keys($scope.marketAreaToCountiesMap);
              for (
                var keyIndex = 0;
                keyIndex < marketAreaKeys.length;
                keyIndex++
              ) {
                var currentMarketArea = marketAreaKeys[keyIndex];
                var countyList =
                  $scope.marketAreaToCountiesMap[currentMarketArea];
                for (
                  var countyIndex = 0;
                  countyIndex < countyList.length;
                  countyIndex++
                ) {
                  var currentCounty = countyList[countyIndex]
                    .trim()
                    .toUpperCase();
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
            bidItem.MarketArea = foundMarketArea || 'Unknown';
          });
          
          $scope.trendData = processTrendData();
          if (responseSize > maxSize) {
            $scope.isLargeDataset = true;
            $scope.largeDatasetMessage = `The dataset is too large (${(
              responseSize /
              (1024 * 1024)
            ).toFixed(
              2
            )} MB) to fully display the table in the browser, as it may impact performance. However, summary statistics and charts are still available for review, and you can download the complete data as a CSV file. Please consider refining your filters for a more responsive experience.`;
          } else {
            $scope.isLargeDataset = false;
          }
        })
        .error(function (err) {
          console.error('Error fetching bid data:', err);
        })
        .finally(function () {
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
          if (
            item.CalculatedUnit === 'LS - Lump Sum' &&
            item.InflationAdjustedPrice
          ) {
            value =
              item.Quantity > 0
                ? item.InflationAdjustedPrice / item.Quantity
                : 0;
          } else {
            value = item.InflationAdjustedPrice || 0;
          }
          break;
        case 'PvBidTotal':
          value = item.PvBidTotal || 0;
          break;
        case 'l':
          if (item.l) {
            value = new Date(
              parseInt(item.l.replace(/\/Date\((\d+)\)\//, '$1'))
            );
          } else {
            value = new Date(0);
          }
          break;
        case 'ExecutedDate':
          if (item.ExecutedDate) {
            value = new Date(
              parseInt(item.ExecutedDate.replace(/\/Date\((\d+)\)\//, '$1'))
            );
          } else {
            value = new Date(0);
          }
          break;
        default:
          value = item[$scope.sortColumn] || '';
      }
      return value;
    };
    $scope.getSortClass = function (column) { if ($scope.sortColumn === column) { return $scope.reverseSort ? 'fa-sort-down' : 'fa-sort-up'; } return ''; };
    $scope.clearProjectNumberSearch = function () { $scope.searchProjectNumber = ''; };
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
        Object.values($scope.districtCountyMap).forEach((countyList) =>
          countyList.forEach((c) => {
            const cleaned = c.includes(' - ')
              ? c.split(' - ')[1].trim()
              : c.trim();
            if (cleaned !== 'TURNPIKE' && cleaned !== 'DIST/ST-WIDE') {
            allCounties.add(cleaned);
            }
          })
        );
        Object.values($scope.marketAreaToCountiesMap).forEach((countyList) =>
          countyList.forEach((c) => {
            const cleaned = c.trim();
            if (cleaned !== 'TURNPIKE' && cleaned !== 'DIST/ST-WIDE') {
              allCounties.add(cleaned);
            }
          })
        );
        $scope.regionOptions = Array.from(allCounties).sort();
      } else {
        $scope.regionOptions = [];
        $scope.relatedCounties = [];
        $scope.selectedRegionCounties = null;
      }
    };
    $scope.toggleRegionSelection = function (option) { const idx = $scope.selectedRegions.indexOf(option); if (idx > -1) { $scope.selectedRegions.splice(idx, 1); } else { $scope.selectedRegions.push(option); } $scope.onMultiRegionChange(); };
    $scope.onMultiRegionChange = function () {
      let combined = [];
      $scope.selectedRegions.forEach((region) => {
        let rawList = [];
        if ($scope.regionType === 'market') {
          rawList = $scope.marketAreaToCountiesMap[region] || [];
        } else if ($scope.regionType === 'county') {
          rawList = [region];
        }
        rawList.forEach((c) => {
          const cleaned = c.includes(' - ')
            ? c.split(' - ')[1].trim()
            : c.trim();
          // For market areas, include all counties including TURNPIKE and DIST/ST-WIDE
          if ($scope.regionType === 'market') {
            if (!combined.includes(cleaned)) {
              combined.push(cleaned);
            }
          } else {
            // For other region types, exclude TURNPIKE and DIST/ST-WIDE
            if (cleaned !== 'TURNPIKE' && cleaned !== 'DIST/ST-WIDE' && !combined.includes(cleaned)) {
              combined.push(cleaned);
            }
          }
        });
      });
      $scope.relatedCounties = combined.map((c) => ({
        name: c,
        selected: true,
      }));
      $scope.selectedRegionCounties = combined;
    };
    $scope.toggleMultiSelectDropdown = function () { $scope.isRegionDropdownOpen = !$scope.isRegionDropdownOpen; };
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
    $scope.toggleRegionCounty = function (county) { const index = $scope.selectedRegionCounties.indexOf(county.name); if (county.selected && index === -1) { $scope.selectedRegionCounties.push(county.name); } else if (!county.selected && index > -1) { $scope.selectedRegionCounties.splice(index, 1); } };
    $scope.selectAllRegionCounties = function () { $scope.relatedCounties.forEach((c) => (c.selected = true)); $scope.selectedRegionCounties = $scope.relatedCounties.map((c) => c.name); };
    $scope.clearAllRegionCounties = function () { $scope.relatedCounties.forEach((c) => (c.selected = false)); $scope.selectedRegionCounties = []; };
    $scope.removeCounty = function (countyName) { const idx = $scope.selectedRegionCounties.indexOf(countyName); if (idx > -1) $scope.selectedRegionCounties.splice(idx, 1); const match = $scope.relatedCounties.find((c) => c.name === countyName); if (match) match.selected = false; };
    $scope.onRegionTypeChange();
    $scope.formatBidAmount = function (type) {
      if (type === 'min' && $scope.selectedMinBidAmount) {
        const value = $scope.selectedMinBidAmount.toString().replace(/,/g, '');
        if (!isNaN(value) && value !== '') {
          const numValue = parseFloat(value);
          $scope.selectedMinBidAmount = numValue.toLocaleString('en-US', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2,
          });
        }
      } else if (type === 'max' && $scope.selectedMaxBidAmount) {
        const value = $scope.selectedMaxBidAmount.toString().replace(/,/g, '');
        if (!isNaN(value) && value !== '') {
          const numValue = parseFloat(value);
          $scope.selectedMaxBidAmount = numValue.toLocaleString('en-US', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2,
          });
        }
      }
    };

    $scope.unformatBidAmount = function (type) {
      if (type === 'min' && $scope.selectedMinBidAmount) {
        $scope.selectedMinBidAmount = $scope.selectedMinBidAmount
          .toString()
          .replace(/,/g, '');
      } else if (type === 'max' && $scope.selectedMaxBidAmount) {
        $scope.selectedMaxBidAmount = $scope.selectedMaxBidAmount
          .toString()
          .replace(/,/g, '');
      }
    };

    $scope.getBidAmountRange = function () {
      const min = parseFloat(
        $scope.selectedMinBidAmount
          ? $scope.selectedMinBidAmount.toString().replace(/,/g, '')
          : 0
      );
      const max = parseFloat(
        $scope.selectedMaxBidAmount
          ? $scope.selectedMaxBidAmount.toString().replace(/,/g, '')
          : 0
      );
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
      const match = $scope.relatedCounties.find((c) => c.name === countyName);
      if (match) {
        match.selected = false;
      }
      $scope.selectedRegionCounties = angular.copy(
        $scope.selectedRegionCounties
      );
    };
    // Fetch Pay Item Suggestions
    $scope.fetchPayItemSuggestions = function () {
      if (!$scope.searchText || $scope.searchText.length < 3) {
        $scope.items = [];
        $scope.selectedPayItemNumber = null;
        return;
      }
      $scope.isSearching = true;
      $http
        .get('/UnitPriceSearch/GetPayItemSuggestions', {
          params: { input: $scope.searchText },
        })
        .success(function (data) {
          $scope.items = data;
            $scope.showSuggestions = true;
            $scope.noMatchingSuggestionsFlag = data.length == 0;
        })
        .error(function (err) {
          console.error('Error fetching pay item suggestions:', err);
        })
        .finally(function () {
          $scope.isSearching = false;
        });
    };
    $scope.onSearchKeyPress = function (event) {
      if (event.keyCode === 13) {
        $scope.fetchPayItemSuggestions();
      }
    };
    
    $scope.$watch('searchText', function(newValue, oldValue) {
      if (newValue !== oldValue && $scope.noMatchingSuggestionsFlag) {
        $scope.noMatchingSuggestionsFlag = false;
      }
    });
   
    $scope.clearSearchText = function () {
      $scope.searchText = '';
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
    $scope.getLatestBidDate = function () {
      if (!$scope.bidHistoryData || $scope.bidHistoryData.length === 0) {
        return null;
      }
      let latestDate = null;
      $scope.bidHistoryData.forEach(function (item) {
        if (item.l) {
          const bidDate = new Date(
            parseInt(item.l.replace(/\/Date\((\d+)\)\//, '$1'))
          );
          if (!latestDate || bidDate > latestDate) {
            latestDate = bidDate;
          }
        }
      });
      return latestDate;
    };
    $scope.shouldHideGraphForLumpSum = function () {
      if (!$scope.bidHistoryData || $scope.bidHistoryData.length === 0)
        return false;
      const allLumpSum = $scope.bidHistoryData.every(
        (item) => item.CalculatedUnit === 'LS - Lump Sum'
      );
      if (allLumpSum) {
        const allQuantityOne = $scope.bidHistoryData.every(
          (item) => (item.Quantity || 0) === 1
        );
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
      if ($scope.isExporting) {
        return;
      }
      if (exportDebounceTimer) {
        $timeout.cancel(exportDebounceTimer);
      }
      $scope.isExporting = true;
      try {
        let headers =
          [
            'Contract Number',
            'Project Number',
            'Pay Item',
            'Description',
            'Supplemental Description',
            'Units',
            'Quantity',
            'Unit Price Bid',
            'Inflation-Adjusted Unit Price',
            'Weighted Avg No Outliers',
            'Outlier',
            'Bid Amount',
            'District',
            'Primary County',
            'Bidder Name',
            'Bid Status',
            'Bidder Rank',
            'Contract Type',
            'Work Type',
            'Work Mix',
            'Project Category',
            'Letting Date',
            'Executed Date',
            'Awarded Days',
            'Proposal Type',
            'Bid Type',
          ].join(',') + '\n';

        let rows = $scope.bidHistoryData
          .map((item) => {
            const unitPrice =
              item.CalculatedUnit === 'LS - Lump Sum'
                ? item.Quantity > 0
                  ? item.b / item.Quantity
                  : 0
                : item.b;
            const inflationAdjustedUnitPrice =
              item.CalculatedUnit === 'LS - Lump Sum' &&
              item.InflationAdjustedPrice
                ? item.Quantity > 0
                  ? item.InflationAdjustedPrice / item.Quantity
                  : 0
                : item.InflationAdjustedPrice || item.b;
            const roundedUnitPrice = parseFloat(unitPrice).toFixed(2);
            const roundedInflationAdjustedUnitPrice = parseFloat(
              inflationAdjustedUnitPrice
            ).toFixed(2);
            const roundedWeightedAvgNoOutliers = parseFloat(
              item.WeightedAvgNoOutliers || 0
            ).toFixed(2);
            const roundedBidAmount = parseFloat(item.PvBidTotal || 0).toFixed(
              2
            );
            return [
              `"${item.p}"`,
              `"${item.ProjectNumber}"`,
              `"${item.ri}"`,
              `"${item.Description.replace(/"/g, '""')}"`,
              `"${item.SupplementalDescription}"`,
              `"${item.CalculatedUnit}"`,
              `"${item.Quantity}"`,
              `"${roundedUnitPrice}"`,
              `"${roundedInflationAdjustedUnitPrice}"`,
              `"${roundedWeightedAvgNoOutliers}"`,
              `"${item.IsOutlier ? 'Yes' : 'No'}"`,
              `"${roundedBidAmount}"`,
              `"${item.d}"`,
              `"${item.c}"`,
              `"${item.VendorName}"`,
              `"${item.BidStatus}"`,
              `"${item.VendorRanking}"`,
              `"${
                $scope.contractTypeMap[item.ContractType] || item.ContractType
              }"`,
              `"${
                $scope.workTypeMap[item.ContractWorkType] ||
                item.ContractWorkType
              }"`,
              `"${item.WorkMixDescription}"`,
              `"${item.CategoryDescription}"`,
              `"${formatDotNetDate(item.l)}"`,
              `"${formatDotNetDate(item.ExecutedDate)}"`,
              `"${item.Duration}"`,
              `"${
                $scope.proposalTypeMap[item.ProposalType] || item.ProposalType
              }"`,
              `"${$scope.getBidTypeLabel(item.BidType)}"`,
            ].join(',');
          })
          .join('\n');

        let csvContent = 'data:text/csv;charset=utf-8,' + headers + rows;
        let encodedUri = encodeURI(csvContent);
        let link = document.createElement('a');
        link.setAttribute('href', encodedUri);
        link.setAttribute('download', 'bid_history.csv');
        link.style.display = 'none';
        document.body.appendChild(link);
        link.click();
        $timeout(function () {
          if (link && link.parentNode) {
            link.parentNode.removeChild(link);
          }
        }, 100);
      } catch (error) {
        console.error('Error exporting CSV:', error);
      } finally {
        exportDebounceTimer = $timeout(function () {
          $scope.isExporting = false;
        }, 500);
      }
    };
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
      if (line.trim()) {
        doc.text(line, x, currentY);
        currentY += lineHeight;
      }
      return currentY;
    }
    // Helper function to format date without timezone conversion
    function formatDateForPDF(dateValue) {
      if (!dateValue) return 'All';
      if (typeof dateValue === 'string') {
        return dateValue;
      }
      if (dateValue instanceof Date) {
        const month = (dateValue.getMonth() + 1).toString().padStart(2, '0');
        const day = dateValue.getDate().toString().padStart(2, '0');
        const year = dateValue.getFullYear();
        return `${month}/${day}/${year}`;
      }
      return dateValue.toLocaleDateString();
    }

      $scope.downloadPDF = function () {
          setTimeout(function () {
              var jsPDF = window.jspdf && window.jspdf.jsPDF;
              var doc = new jsPDF({ unit: 'pt', format: 'a4' });
              let y = 40;
              doc.setTextColor('#1F4288');
              doc.setFontSize(18);
              doc.setFont('helvetica', 'bold');
              doc.text('Florida Department of Transportation', 40, y);
              y += 30;
              doc.setFontSize(14);
              doc.setFont('helvetica', 'normal');
              doc.text('Unit Price Search Report', 40, y);
              y += 30;
              var reportTimestamp = new Date();
              var timestampStr = 'Report generated: ' + reportTimestamp.toLocaleString();
              doc.setTextColor('#666666');
              doc.setFontSize(10);
              doc.text(timestampStr, 40, y);
              y += 25;
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
                  formatDateForPDF($scope.startDate) +
                  ' to ' +
                  formatDateForPDF($scope.endDate),
                  40,
                  y
              );
              y += 15;
              const countiesText = 'Selected Counties: ' + ($scope.selectedRegionCounties && $scope.selectedRegionCounties.length > 0 ? $scope.selectedRegionCounties.join(', ') : 'All');
              y = wrapText(doc, countiesText, 40, y, 500, 15);
              y += 10;
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
                      {
                          label: 'Average Quantity',
                          value: ($scope.chartStats.avgQty || 0).toLocaleString(undefined, {
                              minimumFractionDigits: 2,
                              maximumFractionDigits: 2
                          })
                      },
                      { label: 'Outlier Count', value: $scope.chartStats.outlierCount || 0 }
                  ];
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
                  doc.setFontSize(11);
                  doc.setFont('helvetica', 'bold');
                  doc.text('Weighted Averages:', 40, y);
                  y += 15;
                  doc.setFontSize(10);
                  doc.setFont('helvetica', 'normal');
                  doc.text('Weighted Average Unit Price (' + ($scope.useInflationAdjustedPrices ? 'Inflation-Adjusted' : 'Raw') + '): $' + ($scope.chartStats.avg || 0).toFixed(2), 40, y);
                  y += 15;
                  doc.text('Weighted Average (No Outliers) (' + ($scope.useInflationAdjustedPrices ? 'Inflation-Adjusted' : 'Raw') + '): $' + ($scope.chartStats.weightedAvgNoOutliers || 0).toFixed(2), 40, y);
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
              const pageHeight = doc.internal.pageSize.height;
              doc.setFontSize(9);
              doc.setTextColor(100);
              doc.text('For complete data, please use the CSV export option.', 40, pageHeight - 40);
              doc.text('Report generated by DQE Application', 40, pageHeight - 25);
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
    $scope.$watch(
      'selectedContractTypes',
      function (newVal) {
        if (Array.isArray(newVal) && newVal.includes('ALL')) {
          $scope.selectedContractTypes = angular.copy($scope.contractTypes);
        }
      },
      true
    );

    $scope.$watch(
      'selectedWorkTypeCodes',
      function (newVal) {
        if (Array.isArray(newVal) && newVal.includes('ALL')) {
          $scope.selectedWorkTypeCodes = angular.copy($scope.workTypeCodes);
        }
      },
      true
    );
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
      'selectedMaxBidAmount',
    ];

    $scope.$watch('selectedMinBidAmount', function(newVal, oldVal) {
      if (newVal !== oldVal) {
        $scope.validateBidAmount(newVal, 'Min');
        $scope.validateBidAmountRange();
      }
    });

    $scope.$watch('selectedMaxBidAmount', function(newVal, oldVal) {
      if (newVal !== oldVal) {
        $scope.validateBidAmount(newVal, 'Max');
        $scope.validateBidAmountRange();
      }
    });

    $scope.$watch('selectedMinQuantity', function(newVal, oldVal) {
      if (newVal !== oldVal) {
        $scope.validateQuantity(newVal, 'Min');
        $scope.validateQuantityRange();
      }
    });

    $scope.$watch('selectedMaxQuantity', function(newVal, oldVal) {
      if (newVal !== oldVal) {
        $scope.validateQuantity(newVal, 'Max');
        $scope.validateQuantityRange();
      }
    });

    $scope.$watch('monthsOfHistory', function(newVal, oldVal) {
      if (newVal !== oldVal) {
        $scope.validateMonthsOfHistory();
      }
    });

    $scope.$watch('startDate', function(newVal, oldVal) {
      if (newVal !== oldVal) {
        $scope.validateDateRange();
        $scope.validateMonthsOfHistory(); 
      }
    });

    $scope.$watch('endDate', function(newVal, oldVal) {
      if (newVal !== oldVal) {
        $scope.validateDateRange();
        $scope.validateMonthsOfHistory(); 
      }
    });

    $scope.$watch('regionType', function(newVal, oldVal) {
      if (newVal !== oldVal) {
        $scope.validateRegionSelection();
      }
    });

    $scope.$watch('selectedRegions', function(newVal, oldVal) {
      if (newVal !== oldVal) {
        $scope.validateRegionSelection();
      }
    }, true);

    angular.forEach(filterWatchList, function (filter) {
      $scope.$watch(
        filter,
        function (newVal, oldVal) {
          if (
            newVal !== oldVal &&
            !$scope.isLoading &&
            $scope.bidHistoryData &&
            $scope.bidHistoryData.length > 0 &&
            $scope.searchAttempted &&
            $scope.chartStats &&
            $scope.chartInstance &&
            filter !== 'useInflationAdjustedPrices'
          ) {
            $scope.isChartStale = true;
          }
        },
        true
      );
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
        var weightedMean =
          d3.sum(
            prices.map(function (p, i) {
              return p * quantities[i];
            })
          ) / totalQty;
        var weightedStd = Math.sqrt(
          d3.sum(
            prices.map(function (p, i) {
              return quantities[i] * Math.pow(p - weightedMean, 2);
            })
          ) / totalQty
        );
        return { weightedMean: weightedMean, weightedStd: weightedStd };
      } else {
        var totalQty = quantities.reduce((sum, q) => sum + q, 0);
        var weightedMean =
          prices.reduce((sum, p, i) => sum + p * quantities[i], 0) / totalQty;
        var weightedStd = Math.sqrt(
          prices.reduce(
            (sum, p, i) => sum + quantities[i] * Math.pow(p - weightedMean, 2),
            0
          ) / totalQty
        );
        return { weightedMean: weightedMean, weightedStd: weightedStd };
      }
    }

    function filterOutliers(prices, quantities, weightedMean, weightedStd) {
      return $scope.bidHistoryData
        .map((item, i) => {
          const isOutlier = Math.abs(prices[i] - weightedMean) > weightedStd;
          return !isOutlier
            ? {
                q: quantities[i],
                p: prices[i],
                l: item.l,
                pn: item.p,
              }
            : null;
        })
        .filter((d) => d !== null);
    }

    // helper function to aggregate multiple y-values per x
    function aggregateDataByX(x, y, method) {
      if (method === undefined) method = 'mean';
      var groups = {};
      for (var i = 0; i < x.length; i++) {
        var xVal = x[i];
        if (!groups[xVal]) {
          groups[xVal] = [];
        }
        groups[xVal].push(y[i]);
      }
      var result = [];
      for (var xVal in groups) {
        if (groups.hasOwnProperty(xVal)) {
          var yVals = groups[xVal];
          var aggregatedY;
          switch (method) {
            case 'mean':
              aggregatedY =
                yVals.reduce(function (sum, val) {
                  return sum + val;
                }, 0) / yVals.length;
              break;
            case 'median':
              yVals.sort(function (a, b) {
                return a - b;
              });
              var mid = Math.floor(yVals.length / 2);
              aggregatedY =
                yVals.length % 2 === 0
                  ? (yVals[mid - 1] + yVals[mid]) / 2
                  : yVals[mid];
              break;
            case 'weightedMean':
              if (yVals.length === 1) {
                aggregatedY = yVals[0];
              } else {
                var mean =
                  yVals.reduce(function (sum, val) {
                    return sum + val;
                  }, 0) / yVals.length;
                var variance =
                  yVals.reduce(function (sum, val) {
                    return sum + Math.pow(val - mean, 2);
                  }, 0) / yVals.length;
                if (variance === 0) {
                  aggregatedY = mean;
                } else {
                  var weights = yVals.map(function (val) {
                    var weight = 1 / (1 + Math.pow(val - mean, 2) / variance);
                    return Math.max(0.1, Math.min(10, weight));
                  });
                  var totalWeight = weights.reduce(function (sum, w) {
                    return sum + w;
                  }, 0);
                  aggregatedY =
                    yVals.reduce(function (sum, val, i) {
                      return sum + val * weights[i];
                    }, 0) / totalWeight;
                }
              }
              break;
            case 'trimmedMean':
              if (yVals.length <= 2) {
                aggregatedY =
                  yVals.reduce(function (sum, val) {
                    return sum + val;
                  }, 0) / yVals.length;
              } else {
                yVals.sort(function (a, b) {
                  return a - b;
                });
                var trimCount = Math.floor(yVals.length * 0.1);
                var trimmed = yVals.slice(trimCount, yVals.length - trimCount);
                aggregatedY =
                  trimmed.reduce(function (sum, val) {
                    return sum + val;
                  }, 0) / trimmed.length;
              }
              break;
            case 'first':
              aggregatedY = yVals[0];
              break;
            case 'last':
              aggregatedY = yVals[yVals.length - 1];
              break;
            default:
              aggregatedY =
                yVals.reduce(function (sum, val) {
                  return sum + val;
                }, 0) / yVals.length;
          }
          result.push({ x: parseFloat(xVal), y: aggregatedY });
        }
      }
      return result.sort(function (a, b) {
        return a.x - b.x;
      });
    }

    function getCachedBandwidth(x, y, isFiltered, targetQuantity) {
      if (isFiltered === undefined) isFiltered = false;
      if (targetQuantity === undefined) targetQuantity = null;
      var xSum = x.reduce(function (sum, val) {
        return sum + val;
      }, 0);
      var ySum = y.reduce(function (sum, val) {
        return sum + val;
      }, 0);
      var dataKey = x.length + '_' + xSum.toFixed(2) + '_' + ySum.toFixed(2);
      var userBandwidth = $scope.chartSettings.loessBandwidth || 0.3;
      if (isFiltered) {
        lastFilteredDataKey = dataKey;
        cachedFilteredBandwidth = userBandwidth;
        return userBandwidth;
      } else {
        lastUnfilteredDataKey = dataKey;
        cachedUnfilteredBandwidth = userBandwidth;
        return userBandwidth;
      }
    }
    

    function clearBandwidthCache() {
      cachedUnfilteredBandwidth = null;
      cachedFilteredBandwidth = null;
      lastUnfilteredDataKey = null;
      lastFilteredDataKey = null;
      bandwidthCache = {};
    }

    function interpolateLinear(xArr, yArr, targetX) {
      if (xArr.length === 0 || yArr.length === 0) return NaN;
      if (xArr.length !== yArr.length) return NaN;
      if (xArr.length === 1) return yArr[0];

      var validIndices = [];
      for (var i = 0; i < xArr.length; i++) {
        if (
          isFinite(xArr[i]) &&
          isFinite(yArr[i]) &&
          xArr[i] > 0 &&
          yArr[i] > 0
        ) {
          validIndices.push(i);
        }
      }
      if (validIndices.length === 0) return NaN;
      if (validIndices.length === 1) return yArr[validIndices[0]];

      var validX = validIndices.map(function (i) {
        return xArr[i];
      });
      var validY = validIndices.map(function (i) {
        return yArr[i];
      });

      var exactIndex = validX.indexOf(targetX);
      if (exactIndex !== -1) return validY[exactIndex];

      var leftIndex = -1;
      var rightIndex = -1;
      for (var i = 0; i < validX.length - 1; i++) {
        if (targetX >= validX[i] && targetX <= validX[i + 1]) {
          leftIndex = i;
          rightIndex = i + 1;
          break;
        }
      }

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
      var x1 = validX[leftIndex];
      var x2 = validX[rightIndex];
      var y1 = validY[leftIndex];
      var y2 = validY[rightIndex];

      if (x2 === x1) return y1;
      var slope = (y2 - y1) / (x2 - x1);
      var result = y1 + slope * (targetX - x1);

      return isFinite(result) && result > 0 ? result : NaN;
    }
    function loessSmooth(x, y, bandwidth, xvals) {
      if (
        !Array.isArray(x) ||
        !Array.isArray(y) ||
        !Array.isArray(xvals) ||
        x.length === 0 ||
        y.length === 0 ||
        x.length !== y.length
      ) {
        return xvals.map(function () {
          return NaN;
        });
      }

      try {
        var aggregatedData = aggregateDataByX(x, y, 'weightedMean');
        var uniqueX = aggregatedData.map(function (d) {
          return d.x;
        });
        var uniqueY = aggregatedData.map(function (d) {
          return d.y;
        });

        if (uniqueX.length < 3) {
          return xvals.map(function (xval) {
            return interpolateLinear(uniqueX, uniqueY, xval);
          });
        }

        var sortedIndices = uniqueX
          .map(function (_, i) {
            return i;
          })
          .sort(function (a, b) {
            return uniqueX[a] - uniqueX[b];
          });
        var sortedX = sortedIndices.map(function (i) {
          return uniqueX[i];
        });
        var sortedY = sortedIndices.map(function (i) {
          return uniqueY[i];
        });

        return enhancedManualLoessSmooth(sortedX, sortedY, bandwidth, xvals);
      } catch (error) {
        return enhancedManualLoessSmooth(x, y, bandwidth, xvals);
      }
    }
    function enhancedManualLoessSmooth(x, y, bandwidth, xvals) {
      if (!Array.isArray(x) || !Array.isArray(y) || x.length < 3) {
        return xvals.map(function () {
          return NaN;
        });
      }

      var logDataRange =
        Math.log10(Math.max.apply(null, x)) -
        Math.log10(Math.min.apply(null, x));
      //var dataDensity = x.length / dataRange;
      var baseWindowSize = Math.max(4, Math.floor(bandwidth * x.length));
      var windowSize = baseWindowSize;
      if (logDataRange > 3) {
        windowSize = Math.min(x.length, Math.floor(baseWindowSize * 1.2));
      } else if (logDataRange < 1.5) {
        windowSize = Math.max(4, Math.floor(baseWindowSize * 0.8));
      }
      //if (dataDensity > 50) {
      //  windowSize = Math.max(4, Math.floor(windowSize * 0.7));
      //} else if (dataDensity < 1) {
      //  windowSize = Math.min(x.length, Math.floor(windowSize * 1.4));
      //}
      return xvals.map(function (xval) {
        var distances = x.map(function (xi, i) {
          var logDist = Math.abs(Math.log10(xi) - Math.log10(xval));
          var linearDist = Math.abs(xi - xval);
          return {
            dist: logDist,
            linearDist: linearDist,
            index: i,
            x: xi,
            y: y[i],
          };
        });
        distances.sort(function (a, b) {
          return a.dist - b.dist;
        });
        var nearestPoints = distances.slice(
          0,
          Math.min(windowSize, distances.length)
        );
        var maxLogDistance = nearestPoints[nearestPoints.length - 1].dist;
        var weightedSum = 0;
        var totalWeight = 0;
        var validPoints = 0;
        
        for (var i = 0; i < nearestPoints.length; i++) {
          var point = nearestPoints[i];
          var normalizedLogDist =
            maxLogDistance > 0 ? point.dist / maxLogDistance : 0;
          
          var weight = 0;
          if (normalizedLogDist < 1) {
            var absDist = Math.abs(normalizedLogDist);
            weight = Math.pow(1 - Math.pow(absDist, 3), 3);
            if (point.dist < 0.1) {
              weight = Math.max(weight, 0.9);
            }
          }
          
          if (point.x === xval) {
            weight = 1.0;
          }
          
          var proximityBonus = Math.exp(-point.dist * 2);
          weight = Math.max(weight, proximityBonus * 0.5);
          
          if (weight > 0 && isFinite(point.y) && point.y > 0) {
            weightedSum += point.y * weight;
            totalWeight += weight;
            validPoints++;
          }
        }
        if (totalWeight <= 0 || validPoints < 2) {
          var logResult = interpolateLogScale(x, y, xval);
          if (isFinite(logResult) && logResult > 0) {
            return logResult;
          }
          var linearResult = interpolateLinear(x, y, xval);
          if (isFinite(linearResult) && linearResult > 0) {
            return linearResult;
          }
          if (nearestPoints.length > 0) {
            var nearestWeightedSum = 0;
            var nearestTotalWeight = 0;
            for (var j = 0; j < Math.min(3, nearestPoints.length); j++) {
              var np = nearestPoints[j];
              var nw = 1 / (1 + np.dist);
              nearestWeightedSum += np.y * nw;
              nearestTotalWeight += nw;
            }
            return nearestTotalWeight > 0
              ? nearestWeightedSum / nearestTotalWeight
              : nearestPoints[0].y;
          }
          return NaN;
        }

        var result = weightedSum / totalWeight;
        if (!isFinite(result) || result <= 0) {
          var altResult = interpolateLogScale(x, y, xval);
          if (isFinite(altResult) && altResult > 0) {
            return altResult;
          }

          altResult = interpolateLinear(x, y, xval);
          if (isFinite(altResult) && altResult > 0) {
            return altResult;
          }
          return nearestPoints.length > 0 ? nearestPoints[0].y : NaN;
        }
        return result;
      });
    }
    // New log-scale interpolation function
    function interpolateLogScale(xArr, yArr, targetX) {
      if (xArr.length === 0 || yArr.length === 0 || targetX <= 0) return NaN;
      if (xArr.length !== yArr.length) return NaN;
      if (xArr.length === 1) return yArr[0];
      var validData = [];
      for (var i = 0; i < xArr.length; i++) {
        if (
          isFinite(xArr[i]) &&
          isFinite(yArr[i]) &&
          xArr[i] > 0 &&
          yArr[i] > 0
        ) {
          validData.push({
            logX: Math.log10(xArr[i]),
            logY: Math.log10(yArr[i]),
            x: xArr[i],
            y: yArr[i],
          });
        }
      }

      if (validData.length === 0) return NaN;
      if (validData.length === 1) return validData[0].y;
      validData.sort(function (a, b) {
        return a.logX - b.logX;
      });
      var targetLogX = Math.log10(targetX);
      var leftIndex = -1;
      var rightIndex = -1;
      for (var i = 0; i < validData.length - 1; i++) {
        if (
          targetLogX >= validData[i].logX &&
          targetLogX <= validData[i + 1].logX
        ) {
          leftIndex = i;
          rightIndex = i + 1;
          break;
        }
      }

      if (leftIndex === -1) {
        if (targetLogX < validData[0].logX) {
          leftIndex = 0;
          rightIndex = 1;
        } else if (targetLogX > validData[validData.length - 1].logX) {
          leftIndex = validData.length - 2;
          rightIndex = validData.length - 1;
        } else {
          return NaN;
        }
      }
      var x1 = validData[leftIndex].logX;
      var x2 = validData[rightIndex].logX;
      var y1 = validData[leftIndex].logY;
      var y2 = validData[rightIndex].logY;
      if (x2 === x1) return Math.pow(10, y1);
      var slope = (y2 - y1) / (x2 - x1);
      var logResult = y1 + slope * (targetLogX - x1);
      var result = Math.pow(10, logResult);
      return isFinite(result) && result > 0 ? result : NaN;
    }

    $scope.onChartBandwidthChange = function () {
      clearBandwidthCache();
      if ($scope.bidHistoryData && $scope.bidHistoryData.length > 0) {
        if ($scope.chartInstance) {
          $scope.chartInstance.destroy();
          $scope.chartInstance = null;
        }
        $timeout(function () {
          waitForCanvasAndRender();
        }, 100);
        if ($scope.customQuantityData.userQuantity && 
            $scope.customQuantityData.userQuantity > 0 && 
            $scope.isUserQuantityInRange()) {
          $timeout(function () {
            $scope.calculateCustomQuantityStats();
          }, 200);
        }
      }
    };
    $scope.toggleTrendChart = function () {
      $scope.showTrendChart = !$scope.showTrendChart;
      if ($scope.showTrendChart) {
        $timeout(function () {
          renderTrendChart();
        }, 100);
      }
    };
    $scope.onTrendTimeGroupingChange = function () {
      if ($scope.showTrendChart) {
        $timeout(function () {
          renderTrendChart();
        }, 100);
      }
    };

    $scope.calculateCustomQuantityStats = function () {
      if (
        !$scope.customQuantityData.userQuantity ||
        $scope.customQuantityData.userQuantity <= 0
      ) {
        return;
      }

      $scope.isCalculatingPrediction = true;
      $scope.customQuantityPrediction = null;

      $timeout(function () {
        try {
          var userQty = parseFloat($scope.customQuantityData.userQuantity);
          var quantities = $scope.bidHistoryData.map(function (item) {
            return item.Quantity || 0;
          });
          var prices = $scope.bidHistoryData.map(function (item) {
            return $scope.getPriceField(item) || 0;
          });

          var adaptiveBandwidth = getCachedBandwidth(
            quantities,
            prices,
            false,
            userQty
          );

          var loessUnfiltered = loessSmooth(
            quantities,
            prices,
            adaptiveBandwidth,
            [userQty]
          );
          var stats = computeWeightedStats(prices, quantities);
          var filtered = filterOutliers(
            prices,
            quantities,
            stats.weightedMean,
            stats.weightedStd
          );
          var filteredQuantities = filtered.map(function (d) {
            return d.q;
          });
          var filteredPrices = filtered.map(function (d) {
            return d.p;
          });
          var adaptiveBandwidthFiltered = getCachedBandwidth(
            filteredQuantities,
            filteredPrices,
            true,
            userQty
          );
          var loessFiltered = loessSmooth(
            filteredQuantities,
            filteredPrices,
            adaptiveBandwidthFiltered,
            [userQty]
          );
          var ciUnfiltered = bootstrapCI(
            quantities,
            prices,
            [userQty],
            adaptiveBandwidth,
            200
          );
          var ciFiltered = bootstrapCI(
            filteredQuantities,
            filteredPrices,
            [userQty],
            adaptiveBandwidthFiltered,
            200
          );
          $scope.customQuantityPrediction = {
            success: true,
            loessUnfiltered: loessUnfiltered[0],
            loessUnfilteredTotal: loessUnfiltered[0] * userQty,
            loessUnfilteredUpper: ciUnfiltered.upper[0],
            loessUnfilteredLower: ciUnfiltered.lower[0],
            loessFiltered: loessFiltered[0],
            loessFilteredTotal: loessFiltered[0] * userQty,
            loessFilteredUpper: ciFiltered.upper[0],
            loessFilteredLower: ciFiltered.lower[0],
            weightedAvg: stats.weightedMean,
            weightedAvgTotal: stats.weightedMean * userQty,
            weightedAvgNoOutliers: $scope.weightedAvgNoOutliers,
            weightedAvgNoOutliersTotal: $scope.weightedAvgNoOutliers * userQty,
            dataPoints: quantities.length,
            dataPointsNoOutliers: filteredQuantities.length,
          };
        } catch (error) {
          console.error('Custom quantity calculation error:', error);
          $scope.customQuantityPrediction = {
            success: false,
            message:
              'Unable to calculate prediction for this quantity. Please try a different value.',
          };
        }

        $scope.isCalculatingPrediction = false;
      }, 100);
    };


    function renderTrendChart() {
      console.log(
        'Rendering trend chart with grouping:',
        $scope.trendAnalysisData.trendTimeGrouping
      );
    }
    function bootstrapCI(x, y, xvals, frac, nBoot) {
      if (nBoot === undefined) nBoot = 400;

      if (!x.length || !y.length || x.length !== y.length) {
        return {
          lower: xvals.map(function () {
            return null;
          }),
          upper: xvals.map(function () {
            return null;
          }),
        };
      }
      var adaptiveNBoot = nBoot;
      if (x.length > 200) {
        adaptiveNBoot = Math.min(nBoot, 250);
      } else if (x.length > 100) {
        adaptiveNBoot = Math.min(nBoot, 300);
      } else if (x.length < 20) {
        adaptiveNBoot = Math.min(nBoot, 150);
      }

      var seed = 12345;
      function seededRandom() {
        seed = (seed * 9301 + 49297) % 233280;
        return seed / 233280;
      }

      var preds = [];
      var validBootstrapCount = 0;
      var failedBootstraps = 0;

      for (
        var b = 0;
        b < adaptiveNBoot && failedBootstraps < adaptiveNBoot * 0.3;
        b++
      ) {
        var indices = [];
        var sortedIndices = x
          .map(function (_, i) {
            return i;
          })
          .sort(function (a, b) {
            return x[a] - x[b];
          });

        var strataSize = Math.max(1, Math.floor(x.length / 10));
        var selectedIndices = [];
        for (var s = 0; s < x.length; s += strataSize) {
          var strataEnd = Math.min(s + strataSize, x.length);
          var strataIndices = sortedIndices.slice(s, strataEnd);
          for (var j = 0; j < strataIndices.length; j++) {
            var randomIndex =
              strataIndices[Math.floor(seededRandom() * strataIndices.length)];
            selectedIndices.push(randomIndex);
          }
        }
        for (var k = selectedIndices.length - 1; k > 0; k--) {
          var randomIdx = Math.floor(seededRandom() * (k + 1));
          var temp = selectedIndices[k];
          selectedIndices[k] = selectedIndices[randomIdx];
          selectedIndices[randomIdx] = temp;
        }
        var xBoot = selectedIndices.map(function (i) {
          return x[i];
        });
        var yBoot = selectedIndices.map(function (i) {
          return y[i];
        });
        try {
          var smoothed = loessSmooth(xBoot, yBoot, frac, xvals);
          var validResults = smoothed.filter(function (v) {
            return v !== null && !isNaN(v) && isFinite(v) && v > 0;
          });
          if (validResults.length >= xvals.length * 0.7) {
            var median = validResults.slice().sort(function (a, b) {
              return a - b;
            })[Math.floor(validResults.length / 2)];
            var extremeCount = validResults.filter(function (v) {
              return v > median * 10 || v < median / 10;
            }).length;
            if (extremeCount < validResults.length * 0.2) {
              preds.push(smoothed);
              validBootstrapCount++;
            } else {
              failedBootstraps++;
            }
          } else {
            failedBootstraps++;
          }
        } catch (error) {
          console.warn('Bootstrap iteration failed:', error);
          failedBootstraps++;
          continue;
        }
      }
      if (validBootstrapCount < Math.max(10, adaptiveNBoot * 0.2)) {
        console.warn(
          'Insufficient valid bootstrap samples:',
          validBootstrapCount,
          'out of',
          adaptiveNBoot
        );
        return {
          lower: xvals.map(function () {
            return null;
          }),
          upper: xvals.map(function () {
            return null;
          }),
        };
      }
      var lower = [];
      var upper = [];
      for (var i = 0; i < xvals.length; i++) {
        var valuesAtPoint = preds
          .map(function (row) {
            return row[i];
          })
          .filter(function (v) {
            return v !== null && !isNaN(v) && isFinite(v) && v > 0;
          });
        if (valuesAtPoint.length >= Math.max(5, validBootstrapCount * 0.5)) {
          valuesAtPoint.sort(function (a, b) {
            return a - b;
          });
          var lowerIdx = Math.floor(valuesAtPoint.length * 0.05);
          var upperIdx = Math.floor(valuesAtPoint.length * 0.95);
          lowerIdx = Math.max(0, lowerIdx);
          upperIdx = Math.min(valuesAtPoint.length - 1, upperIdx);
          var median = valuesAtPoint[Math.floor(valuesAtPoint.length / 2)];
          var proposedLower = Math.max(0, valuesAtPoint[lowerIdx]);
          var proposedUpper = valuesAtPoint[upperIdx];
          if (proposedUpper > median * 5) {
            proposedUpper = median * 3;
          }
          if (proposedLower < median / 5) {
            proposedLower = Math.max(0, median / 3);
          }

          lower[i] = proposedLower;
          upper[i] = proposedUpper;
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
        if (typeof requestAnimationFrame === 'function') {
          requestAnimationFrame(function () {
            const canvas = document.getElementById('priceChart');
            if (!canvas) {
              $scope.isChartLoading = false;
              return;
            }
            if (!$scope.bidHistoryData || $scope.bidHistoryData.length === 0) {
              $scope.isChartLoading = false;
              return;
            }
            if ($scope.shouldHideGraphForLumpSum()) {
              $scope.isChartLoading = false;
              return;
            }
            var quantities = $scope.bidHistoryData.map(function (item) {
              return item.Quantity || 0;
            });
            var prices = $scope.bidHistoryData.map(function (item) {
              return $scope.getPriceField(item) || 0;
            });
            var stats = computeWeightedStats(prices, quantities);
            var weightedMean = stats.weightedMean;
            var weightedStd = stats.weightedStd;
            var filtered = filterOutliers(
              prices,
              quantities,
              weightedMean,
              weightedStd
            );
            var QuantityFiltered = filtered.map(function (d) {
              return d.q;
            });
            var PriceFiltered = filtered.map(function (d) {
              return d.p;
            });
            var quantitySet = {};
            for (var i = 0; i < quantities.length; i++) {
              quantitySet[quantities[i]] = true;
            }
            var quantityRange = Object.keys(quantitySet)
              .map(function (q) {
                return parseFloat(q);
              })
              .sort(function (a, b) {
                return a - b;
              });
            var filteredSet = {};
            for (var i = 0; i < QuantityFiltered.length; i++) {
              filteredSet[QuantityFiltered[i]] = true;
            }
            var quantityRangeFiltered = Object.keys(filteredSet)
              .map(function (q) {
                return parseFloat(q);
              })
              .sort(function (a, b) {
                return a - b;
              });
            var adaptiveBandwidthUnfiltered = getCachedBandwidth(
              quantities,
              prices,
              false
            );
            var adaptiveBandwidthFiltered = getCachedBandwidth(
              QuantityFiltered,
              PriceFiltered,
              true
            );
            var loessUnfiltered = loessSmooth(
              quantities,
              prices,
              adaptiveBandwidthUnfiltered,
              quantityRange
            );
            var loessFiltered = loessSmooth(
              QuantityFiltered,
              PriceFiltered,
              adaptiveBandwidthFiltered,
              quantityRangeFiltered
            );
            var ciUnfiltered = bootstrapCI(
              quantities,
              prices,
              quantityRange,
              adaptiveBandwidthUnfiltered,
              500
            );
            var ciFiltered = bootstrapCI(
              QuantityFiltered,
              PriceFiltered,
              quantityRangeFiltered,
              adaptiveBandwidthFiltered,
              500
            );
            var lowerUnfiltered = quantityRange
              .map(function (q, i) {
                return { x: q, y: ciUnfiltered.lower[i] };
              })
              .filter(function (pt) {
                return pt.x > 0 && pt.y > 0 && isFinite(pt.x) && isFinite(pt.y);
              });
            var upperUnfiltered = quantityRange
              .map(function (q, i) {
                return { x: q, y: ciUnfiltered.upper[i] };
              })
              .filter(function (pt) {
                return pt.x > 0 && pt.y > 0 && isFinite(pt.x) && isFinite(pt.y);
              });
            var lowerFiltered = quantityRangeFiltered
              .map(function (q, i) {
                return { x: q, y: ciFiltered.lower[i] };
              })
              .filter(function (pt) {
                return pt.x > 0 && pt.y > 0 && isFinite(pt.x) && isFinite(pt.y);
              });
            var upperFiltered = quantityRangeFiltered
              .map(function (q, i) {
                return { x: q, y: ciFiltered.upper[i] };
              })
              .filter(function (pt) {
                return pt.x > 0 && pt.y > 0 && isFinite(pt.x) && isFinite(pt.y);
              });
            var filteredPoints = filtered.map(function (d) {
              var price = d.p;
              if (price > 0 && price < 0.01) {
                price = 0.01;
              }
              return {
                x: d.q,
                y: price,
                l: d.l,
                p: d.pn,
              };
            });
            var loessLineUnfiltered = quantityRange
              .map(function (q, i) {
                var price = loessUnfiltered[i];
                if (price > 0 && price < 0.01) {
                  price = 0.01;
                }
                return { x: q, y: price };
              })
              .filter(function (pt) {
                return pt.x > 0 && pt.y > 0 && isFinite(pt.x) && isFinite(pt.y);
              });
            var loessLineFiltered = quantityRangeFiltered
              .map(function (q, i) {
                var price = loessFiltered[i];
                if (price > 0 && price < 0.01) {
                  price = 0.01;
                }
                return { x: q, y: price };
              })
              .filter(function (pt) {
                return pt.x > 0 && pt.y > 0 && isFinite(pt.x) && isFinite(pt.y);
              });
            var fillGapsInLine = function (lineData) {
              if (lineData.length < 2) return lineData;
              var maxPoints = 50;
              if (lineData.length <= maxPoints) {
                return lineData;
              }
              var step = Math.ceil(lineData.length / maxPoints);
              var sampled = [];

              for (var i = 0; i < lineData.length; i += step) {
                sampled.push(lineData[i]);
              }
              if (
                sampled[sampled.length - 1] !== lineData[lineData.length - 1]
              ) {
                sampled.push(lineData[lineData.length - 1]);
              }
              return sampled;
            };
            var filledLoessUnfiltered = fillGapsInLine(loessLineUnfiltered);
            var filledLoessFiltered = fillGapsInLine(loessLineFiltered);
            var fillGapsInConfidenceIntervals = function (
              lowerData,
              upperData
            ) {
              if (lowerData.length < 2 || upperData.length < 2)
                return { lower: lowerData, upper: upperData };
              var maxCIPoints = 30;
              var reducePoints = function (data) {
                if (data.length <= maxCIPoints) return data;
                var step = Math.ceil(data.length / maxCIPoints);
                var sampled = [];
                for (var i = 0; i < data.length; i += step) {
                  sampled.push(data[i]);
                }
                if (sampled[sampled.length - 1] !== data[data.length - 1]) {
                  sampled.push(data[data.length - 1]);
                }
                return sampled;
              };
              return {
                lower: reducePoints(lowerData),
                upper: reducePoints(upperData),
              };
            };

            var filledConfidenceUnfiltered = fillGapsInConfidenceIntervals(
              lowerUnfiltered,
              upperUnfiltered
            );
            var filledConfidenceFiltered = fillGapsInConfidenceIntervals(
              lowerFiltered,
              upperFiltered
            );
            if (quantities.length === 0 || prices.length === 0) {
              $scope.isChartLoading = false;
              return;
            }

            var outlierPoints = [];
            var normalPoints = [];
            var bidPoints = [];
            for (var i = 0; i < $scope.bidHistoryData.length; i++) {
              var item = $scope.bidHistoryData[i];
              var quantity = item.Quantity || 0;
              let price = $scope.getPriceField(item) || 0;
              if (price > 0 && price < 0.01) {
                price = 0.01;
              }
              bidPoints.push({
                x: quantity,
                y: price,
                l: item.l || 'N/A',
                p: item.p || 'N/A',
              });
            }
            const totalQty = quantities.reduce((sum, q) => sum + q, 0);
            if (totalQty === 0) {
              $scope.isChartLoading = false;
              return;
            }
            const weightedAvg =
              quantities.reduce((sum, q, i) => sum + q * prices[i], 0) /
              totalQty;
            const weightedStdDev = Math.sqrt(
              quantities.reduce(
                (sum, q, i) => sum + q * Math.pow(prices[i] - weightedAvg, 2),
                0
              ) / totalQty
            );
            bidPoints.forEach((point) => {
              const isOutlier =
                Math.abs(point.y - weightedAvg) > weightedStdDev;
              if (isOutlier) {
                outlierPoints.push({
                  x: point.x,
                  y: point.y,
                  l: point.l,
                  p: point.p,
                });
              } else {
                normalPoints.push({
                  x: point.x,
                  y: point.y,
                  l: point.l,
                  p: point.p,
                });
              }
            });
            if ($scope.chartInstance) {
              $scope.chartInstance.destroy();
              $scope.chartInstance = null;
            }
            const chartContainer =
              document.getElementById('priceChart').parentNode;
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
                    stepped: false,
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
                    stepped: false,
                  },
                  {
                    label: 'Weighted Avg',
                    data: [
                      {
                        x: Math.min(...quantities),
                        y:
                          weightedMean > 0 && weightedMean < 0.01
                            ? 0.01
                            : weightedMean,
                      },
                      {
                        x: Math.max(...quantities),
                        y:
                          weightedMean > 0 && weightedMean < 0.01
                            ? 0.01
                            : weightedMean,
                      },
                    ],
                    type: 'line',
                    borderColor: 'black',
                    borderDash: [5, 5],
                    fill: false,
                    borderWidth: 1,
                  },
                  {
                    label: 'Weighted Avg (No Outliers)',
                    data: [
                      {
                        x: Math.min(...QuantityFiltered),
                        y:
                          $scope.weightedAvgNoOutliers > 0 &&
                          $scope.weightedAvgNoOutliers < 0.01
                            ? 0.01
                            : $scope.weightedAvgNoOutliers,
                      },
                      {
                        x: Math.max(...QuantityFiltered),
                        y:
                          $scope.weightedAvgNoOutliers > 0 &&
                          $scope.weightedAvgNoOutliers < 0.01
                            ? 0.01
                            : $scope.weightedAvgNoOutliers,
                      },
                    ],
                    type: 'line',
                    borderColor: '#6366f1',
                    borderDash: [8, 8],
                    fill: false,
                    borderWidth: 1,
                  },
                  {
                    label: '95% CI Outliers (Lower)',
                    data: filledConfidenceUnfiltered.lower,
                    type: 'line',
                    borderColor: 'rgba(255,0,0,0.5)',
                    backgroundColor: 'rgba(255,0,0,0.1)',
                    fill: '+1',
                    pointRadius: 0,
                    borderWidth: 2,
                    tension: 0.4,
                    order: 0,
                    hidden: true,
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
                    order: 0,
                    hidden: true,
                  },
                  {
                    label: '95% CI No Outliers (Lower)',
                    data: filledConfidenceFiltered.lower,
                    type: 'line',
                    borderColor: 'rgba(0,0,255,0.5)',
                    backgroundColor: 'rgba(0,0,255,0.1)',
                    fill: '+1',
                    pointRadius: 0,
                    borderWidth: 2,
                    tension: 0.4,
                    order: 0,
                    hidden: true,
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
                    order: 0,
                    hidden: true,
                  },
                  {
                    label: 'Bid Point',
                    data: filteredPoints,
                    backgroundColor: 'rgba(0, 128, 0, 0.5)',
                    pointStyle: 'circle',
                    pointRadius: 3,
                    pointHoverRadius: 10,
                  },
                  {
                    label: 'Bid Point Outlier',
                    data: outlierPoints,
                    backgroundColor: 'rgba(128, 128, 128, 0.3)',
                    pointRadius: 3,
                    pointHoverRadius: 10,
                  },
                ],
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
                      maxTicksLimit: 6,
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
                      },
                    },
                  },
                  y: {
                    type: 'logarithmic',
                    title: { display: true, text: 'Unit Price (log scale)' },
                    grid: {
                      display: true,
                      drawTicks: true,
                      tickLength: 8,
                      color: 'rgba(0,0,0,0.1)',
                      maxTicksLimit: 6,
                    },
                    ticks: {
                      maxTicksLimit: 6,
                      callback: function (value, index, values) {
                        if (value >= 1000) {
                          return '$' + (value / 1000).toFixed(1) + 'K';
                        } else if (value >= 1) {
                          return '$' + value.toFixed(0);
                        } else if (value > 0) {
                          return '$' + value.toFixed(2);
                        } else {
                          return '$0.00';
                        }
                      },
                    },
                  },
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
                      family:
                        '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif',
                    },
                    bodyFont: {
                      size: 12,
                      weight: 'normal',
                      family:
                        '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif',
                    },
                    padding: {
                      top: 14,
                      right: 18,
                      bottom: 14,
                      left: 18,
                    },
                    titleMarginBottom: 10,
                    bodySpacing: 6,
                    callbacks: {
                      title: function (context) {
                        const datasetLabel =
                          context[0].dataset.label || 'Data Point';
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
                        let formattedPrice;
                        if (price >= 1000) {
                          formattedPrice =
                            '$' + (price / 1000).toFixed(1) + 'K';
                        } else if (price >= 1) {
                          formattedPrice = '$' + price.toFixed(2);
                        } else if (price >= 0.01) {
                          formattedPrice = '$' + price.toFixed(2);
                        } else if (price > 0 && price < 0.01) {
                          formattedPrice = '$' + price.toFixed(4);
                        } else {
                          formattedPrice = '$' + price.toFixed(2);
                        }
                        lines.push(`💰 Price: ${formattedPrice}`);
                        if (
                          label === 'Bid Point Outlier' ||
                          label === 'Bid Point'
                        ) {
                          const lettingDate = point.l || 'N/A';
                          const contract = point.p || 'N/A';
                          lines.push(
                            `📅 Letting Date: ${formatDotNetDate(lettingDate)}`
                          );
                          lines.push(`📄 Contract #: ${contract}`);
                        }
                        if (
                          label.includes('LOESS') ||
                          label.includes('Weighted Avg')
                        ) {
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
                          return [
                            '⚠️ This point is identified as an outlier',
                            'based on statistical analysis',
                          ];
                        } else if (label.includes('Bid Point')) {
                          return [
                            '✅ This is a normal bid point',
                            'within expected statistical range',
                          ];
                        } else if (label.includes('LOESS')) {
                          return [
                            '📈 Smoothed trend line',
                            'showing price patterns',
                          ];
                        } else if (label.includes('Weighted Avg')) {
                          return [
                            '📊 Statistical average',
                            'weighted by quantity',
                          ];
                        } else if (label.includes('95% CI')) {
                          return [
                            '📉 Confidence interval',
                            'showing statistical uncertainty',
                          ];
                        }
                        return [];
                      },
                    },
                  },
                  legend: {
                    display: true,
                  },
                },
              },
            });
            $scope.chartStats = {
              avg: weightedAvg,
              weightedAvgNoOutliers: $scope.weightedAvgNoOutliers,
              totalContracts: new Set(
                $scope.bidHistoryData.map((item) => item.p)
              ).size,
              totalBidAmount: $scope.bidHistoryData.reduce(
                (sum, item) => sum + (item.PvBidTotal || 0),
                0
              ),
              totalQuantity: $scope.bidHistoryData.reduce(
                (sum, item) => sum + (item.Quantity || 0),
                0
              ),
              count: $scope.bidHistoryData.length,
              avgQty:
                $scope.bidHistoryData.reduce(
                  (sum, item) => sum + (item.Quantity || 0),
                  0
                ) / $scope.bidHistoryData.length,
              outlierCount: $scope.bidHistoryData.filter(
                (item) => item.IsOutlier
              ).length,
              avgCurrentPrice:
                $scope.bidHistoryData.reduce(
                  (sum, item) => sum + ($scope.getPriceField(item) || 0),
                  0
                ) / $scope.bidHistoryData.length,
              avgInflationAdjustedPrice:
                $scope.bidHistoryData.reduce(
                  (sum, item) =>
                    sum + (item.InflationAdjustedPrice || item.b || 0),
                  0
                ) / $scope.bidHistoryData.length,
              maxInflationIncrease: Math.max(
                ...$scope.bidHistoryData.map(
                  (item) => item.InflationPercentIncrease || 0
                )
              ),
              minInflationIncrease: Math.min(
                ...$scope.bidHistoryData.map(
                  (item) => item.InflationPercentIncrease || 0
                )
              ),
              avgInflationIncrease:
                $scope.bidHistoryData.reduce(
                  (sum, item) => sum + (item.InflationPercentIncrease || 0),
                  0
                ) / $scope.bidHistoryData.length,
              currentPriceField: $scope.useInflationAdjustedPrices
                ? 'Inflation-Adjusted'
                : 'Raw',
            };
            $scope.isChartLoading = false;
            $scope.isChartStale = false;
            $scope.$apply();
          });
        } else {
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
      const validData = $scope.bidHistoryData.filter(
        (item) => item.l && item.Quantity && $scope.getPriceField(item)
      );
      if (validData.length === 0) {
        return [];
      }
      const groupedData = {};
      validData.forEach((item) => {
        const lettingDate = new Date(
          parseInt(item.l.replace(/\/Date\((\d+)\)\//, '$1'))
        );
        let timeKey;
        switch ($scope.trendAnalysisData.trendTimeGrouping) {
          case 'year':
            timeKey = lettingDate.getFullYear();
            break;
          case 'quarter':
            const quarter = Math.floor(lettingDate.getMonth() / 3) + 1;
            timeKey = `${lettingDate.getFullYear()}-Q${quarter}`;
            break;
          case 'month':
            timeKey = `${lettingDate.getFullYear()}-${String(
              lettingDate.getMonth() + 1
            ).padStart(2, '0')}`;
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
            count: 0,
            uniqueContracts: new Set(),
          };
        }
        groupedData[timeKey].quantities.push(item.Quantity);
        groupedData[timeKey].prices.push($scope.getPriceField(item));
        groupedData[timeKey].totalQuantity += item.Quantity;
        groupedData[timeKey].totalAmount += item.PvBidTotal || 0;
        groupedData[timeKey].count++;
        groupedData[timeKey].uniqueContracts.add(item.p);
      });
      const trendData = Object.keys(groupedData).map((timeKey) => {
        const data = groupedData[timeKey];
        const weightedAvg =
          data.totalQuantity > 0
            ? data.quantities.reduce(
                (sum, qty, idx) => sum + qty * data.prices[idx],
                0
              ) / data.totalQuantity
            : 0;
        return {
          timeKey: timeKey,
          weightedAvg: weightedAvg,
          totalQuantity: data.totalQuantity,
          totalAmount: data.totalAmount,
          count: data.count,
          uniqueContractCount: data.uniqueContracts.size,
          date: parseTimeKeyToDate(timeKey),
        };
      });
      trendData.sort((a, b) => a.date - b.date);
      const sparseIntervals = trendData.filter((item) => item.uniqueContractCount < 5);
      if (sparseIntervals.length > 0) {
        const timeUnit =
          $scope.trendAnalysisData.trendTimeGrouping === 'year'
            ? 'years'
            : $scope.trendAnalysisData.trendTimeGrouping === 'quarter'
            ? 'quarters'
            : 'months';
        $scope.trendWarning = `Warning: Some of your time intervals (${timeUnit}) include fewer than 5 contracts, which may affect the accuracy of the price trend. `;
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
        const monthNames = [
          'Jan',
          'Feb',
          'Mar',
          'Apr',
          'May',
          'Jun',
          'Jul',
          'Aug',
          'Sep',
          'Oct',
          'Nov',
          'Dec',
        ];
        return `${monthNames[parseInt(month) - 1]} ${year}`;
      } else {
        return timeKey;
      }
    }
    
    // Expose formatTimeKey to scope for use in template
    $scope.formatTimeKey = formatTimeKey;
    function renderTrendChart() {
      $scope.isTrendChartLoading = true;

      $timeout(function () {
        const canvas = document.getElementById('trendChart');
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
        const chartData = $scope.trendData.map((item) => ({
          x: item.timeKey,
          y: item.weightedAvg,
          totalQuantity: item.totalQuantity,
          totalAmount: item.totalAmount,
          count: item.count,
        }));
        const labels = chartData.map((item) => formatTimeKey(item.x));
        if (typeof Chart === 'undefined') {
          console.error('Chart.js is not loaded');
          $scope.isTrendChartLoading = false;
          return;
        }
        $scope.trendChartInstance = new Chart(newCtx, {
          type: 'line',
          data: {
            labels: labels,
            datasets: [
              {
                label:
                  'Weighted Average Unit Price (' +
                  ($scope.useInflationAdjustedPrices
                    ? 'Inflation-Adjusted'
                    : 'Raw') +
                  ')',
                data: chartData.map((item) => item.y),
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
                pointHoverBorderColor: '#ffffff',
              },
            ],
          },
          options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
              x: {
                title: {
                  display: true,
                  text:
                    $scope.trendAnalysisData.trendTimeGrouping === 'year'
                      ? 'Year'
                      : $scope.trendAnalysisData.trendTimeGrouping === 'quarter'
                      ? 'Quarter'
                      : 'Month',
                  font: { size: 14, weight: 'bold' },
                },
                grid: {
                  display: true,
                  color: 'rgba(0,0,0,0.1)',
                },
              },
              y: {
                title: {
                  display: true,
                  text:
                    'Weighted Average Unit Price (' +
                    ($scope.useInflationAdjustedPrices
                      ? 'Inflation-Adjusted'
                      : 'Raw') +
                    ') ($)',
                  font: { size: 14, weight: 'bold' },
                },
                grid: {
                  display: true,
                  color: 'rgba(0,0,0,0.1)',
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
                  },
                },
              },
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
                },
                bodyFont: {
                  size: 12,
                  weight: 'normal',
                },
                padding: {
                  top: 14,
                  right: 18,
                  bottom: 14,
                  left: 18,
                },
                callbacks: {
                  title: function (context) {
                    return '📈 Price Trend Analysis';
                  },
                  label: function (context) {
                    const dataPoint = $scope.trendData[context.dataIndex];
                    const lines = [];
                    lines.push(
                      `📅 Period: ${formatTimeKey(dataPoint.timeKey)}`
                    );
                    lines.push(
                      `💰 Weighted Avg Price: $${dataPoint.weightedAvg.toLocaleString(
                        undefined,
                        { minimumFractionDigits: 2, maximumFractionDigits: 2 }
                      )}`
                    );
                    lines.push(
                      `📊 Total Quantity: ${dataPoint.totalQuantity.toLocaleString()}`
                    );
                    lines.push(
                      `💵 Total Amount: $${dataPoint.totalAmount.toLocaleString()}`
                    );
                      lines.push(`📄 Number of Contracts: ${dataPoint.uniqueContractCount}`);
                    lines.push(`📋 Number of Bids: ${dataPoint.count}`);
                   
                    return lines;
                  },
                },
              },
              legend: {
                display: false,
              },
            },
          },
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
      return code ? $scope.bidTypeMap[code] || 'Unknown' : 'Unknown';
    };

    $scope.getBidStatusLabel = function (code) {
      return code ? $scope.bidStatusMap[code] || 'Unknown' : 'Unknown';
    };

    $scope.getInflationInfo = function (item) {
      if (!item.InflationAdjustedPrice || !item.InflationPercentIncrease) {
        return 'No inflation data available';
      }
      return `Adjusted to 2024 Q4 (${item.InflationPercentIncrease.toFixed(
        1
      )}% )`;
    };
    $scope.calculateCustomQuantityStats = function () {
      if (
        !$scope.customQuantityData.userQuantity ||
        $scope.customQuantityData.userQuantity <= 0 ||
        !$scope.bidHistoryData ||
        $scope.bidHistoryData.length === 0
      ) {
        $scope.customQuantityPrediction = null;
        return;
      }
      $scope.isCalculatingPrediction = true;
      try {
        const quantities = $scope.bidHistoryData.map(
          (item) => item.Quantity || 0
        );
        const prices = $scope.bidHistoryData.map(
          (item) => $scope.getPriceField(item) || 0
        );
        const validData = quantities
          .map((qty, i) => ({ qty, price: prices[i] }))
          .filter(
            (item) =>
              item.qty > 0 &&
              item.price > 0 &&
              isFinite(item.qty) &&
              isFinite(item.price)
          );
        if (validData.length < 3) {
          $scope.customQuantityPrediction = {
            success: false,
            message:
              'Insufficient data points for LOESS prediction (need at least 3 valid data points)',
          };
          return;
        }
        const x = validData.map((item) => item.qty);
        const y = validData.map((item) => item.price);
        const totalQty = x.reduce((sum, q) => sum + q, 0);
        const weightedAvg =
          x.reduce((sum, q, i) => sum + q * y[i], 0) / totalQty;
        const weightedStdDev = Math.sqrt(
          x.reduce(
            (sum, q, i) => sum + q * Math.pow(y[i] - weightedAvg, 2),
            0
          ) / totalQty
        );
        const cleanData = validData.filter(
          (item) => Math.abs(item.price - weightedAvg) <= weightedStdDev
        );
        const cleanTotalQty = cleanData.reduce(
          (sum, item) => sum + item.qty,
          0
        );
        const weightedAvgNoOutliers =
          cleanTotalQty > 0
            ? cleanData.reduce((sum, item) => sum + item.qty * item.price, 0) /
              cleanTotalQty
            : 0;
        const adaptiveBandwidthUnfiltered = getCachedBandwidth(
          x,
          y,
          false,
          $scope.customQuantityData.userQuantity
        );
        const adaptiveBandwidthFiltered = getCachedBandwidth(
          cleanData.map((item) => item.qty),
          cleanData.map((item) => item.price),
          true,
          $scope.customQuantityData.userQuantity
        );
        const loessUnfiltered = loessSmooth(x, y, adaptiveBandwidthUnfiltered, [
          $scope.customQuantityData.userQuantity,
        ]);
        const prediction = loessUnfiltered;
        const loessFiltered = loessSmooth(
          cleanData.map((item) => item.qty),
          cleanData.map((item) => item.price),
          adaptiveBandwidthFiltered,
          [$scope.customQuantityData.userQuantity]
        );
        const ciUnfiltered = bootstrapCI(
          x,
          y,
          [$scope.customQuantityData.userQuantity],
          adaptiveBandwidthUnfiltered,
          200
        );
        const ciFiltered = bootstrapCI(
          cleanData.map((item) => item.qty),
          cleanData.map((item) => item.price),
          [$scope.customQuantityData.userQuantity],
          adaptiveBandwidthFiltered,
          200
        );
        if (
          prediction &&
          prediction.length > 0 &&
          isFinite(prediction[0]) &&
          prediction[0] > 0
        ) {
          const predictedPrice = prediction[0];
          const totalCost =
            $scope.customQuantityData.userQuantity * predictedPrice;
          const loessUnfilteredValue = loessUnfiltered[0] || 0;
          const loessFilteredValue = loessFiltered[0] || 0;
          const hasCounterintuitiveResult =
            loessFilteredValue > loessUnfilteredValue;
          let counterintuitiveExplanation = '';
          if (hasCounterintuitiveResult) {
            const userQty = $scope.customQuantityData.userQuantity;
            const filteredQuantities = cleanData.map((item) => item.qty);
            const minFiltered = Math.min(...filteredQuantities);
            const maxFiltered = Math.max(...filteredQuantities);
            if (userQty < minFiltered || userQty > maxFiltered) {
              counterintuitiveExplanation =
                'This result occurs because your quantity is outside the range of non-outlier data, causing different extrapolation behavior.';
            } else {
              const difference = loessFilteredValue - loessUnfilteredValue;
              const percentChange = (
                (difference / loessUnfilteredValue) *
                100
              ).toFixed(1);
              counterintuitiveExplanation = `This result occurs because removing outliers changed the local data patterns around your quantity (${userQty.toLocaleString()}). The LOESS algorithm detected a different local trend, resulting in a ${percentChange}% change in prediction. This is normal for local regression methods when data distribution changes.`;
            }
          }
          $scope.customQuantityPrediction = {
            success: true,
            userQuantity: $scope.customQuantityData.userQuantity,
            predictedUnitPrice: parseFloat(predictedPrice.toFixed(4)),
            totalCost: parseFloat(totalCost.toFixed(2)),
            priceType: $scope.useInflationAdjustedPrices
              ? 'Inflation-Adjusted'
              : 'Raw',
            loessUnfiltered: parseFloat(loessUnfilteredValue.toFixed(4)),
            loessFiltered: parseFloat(loessFilteredValue.toFixed(4)),
            weightedAvg: parseFloat(weightedAvg.toFixed(4)),
            weightedAvgNoOutliers: parseFloat(weightedAvgNoOutliers.toFixed(4)),
            dataPoints: validData.length,
            dataPointsNoOutliers: cleanData.length,
            loessUnfilteredTotal: parseFloat(
              (
                loessUnfilteredValue * $scope.customQuantityData.userQuantity
              ).toFixed(2)
            ),
            loessFilteredTotal: parseFloat(
              (
                loessFilteredValue * $scope.customQuantityData.userQuantity
              ).toFixed(2)
            ),
            weightedAvgTotal: parseFloat(
              (weightedAvg * $scope.customQuantityData.userQuantity).toFixed(2)
            ),
            weightedAvgNoOutliersTotal: parseFloat(
              (
                weightedAvgNoOutliers * $scope.customQuantityData.userQuantity
              ).toFixed(2)
            ),
            loessUnfilteredLower: ciUnfiltered.lower[0]
              ? parseFloat(ciUnfiltered.lower[0].toFixed(4))
              : 0,
            loessUnfilteredUpper: ciUnfiltered.upper[0]
              ? parseFloat(ciUnfiltered.upper[0].toFixed(4))
              : 0,
            loessFilteredLower: ciFiltered.lower[0]
              ? parseFloat(ciFiltered.lower[0].toFixed(4))
              : 0,
            loessFilteredUpper: ciFiltered.upper[0]
              ? parseFloat(ciFiltered.upper[0].toFixed(4))
              : 0,
            hasCounterintuitiveResult: hasCounterintuitiveResult,
            counterintuitiveExplanation: counterintuitiveExplanation,
          };
        } else {
          $scope.customQuantityPrediction = {
            success: false,
            message:
              'Unable to generate prediction for this quantity. Try a different value.',
          };
        }
      } catch (error) {
        console.error('Error calculating LOESS prediction:', error);
        $scope.customQuantityPrediction = {
          success: false,
          message: 'Error calculating prediction: ' + error.message,
        };
      } finally {
        $scope.isCalculatingPrediction = false;
      }
    };
    $scope.isUserQuantityInRange = function () {
      if (!$scope.customQuantityData.userQuantity) {
        return true; 
      }
      
      if ($scope.customQuantityData.userQuantity <= 0) {
        return false; 
      }
      
      const range = $scope.getValidQuantityRange();
      const userQty = parseFloat($scope.customQuantityData.userQuantity);
      
      return userQty >= range.min && userQty <= range.max;
    };
    // Get Valid Quantity range  bid history data
    $scope.getValidQuantityRange = function () {
      if (!$scope.bidHistoryData || $scope.bidHistoryData.length === 0) {
        return { min: 0, max: 0 };
      }
      const quantities = $scope.bidHistoryData
        .map((item) => item.Quantity || 0)
        .filter((qty) => qty > 0);
      if (quantities.length === 0) {
        return { min: 0, max: 0 };
      }
      const minQuantity = Math.min(...quantities);
      const maxQuantity = Math.max(...quantities);
      return {
        min: minQuantity,
        max: maxQuantity,
      };
    };

    $scope.$on('$destroy', function () {
      $rootScope.showStatisticsDetails = false;
    });
  },
]);
angular
  .module('dqeControllers')
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
      } else if (value >= 0.01 && value < 1) {
        return '$' + value.toFixed(2);
      } else if (value >= 1) {
        return (
          '$' +
          value.toLocaleString('en-US', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2,
          })
        );
      } else {
        return '$' + value.toFixed(2);
      }
    };
  });
