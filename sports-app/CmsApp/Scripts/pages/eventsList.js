var EventList = /** @class */ (function () {
    function EventList(cmn) {
        this.commonLib = cmn;
    }
    EventList.prototype.eventsOpen = function () {
        document.getElementById("eventsOpen").style.display = 'none';
        document.getElementById("events_tbl").style.display = 'block';
        document.getElementById("eventsClose").style.display = 'block';
    };
    EventList.prototype.eventsClose = function () {
        document.getElementById("eventsOpen").style.display = 'block';
        document.getElementById("events_tbl").style.display = 'none';
        document.getElementById("eventsClose").style.display = 'none';
    };
    EventList.prototype.publishEvent = function (eventId) {
        var eventCheckBoxId = "#EventChbx_" + eventId;
        var currCheckBox = $(eventCheckBoxId)[0];
        var data = {
            eventId: eventId,
            isPublished: $(currCheckBox).is(':checked')
        };
        $.post('/Events/PublishEvent', data).fail(function (resp) {
            $(currCheckBox).prop('checked', !data.isPublished);
            alert("Error: " + resp.responseText);
            console.log("Response: ", resp);
        });
    };
    EventList.prototype.UpdateEvent = function (eventId) {
        var event_line = $("table#events_tbl tbody tr#event_" + eventId);
        var dStr = $(event_line).find("input[name=EventTime]").val();
        dStr = dStr.replace(/(\d{2})\/(\d{2})\/(\d{4}) (\d{2}):(\d{2})/, "$2/$1/$3 $4:$5:00");
        var d = new Date(Date.parse(dStr));
        var evTimeStr = d.getFullYear() + '-' + ("0" + (d.getMonth() + 1)).slice(-2) + '-' + ("0" + d.getDate()).slice(-2)
            + ' ' + ("0" + (d.getHours())).slice(-2) + ':' + ("0" + d.getMinutes()).slice(-2) + ':' + ("0" + d.getSeconds()).slice(-2);
        var data = {
            EventId: eventId,
            Place: $(event_line).find("input[name=Place]").val(),
            Title: $(event_line).find("input[name=Title]").val(),
            EventTime: evTimeStr,
            IsPublished: $(event_line).find("input[name=IsPublished]").is(":checked"),
            LeagueId: $(event_line).find("input[name=LeagueId]").val(),
            ClubId: $(event_line).find("input[name=ClubId]").val(),
            CreateDate: $(event_line).find("input[name=CreateDate]").val(),
            UnionId: $(event_line).find("input[name=UnionId]").val(),
        };
        var form_data = new FormData();
        for (var key in data) {
            form_data.append(key, data[key]);
        }
        if (data.UnionId != null) {
            form_data.append("EventDescription", $(event_line).find("textarea[name=EventDescription]").val());
            form_data.append("RemoveEventImageFile", $(event_line).find("input[name=RemoveImage]").val());
        }
        var imageFileInput = $(event_line).find("input:file")[0];
        if (imageFileInput && imageFileInput.files.length > 0) {
            form_data.append("ImageFile", imageFileInput.files[0]);
        }
        //$.post("/Events/UpdateEvent", data, function (response) {
        //    if (response.stat === 'ok') {
        //        $(event_line).find("a[name=savebtn]").attr('disabled', 'disabled');
        //    }
        //});
        $.ajax({
            url: '/Events/UpdateEvent',
            data: form_data,
            processData: false,
            contentType: false,
            type: 'POST'
        }).done(function (data) {
            if (data.stat === 'ok') {
                $(event_line).find("a[name=savebtn]").attr('disabled', 'disabled');
                if (data.imgChanged) {
                    $(event_line).find("img[name=event-img]")[0].style.visibility = 'visible';
                    $(event_line).find("img[name=event-img]")[0].src = data.imgPath;
                    $(event_line).find("input:file")[0].value = "";
                    $($($(event_line).find("input:file").parent()[0]).find("span")[0]).text("Browse");
                    $(event_line).find("div[name=remove-photo]")[0].style.visibility = 'visible';
                }
            }
        });
    };
    EventList.prototype.hideModalDialog = function () {
        var dial = $('#addevent');
        if (($(dial).data('bs.modal') || {}).isShown) {
            $(dial).modal('hide');
        }
    };
    EventList.prototype.documentReady = function () {
        $('#events_tbl tbody tr').each(function () {
            var me = $(this);
            var btn = $('[name=savebtn]', me);
            $('.form-control', me).change(function () {
                btn.attr('disabled', null);
            });
            $("input:file", me).change(function () {
                btn.attr('disabled', null);
            });
            $('.frm-date', me).on('changedatetime.xdsoft', function (e) {
                btn.attr('disabled', null);
            });
            var rmFile = $('div[name=remove-photo]', me);
            if (rmFile != null) {
                var file = $('img[name=event-img]', me)[0];
                var rmImg = $('input[name=RemoveImage]', me)[0];
                rmFile.on('click', function () {
                    file.style.visibility = 'hidden';
                    rmImg.value = 'true';
                    btn.attr('disabled', null);
                });
            }
        });
        this.commonLib.initDateTimePickers();
    };
    return EventList;
}());
window.evList = new EventList(window.cmn);
//# sourceMappingURL=eventsList.js.map