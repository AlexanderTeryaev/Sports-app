var busyIndicator = $(".loader-overlay");

function LoadPlayer(identity) {
    var activityId = $("#buildForm").data("activity");

    busyIndicator.show();
    $.get("/Activity/GetUnionCustomPersonalPlayerUser/?idNum=" + identity + "&activityId=" + activityId,
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

                $("#buildForm-body-playerGender").val(data.Gender);
                $("#buildForm-body-playerEmail").val(data.Email);
                $("#buildForm-body-playerPhone").val(data.Phone);
                $("#buildForm-body-playerAddress").val(data.Address);
                $("#buildForm-body-playerCity").val(data.City);
                $("#buildForm-body-playerBirthDate").val(data.BirthDate);
                $("#buildForm-body-playerParentName").val(data.ParentName);

                var medicalCertControl = $("#buildForm-body-medicalCert");
                var insuranceCertControl = $("#buildForm-body-insuranceCert");
                var profilePictureControl = $("#buildForm-body-playerProfilePicture");
                var idFileControl = $("#buildForm-body-idFile");

                if (data.ProfilePicture && profilePictureControl.length) {
                    profilePictureControl.after($("<a>",
                        {
                            href: data.ProfilePicture,
                            "class": "glyphicon glyphicon-eye-open glyph-btn",
                            target: "_blank"
                        }));
                    profilePictureControl.rules("remove");
                    $("#buildForm-body-playerProfilePicture-error").text("");
                }

                if (data.MedicalCert && medicalCertControl.length) {
                    medicalCertControl.after($("<a>",
                        {
                            href: data.MedicalCert,
                            "class": "glyphicon glyphicon-eye-open glyph-btn",
                            target: "_blank"
                        }));
                    medicalCertControl.rules("remove");
                    $("#buildForm-body-medicalCert-error").text("");
                }

                if (data.InsuranceCert && insuranceCertControl.length) {
                    insuranceCertControl.after($("<a>",
                        {
                            href: data.InsuranceCert,
                            "class": "glyphicon glyphicon-eye-open glyph-btn",
                            target: "_blank"
                        }));
                    insuranceCertControl.rules("remove");
                    $("#buildForm-body-insuranceCert-error").text("");
                }

                if (data.IdFile && idFileControl.length > 0) {
                    idFileControl.after($("<a>",
                        {
                            href: data.IdFile,
                            "class": "glyphicon glyphicon-eye-open glyph-btn",
                            target: "_blank"
                        }));

                    idFileControl.rules("remove");
                    $("#buildForm-body-idFile-error").text("");
                }

                if (data.ApprovedPlayerCustomPricesDiscount > 0) {
                    // update custom prices with discount
                    $(".customPrice-price").data("approved-discount", data.ApprovedPlayerCustomPricesDiscount);
                    ApplyDiscountToCustomPrices(data.ApprovedPlayerCustomPricesDiscount);
                }

                if ($("#buildForm-body-playerIsEscort-escort").length) {
                    if (data.EscortPlayerCustomPricesDiscount > 0) {
                        // escort discount
                        $("#buildForm-body-playerIsEscort-escort")
                            .data("escort-discount", data.EscortPlayerCustomPricesDiscount);
                    }

                    $("#buildForm-body-playerIsEscort-escort")
                        .data("escort-noinsurance", data.EscortNoInsurance);
                }

                var isLeaguesDropdownPresent = $("#buildForm-body-playerLeagueDropDown").length > 0;
                var isCompetitionCategoryDropdownPresent = $("#buildForm-body-playerCompetitionCategory").length > 0;
                if (!data.PlayerExist) {
                    $("#buildForm-body-playerBirthDate").removeAttr("readonly");

                    if (isCompetitionCategoryDropdownPresent) {
                        $("#buildForm-body-playerBirthDate").datetimepicker({
                            dayOfWeekStart: window.Resources.Messages.DateTimePicker_StartOfWeek,
                            format: "d/m/Y",
                            closeOnDateSelect: true,
                            timepicker: false,
                            scrollInput: false,
                            onSelectDate: function(current, input) {
                                LoadCompetitionCategories();
                            }
                        });

                        $("#buildForm-body-playerGender").on("change",
                            function() {
                                LoadCompetitionCategories();
                            });
                    } else {
                        if (data.RestrictLeaguesByAge) {
                            $("#buildForm-body-playerBirthDate").datetimepicker({
                                dayOfWeekStart: window.Resources.Messages.DateTimePicker_StartOfWeek,
                                format: "d/m/Y",
                                closeOnDateSelect: true,
                                timepicker: false,
                                scrollInput: false,
                                onSelectDate: function (current, input) {
                                    if (isCompetitionCategoryDropdownPresent) {
                                        LoadCompetitionCategories();
                                    } else {
                                        LoadLeagues(isLeaguesDropdownPresent, input.val());
                                    }
                                }
                            });

                            alert(window.Resources.Messages.Activity_LeagueAge_SelectBirthDateFirst);
                            $("#buildForm-body-playerBirthDate").focus();

                            return; //exiting as the player have to select his birthdate first
                        }

                        $("#buildForm-body-playerBirthDate").datetimepicker({
                            dayOfWeekStart: window.Resources.Messages.DateTimePicker_StartOfWeek,
                            format: "d/m/Y",
                            closeOnDateSelect: true,
                            timepicker: false,
                            scrollInput: false
                        });
                    }
                } else {
                    if ($("#buildForm").data("forbidtochangenameforexistingplayers") === "True") {
                        DisablePlayerNameFields();
                    }
                }

                if (isCompetitionCategoryDropdownPresent) {
                    LoadCompetitionCategories();
                } else {
                    LoadLeagues(isLeaguesDropdownPresent);
                }

                $("#buildForm form").valid();
            }
        });
}

