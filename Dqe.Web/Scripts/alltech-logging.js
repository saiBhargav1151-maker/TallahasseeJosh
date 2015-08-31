if (!String.prototype.format) {
    String.prototype.format = function () {
        var args = arguments;
        return this.replace(/{(\d+)}/g, function (match, number) {
            return typeof args[number] != 'undefined'
              ? args[number]
              : match
            ;
        });
    };
}

(function (angular) {

    var alltechLogging = angular.module('alltech.logging', []);

    alltechLogging.config(["$provide", "$httpProvider", function ($provide, $httpProvider) {
        $httpProvider.interceptors.push('alltechLoggingErrorResponseInterceptor');
        $httpProvider.interceptors.push('alltechLoggingLogInterceptor');
    }]);

    alltechLogging.constant('alltechLoggingUrl', "/AlltechLogging/Log");

    alltechLogging.factory('$exceptionHandler', ['$log', 'alltechLogService', function ($log, alltechLogService) {

        return function (exception, cause) {
            // Pass off the error to the default error handler
            // on the AngualrJS logger. This will output the
            // error to the console (and let the application
            // keep running normally for the user).
            $log.error.apply($log, arguments);

            // Now, we need to try and log the error the server.
            // --
            try {
                alltechLogService.postClientSideInfoToServer(exception, cause);
            }
            catch (loggingError) {
                // For Developers - log the log-failure.
                $log.warn("Error logging failed");
                $log.log(loggingError);
            }
        };
    }]);

    alltechLogging.factory('alltechLoggingErrorResponseInterceptor', ['$q', 'alltechLogService', 'alltechLoggingUrl', function ($q, alltechLogService, alltechLoggingUrl) {
        var responseInterceptor = {
            'responseError': function (rejection) {
                alltechLogService.logServerResult(rejection);
                if (!rejection.config.url.endsWith(alltechLoggingUrl)) {
                    if (rejection.status == 500) {
                        var serverSideGuid = rejection.data.serverSideExceptionGuid;
                        alltechLogService.postClientSideInfoToServer(undefined, undefined, serverSideGuid);
                    }
                }
                return $q.reject(rejection);
            }
        };
        return responseInterceptor;
    }]);


    alltechLogging.factory('alltechLoggingLogInterceptor', ['alltechLogService', 'alltechLoggingUrl', function (alltechLogService, alltechLoggingUrl) {
        return {
            request: function (config) {
                if (config.method == 'GET' && !config.url.endsWith('.html')) {
                    var getMsg = "HTTP GET: {0}\n";
                    alltechLogService.logInfo(getMsg.format(config.url));
                }
                else if (config.method == 'POST' && !config.url.endsWith(alltechLoggingUrl)) {
                    var postMsg = "HTTP POST: {0}\nData: {1}";
                    alltechLogService.logInfo(postMsg.format(config.url, angular.toJson(config.data)));
                }

                return config;
            }
        };
    }]);

    alltechLogging.factory("alltechLogService", ['$log', '$window', '$injector', 'stacktraceService', 'alltechLoggingUrl', function ($log, $window, $injector, stacktraceService, alltechLoggingUrl) {

        var loggingDisabled = false;
        var rollingLog = [];

        // Only the first 15 issues for each client are reported to the server to prevent flooding the server if there are mass failures  
        var errorsReported = 0;
        var errorReportThreshold = 15;

        var addToRollingLog = function (logMsg) {
            if (rollingLog.length >= 100) {
                rollingLog = rollingLog.splice(50, 50);
            }
            rollingLog.push(logMsg);
        };

        var scrubStackTrace = function (stackTrace) {
            var scrubbedStackTrace = [];
            for (var i = 0; i < stackTrace.length; i++) {
                // The @ character causes outlook to make links of the stack trace.  
                scrubbedStackTrace.push(stackTrace[i].replace("@", " at "));
            }
            return scrubbedStackTrace;
        }

        var factory = {};

        factory.logInfo = function (logMsg) {
            try {
                var logTemplate = "Time: {0} - Alltech Logging Message: {1}";
                var timestamp = new Date();
                var timeFormatted = timestamp.toLocaleTimeString() + " " + timestamp.getMilliseconds() + " Milliseconds";
                var msg = logTemplate.replace("{0}", timeFormatted).replace("{1}", logMsg);
                addToRollingLog(msg);
                $log.info(msg);
            }
            catch (ex) {
                $log(logMsg);
            }
        };

        factory.logServerResult = function (result) {
            try {
                if (result.status == 500) {
                    factory.logErrorResult(result);
                } else if (result.status == 400) {
                    factory.logBadResult(result);
                }
            } catch (ex) {
                $log(ex.message);
            }
        };

        // 400 status code
        factory.logBadResult = function (badResult) {
            try {
                var data = badResult.data;
                var logTemplate = "Validation Messages:\n {0} \n\n";
                var logMsg = logTemplate.replace("{0}", angular.toJson(data.messages));
                addToRollingLog(logMsg);
                $log.error(logMsg);
            } catch (ex) {
                $log(ex.message);
            }
        };

        // 500 status code
        factory.logErrorResult = function (errorResult) {
            try {
                var logTemplate = "Server side exception!!  Method: {0}, URL: {1}.";
                var logMsg = logTemplate
                    .replace("{0}", errorResult.config.method)
                    .replace("{1}", errorResult.config.url);
                addToRollingLog(logMsg);
                $log.error(logMsg);
            } catch (ex) {
                $log(ex.message);
            }
        };

        factory.postClientSideInfoToServer = function (exception, cause, serverSideExceptionIdentifier) {
            if (loggingDisabled) return;
            if (errorsReported > errorReportThreshold) return;

            var errorMessage = exception ? exception.toString() : "No Exception";
            var stackTrace = exception ? stacktraceService.print({ e: exception }) : ["No Stack Trace"];
            stackTrace = scrubStackTrace(stackTrace);

            var url = "." + alltechLoggingUrl;

            // Not using DI because the $http service uses the errorLogService, so any attempt to use the $http service inside of our exception handler will cause a circular dependency
            var http = $injector.get('$http');

            errorsReported++;

            // Log the JavaScript error to the server.
            http.post(url, {
                errorUrl: $window.location.href,
                errorMessage: errorMessage,
                stackTrace: stackTrace,
                cause: (cause || ""),
                rollingLog: rollingLog,
                serverSideExceptionIdentifier: serverSideExceptionIdentifier
            }).then(function (result) {
                // This allows logging to be disabled from the server in the web.config
                loggingDisabled = result.data.loggingDisabled;
            });

            // Clear the rolling log
            rollingLog = [];
        };

        return factory;
    }]);

    // The "stacktrace" library that we included in the Scripts
    // is now in the Global scope; but, we don't want to reference
    // global objects inside the AngularJS components - that's
    // not how AngularJS rolls; as such, we want to wrap the
    // stacktrace feature in a proper AngularJS service that
    // formally exposes the print method.
    alltechLogging.factory("stacktraceService", function () {
        var factory = {};

        factory.print = printStackTrace; // "printStackTrace" is a global object.

        return factory;
    });

})(angular);