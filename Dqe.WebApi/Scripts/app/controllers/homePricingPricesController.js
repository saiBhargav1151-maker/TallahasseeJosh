dqeControllers.controller('HomePricingPricesController', ['$scope', '$rootScope', function ($scope, $rootScope) {
    $rootScope.$broadcast('initializeNavigation');
    $('[data-toggle="comment"]').on('click', function () {
        var id = $(this).attr('data-id');
        if ($('#' + id).is(':visible')) {
            $('#' + id).hide();
        } else {
            $('#' + id).show();
        }
    });
    $('[data-toggle="history"]').on('click', function () {
        var id = $(this).attr('data-id');
        if ($('#' + id).is(':visible')) {
            $('#icon-' + id).removeClass('glyphicon-minus');
            $('#icon-' + id).addClass('glyphicon-plus');
            $('#' + id).hide();
        } else {
            $('#icon-' + id).removeClass('glyphicon-plus');
            $('#icon-' + id).addClass('glyphicon-minus');
            $('#' + id).show();
        }
    });
    $('[data-toggle="price"]').on('click', function () {
        var id = $(this).attr('data-id');
        if ($(this).is(':checked')) {
            $('#' + id).attr('disabled', 'disabled');
        } else {
            $('#' + id).removeAttr('disabled');
        }
    });
}]);