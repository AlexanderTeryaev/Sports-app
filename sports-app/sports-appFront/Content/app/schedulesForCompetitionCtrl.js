(function () {
    var players = [];
    var rounds_rounds = [];
    var titles = [];
    var basketballScore = false;
    var tree_num = 0;

    var gameStatus = {
        "started": { text: '@Messages.Started', labelClass: 'label-success' },
        "ended": { text: '@Messages.Ended', labelClass: 'label-danger' },
        "next": { text: '@Messages.Waiting', labelClass: 'label-default' },
        "closetodate": { text: '@Messages.Waiting', labelClass: 'label-default' }
    };

    var gameTypes = { Playoff: 2, Knockout: 3, Knockout34Consolences1Round: 4, Knockout34ConsolencesQuarterRound: 5, Knockout34: 6  }

    var nums = {
        2: [1, 2],
        4: [1, 4, 3, 2],
        8: [1, 8, 5, 4, 3, 6, 7, 2],
        16: [1, 16, 9, 8, 5, 12, 13, 4, 3, 14, 11, 6, 7, 10, 15, 2],
        32: [1, 32, 16, 17, 9, 24, 8, 25, 5, 28, 21, 12, 20, 13, 29, 4, 3, 30, 19, 14, 11, 22, 27, 6, 26, 7, 23, 10, 15, 18, 31, 2],
        64: [1, 64, 32, 33, 17, 48, 16, 49, 9, 56, 24, 41, 25, 40, 8, 57, 4, 61, 29, 36, 20, 45, 13, 52, 12, 53, 21, 44, 28, 37, 5, 60, 2, 63, 31, 34, 18, 47, 15, 50, 10, 55, 23, 42, 26, 39, 7, 58, 3, 62, 30, 35, 19, 46, 14, 51, 11, 54, 22, 43, 27, 38, 6, 59]
    }
    
    var insertBracketsIndex = {
        64: [1, 32, 17, 16, 9, 24, 25, 8, 5, 28, 21, 12, 13, 20, 29, 4, 3, 30, 19, 14, 11, 22, 27, 6, 7, 26, 23, 10, 15, 18, 31, 2],
        32: [1, 16, 9, 8, 5, 12, 13, 4, 3, 14, 11, 6, 7, 10, 15, 2],
        16: [1, 8, 5, 4, 3, 6, 7, 2],
        8: [1, 4, 3, 2],
        4: [1, 2],
        2: [1]
    }

    var gameType = 0;

    var publicActions = {
        toggleGrid: toggleGrid,
        toggleBracketList: toggleBracketList,
        fillBrackets: fillBrackets
    };

    window.schedulesForCompetitionCtrl = publicActions;

    function fillBrackets(winnerElementId, loserElementId, schedule_players, _rounds, isBasketballOrWaterpoloGame, treeNum) {
        players = schedule_players;
        rounds = _rounds;
        tree_num = treeNum || 0;
        titles = [];
        basketballScore = isBasketballOrWaterpoloGame || false;
        init(winnerElementId, loserElementId);
    }

    function init(winnerElementId, loserElementId) {
        if (players && players[0] && players[0].Items && players[0].Items[0]) {
            var gameType = players[0].Items[0].GameType;
            switch (gameType) {
                case gameTypes.Playoff:
                    var useFirstPlayers = players[0].Items.slice(0);
                    var winnerArr = getWinnerBrackets(useFirstPlayers.concat(players[1].Items), getBracketSize(useFirstPlayers.length / rounds), rounds);
                    var loserArr = getLoserPlayerBracket(useFirstPlayers.concat(players[1].Items), winnerArr.length);
                    createBrackets(winnerElementId, winnerArr, true);
                    createBrackets(loserElementId, loserArr, true, true);
                    break;
                case gameTypes.Knockout34Consolences1Round:
                    var winnerBrackets = [];
                    var looserBrackets = [];
                    var minWinRes = null;
                    var maxlooseRes = null;
                    var lastLosers = null;


                    for (var i = 0; i < players.length; i++) {
                        var winnerStage = {};
                        var looserStage = {};
                        winnerStage.Items = [];
                        looserStage.Items = [];

                        for (var j = 0; j < players[i].Items.length; j++) {
                            if ((players[i].Items[j].Bracket.Type == 2 && (minWinRes == null || minWinRes > players[i].Items[j].MaxPlayoffPos)) || players[i].Items[j].Bracket.Type == 0) {
                                if (minWinRes == null || minWinRes > players[i].Items[j].MinPlayoffPos) {
                                    minWinRes = players[i].Items[j].MinPlayoffPos;
                                }
                                winnerStage.Items.push(players[i].Items[j]);
                            }
                            if ((players[i].Items[j].Bracket.Type == 1 && players[i].Items[j].MaxPlayoffPos != 3) || players[i].Items[j].Bracket.Type == 3 || (players[i].Items[j].Bracket.Type == 0 && i == 0) || players[i].Items[j].Bracket.Type == 4 || players[i].Items[j].Bracket.Type == 5 || (players[i].Items[j].Bracket.Type == 2 && (maxlooseRes != null && maxlooseRes <= players[i].Items[j].MaxPlayoffPos))) {
                                if (players[i].Items[j].Bracket.Type != 0) {
                                    if (maxlooseRes == null || maxlooseRes > players[i].Items[j].MaxPlayoffPos)
                                        maxlooseRes = players[i].Items[j].MaxPlayoffPos;
                                }
                                if (players[i].Items[j].Bracket.Type == 2 && players[i].Items[j].MaxPlayoffPos == players[i].Items[j].MinPlayoffPos - 1) {
                                    lastLosers = players[i].Items[j];
                                } else
                                    looserStage.Items.push(players[i].Items[j]);
                            }
                        }
                        winnerBrackets.push(winnerStage);
                        looserBrackets.push(looserStage);
                        if (lastLosers != null) {
                            var tStage = {};
                            tStage.Items = [];
                            tStage.Items.push(lastLosers);
                            looserBrackets.push(tStage);
                            lastLosers = null;
                        }
                    }
                    createBrackets(winnerElementId, winnerBrackets, true);
                    createBrackets(loserElementId, looserBrackets, true, true);
                    break;
                case gameTypes.Knockout34ConsolencesQuarterRound:
                    gameType = 5;
                    var winnerBrackets = [];
                    var looserBrackets = [];
                    var minWinRes = null;
                    var maxlooseRes = null;
                    var lastLosers = null;
                    for (var i=0 ; i < players.length ; i++)
                    {
                        var winnerStage = {};
                        var looserStage = {};
                        winnerStage.Items = [];
                        looserStage.Items = [];

                        for (var j=0; j < players[i].Items.length; j++) {
                            if ((players[i].Items[j].Bracket.Type == 2 && (minWinRes == null || minWinRes > players[i].Items[j].MaxPlayoffPos)) || players[i].Items[j].Bracket.Type == 0) {
                                if (minWinRes == null || minWinRes > players[i].Items[j].MinPlayoffPos) {
                                    minWinRes = players[i].Items[j].MinPlayoffPos;
                                }
                                winnerStage.Items.push(players[i].Items[j]);
                            }
                            if ((players[i].Items[j].Bracket.Type == 1 && players[i].Items[j].MaxPlayoffPos != 3) || players[i].Items[j].Bracket.Type == 3 || players[i].Items[j].Bracket.Type == 4 || players[i].Items[j].Bracket.Type == 5 || (players[i].Items[j].Bracket.Type == 2 && (maxlooseRes != null && maxlooseRes <= players[i].Items[j].MaxPlayoffPos))) {
                                if (maxlooseRes == null || maxlooseRes > players[i].Items[j].MaxPlayoffPos)
                                    maxlooseRes = players[i].Items[j].MaxPlayoffPos;
                                if (players[i].Items[j].Bracket.Type == 2 && players[i].Items[j].MaxPlayoffPos == players[i].Items[j].MinPlayoffPos-1) {
                                    lastLosers = players[i].Items[j];
                                }else
                                    looserStage.Items.push(players[i].Items[j]);
                            }
                        }
                        winnerBrackets.push(winnerStage);
                        looserBrackets.push(looserStage);
                        if (lastLosers != null) {
                            var tStage = {};
                            tStage.Items = [];
                            tStage.Items.push(lastLosers);
                            looserBrackets.push(tStage);
                            lastLosers = null;
                        }
                    }
                    winnerBrackets = winnerBrackets.filter(element => element.Items.length > 0);
                    looserBrackets = looserBrackets.filter(element => element.Items.length > 0);

                    createBrackets(winnerElementId, winnerBrackets, true);
                    createBrackets(loserElementId, looserBrackets, true, true);
                    break;
                case gameTypes.Knockout:
                case gameTypes.Knockout34:
                    createBrackets(winnerElementId, players);
                    break;
                default:
            }
        }
    }

    function getLoserPlayerBracket(players, rounds) {
        var loserBrackets = [];
        for (var i = 0; i < rounds; i++) {
            loserBrackets.push({ Items: [] });
            var countOfPlayersToSlice = 0;
            var countOfElements = 0;
            for (var j = 0; j < players.length; j++) {
                var isCorrectType = players[j].Bracket.Type == 1 || players[j].Bracket.Type == 0 || players[j].Bracket.Type == 5;
                if (loserBrackets[i].Items.length == 0 && isCorrectType) {
                    loserBrackets[i].Items.push(players[j]);
                    countOfElements++;
                    countOfPlayersToSlice++;
                }
                else if (isCorrectType && loserBrackets[i].Items[countOfElements - 1].StageId == players[j].StageId) {
                    loserBrackets[i].Items.push(players[j]);
                    countOfElements++;
                    countOfPlayersToSlice++;
                }
                else if (!isCorrectType && loserBrackets[i].Items[countOfElements - 1].StageId == players[j].StageId) {
                    countOfPlayersToSlice++;
                }
                else {
                    players = players.slice(countOfPlayersToSlice, players.length);
                    break;
                }
            }
        }
        return loserBrackets;
    }

    function getBracketSize(playersLength) {
        if (playersLength > 32)
            return 64;
        if (playersLength > 16)
            return 32;
        if (playersLength > 8)
            return 16;
        if (playersLength > 4)
            return 8;
        if (playersLength > 2)
            return 4;
        if (playersLength > 0)
            return 2;
        return 0;
    }

    function getWinnerBrackets(players, brLength, rounds) {
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
                    retBrackets[i].Items.push(players[u]);
                }
                startIndex = endIndex;
            }
            itemsToAdd /= 2;
        }
        return retBrackets;
    }

    function formatPlayoffsBrackets(players) {
        var winnerBrackets = [players[0]];
        var mainbrLength = players[0].Items.length;
        var playoffsLength = players[1].Items.length;
        if (mainbrLength < 1)
            return {};
        var looserBracketPlayers = [];
        looserBracketPlayers.push({ Items: players[0].Items.slice(0) });

        var playersToAdd = mainbrLength;
        var playersToAddInstage = playersToAdd;
        var bracketsIndex = 1;
        if (playersToAdd > 0) {
            looserBracketPlayers.push({ Items: [] });
            winnerBrackets.push({ Items: [] });
        }
        var break8Index = { stop: 5, goto: 7 };
        for (var i = 0; i < playoffsLength; i++) {
            var isLooserBr = playersToAdd / 2 < playersToAddInstage;
            if (break8Index.stop === i) {
                i = break8Index.goto;
            }
            var item = players[1].Items[i];
            item.MinPlayoffPos = playersToAdd * 2;
            if (isLooserBr) {
                looserBracketPlayers[bracketsIndex].Items.push(item);
            } else {
                winnerBrackets[bracketsIndex].Items.push(item);
            }
            playersToAddInstage--;

            if (playersToAddInstage === 0) {
                playersToAdd = playersToAdd / 2;
                if (playersToAdd < 2) {
                    i = playoffsLength;
                }

                playersToAddInstage = playersToAdd;
                bracketsIndex++;
                if (playersToAdd > 1) {
                    looserBracketPlayers.push({ Items: [] });
                    winnerBrackets.push({ Items: [] });
                }
            }
        }
        return {
            looserBracketPlayers,
            winnerBrackets
        };
    }

    function getTablesData() {

    }

    function getBracketsPlayersNr(number) {
        if (number < 5)
            return 4;
        if (number < 9)
            return 8;
        if (number < 17)
            return 16;
        if (number < 33) 
            return 32;
        if (number < 65) {
            return 64;
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

    function reOrderPlayersForBracketsPlayoff(players, length, rounds, isLoserSide = false) {
        players.sort(function (a, b) {
            return a.BracketIndex - b.BracketIndex;
        });
        var ret = [];
        var index = 0;
       // var gameType = players[0].GameType;
        var tLengthInsert = players.length / rounds;
        if (gameType == gameTypes.Knockout34ConsolencesQuarterRound && isLoserSide && rounds != undefined) {
            length = 2*players.length;
            tLengthInsert = length;
        }
        for (var j = 0; j < rounds; j++) {
            for (var k = 0; k < tLengthInsert; k++) {
                var insertindex = k + 1;
                var retIndex = insertBracketsIndex[length].indexOf(insertindex);

                if (ret[retIndex] === undefined) {
                    ret[retIndex] = [];
                }
                ret[retIndex].push(players[index++]);
            }
        }
        return ret;
    }

    function reOrderPlayersForBrackets(players, length) {
        players.sort(function (a, b) {
            return a.BracketIndex - b.BracketIndex;
        });
        var ret = [];
        var playersBraketIds = [];
        var forIndex = 0;

        for (var i = 0; i < players.length; i++) {
            var bracketId = players[i].Bracket.Id;
            var bracketIdIndex = playersBraketIds.lastIndexOf(bracketId);
            if (bracketIdIndex === -1) {
                var retIndex = insertBracketsIndex[length].indexOf(forIndex + 1);
                playersBraketIds[retIndex] = players[i].Bracket.Id;
                ret[retIndex] = [players[i]];
                forIndex++;
            } else {
                ret[bracketIdIndex].push(players[i]);
            }
        }
        return ret;
    }

    function createBrackets(appendTo, players, defaultTitle, isLoserSide) {
        //player.sort(function (a, b) { return a.Bracket.Id - b.Bracket.Id });
        var stages = [];
        var stagesCount = players.length;
        var titles2 = [];
        //StageName
        if (isLoserSide) {
            titles = [];
        }
        for (var f = 0; f < stagesCount; f++) {
            if (defaultTitle) {
                if (isLoserSide) {
                    var firstItem = players[f].Items[0];
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
                var items = players[f].Items;
                var t0 = items[0];
                if (t0) {
                    titles.push(t0.StageName);
                }
            }
        }
        for (var u = 0; u < stagesCount; u++) {
            var matches = [];
            var roundPlayers = players[u].Items;
            var bracketsSize = Math.pow(2, stagesCount - u);

            if (defaultTitle && isLoserSide) {
                roundPlayers = reOrderPlayersForBracketsPlayoff(roundPlayers, bracketsSize, rounds, isLoserSide);
            } else {
                roundPlayers = reOrderPlayersForBrackets(roundPlayers, bracketsSize);
            }

            if (gameType == gameTypes.Knockout34ConsolencesQuarterRound && isLoserSide && defaultTitle) {
                bracketsSize = roundPlayers.length;
            }
            var itemsCount = roundPlayers.length;
            for (var i = 0; i < itemsCount; i++) {
                var playerRoundGames = roundPlayers[i] || {};
                for (var z = 0; z < playerRoundGames.length; z++) {
                    if (z > 0) {
                        var match = matches[matches.length - 1];
                        var roundT = playerRoundGames[z];
                        var isMatchPlayer = match.player1 != undefined;
                        var matchPlayer1 = match.team1;
                        var matchPlayer2 = match.team2;
                        if (isMatchPlayer) {
                            matchPlayer1 = matchPlayer1;
                            matchPlayer2 = match.player2;
                        }
                        var isGuestPlayer = matchPlayer1.ID != null && matchPlayer1.ID === roundT.SecondPlayerId;

                        if (!playerRoundGames[z].IsPublished) {
                            matchPlayer1.score.push('&nbsp;');
                            matchPlayer2.score.push('&nbsp;');
                            match.CycleId = playerRoundGames[z].CycleId;
                        }
                        else {
                            matchPlayer1.score.push(isGuestPlayer ? roundT.FirstPlayerScore : roundT.SecondPlayerScore);
                            matchPlayer2.score.push(isGuestPlayer ? roundT.FirstPlayerScore : roundT.SecondPlayerScore);
                            match.CycleId = -1;
                        }
                    } else {
                        var t1 = playerRoundGames[z].IsPublished ? (basketballScore ?
                            {
                                name: playerRoundGames[z].SecondPlayer, score: [playerRoundGames[z].BasketBallWaterpoloGuestTeamScore], winner: false, ID: playerRoundGames[z].SecondPlayerId, droptarget: false,
                                number: (u === 0 ? nums[bracketsSize][i * 2] : undefined)
                            } :
                            {
                                name: playerRoundGames[z].SecondPlayer, score: [playerRoundGames[z].SecondPlayerScore], winner: false, ID: playerRoundGames[z].SecondPlayerId, droptarget: false,
                                number: (u === 0 ? nums[bracketsSize][i * 2] : undefined)
                            }) :
                            {
                                name: playerRoundGames[z].SecondPlayer, score: ['&nbsp;'], winner: false, ID: playerRoundGames[z].SecondPlayerId, droptarget: false,
                                number: (u === 0 ? nums[bracketsSize][i * 2] : undefined)
                            };

                        var t2 = playerRoundGames[z].IsPublished ? (basketballScore ?
                            {
                                name: playerRoundGames[z].FirstPlayer, score: [playerRoundGames[z].FirstPlayerScore], winner: false, ID: playerRoundGames[z].FirstPlayerId, droptarget: false,
                                number: (u === 0 ? nums[bracketsSize][i * 2 + 1] : undefined)
                            } :
                            {
                                name: playerRoundGames[z].FirstPlayer, score: [playerRoundGames[z].FirstPlayerScore], winner: false, ID: playerRoundGames[z].FirstPlayerId, droptarget: false,
                                number: (u === 0 ? nums[bracketsSize][i * 2 + 1] : undefined)
                            }) :
                            {
                                name: playerRoundGames[z].FirstPlayer, score: ['&nbsp;'], winner: false, ID: playerRoundGames[z].FirstPlayerId, droptarget: false,
                                number: (u === 0 ? nums[bracketsSize][i * 2 + 1] : undefined)
                            };
                        //winner prop to be determined because i do not know the rules for this eg. best of 6 3 2 etc
                        if (playerRoundGames[z].GameStatus === "ended") {
                            if (playerRoundGames[z].IsPublished) {
                                var isGuestWinner = basketballScore ?
                                    (playerRoundGames[z].BasketBallWaterpoloGuestTeamScore > playerRoundGames[z].BasketBallWaterpoloHomeTeamScore || playerRoundGames[z].SecondPlayerId === undefined) :
                                    (playerRoundGames[z].SecondPlayerScore > playerRoundGames[z].FirstPlayerScore || playerRoundGames[z].SecondPlayerScore === undefined);
                                t1.winner = isGuestWinner;
                                t2.winner = !isGuestWinner;
                            } else {
                                t1.winner = false;
                                t2.winner = false;
                            }
                        }
                        if (i % 2 == 0 || u == 0 && !isLoserSide) {
                            matches.push({
                                team1: t2,
                                team2: t1,
                                CycleId: playerRoundGames[z].CycleId
                            });
                        } else {
                            matches.push({
                                team1: t1,
                                team2: t2,
                                CycleId: playerRoundGames[z].CycleId
                            });
                        }

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
            brackets_width: 120,
            "tree_num": tree_num
        });
    }

    function getRoundsCount(playersCount) {
        return Math.log(playersCount) / Math.log(2) + 1;
    }
})();