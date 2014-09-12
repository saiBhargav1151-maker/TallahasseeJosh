var FDOT = FDOT || {};

//Self-Executing Anonymous Function 
(function (ajaxExceptionHandling, $, undefined) {

    //Private Property
    var ignoreHandler = false;

    //Public Property
    // Can be utilized in applications that latch onto beforeunload event to alert the user that unsaved changes may be discarded
    //  if they leave the page.  By checking the value of this variable (it is set to true when redirecting due to an AJAX Exception) an 
    //  application can elect not to alert the user to save their data before leaving the page in the case of an AJAX Exception. 
    ajaxExceptionHandling.currenltyHandlingAjaxExceptionViaRedirect = false;



    //Public Method
    // Can be utilized to disable the AJAXExceptionHandling if custom error handling is preferred when and AJAX Exception occurs.
    ajaxExceptionHandling.ignoreGenericHandler = function () {
        ignoreHandler = true;
    };

    //Private Method
    // IE doesn't like e.preventDefault so the e.returnValue has been added to accommodate
    function preventDefaultEvents(e) {
        if (e.preventDefault)
            e.preventDefault();
        else
            e.returnValue = false;
    }

    //Private Method
    // Firefox does not have an event property like IE and chrome so must pass as parameter
    function redirectToErrorPageIfAppropriate(evnt, request) {
        preventDefaultEvents(evnt);
        var myData = $.parseJSON(request.responseText);
        if (myData != null) {
            if ('undefined' !== typeof myData.redirectUrl) {
                ajaxExceptionHandling.currenltyHandlingAjaxExceptionViaRedirect = true;
                window.location.href = myData.redirectUrl;
            }
        }
    }

    //Private Method
    function genericAjaxHandler(evnt, request) {
        if (!ignoreHandler) {
            redirectToErrorPageIfAppropriate(evnt, request);
        }
    }

    $(document).ready(function() {
        $(document).bind('ajaxError', genericAjaxHandler);
    });

} (window.FDOT.AjaxExceptionHandling = window.FDOT.AjaxExceptionHandling || {}, jQuery));