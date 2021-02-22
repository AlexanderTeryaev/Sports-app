var loader = "";
var showAll = false;

function ConvertValue(value) {
    if (value === "true" || value === true) {
        return window.Resources.Messages.Yes;
    } else if (value === "false" || value === false) {
        return window.Resources.Messages.No;
    }

    return value;
}

function GetHeaderNodeValueForExport(node) {
    return $(node).clone().children().remove().end()
        .text(); //Getting rid of possible header additions, like <select> filters
}

function showEmailMessage() {
    var isChecked = $(event.target).is(":checked");
    if (isChecked) {
        $("#activityEmailModal").modal("toggle");
    }
    else {
        sendByClubEmail(false);
    }
}

function sendByClubEmail(isClubEmailChecked) {
    if (isClubEmailChecked) {
        $("#notificationForm").append('<input type="hidden" value="True" name= "IsClubEmailChecked" id="clubEmailHidden" />');
    }
    else {
        var clubEmailHidden = $("#clubEmailHidden");
        if (clubEmailHidden) {
            $("#clubEmailHidden").remove();
        }
    }
    $("#activityEmailModal").modal("toggle");
}

function GetBodyNodeValueForExport(node) {
    var nodeChild = $(node).children(":first");

    if (nodeChild.is("input") || nodeChild.is("select") || nodeChild.is("textarea")
    ) { //convert value of editable elements
        if (nodeChild.is(":checkbox")) {
            var checkboxValue = nodeChild.prop("checked");

            return ConvertValue(checkboxValue);
        }

        return ConvertValue(nodeChild.val());
    }

    return $(node).text();
}

function LoadRegistrations(activityId) {
    $.get("/Activity/GetRegistrations/" + activityId + "?showAll=" + showAll,
        function (data) {
            $(".loader .message").fadeOut(100,
                function () {
                    $(this).text(window.Resources.Messages.Activity_Status_FormattingTable).fadeIn(100);
                });

            if (!loader.length) {
                loader = $("#registrationStatusBody").html();
            }

            setTimeout(function () {
                $("#registrationStatusBody").html(data);

                var regStatusTable = $("#registrationStatus-table");
                var modalTitle = $("#registrationStatusModalLabel");

                modalTitle.text(regStatusTable.data("registrationstitle"));

                InitializeStatusDataTable(activityId);

                $("#registrationStatusModal").modal("handleUpdate");
            }, 200);
        });
}

