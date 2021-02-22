var busyIndicator = $(".loader-overlay");

function LoadPlayer(identity) {
    var activityId = $("#buildForm").data("activity");

    $.get("/Activity/GetDepartmentClubPlayerUser/?idNum=" + identity + "&activityId=" + activityId,
        function(data) {
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

                if (!data.Schools) {
                    $("#buildForm-body-clubSchool").rules("remove");
                    $("#buildForm-body-clubSchool").closest(".control").hide();

                    $("#buildForm-body-medicalCert").closest(".control").hide();

                    ProcessTeams(data.Teams);
                } else {
                    $("#buildForm-body-clubSchool").closest(".control").show();

                    $("#buildForm-body-medicalCert").closest(".control").show();

                    var schoolsDropdown = $("#buildForm-body-clubSchool");

                    schoolsDropdown.find("option:not([value=''])").remove().end();
                    $.each(data.Schools,
                        function(index, item) {
                            schoolsDropdown.append(
                                $("<option>", { value: item.SchoolId, text: item.SchoolName }));
                        });
                }


                //var teamDropdown = $("select[name='playerTeam']");

                //teamDropdown.find("option:not([value=''])").remove().end();
                //$.each(data.Teams, function(index, item) {
                //    teamDropdown.append($("<option>", {value: item.TeamId, text: item.TeamName}));
                //});

                $("#buildForm form").valid();
            }
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
                    '" data-regandequipprice="' +
                    value.RegistrationAndEquipmentPrice +
                    '" data-participationprice="' +
                    value.ParticipationPrice +
                    '" data-insuranceprice="' +
                    value.InsurancePrice +
                    '"><a href="#">' +
                    value.TeamName +
                    "</a></li>");
            });

        $("#selectTeamModal").modal("show");
    } else {
        SetTeamData(teams[0].TeamId,
            teams[0].TeamName,
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
            "&activityId=" + activityId,
            function (data) {
                busyIndicator.hide();
                if (data.error) {
                    alert(data.error);
                    $("#buildForm-body-clubSchool").val("");
                    return;
                }

                ProcessTeams(data.Teams);
            });
    });

$("#selectTeamModal .modal-body ul.nav").on("click",
    "li",
    function() {
        SetTeamData($(this).data("teamid"),
            $(this).data("teamname"),
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

function SetTeamData(teamId, teamName, regAndEquipPrice, participationPrice, insurancePrice) {

    $("#buildForm-body-playerTeam").val($("<div/>").html(teamName).text());
    $("#buildForm-body input[name=TeamId]").remove();
    $("#buildForm-body-playerTeam").after("<input type=\"hidden\" name=\"TeamId\" value=\"" + teamId + "\">");

    //$("#buildForm-body input[name=SeasonId]").remove();
    //$("#buildForm-body-playerTeam").after("<input type=\"hidden\" name=\"SeasonId\" value=\"" + seasonId + "\">");

    //$("#buildForm-body-playerLeague").val(league);
    //$("#buildForm-body input[name=LeagueId]").remove();
    //$("#buildForm-body-playerTeam").after("<input type=\"hidden\" name=\"LeagueId\" value=\"" + leagueId + "\">");

    $("#buildForm-body-playerRegistrationPrice").val(regAndEquipPrice);
    $("#buildForm-body-playerParticipationPrice").val(participationPrice);
    $("#buildForm-body-playerInsurancePrice").val(insurancePrice);
}