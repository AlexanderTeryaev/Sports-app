var formLoader = "";

$(document).ready(function () {
    //$(document).on("change", ".btn-file :file", function () {
    //    var input = $(this),
    //        label = input.val().replace(/\\/g, "/").replace(/.*\//, "");
    //    input.trigger("fileselect", [label]);
    //});
    function readURL(input) {
        if (input.files && input.files[0]) {
            var reader = new FileReader();

            reader.onload = function (e) {
                $(".activity-build-form .header #img-upload").attr("src", e.target.result);
            }

            reader.readAsDataURL(input.files[0]);
        }
    }

    $(".activity-build-form .header #formImage").change(function () {
        readURL(this);
    });
});

function InitBootstrapSwitch() {
    $("#buildForm-body .isRequiredChk").bootstrapSwitch("destroy");
    $("#buildForm-body .isEnabledChk").bootstrapSwitch("destroy");

    $("#buildForm-body .isRequiredChk").bootstrapSwitch();
    $("#buildForm-body .isEnabledChk").bootstrapSwitch();
}

function InitMultiselectDropdowns() {
    $(".control select[multiple]").multiselect("destroy");
    $(".control select[multiple]").multiselect();
}

function LoadForm(activityId) {
    $("#saveActivityForm").data("activity", activityId);
    $("#resetActivityForm").data("activity", activityId);

    $.get("/Activity/GetActivityForm/" + activityId,
        function (data) {
            formLoader = $("#buildForm-body").html();

            $("#buildForm-body").html(data.FormContent);
            $("#buildForm-header #activityName").val(data.FormName);
            $("#buildForm-header #activityDescription").val(data.FormDescription);
            $("#buildForm-header #activityEndDate").text(data.EndDate);

            //$("#buildForm-body .isRequiredChk").bootstrapSwitch();
            //$("#buildForm-body .isEnabledChk").bootstrapSwitch();
            InitBootstrapSwitch();

            InitMultiselectDropdowns();

            $("#buildForm-body").sortable({
                items: ".control"
            });

            $("#custom-controls .controls .control").draggable({
                connectToSortable: "#buildForm-body",
                helper: "clone",
                scroll: true,
                appendTo: $("#buildForm-body"),
                stop: function (event, ui) {
                    $.get("/Activity/GetCustomControlDefinition?customControlsCount=" + $("div.control.custom-control").length +
                        "&controlType=" + $(ui.helper[0]).data("controltype"),
                        function (controlData) {
                            $(ui.helper[0]).html(controlData);
                            InitBootstrapSwitch();
                            InitMultiselectDropdowns();
                        });
                }
            });

            $(".date-input").datetimepicker({
                dayOfWeekStart: window.Resources.Messages.DateTimePicker_StartOfWeek,
                format: "d/m/Y",
                closeOnDateSelect: true,
                timepicker: false,
                scrollInput: false
            });

            setTimeout(function() {
                    $("#buildForm-body .isRequiredChk").each(function() {
                        $(this).bootstrapSwitch("state",
                            $(this).closest(".control").data("isrequired") === "True",
                            true);
                    });
                    $("#buildForm-body .isEnabledChk").each(function() {
                        $(this).bootstrapSwitch("state",
                            !($(this).closest(".control").data("isdisabled") === "True"),
                            true);
                    });
                },
                500);

            $("#activityFormModal").modal("handleUpdate");
        });

    $.get("/Activity/GetActivityFormImage/" + activityId,
        function (data) {
            $("#img-upload").attr("src", data);
        });
}

$(".openActivityForm").click(function() {
    var activityId = $(this).data("activity");

    LoadForm(activityId);
});

$("#buildForm-body").on("click", ".remove-custom-control",
    function () {
        $(this).parents(".control.custom-control").remove();
    });

$("#buildForm-body").on("click", ".edit-custom-control",
    function () {
        $(this).parent().find(".modal").modal("show");

        $("form#testForm") //reset validation definition
            .removeData("validator") /* added by the raw jquery.validate plugin */
            .removeData("unobtrusiveValidation");  /* added by the jquery unobtrusive plugin*/

        $.validator.unobtrusive.parse($("form#testForm .control .modal"));
    });

$("#buildForm-body").on("click", ".custom-field-options .modal .save-custom-field-options",
    function () {
        var activityForm = $("form#testForm");
        var optionsValid = activityForm.valid();

        if (!optionsValid) {
            $(activityForm.validate().errorList[0].element).focus();
            return;
        }

        var field = $(this).data("field");
        var modal = $(this).closest(".modal");

        var control = $("#buildForm-body .control[data-propertyname=" + field + "]");
        var controlLabel = control.find("label[for=buildForm-body-" + field + "]");

        var properties = modal.find(".modal-body");

        control.data("labeltexten", properties.find("#label-en").val());
        control.data("labeltextheb", properties.find("#label-he").val());
        control.data("labeltextuk", properties.find("#label-uk").val());

        var dropdownAllowedValues = properties.find("#customDropdown-allowedValues");
        if (dropdownAllowedValues.length) {
            var lines = dropdownAllowedValues.val().split("\n");

            var allowedValues = [];

            control.find("select").find("option:not([value=''])").remove().end();

            $.each(lines, function(index, item) {
                allowedValues.push(item);

                control.find("select").append($("<option>", {value: item, text: item}));
            });

            control.data("customdropdown-allowedvalues", JSON.stringify(allowedValues));
        }

        var culture = $(this).data("culture");
        switch (culture) {
            case "En_US":
                controlLabel.text(properties.find("#label-en").val() + ":");
                break;

            case "He_IL":
                controlLabel.text(properties.find("#label-he").val() + ":");
                break;

            case "Uk_UA":
                controlLabel.text(properties.find("#label-uk").val() + ":");
                break;
        }

        InitMultiselectDropdowns();
        modal.modal("hide");
    });

