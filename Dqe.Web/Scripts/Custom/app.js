$.ajaxSetup({
    // Disable caching of AJAX responses
    cache: false
});
jQuery.fn.center = function () {
    this.css("position", "fixed");
    this.css("z-index", "9999");
    this.css("top", Math.max(0, (($(window).height() - this.outerHeight()) / 2) + $(window).scrollTop()) + "px");
    this.css("left", Math.max(0, (($(window).width() - this.outerWidth()) / 2) + $(window).scrollLeft()) + "px");
    return this;
};
function InitializeConnection(url) {
    var connection = $.connection(url);
    connection.received(function (data) {
        if (data.MessageHeader === 'Status') {
            $('#statusMessageText').html(data.MessageBody);
            $('#statusMessage').show();
            if (!data.PersistMessage) {
                setTimeout(function () {
                    $('#statusMessage').fadeOut(2000);
                }, 3000);
            }
        }
        if (data.MessageHeader === 'Disconnect') {
            connection.stop();
        }
    });
    connection.start();
}