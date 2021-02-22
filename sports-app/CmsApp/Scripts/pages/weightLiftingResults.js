$(document).ready(function () {
    function setFormData(obj) {
        var thisObj = $(obj);
        var formId = $(thisObj).attr('form');
        var thisName = $(thisObj).attr('name');
        var thisVal = $(thisObj).val();

        var oppoName = '';
        var oppoObj = '';
        var oppoVal = '';

        var index = thisName.substr(test.length - 1, 1);
        if (thisName.substr(0, thisName.length - 1) === 'Lifting') {
            oppoName = 'Push' + index;
        } else {
            oppoName = 'Lifting' + index;
        }

        oppoObj = $('input[name="' + oppoName + '"][form="' + formId + '"]');
        oppoVal = $(oppoObj).val();

        var decVal = $('td[extra_data="' + formId + '"]').html();

        if (isNaN(parseFloat(thisVal)) && thisVal != '') {
            alert('This Field must be Number');
            $(thisObj).val('');
            return false;
        }

        if (index === '1') {
            if (!isNan(parseFloat(decVal))) {
                var can = parseFloat(decVal) - parseFloat(thisVal) - 20;
                if (can < 0)
                    can = 0;

                if ($(oppoObj.closest('td').next().children('select').val() === '')) {
                    if (isNaN(oppoVal) || can > parseFloat(oppoVal))
                        $(oppoObj).val(can);
                }
            }
        } else {

        }
    }

    $('.modal-body input[type=text]').focusout(function () {
        var formid = $(this).attr("form");
        var res = setFormData(this);
        if (res)
            $('form#' + formid).submit();
    });

    $('.modal-body select').change(function () {
        var val = $(this).val();
        if ($(this).attr('id') !== 'Lift3Success' && $(this).attr('id') !== 'Push3Success') {
            var obj = $(this).closest('td').next('td').children('input');
            var can = parseFloat($(this).closest('td').prev('td').children('input').val());
            if (!isNaN(can) && val !== '') {
                if (val === 'True') {
                    can += 1;
                } 
                $(obj).val(can);
                $(obj).attr('min', can);
            }
        }

        var formid = $(this).attr("form");
        $('form#' + formid).submit();
    });
});