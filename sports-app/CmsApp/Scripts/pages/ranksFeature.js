function addRank(routeId) {
    var newRank = $('#new_rank_' + routeId).val();
    var fromAge = $('#new_rank_AgeFrom_' + routeId).val();
    var toAge = $('#new_rank_AgeTo_' + routeId).val();
    $.ajax({
        type: "POST",
        url: "/Disciplines/AddRank",
        data: {
            routeId: routeId,
            rank: newRank,
            fromAge: fromAge,
            toAge: toAge,
            disciplineId: 1
        },
        success: function (data) {
            $('#modal_window_' + routeId).html(data);

            updateMask();
        },
        error: function (ajaxContext) {
            alert("Something is wrong.")
        }
    });
}

function removeRank(rankId, routeId, relationCount, confirmMessage) {

    
    if (relationCount > 0) {
        if (!confirm(confirmMessage)) {
            return;
        }
    }
    
    $.ajax({
        type: "POST",
        url: "/Disciplines/DeleteRank",
        data: {
            rankId: rankId,
            disciplineId: 1
        },
        error: function (ajaxContext) {
            alert("Something is wrong.")
        },
        success: function (data) {
            $('#modal_window_' + routeId).html(data);
        }
    });
}

function updateRank(rankId) {
    var newValue = $('#rankValue_' + rankId).val();
    var fromAge = $('#rankFromAge_' + rankId).val();
    var toAge = $('#rankToAge_' + rankId).val();
    $.ajax({
        type: "POST",
        url: "/Disciplines/UpdateRank",
        data: {
            rankId: rankId,
            fromAge: fromAge,
            toAge: toAge,
            newValue: newValue
        },
        error: function (ajaxContext) {
            alert("Something is wrong.")
        }
    });
}





function addTeamRank(routeId) {
    var newRank = $('#new_teamrank_' + routeId).val();
    var fromAge = $('#new_teamrank_AgeFrom_' + routeId).val();
    var toAge = $('#new_teamrank_AgeTo_' + routeId).val();
    $.ajax({
        type: "POST",
        url: "/Disciplines/AddTeamRank",
        data: {
            routeId: routeId,
            rank: newRank,
            fromAge: fromAge,
            toAge: toAge,
            disciplineId: 1
        },
        success: function (data) {
            $('#team_modal_window_' + routeId).html(data);

            updateMask();
        },
        error: function (ajaxContext) {
            alert("Something is wrong.")
        }
    });
}

function removeTeamRank(rankId, routeId, relationCount, confirmMessage) {

    if (relationCount > 0) {
        if (!confirm(confirmMessage)) {
            return;
        }
    }

    $.ajax({
        type: "POST",
        url: "/Disciplines/DeleteTeamRank",
        data: {
            rankId: rankId,
            disciplineId: 1
        },
        error: function (ajaxContext) {
            alert("Something is wrong.")
        },
        success: function (data) {
            $('#team_modal_window_' + routeId).html(data);
        }
    });
}

function updateTeamRank(rankId) {
    var newValue = $('#teamrankValue_' + rankId).val();
    var fromAge = $('#teamrankFromAge_' + rankId).val();
    var toAge = $('#teamrankToAge_' + rankId).val();
    $.ajax({
        type: "POST",
        url: "/Disciplines/UpdateTeamRank",
        data: {
            rankId: rankId,
            fromAge: fromAge,
            toAge: toAge,
            newValue: newValue
        },
        error: function (ajaxContext) {
            alert("Something is wrong.")
        }
    });
}
