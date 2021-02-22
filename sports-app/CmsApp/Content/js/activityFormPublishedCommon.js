$(document).ready(function() {
    $("#buildForm form").valid();

    var playerIdField = $("#buildForm-body-playerID");

    playerIdField.focus();

    $(".control select[multiple]").multiselect({
        onChange: function () {
            $("#buildForm form").valid();
        }
    });

    var buildForm = $("#buildForm");

    var noRestrictions = buildForm.data("donotrestrictcustomprices") === "True";
    if (!noRestrictions) {
        var quantityDropdowns = $(".customPrice-quantity");
        var quantityDropdownsFiltered = quantityDropdowns.filter(function() { return parseInt($(this).val()) > 0; });
        if (quantityDropdowns.length > 0 && quantityDropdownsFiltered.length === 0) {
            $("#submitActivityForm").prop("disabled", true);
            $("#noquantity-warning").show();
        }
    }

    if (buildForm.data("alternativeplayeridentity") === "True") {
        playerIdField.data("manual", true);

        playerIdField.after($("<button/>",
                {
                    id: "search-player",
                    "class": "btn btn-default",
                    text: window.Resources.Messages.Search,
                    type: "button"
                })
            .prepend($("<i/>", { "class": "glyphicon glyphicon-search" }))
        );

        LockFormInput();
    } else {
        $("#buildForm-body-playerID").rules("add",
            {
                minlength: 9,
                maxlength: 9,
                messages: {
                    minlength: window.Resources.Messages.Activity_BuildForm_IdNum9CharsRequired,
                    maxlength: window.Resources.Messages.Activity_BuildForm_IdNum9CharsRequired
                }
            });
    }

    $.validator.addMethod("minimumSelection", function (value, element, minimum) { //minimum selection rule for multiselect comboboxes
        return $(element).find("option:selected").length >= minimum;
    }, $.validator.format(window.Resources.Messages.Activity_BuildForm_MinimumSelectionValidation));

    $("#buildForm-body-playerBrotherIdForDiscount").prop("disabled", true);

    //Trigger custom price controls to recalculate Total price, useful in case of default quantity > 0
    $(".control select.customPrice-quantity").trigger("change");
});

function LockForm() {
    $("#buildForm-body :input").prop("disabled", true);
    $("#submitActivityForm").remove();
}

var lockedControls = {};
function LockFormInput() {
    var controls = $("#buildForm-body :input:enabled:not(#buildForm-body-playerID):not(#search-player)");

    lockedControls = controls;
    controls.prop("disabled", true);

    $("#submitActivityForm").prop("disabled", true);
}
function UnlockFormInput() {
    if (lockedControls && lockedControls.length) {
        lockedControls.prop("disabled", false);
        $("#submitActivityForm").prop("disabled", false);
    }
}

$("#buildForm-body").on("click",
    "#search-player",
    function() {
        var playerId = $("#buildForm-body-playerID").val();

        LoadPlayer(playerId);
    });

$("#buildForm-body").on("input",
    "#buildForm-body-playerID",
    function() {
        if ($(this).data("manual") === true) {
            return;
        }

        var id = this.value;

        if (id.length === 9) {
            LoadPlayer(id);
        }
    });

$("#buildForm-body").on("input",
    "#buildForm-body-playerBrotherIdForDiscount",
    function () {
        var brotherIdControl = this;
        var id = brotherIdControl.value;

        if (id.length === 9) {

            var activityId = $("#buildForm").data("activity");

            $.get("/Activity/CheckBrotherRegistration?activityId=" + activityId + "&brotherIdNum=" + id)
                .done(function () {
                    $(brotherIdControl).data("brother-found", true);
                    $(brotherIdControl).prop("disabled", true);

                    LoadPlayer($("#buildForm-body-playerID").val());
                })
                .fail(function() {
                    alert(window.Resources.Messages.Activity_Brother_NotFound);

                    $(brotherIdControl).val("");
                });
        }
    });

$("#buildForm-body").on("keypress",
    "#buildForm-body-playerID",
    function (e) {
        if (e.which === 13/*'enter' key*/ && $(this).data("manual") === true) {
            LoadPlayer(this.value);
        }
    });

