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
    'blockUI'
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
        when('/home_project_prices', {
            templateUrl: './Views/partials/home_project_prices.html',
            controller: 'HomeProjectPricesController'
        }).
        when('/home_proposal_prices', {
            templateUrl: './Views/partials/home_proposal_prices.html',
            controller: 'HomeProposalPricesController'
        }).
        //when('/home_pricing_prices_mock', {
        //    templateUrl: './Views/partials/home_pricing_prices_mock.html',
        //    controller: 'HomePricingPricesMockController'
        //}).
        when('/home_reports', {
            templateUrl: './Views/partials/home_reports.html',
            controller: 'HomeReportsController'
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
        when('/admin_defaultvalues_market_areas', {
            templateUrl: './Views/partials/admin_defaultvalues_market_areas.html',
            controller: 'AdminDefaultValuesMarketAreasController'
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
            growl.addErrorMessage("ERROR: An unexpected error has occurred.");
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
        if (config.url.match(/^\.\/estimate\/LoadProjectEstimateSummary($|\/).*/)) {
            return false; // ... don't block it.
        }
        if (config.url.match(/^\.\/estimate\/LoadProposalEstimateSummary($|\/).*/)) {
            return false; // ... don't block it.
        }
        if (config.url.match(/^\.\/projectproposal\/IsProjectSynced($|\/).*/)) {
            return false; // ... don't block it.
        }
        if (config.url.match(/^\.\/staff\/GetStaffByName($|\/).*/)) {
            return false; // ... don't block it.
        }
        if (config.url.match(/^\.\/staff\/GetDqeStaffByName($|\/).*/)) {
            return false; // ... don't block it.
        }
        if (config.url.match(/^\.\/projectproposal\/GetProjects($|\/).*/)) {
            return false; // ... don't block it.
        }
        if (config.url.match(/^\.\/projectproposal\/GetProposals($|\/).*/)) {
            return false; // ... don't block it.
        }
        if (config.url.match(/^\.\/pricingengine\/AsyncCalculateBidHistory($|\/).*/)) {
            return false; // ... don't block it.
        }
        if (config.url.match(/^\.\/estimate\/AsyncSaveBidHistory($|\/).*/)) {
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
            if (this.value == '0') {
                this.select();
            }
        });
    };
});
