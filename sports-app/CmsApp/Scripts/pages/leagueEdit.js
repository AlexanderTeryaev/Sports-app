(function () {
    var lib = {};
    lib.UpdateRank = function (leagueId, seasonId, unionId) {
        var leagueStandingUrl = "/LeagueRank/Details/" + leagueId + "?seasonId=" + seasonId + "&unionId=" + unionId;
        $.ajax({
            type: "GET",
            url: leagueStandingUrl,
            success: function (data) {
                $("#leagueranktable").html(data);
            }
        });
        var rankedStandingUrl = "/LeagueRank/RankedStanding?leagueId=" + leagueId + "&seasonId=" + seasonId;
        $.ajax({
            type: "GET",
            url: rankedStandingUrl,
            success: function (data) {
                $("#ranked-standing").html(data);
            }
        });
    };
    window.lgEdit = lib;
})();
//# sourceMappingURL=leagueEdit.js.map