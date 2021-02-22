var busyIndicator = $(".loader-overlay");
var activityId = $("#buildForm").data("activity");

var birthDateField = $("#buildForm-body-playerBirthDate");

var regionId = 0;

function LoadPlayer(identity) {
    busyIndicator.show();
    $.get("/Activity/GetUnionPersonalClubUser/?idNum=" + identity +
        "&activityId=" + activityId +
        "&regionId=" + regionId +
        "&birthDate=" + birthDateField.val(),
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
                if (data.BirthdateNeeded) {
                    birthDateField.datetimepicker({
                        dayOfWeekStart: window.Resources.Messages.DateTimePicker_StartOfWeek,
                        onSelectDate: function (current, input) {
                            LoadPlayer(identity);
                        }
                    });

                    alert(window.Resources.Messages.Activity_Club_SelectBirthDateFirst);
                    birthDateField.prop("disabled", false);
                    birthDateField.focus();
                    return;
                }

                var regionControl = $("#buildForm-body-playerRegion");
                if (regionControl.length && !data.IsFilteredByRegion) {
                    regionControl.prop("disabled", false);

                    regionControl.find("option").not(":first").remove();
                    $.each(data.Regions,
                        function (index, value) {
                            regionControl.append("<option value='" + value.Id + "'>" + value.Name + "</option>");
                        });

                    return;
                }

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

                var motherNameControl = $("#buildForm-body-playerMotherName");
                if (motherNameControl.length) {
                    motherNameControl.val(data.MotherName);
                }
                var parentPhoneControl = $("#buildForm-body-playerParentPhone");
                if (parentPhoneControl.length) {
                    parentPhoneControl.val(data.ParentPhone);
                }
                var identCardControl = $("#buildForm-body-playerIdentCard");
                if (identCardControl.length) {
                    identCardControl.val(data.IdentCard);
                }
                var licenseDateControl = $("#buildForm-body-playerLicenseDate");
                if (licenseDateControl.length) {
                    licenseDateControl.val(data.LicenseDate);
                }
                var parentEmailControl = $("#buildForm-body-playerParentEmail");
                if (parentEmailControl.length) {
                    parentEmailControl.val(data.ParentEmail);
                }
                var parentNameControl = $("#buildForm-body-playerParentName");
                if (parentNameControl.length) {
                    parentNameControl.val(data.ParentName);
                }

                var postalCodeControl = $("#buildForm-body-playerPostalCode");
                if (postalCodeControl.length) {
                    postalCodeControl.val(data.PostalCode);
                }
                var passportNumControl = $("#buildForm-body-playerPassportNumber");
                if (passportNumControl.length) {
                    passportNumControl.val(data.PassportNum);
                }
                var foreignFirstNameControl = $("#buildForm-body-playerForeignFirstName");
                if (foreignFirstNameControl.length) {
                    foreignFirstNameControl.val(data.ForeignFirstName);
                }
                var foreignLastNameControl = $("#buildForm-body-playerForeignLastName");
                if (foreignLastNameControl.length) {
                    foreignLastNameControl.val(data.ForeignLastName);
                }

                $("#buildForm-body-playerGender").val(data.Gender);
                $("#buildForm-body-playerEmail").val(data.Email);
                $("#buildForm-body-playerPhone").val(data.Phone);
                $("#buildForm-body-playerCity").val(data.City);
                $("#buildForm-body-playerAddress").val(data.Address);

                $("#buildForm-body-playerBirthDate").val(data.BirthDate);
                //if (data.Id === 0) {
                //    $("#buildForm-body-playerBirthDate").prop("disabled", false);
                //    $("#buildForm-body-playerBirthDate").prop("readonly", false);
                //} else {
                //    $("#buildForm-body-playerBirthDate").prop("disabled", true);
                //    $("#buildForm-body-playerBirthDate").prop("readonly", true);
                //    $("#buildForm-body-playerBirthDate-error").empty();
                //    $("#buildForm form").valid();
                //}
                $("#buildForm-body-playerBirthDate").datetimepicker({
                    dayOfWeekStart: window.Resources.Messages.DateTimePicker_StartOfWeek,
                    format: "d/m/Y",
                    closeOnDateSelect: true,
                    timepicker: false,
                    scrollInput: false
                });

                $("#buildForm-body-playerDateOfMedicalExamination").val(data.MedExamDate);
                $("#buildForm-body-playerDateOfInsuranceValidity").val(data.DateOfInsuranceValidity);
                $("#buildForm-body-playerTenicardValidity").val(data.TenicardValidity);

                $("#buildForm-body-playerRegistrationPrice").data("regular", data.RegularRegistrationPrice);
                $("#buildForm-body-playerRegistrationPrice").data("competitive", data.CompetitiveRegistrationPrice);
                if ($("#buildForm-body-playerRegistrationPrice-competitive").length) {
                    $("#buildForm-body-playerRegistrationPrice").val(data.CompetitiveRegistrationPrice);
                } else {
                    $("#buildForm-body-playerRegistrationPrice").val(data.RegularRegistrationPrice);
                }
                $("#buildForm-body-playerRegistrationPrice-competitive").prop("disabled", false);

                $("#buildForm-body-playerTenicardPrice").val(data.TenicardPrice);
                $("#buildForm-body-playerTenicardPrice").data("price", data.TenicardPrice);
                $("#buildForm-body-playerTenicardPrice-donotpay").prop("disabled", false);

                $("#buildForm-body-playerInsurancePrice").val(data.InsurancePrice);
                $("#buildForm-body-playerInsurancePrice").data("price", data.InsurancePrice);
                $("#buildForm-body-playerInsurancePrice-school").prop("disabled", false);

                var medicalCertControl = $("#buildForm-body-medicalCert");
                var insuranceCertControl = $("#buildForm-body-insuranceCert");
                var playerProfilePictureControl = $("#buildForm-body-playerProfilePicture");
                var idFileControl = $("#buildForm-body-idFile");

                if (data.ProfilePicture && playerProfilePictureControl.length > 0) {
                    playerProfilePictureControl.after($("<a>",
                        {
                            href: data.ProfilePicture,
                            "class": "glyphicon glyphicon-eye-open glyph-btn",
                            target: "_blank"
                        }));

                    playerProfilePictureControl.rules("remove");
                    $("#buildForm-body-playerProfilePicture-error").text("");
                }
                if (data.IdFile && idFileControl.length > 0) {
                    idFileControl.after($("<a>",
                        { href: data.IdFile, "class": "glyphicon glyphicon-eye-open glyph-btn", target: "_blank" }));

                    idFileControl.rules("remove");
                    $("#buildForm-body-idFile-error").text("");
                }
                if (data.MedicalCert && medicalCertControl.length > 0) {
                    medicalCertControl.after($("<a>",
                        {
                            href: data.MedicalCert,
                            "class": "glyphicon glyphicon-eye-open glyph-btn",
                            target: "_blank"
                        }));

                    medicalCertControl.rules("remove");
                    $("#buildForm-body-medicalCert-error").text("");
                }
                if (data.InsuranceCert && insuranceCertControl.length > 0) {
                    insuranceCertControl.after($("<a>",
                        {
                            href: data.InsuranceCert,
                            "class": "glyphicon glyphicon-eye-open glyph-btn",
                            target: "_blank"
                        }));

                    insuranceCertControl.rules("remove");
                    $("#buildForm-body-insuranceCert-error").text("");
                }

                var clubsDropdown = $("#buildForm-body-playerClub");

                clubsDropdown.find("option").not(":first").remove();
                clubsDropdown.prop("disabled", false);
                $.each(data.Clubs,
                    function(index, value) {
                        clubsDropdown.append("<option value='" + value.Id + "'>" + value.Name + "</option>");
                    });

                if (data.Clubs.length === 1) {
                    clubsDropdown.find("option:last").prop("selected", true);
                    clubsDropdown.trigger("change");
                    clubsDropdown.prop("disabled", true);
                    clubsDropdown.rules("remove");
                    $("#buildForm-body-playerClub-error").text("");

                    SetClubData(data.Clubs[0].Id);
                }

                if (data.CurrentClubId) {
                    clubsDropdown.val(data.CurrentClubId);
                    clubsDropdown.data("teamId", data.CurrentTeamId);
                    clubsDropdown.trigger("change");
                }

                UnlockFormInput();

                $("#buildForm form").valid();
            }
        });
}

