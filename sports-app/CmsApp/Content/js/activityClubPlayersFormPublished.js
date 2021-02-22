var busyIndicator = $(".loader-overlay");
var newPlayer = false;
var brotherIdValue = "";

function LoadPlayer(identity) {
    var activityId = $("#buildForm").data("activity");

    var birthDateField = $("#buildForm-body-playerBirthDate");

    var brotherIdField = $("#buildForm-body-playerBrotherIdForDiscount");
    if (brotherIdField.length) {
        brotherIdValue = brotherIdField.val();
    }

    var priceAdjustStartDateField = $("#buildForm-body-playerAdjustPricesStartDate");
    
    busyIndicator.show();
    $.get("/Activity/GetClubPlayerUser/?idNum=" + identity +
        "&activityId=" + activityId +
        "&birthDate=" + birthDateField.val() +
        "&brotherIdNum=" + brotherIdValue +
        "&startDateForPriceAdjustment=" + priceAdjustStartDateField.val(),
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

            if (!brotherIdField.data("brother-found")) {
                brotherIdField.prop("disabled", false);
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

                $("#buildForm-body-playerGender").val(data.Gender);
                $("#buildForm-body-playerEmail").val(data.Email);
                $("#buildForm-body-playerPhone").val(data.Phone);
                $("#buildForm-body-playerAddress").val(data.Address);
                $("#buildForm-body-playerCity").val(data.City);
                $("#buildForm-body-playerBirthDate").val(data.BirthDate);
                $("#buildForm-body-playerParentName").val(data.ParentName);

                if (data.ProfilePicture) {
                    $("#buildForm-body-playerProfilePicture").after($("<a>",
                        {
                            href: data.ProfilePicture,
                            "class": "glyphicon glyphicon-eye-open glyph-btn",
                            target: "_blank"
                        }));
                }
                if (data.MedicalCert) {
                    $("#buildForm-body-medicalCert").after($("<a>",
                        {
                            href: data.MedicalCert,
                            "class": "glyphicon glyphicon-eye-open glyph-btn",
                            target: "_blank"
                        }));
                }
                if (data.InsuranceCert) {
                    $("#buildForm-body-insuranceCert").after($("<a>",
                        {
                            href: data.InsuranceCert,
                            "class": "glyphicon glyphicon-eye-open glyph-btn",
                            target: "_blank"
                        }));
                }

                ProcessSchools(data.Schools);

                if (data.Id === 0) {
                    newPlayer = true;

                    $("#buildForm-body-playerBirthDate").datetimepicker({
                        dayOfWeekStart: window.Resources.Messages.DateTimePicker_StartOfWeek,
                        onSelectDate: function(current, input) {
                            busyIndicator.show();

                            $.get("/Activity/GetClubPlayerUser/?idNum=" +
                                identity +
                                "&activityId=" +
                                activityId +
                                "&birthDate=" +
                                input.val() +
                                "&brotherIdNum=" + brotherIdValue,
                                function(data) {
                                    busyIndicator.hide();

                                    if (data.error) {
                                        LockForm();
                                        alert(data.error);
                                        location.reload();
                                        return;
                                    }

                                    if (data && data.Teams && data.Teams.length) {
                                        if (data.MultiTeamEnabled === true) {
                                            ProcessTeamsMultiSelect(data.Teams);
                                        } else {
                                            ProcessTeams(data.Teams);
                                        }
                                    }
                                });
                        }
                    });

                    alert(window.Resources.Messages.Activity_Club_SelectBirthDateFirst);
                    $("#buildForm-body-playerBirthDate").focus();
                }

                if (data.Teams && data.Teams.length && data.Id !== 0) {
                    if (data.MultiTeamEnabled === true) {
                        ProcessTeamsMultiSelect(data.Teams);
                    } else {
                        ProcessTeams(data.Teams);
                    }
                }

                priceAdjustStartDateField.datetimepicker({
                    dayOfWeekStart: window.Resources.Messages.DateTimePicker_StartOfWeek,
                    onSelectDate: function (current, input) {
                        busyIndicator.show();

                        $.get("/Activity/GetClubPlayerUser/?idNum=" + identity +
                            "&activityId=" + activityId +
                            "&birthDate=" + birthDateField.val() +
                            "&brotherIdNum=" + brotherIdValue +
                            "&startDateForPriceAdjustment=" + priceAdjustStartDateField.val(),
                            function (data) {
                                busyIndicator.hide();

                                if (data.error) {
                                    LockForm();
                                    alert(data.error);
                                    location.reload();
                                    return;
                                }

                                var teamsMultipleDropdown = $("#buildForm-body-playerTeamMultiple");
                                if (teamsMultipleDropdown.length) {
                                    var selectedTeamsMultiple = teamsMultipleDropdown.val();

                                    teamsMultipleDropdown.data("selected", selectedTeamsMultiple);

                                    //true means trigger onChange
                                    teamsMultipleDropdown.multiselect("deselect", selectedTeamsMultiple, true);
                                }

                                var schoolsDropdown = $("#buildForm-body-clubSchool");
                                var selectedSchool = 0;
                                if (schoolsDropdown.length) {
                                    selectedSchool = schoolsDropdown.val();
                                }

                                ProcessSchools(data.Schools);

                                if (schoolsDropdown.length) {
                                    schoolsDropdown.val(selectedSchool);
                                    schoolsDropdown.trigger("change");
                                }

                                if (data.Teams && data.Teams.length) {
                                    if (data.MultiTeamEnabled === true) {
                                        ProcessTeamsMultiSelect(data.Teams);
                                    } else {
                                        ProcessTeams(data.Teams);
                                    }
                                }

                                $("#buildForm form").valid();
                            });
                    }
                });

                //var teamDropdown = $("select[name='playerTeam']");

                //teamDropdown.find("option:not([value=''])").remove().end();
                //$.each(data.Teams, function(index, item) {
                //    teamDropdown.append($("<option>", {value: item.TeamId, text: item.TeamName}));
                //});

                $("#buildForm form").valid();
            }
        });
}

