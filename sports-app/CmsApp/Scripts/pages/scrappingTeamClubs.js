$(document).ready(function () {
    $('#save_schedule_url').click(function () {
        var url = $("#schedules_url").val();
        var teamName = $('#team_name').val();
        var clubId = $('#ClubId').val();
        var seasonId = $('#SeasonId').val();
        var teamId = $(this).data('team-id');
        if ((url === undefined || url === null || url.length <= 0) && (teamName === undefined || teamName === null || teamName.length <= 0)) {
            alert('Url and team name cannot be empty.');
            return;
        }
        $.ajax({
            url: '/Teams/SaveGamesUrl',
            data: { "teamId": teamId, "url": url, "teamName": teamName, "clubId": clubId, "seasonId": seasonId },
            dataType: "json",
            type: "POST",
            success: function (result) {
                $('#schedules_result').hide();
                $('#schedules_result tbody').empty();
                if (result.Success) {
                    var data = result.Data;
                    window.location.reload();
                }
                else {
                    alert(result.Error);
                }
            },
            error: function (exception) {
                alert("Something went wrong.");
            }
        });
    });
    $(document).on('click', '#save_standings_url', function () {
        var url = $('input[name="GamesUrl"]').val();
        var teamName = $('input[name="TeamName"]').val();
        var teamId = $('#TeamId').val();
        var clubId = $('#ClubId').val();
        var seasonId = $('#SeasonId').val();
        if ((url === undefined || url === null || url.length <= 0) && (teamName === undefined || teamName === null || teamName.length <= 0)) {
            alert('Url and team name cannot be empty.');
            return;
        }
        $.post('/Teams/SaveTeamStandingGameUrl', { "teamId": teamId, "clubId": clubId, "url": url, "teamname": teamName, "seasonId": seasonId }, function (result) {
            if (result.Success) {
                $('#standings').empty();
                //$('#standings').load('/Teams/TeamStandings?clubId=' + clubId + '&teamId=' + teamId);
                $.get('/Teams/TeamStandings?clubId=' + clubId + '&teamId=' + teamId, function (html) {
                    $('#standings').append(html);
                });
                var data = result.Data;
                window.location.reload();
            }
            else if (!result.Success) {
                alert(result.Error);
            }
            else {
                $('input[name="GamesUrl"]').val('');
                alert("Something went wrong.");
            }
        });
    });
});
//# sourceMappingURL=scrappingTeamClubs.js.map