$(document).on("hidden.bs.modal", ".modal", function () { //SCROLLBAR FIX FOR MODAL OVERLAP
    $(".modal:visible").length && $(document.body).addClass("modal-open");
});

$("#buildForm-body").on("click", ".custom-field-options .modal #close-modal",
    function () {
        $(this).closest(".modal").modal("hide");
    });

function ResetForm() {
    $("#buildForm-body").html(formLoader);

    $("#buildForm-body .isRequiredChk").bootstrapSwitch("destroy");
    $("#buildForm-body .isEnabledChk").bootstrapSwitch("destroy");

    $("#buildForm-header #activityName").val("");
    $("#buildForm-header #activityDescription").val("");
}

$("#activityFormModal").on("hidden.bs.modal",
    function (a) {
        if (a.target.id === "activityFormModal") {
            ResetForm();
        }
    });

$(document).on("change", "#buildForm-body input.fieldDescription",
    function () {
        console.log(this.value);
        $(this).closest(".control").data("fieldnote", this.value);
    });

$(document).on("change", "#buildForm-body .custom-text-control input[type='text']",
    function () {
        console.log(this.value);
        $(this).closest(".control").data("labeltextheb", this.value);
        $(this).closest(".control").data("labeltexten", this.value);
        $(this).closest(".control").data("labeltextuk", this.value);
    });

$(document).on("change", "#buildForm-body .custom-text-control textarea.custom-text-textarea",
    function () {
        console.log(this.value);
        $(this).closest(".control").data("labeltextheb", this.value);
        $(this).closest(".control").data("labeltexten", this.value);
        $(this).closest(".control").data("labeltextuk", this.value);
    });

$("#resetActivityForm").click(function () {
    $("#resetForm-modal").modal("show");
});

$("#activityFormModal").on("shown.bs.modal", ".modal", function () {
    $(".modal").animate({ scrollTop: 0 }, 500, "swing");
});

$("#resetForm-modal #resetForm-modal-confirm").click(function () {
    var activityId = $("#resetActivityForm").data("activity");

    $.post("/Activity/ResetForm/" + activityId,
        function () {
            ResetForm();

            LoadForm(activityId);

            $("#resetForm-modal").modal("hide");
        });
});

$("#saveActivityForm").click(function() {
    var activityId = $(this).data("activity");

    //var editingForm = $("#buildForm-body").html();
    var fields = [];

    $("#buildForm-body div.control").each(function () {
        var currentControl = $(this);

        var dropdownValues = currentControl.data("customdropdown-allowedvalues");
        if (dropdownValues === Object(dropdownValues)) { //Check if it's an object already
            dropdownValues = JSON.stringify(dropdownValues);
        }

        var fieldObject = {
            type: currentControl.data("type"),
            isrequired: currentControl.data("isrequired"),
            isdisabled: currentControl.data("isdisabled"),
            canberequired: currentControl.data("canberequired"),
            canbedisabled: currentControl.data("canbedisabled"),
            labeltexten: currentControl.data("labeltexten"),
            labeltextheb: currentControl.data("labeltextheb"),
            labeltextuk: currentControl.data("labeltextuk"),
            propertyname: currentControl.data("propertyname"),
            isreadonly: currentControl.data("isreadonly"),
            fieldnote: currentControl.data("fieldnote"),
            customdropdownvalues: dropdownValues,
            canberemoved: currentControl.data("canberemoved"),
            hasoptions: currentControl.data("hasoptions"),
            custompriceid: currentControl.data("custompriceid")
        };

        fields.push(fieldObject);
    });

    var imageFormData = new FormData();
    imageFormData.append("formImage", $("#formImage").prop("files")[0]);

    $.post("/Activity/SaveActivityForm/" + activityId,
        {
            formName: $("#buildForm-header #activityName").val(),
            formDescription: $("#buildForm-header #activityDescription").val(),
            fields: fields
        },
        function (data) {
            $.ajax({
                type: "POST",
                url: "/Activity/SaveActivityFormImage/" + activityId,
                data: imageFormData,
                processData: false,
                contentType: false,
                dataType: "json"
            });

            $(".read-only-file").each(function() {
                var currentFile = $(this).prop("files")[0];
                if (currentFile !== undefined) {
                    var formData = new FormData();

                    formData.append("file", currentFile);
                    formData.append("propertyName", $(this).prop("name"));

                    $.ajax({
                        type: "POST",
                        url: "/Activity/SaveActivityFormReadOnlyFile/" + activityId,
                        data: formData,
                        processData: false,
                        contentType: false,
                        dataType: "json"
                    });
                }
            });


            $("#saveResult").text(data);

            if (data === "Success") {
                setTimeout(function () {
                    //$('#activityFormModal').modal('hide');
                    $("#saveResult").text("");
                }, 1500);
            }
        });
});

//$("#buildForm-body").on("change", ".control input:checkbox",
//    function () {
//        $(this).attr("checked", this.checked);
//        alert("s");
//    });

$("#buildForm-body").on("switchChange.bootstrapSwitch", ".control input.isEnabledChk:checkbox",
    function () {
        $(this).closest(".control").data("isdisabled", !this.checked);
    });
$("#buildForm-body").on("switchChange.bootstrapSwitch", ".control input.isRequiredChk:checkbox",
    function () {
        $(this).closest(".control").data("isrequired", this.checked);
    });

$("#buildForm-body").on("change", ".control select.customPrice-quantity",
    function () {
        var quantity = $(this).val();
        var price = $(this).closest(".control").find(".customPrice-price").val();

        $(this).closest(".control").find(".customPrice-total").val(price * quantity);
    });