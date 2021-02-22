(function () {
    var teams = [];
    var rounds_rounds = [];
    var titles = [];
    var basketballScore = false;

    var gameStatus = {
        "started": { text: '@Messages.Started', labelClass: 'label-success' },
        "ended": { text: '@Messages.Ended', labelClass: 'label-danger' },
        "next": { text: '@Messages.Waiting', labelClass: 'label-default' },
        "closetodate": { text: '@Messages.Waiting', labelClass: 'label-default' }
    };

    var gameTypes = { Playoff: 2, Knockout: 3 }

    var nums = {
        2: [1, 2],
        4: [1, 4, 3, 2],
        8: [1, 8, 5, 4, 3, 6, 7, 2],
        16: [1, 16, 9, 8, 5, 12, 13, 4, 3, 14, 11, 6, 7, 10, 15, 2],
        32: [1, 32, 16, 17, 9, 24, 8, 25, 5, 28, 21, 12, 20, 13, 29, 4, 3, 30, 19, 14, 11, 22, 27, 6, 26, 7, 23, 10, 15, 18, 31, 2]
    }

    var insertBracketsIndex = {
        32: [1, 16, 9, 8, 5, 12, 13, 4, 3, 14, 11, 6, 7, 10, 15, 2],
        16: [1, 8, 5, 4, 3, 6, 7, 2],
        8: [1, 4, 3, 2],
        4: [1, 2],
        2: [1]
    }

    var publicActions = {
        toggleGrid: toggleGrid,
        toggleBracketList: toggleBracketList,
        fillBrackets: fillBrackets
    };

    window.schedulesCtrl = publicActions;

    function fillBrackets(winnerElementId, loserElementId, schedule_teams, _rounds, isBasketballOrWaterpoloGame) {
        teams = schedule_teams;
        rounds = _rounds;
        titles = [];
        basketballScore = isBasketballOrWaterpoloGame || false;
        init(winnerElementId, loserElementId);
    }

    function init(winnerElementId, loserElementId) {
        if (teams && teams[0] && teams[0].Items && teams[0].Items[0]) {
            var gameType = teams[0].Items[0].GameType;
            switch (gameType) {
                case gameTypes.Playoff:
                    var useFirstTeams = teams[0].Items.slice(0);
                    var team1Items = [];
                    if (teams.length > 1) {
                        team1Items = teams[1].Items;
                    }
                    var winnerArr = getWinnerBrackets(useFirstTeams.concat(team1Items), getBracketSize(useFirstTeams.length / rounds), rounds);
                    var loserArr = getLoserTeamBracket(useFirstTeams.concat(team1Items), winnerArr.length);
                    createBrackets(winnerElementId, winnerArr, true);
                    createBrackets(loserElementId, loserArr, true, true);
                    break;
                case gameTypes.Knockout:
                    createBrackets(winnerElementId, teams);
                    break;
                default:
            }
        }
    }

    function getLoserTeamBracket(teams, rounds) {
        var loserBrackets = [];
        for (var i = 0; i < rounds; i++) {
            loserBrackets.push({ Items: [] });
            var countOfTeamsToSlice = 0;
            var countOfElements = 0;
            for (var j = 0; j < teams.length; j++) {
                var isCorrectType = teams[j].Bracket.Type == 1 || teams[j].Bracket.Type == 0;
                if (loserBrackets[i].Items.length == 0 && isCorrectType) {
                    loserBrackets[i].Items.push(teams[j]);
                    countOfElements++;
                    countOfTeamsToSlice++;
                }
                else if (isCorrectType && loserBrackets[i].Items[countOfElements - 1].StageId == teams[j].StageId) {
                    loserBrackets[i].Items.push(teams[j]);
                    countOfElements++;
                    countOfTeamsToSlice++;
                }
                else if (!isCorrectType && loserBrackets[i].Items[countOfElements - 1].StageId == teams[j].StageId) {
                    countOfTeamsToSlice++;
                }
                else {
                    teams = teams.slice(countOfTeamsToSlice, teams.length);
                    break;
                }
            }
        }
        return loserBrackets;
    }

    function getBracketSize(teamsLength) {
        if (teamsLength > 16)
            return 32;
        if (teamsLength > 8)
            return 16;
        if (teamsLength > 4)
            return 8;
        if (teamsLength > 2)
            return 4;
        if (teamsLength > 0)
            return 2;
        return 0;
    }

    function getWinnerBrackets(teams, brLength, rounds) {
        var stages = Math.log(brLength * 2) / Math.log(2);
        var retBrackets = [];
        var startIndex = 0;
        var itemsToAdd = brLength;
        for (var i = 0; i < stages; i++) {
            retBrackets.push({ Items: [] });
            for (var j = 0; j < rounds; j++) {
                startIndex += brLength - itemsToAdd;
                var endIndex = startIndex + itemsToAdd;
                for (var u = startIndex; u < endIndex; u++) {
                    retBrackets[i].Items.push(teams[u]);
                }
                startIndex = endIndex;
            }
            itemsToAdd /= 2;
        }

        return retBrackets;
    }

    function formatPlayoffsBrackets(teams) {
        var winnerBrackets = [teams[0]];
        var mainbrLength = teams[0].Items.length;
        var playoffsLength = teams[1].Items.length;
        if (mainbrLength < 1)
            return {};
        var looserBracketTeams = [];
        looserBracketTeams.push({ Items: teams[0].Items.slice(0) });

        var teamsToAdd = mainbrLength;
        var teamsToAddInstage = teamsToAdd;
        var bracketsIndex = 1;
        if (teamsToAdd > 0) {
            looserBracketTeams.push({ Items: [] });
            winnerBrackets.push({ Items: [] });
        }
        var break8Index = { stop: 5, goto: 7 };
        for (var i = 0; i < playoffsLength; i++) {
            var isLooserBr = teamsToAdd / 2 < teamsToAddInstage;
            if (break8Index.stop === i) {
                i = break8Index.goto;
            }
            var item = teams[1].Items[i];
            item.MinPlayoffPos = teamsToAdd * 2;
            if (isLooserBr) {
                looserBracketTeams[bracketsIndex].Items.push(item);
            } else {
                winnerBrackets[bracketsIndex].Items.push(item);
            }
            teamsToAddInstage--;

            if (teamsToAddInstage === 0) {
                teamsToAdd = teamsToAdd / 2;
                if (teamsToAdd < 2) {
                    i = playoffsLength;
                }

                teamsToAddInstage = teamsToAdd;
                bracketsIndex++;
                if (teamsToAdd > 1) {
                    looserBracketTeams.push({ Items: [] });
                    winnerBrackets.push({ Items: [] });
                }
            }
        }
        return {
            looserBracketTeams,
            winnerBrackets
        };
    }

    function getTablesData() {

    }

    function getBracketsTeamsNr(number) {
        if (number < 5)
            return 4;
        if (number < 9)
            return 8;
        if (number < 17)
            return 16;
        if (number < 33) {
            return 32;
        } else {
            return 4;
        }
    }

    function toggleGrid(btn, elem1, elem2) {
        $(btn).text('Show' + ($(btn).text() == 'Show Brackets View' ? 'List' : 'Brackets') + ' View');
        var isNotVisible = $('#' + elem1).css('display') == 'none';
        var hideElem = !isNotVisible ? elem1 : elem2;
        var showElem = !isNotVisible ? elem2 : elem1;

        $('#' + hideElem).slideUp(1000,
            function () {
                $('#' + showElem).hide().slideDown(1000);
            });
    }

    function toggleBracketList(elem, id, untillNextId) {
        $(elem).text($(elem).text() === 'Hide List' ? 'Show List' : 'Hide List');
        if (untillNextId) {
            var currElem = $(elem).parents('tr').next();
            var found = false;
            while (!found) {
                found = currElem.find('.btn-group').length > 0;
                if (!found) {
                    currElem.slideToggle();
                }
                currElem = currElem.next();
                if (currElem.length === 0) {
                    found = true;
                }
            }
        } else {
            $("#" + id).slideToggle();
        }
    }

    function reOrderTeamsForBracketsPlayoff(teams, length, rounds) {
        teams.sort(function (a, b) {
            return a.BracketIndex - b.BracketIndex;
        });
        var ret = [];
        var index = 0;
        //testing purpose: console.log("reorderTeams \n");
        var tLengthInsert = teams.length / rounds;
        for (var j = 0; j < rounds; j++) {
            for (var k = 0; k < tLengthInsert; k++) {
                var retIndex = insertBracketsIndex[length].indexOf(k + 1);
                if (ret[retIndex] === undefined) {
                    ret[retIndex] = [];
                }
                ret[retIndex].push(teams[index++]);
            }
        }
        //testing purpose:console.log("Gme: " + teams[i].HomeTeam + "vs" + teams[i].GuestTeam + "\n");
        /*
        var bracketId = teams[i].Bracket.Id;
        var bracketIdIndex = teamsBraketIds.lastIndexOf(bracketId);
        if (bracketIdIndex === -1) {
            var retIndex = insertBracketsIndex[length].indexOf(forIndex + 1);
            teamsBraketIds[retIndex] = teams[i].Bracket.Id;
            ret[retIndex] = [teams[i]];
            forIndex++;
        } else {
            ret[bracketIdIndex].push(teams[i]);
        }*/
        return ret;
    }

    function reOrderTeamsForBrackets(teams, length) {
        teams.sort(function (a, b) {
            return a.BracketIndex - b.BracketIndex;
        });
        var ret = [];
        var teamsBraketIds = [];
        var forIndex = 0;
        //testing purpose: console.log("reorderTeams \n");
        for (var i = 0; i < teams.length; i++) {
            //testing purpose:console.log("Gme: " + teams[i].HomeTeam + "vs" + teams[i].GuestTeam + "\n");
            var bracketId = teams[i].Bracket.Id;
            var bracketIdIndex = teamsBraketIds.lastIndexOf(bracketId);
            if (bracketIdIndex === -1) {
                var retIndex = insertBracketsIndex[length].indexOf(forIndex + 1);
                teamsBraketIds[retIndex] = teams[i].Bracket.Id;
                ret[retIndex] = [teams[i]];
                forIndex++;
            } else {
                ret[bracketIdIndex].push(teams[i]);
            }
        }
        return ret;
    }

    function createBrackets(appendTo, teams, defaultTitle, isLoserSide) {
        //team.sort(function (a, b) { return a.Bracket.Id - b.Bracket.Id });
        var stages = [];
        var stagesCount = teams.length;
        var titles2 = [];
        //StageName
        if (isLoserSide) {
            titles = [];
        }
        for (var f = 0; f < stagesCount; f++) {
            if (defaultTitle) {
                if (isLoserSide) {
                    var firstItem = teams[f].Items[0];
                    if (firstItem) {
                        if (f != 0) {
                            var maxPosition = firstItem.MaxPlayoffPos;
                            var minPosition = firstItem.MinPlayoffPos;
                            if (maxPosition && minPosition) {
                                var value = maxPosition + "-" + minPosition;
                                titles.push(value);
                            }
                        }
                    }
                }
                else {
                    titles.push('1-' + Math.pow(2, stagesCount - f).toString());
                }
            }
            else {
                var items = teams[f].Items;
                var t0 = items[0];
                if (t0) {
                    titles.push(t0.StageName);
                }
            }
        }


        for (var u = 0; u < stagesCount; u++) {
            var matches = [];
            var roundTeams = teams[u].Items;
            var bracketsSize = Math.pow(2, stagesCount - u);

            if (defaultTitle) {
                roundTeams = reOrderTeamsForBracketsPlayoff(roundTeams, bracketsSize, rounds);
            } else {
                roundTeams = reOrderTeamsForBrackets(roundTeams, bracketsSize);
            }

            var itemsCount = roundTeams.length;

            for (var i = 0; i < itemsCount; i++) {
                var teamRoundGames = roundTeams[i] || {};
                for (var z = 0; z < teamRoundGames.length; z++) {
                    if (z > 0) {
                        var match = matches[matches.length - 1];
                        var roundT = teamRoundGames[z];
                        if (roundT == undefined) {
                            teamRoundGames[z] = { IsPublished: false, GuestTeamScore: 0, HomeTeamScore: 0, GuestTeamId: undefined };
                            roundT = teamRoundGames[z];
                        }
                        var isGuestTeam = match.team1.ID === roundT.GuestTeamId;
                        if (!teamRoundGames[z].IsPublished) {
                            match.team1.score.push('&nbsp;');
                            match.team2.score.push('&nbsp;');
                        } else if (basketballScore) {
                            match.team1.score.push(isGuestTeam ? roundT.BasketBallWaterpoloGuestTeamScore : roundT.BasketBallWaterpoloHomeTeamScore);
                            match.team2.score.push(isGuestTeam ? roundT.BasketBallWaterpoloHomeTeamScore : roundT.BasketBallWaterpoloGuestTeamScore);
                        } else {
                            match.team1.score.push(isGuestTeam ? roundT.GuestTeamScore : roundT.HomeTeamScore);
                            match.team2.score.push(isGuestTeam ? roundT.HomeTeamScore : roundT.GuestTeamScore);
                        }
                    } else {
                        var t1 = (teamRoundGames[z] != undefined && teamRoundGames[z].IsPublished) ? (basketballScore ?
                            {
                                name: teamRoundGames[z].GuestTeam, score: [teamRoundGames[z].BasketBallWaterpoloGuestTeamScore], winner: false, ID: teamRoundGames[z].GuestTeamId, droptarget: false,
                                number: (u === 0 ? nums[bracketsSize][i * 2] : undefined)
                            } :
                            {
                                name: teamRoundGames[z].GuestTeam, score: [teamRoundGames[z].GuestTeamScore], winner: false, ID: teamRoundGames[z].GuestTeamId, droptarget: false,
                                number: (u === 0 ? nums[bracketsSize][i * 2] : undefined)
                            }) :
                            {
                                name: teamRoundGames[z] == undefined ? '&nbsp;' : teamRoundGames[z].GuestTeam, score: ['&nbsp;'], winner: false, ID: teamRoundGames[z] == undefined ? 0 : teamRoundGames[z].GuestTeamId, droptarget: false,
                                number: (u === 0 ? nums[bracketsSize][i * 2] : undefined)
                            };

                        var t2 = (teamRoundGames[z] != undefined && teamRoundGames[z].IsPublished) ? (basketballScore ?
                            {
                                name: teamRoundGames[z].HomeTeam, score: [teamRoundGames[z].BasketBallWaterpoloHomeTeamScore], winner: false, ID: teamRoundGames[z].HomeTeamId, droptarget: false,
                                number: (u === 0 ? nums[bracketsSize][i * 2 + 1] : undefined)
                            } :
                            {
                                name: teamRoundGames[z].HomeTeam, score: [teamRoundGames[z].HomeTeamScore], winner: false, ID: teamRoundGames[z].HomeTeamId, droptarget: false,
                                number: (u === 0 ? nums[bracketsSize][i * 2 + 1] : undefined)
                            }) :
                            {
                                name: teamRoundGames[z] == undefined ? '&nbsp;' : teamRoundGames[z].HomeTeam, score: ['&nbsp;'], winner: false, ID: teamRoundGames[z] == undefined ? 0 : teamRoundGames[z].HomeTeamId, droptarget: false,
                                number: (u === 0 ? nums[bracketsSize][i * 2 + 1] : undefined)
                            };
                        //winner prop to be determined because i do not know the rules for this eg. best of 6 3 2 etc
                        if (teamRoundGames[z] != undefined && teamRoundGames[z].GameStatus === "ended") {
                            if (teamRoundGames[z].IsPublished) {
                                var isGuestWinner = basketballScore ?
                                    (teamRoundGames[z].BasketBallWaterpoloGuestTeamScore > teamRoundGames[z].BasketBallWaterpoloHomeTeamScore || teamRoundGames[z].GuestTeamId === undefined) :
                                    (teamRoundGames[z].GuestTeamScore > teamRoundGames[z].HomeTeamScore || teamRoundGames[z].GuestTeamId === undefined);
                                t1.winner = isGuestWinner;
                                t2.winner = !isGuestWinner;
                            } else {
                                t1.winner = false;
                                t2.winner = false;
                            }
                        }

                        matches.push({
                            team1: t1,
                            team2: t2
                        });
                    }
                }

            }
            stages.push(matches);

            if (u === stagesCount - 1) {
                stages.push([{
                    player1: { name: "", winner: false, ID: 0 }
                }]);
            }
        }

        $("#" + appendTo).brackets({
            titles: titles,
            rounds: stages,
            color_title: '#777777',
            bg_title: '#f9f9f9',
            winner_color: '#545454',
            border_color: '#a2a2a2',
            color_team: '#777777',
            bg_team: '#f9f9f9',
            color_team_hover: '#484848',
            bg_team_hover: '#dedede',
            border_radius_team: '0',
            border_radius_lines: '0',
            brackets_width: 120
        });
    }

    function getRoundsCount(teamsCount) {
        return Math.log(teamsCount) / Math.log(2) + 1;
    }
})();