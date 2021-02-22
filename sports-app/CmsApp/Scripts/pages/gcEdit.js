(function () {
    var lib = {};
    //lib.resetGame = function (warnOnReset, msg, cycleId, departmentId) {
    //    if (!warnOnReset || confirm(msg)) {
    //        window.location.replace("/GameCycle/ResetGame/" + cycleId + "?departmentId=" + departmentId);
    //    }
    //}
    //lib.updateGameResult = function (warnOnReset, msg, cycleId, isWaterpoloOrBasketball, departmentId) {
    //    if (!warnOnReset || confirm(msg)) {
    //        window.location.replace("/GameCycle/UpdateGameResults/" + cycleId + "?isWaterpoloOrBasketball=" + isWaterpoloOrBasketball + "&departmentId=" + departmentId);
    //    }
    //}
    lib.startGame = function (id, departmentId) {
        $.ajax({
            url: "/GameCycle/StartGame",
            type: "GET",
            data: {
                id: id,
                departmentId: departmentId
            },
            success: function (data) {
                $("#edit_game_modal_body").html(data);
            }
        });
    };
    lib.endGame = function (id, departmentId) {
        $.ajax({
            url: "/GameCycle/EndGame",
            type: "GET",
            data: {
                id: id,
                departmentId: departmentId
            },
            success: function (data) {
                $("#edit_game_modal_body").html(data);
            }
        });
    };
    lib.resetGame = function (id, departmentId) {
        $.ajax({
            url: "/GameCycle/ResetGame",
            type: "GET",
            data: {
                id: id,
                departmentId: departmentId
            },
            success: function (data) {
                $("#edit_game_modal_body").html(data);
            }
        });
    };
    lib.techWin = function (id, teamId, athleteId, departmentId) {
        $.ajax({
            url: "/GameCycle/TechnicalWin",
            type: "GET",
            data: {
                id: id,
                teamId: teamId,
                athleteId: athleteId,
                departmentId: departmentId
            },
            success: function (data) {
                $("#edit_game_modal_body").html(data);
            }
        });
    };
    /** add cheng for comment of EditGamePartial */
    lib.saveNote = function (id, comments, departmentId) {
        $.ajax({
            url: "/GameCycle/GameCycleComment",
            type: "GET",
            data: {
                id: id,
                comments: comments,
                departmentId: departmentId
            },
            success: function (data) {
                $("#edit_game_modal_body").html(data);
            }
        });
    };
    lib.documentReady = function () {
        $('#gamefrm').validateBootstrap(true);
        $(".game-date").datetimepicker({
            format: 'd/m/Y H:i',
            formatTime: 'H:i',
            formatDate: 'd/m/Y',
            step: 15,
            closeOnDateSelect: false,
            closeOnTimeSelect: true,
            onChangeDateTime: function () {
                $(this).data("input").trigger("changedatetime.xdsoft");
            }
        });
    };
    window.gcEdit = lib;
})();
//# sourceMappingURL=gcEdit.js.map