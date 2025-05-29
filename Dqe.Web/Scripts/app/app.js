//instance application
var dqeApp = angular.module('dqeApp', [
    'ui.bootstrap',
    'ngRoute',
    'ngCookies',
    'dqeControllers',
    'dqeServices',
    'dqeDirectives',
    'angular-growl',
    'ui.utils',
    'blockUI',
    'ngStorage',
    'alltech.logging'
]);
//routing by state
dqeApp.config(['$routeProvider', function ($routeProvider) {
    $routeProvider.
        when('/home_gaming', {
            templateUrl: './Views/partials/home_gaming.html',
            controller: 'HomeGamingController'
        }).
        when('/home_pricing_parameters', {
            templateUrl: './Views/partials/home_pricing_parameters.html',
            controller: 'HomePricingParametersController'
        }).
        when('/home_payitems', {
            templateUrl: './Views/partials/home_payitems.html',
            controller: 'HomePayItemsController'
        }).
        when('/boe/:index?', {
            templateUrl: './Views/partials/boe.html',
            controller: 'HomeBoeController'
        }).
        when('/payitems', {
            templateUrl: './Views/partials/home_payitems.html',
            controller: 'HomePayItemsController'
        }).
        when('/home_estimates', {
            templateUrl: './Views/partials/profile_projects.html',
            controller: 'ProfileProjectsController'
        }).
        when('/home_project_prices/:project?', {
            templateUrl: './Views/partials/home_project_prices.html',
            controller: 'HomeProjectPricesController'
        }).
        when('/home_proposal_prices/:proposal?', {
            templateUrl: './Views/partials/home_proposal_prices.html',
            controller: 'HomeProposalPricesController'
        }).
        //when('/home_pricing_prices_mock', {
        //    templateUrl: './Views/partials/home_pricing_prices_mock.html',
        //    controller: 'HomePricingPricesMockController'
        //}).
        when('/home_reports_proposal_summary', {
            templateUrl: './Views/partials/home_reports_proposal_summary.html',
            controller: 'HomeReportsProposalSummaryController'
        }).
        when('/home_reports_unbalanced_items', {
            templateUrl: './Views/partials/home_reports_unbalanced_items.html',
            controller: 'HomeReportsUnbalancedItemsController'
        }).
        when('/home_reports_summary_letting', {
            templateUrl: './Views/partials/home_reports_summary_letting.html',
            controller: 'HomeReportsSummaryLettingController'
        }).
        when('/home_reports_bid_tolerance', {
            templateUrl: './Views/partials/home_reports_bid_tolerance.html',
            controller: 'HomeReportsBidToleranceController'
        }).
        //when('/home_selection_lre', {
        //    templateUrl: './Views/partials/home_selection_lre.html',
        //    controller: 'HomeSelectionLreController'
        //}).
        when('/home_project/:project?', {
            templateUrl: './Views/partials/home_selection_project.html',
            controller: 'HomeSelectionProjectController'
        }).
        when('/home_selection_project_mock', {
            templateUrl: './Views/partials/home_selection_project_mock.html',
            controller: 'HomeSelectionProjectMockController'
        }).
        when('/home_proposal/:proposal?', {
            templateUrl: './Views/partials/home_selection_proposal.html',
            controller: 'HomeSelectionProposalController'
        }).
        //when('/home_selection_proposal_mock', {
        //    templateUrl: './Views/partials/home_selection_proposal_mock.html',
        //    controller: 'HomeSelectionProposalMockController'
        //}).
        when('/home_snapshots', {
            templateUrl: './Views/partials/home_snapshots.html',
            controller: 'HomeSnapshotsController'
        }).
        when('/home_workingestimate_estimate', {
            templateUrl: './Views/partials/home_workingestimate_estimate.html',
            controller: 'HomeWorkingEstimateEstimateController'
        }).
        when('/home_workingestimate_lsdb', {
            templateUrl: './Views/partials/home_workingestimate_lsdb.html',
            controller: 'HomeWorkingEstimateLsdbController'
        }).
        when('/admin_codevalues', {
            templateUrl: './Views/partials/admin_codevalues.html',
            controller: 'AdminCodeValuesController'
        }).
        when('/admin_costbasedtemplates', {
            templateUrl: './Views/partials/admin_costbasedtemplates.html',
            controller: 'AdminCostBasedTemplatesController'
        }).
        when('/admin_costgroups', {
            templateUrl: './Views/partials/admin_costgroups.html',
            controller: 'AdminCostGroupsController'
        }).
        when('/admin_defaultvalues_market_areas', {
            templateUrl: './Views/partials/admin_defaultvalues_market_areas.html',
            controller: 'AdminDefaultValuesMarketAreasController'
        }).
        when('/admin_defaultvalues_general', {
            templateUrl: './Views/partials/admin_defaultvalues_general.html',
            controller: 'AdminDefaultValuesGeneralController'
        }).
        when('/admin_defaultvalues_pricing_parameters', {
            templateUrl: './Views/partials/admin_defaultvalues_pricing_parameters.html',
            controller: 'DefaultValuesController'
        }).
        when('/admin_payitems_factors', {
            templateUrl: './Views/partials/admin_payitems_factors.html',
            controller: 'AdminPayItemsFactorsController'
        }).
        when('/admin_payitems_maintain', {
            templateUrl: './Views/partials/admin_payitems_maintain.html',
            controller: 'AdminPayItemsMaintainController'
        }).
        when('/admin_payitems_opencopy', {
            templateUrl: './Views/partials/admin_payitems_opencopy.html',
            controller: 'AdminPayItemsOpenCopyController'
        }).
        when('/admin_payitems_structure', {
            templateUrl: './Views/partials/admin_payitems_structure.html',
            controller: 'AdminPayItemsStructureController'
        }).
        when('/admin_security', {
            templateUrl: './Views/partials/admin_security.html',
            controller: 'AdminSecurityController'
        }).
         when('/admin_weblinks', {
             templateUrl: './Views/partials/admin_weblinks.html',
             controller: 'AdminWebLinksController'
         }).
        when('/profile_edit', {
            templateUrl: './Views/partials/profile_edit.html',
            controller: 'ProfileController'
        }).
        when('/profile_projects', {
            templateUrl: './Views/partials/profile_projects.html',
            controller: 'ProfileProjectsController'
        }).
        when('/profile_defaultvalues', {
            templateUrl: './Views/partials/admin_defaultvalues.html',
            controller: 'DefaultValuesController'
        }).
        when('/signin', {
            templateUrl: './Views/partials/signin.html',
            controller: 'SigninController'
        }).when('/unitpricesearch', {
            templateUrl: './Views/partials/unit_price_search.html',
            controller: 'UnitPriceSearchController'
        }).
        otherwise({
            redirectTo: '/signin'
        });
}]);
dqeApp.config(['growlProvider', '$httpProvider', function (growlProvider, $httpProvider) {
    $httpProvider.responseInterceptors.push(growlProvider.serverMessagesInterceptor);
}]);
dqeApp.factory('myInterceptor', ['$q', 'growl', function ($q, growl) {
    var responseInterceptor = {
        'responseError': function (rejection) {
            growl.addErrorMessage("An unexpected error has occurred.");
            return $q.reject(rejection);
        }
    };
    return responseInterceptor;
}]);
// ReSharper disable InconsistentNaming
dqeApp.config(['blockUIConfigProvider', function (blockUIConfigProvider) {
// ReSharper restore InconsistentNaming
    // Change the default overlay message
    //blockUIConfigProvider.message('Processing...');
    // Change the default delay to 100ms before the blocking is visible
    //blockUIConfigProvider.delay(0);
    //blockUIConfigProvider.resetOnException = false;
    blockUIConfigProvider.requestFilter(function (config) {
        // If the request starts with './staff/GetStaffByName' ...
        if (config.url.toLowerCase().endsWith('.html')) {
            return false;
        }
        if (config.url.match(/^\.\/security\/GetTimeout($|\/).*/i)) {
            return false; // ... don't block it.
        }
        if (config.url.match(/^\.\/estimate\/LoadProjectEstimateSummary($|\/).*/i)) {
            return false; // ... don't block it.
        }
        if (config.url.match(/^\.\/estimate\/LoadProposalEstimateSummary($|\/).*/i)) {
            return false; // ... don't block it.
        }
        if (config.url.match(/^\.\/projectproposal\/IsProjectSynced($|\/).*/i)) {
            return false; // ... don't block it.
        }
        if (config.url.match(/^\.\/staff\/GetStaffByName($|\/).*/i)) {
            return false; // ... don't block it.
        }
        if (config.url.match(/^\.\/staff\/GetDqeStaffByName($|\/).*/i)) {
            return false; // ... don't block it.
        }
        if (config.url.match(/^\.\/projectproposal\/GetProjects($|\/).*/i)) {
            return false; // ... don't block it.
        }
        if (config.url.match(/^\.\/projectproposal\/GetProposals($|\/).*/i)) {
            return false; // ... don't block it.
        }
        if (config.url.match(/^\.\/projectproposal\/GetDqeProposals($|\/).*/i)) {
            return false; // ... don't block it.
        }
        if (config.url.match(/^\.\/pricingengine\/AsyncCalculateBidHistory($|\/).*/i)) {
            return false; // ... don't block it.
        }
        if (config.url.match(/^\.\/estimate\/AsyncSaveBidHistory($|\/).*/i)) {
            return false; // ... don't block it.
        }
        if (config.url.match(/^\.\/estimate\/GenerateParameterPrices($|\/).*/i)) {
            return false; // ... don't block it.
        }
        if (config.url.match(/^\.\/PayItemStructureAdministration\/GetUnlinkedItems($|\/).*/i)) {
            return false; // ... don't block it.
        }
        if (config.url.match(/^\.\/report\/GetLettings($|\/).*/i)) {
            return false; // ... don't block it.
        }
        if (config.url.match(/^\.\/report\/GetDqeReportProposals($|\/).*/i)) {
            return false; // ... don't block it.
        }
        if (config.url.match(/^\.\/weblinkadministration\/SearchWebLinks($|\/).*/i)) {
            return false; // ... don't block it.
        }
        if (config.url.match(/^\.\/costgroup\/GetPayItems($|\/).*/i)) {
            return false; // ... don't block it.
        }
        return true;
    });
}]);
dqeApp.config(['$httpProvider', function($httpProvider) {
    $httpProvider.interceptors.push('myInterceptor');
}]);
dqeApp.config(['$httpProvider', function ($httpProvider) {
    $httpProvider.interceptors.push('noCacheInterceptor');
}]).factory('noCacheInterceptor', function () {
    return {
        request: function (config) {
            //console.log(config.method);
            //console.log(config.url);
            if (config.method == 'GET') {

                if (!config.url.toLowerCase().endsWith('.html')) {
                    var separator = config.url.indexOf('?') === -1 ? '?' : '&';
                    config.url = config.url + separator + 'noCache=' + new Date().getTime();
                }
            }
            //console.log(config.method);
            //console.log(config.url);
            return config;
        }
    };
});
dqeApp.filter('startFrom', function() {
    return function (input, start) {
        if (input == undefined) return null;
        return input.slice(start);
    }
});
dqeApp.run(function ($rootScope, $templateCache, $cookies, $location) {
    $rootScope.$on('$routeChangeStart', function (event, next, current) {
        if (typeof (current) !== 'undefined') {
            $templateCache.remove(current.templateUrl);
        }
    });
    $rootScope.$on('$locationChangeStart', function (event, next, current) {
        var basePath = $location.$$absUrl.replace($location.$$url, '').toLowerCase();
        var currentUrl = current.toLowerCase().replace(basePath, '');
        var nextUrl = next.toLowerCase().replace(basePath, '');
        if (nextUrl == '/signin') {
            //don't allow if signed in
            if ($cookies.DQE_AUTH_TICKET != undefined) {
                event.preventDefault();
                $rootScope.$broadcast('initializeNavigation');
            }
        } else {
            if ($cookies.DQE_AUTH_TICKET == undefined) {
                if (currentUrl != '/signin') {
                    //if signed out, only allow access to select urls
                    if (nextUrl != '' && !nextUrl.startsWith('/boe') && nextUrl != '/unitpricesearch' && nextUrl != '/payitems' && nextUrl != '/signin') {
                        //event.preventDefault();
                        $location.path('/signin');
                    }
                }
                //else if (nextUrl != '' && !nextUrl.startsWith('/boe') && nextUrl != '/payitems' && nextUrl != '/signin') {
                //    //event.preventDefault();
                //    $location.path('/signin');
                //}
            } 
        }
    });
});
//instance services map
var dqeServices = angular.module('dqeServices', []);
//instance controllers map
var dqeControllers = angular.module('dqeControllers', []);
//instance directives map
var dqeDirectives = angular.module('dqeDirectives', []);
//select-on-click
dqeDirectives.directive('selectOnClick', function () {
    // Linker function
    return function (scope, element, attrs) {
        element.bind('click', function () {
            if (this.value == '$0.00') {
                this.select();
            }
        });
    };
});
dqeDirectives.directive('scrollOnClick', function () {
    return {
        restrict: 'A',
        link: function (scope, $elm, attrs) {
            var idToScroll = attrs.href;
            $elm.on('click', function (event) {
                event.preventDefault();
                var $target;
                if (idToScroll) {
                    $target = $(idToScroll);
                } else {
                    $target = $elm;
                }
                $("body, html").animate({ scrollTop: $target.offset().top - 60 }, "slow");
            });
        }
    }
});
dqeDirectives.directive('scrollVisible', function () {
    return {
        restrict: 'A',
        link: function (scope, $elm) {
            $elm.hide();
            $(window).bind("scroll", function() {
                if (this.pageYOffset >= 400) {
                    $elm.show();
                } else {
                    $elm.hide();
                }
                scope.$apply();
            });
        }
    }
});
dqeDirectives.directive('myMaxlength', function () {
    return {
        require: 'ngModel',
        link: function (scope, element, attrs, ngModelCtrl) {
            var maxlength = Number(attrs.myMaxlength);
            function fromUser(text) {
                if (text.length > maxlength) {
                    var transformedInput = text.substring(0, maxlength);
                    ngModelCtrl.$setViewValue(transformedInput);
                    ngModelCtrl.$render();
                    return transformedInput;
                }
                return text;
            }
            ngModelCtrl.$parsers.push(fromUser);
        }
    };
});
dqeDirectives.directive('focusContent', function () {
    return {
        restrict: 'A',
        scope: {
            focusTrigger: '=focusContent'
        },
        link: function (scope, element) {
            scope.$watch('focusTrigger', function (newValue) {
                if (newValue === true) {
                    element[0].focus();
                    scope.focusTrigger = false;
                }
            });
        }
    };
});
dqeDirectives.directive('format', ['$filter', function ($filter) {
    return {
        require: '?ngModel',
        link: function (scope, elem, attrs, ctrl) {
            if (!ctrl) return;

            var format = {
                prefix: '$',
                centsSeparator: '.',
                thousandsSeparator: ','
            };

            ctrl.$parsers.unshift(function (value) {
                elem.priceFormat(format);
                //console.log('parsers', elem[0].value);
                return elem[0].value;
            });

            ctrl.$formatters.unshift(function (value) {
                var num = Number(ctrl.$modelValue.toString().replace(/[^0-9\.]+/g, ""));
                elem[0].value = Math.round(num * 100);
                //elem[0].value = ctrl.$modelValue * 100;
                elem.priceFormat(format);
                //console.log('formatters', elem[0].value);
                return elem[0].value;
            });
        }
    };
}]);
dqeDirectives.directive('a', function () {
    return {
        restrict: 'E',
        link: function (scope, elem, attrs) {
            if (attrs.ngClick || attrs.href === '' || attrs.href === '#') {
                elem.on('click', function (e) {
                    e.preventDefault();
                });
            }
        }
    };
});
dqeDirectives.directive('focusMe', function ($timeout, $parse) {
    return {
        //scope: true,   // optionally create a child scope
        link: function (scope, element, attrs) {
            var model = $parse(attrs.focusMe);
            scope.$watch(model, function (value) {
                //console.log('value=', value);
                if (value === true) {
                    $timeout(function () {
                        element[0].focus();
                    });
                }
            });
            // to address @blesh's comment, set attribute value to 'false'
            // on blur event:
            //element.bind('blur', function () {
            //    console.log('blur');
            //    scope.$apply(model.assign(scope, false));
            //});
        }
    };
});