function InitializeStatusDataTable(activityId) {
    var regStatusTable = $("#registrationStatus-table");

    $.fn.dataTable.ext.order["dom-checkbox"] = function(settings, col) {
        return this.api().column(col, { order: "index" }).nodes().map(function(td, i) {
            return $("input", td).prop("checked") ? "1" : "0";
        });
    };

    $.fn.dataTable.ext.order["dom-text"] = function(settings, col) {
        return this.api().column(col, { order: "index" }).nodes().map(function(td, i) {
            return $("input", td).val();
        });
    };

    $.fn.dataTableExt.ofnSearch["html-input"] = function(value) {
        var element = $(value);

        var actualValue;

        if (element.is(":checkbox")) {
            actualValue = element.prop("checked");
        } else if (element.is("a")) {
            actualValue = $.trim(element.text());
        } else {
            actualValue = $(value).val();
        }

        return actualValue === undefined || actualValue === null
            ? ""
            : actualValue.toString().replace(/"/g, "{QUOTE}");
    };

    var table = regStatusTable.DataTable({
        "order": [],
        dom: "lBf<'sorting-tip'>rtip",
        //stateSave: true,
        "scrollY": "55vh",
        "scrollX": true,
        "scrollCollapse": true,
        "deferRender": true,
        "scroller": false,
        colReorder: true,
        "lengthMenu": [[10, 25, 50, 100, -1], [10, 25, 50, 100, window.Resources.Messages.All]],
        "pageLength": 50,
        buttons: [
            {
                extend: "excel",
                text: window.Resources.Messages.ExportToExcel,
                title: null,
                filename: regStatusTable.data("activityname"),
                exportOptions: {
                    columns: ":visible:not([data-nofilter])",
                    modifier: {
                        page: "current"
                    },
                    format: {
                        header: function(data, index, node) {
                            return GetHeaderNodeValueForExport(node);
                        },

                        body: function(data, row, col, node) {
                            return GetBodyNodeValueForExport(node);
                        }
                    }
                },
                action: function(e, dt, node, config) {
                    if ($.fn.dataTable.ext.buttons.excelHtml5.available(dt, config)) {
                        $.fn.dataTable.ext.buttons.excelHtml5.action.call(this, e, dt, node, config);
                    } else {
                        $.fn.dataTable.ext.buttons.excelFlash.action.call(this, e, dt, node, config);
                    }
                }
            },
            {
                extend: "print",
                text: window.Resources.Messages.Print,
                title: regStatusTable.data("activityname"),
                exportOptions: {
                    columns: ":visible:not([data-nofilter])",
                    modifier: {
                        page: "current"
                    },
                    format: {
                        header: function(data, index, node) {
                            return GetHeaderNodeValueForExport(node);
                        },

                        body: function(data, row, col, node) {
                            return GetBodyNodeValueForExport(node);
                        }
                    }
                },
                action: function(e, dt, node, config) {
                    $.fn.dataTable.ext.buttons.print.action.call(this, e, dt, node, config);
                }
            },
            //{
            //    extend: "pdf",
            //    title: regStatusTable.data("activityname"),
            //    filename: regStatusTable.data("activityname"),
            //    orientation: "landscape",
            //    exportOptions: {
            //        columns: ":not([data-nofilter])",
            //        format: {
            //            header: function(data, index, node) {
            //                return GetHeaderNodeValueForExport(node);
            //            },

            //            body: function (data, row, col, node) {
            //                return GetBodyNodeValueForExport(node);
            //            }
            //        }
            //    },
            //    action: function (e, dt, node, config) {
            //        if ($.fn.dataTable.ext.buttons.pdfHtml5.available(dt, config)) {
            //            $.fn.dataTable.ext.buttons.pdfHtml5.action.call(this, e, dt, node, config);
            //        } else {
            //            $.fn.dataTable.ext.buttons.pdfFlash.action.call(this, e, dt, node, config);
            //        }
            //    }
            //},
            {
                text: window.Resources.Messages.Activity_Status_ResetColumnsOrder,
                action: function(e, dt, node, config) {
                    dt.colReorder.reset();
                }
            },
            {
                text: window.Resources.Messages.Activity_Status_ResetColumnsFilters,
                action: function(e, dt, node, config) {
                    $("select.status-filter").each(function(index, element) {
                        $(element).val("");
                        $(element).trigger("change");
                    });
                }
            },
            {
                text: window.Resources.Messages.Activity_Status_ResetColumnsNames,
                action: function(e, dt, node, config) {
                    dt.columns().every(function() {
                        var columnHeader = $(this.header());

                        var originalName = columnHeader.data("originalname");

                        if (originalName !== undefined) {
                            var childControls =
                                columnHeader.clone(true)
                                    .children(); //clone(true) means deep copy with all data and bound events

                            columnHeader.text(originalName).append(childControls);
                            columnHeader.editable("setValue", originalName, false);
                            $.post("/Activity/ResetStatusColumnsNames/" + activityId);
                        }
                    });
                }
            },
            {
                text: window.Resources.Messages.Activity_Status_ResetSorting,
                action: function(e, dt, node, config) {
                    dt.column("[name='IsActiveColumn']").order("asc").draw();
                }
            }
        ],
        "language": {
            "processing": window.Resources.Messages.Loading,
            "search": window.Resources.Messages.Search,
            "lengthMenu": window.Resources.Messages.Datatable_LengthMenu,
            "zeroRecords": window.Resources.Messages.Datatable_NothingFound,
            "info": window.Resources.Messages.Datatable_Info,
            "infoEmpty": window.Resources.Messages.Datatable_InfoEmpty,
            "infoFiltered": window.Resources.Messages.Datatable_InfoFiltered,
            "paginate": {
                "next": window.Resources.Messages.Next,
                "previous": window.Resources.Messages.Previous
            }
        },
        "drawCallback": function(settings) {
            var table = this.api();

            $(".x-editable").editable({
                container: "#activity-status-container",
                toggle: "click",
                placement: "top",
                highlight: "#6AB25F",
                emptytext: window.Resources.Messages.Activity_Status_CustomField_NoValue,
                validate: function(value) {
                    if ($.trim(value) === "") {
                        return window.Resources.Messages.PropertyValueRequired;
                    }

                    return null;
                }
            });

            $(".registration-customprices").popover({
                container: "body",
                html: true,
                content: function () {
                    return $(this).next(".registration-customprices-content").html();
                },
                trigger: "hover",
                /**/
                //title: "@Messages."
                /**/
            });

            $("#activity-status-container .lazy").lazy({
                effect: "fadeIn",
                appendScroll: $("#activity-status-container .dataTables_scrollBody"),
                afterLoad: function (element) {
                },
                onError: function (element) {
                }
            });

            //table.columns.adjust();
        }
    });

    var searchInput = $("#registrationStatus-table_filter input[type=search]");
    searchInput.unbind();
    searchInput.bind("keyup",
        function(e) {
            if (e.keyCode === 13) {
                //if (this.value.length === 0 || this.value.length > 2 || e.keyCode === 13) {
                table.search(this.value).draw();
            }
        });

    $("div.sorting-tip")
        .append($("<i>" + window.Resources.Messages.Activity_Status_SortByMultiple_Tip + "</i>"));

    var columnsSorting = regStatusTable.data("columnssorting"); //apply previously saved sorting
    if (columnsSorting !== undefined && columnsSorting !== "") {
        if (columnsSorting.length > 0 &&
            columnsSorting[0].length > 0 &&
            table.columns()[0].length > columnsSorting[0][0]) { //check that saved sorting index exist in current table
            table.order(columnsSorting).draw();
        }
    }
    table.on("order", //save sorting to user
        function() {
            $.post("/Activity/SaveStatusSorting/" + activityId,
                { value: JSON.stringify(table.order()) });
        });

    var columnsNames = regStatusTable.data("columnsnames"); //rename columns from user saved
    $.each(columnsNames,
        function(index, item) {
            var column = table.column(item.index);
            var jHeader = $(column.header());

            jHeader.data("originalname", jHeader.text());
            jHeader.text(item.name);
        });
    table.columns().every(function() {
        var columnHeader = $(this.header());

        var originalName = columnHeader.data("originalname");
        if (originalName === undefined) {
            columnHeader.data("originalname", columnHeader.text());
        }

        $(
                '<a href="#" class="edit-column-header glyphicon glyphicon-pencil" aria-hidden="true" onclick="EditColumn(this);event.stopPropagation();"></a>')
            .appendTo(columnHeader);

        columnHeader.editable({
            type: "text",
            title: window.Resources.Messages.Activity_Status_EditColumn_EnterName,
            container: "#activity-status-container",
            toggle: "manual",
            placement: "bottom",
            highlight: "#6AB25F",
            pk: this.index(),
            url: "/Activity/EditStatusColumnName/" + activityId,
            validate: function(value) {
                if ($.trim(value) === "") {
                    return window.Resources.Messages.PropertyValueRequired;
                }

                return null;
            },
            display: function(value, sourceData) {
                var childControls =
                    $(this).clone(true)
                        .children(); //clone(true) means deep copy with all data and bound events

                $(this).text($.trim(value)).append(childControls);
            }
        });
    });

    var columns =
        regStatusTable.DataTable()
            .columns(); //init "Toggle columns" feature and hide those that was saved to user
    var hiddenColumns = regStatusTable.data("hiddencolumns").toString().split(",");
    $.each(columns[0],
        function(index, item) {
            var column = regStatusTable.DataTable().column(item);
            var isColumnHidden = hiddenColumns.includes(index.toString());

            var header = $(column.header()).text();
            if (header === "") {
                return;
            }

            if (item !== columns[0][columns[0].length - 1] && item !== columns[0][0]
            ) { //Do not apped " - " to last and first element
                $("#activity-status-columns-list").append(" - ");
            }

            var itemStyle = isColumnHidden ? "color: red" : "";
            if (header !== "") {
                $("#activity-status-columns-list").append($("<a>",
                    { "data-column": item, text: header, style: itemStyle }));
            }
            if (isColumnHidden) {
                column.visible(false);
            }
        });

    table.on("column-reorder",
        function(e, settings, details) {
            $.ajax({
                type: "POST",
                url: "/Activity/SaveStatusColumnsOrder/",
                data: {
                    id: activityId,
                    columns: table.colReorder.order()
                }
            });
        });
    var columnsOrder = regStatusTable.data("columnsorder"); //reorder columns by user saved
    if (columnsOrder.length > 0) {
        if (columnsOrder.length === table.colReorder.order().length) {
            table.colReorder.order(columnsOrder);
        }
    }

    table.columns().every(function() { //add <select> filters to column headers
        var column = this;
        var jColumnHeader = $(column.header());

        if (!jColumnHeader.is("[data-nofilter]")) {
            var select =
                $(
                        '<select class="status-filter" onclick="event.stopPropagation();"><option value=""></option></select>')
                    .appendTo(jColumnHeader)
                    .on("change",
                        function() {
                            var val = $.fn.dataTable.util.escapeRegex(
                                $.trim($(this).val())
                            );

                            if (column.search() !== val) {
                                column.search(val ? "^" + val + "$" : "", true, true, true).draw();
                            }
                        });

            var selectOptions = [];

            column.data().unique().sort().each(function(d, j) {
                var actualValue = d;

                if ((d.match("^<.+>") && d.match("</.+>$")) || d.match("^<input .+>$")) {
                    var element = $(d);

                    if (element.is(":checkbox")) {
                        actualValue = element.prop("checked");
                    } else if (element.is("a")) {
                        actualValue = $.trim(element.text());
                    } else if (element.is("span")) {
                        actualValue = $.trim(element.text());
                    } else {
                        actualValue = element.val();
                    }
                }

                if (actualValue === "true" || actualValue === true) {
                    d = window.Resources.Messages.Yes;
                } else if (actualValue === "false" || actualValue === false) {
                    d = window.Resources.Messages.No;
                } else if (actualValue !== d) {
                    d = actualValue;
                }

                if ($.inArray(d, selectOptions) >= 0) {
                    return;
                }

                selectOptions.push(d);

                actualValue = actualValue.toString().replace(/"/g, "{QUOTE}");

                select.append('<option value="' + actualValue + '">' + d + "</option>");
            });
        }
    });

    $("#registrationStatusBody").on("click",
        "a.edit-column-header",
        function(e) {
            $(this).closest("th").editable("toggle");
        });

    $("#activity-status-columns-list").on("click",
        "a",
        function(e) {
            e.preventDefault();

            var item = $(this).attr("data-column");
            var column = table.column(table.colReorder.transpose(parseInt(item)));
            var visible = !column.visible();

            column.visible(visible);

            e.target.style = visible ? "" : "color: red";
            $.ajax({
                type: "POST",
                url: "/Activity/StatusColumnVisibility/",
                data: {
                    activityId: activityId,
                    item: item,
                    value: visible
                }
            });
        });

    $("body").tooltip({ selector: "[data-toggle=tooltip]" });
    table.columns.adjust();
}

function EditColumn(btn) {
    $(btn).closest("th").editable("toggle");
}

$(".openRegistrationsStatus").click(function () {
    var activityId = $(this).data("activity");
    LoadRegistrations(activityId);
});

$("#registrationStatusModal").on("hidden.bs.modal",
    function (e) {
        if (e.target === e.currentTarget) { //prevent event propagation from child modals
            $("#registrationStatusBody").html(loader);
            showAll = false;
        }
    });

$("#registrationStatusModal").on("show.bs.modal",
    function (event) {
        if (event.target === event.currentTarget) { //prevent event propagation from child modals
            var button = $(event.relatedTarget);
            var modalTitle = button.data("title");
            var modal = $(this);
            modal.find(".modal-title").text(modalTitle);
        }
    });

$("#registrationStatusModal").on("shown.bs.modal",
    function () {
        var modal = $(this);
        modal.modal("handleUpdate");
    });

//$(document).on("shown.bs.modal",
//    "#activityStatus-sendNotification-modal",
//    function () {
//        var notificationForm = $("#activityStatus-sendNotification-modal #notificationFormForActivity");

//        //reset validation definition
//        notificationForm
//            .removeData("validator") /* added by the raw jquery.validate plugin */
//            .removeData("unobtrusiveValidation"); /* added by the jquery unobtrusive plugin*/

//        $.validator.unobtrusive.parse(notificationForm);
//        notificationForm.valid();
//    });

$(document).on("submit",
    "#activityStatus-sendNotification-modal-content form",
    function (e) {
        e.preventDefault();

        var url = $(this).closest("form").attr("action"),
            form = $(this).closest("form"),
            formData = new FormData(form[0]);

        $.ajax({
            url: url,
            type: "POST",
            data: formData,
            contentType: false, // NEEDED
            processData: false // NEEDED
        });

        $("#activityStatus-sendNotification-modal").modal("hide");
    });

$(document).on("change",
    "#sendNotification-selectAll",
    function () {
        var table = $("#registrationStatus-table").DataTable();

        var controls = table.$("input.sendNotification", { filter: "applied", page: "all" });

        if ($(this).prop("checked")) {
            controls.prop("checked", true);
        } else {
            controls.prop("checked", false);
        }

        controls.trigger("change");
    });

$(document).on("change",
    "#registrationStatus-table input.sendNotification",
    function () {
        var table = $("#registrationStatus-table").DataTable();

        var controls = table.$("input.sendNotification:checked", { filter: "applied", page: "all" });

        if (controls.length > 0) {
            $("#sendNotificationToSelected").prop("disabled", false);
        } else {
            $("#sendNotificationToSelected").prop("disabled", true);
            $("#sendNotification-selectAll").prop("checked", false);
        }
    });

$(document).on("click",
    "#sendNotificationToSelected",
    function () {
        var table = $("#registrationStatus-table").DataTable();

        var controls = table.$("input.sendNotification:checked", { filter: "applied", page: "all" });

        var ids = [];

        controls.each(function (index, element) {
            ids.push($(element).data("userid"));
        });

        $.post("/TeamPlayers/GetNotificationsForm",
            { seasonId: $(this).data("seasonid"), playersIds: ids, activityId: $(this).data("activity") },
            function (data) {
                var modal = $("#activityStatus-sendNotification-modal");

                modal.find(".modal-content").html(data);

                modal.modal("show");
            });
    });

$(document).on("click",
    "#registrationStatus-table td.big-text",
    function () {
        $(this).toggleClass("wrap");
    });

$(document).on("click",
    "#registrationStatus-table .approveMedicalCert",
    function () {
        var userIdNum = $(this).data("user");
        var activityId = $(this).data("activity");

        $.post("/Activity/ApproveMedicalCert",
            { idNum: userIdNum, activityId: activityId, value: this.checked },
            function () {
                //LoadRegistrations(activityid);
            });
    });

$(document).on("change",
    "#registrationStatus-table .team-selfinsurance",
    function () {
        var activityid = $(this).data("activity");
        var regId = $(this).data("regid");

        $.post("/Activity/SetTeamSelfInsurance/" + activityid,
            { regid: regId, value: this.value },
            function () {
                //LoadRegistrations(activityid);
            });
    });

$(document).on("click",
    "#registrationStatus-table .registrationActive",
    function () {
        var activityid = $(this).data("activity");
        var regId = $(this).data("regid");

        $.post("/Activity/SetRegistrationActive/" + activityid,
            { regid: regId, value: this.checked },
            function () {
                //LoadRegistrations(activityid);
            });
    });

$(document).on("change",
    "#registrationStatus-table input.paidValue",
    function () {
        var activityid = $(this).data("activity");
        var regId = $(this).data("regid");

        $.post("/Activity/UpdatePaid/" + activityid, { regid: regId, value: this.value });

        var registrationPrice = $(this).closest("tr").find("td.price").text();
        var insurancePrice = $(this).closest("tr").find("td.insurancePrice").text();

        var remains = parseFloat(registrationPrice) - this.value;
        if (insurancePrice) {
            remains = remains + parseFloat(insurancePrice);
        }

        $(this).closest("tr").find("td.remainPayment").text(remains);
    });

$(document).on("change",
    "#registrationStatus-table input.insurancePaidValue",
    function () {
        var control = $(this);
        var activityid = control.data("activity");
        var regId = control.data("regid");

        $.post("/Activity/UpdatePlayerInsurancePaid/" + activityid, { regid: regId, value: this.value }, function () {
            RecalculateRemainForPayment(control);
        });
    });
$(document).on("change",
    "#registrationStatus-table input.registrationPaidValue",
    function () {
        var control = $(this);
        var activityid = control.data("activity");
        var regId = control.data("regid");

        $.post("/Activity/UpdatePlayerRegistrationPaid/" + activityid, { regid: regId, value: this.value }, function () {
            RecalculateRemainForPayment(control);
        });
    });
$(document).on("change",
    "#registrationStatus-table input.participationPaidValue",
    function () {
        var control = $(this);
        var activityid = control.data("activity");
        var regId = control.data("regid");

        $.post("/Activity/UpdatePlayerParticipationPaid/" + activityid, { regid: regId, value: this.value }, function () {
            RecalculateRemainForPayment(control);
        });
    });

$(document).on("change",
    "#registrationStatus-table input.membersFeePaidValue",
    function () {
        var control = $(this);
        var activityid = control.data("activity");
        var regId = control.data("regid");

        $.post("/Activity/UpdateMembersFeePaid/" + activityid, { regid: regId, value: this.value }, function () {
            RecalculateRemainForPayment(control);
        });
    });

$(document).on("change",
    "#registrationStatus-table input.handlingFeePaidValue",
    function () {
        var control = $(this);
        var activityid = control.data("activity");
        var regId = control.data("regid");

        $.post("/Activity/UpdateHandlingFeePaid/" + activityid, { regid: regId, value: this.value }, function () {
            RecalculateRemainForPayment(control);
        });
    });

$(document).on("change",
    "#registrationStatus-table input.tenicardPaidValue",
    function () {
        var control = $(this);
        var activityid = control.data("activity");
        var regId = control.data("regid");

        $.post("/Activity/UpdateTenicardPaid/" + activityid, { regid: regId, value: this.value }, function () {
            RecalculateRemainForPayment(control);
        });
    });

$(document).on("change",
    "#registrationStatus-table input.customPricePaidValue",
    function () {
        var control = $(this);
        var activityid = control.data("activity");
        var regId = control.data("regid");
        var property = control.data("property");

        $.post("/Activity/UpdateCustomPricePaid/" + activityid,
            { regid: regId, value: this.value, property: property }, function () {
                RecalculateRemainForPayment(control);
            });
    });

function RecalculateRemainForPayment(paidField) {
    var activityid = paidField.data("activity");
    var regId = paidField.data("regid");

    var remainField = paidField.closest("tr").find("td.remainPayment");

    remainField.fadeOut(250,
        function () {
            $(this).text("").fadeIn(250);
        });

    $.get("/Activity/GetRemainForPayment/" + activityid,
        { regid: regId },
        function (data) {
            remainField.fadeOut(250,
                function () {
                    $(this).text(data).fadeIn(250);
                });
        });

    //var remain = 0.0;

    //var registrationPrice = paidField.closest("tr").find("td.price");
    //var registrationPaid = paidField.closest("tr").find("td input.registrationPaidValue");

    //var insurancePrice = paidField.closest("tr").find("td.insurancePrice");
    //var insurancePaid = paidField.closest("tr").find("td input.insurancePaidValue");

    //var participationPrice = paidField.closest("tr").find("td.participationPrice");
    //var participationPaid = paidField.closest("tr").find("td input.participationPaidValue");

    //var membersFee = paidField.closest("tr").find("td.membersFee");
    //var membersFeePaid = paidField.closest("tr").find("td input.membersFeePaidValue");

    //var handlingFee = paidField.closest("tr").find("td.handlingFee");
    //var handlingFeePaid = paidField.closest("tr").find("td input.handlingFeePaidValue");

    //var customPricesTotal = 0.0;
    //paidField.closest("tr").find("td.customPrice-total").each(function() {
    //    customPricesTotal += parseFloat($(this).text());
    //});
    //var customPricesPaidTotal = 0.0;
    //paidField.closest("tr").find("td input.customPricePaidValue").each(function() {
    //    customPricesPaidTotal += parseFloat($(this).val());
    //});

    //if (registrationPrice.length && registrationPaid.length) {
    //    remain = remain + (parseFloat(registrationPrice.text()) - parseFloat(registrationPaid.val()));
    //}
    //if (insurancePrice.length && insurancePaid.length && !insurancePrice.hasClass("payment-refused")) {
    //    remain = remain + (parseFloat(insurancePrice.text()) - parseFloat(insurancePaid.val()));
    //}
    //if (participationPrice.length && participationPaid.length) {
    //    remain = remain + (parseFloat(participationPrice.text()) - parseFloat(participationPaid.val()));
    //}
    //if (membersFee.length && membersFeePaid.length && !membersFee.hasClass("payment-refused")) {
    //    remain = remain + (parseFloat(membersFee.text()) - parseFloat(membersFeePaid.val()));
    //}
    //if (handlingFee.length && handlingFeePaid.length && !handlingFee.hasClass("payment-refused")) {
    //    remain = remain + (parseFloat(handlingFee.text()) - parseFloat(handlingFeePaid.val()));
    //}

    //remain = remain + (customPricesTotal - customPricesPaidTotal);

    //var remainField = paidField.closest("tr").find("td.remainPayment");

    //remainField.fadeOut(250, function () {
    //    $(this).text(remain).fadeIn(250);
    //});
}

$(document).on("change",
    "#registrationStatus-table textarea.union-comment",
    function () {
        var activityid = $(this).data("activity");
        var regId = $(this).data("regid");

        $.post("/Activity/UpdateUnionComment/" + activityid, { regid: regId, value: this.value });
    });
$(document).on("change",
    "#registrationStatus-table textarea.club-comment",
    function () {
        var activityid = $(this).data("activity");
        var regId = $(this).data("regid");

        $.post("/Activity/UpdateClubComment/" + activityid, { regid: regId, value: this.value });
    });

$(document).on("show.bs.modal",
    "#deleteRegistrationModal",
    function (event) {
        var button = $(event.relatedTarget);
        var regId = button.data("regid");

        $(this).find("#deleteRegistrationButton").data("regid", regId);
    });

$(document).on("click",
    "#deleteRegistrationModal #deleteRegistrationButton",
    function () {
        var btn = $(this);

        btn.prop("disabled", true);

        var regId = btn.data("regid");
        var activityId = btn.data("activity");

        var modalWindow = btn.closest("#deleteRegistrationModal");

        var tableRow = $("tr td button.glyphicon-trash[data-regid='" + regId + "']").closest("tr");

        var table = $("#registrationStatus-table").DataTable();

        $.ajax({
            type: "POST",
            url: "/Activity/DeleteRegistration?activityId=" + activityId + "&regId=" + regId,
            success: function () {
                table.row(tableRow).remove().draw();
                modalWindow.modal("hide");
                btn.prop("disabled", false);
            }
        });
    });

//$("#registrationStatusBody").on("click",
//    "a#show-all",
//    function () {
//        var activityId = $(this).data("activity");

//        showAll = true;

//        $("#registrationStatusBody").html(loader);

//        LoadRegistrations(activityId);
//    });

$(document).on("click",
    "#registrationStatus-table .activity-unlock-player",
    function () {
        var $that = $(this);
        var id = $that.data("id");

        $.ajax({
            type: "GET",
            url: "/TeamPlayers/UnlockPlayers/",
            data: { id: id },
            success: function (data) {
                if (data.result) {
                    if (data.value == true) {
                        //$that.parent().find(".unlock-link-color").addClass("text-danger");
                        $that.html('<i class="fa fa-lock"></i>');
                    } else {
                        //$that.parent().find(".unlock-link-color").removeClass("text-danger");
                        $that.html('<i class="fa fa-unlock"></i>');
                    }
                } else {
                    alert(data.message);
                }
            },
            error: function (xhr, ajaxOptions, thrownError) {
                alert(xhr.status);
                alert(thrownError);
            }
        });
    });

$(document).on("change",
    "#registrationStatus-table .replaceCustomFile",
    function () {
        var $fileControl = $(this);
        var fileControl = this;

        var propertyName = $fileControl.data("propertyname");
        var regId = $fileControl.data("regid");

        var formData = new FormData();
        formData.append("file", fileControl.files[0]);
        formData.append("propertyName", propertyName);
        formData.append("regId", regId);

        $.ajax({
            url: "/Activity/StatusReplaceFile",
            type: "POST",
            processData: false, // important
            contentType: false, // important
            dataType: "json",
            data: formData,
            success: function (data) {
                $fileControl.val("");

                if (data.error) {
                    alert(data.error);
                    return;
                }

                var fileLink = $("#customFile-" + regId + "-" + propertyName);
                fileLink.attr("href", data.result);
                fileLink.show();
                fileLink.parent().effect("highlight", {}, 2000);
            }
        });
    });