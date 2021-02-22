function changeRefereesNames(cycleId) {

    var refButton = $("#refbtn_" + cycleId);
    var currentMainRefereeHiddenVal = $("#MainRefereeHidden_" + cycleId).val();
    var currentRefereeNamesHiddenVal = $("#MainRefereeHidden_" + cycleId).val();

    refButton.text(currentMainRefereeHiddenVal);
}

function changeDesksNames(cycleId) {

    var refButton = $("#deskbtn_" + cycleId);
    var currentMainDeskHiddenVal = $("#MainDeskHidden_" + cycleId).val();
    var currentDeskNamesHiddenVal = $("#MainDeskHidden_" + cycleId).val();

    refButton.text(currentMainDeskHiddenVal);
}

function checkDesksNotEmpty(desksIds) {
    var desksCompleted = true;
    var notFullPositions = [];
    for (var i = 0; i < desksIds.length; i++) {
        if (desksIds[i] == "" || desksIds == undefined) {
            desksCompleted = false;
            notFullPositions.push("Desk #" + (i + 1));
        }
    }
    var result = {
        completed: desksCompleted,
        notFullPositions: notFullPositions
    };
    return result;
}

function checkRefereesNotEmpty(refereeIds) {
    var refereesCompleted = true;
    var notFullPositions = [];
    for (var i = 0; i < refereeIds.length; i++) {
        if (refereeIds[i] == "" || refereeIds == undefined) {
            refereesCompleted = false;
            notFullPositions.push("Referee #" + (i + 1));
        }
    }
    var result = {
        completed: refereesCompleted,
        notFullPositions: notFullPositions
    };
    return result;
}


function sendReferee(cycleId, isSave) {
    var $modal = $(event.target).closest('.modal-content');
    var $spinner = $modal.find('#spinner_referee');
    var $alert = $modal.find('#alert_' + cycleId); 
    var refereeIds = [];
    $modal.find(":input[name = 'RefereeDropdown']").each(function (e, elem) {
        refereeIds.push($(elem).val());
    });
    var refereesResult = checkRefereesNotEmpty(refereeIds);
    if (refereesResult != undefined && refereesResult.completed != true && !isSave) {
        var resultString = "Warning! Before adding new referee, set value for ";
        for (var i = 0; i < refereesResult.notFullPositions.length; i++) {
            var punctuation = (i == (refereesResult.notFullPositions.length - 1)) ? "." : ", ";
            resultString += refereesResult.notFullPositions[i];
            resultString += punctuation;
        }
        $alert.text(resultString);
        $alert.show();
    }
    else if (refereesResult != undefined && refereesResult.completed != true && isSave) {
        var resultString = "Warning! Before saving, set value for ";
        for (var i = 0; i < refereesResult.notFullPositions.length; i++) {
            var punctuation = (i == (refereesResult.notFullPositions.length - 1)) ? "." : ", ";
            resultString += refereesResult.notFullPositions[i];
            resultString += punctuation;
        }
        $alert.text(resultString);
        $alert.show();
    }
    else {
        var url = isSave ? "/Schedules/SaveReferees" : "/Schedules/AddReferee";
        var leagueId = $("#LeagueId").val();
        $.ajax({
            type: "POST",
            url: url,
            data: {
                leagueId: leagueId,
                cycleId: cycleId,
                refereeIds: refereeIds
            },
            beforeSend: function () {
                $spinner.show();
            },
            success: function(data) {
                if (data.Error == undefined) {
                    $modal.html(data);
                    changeRefereesNames(cycleId);
                    if (isSave) {
                        $modal.parent().parent().modal('hide');
                        $spinner.hide();
                    }
                    $spinner.hide();
                }
                else if (data.Error == true) {
                    $alert.text(data.Message);
                    $alert.show();
                }
            }
        });
    }
}

function sendDesk(cycleId, isSave) {
    var deskIds = [];
    $(".desk_" + cycleId).each(function (e, elem) {
        deskIds.push($(elem).val());
    });
    var desksResult = checkDesksNotEmpty(deskIds);
    if (desksResult != undefined && desksResult.completed != true && !isSave) {
        var resultString = "Warning! Before adding new desk, set value for ";
        for (var i = 0; i < desksResult.notFullPositions.length; i++) {
            var punctuation = (i == (desksResult.notFullPositions.length - 1)) ? "." : ", ";
            resultString += desksResult.notFullPositions[i];
            resultString += punctuation;
        }
        $("#alert_desk" + cycleId).text(resultString);
        $("#alert_desk" + cycleId).show();
    }
    else if (desksResult != undefined && desksResult.completed != true && isSave) {
        var resultString = "Warning! Before saving, set value for ";
        for (var i = 0; i < desksResult.notFullPositions.length; i++) {
            var punctuation = (i == (desksResult.notFullPositions.length - 1)) ? "." : ", ";
            resultString += desksResult.notFullPositions[i];
            resultString += punctuation;
        }
        $("#alert_desk" + cycleId).text(resultString);
        $("#alert_desk" + cycleId).show();
    }
    else {
        var url = isSave ? "/Schedules/SaveDesks" : "/Schedules/AddDesk";
        var leagueId = $("#LeagueId").val();
        $.ajax({
            type: "POST",
            url: url,
            data: {
                leagueId: leagueId,
                cycleId: cycleId,
                desksIds: deskIds
            },
            beforeSend: function () {
                $("#spinner_desk").show();
            },
            complete: function () {
                $("#alert_desk" + cycleId).hide();
            }
        }).done(function (data) {
            if (data.Error == undefined) {
                $("#modal_window_desk_" + cycleId).html(data);
                changeDesksNames(cycleId);
                if (isSave) {
                    $("#deskModal_" + cycleId).modal('hide');
                    $('#spinner_desk').hide();
                }
                $('#spinner_desk').hide();
            }
            else if (data.Error == true) {
                $("#alert_desk" + cycleId).text(data.Message);
                $("#alert_desk" + cycleId).show();
            }
        });
    }
}
function removeReferee(refereeIdent, cycleId) {

    var $modal = $(event.target).closest('.modal-content');
    var $spinner = $modal.find('#spinner_referee');
    var $alert = $modal.find('#alert_' + cycleId); 

    var refereeOrder = refereeIdent - 1;
    var refereeId = $("#refereeDropdown_" + cycleId + "_" + refereeIdent).val();
    var leagueId = $("#LeagueId").val();
    $.ajax({
        type: "POST",
        url: "/Schedules/DeleteReferee",
        data: {
            refereeIdent: refereeId,
            cycleId: cycleId,
            leagueId: leagueId,
            refereeOrder: refereeOrder
        },
        beforeSend: function () {
            $spinner.show();
        }
    }).done(function (data) {
        $modal.html(data);
        changeRefereesNames(cycleId);
        $spinner.hide();
    });
}

function removeDesk(deskIdent, cycleId) {
    var deskOrder = deskIdent - 1;
    var deskId = $("#deskDropdown_" + cycleId + "_" + deskIdent).val();
    var leagueId = $("#LeagueId").val();
    $.ajax({
        type: "POST",
        url: "/Schedules/DeleteDesk",
        data: {
            deskIdent: deskId,
            cycleId: cycleId,
            leagueId: leagueId,
            deskOrder: deskOrder
        },
        beforeSend: function () {
            $("#spinner_desk").show();
        }
    }).done(function (data) {
        $("#modal_window_desk_" + cycleId).html(data);
        changeDesksNames(cycleId);
        $("#spinner_desk").hide();
    });
}

function closeModal(ev) {
    var $button = $(ev.target)
    $button.closest(".modal").modal("hide");
};