$("#buildForm-body-playerRegion").on("change",
    function() {
        regionId = $(this).val();

        var playerId = $("#buildForm-body-playerID").val();

        LoadPlayer(playerId);
    });

$("#buildForm-body-playerInsurancePrice-school").on("change",
    function () {
        var priceControl = $("#buildForm-body-playerInsurancePrice");

        if (this.checked) {
            priceControl.val("");
        } else {
            priceControl.val(priceControl.data("price"));
        }
    });

$("#buildForm-body-playerRegistrationPrice-competitive").on("change",
    function () {
        var priceControl = $("#buildForm-body-playerRegistrationPrice");

        if (this.checked) {
            priceControl.val(priceControl.data("competitive"));
        } else {
            priceControl.val(priceControl.data("regular"));
        }
    });

$("#buildForm-body-playerTenicardPrice-donotpay").on("change",
    function () {
        var priceControl = $("#buildForm-body-playerTenicardPrice");

        if (this.checked) {
            priceControl.val("");
        } else {
            priceControl.val(priceControl.data("price"));
        }
    });

$("#buildForm-body-playerClub").on("change",
    function () {
        var optionSelected = $("option:selected", this);
        var clubId = $(this).val();

        SetClubData(clubId);

        busyIndicator.show();
        $.get("/Activity/GetClubTeams/?clubId=" + clubId + "&activityId=" + activityId,
            function(data) {
                busyIndicator.hide();

                if (data.error) {
                    LockForm();
                    alert(data.error);
                    return;
                }

                if (data) {
                    if (data.MultiTeamEnabled === true) {
                        ProcessTeamsMultiSelect(data.Teams);
                    } else {
                        var teamsDropdown = $("#buildForm-body-playerTeam");

                        teamsDropdown.find("option").not(":first").remove();
                        teamsDropdown.prop("disabled", false);
                        $.each(data.Teams,
                            function(index, value) {
                                teamsDropdown.append("<option value='" + value.Id + "'>" + value.Name + "</option>");
                            });

                        if (data.Teams.length === 1) {
                            teamsDropdown.find("option:last").prop("selected", true);
                            teamsDropdown.prop("disabled", true);
                            teamsDropdown.rules("remove");
                            $("#buildForm-body-playerTeam-error").text("");

                            SetTeamData(data.Teams[0].Id);
                        }

                        var clubsDropDown = $("#buildForm-body-playerClub");
                        if (clubsDropDown.data("teamId") > 0) {
                            teamsDropdown.val(clubsDropDown.data("teamId"));
                            teamsDropdown.trigger("change");

                            clubsDropDown.data("teamId", 0);
                        }
                    }

                    $("#buildForm form").valid();
                }
            });
    });

