dqeControllers.controller('AdminPayItemsStructureController', ['$scope', '$rootScope', function ($scope, $rootScope) {
    $rootScope.$broadcast('initializeNavigation');
    $scope.newPayItemStructure = {
        id: 0,
        structureId: '',
        title: '',
        effectiveDate: '',
        obsoleteDate: '',
        primaryUnit: 0,
        secondaryUnit: 0,
        accuracy: 0,
        isPlanQuantity: 'False',
        isDoNotBid: 'False',
        isFixedPrice: 'False',
        fixedAmount: 0,
        notes: '',
        details: '',
        essHistory: '',
        boeRecentChangeDate: '',
        boeRecentChangeDescription: '',
        structureDescription: '',
        showSummary: false,
        otherReferences: [],
        prepAndDocChapters: [],
        specifications: [],
        ppmChapters: [],
        standards: []
    };
}]);