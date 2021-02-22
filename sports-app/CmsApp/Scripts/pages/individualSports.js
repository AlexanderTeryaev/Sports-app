function changeType() {
    var value = $("#Type").val();
    switch (value) {
        case "0": {
            $("#filterAthletes").hide();
            $("#NrOfTeams > div > label").text(numOfTeams);
            $("#groupform > div.modal-body > div:nth-child(2) > div.row.dragHeaders > div:nth-child(1) > label")
                .text(teamList);
            $("#groupform > div.modal-footer > label").text(incompleteGroupTeams);
            break;
        }
        case "1": {
            $("#filterAthletes").show();
            $("#NrOfTeams > div > label").text(numOfAthl);
            $("#groupform > div.modal-body > div:nth-child(2) > div.row.dragHeaders > div:nth-child(1) > label")
                .text(athletList);
            $("#groupform > div.modal-footer > label").text(incompleteGroupAthletes);

            break;
        }
    }
}
$(document).ready(function () {
    $(".date-time-generate").datetimepicker({
        format: 'd/m/Y',
        closeOnDateSelect: true,
        timepicker: false,
    });
    $('.ranks').multiselect({
        buttonWidth: '350px',
        numberDisplayed: 5
    });
    changeType();
});

$("#Type").change(function () {
    changeType();
});

$("#IsAgesEnabled").click(function () {
    var isChecked = $("#IsAgesEnabled").is(":checked");
    if (isChecked) {
        $("#ages").show();
    }
    else {
        $("#ages").hide();
    }
});
$("#IsWeightEnabled").click(function () {
    var isChecked = $("#IsWeightEnabled").is(":checked");
    if (isChecked) {
        $("#weight").show();
    }
    else {
        $("#weight").hide();
    }
});
$("#IsRankedEnabled").click(function () {
    var isChecked = $("#IsRankedEnabled").is(":checked");
    if (isChecked) {
        $("#rankes").show();
    }
    else {
        $("#rankes").hide();
    }
});