function ProcessTeamsMultiSelect(teams) {
    var teamsDropdown = $("#buildForm-body-playerTeamMultiple");

    //teamsDropdown.find("option:not([value=''], :selected)").remove().end();
    teamsDropdown.find("option:not([value=''])").remove().end();
    $.each(teams,
        function (index, item) {

            //var existingItem = teamsDropdown.find("option[value='" + item.TeamId + "']");

            //if (existingItem.length <= 0) {
                teamsDropdown.append(
                    $("<option>", {
                        value: item.Id,
                        text: $("<div/>").html(item.Name).text(),
                        //"data-teamname": item.TeamName,
                        //"data-school": item.SchoolId,
                        //"data-regandequipprice": item.RegistrationAndEquipmentPrice,
                        //"data-participationprice": item.ParticipationPrice,
                        //"data-insuranceprice": item.InsurancePrice
                    }));
            //}
        });

    teamsDropdown.multiselect("destroy"); //overriding multiselect parameters
    teamsDropdown.multiselect({
        onChange: function (option, checked, select) {
            //var registrationAndEquipmentPrice = parseFloat($(option).data("regandequipprice")) || 0.0;
            //var participationPrice = parseFloat($(option).data("participationprice")) || 0.0;
            //var insurancePrice = parseFloat($(option).data("insuranceprice")) || 0.0;

            //var currentRegPrice = parseFloat($("#buildForm-body-playerRegistrationPrice").val()) || 0.0;
            //var currentParticipationPrice = parseFloat($("#buildForm-body-playerParticipationPrice").val()) || 0.0;
            //var currentInsurancePrice = parseFloat($("#buildForm-body-playerInsurancePrice").val()) || 0.0;

            if (checked) {
                //$("#buildForm-body-playerRegistrationPrice").val(currentRegPrice + registrationAndEquipmentPrice);
                //$("#buildForm-body-playerParticipationPrice").val(currentParticipationPrice + participationPrice);
                //$("#buildForm-body-playerInsurancePrice").val(currentInsurancePrice + insurancePrice);
            } else {
                //$("#buildForm-body-playerRegistrationPrice").val(currentRegPrice - registrationAndEquipmentPrice);
                //$("#buildForm-body-playerParticipationPrice").val(currentParticipationPrice - participationPrice);
                //$("#buildForm-body-playerInsurancePrice").val(currentInsurancePrice - insurancePrice);
            }

            $("#buildForm form").valid();
        }
    });

    //select first team if only one was returned
    if (teamsDropdown.find("option").length === 2) {
        //teamsDropdown.find("option:last").prop("selected", true);
        teamsDropdown.multiselect("select", teamsDropdown.find("option:last").val(), true);
        teamsDropdown.trigger("change");
        $("#buildForm form").valid();
    }
}

