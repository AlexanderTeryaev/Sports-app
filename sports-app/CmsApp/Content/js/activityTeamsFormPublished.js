var busyIndicator = $(".loader-overlay");

function LoadPlayer(identity) {
    var activityId = $("#buildForm").data("activity");

    $.get("/Activity/GetTeamManagerUser/?idNum=" + identity + "&activityId=" + activityId,
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

                $("#buildForm-body-playerEmail").val(data.Email);
                $("#buildForm-body-playerPhone").val(data.Phone);
                $("#buildForm-body-playerAddress").val(data.Address);
                $("#buildForm-body-playerBirthDate").val(data.BirthDate);

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
                                '" data-needshirts="' +
                                value.NeedShirts +
                                '" data-teamregprice="' +
                                value.TeamRegistrationPrice +
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
                        data.Teams[0].NeedShirts,
                        data.Teams[0].SeasonId,
                        data.Teams[0].TeamRegistrationPrice);
                }

                $("#buildForm form").valid();
            }
        });
}

$("#selectTeamModal .modal-body ul.nav").on("click",
    "li",
    function () {
        SetTeamData($(this).data("teamid"), $(this).data("teamname"), $(this).data("leagueid"), $(this).data("league"), $(this).data("needshirts"), $(this).data("seasonid"), $(this).data("teamregprice"));

        $("#selectTeamModal").modal("hide");
        $("#selectTeamModal .modal-body ul.nav").empty();
    });

$("#selectTeamModal .modal-body ul.nav").on("click",
    "a",
    function (e) {
        e.preventDefault();
    });

$("#buildForm-body-paymentByBenefactor-select").on("change",
    function() {
        var selectedValue = $(this).val();
        var regPriceField = $("#buildForm-body-teamsRegistrationPrice");

        if (selectedValue === "true") {
            regPriceField.data("leagueprice", regPriceField.val());
            regPriceField.val(0);
        } else {
            regPriceField.val(regPriceField.data("leagueprice"));
        }
    });

function SetTeamData(teamId, teamName, leagueId, league, needShirts, seasonId, teamRegistrationPrice) {
    var activityId = $("#buildForm").data("activity");
    $.get("/Activity/CheckTeamRegistration/?teamId=" + teamId + "&leagueId=" + leagueId + "&activityId=" + activityId,
        function(data) {
            if (data.error) {
                LockForm();
                alert(data.error);
            }
        });

    $("#buildForm-body-playerTeam").val($("<div/>").html(teamName).text());
    $("#buildForm-body input[name=TeamId]").remove();
    $("#buildForm-body-playerTeam").after("<input type=\"hidden\" name=\"TeamId\" value=\"" + teamId + "\">");

    $("#buildForm-body input[name=SeasonId]").remove();
    $("#buildForm-body-playerTeam").after("<input type=\"hidden\" name=\"SeasonId\" value=\"" + seasonId + "\">");

    $("#buildForm-body-playerLeague").val(league);
    $("#buildForm-body input[name=LeagueId]").remove();
    $("#buildForm-body-playerLeague").after("<input type=\"hidden\" name=\"LeagueId\" value=\"" + leagueId + "\">");

    $("#buildForm-body-teamsRegistrationPrice").val(teamRegistrationPrice);

    if (needShirts != undefined) {
        $("#buildForm-body-needShirts").val(needShirts.toString());
    }
}