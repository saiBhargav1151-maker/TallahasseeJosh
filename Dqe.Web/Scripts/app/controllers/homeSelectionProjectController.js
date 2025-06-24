dqeControllers.controller('HomeSelectionProjectController', ['$scope', '$rootScope', '$http', '$route', 'stateService', function ($scope, $rootScope, $http, $route, stateService) {
    $rootScope.$broadcast('initializeNavigation');
    function processResult(result) {
        if (!containsDqeError(result)) {
            var r = getDqeData(result);
            stateService.currentEstimateId = r.workingEstimate == undefined ? 0 : r.workingEstimate.projectSnapshotId;
            $scope.security = r.security;
            $scope.project = r.project;
            $scope.proposals = r.proposals;
            $scope.workingEstimate = r.workingEstimate;
            $scope.versions = r.versions;
            $scope.hasReviews = r.versions.some(item => item.versionLabel === 'Review');
            $scope.authorizedUsers = r.authorizedUsers;
            document.getElementById("hiddenProjectNumber").value = r.project.number;
        }
    }
    $scope.isSynced = null;
    function checkSync(result) {
        if (!containsDqeError(result)) {
            $scope.isSynced = null;
            if ($scope.project == undefined || $scope.project.id == undefined) return;
            $http.get('./projectproposal/IsProjectSynced', { params: { projectId: $scope.project.id } }).success(function (res) {
                if (!containsDqeError(res)) {
                    var r = getDqeData(res);
                    $scope.isSynced = r.isSynced;
                }
            });
        }
    }
    function getRecentProject() {
        $http.get('./projectproposal/GetRecentProject').success(function (result) {
            processResult(result);
            checkSync(result);
        });
    }
    if ($route.current.params != 'undefined' && $route.current.params != null) {
        if ($route.current.params.project != 'undefined' && $route.current.params.project != null) {
            $http.get('./projectproposal/GetProject', { params: { number: $route.current.params.project } }).success(function (result) {
                stateService.currentProject = $route.current.params.project;
                processResult(result);
                checkSync(result);
            });
        } else {
            if (stateService.currentEstimateId != 0) getRecentProject();
        }
    }
    $scope.editComment = function (snapshot) {
        snapshot.isEditingComment = true;
        snapshot.currentComment = snapshot.comment;
    }
    $scope.cancelEditComment = function(snapshot) {
        snapshot.isEditingComment = false;
        snapshot.comment = snapshot.currentComment;
    }
    $scope.saveComment = function(snapshot) {
        $http.post('./projectproposal/SaveComment', snapshot).success(function (result) {
            if (!containsDqeError(result)) {
                snapshot.isEditingComment = false;
            }
        });
    }
    $scope.deleteProject = function(project) {
        $http.post('./projectproposal/DeleteProject', project).success(function (result) {
            if (!containsDqeError(result)) {
                $scope.security = undefined;
                $scope.project = undefined;
                $scope.proposals = undefined;
                $scope.workingEstimate = undefined;
                $scope.versions = undefined;
                $scope.authorizedUsers = undefined;
            }
        });
    }
    $scope.deleteSnapshot = function (project) {
        $http.post('./projectproposal/DeleteProjectSnapshot', project).success(function (result) {
            if (!containsDqeError(result)) {
                processResult(result);
            }
        });
    }
    $scope.synchronizeWorkingEstimate = function (estimate, project) {
        $http.post('./projectproposal/SyncWorkingEstimate', estimate).success(function (result) {
            //processResult(result);
            //getRecentProject();
            if (!containsDqeError(result)) {
                $http.get('./projectproposal/GetProject', { params: { number: project.number } }).success(function (res) {
                    processResult(res);
                    checkSync(result);
                });
            }
        });
    }
    $scope.loadProject = function () {
        $http.get('./projectproposal/GetProject', { params: { number: $scope.selectedProject.number } }).success(function (result) {
            stateService.currentProject = $scope.selectedProject.number;
            processResult(result);
        });
    };
    $scope.createProjectVersionFromWt = function (project) {
        $http.post('./projectproposal/CreateProjectVersionFromWt', project).success(function (result) {
            processResult(result);
            checkSync(result);
        });
    }
    $scope.createProjectVersionFromLre = function (project) {
        alert("Not Implemented");
    }
    $scope.releaseCustody = function (project) {
        $http.post('./projectproposal/ReleaseCustody', project).success(function (result) {
            processResult(result);
        });
    }
    $scope.aquireCustody = function (project) {
        $http.post('./projectproposal/AquireCustody', project).success(function (result) {
            processResult(result);
        });
    }
    $scope.snapshotWorkingEstimate = function (project) {
        $http.post('./projectproposal/SnapshotWorkingEstimate', project).success(function (result) {
            processResult(result);
        });
    }
    $scope.assignWorkingEstimate = function (version) {
        $http.post('./projectproposal/AssignWorkingEstimate', version).success(function (result) {
            processResult(result);
            checkSync(result);
        });
    }
    $scope.createProjectVersionFromEstimate = function (snapshot) {
        $http.post('./projectproposal/CreateProjectVersionFromEstimate', snapshot).success(function (result) {
            processResult(result);
        });
    }
    $scope.createNewReviewSnapshot = function (snapshot) {
        $http.post('./projectproposal/CreateReviewProjectVersionFromEstimate', snapshot).success(function (result) {
            processResult(result);
        });
    }
    //$scope.saveComment = function (project) {
    //    $http.post('./projectproposal/SaveComment', project).success(function (result) {
    //        processResult(result);
    //    });
    //}
    $scope.getPotentialAuthorizedUsers = function (val) {
        return $http.get('./staff/GetDqeStaffByName', { params: { id: val } })
            .then(function (response) {
                var users = [];
                angular.forEach(response.data, function (item) {
                    users.push(item);
                });
                return users;
            });
    }
    $scope.authorizeUser = function () {
        if ($scope.selectedUser == undefined) return;
        $scope.selectedUser.project = $scope.project;
        $http.post('./projectproposal/AuthorizeUser', $scope.selectedUser).success(function (result) {
            if (!containsDqeError(result)) {
                var r = getDqeData(result);
                $scope.authorizedUsers = r;
            }
            $scope.selectedUser = '';
        });
    }
    $scope.removeAuthorization = function (authorizedUser) {
        authorizedUser.project = $scope.project;
        $http.post('./projectproposal/DeauthorizeUser', authorizedUser).success(function (result) {
            if (!containsDqeError(result)) {
                var r = getDqeData(result);
                $scope.authorizedUsers = r;
            }
        });
    }
    $scope.getProjects = function (val) {
        return $http.get('./projectproposal/GetProjects', { params: { number: val } })
            .then(function (response) {
                var projects = [];
                angular.forEach(response.data, function (item) {
                    projects.push(item);
                });
                return projects;
            });
    };

    $scope.reportFormat = {
        type: "PDF"
    };

    $scope.updateSelection = function (versions) {
        var estimateIds = new Array();
        angular.forEach(versions, function (version) {
            angular.forEach(version.snapshots, function (snapshot) {
                if (snapshot.selected) {
                    estimateIds.push(snapshot.projectSnapshotId);
                }
            });
        });

        $scope.disableReportButton = estimateIds.length > 0 ? false : true;

        if (estimateIds.length === 2) {
            angular.forEach(versions, function (version) {
                angular.forEach(version.snapshots, function (snapshot) {
                    if (!snapshot.selected) {
                        snapshot.disabled = true;
                    } else {
                        snapshot.disabled = false;
                    }
                });
            });
        } else {
            angular.forEach(versions, function (version) {
                angular.forEach(version.snapshots, function (snapshot) {
                    snapshot.disabled = false;
                });
            });
        }
        document.getElementById("hiddenProjectSnapshotIds").value = estimateIds.join(",");
    };

    $scope.disableReportButton = true;

    $scope.viewProjectItemsReport = function () {
        $.download('./report/ViewProjectItemsReport', $('form#ViewProjectItemsReport').serialize());
    };

    $scope.viewScopeTrackingGraph = function () {
        $.download('./report/ViewScopeTrackingGraph', $('form#ViewScopeTrackingGraph').serialize());
    };

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