//instance application
var dqeApp = angular.module('dqeApp', [
    'ui.bootstrap',
    'ngRoute',
    'ngCookies',
    'dqeControllers',
    'dqeServices',
    'dqeDirectives',
    'angular-growl'
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
        when('/home_pricing_prices', {
            templateUrl: './Views/partials/home_pricing_prices.html',
            controller: 'HomePricingPricesController'
        }).
        when('/home_reports', {
            templateUrl: './Views/partials/home_reports.html',
            controller: 'HomeReportsController'
        }).
        when('/home_selection_lre', {
            templateUrl: './Views/partials/home_selection_lre.html',
            controller: 'HomeSelectionLreController'
        }).
        when('/home_selection_project', {
            templateUrl: './Views/partials/home_selection_project.html',
            controller: 'HomeSelectionProjectController'
        }).
        when('/home_selection_proposal', {
            templateUrl: './Views/partials/home_selection_proposal.html',
            controller: 'HomeSelectionProposalController'
        }).
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
        when('/admin_defaultvalues', {
            templateUrl: './Views/partials/admin_defaultvalues.html',
            controller: 'AdminDefaultValuesController'
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
        when('/profile', {
            templateUrl: './Views/partials/profile.html',
            controller: 'ProfileController'
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
dqeApp.config(['$httpProvider', function($httpProvider) {
    $httpProvider.interceptors.push('myInterceptor');
}]);
//instance services map
var dqeServices = angular.module('dqeServices', []);
//instance controllers map
var dqeControllers = angular.module('dqeControllers', []);
//instance directives map
var dqeDirectives = angular.module('dqeDirectives', []);