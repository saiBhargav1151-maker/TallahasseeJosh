dqeDirectives.directive('adminPayItem', function() {
    return {
        restrict: 'E',
        scope: {
            payItem: '=',
            payItemStructure: '='
        },
        templateUrl: './Views/directives/admin-payitem.html',
        controller: function ($scope, $http) {
            if ($scope.payItem == undefined) {
                $scope.payItem = getNewPayItem();
                setFilteredPayItemId();
            }
            $scope.masterFiles = [];
            $http.get('./masterfileadministration/GetMasterFiles').success(function (result) {
                if (!containsDqeError(result)) {
                    var data = getDqeData(result);
                    if (data.length > 0) {
                        $scope.masterFiles = data;
                    }
                }
            });
            $scope.effectiveOpen = function ($event) {
                $event.preventDefault();
                $event.stopPropagation();
                $scope.effectiveOpened = true;
                $scope.obsoleteOpened = false;
                $scope.boeRecentChangeDateOpened = false;
            };
            $scope.obsoleteOpen = function ($event) {
                $event.preventDefault();
                $event.stopPropagation();
                $scope.obsoleteOpened = true;
                $scope.effectiveOpened = false;
                $scope.boeRecentChangeDateOpened = false;
            };
            $scope.addPayItem = function () {
                var resetPayItem = false;
                if ($scope.payItem.id == 0) {
                    resetPayItem = true;
                }
                $scope.payItem.payItemId = $scope.payItem.prefix + $scope.payItem.filteredPayItemId;
                $scope.payItem.masterFileId = $scope.payItem.masterFile == undefined ? 0 : $scope.payItem.masterFile.id;
                $http.post('./payitemadministration/UpdatePayItem', $scope.payItem).success(function (result) {
                    if (resetPayItem && !containsDqeError(result)) {
                        $scope.payItem = getNewPayItem();
                        setFilteredPayItemId();
                    }
                });
            };
            $scope.toggleDetail = function () {
                if ($scope.payItem.showSummary) {
                    $http.get('./payitemadministration/GetPayItem', { params: { id: $scope.payItem.id } }).success(function(result) {
                        if (!containsDqeError(result)) {
                            $scope.payItem = getDqeData(result);
                            setFilteredPayItemId();
                        }
                    });
                } else {
                    $scope.payItem.showSummary = !$scope.payItem.showSummary;
                }
            };
            function setFilteredPayItemId() {
                var piArr = $scope.payItemStructure.structureId.split('');
                $scope.payItem.prefix = '';
                for (var i = 0; i < piArr.length ; i++) {
                    if (piArr[i] == '0'
                            || piArr[i] == '1'
                            || piArr[i] == '2'
                            || piArr[i] == '3'
                            || piArr[i] == '4'
                            || piArr[i] == '5'
                            || piArr[i] == '6'
                            || piArr[i] == '7'
                            || piArr[i] == '8'
                            || piArr[i] == '9'
                            || piArr[i] == '-'
                    ) {
                        $scope.payItem.prefix += piArr[i];
                    } else {
                        break;
                    }
                }
                $scope.payItem.filteredPayItemId = $scope.payItem.payItemId.replace($scope.payItem.prefix, '');
            }
            function getNewPayItem() {
                return {
                    id: 0,
                    payItemId: '',
                    shortDescription: '',
                    primaryUnit: 0,
                    secondaryUnit: 0,
                    masterFileId: 0,
                    structureId: $scope.payItemStructure.id,
                    description: '',
                    isSupplementalDescriptionRequired: 'False',
                    isLikeItemsCombined: 'False',
                    isFederalFunded: 'False',
                    lreReferencePrice: '',
                    concreteFactor: '',
                    dqeReferencePrice: '',
                    asphaltFactor: '',
                    effectiveDate: '',
                    obsoleteDate: '',
                    factorNotes: '',
                    fuelFactor: '',
                    showSummary: false,
                };
            }
        }
    }
});