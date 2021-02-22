var busyIndicator = $(".loader-overlay");

function LoadPlayer(identity) {
    var activityId = $("#buildForm").data("activity");

    busyIndicator.show();
    $.get("/Activity/GetUnionCustomTeamManagerUser/?idNum=" + identity + "&activityId=" + activityId,
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
                $("#buildForm-body-playerCity").val(data.City);
                $("#buildForm-body-playerBirthDate").val(data.BirthDate);

                var leaguesDropdown = $("#buildForm-body-playerLeague");

                $.each(data.Leagues,
                    function(index, value) {
                        leaguesDropdown.append("<option value='" +
                            value.Id +
                            "' data-regprice='" +
                            value.TeamRegistrationPrice +
                            "'>" +
                            value.Name +
                            "</option>");
                    });

                if (data.Leagues.length === 1) {
                    leaguesDropdown.find("option:last").prop("selected", true);
                    leaguesDropdown.prop("disabled", true);
                    leaguesDropdown.rules("remove");
                    $("#buildForm-body-playerLeague-error").text("");

                    SetData(data.Leagues[0].Id, data.Leagues[0].TeamRegistrationPrice);
                }

                $("#buildForm form").valid();
            }
        });
}

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

$("#buildForm-body-playerLeague").on("change",
    function () {
        var optionSelected = $("option:selected", this);

        SetData($(this).val(), optionSelected.data("regprice"));
    });

function SetData(leagueId, teamRegistrationPrice) {
    $("#buildForm-body input[name=LeagueId]").remove();
    $("#buildForm-body-playerLeague").after("<input type=\"hidden\" name=\"LeagueId\" value=\"" + leagueId + "\">");

    $("#buildForm-body-teamsRegistrationPrice").val(teamRegistrationPrice);

    //if (needShirts != undefined) {
    //    $("#buildForm-body-needShirts").val(needShirts.toString());
    //}
}