dqeControllers.controller('AdminPayItemsMaintainController', ['$scope', '$rootScope', '$http', function ($scope, $rootScope, $http) {
    $rootScope.$broadcast('initializeNavigation');
    $scope.showContent = function(payItemStructureGroup) {
        if (payItemStructureGroup.showList) {
            payItemStructureGroup.showList = false;
        } else {
            payItemStructureGroup.showList = true;
        }
    };
    $scope.payItemStructureGroups = [
        {
            heading: '000-099 Items',
            panel: 0,
            isOpen: true,
            payItemStructures: [],
            showList: true,
            prefix: '00'
        },
        {
            heading: '100-199 Items',
            panel: 1,
            isOpen: false,
            payItemStructures: [],
            showList: true,
            prefix: '01'
        },
        {
            heading: '200-299 Items',
            panel: 2,
            isOpen: false,
            payItemStructures: [],
            showList: true,
            prefix: '02'
        },
        {
            heading: '300-399 Items',
            panel: 3,
            isOpen: false,
            payItemStructures: [],
            showList: true,
            prefix: '03'
        },
        {
            heading: '400-499 Items',
            panel: 4,
            isOpen: false,
            payItemStructures: [],
            showList: true,
            prefix: '04'
        },
        {
            heading: '500-599 Items',
            panel: 5,
            isOpen: false,
            payItemStructures: [],
            showList: true,
            prefix: '05'
        },
        {
            heading: '600-699 Items',
            panel: 6,
            isOpen: false,
            payItemStructures: [],
            showList: true,
            prefix: '06'
        },
        {
            heading: '700-799 Items',
            panel: 7,
            isOpen: false,
            payItemStructures: [],
            showList: true,
            prefix: '07'
        },
        {
            heading: '800-899 Items',
            panel: 8,
            isOpen: false,
            payItemStructures: [],
            showList: true,
            prefix: '08'
        },
        {
            heading: '900-999 Items',
            panel: 9,
            isOpen: false,
            payItemStructures: [],
            showList: true,
            prefix: '09'
        },
        {
            heading: '1000-9999 Items',
            panel: 10,
            isOpen: false,
            payItemStructures: [],
            showList: true,
            prefix: '10'
        }
    ];
    $scope.$watch('payItemStructureGroups', function (newVal, oldVal) {
        if (newVal != undefined && oldVal != undefined) {
            for (var i = 0; i < newVal.length; i++) {
                if (newVal[i].isOpen && !oldVal[i].isOpen) {
                    loadPayItemGroups(newVal[i]);
                } else if (newVal[i].showList && !oldVal[i].showList) {
                    loadPayItemGroups(newVal[i]);
                }
            }
        }
    }, true);
    function loadPayItemGroups(payItemStructureGroup) {
        $http.get('./payitemstructureadministration/GetPayItemStructures', {params: { panel: payItemStructureGroup.panel }}).success(function (result) {
            if (!containsDqeError(result)) {
                payItemStructureGroup.payItemStructures = getDqeData(result);
            }
        });
    }
}]);