function LoadCompetitionCategories() {
    busyIndicator.show();

    $.get("/Activity/GetCompetitionCategories?activityId=" +
        $("#buildForm").data("activity") +
        "&playerId=" +
        $("#buildForm-body-playerID").val() +
        "&birthDate=" +
        $("#buildForm-body-playerBirthDate").val() +
        "&gender=" +
        $("#buildForm-body-playerGender").val(),
        function(data) {
            busyIndicator.hide();

            if (data.error) {
                LockForm();
                alert(data.error);
                //location.reload();
                return;
            }

            var competitionCategoryDropdown = $("#buildForm-body-playerCompetitionCategory");

            competitionCategoryDropdown.rules("add",
                {
                    minimumSelection: data.MinimumSelection
                });

            competitionCategoryDropdown.find("option:not([value=''], :selected)").remove().end();
            $.each(data.Categories,
                function (index, item) {

                    var existingItem = competitionCategoryDropdown.find("option[value='" + item.Id + "']");

                    if (existingItem.length <= 0) {
                        var option = $("<option>",
                            {
                                value: item.Id,
                                text: $("<div/>").html(item.Name).text(),
                                "data-registrationprice": item.RegistrationPrice,
                            });

                        if (item.AlreadyRegistered) {
                            option.prop("selected", true);

                            option.data("registered", true);
                            option.data("regid", item.RegistrationId);
                        }

                        if (item.AlreadyPaid) {
                            option.prop("disabled", true);

                            option.data("paid", true);
                        }

                        if (item.SelectionDisabled) {
                            option.prop("disabled", true);
                        }

                        competitionCategoryDropdown.append(option);
                    } else {
                        //update price data if option already exist
                        existingItem.data("registrationprice", item.RegistrationPrice);
                    }
                });

            competitionCategoryDropdown.multiselect("destroy"); //overriding multiselect parameters
            competitionCategoryDropdown.multiselect({
                numberDisplayed: 1,
                maxHeight: 400,
                onChange: function (option, checked, select) {
                    HandleCompetitionSelection(option, checked);

                    if (option.data("registered")) {
                        var registrationId = option.data("regid");

                        if (checked) {
                            $("#buildForm form input[name='removedRegistrations[]'][value='" + registrationId + "']").remove();
                        } else {
                            var input = $("<input>",
                                {
                                    name: "removedRegistrations[]",
                                    type: "hidden",
                                    value: registrationId
                                });

                            $("#buildForm form").append(input);
                        }
                    }
                }
            });

            $("#buildForm form").valid();

            //select first item if only one was returned
            if (competitionCategoryDropdown.find("option").length === 2) {
                competitionCategoryDropdown.multiselect("select", competitionCategoryDropdown.find("option:last").val(), true);
                competitionCategoryDropdown.trigger("change");
                $("#buildForm form").valid();
            }
        });
}

