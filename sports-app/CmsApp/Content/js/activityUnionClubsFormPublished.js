var busyIndicator = $(".loader-overlay");

function LoadPlayer(identity) {
    var activityId = $("#buildForm").data("activity");

    busyIndicator.show();
    $.get("/Activity/GetUnionClubUser/?idNum=" + identity + "&activityId=" + activityId,
        function(data) {
            busyIndicator.hide();

            if (data.error) {
                LockForm();
                alert(data.error);
                location.reload();
                return;
            }

            if (data.restartPayment) {
                LockForm();

                RestartPayment(data);

                return;
            }

            if (data) {
                var fullNameControl = $("#buildForm-body-playerFullName");
                if (fullNameControl.length) {
                    fullNameControl.val(data.FullName);
                }
                var firstNameControl = $("#buildForm-body-playerFirstName");
                if (firstNameControl.length) {
                    firstNameControl.val(data.FirstName);
                }
                var lastNameControl = $("#buildForm-body-playerLastName");
                if (lastNameControl.length) {
                    lastNameControl.val(data.LastName);
                }
                var middleNameControl = $("#buildForm-body-playerMiddleName");
                if (middleNameControl.length) {
                    middleNameControl.val(data.MiddleName);
                }

                $("#buildForm-body-playerEmail").val(data.Email);
                $("#buildForm-body-playerPhone").val(data.Phone);
                $("#buildForm-body-playerAddress").val(data.Address);
                $("#buildForm-body-playerBirthDate").val(data.BirthDate);

                if (data.DisableRegPaymentForExistingClubs && data.Clubs.length) {
                    ResetAndLockCustomPrices();
                }

                var sportCentersDropdown = $("#buildForm-body-clubSportsCenter");
                var regionsDropdown = $("#buildForm-body-region");

                $.each(data.SportCenters,
                    function(index, value) {
                        sportCentersDropdown.append("<option value='" + value.Id + "'>" + value.Caption + "</option>");
                    });

                $.each(data.Regions,
                    function(index, value) {
                        regionsDropdown.append("<option value='" + value.Id + "'>" + value.Name + "</option>");
                    });

                if (data.Clubs.length > 1) {
                    var modalBody = $("#selectClubModal .modal-body ul.nav");

                    $.each(data.Clubs,
                        function(index, value) {
                            modalBody.append('<li data-clubid="' +
                                value.Id +
                                '" data-name="' +
                                encodeURI(value.Name) +
                                '" data-numberofcourts="' +
                                value.NumberOfCourts +
                                '" data-ngonumber="' +
                                value.NGONumber +
                                '" data-sportscenter="' +
                                value.SportsCenter +
                                '" data-address="' +
                                encodeURI(value.Address) +
                                '" data-phone="' +
                                value.Phone +
                                '" data-email="' +
                                value.Email +
                                '"><a href="#">' +
                                value.Name +
                                '</a></li>');
                        });

                    $("#selectClubModal").modal("show");
                } else if (data.Clubs.length) {
                    SetClubData(data.Clubs[0].Id,
                        data.Clubs[0].Name,
                        data.Clubs[0].NumberOfCourts,
                        data.Clubs[0].NGONumber,
                        data.Clubs[0].SportsCenter,
                        data.Clubs[0].Address,
                        data.Clubs[0].Phone,
                        data.Clubs[0].Email);
                }

                UnlockFormInput();

                $("#buildForm form").valid();
            }
        });
}

function ResetAndLockCustomPrices() {
    $(".customPrice-quantity").each(function (index, element) {
        $(element).val(0);
        $(element).trigger("change");

        $(element).prop("disabled", true);

        $("#submitActivityForm").prop("disabled", false);
        $("#noquantity-warning").hide();
    });
}

function SetClubData(id, name, numberofcourts, ngonumber, sportscenter, address, phone, email) {
    $("#buildForm-body input[name=ClubId]").remove();
    $("#buildForm-body-playerID").after("<input type=\"hidden\" name=\"ClubId\" value=\"" + id + "\">");

    $("#buildForm-body-clubName").val(name);
    $("#buildForm-body-clubNumberOfCourts").val(numberofcourts);
    $("#buildForm-body-clubNgoNumber").val(ngonumber);
    $("#buildForm-body-clubSportsCenter").val(sportscenter);
    $("#buildForm-body-clubAddress").val(address);
    $("#buildForm-body-clubPhone").val(phone);
    $("#buildForm-body-clubEmail").val(email);

    $("#buildForm form").valid();
}

$("#selectClubModal .modal-body ul.nav").on("click",
    "li",
    function () {
        SetClubData(
            $(this).data("clubid"),
            decodeURI($(this).data("name")),
            $(this).data("numberofcourts"),
            $(this).data("ngonumber"),
            $(this).data("sportscenter"),
            decodeURI($(this).data("address")),
            $(this).data("phone"),
            $(this).data("email"));

        $("#selectClubModal").modal("hide");
        $("#selectClubModal .modal-body ul.nav").empty();
    });

$("#selectClubModal .modal-body ul.nav").on("click",
    "a",
    function (e) {
        e.preventDefault();
    });

$("#buildForm-body-paymentByBenefactor-select").on("change", function () {
    HandleBenefactorPrices();
});

function HandleBenefactorPrices() {
    var selectedValue = $("#buildForm-body-paymentByBenefactor-select").val();

    var customPrices = $(".customPrice-price");

    if (selectedValue === "true") {
        customPrices.each(function (index, item) {
            $(item).data("price", $(item).val());
            $(item).val(0);
        });
    } else {
        customPrices.each(function (index, item) {
            $(item).val($(item).data("price"));
        });
    }

    $(".control select.customPrice-quantity").trigger("change"); //workaround to update "Total" value due to hidden/restored prices of benefactor selection
}