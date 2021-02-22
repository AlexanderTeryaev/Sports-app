var MESSAGE_ALERT_SELECT_PAIR_FIRST = "";
var MESSAGE_TOTAL_PAIRS = "";
var MESSAGE_TOTAL_PLAYERS = "";
var MESSAGE_TOTAL_WARNING_ALERT_PAIRS = "";
var RETURN_MESSAGE_WARING_ALERT = "";
var RETURN_MESSAGE_TOTAL = "";
var MESSAGE_TOTAL_WARNING_ALERT_PLAYERS = "";
var GameTypeId;
(function (GameTypeId) {
    GameTypeId[GameTypeId["Division"] = 1] = "Division";
    GameTypeId[GameTypeId["Playoff"] = 2] = "Playoff";
    GameTypeId[GameTypeId["Knockout"] = 3] = "Knockout";
    GameTypeId[GameTypeId["Knockout34Consolences1Round"] = 4] = "Knockout34Consolences1Round";
    GameTypeId[GameTypeId["Knockout34ConsolencesUntilQuarter"] = 5] = "Knockout34ConsolencesUntilQuarter";
    GameTypeId[GameTypeId["Knockout34"] = 6] = "Knockout34";
})(GameTypeId || (GameTypeId = {}));
var EditGroup = /** @class */ (function () {
    function EditGroup() {
        this.teams = [];
        this.editableTeams = [];
        this.viewExpanded = false;
        this.isRankedOption = false;
        this.currGameTypeId = GameTypeId.Division;
    }
    EditGroup.prototype.init = function () {
        var self = this;
        self.isRankedOption = false;
        var realCount = $("#GroupTeamsAndHiddenCount").val();
        var fakeCount = $("#GroupTeamsNoHiddenCount").val();
        if (realCount != 0 && realCount != fakeCount) {
            console.log("init done and realCount: " + realCount);
            self.isRankedOption = true;
        }
        else {
            console.log("init done but no specific count, fakeCount: " + fakeCount);
        }
        if ($('#groupsedit.modal.fade.in').length < 1) {
            //prevent from calling init again when backend saves the data           
            return;
        }
        self.setAllTeamsArr();
        self.mainDropDownListSelectActions($('#MainDropDownListID').val(), true);
        $('#MainDropDownListID').change(function () {
            self.mainDropDownListSelectActions($(this).val(), false);
        });
        $('#NumberOfTeams').change((self.setDragListDragSelectedItems).bind(self));
        $('#groupsedit').on('hidden.bs.modal', function () {
            self.removeListeners();
            if (self.viewExpanded)
                self.expandView();
        }).bind(self);
        $('#groupmodal .glyphicon-fullscreen').click((self.expandView).bind(self));
        $(document).on('click', '.removePlayer', (self.removePlayerAddToTeamList).bind(self));
        $(document).on('change', '#Type', (self.changeAllTeamsArrValues).bind(self));
        $(document).on('change', '#Type', (self.setDragListDragSelectedItems).bind(self));
        $(document).on('change', '#dragTeamList :checkbox', (self.checkPairsDivStatus).bind(self));
        $(document).on('click', '#pairsdiv :button', (self.addNewPair).bind(self));
        $(document).on('change', '#dragPairsList :checkbox', (self.changeDisableStatusForPairs).bind(self));
        $(document).on('click', '#removeapair', (self.removeSelectedPairs).bind(self));
        $("#apply").click(function () { self.filterPlayers(); });
        document.addEventListener("dragstart", self.dragStart);
        document.addEventListener("dragenter", self.dragenter);
        document.addEventListener("dragend", self.dragend);
        document.addEventListener("dragleave", self.dragleave);
        document.addEventListener("drop", self.drop.bind(self));
        document.addEventListener("dragover", self.dragover);
    };
    EditGroup.prototype.randomLottery = function () {
        var self = this;
        var checkedTeams = $("#dragTeamList :checkbox").filter(":checked");
        self.isRankedOption = false;
        if (checkedTeams.length == Number($("#NumberOfTeams").val())) {
            var teamsArr = self.getSelectedTeamsArray(self.teams, checkedTeams);
            var neededCount = checkedTeams.length;
            if (neededCount > 0) {
                var randomedTeams = teamsArr.sort(function () { return .5 - Math.random(); });
                var selected = randomedTeams.slice(0, neededCount);
                var emptyPlaces = self.getEmptySpaces(self);
                if (emptyPlaces.length == neededCount) {
                    self.fillInTeams(selected, emptyPlaces, self, false);
                }
                else {
                    self.setDragListDragSelectedItems();
                    var checkedTeamsIds = [];
                    checkedTeams.each(function (index, elem) {
                        checkedTeamsIds.push($(elem).attr("id"));
                    });
                    self.setCurrentCheckboxList(checkedTeamsIds);
                    self.randomLottery();
                }
                self.IndexSelectedList();
            }
        }
        else {
            if ($("#Type").val() == "2") {
                RETURN_MESSAGE_TOTAL = $("#randomWarningLabel").text();
                $("#randomWarningLabel").text(MESSAGE_TOTAL_WARNING_ALERT_PAIRS);
            }
            else if ($("#Type").val() == "1") {
                $("#randomWarningLabel").text(RETURN_MESSAGE_TOTAL);
            }
            $("#randomWarningLabel").show(500);
            setTimeout(function () {
                $('#randomWarningLabel').hide(500);
            }, 5000);
        }
    };
    EditGroup.prototype.rankedLottery = function () {
        var self = this;
        self.isRankedOption = true;
        var numberOfPlayers = Number($("#NumberOfTeams").val());
        var checkedPlayers = $("#dragTeamList :checkbox").filter(":checked");
        var selectedPlayers = self.getSelectedTeamsArray(self.teams, checkedPlayers);
        var allPlaces = self.getAllSpaces(self);
        if (checkedPlayers && checkedPlayers.length == numberOfPlayers) { //&& emptyPlaces && emptyPlaces.length == checkedPlayers.length
            self.fillInTeams(selectedPlayers, allPlaces, self, true);
        }
        else {
            if ($("#Type").val() == "2") {
                RETURN_MESSAGE_TOTAL = $("#randomWarningLabel").text();
                $("#randomWarningLabel").text(MESSAGE_TOTAL_WARNING_ALERT_PAIRS);
            }
            else if ($("#Type").val() == "1") {
                if (RETURN_MESSAGE_TOTAL)
                    $("#randomWarningLabel").text(RETURN_MESSAGE_TOTAL);
                else
                    $("#randomWarningLabel").text(MESSAGE_TOTAL_WARNING_ALERT_PLAYERS);
            }
            $("#randomWarningLabel").show(500);
            setTimeout(function () {
                $('#randomWarningLabel').hide(500);
            }, 5000);
        }
    };
    EditGroup.prototype.fillRankedTeams = function (numberOfPlayers, checkedPlayers, emptyPlaces, self) {
        var neededPlayers = [];
        var otherPlayers = [];
        var places = [];
        var otherPlaces = [];
        switch (numberOfPlayers) {
            case 16:
                neededPlayers = checkedPlayers.slice(0, 4);
                otherPlayers = checkedPlayers.slice(4, 16);
                places = [1, 16, 5, 12];
                otherPlaces = self.getOtherPlacesArray(numberOfPlayers, places);
                break;
            case 32:
                neededPlayers = checkedPlayers.slice(0, 8);
                otherPlayers = checkedPlayers.slice(8, 32);
                places = [1, 32, 9, 24, 8, 16, 17, 25];
                otherPlaces = self.getOtherPlacesArray(numberOfPlayers, places);
                break;
            case 64:
                neededPlayers = checkedPlayers.slice(0, 16);
                otherPlayers = checkedPlayers.slice(16, 64);
                places = [1, 64, 17, 48, 16, 32, 33, 49, 8, 9, 24, 25, 40, 41, 56, 57];
                otherPlaces = self.getOtherPlacesArray(numberOfPlayers, places);
                break;
            default:
                self.randomLottery();
                break;
        }
        var randomedPlayers = otherPlayers.sort(function () { return .5 - Math.random(); });
        self.fillPlayersInRanks(neededPlayers, emptyPlaces, places, self);
        self.fillPlayersInRanks(randomedPlayers, emptyPlaces, otherPlaces, self);
    };
    EditGroup.prototype.getOtherPlacesArray = function (numberOfPlayers, selectedPlaces) {
        var necessaryPlaces = [];
        for (var i = 1; i <= numberOfPlayers; i++) {
            if (!(selectedPlaces.indexOf(i) > -1)) {
                necessaryPlaces.push(i);
            }
        }
        return necessaryPlaces;
    };
    EditGroup.prototype.fillPlayersInRanks = function (neededPlayers, emptyPlaces, places, self) {
        for (var i = 0; i < places.length; i++) {
            var place = places[i] - 1;
            var placeToFill = emptyPlaces[place];
            if (self.currGameTypeId == GameTypeId.Division)
                self.moveTeamsToBrackets(neededPlayers[i], $(placeToFill), neededPlayers[i].text, self);
            else
                self.moveTeamsToBrackets(neededPlayers[i], $(placeToFill).parent(), neededPlayers[i].text, self);
        }
    };
    EditGroup.prototype.setCurrentCheckboxList = function (currentIds) {
        if (currentIds.length > 0) {
            $("#dragTeamList :checkbox").each(function (index, elem) {
                for (var i = 0; i < currentIds.length; i++) {
                    var insideId = $(elem).attr("id");
                    if (insideId == currentIds[i]) {
                        $(elem).attr('checked', "checked");
                    }
                }
            });
        }
    };
    EditGroup.prototype.getSelectedTeamsArray = function (teams, checkedTeams) {
        var resultArray = [];
        checkedTeams.each(function (index, elem) {
            teams.forEach(function (team) {
                if (+($(elem).parent().attr('data-id')) == team.val) {
                    resultArray.push(team);
                }
            });
        });
        return resultArray;
    };
    ;
    EditGroup.prototype.swapArray = function (Array, Swap1, Swap2) {
        var temp = Array[Swap1];
        Array[Swap1] = Array[Swap2];
        Array[Swap2] = temp;
        return Array;
    };
    EditGroup.prototype.fillInTeams = function (teams, emptyPlaces, self, isRanked) {
        var compType = $("select[name=TypeId]").val();
        var teamsCount = teams.length;
        var orderedTeams = teams;
        // check if the gametype is other than first type which is home type. 
        if (compType > 1) {
            if (isRanked) {
                if (teamsCount <= 8) {
                    self.createBrackets(8);
                    for (var i = teamsCount; i < 8; i++) {
                        teams.push({ text: "", val: undefined });
                    }
                    this.swapArray(teams, 1, 7);
                }
                else if (teamsCount <= 16) {
                    self.createBrackets(16);
                    var emptyPlayers = 16 - teamsCount;
                    for (var i = teamsCount; i < 16; i++) {
                        teams.push({ text: "", val: undefined });
                    }
                    this.swapArray(teams, 1, 15);
                    this.swapArray(teams, 2, 4);
                    if (emptyPlayers > 2) {
                        this.swapArray(teams, 13, 5);
                    }
                    this.swapArray(teams, 3, 11);
                    if (emptyPlayers > 3) {
                        this.swapArray(teams, 12, 10);
                    }
                }
                else if (teamsCount <= 32) {
                    self.createBrackets(32);
                    var emptyPlayers = 32 - teamsCount;
                    for (var i = teamsCount; i < 32; i++) {
                        teams.push({ text: "", val: undefined });
                    }
                    this.swapArray(teams, 1, 31);
                    this.swapArray(teams, 2, 8);
                    this.swapArray(teams, 3, 23);
                    this.swapArray(teams, 4, 7);
                    this.swapArray(teams, 5, 15);
                    this.swapArray(teams, 6, 16);
                    this.swapArray(teams, 4, 24);
                    // freespaces to ranked
                    this.swapArray(teams, 9, 29);
                    this.swapArray(teams, 22, 28);
                    this.swapArray(teams, 6, 27);
                    this.swapArray(teams, 14, 26);
                    this.swapArray(teams, 17, 25);
                    this.swapArray(teams, 25, 4);
                    // fill empty games
                    this.swapArray(teams, 5, 26);
                    this.swapArray(teams, 13, 18);
                    this.swapArray(teams, 11, 20);
                }
                else if (teamsCount <= 64) {
                    self.createBrackets(64);
                    var emptyPlayers = 64 - teamsCount;
                    for (var i = teamsCount; i < 64; i++) {
                        teams.push({ text: "", val: undefined });
                    }
                    // applying rankings
                    this.swapArray(teams, 1, 63);
                    this.swapArray(teams, 2, 16);
                    this.swapArray(teams, 3, 47);
                    this.swapArray(teams, 4, 15);
                    this.swapArray(teams, 5, 31);
                    this.swapArray(teams, 6, 32);
                    this.swapArray(teams, 7, 48);
                    this.swapArray(teams, 8, 7);
                    this.swapArray(teams, 9, 8);
                    this.swapArray(teams, 10, 23);
                    this.swapArray(teams, 11, 24);
                    this.swapArray(teams, 12, 39);
                    this.swapArray(teams, 13, 40);
                    this.swapArray(teams, 14, 55);
                    this.swapArray(teams, 4, 56);
                    this.swapArray(teams, 61, 17);
                    this.swapArray(teams, 46, 60);
                    this.swapArray(teams, 14, 59);
                    this.swapArray(teams, 30, 58);
                    this.swapArray(teams, 33, 57);
                    this.swapArray(teams, 49, 4);
                    this.swapArray(teams, 6, 59);
                    this.swapArray(teams, 54, 9);
                    this.swapArray(teams, 22, 53);
                    this.swapArray(teams, 25, 52);
                    this.swapArray(teams, 38, 51);
                    this.swapArray(teams, 41, 50);
                    this.swapArray(teams, 54, 4);
                    this.swapArray(teams, 57, 4);
                    // fix empty brackets
                    this.swapArray(teams, 11, 12);
                    this.swapArray(teams, 21, 34);
                    this.swapArray(teams, 27, 36);
                    this.swapArray(teams, 29, 42);
                    this.swapArray(teams, 53, 44);
                    this.swapArray(teams, 50, 19);
                }
            }
        }
        orderedTeams = teams.filter(function (element) { return element !== undefined; });
        if (isRanked) {
            emptyPlaces = self.getEmptySpaces(self);
        }
        for (var i = 0; i < orderedTeams.length; i++) {
            if (self.currGameTypeId == GameTypeId.Division)
                self.moveTeamsToBrackets(orderedTeams[i], $(emptyPlaces[i]), orderedTeams[i].text, self);
            else
                self.moveTeamsToBrackets(orderedTeams[i], $(emptyPlaces[i]).parent(), orderedTeams[i].text, self);
        }
        if (isRanked) {
            if (this.currGameTypeId == GameTypeId.Knockout || this.currGameTypeId == GameTypeId.Knockout34 || this.currGameTypeId == GameTypeId.Playoff || this.currGameTypeId == GameTypeId.Knockout34Consolences1Round || this.currGameTypeId == GameTypeId.Knockout34ConsolencesUntilQuarter) {
                //self.createBrackets(teamsNr);
                //TODO remove drag and add gray bg color to empty boxes.
            }
        }
    };
    EditGroup.prototype.getEmptySpaces = function (self) {
        if (self.currGameTypeId == GameTypeId.Division) {
            return $('#dragToSelectedTeams .container-division-index .player')
                .filter(function (index) {
                return !(Number($(this).attr('data-id')) > 0);
            });
        }
        else {
            return $('#dragToSelectedTeams .container-brackets .player .dropItems.droptarget')
                .filter(function (index, elem) {
                return ($(elem).attr('id') != 'brundefined' && !(Number($(this).attr('data-id')) > 0)
                    && $(elem).parent().attr('data-id') == "");
            });
        }
    };
    EditGroup.prototype.getAllSpaces = function (self) {
        if (self.currGameTypeId == GameTypeId.Division) {
            return $('#dragToSelectedTeams .container-division-index .player');
        }
        else {
            return $('#dragToSelectedTeams .container-brackets .player .dropItems.droptarget');
        }
    };
    EditGroup.prototype.moveTeamsToBrackets = function (team, toElement, text, self) {
        var dataId = Number(toElement.attr('data-id'));
        if (dataId) {
            var listIndex = toElement[0].dataset.listindex;
            var index = +listIndex.replace('tlist', '');
            var newElem = $(this.getTeamListElement(index, dataId, toElement.find('.playername').text()));
            var parentNode = $('#dragTeamList>ol')[0];
            if (index == 0) {
                parentNode.insertBefore(newElem[0], parentNode.childNodes[0]);
            }
            else {
                var referenceNode = EditGroup.getClosestElem(index - 1);
                if (referenceNode == 0) {
                    referenceNode = +parentNode.childNodes[0];
                }
                parentNode.insertBefore(newElem[0], referenceNode == undefined ? null : referenceNode.nextSibling);
            }
        }
        toElement.attr({
            'data-id': team.val,
            'data-listindex': text
        }).find('.playername').text(team.text);
        toElement.find('.dropItems').attr('draggable', 'True');
        EditGroup.showRemovePlayerIcon(toElement, true);
        self.removeItemFromList(team.val);
    };
    EditGroup.prototype.removeItemFromList = function (dataId) {
        var elements = $("#dragTeamList li");
        if (elements && elements.length > 0) {
            elements.each(function (index, elem) {
                if (+($(elem).attr("data-id")) == dataId) {
                    $(elem).remove();
                }
            });
        }
    };
    EditGroup.prototype.setAllTeamsArr = function () {
        var self = this;
        var value = $("#Type").val();
        if (value == "0") {
            self.teams = [];
            $('select#allteams>option')
                .each(function (e, elem) {
                self.teams.push({ text: $(elem).text(), val: $(elem).val() });
            });
            self.changePairsStatus("hide");
        }
        else if (value == "1") {
            self.teams = [];
            $('select#allathletes>option')
                .each(function (e, elem) {
                self.teams.push({ text: $(elem).text(), val: $(elem).val() });
            });
            self.changePairsStatus("hide");
        }
        else if (value == "2") {
            self.changePairsStatus("show");
        }
        else {
            self.teams = [];
            $('select#allteams>option')
                .each(function (e, elem) {
                self.teams.push({ text: $(elem).text(), val: $(elem).val() });
            });
        }
    };
    EditGroup.prototype.getSelectedTeamsArr = function () {
        var self = this;
        self.editableTeams = [];
        $('select#allselectedathletes>option')
            .each(function (e, elem) {
            self.editableTeams.push({ text: $(elem).text(), val: $(elem).val() });
        });
        self.changePairsStatus("hide");
    };
    EditGroup.prototype.filterPlayers = function () {
        var self = this;
        var startDate = $("#Athtletes_AgeStart").val();
        var endDate = $("#Athtletes_AgeEnd").val();
        var weightType = $("#weightSelector").val();
        var weightFrom = $("#Athtletes_WeightFrom").val();
        var weightTo = $("#Athtletes_WeightTo").val();
        var selectedRanks = $("#Sports").val();
        var leagueId = $("#LeagueId").val();
        var seasonId = $("#SeasonId").val();
        var isAgesEnabled = $("#IsAgesEnabled").is(":checked");
        var isRankedEnabled = $("#IsRankedEnabled").is(":checked");
        var isWeightEnabled = $("#IsWeightEnabled").is(":checked");
        var stageId = $("#StageId").val();
        var selectedAthletesIds = [];
        var selectedPlayoffAthletesIds = [];
        $("#dragToSelectedTeams > div > div").each(function (e, elem) {
            selectedAthletesIds.push(Number($(elem).attr("data-id")));
        });
        $(".match > div").each(function (e, elem) {
            selectedPlayoffAthletesIds.push(Number($(elem).attr("data-id")));
        });
        $.ajax({
            type: "POST",
            url: "/Groups/FilterAthletes",
            data: {
                ageStart: startDate,
                ageEnd: endDate,
                WeightFrom: weightFrom,
                weightTo: weightTo,
                weightType: weightType,
                selectedRanks: selectedRanks,
                leagueId: leagueId,
                seasonId: seasonId,
                isAgesEnabled: isAgesEnabled,
                isRankedEnabled: isRankedEnabled,
                isWeightEnabled: isWeightEnabled,
                selectedAthletesIds: selectedAthletesIds,
                selectedPlayoffAthletesIds: selectedPlayoffAthletesIds,
                stageId: stageId
            },
            beforeSend: function () {
                $("#spinner").show();
            },
            complete: function () {
                $('#spinner').hide();
            },
            success: function (data) {
                self.teams = [];
                if (data.FiltredAthletes.length != 0) {
                    for (var i = 0; i < data.FiltredAthletes.length; i++) {
                        self.teams.push({ text: data.FiltredAthletes[i].Text, val: data.FiltredAthletes[i].Value });
                    }
                }
                $('#dragTeamList').empty().append(self.fillInTeamList(self.teams)).bind(self);
            }
        });
    };
    EditGroup.prototype.changeAllTeamsArrValues = function () {
        var self = this;
        var value = $("#Type").val();
        if (value == "0") {
            self.teams = [];
            $('select#allteams>option')
                .each(function (e, elem) {
                self.teams.push({ text: $(elem).text(), val: $(elem).val() });
            });
            self.changePairsStatus("hide");
        }
        else if (value == "1") {
            self.teams = [];
            $('select#allathletes>option')
                .each(function (e, elem) {
                self.teams.push({ text: $(elem).text(), val: $(elem).val() });
            });
            self.changePairsStatus("hide");
        }
        else if (value == "2") {
            self.changePairsStatus("show");
        }
    };
    EditGroup.prototype.changePairsStatus = function (status) {
        var buttonDiv = $("#pairsdiv");
        var deleteButtonDiv = $("#pairsdivremove");
        var pairsDiv = $("#dragPairsList");
        var randomLotteryDiv = $("#randomLotteryDiv");
        var rankedLotteryDiv = $("#rankedLotteryDiv");
        if (buttonDiv && pairsDiv) {
            switch (status) {
                case "show": {
                    buttonDiv.show();
                    pairsDiv.show();
                    deleteButtonDiv.show();
                    randomLotteryDiv.hide();
                    rankedLotteryDiv.hide();
                    $("#AllowIncomplete").hide();
                    $("#allowIncompleteLabel").hide();
                    $("#saveBtnForCompetitionGroup").attr("disabled", "disabled");
                    break;
                }
                case "hide": {
                    buttonDiv.hide();
                    pairsDiv.hide();
                    deleteButtonDiv.hide();
                    randomLotteryDiv.show();
                    rankedLotteryDiv.show();
                    $("#AllowIncomplete").show();
                    $("#allowIncompleteLabel").show();
                    $("#saveBtnForCompetitionGroup").removeAttr("disabled");
                    break;
                }
            }
        }
    };
    EditGroup.prototype.getIdOfList = function () {
        var value = $("#Type").val();
        var listIdValue = (value == "0") ? "allteams" : "allathletes";
        return listIdValue;
    };
    EditGroup.prototype.mainDropDownListSelectActions = function (mainDropDownListval, isInit) {
        if (mainDropDownListval === void 0) { mainDropDownListval = null; }
        this.currGameTypeId = mainDropDownListval;
        var listIdValue = this.getIdOfList();
        if (isInit) {
            this.setFormData();
        }
        else {
            this.moveSelected('selectedteams', listIdValue);
            this.setDragListDragSelectedItems();
        }
        if (mainDropDownListval == GameTypeId.Division) {
            $("#temp").show();
        }
        else {
            $("#temp").hide();
        }
        $('#moveTeamButtons').show();
        $('#dragToSelectedTeams').removeClass('col-xs-8').addClass('col-xs-6');
        if ($("#Type").val() == "2") {
            $("#dragPairsList li").each(function (index, elem) {
                $(elem).remove();
            });
        }
        $('[data-toggle="tooltip"]').tooltip();
    };
    EditGroup.prototype.setDragListDragSelectedItems = function () {
        var self = this;
        $('#dragTeamList').empty().append(self.fillInTeamList(self.teams)).bind(self);
        $('#dragToSelectedTeams').empty();
        if (this.currGameTypeId == GameTypeId.Division) {
            self.createDivisionIndex($('#NumberOfTeams').val());
        }
        else {
            self.createBrackets($('#NumberOfTeams').val());
        }
        $('[data-toggle="tooltip"]').tooltip();
        $(".ranking").hide();
        if ($("#Type").val() == "2") {
            $("#dragPairsList li").each(function (index, elem) {
                $(elem).remove();
            });
            RETURN_MESSAGE_TOTAL = $("#NumberOfTeams").closest("div").find("label").text();
            $("#NumberOfTeams").closest("div").find("label").text(MESSAGE_TOTAL_PAIRS);
        }
        else if ($("#Type").val() == "1") {
            $("#NumberOfTeams").closest("div").find("label").text(MESSAGE_TOTAL_PLAYERS);
        }
    };
    EditGroup.prototype.checkPairsDivStatus = function () {
        if ($("#Type").val() == "2") {
            var elements = $("#dragTeamList :checkbox").filter(":checked");
            if (elements.length == 2) {
                $("#pairsdiv :button").removeAttr("disabled");
                $("#dragTeamList li").attr("draggable", "False");
            }
            else {
                $("#pairsdiv :button").attr("disabled", "disabled");
                $("#dragTeamList li").attr("draggable", "True");
            }
        }
    };
    EditGroup.prototype.fillInTeamList = function (array) {
        //Fastest way to add elements to dom
        var list = [];
        for (var i = 0; i < array.length; i++) {
            list[list.length] = this.getTeamListElement(i, array[i].val, array[i].text);
        }
        var result = "<ol>" + list.join('') + "</ol>";
        return result;
    };
    EditGroup.prototype.getTeamListElement = function (index, dataId, text) {
        return this.currGameTypeId == GameTypeId.Division ?
            "<li id=\"tlist" + index + "\" data-id=\"" + dataId + "\" draggable=\"True\">\n                  <input type=checkbox id=\"tcb" + index + "\"><label for=\"tcb" + index + "\">" + text + "</label>\n                </li>" :
            "<li id=\"tlist" + index + "\" data-id=\"" + dataId + "\" draggable=\"True\"><input type=checkbox id=\"tcb" + index + "\"><label for=\"tcb" + index + "\">" + text + "</label></li>";
    };
    EditGroup.prototype.createDivisionIndex = function (teamsCount) {
        var html = "";
        for (var i = 1; i <= teamsCount; i++) {
            html += "\n                <div data-index=\"" + i + "\" class=\"player  player- hover\" data-id=\"\">\n                    <div id=\"br" + i + "\" class=\"dropItems  droptarget\">\n                    </div>\n                    <span class=\"ranking\">" + i + "</span>\n                    <div class=\"text-holder\">\n                        <span class=\"playername\"></span>\n                        <span class=\"removePlayer glyphicon glyphicon-minus\" style=\"display:none;\" data-toggle=\"tooltip\" data-placement=\"top\" title=\"\" data-container=\"body\" data-original-title=\"Remove Team\"></span>\n                    </div>\n                </div>";
        }
        $("#dragToSelectedTeams").append("<div class=\"container-division-index\">" + html + "</div>");
    };
    EditGroup.prototype.createBrackets = function (teamsCount) {
        var rounds = [];
        var slotsAvailable = teamsCount;
        var roundsCount = this.getRoundsCount(this.getBracketsTeamsNr(teamsCount));
        var maxRound = roundsCount - 1;
        var nums = {
            4: [1, 4, 3, 2],
            8: [1, 8, 5, 4, 3, 6, 7, 2],
            16: [1, 16, 9, 8, 5, 12, 13, 4, 3, 14, 11, 6, 7, 10, 15, 2],
            32: [1, 32, 16, 17, 9, 24, 8, 25, 5, 28, 21, 12, 20, 13, 29, 4, 3, 30, 19, 14, 11, 22, 27, 6, 26, 7, 23, 10, 15, 18, 31, 2],
            64: [1, 64, 32, 33, 17, 48, 16, 49, 9, 56, 24, 41, 25, 40, 8, 57, 4, 61, 29, 36, 20, 45, 13, 52, 12, 53, 21, 44, 28, 37, 5, 60, 2, 63, 31, 34, 18, 47, 15, 50, 10, 55, 23, 42, 26, 39, 7, 58, 3, 62, 30, 35, 19, 46, 14, 51, 11, 54, 22, 43, 27, 38, 6, 59]
        };
        while (roundsCount--) {
            var matches = [];
            if (roundsCount === 0) {
                var match = {
                    player1: {
                        name: "",
                        winner: false,
                        ID: 0
                    }
                };
                matches.push(match);
            }
            else {
                var teamsToAdd = Math.pow(2, roundsCount);
                for (var i = 0; i < teamsToAdd; i += 2) {
                    matches.push({
                        player1: {
                            name: "", winner: false, ID: 0, droptarget: maxRound === roundsCount, maxSlots: slotsAvailable,
                            number: (maxRound === roundsCount ? nums[teamsToAdd][i] : undefined)
                        },
                        player2: {
                            name: "", winner: false, ID: 0, droptarget: maxRound === roundsCount, maxSlots: slotsAvailable,
                            number: (maxRound === roundsCount ? nums[teamsToAdd][i + 1] : undefined)
                        }
                    });
                }
            }
            rounds.push(matches);
        }
        $("#dragToSelectedTeams").brackets({
            titles: false,
            rounds: rounds,
            color_title: 'black',
            border_color: '#ccc',
            color_player: 'black',
            bg_player: 'white',
            color_player_hover: 'black',
            bg_player_hover: '',
            border_radius_player: '0',
            border_radius_lines: '0',
            brackets_width: 120
        });
    };
    EditGroup.prototype.getRoundsCount = function (teamsCount) {
        return Math.log(teamsCount) / Math.log(2) + 1;
    };
    EditGroup.prototype.getBracketsTeamsNr = function (number) {
        if (number < 5)
            return 4;
        if (number < 9)
            return 8;
        if (number < 17)
            return 16;
        if (number < 33) {
            return 32;
        }
        if (number < 65) {
            return 64;
        }
        else {
            return 4;
        }
    };
    EditGroup.prototype.removeListeners = function () {
        var self = this;
        $("#groupsedit").off("hidden.bs.modal");
        $(document).off('click', '.removePlayer', (self.removePlayerAddToTeamList).bind(self));
        $('#MainDropDownListID').off('change');
        $('#NumberOfTeams').off('change');
        document.removeEventListener("dragstart", self.dragStart);
        document.removeEventListener("dragenter", (self.dragenter).bind(self));
        document.removeEventListener("dragend", self.dragend);
        document.removeEventListener("dragleave", self.dragleave);
        document.removeEventListener("drop", self.drop);
        document.removeEventListener("dragover", self.dragover);
    };
    EditGroup.prototype.changeDisableStatusForPairs = function () {
        var needEnable = false;
        $("#dragPairsList :checkbox").each(function (index, elem) {
            if ($(elem).is(":checked") == true) {
                needEnable = true;
            }
        });
        if (needEnable) {
            $("#pairsdivremove button").removeAttr("disabled");
        }
        else {
            $("#pairsdivremove button").attr("disabled", "disabled");
        }
    };
    EditGroup.prototype.removeSelectedPairs = function () {
        var self = this;
        $("#dragPairsList :checkbox").each(function (index, elem) {
            self.removePair($(elem).parent(), true);
            $(elem).parent().remove();
        });
    };
    EditGroup.prototype.expandView = function () {
        var self = this;
        $('#groupsedit .modal-dialog').toggleClass('coverScreen');
        $('#groups_fieldsContainer').slideToggle();
        var heightToSubtract = $('#groupsedit .modal-header').outerHeight() +
            $('#groupsedit .modal-footer').outerHeight();
        ($('#dragTeamList ol').height(self.getElemHeight('300', self.viewExpanded, self.calculateNewHeight(heightToSubtract + 60)))).bind(self);
        ($('#dragToSelectedTeams').height(self.getElemHeight('300', self.viewExpanded, self.calculateNewHeight(heightToSubtract + 60)))).bind(self);
        ($('#groupsedit .modal-body').height(self.getElemHeight(self.calculateNewHeight(heightToSubtract + 35), !self.viewExpanded, null))).bind(self);
        self.viewExpanded = !self.viewExpanded;
    };
    EditGroup.prototype.getElemHeight = function (height, isExpanded, defaultHeight) {
        return isExpanded ? height : defaultHeight ? defaultHeight : 'auto';
    };
    EditGroup.prototype.calculateNewHeight = function (heightToSubtract) {
        return $(window).innerHeight() - heightToSubtract;
    };
    EditGroup.prototype.removePlayerAddToTeamList = function (ev) {
        ev.preventDefault();
        var self = this;
        var toElement = $(ev.target).parents('.player');
        if ($("#Type").val() != "2") {
            var dataId = Number(toElement.attr('data-id'));
            if (dataId) {
                var listIndex = toElement[0].dataset.listindex;
                var index = +listIndex.replace('tlist', '');
                var newElem = $(this.getTeamListElement(index, dataId, toElement.find('.playername').text()));
                var parentNode = $('#dragTeamList>ol')[0];
                if (index == 0) {
                    parentNode.insertBefore(newElem[0], parentNode.childNodes[0]);
                }
                else {
                    var referenceNode = EditGroup.getClosestElem(index - 1);
                    if (referenceNode == 0) {
                        referenceNode = +parentNode.childNodes[0];
                    }
                    parentNode.insertBefore(newElem[0], referenceNode == undefined ? null : referenceNode.nextSibling);
                }
            }
            toElement.attr({
                'data-id': '',
                'data-listindex': ''
            }).find('.playername').text('');
            toElement.find('.dropItems').removeAttr('draggable');
            EditGroup.showRemovePlayerIcon(toElement, false);
        }
        else {
            self.removePair(toElement, false);
            $("#saveBtnForCompetitionGroup").attr("disabled", "disabled");
        }
    };
    EditGroup.prototype.removePair = function (toElement, fromPairsDiv) {
        var dataIds = toElement.attr("data-id").split('/');
        var names = fromPairsDiv ? toElement.find('span').text().split('/') : toElement.find('.playername').text().split('/');
        for (var i = 0; i < 2; i++) {
            var dataId = Number(dataIds[i]);
            if (dataId) {
                var listIndex = fromPairsDiv ? toElement[0].dataset.id : toElement[0].dataset.listindex;
                var index = +listIndex.replace('tlist', '');
                var newElem = $(this.getTeamListElement(index, dataId, names[i].trim()));
                var parentNode = $('#dragTeamList>ol')[0];
                if (index == 0) {
                    parentNode.insertBefore(newElem[0], parentNode.childNodes[0]);
                }
                else {
                    var referenceNode = EditGroup.getClosestElem(index - 1);
                    if (referenceNode == 0) {
                        referenceNode = +parentNode.childNodes[0];
                    }
                    parentNode.insertBefore(newElem[0], referenceNode == undefined ? null : referenceNode.nextSibling);
                }
            }
            toElement.attr({
                'data-id': '',
                'data-listindex': ''
            }).find('.playername').text('');
            toElement.find('.dropItems').removeAttr('draggable');
            EditGroup.showRemovePlayerIcon(toElement, false);
        }
    };
    EditGroup.showRemovePlayerIcon = function (elem, show) {
        elem.find('.glyphicon-minus').css('display', show ? 'block' : 'none');
    };
    EditGroup.getClosestElem = function (index) {
        var startIndex = 0;
        $('#dragTeamList>ol>li')
            .each(function () {
            var newIndex = +($(this).attr('id').replace('tlist', ''));
            if (newIndex <= index) {
                startIndex = +($(this)[0]);
            }
            else {
                return startIndex;
            }
        });
        return startIndex;
    };
    EditGroup.prototype.dragStart = function (ev) {
        ev.dataTransfer.setData("text", ev.target.id);
        event.target.style.opacity = "0.8";
        document.getElementById(ev.target.id).style.border = '2px solid #db520c';
        setTimeout((function () {
            return function () {
                document.getElementById(ev.target.id).style.border = '';
            };
        })(), 1);
    };
    EditGroup.prototype.dragenter = function (event) {
        if (event.target.className.indexOf('droptarget') !== -1) {
            event.target.style.border = "2px solid #db520c";
        }
    };
    EditGroup.prototype.dragend = function (event) {
        event.target.style.opacity = "1";
    };
    EditGroup.prototype.dragleave = function (event) {
        if (event.target.className.indexOf('droptarget') !== -1) {
            event.target.style.border = "";
        }
    };
    EditGroup.prototype.dragover = function (ev) {
        if (event.target.className.indexOf('droptarget') !== -1) {
            ev.preventDefault();
        }
    };
    EditGroup.prototype.drop = function (ev) {
        debugger;
        if (!ev.dataTransfer)
            return;
        ev.preventDefault();
        var toElem = ev.target;
        if (toElem.className.indexOf('droptarget') !== -1) {
            toElem.style.border = "";
            var fromData = ev.dataTransfer.getData("text");
            var fromElem = $('#' + fromData);
            if (EditGroup.swapBracketItems(fromElem, $(toElem))) {
                return;
            }
            var toElement = $(toElem).parent('.player');
            this.moveTeamToBrackets(fromElem, toElement, fromData);
        }
        var isPairs = $("#Type").val() == "2";
        var self = this;
        if (isPairs) {
            var filledCount = self.getNumberOfFilledPlaces();
            var necessaryCount = +($("#NumberOfTeams").val());
            if (filledCount == necessaryCount) {
                $("#saveBtnForCompetitionGroup").removeAttr("disabled");
            }
            else {
                $("#saveBtnForCompetitionGroup").attr("disabled", "disabled");
            }
        }
    };
    EditGroup.prototype.moveTeamToBrackets = function (fromElem, toElement, text) {
        var dataId = Number(toElement.attr('data-id'));
        if (dataId) {
            var listIndex = toElement[0].dataset.listindex;
            var index = +listIndex.replace('tlist', '');
            var newElem = $(this.getTeamListElement(index, dataId, toElement.find('.playername').text()));
            var parentNode = $('#dragTeamList>ol')[0];
            if (index == 0) {
                parentNode.insertBefore(newElem[0], parentNode.childNodes[0]);
            }
            else {
                var referenceNode = EditGroup.getClosestElem(index - 1);
                if (referenceNode == 0) {
                    referenceNode = +parentNode.childNodes[0];
                }
                parentNode.insertBefore(newElem[0], referenceNode == undefined ? null : referenceNode.nextSibling);
            }
        }
        toElement.attr({
            'data-id': fromElem.attr('data-id'),
            'data-listindex': text
        }).find('.playername').text(fromElem.text());
        toElement.find('.dropItems').attr('draggable', 'True');
        EditGroup.showRemovePlayerIcon(toElement, true);
        fromElem.remove();
    };
    EditGroup.swapBracketItems = function (from, to) {
        if (!EditGroup.areBracketItems(from, to)) {
            return false;
        }
        var toParent = to.parent('.player');
        var fromParent = from.parent('.player');
        var fromParentDataId = fromParent.attr('data-id');
        var fromPlayerName = fromParent.find('.playername').text();
        var fromDataListIndex = fromParent.attr('data-listindex');
        if (toParent.attr('data-id')) {
            fromParent.attr({
                'data-id': toParent.attr('data-id'),
                'data-listindex': toParent.attr('data-listindex')
            })
                .find('.playername')
                .text(toParent.find('.playername').text());
        }
        else {
            fromParent.attr({
                'data-id': '',
                'data-listindex': ''
            })
                .find('.playername')
                .text('');
            fromParent.find('.dropItems').removeAttr('draggable');
            EditGroup.showRemovePlayerIcon(fromParent, false);
        }
        toParent.attr({
            'data-id': fromParentDataId,
            'data-listindex': fromDataListIndex
        }).find('.playername').text(fromPlayerName);
        toParent.find('.dropItems').attr('draggable', 'True');
        EditGroup.showRemovePlayerIcon(toParent, true);
        return true;
    };
    EditGroup.areBracketItems = function (from, to) {
        return from.attr('id').indexOf('br') !== -1 && to.attr('id').indexOf('br') !== -1;
    };
    EditGroup.prototype.setFormData = function () {
        var self = this;
        var teamsNr = $('#selectedteams >option').length;
        var listIdValue = this.getIdOfList();
        $("#NumberOfTeams option").each(function () {
            if ($(this).val() == teamsNr) {
                $(this).prop('selected', true);
            }
        });
        var realCount = $("#GroupTeamsAndHiddenCount").val();
        ($('#dragTeamList').empty().append(self.fillInTeamList(self.teams))).bind(self);
        if (teamsNr == 0) {
            teamsNr = $("#NumberOfTeams").val();
        }
        if (self.isRankedOption) {
            teamsNr = realCount;
        }
        if (this.currGameTypeId == GameTypeId.Knockout || this.currGameTypeId == GameTypeId.Knockout34 || this.currGameTypeId == GameTypeId.Playoff || this.currGameTypeId == GameTypeId.Knockout34Consolences1Round || this.currGameTypeId == GameTypeId.Knockout34ConsolencesUntilQuarter) {
            self.createBrackets(teamsNr);
        }
        else {
            self.createDivisionIndex(teamsNr);
        }
        if (self.isRankedOption) {
            self.setSelectedTeamsArrForEdit();
            $("#allselectedathletes option").prop('selected', true);
            self.moveSelected('allselectedathletes', listIdValue);
            $("#" + listIdValue + " option").prop('selected', false);
        }
        else {
            self.setSelectedTeamsArr();
            $("#selectedteams option").prop('selected', true);
            self.moveSelected('selectedteams', listIdValue);
            $("#" + listIdValue + " option").prop('selected', false);
        }
    };
    EditGroup.prototype.setSelectedTeamsArr = function () {
        var self = this;
        var indextoAssign = self.teams.length;
        var indexOfBrackets = 1;
        $('#selectedteams > option')
            .each(function (e, elem) {
            if ($(elem).text().length > 0) {
                var elementSelector = self.currGameTypeId == GameTypeId.Division ?
                    ".container-division-index [data-index=\"" + indexOfBrackets + "\"]" :
                    ".round.rd-1 [data-index=\"" + indexOfBrackets + "\"]";
                var elementTo = $(elementSelector);
                elementTo.attr({
                    'data-id': $(elem).val(),
                    'data-listindex': 'tlist' + indextoAssign++
                }).find('.playername').text($(elem).text());
                elementTo.find('.dropItems').prop('draggable', 'True');
                self.teams.push({ text: $(elem).text(), val: $(elem).val() });
                EditGroup.showRemovePlayerIcon(elementTo, true);
            }
            indexOfBrackets++;
        });
    };
    EditGroup.prototype.setSelectedTeamsArrForEdit = function () {
        var self = this;
        var indextoAssign = self.teams.length;
        var indexOfBrackets = 1;
        $('#allselectedathletes > option')
            .each(function (e, elem) {
            if ($(elem).text().length > 0) {
                var elementSelector = self.currGameTypeId == GameTypeId.Division ?
                    ".container-division-index [data-index=\"" + indexOfBrackets + "\"]" :
                    ".round.rd-1 [data-index=\"" + indexOfBrackets + "\"]";
                var elementTo = $(elementSelector);
                elementTo.attr({
                    'data-id': $(elem).val(),
                    'data-listindex': 'tlist' + indextoAssign++
                }).find('.playername').text($(elem).text());
                elementTo.find('.dropItems').prop('draggable', 'True');
                self.teams.push({ text: $(elem).text(), val: $(elem).val() });
                EditGroup.showRemovePlayerIcon(elementTo, true);
            }
            indexOfBrackets++;
        });
    };
    EditGroup.prototype.moveSelected = function (from, to) {
        $('#' + from + ' option:selected').remove().appendTo('#' + to);
    };
    EditGroup.prototype.selectAllTeams = function () {
        $("input[type=checkbox]").prop("checked", true);
    };
    EditGroup.prototype.addSelectedTeams = function () {
        var self = this;
        self.isRankedOption = false;
        var emptyPlaces = self.getEmptySpaces(self);
        var isPairs = $("#Type").val() == "2";
        if (isPairs) {
            var hasSelected_1 = false;
            $('#dragPairsList ol li').each(function (index, elem) {
                if ($(elem).children().is(":checked"))
                    hasSelected_1 = true;
            });
            if (!hasSelected_1)
                alert(MESSAGE_ALERT_SELECT_PAIR_FIRST);
        }
        if (emptyPlaces.length > 0) {
            var targetIndex_1 = 0;
            $(isPairs ? '#dragPairsList ol li' : '#dragTeamList ol li').each(function (index, elem) {
                if ($(elem).find(':checkbox')[0]['checked']) {
                    if (targetIndex_1 >= emptyPlaces.length)
                        return;
                    if (self.currGameTypeId == GameTypeId.Division) {
                        self.moveTeamToBrackets($(elem), $(emptyPlaces[targetIndex_1]), $(elem).val());
                    }
                    else {
                        if ($(emptyPlaces[targetIndex_1]).parent().attr("data-id") == "") {
                            self.moveTeamToBrackets($(elem), $(emptyPlaces[targetIndex_1]).parent(), $(elem).val());
                        }
                    }
                    targetIndex_1++;
                }
            });
        }
        if (isPairs) {
            var filledCount = self.getNumberOfFilledPlaces();
            var necessaryCount = +($("#NumberOfTeams").val());
            if (filledCount == necessaryCount) {
                $("#saveBtnForCompetitionGroup").removeAttr("disabled");
            }
            else {
                $("#saveBtnForCompetitionGroup").attr("disabled", "disabled");
            }
        }
        self.IndexSelectedList();
    };
    EditGroup.prototype.IndexSelectedList = function () {
        var selectedlist = $('#selectedteams option');
        var listIdValue = this.getIdOfList();
        for (var i = 0; i < selectedlist.length; i++) {
            var element = $(selectedlist[i]);
            var index = i + 1;
            var clearText = this.removeIndexFromElement(element);
            element.text(clearText);
            element.attr("index", index + " ");
            element.text(element.attr("index") + element.text());
        }
        var allList = $('#' + listIdValue + ' option');
        for (var i = 0; i < allList.length; i++) {
            var element = $(allList[i]);
            var clearText = this.removeIndexFromElement(element);
            element.text(clearText);
            element.attr("index", null);
            element.text(clearText);
        }
    };
    EditGroup.prototype.selectAllInSelectedList = function () {
        var self = this;
        var players;
        if (this.currGameTypeId == GameTypeId.Playoff || this.currGameTypeId == GameTypeId.Knockout || this.currGameTypeId == GameTypeId.Knockout34 || this.currGameTypeId == GameTypeId.Knockout34Consolences1Round || this.currGameTypeId == GameTypeId.Knockout34ConsolencesUntilQuarter) {
            players = $('.round.rd-1>.match >.player');
        }
        else {
            players = $('.container-division-index > .player');
        }
        var arr = [];
        $(players).each(function () {
            var dataIndex = +$(this).attr('data-index');
            var numberOfTeams = +$('#NumberOfTeams').val();
            if (dataIndex) {
                if (dataIndex > numberOfTeams && self.isRankedOption == false)
                    return;
                if ($(this).attr('data-id')) {
                    arr[dataIndex - 1] = $(this).attr('data-id');
                }
                else {
                    arr[dataIndex - 1] = '';
                }
            }
        });
        var length = arr.length;
        var selectedTeams = $('#selectedteams')[0];
        $(selectedTeams).empty();
        arr.forEach(function (value, index, array) {
            var option = document.createElement('option');
            option.value = arr[index];
            option.innerHTML = '';
            selectedTeams.appendChild(option);
        });
        //$('#allteams option[value="' + arr[i] + '"]').prop('selected', true);
        //this.moveSelected('allteams', 'selectedteams');
        $("#selectedteams option").prop('selected', true);
    };
    EditGroup.prototype.removeIndexFromElement = function (element) {
        var elementText = element.text();
        if (element.attr("index")) {
            var txtIndex = elementText.indexOf(element.attr("index"));
            var txtLength = element.attr("index").length;
            elementText = elementText.replace(elementText.substring(txtIndex, txtLength), "");
        }
        return elementText;
    };
    EditGroup.prototype.addNewPair = function () {
        var self = this;
        var selectedPlayers = $("#dragTeamList :checkbox").filter(":checked");
        if (selectedPlayers.length == 2) {
            var appendedText = "";
            var players = self.getPairPlayersArray(selectedPlayers);
            var dataIds = "" + players[0].id + "/" + players[1].id;
            var names = "" + players[0].playerName + " / " + players[1].playerName;
            var numberOfElement = self.getLastTlistNumber() + 1;
            appendedText += '<li id="tlist' + numberOfElement + '" data-id="' + dataIds + '" data-pair="True" draggable="True">';
            appendedText += '<input type="checkbox" id="tcb' + numberOfElement + '">';
            appendedText += '<span>' + names + '</span>';
            appendedText += '</li>';
            $("#dragPairsList ol").append(appendedText);
            self.removePlayersFromCommonList(selectedPlayers);
        }
        $("#pairsdiv :button").attr("disabled", "disabled");
    };
    EditGroup.prototype.removePlayersFromCommonList = function (selectedPlayers) {
        $(selectedPlayers).each(function (index, elem) {
            $(elem).closest("li").remove();
        });
    };
    EditGroup.prototype.getPairPlayersArray = function (selectedPlayers) {
        var players = [];
        $(selectedPlayers).each(function (index, elem) {
            var id = $(elem).closest("li").attr("data-id");
            var playerName = $(elem).closest("li").text().trim();
            players.push({
                id: id,
                playerName: playerName
            });
        });
        return players;
    };
    EditGroup.prototype.getLastTlistNumber = function () {
        var elements = $("#dragTeamList li");
        var pairElementsLength = $("#dragPairsList li").length;
        var lastElement = elements[elements.length - 1];
        var idOfElement = $(lastElement).attr("id");
        for (var i = 0; i < idOfElement.length; i++) {
            var charEl = idOfElement.charAt(i);
            if (!isNaN(Number(charEl))) {
                return Number(charEl) + pairElementsLength;
            }
        }
        return null;
    };
    EditGroup.prototype.getNumberOfFilledPlaces = function () {
        var countOfCurrentFilledPlayers = 0;
        $(".player").each(function (index, elem) {
            var attr = $(elem).attr("data-id");
            if (attr)
                countOfCurrentFilledPlayers++;
        });
        return countOfCurrentFilledPlayers;
    };
    return EditGroup;
}());
window.editGroupNsp = new EditGroup();
setTimeout(function () { window.editGroupNsp.init(); }, 150);
$('#groupform').validateBootstrap(true);
$('.updownbtn').click(function () {
    var $op = $('#selectedteams option:selected'), $this = $(this);
    if ($op.length) {
        ($this.data('val') == 'Up') ?
            $op.first().prev().before($op) :
            $op.last().next().after($op);
        window.editGroupNsp.IndexSelectedList();
    }
});
//# sourceMappingURL=editgroups.js.map