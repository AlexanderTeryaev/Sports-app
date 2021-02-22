var CommonLib = /** @class */ (function () {
    function CommonLib() {
    }
    CommonLib.prototype.initDateTimePickers = function () {
        $(".frm-date").datetimepicker({
            format: 'd/m/Y H:i',
            formatTime: 'H:i',
            formatDate: 'd/m/Y',
            step: 15,
            closeOnDateSelect: false,
            onChangeDateTime: function () {
                $(this).data("input").trigger("changedatetime.xdsoft");
            },
            scrollMonth: false,
            scrollTime: false,
            scrollInput: false,
            dayOfWeekStart: window.Resources.Messages.DateTimePicker_StartOfWeek
        });
        $(".frm-date-wo-time").datetimepicker({
            format: 'd/m/Y',
            timepicker: false,
            scrollMonth: false,
            scrollTime: false,
            scrollInput: false,
            dayOfWeekStart: window.Resources.Messages.DateTimePicker_StartOfWeek,
        });
    };
    return CommonLib;
}());
window.cmn = new CommonLib();
//# sourceMappingURL=commonLib.js.map