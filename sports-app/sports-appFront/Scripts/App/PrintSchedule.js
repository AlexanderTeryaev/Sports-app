
$(document).ready(function () {
    reloadPrintEvents();
});

function reloadPrintEvents() {
    $('#print').click(function () {
        if ($('.res-table').length > 0) {
            $('.no-print').hide();
            windowPrint($('div.col-sm-12'));
            $('.no-print').show();
        }
    });
    $('#print_print_id , .print_button_class').click(function () {
        if ($('.print_id').length > 0) {
            $('.no-print').hide();
            windowPrint($('.print_id'), $('.print_id').data("print-title"));
            $('.no-print').show();
        }
    });

}


function setAutoRefresh(seconds) {
    setTimeout(function () {
        location.reload();
    }, seconds * 1000);
}

function printClickAlternative () {
    if ($('#print_print_id , .print_button_class').length > 0) {
        //$('#print_print_id , .print_button_class').hide();
        windowPrint($('#print_print_id , .print_button_class'), $('#print_print_id , .print_button_class').data("print-title"));
        //$('#print_print_id , .print_button_class').show();
    }
    return false;
}

function printClick() {
    if ($('.print_id').length > 0) {
        $('.no-print').hide();
        windowPrint($('.print_id'), $('.print_id').data("print-title"));
        $('.no-print').show();
    }
}

function printClick(element) {
    if ($('.print_id', $(element)).length > 0) {
        $('.no-print', $(element)).hide();
        windowPrint($('.print_id', $(element)), $('.print_id', $(element)).data("print-title"), true);
        $('.no-print', $(element)).show();
    }
}

function printClickInstant(isLandscape) {
    if ($('.print_id').length > 0) {
        $('.no-print').hide();
        windowPrint($('.print_id'), $('.print_id').data("print-title"), true, isLandscape);
        $('.no-print').show();
    }
}


function windowPrint(element, title = 'Game schedules', isInstant = false, isLandscape = false) {
    var wnd = window.open('', 'Game schedules', 'height=800, width=800');
    wnd.document.write('<html><head><title>' + title + '</title>');
    //if need to add styles print-title
    wnd.document.write('<link rel="stylesheet" href="/Content/bootstrap.css" type="text/css" media="print" />');
    wnd.document.write('<link rel="stylesheet" href="/Content/css/bootstrap-rtl.css" type="text/css" />');
    wnd.document.write('<link rel="stylesheet" href="/Content/site.css" type="text/css" /><style class="bracket_print"></style>');

    if (isLandscape) {
        wnd.document.write('<style>@media print{@page {size: landscape}}</style>');
    }

    $(wnd.document).find('.bracket_print').append($(element).parent().parent().parent().prev().find("#bracketsStyle").html());

    $(element).parent().parent().parent().parent().parent().find(".bracketsStyle").each(function (index, element) {
        $(wnd.document).find('.bracket_print').append($(element).html());
    });

    if ($("#bracket_loser_div_urrg").children().length == 0) {
        $(".container-fluid > .row").css("margin-right", "0px");
    }


    wnd.document.write('<body>');
    wnd.document.write($(element).prop('outerHTML'));

    $(wnd.document).find('.remove_print').remove();
    $(wnd.document).find('.main-btn').remove();
    $(wnd.document).find('img:not(".keep-for-print")').remove();
    $(wnd.document).find('a').replaceWith(function () { return this.innerHTML });
    $(wnd.document).find('footer').remove();
    wnd.document.write('</body></html>');
    wnd.focus();   
    if (isInstant) {
        wnd.document.close();
        wnd.print();
    } else {
        setTimeout(function () {
            wnd.document.close();
            wnd.print();
        }, 1000);
    }
}