var competitionsFreeSelections = 0;
var newCompetitionsSelected = 0;
function HandleCompetitionSelection(option, checked) {
    var competitionCategoryDropdown = $("#buildForm-body-playerCompetitionCategory");

    var isRegistered = option.data("registered");
    var isFree = option.data("free");
    var isPaid = option.data("paid");
    
    if (checked) {
        if (isRegistered) {
            competitionsFreeSelections--;

            if (isPaid) {
                newCompetitionsSelected++;
            }
        } else {
            newCompetitionsSelected++;
        }

        if (newCompetitionsSelected > 0) {
            competitionCategoryDropdown
                .find("option:selected")
                .filter(function() {
                     return $(this).data("paid") === true;
                })
                .prop("disabled", false);

            competitionCategoryDropdown.multiselect("refresh");
        }

        if (competitionsFreeSelections > 0) {
            option.data("free", true);
            competitionsFreeSelections--;
        }
    } else {
        if (isRegistered) {
            competitionsFreeSelections++;

            if (isPaid) {
                newCompetitionsSelected--;
            }
        } else {
            newCompetitionsSelected--;
        }

        if (newCompetitionsSelected <= 0) {
            competitionCategoryDropdown
                .find("option:selected")
                .filter(function() {
                     return $(this).data("paid") === true;
                })
                .prop("disabled", true);

            competitionCategoryDropdown.multiselect("refresh");
        }

        if (isFree) {
            option.data("free", false);
            competitionsFreeSelections++;
        }
    }

    if (newCompetitionsSelected < 0) {
        var paidOptions = competitionCategoryDropdown
            .find("option:not(:selected)")
            .filter(function() {
                return $(this).data("paid") === true;
            });

        if (paidOptions.length > 0) {
            var paidOption = $(paidOptions[0]);

            competitionCategoryDropdown.multiselect("select", paidOption.val(), true);

            paidOption.prop("disabled", true);

            competitionCategoryDropdown.multiselect("refresh");
        }
    }

    if (competitionsFreeSelections > 0) {
        //free selections left, try to make non-free option as free
        var nonFreeOptions = competitionCategoryDropdown.find("option:selected").filter(function() {
            return $(this).data("free") !== true;
        });
        if (nonFreeOptions.length > 0) {
            $(nonFreeOptions).each(function (index, nonFreeOption) {
                var registered = $(nonFreeOption).data("registered");

                if (registered) {
                    return; //skip
                }

                $(nonFreeOption).data("free", true);
                competitionsFreeSelections--;
                return false;
            });
        }
    } else if (competitionsFreeSelections < 0) {
        //free selection revoked, try to make free option as non-free
        var freeOptions = competitionCategoryDropdown.find("option:selected").filter(function() {
            return $(this).data("free") === true;
        });
        if (freeOptions.length > 0) {
            $(freeOptions).each(function(index, freeOption) {
                var registered = $(freeOption).data("registered");

                if (registered) {
                    return; //skip
                }

                $(freeOption).data("free", false);
                competitionsFreeSelections++;
                return false;
            });
        }
    }

    RecalculateCompetitionCategoriesPrices();
}

function RecalculateCompetitionCategoriesPrices() {
    var competitionCategoryDropdown = $("#buildForm-body-playerCompetitionCategory");

    var selectedOptions = competitionCategoryDropdown.find("option:selected");

    var resultPrice = 0.00;

    $(selectedOptions).each(function (index, item) {
        var isFree = $(item).data("free");
        var isRegistered = $(item).data("registered");
        var price = parseFloat($(item).data("registrationprice")) || 0.0;

        if (isFree || isRegistered) {
            return; //skip
        }

        resultPrice += price;
    });

    $("#buildForm-body-playerRegistrationPrice").val(GetFixedNumber(resultPrice));

    $("#buildForm form").valid();
}