function ProcessSchools(schools) {
    if (!schools) {
        var schoolsControl = $("#buildForm-body-clubSchool");

        if (schoolsControl.length) {
            schoolsControl.rules("remove");
            schoolsControl.closest(".control").hide();
        }

        $("#buildForm-body-medicalCert").closest(".control").hide();

        //ProcessTeams(data.Teams);
    } else {
        $("#buildForm-body-clubSchool").closest(".control").show();

        $("#buildForm-body-medicalCert").closest(".control").show();

        var schoolsDropdown = $("#buildForm-body-clubSchool");

        schoolsDropdown.find("option:not([value=''])").remove().end();
        $.each(schools,
            function (index, item) {
                schoolsDropdown.append(
                    $("<option>", { value: item.SchoolId, text: item.SchoolName }));
            });
    }
}

function ProcessTeamsMultiSelect(teams) {
    var teamsDropdown = $("#buildForm-body-playerTeamMultiple");

    teamsDropdown.find("option:not([value=''], :selected)").remove().end();
    $.each(teams,
        function (index, item) {

            var existingItem = teamsDropdown.find("option[value='" + item.TeamId + "']");

            if (existingItem.length <= 0) {
                teamsDropdown.append(
                    $("<option>", {
                        value: item.TeamId,
                        text: $("<div/>").html(item.TeamName).text() + (item.LeaguesNames != undefined && item.LeaguesNames.length ? " - <i>" + $("<div/>").html(item.LeaguesNames).text() + "</i>" : ""),
                        "data-teamname": item.TeamName,
                        "data-school": item.SchoolId,
                        "data-regandequipprice": item.RegistrationAndEquipmentPrice,
                        "data-participationprice": item.ParticipationPrice,
                        "data-insuranceprice": item.InsurancePrice
                    }));
            } else {
                //update price data if option already exist
                existingItem.data("regandequipprice", item.RegistrationAndEquipmentPrice);
                existingItem.data("participationprice", item.ParticipationPrice);
                existingItem.data("insuranceprice", item.InsurancePrice);
            }
        });

    teamsDropdown.multiselect("destroy"); //overriding multiselect parameters
    teamsDropdown.multiselect({
        onChange: function (option, checked, select) {
            var registrationAndEquipmentPrice = parseFloat($(option).data("regandequipprice")) || 0.0;
            var participationPrice = parseFloat($(option).data("participationprice")) || 0.0;
            var insurancePrice = parseFloat($(option).data("insuranceprice")) || 0.0;

            var currentRegPrice = parseFloat($("#buildForm-body-playerRegistrationPrice").val()) || 0.0;
            var currentParticipationPrice = parseFloat($("#buildForm-body-playerParticipationPrice").val()) || 0.0;
            var currentInsurancePrice = parseFloat($("#buildForm-body-playerInsurancePrice").val()) || 0.0;

            if (checked) {
                $("#buildForm-body-playerRegistrationPrice").val(GetFixedNumber(currentRegPrice + registrationAndEquipmentPrice));
                $("#buildForm-body-playerParticipationPrice").val(GetFixedNumber(currentParticipationPrice + participationPrice));
                $("#buildForm-body-playerInsurancePrice").val(GetFixedNumber(currentInsurancePrice + insurancePrice));
            } else {
                $("#buildForm-body-playerRegistrationPrice").val(GetFixedNumber(currentRegPrice - registrationAndEquipmentPrice));
                $("#buildForm-body-playerParticipationPrice").val(GetFixedNumber(currentParticipationPrice - participationPrice));
                $("#buildForm-body-playerInsurancePrice").val(GetFixedNumber(currentInsurancePrice - insurancePrice));
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

function ProcessTeams(teams) {
    if (teams.length > 1) {
        var modalBody = $("#selectTeamModal .modal-body ul.nav");
        modalBody.empty();

        $.each(teams,
            function(index, value) {
                modalBody.append('<li data-teamid="' +
                    value.TeamId +
                    '" data-teamname="' +
                    value.TeamName +
                    '" data-school="' +
                    value.SchoolId +
                    '" data-regandequipprice="' +
                    value.RegistrationAndEquipmentPrice +
                    '" data-participationprice="' +
                    value.ParticipationPrice +
                    '" data-insuranceprice="' +
                    value.InsurancePrice +
                    '"><a href="#">' +
                    value.TeamName + (value.LeaguesNames != undefined && value.LeaguesNames.length ? " - <i>" + value.LeaguesNames + "</i>" : "") +
                    "</a></li>");
            });

        $("#selectTeamModal").modal("show");
    } else {
        SetTeamData(teams[0].TeamId,
            teams[0].TeamName,
            teams[0].SchoolId,
            teams[0].RegistrationAndEquipmentPrice,
            teams[0].ParticipationPrice,
            teams[0].InsurancePrice);
    }
}

$("#buildForm-body-clubSchool").on("change",
    function () {
        busyIndicator.show();

        var activityId = $("#buildForm").data("activity");

        $.get("/Activity/GetSchoolTeamsByAge/?schoolId=" +
            $(this).val() +
            "&date=" +
            $("#buildForm-body-playerBirthDate").val() +
            "&activityId=" + activityId +
            "&idNum=" + $("#buildForm-body-playerID").val() +
            "&brotherIdNum=" + brotherIdValue +
            "&startDateForPriceAdjustment=" + $("#buildForm-body-playerAdjustPricesStartDate").val(),
            function (data) {
                busyIndicator.hide();

                if (data.error) {
                    alert(data.error);
                    $("#buildForm-body-clubSchool").val("");
                    return;
                }

                if (data.MultiTeamEnabled === true) {
                    ProcessTeamsMultiSelect(data.Teams);

                    var teamsMultipleDropdown = $("#buildForm-body-playerTeamMultiple");
                    var teamsSelected = teamsMultipleDropdown.data("selected");
                    if (teamsSelected && teamsSelected.length > 0) { //used for priceAdjustStartDateField
                        //true means trigger onChange
                        teamsMultipleDropdown.multiselect("select", teamsSelected, true);
                    }
                } else {
                    ProcessTeams(data.Teams);
                }
            });
    });

$("#selectTeamModal .modal-body ul.nav").on("click",
    "li",
    function() {
        SetTeamData($(this).data("teamid"),
            $(this).data("teamname"),
            $(this).data("school"),
            $(this).data("regandequipprice"),
            $(this).data("participationprice"),
            $(this).data("insuranceprice"));

        $("#selectTeamModal").modal("hide");
        $("#selectTeamModal .modal-body ul.nav").empty();
    });

$("#selectTeamModal .modal-body ul.nav").on("click",
    "a",
    function(e) {
        e.preventDefault();
    });

//$("#buildForm-body-paymentByBenefactor-select").on("change",
//    function() {
//        var selectedValue = $(this).val();
//        var regPriceField = $("#buildForm-body-teamsRegistrationPrice");

//        if (selectedValue === "true") {
//            regPriceField.data("leagueprice", regPriceField.val());
//            regPriceField.val(0);
//        } else {
//            regPriceField.val(regPriceField.data("leagueprice"));
//        }
//    });

function SetTeamData(teamId, teamName, school, regAndEquipPrice, participationPrice, insurancePrice) {

    $("#buildForm-body-playerTeam").val($("<div/>").html(teamName).text());
    $("#buildForm-body input[name=TeamId]").remove();
    $("#buildForm-body-playerTeam").after("<input type=\"hidden\" name=\"TeamId\" value=\"" + teamId + "\">");

    if (school) {
        $("#buildForm-body-clubSchool").val(school);
        $("#buildForm-body-clubSchool").trigger("change");
    }

    //$("#buildForm-body input[name=SeasonId]").remove();
    //$("#buildForm-body-playerTeam").after("<input type=\"hidden\" name=\"SeasonId\" value=\"" + seasonId + "\">");

    //$("#buildForm-body-playerLeague").val(league);
    //$("#buildForm-body input[name=LeagueId]").remove();
    //$("#buildForm-body-playerTeam").after("<input type=\"hidden\" name=\"LeagueId\" value=\"" + leagueId + "\">");

    $("#buildForm-body-playerRegistrationPrice").val(regAndEquipPrice);
    $("#buildForm-body-playerParticipationPrice").val(participationPrice);
    $("#buildForm-body-playerInsurancePrice").val(insurancePrice);
}

$("#buildForm-body-disableInsurancePayment").change(function() {
    var checked = $(this).is(":checked");
    var insurance = $("#buildForm-body-playerInsurancePrice");

    if (checked) {
        insurance.data("price", insurance.val());

        insurance.val("0");
    } else {
        insurance.val(insurance.data("price"));
    }
});

$("#buildForm-body-disableParticipationPayment").change(function() {
    ToggleParticipationPrice($(this));
});
$("#buildForm-body-postponeParticipationPayment").change(function() {
    ToggleParticipationPrice($(this));
});

function ToggleParticipationPrice($checkbox) {
    var checked = $checkbox.is(":checked");

    var participation = $("#buildForm-body-playerParticipationPrice");

    if (checked) {
        participation.data("price", participation.val());

        participation.val("0");
    } else {
        participation.val(participation.data("price"));
    }
}