dqeControllers.controller('AdminPayItemsMaintainController', ['$scope', '$rootScope', '$http', '$filter', function ($scope, $rootScope, $http, $filter) {
    $rootScope.$broadcast('initializeNavigation');
    $scope.showCurrentItems = true;
    $scope.showCurrentStructures = true;

    $http.get('./PayItemStructureAdministration/GetUnitCodes').success(function (result) {
        if (!containsDqeError(result)) {
            var units = getDqeData(result);
            $scope.primaryUnits = units.slice();
            $scope.hybridUnits = units.slice();
        }
    });
    $http.get('./PayItemStructureAdministration/GetItemTypeCodes').success(function (result) {
        if (!containsDqeError(result)) {
            $scope.itemTypes = getDqeData(result);
            $scope.itemTypes.splice(0, 0, {
                name: "",
                description: "Not Selected"
            });
        }
    });
    $http.get('./PayItemStructureAdministration/GetItemClassCodes').success(function (result) {
        if (!containsDqeError(result)) {
            $scope.itemClassifications = getDqeData(result);
            $scope.itemClassifications.splice(0, 0, {
                name: "",
                description: "Not Selected"
            });
        }
    });
    $http.get('./PayItemStructureAdministration/GetContractClassCodes').success(function (result) {
        if (!containsDqeError(result)) {
            $scope.contractClassifications = getDqeData(result);
            $scope.contractClassifications.splice(0, 0, {
                name: "",
                description: "Not Selected"
            });
        }
    });
    $http.get('./PayItemStructureAdministration/GetFuelAdjustmentTypeCodes').success(function (result) {
        if (!containsDqeError(result)) {
            $scope.fuelTypes = getDqeData(result);
            $scope.fuelTypes.splice(0, 0, {
                name: "",
                description: "Not Selected"
            });
        }
    });
    $http.get('./PayItemStructureAdministration/GetPrecisionCodes').success(function (result) {
        if (!containsDqeError(result)) {
            $scope.precisionCodes = getDqeData(result);
            $scope.precisionCodes.splice(0, 0, {
                name: "",
                description: "Not Selected"
            });
        }
    });
    $http.get('./PayItemStructureAdministration/GetCostBasedTemplates').success(function (result) {
        if (!containsDqeError(result)) {
            $scope.costBasedTemplates = getDqeData(result);
            $scope.costBasedTemplates.splice(0, 0, {
                name: "Not Selected",
                id: 0
            });
        }
    });
    $http.get('./PayItemStructureAdministration/GetSpecBooks').success(function (result) {
        if (!containsDqeError(result)) {
            $scope.specBooks = getDqeData(result);
            $http.get('./PayItemStructureAdministration/GetCurrentSpecBook').success(function (res) {
                if (!containsDqeError(res)) {
                    var currentSpecBook = getDqeData(res);
                    $scope.specBook = {
                        id: currentSpecBook.id,
                        current: currentSpecBook.specBook
                    }
                    document.getElementById("hiddenSpecBookId").value = currentSpecBook.id;
                }
            });
        }
    });
    $scope.listItems = function ($event, set) {
        $scope.holdEvent = $event;
        $scope.holdSet = set;
        $http.get('./PayItemStructureAdministration/GetStructuresRange', { params: { set: set, currentStructuresOnly: $scope.showCurrentStructures } }).success(function (result) {
            if (!containsDqeError(result)) {
                $scope.structures = getDqeData(result);
                $scope.filter = {
                    name: '',
                    title: '',
                    unit: ''
                };
                $scope.filterItems();
                // JWW 02/25/25 - Title attribute of link is truncated and assigned to variable to be displayed on admin_payitems_maintain page as title above grid
                $scope.boeChapterTitle = $event.currentTarget.title.substring(25);
            }
        });
    }
    $scope.toggleStructureDetail = function (structure) {
        if (structure.showDetail) {
            structure.showDetail = false;
            return;
        }
        $http.get('./PayItemStructureAdministration/GetProtectedStructureDetail', { params: { structureId: structure.id } }).success(function (result) {
            if (!containsDqeError(result)) {
                structure = getDqeData(result);
                //bug fix for null replacement items
                if (structure.replacementItems == null || structure.replacementItems == undefined) {
                    structure.replacementItems = '';
                }
                $scope.getItemsForStructure(structure);
                for (var i = 0; i < $scope.filteredItems.length; i++) {
                    if ($scope.filteredItems[i].id == structure.id) {
                        $scope.filteredItems[i] = structure;
                        structure.showDetail = true;
                        break;
                    }
                }
            }
        });
    }
    $scope.addNewStructure = function () {
        $scope.newStructure =
        {
            showDetail: true,
            id: 0,
            name: '',
            title: '',
            unit: '',
            accuracy: '',
            planQuantity: '',
            notes: '',
            isObsolete: false,
            details: '',
            planSummary: '',
            designForms: '',
            constructionForms: '',
            ppmChapterText: '',
            trnsportText: '',
            otherText: '',
            standardsText: '',
            specificationsText: '',
            structureDetails: '',
            boeRecentChangeDate: '',
            boeRecentChangeDescription: '',
            essHistory: '',
            pendingInformation: '',
            ppmChapters: [],
            prepAndDocChapters: [],
            otherReferences: [],
            standards: [],
            specifications: [],
            requiredItems: '',
            recommendedItems: '',
            replacementItems: '',
            srsId: 0
        };
        $scope.showNewStructure = true;
    }
    $scope.cancelAddNewStructure = function () {
        $scope.showNewStructure = false;
    }
    $scope.addStructureToList = function (structure) {
        $scope.resetList();
    }
    $scope.removeStructureFromList = function (structure) {
        var index = 0;
        for (var i = 0; i < $scope.structures.length; i++) {
            if (structure.id == $scope.structures[i].id) {
                index = i;
                break;
            }
        }
        $scope.structures.splice(index, 1);
        $scope.filterItems();
    }
    $scope.filterStructure = function (structureNumber) {
        $scope.filter.name = structureNumber;
        $scope.filterItems();
    }
    $scope.filterItems = function () {
        $scope.currentPage = 0;
        $scope.filteredItems = $filter('filter')($scope.structures,
        {
            name: $scope.filter.name,
            title: $scope.filter.title,
            unit: $scope.filter.unit
        });
    }
    $scope.clearAllFilters = function () {
        $scope.filteredItems = $filter('filter')($scope.structures,
        {
            name: '',
            title: '',
            unit: ''
        });
        $scope.filter.name = '';
        $scope.filter.title = '';
        $scope.filter.unit = '';
        $scope.filterItems();
    }
    $scope.getItemsForStructure = function (structure) {
        $http.get('./PayItemStructureAdministration/GetItemHeadersForStructure', { params: { structureId: structure.id, currentItemsOnly: $scope.showCurrentItems } }).success(function (result) {
            if (!containsDqeError(result)) {
                structure.items = getDqeData(result);
                var unit = '';
                if (structure.items != undefined) {
                    for (var i = 0; i < structure.items.length; i++) {
                        if (structure.items[i].isObsolete) continue;
                        if (unit == '') {
                            unit = structure.items[i].unit;
                        } else if (unit != structure.items[i].unit) {
                            unit = "MIXED";
                            break;
                        }
                    }
                }
                structure.unit = unit;
            }
        });
    }
    $scope.resetStructures = function () {
        for (var i = 0; i < $scope.filteredItems.length; i++) {
            $scope.filteredItems[i].showDetail = false;
        }
    }
    $scope.resetList = function () {
        $scope.listItems($scope.holdEvent, $scope.holdSet);
    }

    $scope.downloadStructures = function () {
        $.download('./report/DownloadStructureData', $('form#ViewStructureData').serialize());
    }

    $scope.downloadPayItems = function () {
        $.download('./report/DownloadPayItemData', $('form#ViewStructureData').serialize());
    }

    $scope.changeSpecId = function (item) {
        angular.forEach($scope.specBooks, function (specBook) {
            if (specBook.name === item.current) {
                document.getElementById("hiddenSpecBookId").value = specBook.id;
            }
        });
    }

    jQuery.download = function (url, data, method) {
        //url and data options required
        if (url && data) {
            //data can be string of parameters or array/object
            data = typeof data == 'string' ? data : jQuery.param(data);
            //split params into form inputs
            var inputs = '';
            jQuery.each(data.split('&'), function () {
                var pair = this.split('=');
                inputs += '<input type="hidden" name="' + pair[0] + '" value="' + pair[1] + '" />';
            });
            //send request
            jQuery('<form action="' + url + '" method="' + (method || 'post') + '">' + inputs + '</form>')
            .appendTo('body').submit().remove();
        };
    };
}]);