$("#buildForm-body-playerTeam").on("change",
    function() {
        var optionSelected = $("option:selected", this);
        var teamId = $(this).val();

        SetTeamData(teamId);
    });

function SetClubData(clubId) {
    $("#buildForm-body input[name=ClubId]").remove();
    $("#buildForm-body-playerClub").after("<input type=\"hidden\" name=\"ClubId\" value=\"" + clubId + "\">");
}

function SetTeamData(teamId) {
    $("#buildForm-body input[name=TeamId]").remove();
    $("#buildForm-body-playerTeam").after("<input type=\"hidden\" name=\"TeamId\" value=\"" + teamId + "\">");
}

$("#buildForm-body-paymentByBenefactor-select").on("change", function () {
    HandleBenefactorPrices(true);
});

function HandleBenefactorPrices(restorePrices) {
    var selectedValue = $("#buildForm-body-paymentByBenefactor-select").val();
    var registrationPriceField = $("#buildForm-body-playerRegistrationPrice");
    var insurancePriceField = $("#buildForm-body-playerInsurancePrice");

    if (selectedValue === "true") {
        registrationPriceField.data("registrationprice", registrationPriceField.val());
        registrationPriceField.val(0);

        insurancePriceField.data("insuranceprice", insurancePriceField.val());
        insurancePriceField.val(0);

    } else if (restorePrices) {
        registrationPriceField.val(registrationPriceField.data("registrationprice"));
        insurancePriceField.val(insurancePriceField.data("insuranceprice"));
    }
}