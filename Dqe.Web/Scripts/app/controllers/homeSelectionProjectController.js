dqeControllers.controller('HomeSelectionProjectController', ['$scope', '$rootScope', '$http', '$route', 'stateService', function ($scope, $rootScope, $http, $route,stateService) {
 
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
            $scope.hasReviewsInProject = false;

            for (var i = 0; i < $scope.versions.length; i++) {
                if ($scope.versions[i].versionLabel == 'Review') {
                    $scope.hasReviewsInProject = true;
                    break;
                }
            }
            
            
            $scope.authorizedUsers = r.authorizedUsers;
            document.getElementById("hiddenProjectNumber").value = r.project.number;

            //$scope.versionsNonReviews = $scope.versions.find(v => v.versionLabel !== 'Review');
            angular.forEach($scope.versions, function (version) {
                version.lastModified = getLatestEstimateModified(version);
                version.displayOrder = version.projectVersion;
                version.outdated = false;
                if (version.versionLabel == 'Review') {
                    var words = version.source.toString().split(" "); 

                    var versionSrc = words[1];
                    var estimateNumSrc = words[3];
                    version.versionSrc = versionSrc;
                    version.estimateSrc = estimateNumSrc;

                    /* if ($scope.versions[versionSrc].)*/
                    //find the version/est last modified date

                    var srcSnapshot = null;
                    for (var i = 0; i < $scope.versions.length; i++) {
                        if ($scope.versions[i].projectVersion == versionSrc) {
                            var snapshots = $scope.versions[i].snapshots;
                            if (snapshots) {
                                for (var j = 0; j < snapshots.length; j++) {
                                    if (snapshots[j].projectSnapshot == version.estimateSrc) {
                                        srcSnapshot = snapshots[j];
                                        break;
                                    }
                                }
                            }
                            break;
                        }
                    }

                    if (srcSnapshot != null) {
                        srcSnapshot.hasRelatedReview = true;
                        //commented this out as it might not represent accurate data if there are multiple reviews on an individual estimate.MB.
                        //srcSnapshot.relatedReviewVersionNumber = version.projectVersion;
                        const dateA = new Date(formatDotNetDate(version.snapshots[0].lastUpdatedRaw));
                        const dateB = new Date(formatDotNetDate(srcSnapshot.lastUpdatedRaw));

                        //if the version/est last modified date of this review is earlier than that vers/est last modified date 
                        //then add a field to that review version named "outdated"
                        if (dateB.getTime() - dateA.getTime() > 0) {
                            version.outdated = true;
                        }
                        else {
                            version.outdated = false;
                        }
                    }
                }
            });

            var latestRunningModifiedVersion = 0;
            if ($scope.versions[0]) {
                $scope.versions[0].displayOrder = 999;
                for (let i = 1; i < ($scope.versions.length - 1); i++) {
                    if ($scope.versions[i].versionLabel !== 'Review') {
                        const dateA = new Date($scope.versions[latestRunningModifiedVersion].lastModified);
                        const dateB = new Date($scope.versions[i].lastModified);

                        if (dateB.getTime() - dateA.getTime() > 0) {
                            $scope.versions[i].displayOrder = 999;
                            $scope.versions[latestRunningModifiedVersion].displayOrder = $scope.versions[latestRunningModifiedVersion].projectVersion;
                            latestRunningModifiedVersion = i;
                        }
                    }
                }
            }

        }
    }


    function formatDotNetDate(msDateString) {
        if (!msDateString) return '';
        const match = /\/Date\((\d+)\)\//.exec(msDateString);
        if (!match) return '';
        const date = new Date(parseInt(match[1]));
        return date.toISOString();
    }

    ///Determines the Estimate in the Versions that has the last modified
    function getLatestEstimateModified(version) {
        if (!version.snapshots || version.snapshots.length === 0) {
            return null; // Or handle as desired
        }
        return formatDotNetDate(version.snapshots[0].lastUpdatedRaw);
    };

    // In your controller
    $scope.customTimeSort = function (item) {
        // Assuming 'item.nestedObject.timeString' holds a date string
        return new Date(item.nestedObject.timeString).getTime();
    };

    ///Used for scrolling to a given id of an element on a page. 
    ///Just adding a regular inline html anchor tag to #ElementID does not work
    ///Our AngularJS vrsn doesn't play well with scrollTo or 
    // Also tried adding these two and did not work well - '$location', '$anchorScroll',
    $scope.scrollTo = function (id, versionHeader = null) {
        var element = null;
        if (versionHeader == "versionHeader") {
            element = document.getElementById('versionHeader' + id);
        }
        else {
            element = document.getElementById(id);
        }
        if (element) {
            element.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
    };

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
        $scope.scrollTo('reviewSectionHeader');
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
        $.download('./report/ViewScopeTrackingGraph', $('form#ViewScopeTrackingGraph').serialize(), "");
    };

    $scope.viewReviewTrackingGraph = function () {
        $.download('./report/ViewReviewTrackingGraph', $('form#ViewScopeTrackingGraph').serialize(), "");
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