$("#buildForm-body").on("change", ".control select.customPrice-quantity",
    function () {
        var quantity = $(this).val();
        var price = $(this).closest(".control").find(".customPrice-price").val();

        $(this).closest(".control").find(".customPrice-total").val(price * quantity);
    });

$("#buildForm form").on("submit",
    function (e) {
        if ($(this).valid()) {
            $("#submitActivityForm").prop("disabled", true);
            $("#buildForm-body-playerBrotherIdForDiscount").prop("disabled", false);

            var customFields = [];
            $(".custom-control").each(function () {
                var currentControl = $(this);

                var controlValue = "";
                switch (currentControl.data("type")) {
                case "CustomTextBox":
                    controlValue = currentControl.find("input[name='" + currentControl.data("propertyname") + "']")
                        .val();
                    break;

                case "CustomCheckBox":
                    controlValue = currentControl.find("input[name='" + currentControl.data("propertyname") + "']")
                        .is(":checked");
                    break;
                case "CustomTextArea":
                    controlValue =
                        currentControl.find("textarea[name='" + currentControl.data("propertyname") + "']").val();
                    break;
                case "CustomDropdown":
                    controlValue =
                        currentControl.find("select[name='" + currentControl.data("propertyname") + "']").val();
                    break;
                case "CustomDropdownMultiselect":
                    controlValue =
                        currentControl
                        .find("select[multiple][name='" + currentControl.data("propertyname") + "']")
                        .find("option:selected")
                        .map(function (a, item) { return item.value; })
                        .get()
                        .join();
                    break;

                case "CustomPrice":
                    controlValue =
                        currentControl.find("select[name='" +
                            currentControl.data("propertyname") + "-quantity']").val();
                    break;

                case "CustomFileUpload":
                    controlValue =
                        currentControl.find("input[name='" + currentControl.data("propertyname") + "']").val();
                    break;
                }

                customFields.push({
                    propertyname: currentControl.data("propertyname"),
                    type: currentControl.data("type"),
                    value: controlValue
                });
            });

            $("<input>", { type: "hidden", name: "customfields", value: JSON.stringify(customFields) }).appendTo(this);
        } else {
            e.preventDefault();
        }
    });

$("#buildForm form input").on("keypress", function (e) { //prevent submit of form on enter key
    if (e.which === 13) {
        e.preventDefault();
    }
});

$(".customPrice-quantity").on("change",
    function () {
        var noRestrictions = $("#buildForm").data("donotrestrictcustomprices") === "True";

        if (!noRestrictions) {
            var quantityRestricted = $("#buildForm").data("restrictcustompricestoone") === "True";
            var quantityDropdowns = $(".customPrice-quantity").filter(function () { return parseInt($(this).val()) > 0; });

            if (quantityDropdowns.length === 0) {
                $("#submitActivityForm").prop("disabled", true);
                $("#noquantity-warning").show();
                return;
            }

            if (quantityRestricted && quantityDropdowns.length > 1) {
                $("#submitActivityForm").prop("disabled", true);
                $("#maxquantity-warning").show();
                return;
            }
        }
        
        $("#submitActivityForm").prop("disabled", false);
        $("#noquantity-warning").hide();
        $("#maxquantity-warning").hide();
    });

function RestartPayment(data) {
    var modal = $("#restartPaymentModal");

    var modalBody = modal.find(".modal-body");
    modalBody.html(modalBody.html().replace("{date}", data.regDate));

    modalBody.find("#restart-payment-link").prop("href", "/Activity/RestartPayment/" + data.regId);

    modal.modal("show");
}

function GetFixedNumber(value) {
    return value.toFixed(2);
}

function DisablePlayerNameFields() {
    $("#buildForm form").valid();

    $("#buildForm-body-playerFullName").prop("disabled", true);
    $("#buildForm-body-playerFirstName").prop("disabled", true);
    $("#buildForm-body-playerLastName").prop("disabled", true);
    $("#buildForm-body-playerMiddleName").prop("disabled", true);
}