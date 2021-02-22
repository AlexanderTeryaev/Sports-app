var TennisGame = /** @class */ (function () {

    function TennisGame() {
        this.tech_winner_id_capture = "_tech_winner_";
        this.results_capture = "results_";
        this.player_capture = "_player_";
        this.results_row_capture = "results-row";
        this.home_score_capture = "home-score";
        this.guest_score_capture = "guest-score";
        this.tie_break_capture = "tie_break";
    }

    TennisGame.prototype.init = function () {
        this.invokeEventHandlers();
    };

    TennisGame.prototype.invokeEventHandlers = function () {
        $(document).on('click', '.save-score-btn', (this.saveScoresEvent).bind(this));
        $(document).on('click', '.end-game-btn', (this.endGameEvent).bind(this));
        $(document).on('click', '.reset-game-btn', (this.resetGameEvent).bind(this));
        //$(document).on('click', '#end-and-publish-btn', function () { $("#end-and-publish-btn").unbind("click"); $("#end-and-publish-btn").bind("click", this.endAndPublishEvent); });
        $(document).on('click', '#end-and-publish-hometeam-btn', (this.endAndPublishHomeTeamEvent).bind(this));
        $(document).on('change', '.score-value', (this.setButtonEnabled).bind(this));
        $(document).on('click', '#reset-all-games', (this.resetAllGames).bind(this));
        $(document).on('click', '#reset-all-games-hometeam', (this.resetAllGamesHomeTeam).bind(this));
    };

    TennisGame.prototype.setTechWinner = function (ev) {
        var winnerType = $(ev.target).attr("data-type");
        var setNumber = $(ev.target).attr("data-set");
        var isChecked = $(ev.target).is(":checked");
        var tennisTechWinnerScore = Number($("#tech-tennis-home").val());
        var tennisTechLoserScore = Number($("#tech-tennis-guest").val());

        var number = this.getNumberOfSets(Number($("#bestOfSets").val()));

        if (!tennisTechWinnerScore) {
            tennisTechWinnerScore = 6;
        }
        if (!tennisTechLoserScore) {
            tennisTechLoserScore = 0;
        }

        if (winnerType == "home" && isChecked) {
            $("#guest" + this.tech_winner_id_capture + setNumber).attr("disabled", "disabled");
            this.setValuesForTechnicalWinnerScores(setNumber, winnerType, tennisTechWinnerScore, number);
            this.setValuesForTechnicalWinnerScores(setNumber, "guest", tennisTechLoserScore, number);
        }
        else if (winnerType == "guest" && isChecked) {
            $("#home" + this.tech_winner_id_capture + setNumber).attr("disabled", "disabled");
            this.setValuesForTechnicalWinnerScores(setNumber, winnerType, tennisTechWinnerScore, number);
            this.setValuesForTechnicalWinnerScores(setNumber, "home", tennisTechLoserScore, number);
        }
        else if (winnerType == "home" && !isChecked) {
            $("#guest" + this.tech_winner_id_capture + setNumber).removeAttr("disabled");
            this.setValuesForTechnicalWinnerScores(setNumber, winnerType, "", number);
            this.setValuesForTechnicalWinnerScores(setNumber, "guest", "", number);
        }
        else if (winnerType == "guest" && !isChecked) {
            $("#home" + this.tech_winner_id_capture + setNumber).removeAttr("disabled");
            this.setValuesForTechnicalWinnerScores(setNumber, winnerType, "", number);
            this.setValuesForTechnicalWinnerScores(setNumber, "home", "", number);
        }

        this.saveScores(setNumber);
        this.endGame(setNumber);
    };

    TennisGame.prototype.getNumberOfSets = function (number) {
        switch (number) {
            case 1:
                return 1;
            case 2:
                return 2;
            case 3:
                return 2;
            case 4:
                return 3;
            case 5:
                return 3;
        }
    }

    TennisGame.prototype.setValuesForScores = function (setNumber, winnerType, score) {
        var sets = $("#" + this.results_capture + setNumber).find("." + winnerType + "-score");
        if (sets.length > 0) {
            sets.each(function (index, elem) {
                $(elem).val(score);
            });
        }
    };

    TennisGame.prototype.setValuesForTechnicalWinnerScores = function (setNumber, winnerType, score, number) {
        var sets = $("#" + this.results_capture + setNumber).find("." + winnerType + "-score");
        if (sets.length > 0) {
            for (var i = 1; i <= sets.length; i++) {
                if (i <= number) {
                    $(sets[i - 1]).val(score);
                }
                else {
                    $(sets[i - 1]).val("");
                }
            }
        }
    };

    TennisGame.prototype.checkDropdowns = function () {
        alert("Success");
    }

    TennisGame.prototype.saveScoresEvent = function (ev) {
        var gameNumber = $(ev.target).attr("data-set");
        this.saveScores(gameNumber);
        $(ev.target).attr('disabled', true);
    };

    TennisGame.prototype.setButtonEnabled = function (ev) {
        var setNumber = $(ev.target).attr('data-set');
        $("#save-score-" + setNumber).attr('disabled', false);
    }

    TennisGame.prototype.saveScores = async function (gameNumber) {
        var gameCycle = $("#gameCycleIdHidden").val();
        var homeInformation = this.getPlayerInformation(gameNumber, "home");
        var guestInformation = this.getPlayerInformation(gameNumber, "guest");
        var sets = this.calculateCurrentSets(gameNumber);

        var data = {
            gameCycleId: gameCycle,
            gameNumber: gameNumber,
            homeInformation: homeInformation,
            guestInformation: guestInformation,
            sets: sets
        };

        var result = await $.ajax({
            url: "/GameCycle/SaveTennisLeagueScores",
            type: "POST",
            data: data,
            async: false
        });

        $(this).css('color', 'green');
        return true;
    }

    TennisGame.prototype.endGameEvent = function (ev) {
        var gameNumber = $(ev.target).attr("data-set");
        this.saveScores(gameNumber);
        this.endGame(gameNumber);

    }

    TennisGame.prototype.resetGameEvent = function (ev) {
        var gameNumber = $(ev.target).attr("data-set");
        this.resetGame(gameNumber);
        var closestDiv = $(ev.target).closest('div');
    }

    TennisGame.prototype.setGameDefaultValue = function (gameNumber) {
        this.setValuesForScores(gameNumber, "guest", "");
        this.setValuesForScores(gameNumber, "home", "");
        $("#home_player_" + gameNumber).find("input:checkbox").each(function (index, item) {
            $(item).prop('checked', false);
            $(item).prop('disabled', false);
        });
        $("#guest_player_" + gameNumber).find("input:checkbox").each(function (index, item) {
            $(item).prop('checked', false);
            $(item).prop('disabled', false);
        });
    }

    TennisGame.prototype.endGame = function (gameNumber) {

        var gameCycle = $("#gameCycleIdHidden").val();
        $.ajax({
            url: "/GameCycle/EndTennisGame",
            type: "POST",
            data: {
                gameCycleId: gameCycle,
                gameNumber: gameNumber
            },
            success: function () {
                var div = $("#button_div_" + gameNumber);
                div.find('button[name="event-input"]').remove();
                $(div).append('<button type="button" name="event-input" class="btn btn-danger ' + pullLeftCapture + ' reset-game-btn" data-set="' + gameNumber + '" id="reset-game-' + gameNumber + '">' + resetGameCapture + '</button>');
            }
        })
    }


    TennisGame.prototype.resetGame = function (gameNumber) {
        var self = this;
        var gameCycle = $("#gameCycleIdHidden").val();
        $.ajax({
            url: "/GameCycle/ResetTennisGame",
            type: "POST",
            data: {
                gameCycleId: gameCycle,
                gameNumber: gameNumber
            },
            success: function () {
                var div = $("#button_div_" + gameNumber);
                div.find('button[name="event-input"]').remove();
                $(div).append('<button type="button" name="event-input" class="btn btn-primary ' + pullLeftCapture + ' end-game-btn" data-set="' + gameNumber + '" id="end-game-' + gameNumber + '">' + endGameCapture + '</button>');
                self.setGameDefaultValue(gameNumber);
            }
        })
    }

    TennisGame.prototype.getPlayerInformation = function (set, type) {
        var currentDiv = $("#" + type + this.player_capture + set);
        var selectNotPair = currentDiv.find('.not-pair-select');
        var selectPair = currentDiv.find('.pair-select');

        var isTechnicalWinner = currentDiv.find('#' + type + this.tech_winner_id_capture + set).is(":checked");
        var playerId = selectNotPair.val();
        var pairPlayerId = selectPair.val()
        return {
            isTechnicalWinner: isTechnicalWinner,
            playerId: playerId,
            pairPlayerId: pairPlayerId
        };
    };

    
    TennisGame.prototype.resetAllGamesHomeTeam = function (ev) {
        var gameCycleId = $("#gameCycleIdHidden").val();
        var seasonId = $("#seasonIdHidden").val();
        $.ajax({
            url: "/GameCycle/ResetAllTennisGames",
            type: "POST",
            data: {
                id: gameCycleId,
                seasonId: seasonId
            },
            success: function (data) {
                location.reload();
            }
        });
    }

    TennisGame.prototype.resetAllGames = function (ev) {
        var gameCycleId = $("#gameCycleIdHidden").val();
        var seasonId = $("#seasonIdHidden").val();
        var leagueId = $("#leagueIdHidden").val();
        var isChronological = $("#chronologicalHidden").val();
        $.ajax({
            url: "/GameCycle/ResetAllTennisGames",
            type: "POST",
            data: {
                id: gameCycleId,
                seasonId: seasonId
            },
            success: function (data) {
                $("#schedules").html(data);
            }
        });
    }

    TennisGame.prototype.calculateCurrentSets = function (set) {
        var sets = [];
        var self = this;
        var resultDivs = $("#" + this.results_capture + set).find("." + this.results_row_capture);
        if (resultDivs.length > 0) {
            resultDivs.each(function (index, elem) {
                var homePlayerScore = (Number($(elem).find("." + self.home_score_capture).val()));
                var guestPlayerScore = (Number($(elem).find("." + self.guest_score_capture).val()));
                var isTieBreakGame = $("input[name='" + self.tie_break_capture + "']", elem);
                var isTieBreak = false;
                if (isTieBreakGame.length > 0) {
                    var is_tie_checked = isTieBreakGame[isTieBreakGame.length - 1];
                    if (is_tie_checked.checked) {
                        isTieBreak = true;
                    }
                }
                sets.push({
                    homeScore: homePlayerScore,
                    guestScore: guestPlayerScore,
                    IsTieBreak: isTieBreak
                });
            });
        }
        return sets;
    };
    
    TennisGame.prototype.endAndPublishHomeTeamEvent = function (ev) {
        var numberOfGames = Number($("#numberOfGames").val());
        var gameCycleId = $("#gameCycleIdHidden").val();
        if (numberOfGames != NaN) {
            for (var currGameNumber = 1; currGameNumber <= numberOfGames; currGameNumber++) {
                this.saveScores(currGameNumber);
                this.endGame(currGameNumber);
            }
            $.ajax({
                url: "/GameCycle/EndAndPublishGames",
                type: "POST",
                data: {
                    gameCycleId: gameCycleId
                },
                success: function (data) {
                    location.reload();
                }
            });
        }
    }


    return TennisGame;
}());

window.tennisGame = new TennisGame();
setTimeout(function () { window.tennisGame.init(); }, 150);