function LoadLeagues(onlyLeagues, birthDate) {
    busyIndicator.show();
    $.get("/Activity/GetAllAvailableTeams/?activityId=" +
        $("#buildForm").data("activity") +
        "&playerId=" +
        $("#buildForm-body-playerID").val() +
        "&onlyLeagues=" +
        onlyLeagues +
        "&birthDate=" + 
        birthDate,
        function (data) {
            busyIndicator.hide();

            if (data.error) {
                LockForm();
                alert(data.error);
                //location.reload();
                return;
            }

            if (onlyLeagues) {
                var leaguesDropdown = $("#buildForm-body-playerLeagueDropDown");

                leaguesDropdown.find("option:not([value=''])").remove().end();
                $.each(data.Leagues,
                    function (index, item) {
                        leaguesDropdown.append(
                            $("<option>", { value: item.LeagueId, text: item.LeagueName }));
                    });

                //select first league if only one was returned
                if ($("#buildForm-body-playerLeagueDropDown option").length === 2) {
                    $("#buildForm-body-playerLeagueDropDown option:last").prop("selected", true);
                    $("#buildForm-body-playerLeagueDropDown").trigger("change");
                }
            } else {
                var teamsDropdown = $("#buildForm-body-playerTeamDropDown");

                teamsDropdown.find("option:not([value=''])").remove().end();
                $.each(data.Teams,
                    function (index, item) {
                        teamsDropdown.append(
                            $("<option>",
                                {
                                    value: item.TeamId,
                                    text: item.TeamName + " - " + item.League.LeagueName,
                                    "data-teamname": item.TeamName,
                                    "data-registrationprice": item.RegistrationPrice,
                                    "data-membersfee": item.MembersFee,
                                    "data-handlingfee": item.HandlingFee,
                                    "data-insuranceprice": item.InsurancePrice,
                                    "data-leagueid": item.League.LeagueId,
                                    "data-leaguename": item.League.LeagueName
                                }));
                    });

                //select first team if only one was returned
                if (teamsDropdown.find("option").length === 2) {
                    teamsDropdown.find("option:last").prop("selected", true);
                    teamsDropdown.trigger("change");
                    $("#buildForm form").valid();
                }
            }
        });
}

$(".escort-selector input[type=radio][name=playerIsEscort]").change(function () {
    var insuranceField = $("#buildForm-body-playerInsurancePrice");
    var escortNoInsurance = $("#buildForm-body-playerIsEscort-escort").data("escort-noinsurance");

    if (this.value === "true") {
        ApplyDiscountToCustomPrices($(this).data("escort-discount"));

        if (escortNoInsurance) {
            insuranceField.data("noinsurance", true);
            insuranceField.data("price", insuranceField.val());
            insuranceField.val("0");
        }
    } else {
        ResetCustomPrices();

        if (escortNoInsurance) {
            insuranceField.data("noinsurance", false);
            insuranceField.val(insuranceField.data("price"));
        }
    }
});

function ApplyDiscountToCustomPrices(amount) {
    $(".customPrice-price").each(function (index, priceItem) {
        var originalPrice = parseFloat($(priceItem).data("originalprice"));

        var resultPrice = Math.max(0, originalPrice - parseFloat(amount));
        $(priceItem).val(resultPrice);

        $(".control select.customPrice-quantity").trigger("change");
    });
}

function ResetCustomPrices() {
    $(".customPrice-price").each(function (index, priceItem) {
        var approvedDiscount = $(".customPrice-price").data("approved-discount");
        if (approvedDiscount > 0) {
            ApplyDiscountToCustomPrices(approvedDiscount);
            return;
        }

        var originalPrice = parseFloat($(priceItem).data("originalprice"));

        $(priceItem).val(originalPrice);

        $(".control select.customPrice-quantity").trigger("change");
    });
}

