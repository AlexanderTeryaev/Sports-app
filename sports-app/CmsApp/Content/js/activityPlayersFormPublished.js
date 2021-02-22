var busyIndicator = $(".loader-overlay");

function LoadPlayer(identity) {
    var activityId = $("#buildForm").data("activity");

    $.get("/Activity/GetPlayerUser/?idNum=" + identity + "&activityId=" + activityId,
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
                $("#buildForm-body-playerCity").val(data.City);
                $("#buildForm-body-playerBirthDate").val(data.BirthDate);

                var medicalCertControl = $("#buildForm-body-medicalCert");
                var insuranceCertControl = $("#buildForm-body-insuranceCert");
                var profilePictureControl = $("#buildForm-body-playerProfilePicture");

                if (data.ProfilePicture && profilePictureControl.length > 0) {
                    profilePictureControl.after($("<a>",
                        {
                            href: data.ProfilePicture,
                            "class": "glyphicon glyphicon-eye-open glyph-btn",
                            target: "_blank"
                        }));
                    profilePictureControl.rules("remove");
                    $("#buildForm-body-playerProfilePicture-error").text("");
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

                if (data.Teams.length > 1) {
                    var modalBody = $("#selectTeamModal .modal-body ul.nav");

                    $.each(data.Teams,
                        function(index, value) {
                            modalBody.append('<li data-teamid="' +
                                value.TeamId +
                                '" data-teamname="' +
                                value.TeamName +
                                '" data-league="' +
                                value.League +
                                '" data-leagueid="' +
                                value.LeagueId +
                                '" data-playerregprice="' +
                                value.PlayerRegistrationPrice +
                                '" data-playerinsuranceprice="' +
                                value.PlayerInsurancePrice +
                                '" data-seasonid="' +
                                value.SeasonId +
                                '"><a href="#">' +
                                value.TeamName +
                                " - " +
                                value.League +
                                '</a></li>');
                        });

                    $("#selectTeamModal").modal("show");
                } else {
                    SetTeamData(data.Teams[0].TeamId,
                        data.Teams[0].TeamName,
                        data.Teams[0].LeagueId,
                        data.Teams[0].League,
                        data.Teams[0].SeasonId,
                        data.Teams[0].PlayerRegistrationPrice,
                        data.Teams[0].PlayerInsurancePrice);
                }

                if (data.Id > 0) {
                    if ($("#buildForm").data("forbidtochangenameforexistingplayers") === "True") {
                        DisablePlayerNameFields();
                    }
                }

                $("#buildForm form").valid();
            }
        });
}

$("#selectTeamModal .modal-body ul.nav").on("click",
    "li",
    function () {
        SetTeamData($(this).data("teamid"), $(this).data("teamname"), $(this).data("leagueid"), $(this).data("league"), $(this).data("seasonid"), $(this).data("playerregprice"), $(this).data("playerinsuranceprice"));

        $("#selectTeamModal").modal("hide");
        $("#selectTeamModal .modal-body ul.nav").empty();
    });

$("#selectTeamModal .modal-body ul.nav").on("click",
    "a",
    function (e) {
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

function SetTeamData(teamId, teamName, leagueId, league, seasonId, playerRegistrationPrice, playerInsurancePrice) {
    //var activityId = $("#buildForm").data("activity");
    //$.get("/Activity/CheckTeamRegistration/?teamId=" + teamId + "&activityId=" + activityId,
    //    function(data) {
    //        if (data.error) {
    //            LockForm();
    //            alert(data.error);
    //        }
    //    });

    $("#buildForm-body-playerTeam").val($("<div/>").html(teamName).text());
    $("#buildForm-body input[name=TeamId]").remove();
    $("#buildForm-body-playerTeam").after("<input type=\"hidden\" name=\"TeamId\" value=\"" + teamId + "\">");

    $("#buildForm-body input[name=SeasonId]").remove();
    $("#buildForm-body-playerTeam").after("<input type=\"hidden\" name=\"SeasonId\" value=\"" + seasonId + "\">");

    $("#buildForm-body-playerLeague").val(league);
    $("#buildForm-body input[name=LeagueId]").remove();
    $("#buildForm-body-playerTeam").after("<input type=\"hidden\" name=\"LeagueId\" value=\"" + leagueId + "\">");

    $("#buildForm-body-playerRegistrationPrice").val(playerRegistrationPrice);
    $("#buildForm-body-playerInsurancePrice").val(playerInsurancePrice);
}