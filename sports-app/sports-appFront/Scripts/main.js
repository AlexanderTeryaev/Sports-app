var frontLib = (function ($) {

    return {
        init: function () {

            setDefaults();
            setRsultsFocus();
            showHideLeagueInfoText();
        }
    }

    function setDefaults()
    {
        $('body').on('hidden.bs.modal', '.modal', function () {
            $(this).removeData('bs.modal');
        });
    }

    function setRsultsFocus()
    {
        if ($('#closemonth').length == 0)
            return;

        $('html, body').animate({
            scrollTop: $("#closemonth").offset().top
        }, 1000);
    }

    function showHideLeagueInfoText() {
        $('.show-hide-league-info').on('click', function () {
            $('.league-info').toggle();
        });
    }
})(jQuery);