function ProcessTeams(teams) {
    if (teams.length > 1) {
        var modalBody = $("#selectTeamModal .modal-body ul.nav");

        $.each(teams,
            function(index, value) {
                modalBody.append('<li data-teamid="' +
                    value.TeamId +
                    '" data-teamname="' +
                    value.TeamName +
                    '" data-registrationprice="' +
                    value.RegistrationPrice +
                    '" data-membersfee="' +
                    value.MembersFee +
                    '" data-handlingfee="' +
                    value.HandlingFee +
                    '" data-insuranceprice="' +
                    value.InsurancePrice +
                    '" data-leagueid="' +
                    value.League.LeagueId +
                    '" data-leaguename="' +
                    value.League.LeagueName +
                    '"><a href="#">' +
                    value.TeamName + " - " + value.League.LeagueName +
                    "</a></li>");
            });

        $("#selectTeamModal").modal("show");
    } else {
        SetTeamData(teams[0].TeamId,
            teams[0].TeamName,
            teams[0].RegistrationPrice,
            teams[0].MembersFee,
            teams[0].HandlingFee,
            teams[0].InsurancePrice,
            teams[0].League.LeagueId,
            teams[0].League.LeagueName
        );
    }
}

$("#buildForm-body-playerTeamDropDown").on("change",
    function() {
        var selected = $(this).find("option:selected");

        SetTeamData(selected.val(),
            selected.data("teamname"),
            selected.data("registrationprice"),
            selected.data("membersfee"),
            selected.data("handlingfee"),
            selected.data("insuranceprice"),
            selected.data("leagueid"),
            selected.data("leaguename")
        );
    });

$("#buildForm-body-playerLeagueDropDown").on("change",
    function () {
        busyIndicator.show();
        $.get("/Activity/GetLeagueTeams/?leagueId=" +
            $(this).val() +
            "&activityId=" +
            $("#buildForm").data("activity") +
            "&playerId=" +
            $("#buildForm-body-playerID").val(),

            function (data) {
                busyIndicator.hide();

                if (data.error) {
                    alert(data.error);
                    LockForm();
                    $("#buildForm-body-clubSchool").val("");
                    return;
                }

                //ProcessTeams(data.Teams);
                var teamsDropdown = $("#buildForm-body-playerTeamDropDown");

                teamsDropdown.find("option:not([value=''])").remove().end();
                $.each(data.Teams,
                    function (index, item) {
                        teamsDropdown.append(
                            $("<option>", {
                                value: item.TeamId,
                                text: item.TeamName + " - " + item.League.LeagueName,
                                "data-teamname": item.TeamName,
                                "data-registrationprice": item.RegistrationPrice,
                                "data-membersfee": item.MembersFee,
                                "data-handlingfee": item.HandlingFee,
                                "data-insuranceprice": item.InsurancePrice,
                                "data-leagueid": item.League.LeagueId,
                                "data-leaguename": item.League.LeagueName
                            }));
                    });

                //select first team if only one was returned
                if (teamsDropdown.find("option").length === 2) {
                    teamsDropdown.find("option:last").prop("selected", true);
                    teamsDropdown.trigger("change");
                    $("#buildForm form").valid(); //trigger validation so the "Value required" will disappear
                }
            });
    });

$("#selectTeamModal .modal-body ul.nav").on("click",
    "li",
    function() {
        SetTeamData($(this).data("teamid"),
            $(this).data("teamname"),
            $(this).data("registrationprice"),
            $(this).data("membersfee"),
            $(this).data("handlingfee"),
            $(this).data("insuranceprice"),
            $(this).data("leagueid"),
            $(this).data("leaguename")
        );

        $("#selectTeamModal").modal("hide");
        $("#selectTeamModal .modal-body ul.nav").empty();

        $("#buildForm-body-playerTeamDropDown").hide();
        $("#buildForm-body-playerTeamDropDown").rules("remove");
        $("#buildForm-body-playerTeamDropDown-error").text("");
        $("#buildForm-body input.teamname").remove();
        $("#buildForm-body-playerTeamDropDown").after("<input type=\"text\" readonly class=\"form-control teamname\" value=\"" + $(this).data("teamname") + "\">");
    });

$("#selectTeamModal .modal-body ul.nav").on("click",
    "a",
    function(e) {
        e.preventDefault();
    });

