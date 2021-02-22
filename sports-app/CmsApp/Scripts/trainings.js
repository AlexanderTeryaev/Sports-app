
function publishToApp(teamId) {
    $.ajax({
        url: "/Teams/PublishToApp",
        type: "POST",
        data: { teamId: teamId }
    }).success(function (data) {
        if (data.Message == "Checked") {
            $('.checkboxes').prop('checked', 'checked');
        }
        else if (data.Message == "Unchecked") {
            $(".checkboxes").removeAttr('checked');
        }
    })
}

function publishByCheckbox(trainingId) {
    var publishValue = $("#isPublished_" + trainingId).is(':checked');
    $.ajax({
        url: "/Teams/PublishTraining",
        type: "POST",
        data: {
            trainingId: trainingId,
            publishValue: publishValue
        }
    })
}

function updateExcel() {
    $('#uploadTeamTrainingBtn').change(function () {
        $(this).closest('form').submit();
    })
}

function redactText(id) {
    var text = $("#content" + id).val();
    $("#contentText").val(text);
    $("#saveTextBtn").click(function () {
        $("#content" + id).val($("#contentText").val());
        changeEnabled(id);
        id = 0;
    })
}

$('#selectAllCheckboxes').click(function () {
    var c = this.checked;
    $('.checkboxes').prop('checked', c);
})

function selectedValueCheck(elem) {
    if (elem.value == "fltr_ranged")
        document.getElementById('dateRangedFilter').style.display = "block";
    else {
        document.getElementById('dateRangedFilter').style.display = "none";
    }
}

function getSelectedAttendanceIds(id) {
    var selectedValues = $('#PlayerIds' + id).val();
    return selectedValues;
}

//Function that allows to delete trainings
function deleteTraining(id) {
    if (confirm("Are you sure you want to delete this training with id " + id + "?")) {
        $.ajax({
            url: "/Teams/DeleteTeamTraining",
            type: "POST",
            data: { id: id },
            success: $(function () {
                $('#tableRow_' + id).remove();
            })
        })
    }
    return false;
}

//Save button change enable function
function changeEnabled(id) {
    $("a[name='savebtn_" + id + "']").removeAttr('disabled');
}

function showMultiselectValues(id) {
    $("#" + id).attr('class', 'btn-group open');
}

function updateTraining(id) {
    var dateToUpdate = $('#trainingDate' + id).val();
    var content = $("#content" + id).val();
    var trainingLine = $('#tableRow_' + id);
    var data = {
        teamId: $('#TeamId').val(),
        id: id,
        title: $('#title' + id).val(),
        date: dateToUpdate,
        auditoriumId: $('#auditorium' + id).val(),
        content: content,
        playersId: getSelectedAttendanceIds(id)
    };
    var form_data = new FormData();
    for (var key in data) {
        form_data.append(key, data[key]);
    }

    form_data.append("isImageDeleted", $(trainingLine).find("input[name=RemoveReport]").val());

    form_data.append("ImageFile", $(trainingLine).find("input:file")[0].files[0]);
    $.ajax({
        url: "/Teams/UpdateTeamTraining",
        type: "POST",
        data: form_data,
        processData: false,
        contentType: false,
        success: function (data) {
            if (data.Message !== "") {
                var result = confirm(data.Message);
                if (result) {
                    $.ajax({
                        url: "/Teams/UpdateTeamTraining",
                        type: "POST",
                        data: {
                            teamId: $('#TeamId').val(),
                            id: id,
                            title: $('#title' + id).val(),
                            date: dateToUpdate,
                            auditoriumId: $('#auditorium' + id).val(),
                            content: $('#content' + id).val(),
                            playersId: getSelectedAttendanceIds(id),
                            dateApproved: true
                        }
                    });
                }
                else {
                    var date = data.Date;
                    $("#trainingDate" + id).val(date);
                }
            }
            $("a[name='savebtn_" + id + "']").attr('disabled', 'disabled');
            if (data.stat === 'ok') {
                if (data.reportChanged) {
                    $(trainingLine).find("img[name=training-report]")[0].style.visibility = 'visible';
                    $(trainingLine).find("img[name=training-report]")[0].src = data.reportPath;
                    $(trainingLine).find("input:file")[0].value = "";
                    $($($(trainingLine).find("input:file").parent()[0]).find("span")[0]).text("Browse");
                    $(trainingLine).find("div[name=remove-report]")[0].style.visibility = 'visible';
                }
            }
        }
    })
}

function filterValues() {
    var seasonId = $("#seasonIdHidden").val();
    $("#trainingsListPage").load('/Teams/Filter',
        {
            teamId: $('#TeamId').val(),
            startFilterDate: $('#startDate').val(),
            endFilterDate: $('#endDate').val(),
            filterValue: $('#dateFilterType option:selected').val(),
            seasonId: seasonId,
            pageNumber: 1,
            pageSize: 10
        });
    }

$(document).ready(function () {

    $(".date-time-training").datetimepicker({
        format: 'd/m/Y H:i',
        formatTime: 'H:i',
        formatDate: 'd/m/Y',
        step: 15,
        closeOnDateSelect: false,
        closeOnTimeSelect: true,
        onChangeDateTime: function() {
            $(this).data("input").trigger("changedatetime.xdsoft");
        }
    });
});