$("#buildForm-body-paymentByBenefactor-select").on("change", function() {
    HandleBenefactorPrices(true);
});

function HandleBenefactorPrices(restorePrices) {
    var selectedValue = $("#buildForm-body-paymentByBenefactor-select").val();
    var registrationPriceField = $("#buildForm-body-playerRegistrationPrice");
    var insurancePriceField = $("#buildForm-body-playerInsurancePrice");
    var memberFeeField = $("#buildForm-body-playerMemberFee");
    var handlingFeeField = $("#buildForm-body-playerHandlingFee");

    if (selectedValue === "true") {
        registrationPriceField.data("registrationprice", registrationPriceField.val());
        registrationPriceField.val(0);

        insurancePriceField.data("insuranceprice", insurancePriceField.val());
        insurancePriceField.val(0);

        memberFeeField.data("memberfee", memberFeeField.val());
        memberFeeField.val(0);

        handlingFeeField.data("handlingfee", handlingFeeField.val());
        handlingFeeField.val(0);
    } else if(restorePrices) {
        registrationPriceField.val(registrationPriceField.data("registrationprice"));
        insurancePriceField.val(insurancePriceField.data("insuranceprice"));
        memberFeeField.val(memberFeeField.data("memberfee"));
        handlingFeeField.val(handlingFeeField.data("handlingfee"));
    }
}

function SetTeamData(teamId, teamName, registrationPrice, membersFee, handlingFee, insurancePrice, leagueId, leagueName) {

    $("#buildForm-body input[name=TeamId]").remove();
    $("#buildForm-body-playerID").after("<input type=\"hidden\" name=\"TeamId\" value=\"" + teamId + "\">");

    //$("#buildForm-body-playerTeamDropDown").hide(teamId);
    //$("#buildForm-body-playerTeamDropDown").rules("remove");
    //$("#buildForm-body-playerTeamDropDown-error").text("");
    //$("#buildForm-body-playerTeamDropDown").after("<input type=\"text\" readonly class=\"form-control\" value=\"" + teamName + "\">");

    //$("#buildForm-body input[name=SeasonId]").remove();
    //$("#buildForm-body-playerTeam").after("<input type=\"hidden\" name=\"SeasonId\" value=\"" + seasonId + "\">");

    $("#buildForm-body-playerLeagueDropDown").hide();
    $("#buildForm-body input.leaguename").remove();
    $("#buildForm-body-playerLeagueDropDown").after("<input type=\"text\" readonly class=\"form-control leaguename\" value=\"" + leagueName + "\">");

    $("#buildForm-body input[name=LeagueId]").remove();
    $("#buildForm-body-playerID").after("<input type=\"hidden\" name=\"LeagueId\" value=\"" + leagueId + "\">");

    $("#buildForm-body-playerRegistrationPrice").val(registrationPrice);

    var insuranceField = $("#buildForm-body-playerInsurancePrice");
    insuranceField.data("price", insurancePrice);
    if (insuranceField.data("noinsurance") !== true) {
        insuranceField.val(insurancePrice);
    }
    

    $("#buildForm-body-playerMemberFee").val(membersFee);
    $("#buildForm-body-playerHandlingFee").val(handlingFee);

    HandleBenefactorPrices(false);
}

$("#buildForm-body-disableRegistrationPayment").change(function() {
    var checked = $(this).is(":checked");
    var registration = $("#buildForm-body-playerRegistrationPrice");

    if (checked) {
        registration.data("price", registration.val());

        registration.val("0");
    } else {
        registration.val(registration.data("price"));
    }
});

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

$("#buildForm-body-disableMembersFeePayment").change(function () {
    var checked = $(this).is(":checked");
    var fee = $("#buildForm-body-playerMemberFee");

    if (checked) {
        fee.data("price", fee.val());

        fee.val("0");
    } else {
        fee.val(fee.data("price"));
    }
});

$("#buildForm-body-disableHandlingFeePayment").change(function () {
    var checked = $(this).is(":checked");
    var fee = $("#buildForm-body-playerHandlingFee");

    if (checked) {
        fee.data("price", fee.val());

        fee.val("0");
    } else {
        fee.val(fee.data("price"));
    }
});