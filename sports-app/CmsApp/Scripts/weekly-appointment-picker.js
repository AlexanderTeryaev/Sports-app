document.addEventListener("click", function (event) {
    var popupElements = document.getElementsByClassName("popuptext");
    for (var i = 0; i < popupElements.length; i++) {
        if (popupElements[i].classList.contains("show") && popupElements[i] != event.target.parentElement) {
            popupElements[i].classList.remove("show");
            dayOptionSelected = popupElements[i].children[0].querySelector("option:checked");
            hourOptionSelected = popupElements[i].children[1].querySelector("option:checked");
            popupElements[i].parentElement.children[0].innerText = dayOptionSelected.innerText + " " + hourOptionSelected.innerText;
            var date = new Date(0);
            date.setMonth(1);
            var ms = date.getTime() + 86400000 * dayOptionSelected.value + 1000 * 60 * 60 * hourOptionSelected.value;
            var newDate = new Date(ms);
            popupElements[i].parentElement.setAttribute("time", newDate.getTime());
            $(popupElements[i].parentElement).data('onClosePicker')();
        }
    }
});

function closeOpenedPopups(element) {
    var popupElements = document.getElementsByClassName("popuptext");
    for (var i = 0; i < popupElements.length; i++) {
        if (popupElements[i].classList.contains("show") && popupElements[i] != element) {
            popupElements[i].classList.remove("show");
            dayOptionSelected = popupElements[i].children[0].querySelector("option:checked");
            hourOptionSelected = popupElements[i].children[1].querySelector("option:checked");
            popupElements[i].parentElement.children[0].innerText = dayOptionSelected.innerText + " " + hourOptionSelected.innerText;
            var date = new Date(0);
            date.setMonth(1);
            var ms = date.getTime() + 86400000 * dayOptionSelected.value + 1000 * 60 * 60 * hourOptionSelected.value;
            var newDate = new Date(ms);
            popupElements[i].parentElement.setAttribute("time", newDate.getTime());
            $(popupElements[i].parentElement).data('onClosePicker')();
        }
    }
}


// When the user clicks on div, open the popup
function appointmentButton(event) {
    event.stopPropagation();
    closeOpenedPopups(event.target);
    if (event.target.parentElement.children[1].classList.contains("show")) {
        var popupElement = event.target.parentElement.children[1];
        popupElement.classList.remove("show");
        dayOptionSelected = popupElement.children[0].querySelector("option:checked");
        hourOptionSelected = popupElement.children[1].querySelector("option:checked");
        popupElement.parentElement.children[0].innerText = dayOptionSelected.innerText + " " + hourOptionSelected.innerText;
        var date = new Date(0);
        date.setMonth(1);
        var ms = date.getTime() + 86400000 * dayOptionSelected.value + 1000 * 60 * 60 * hourOptionSelected.value;
        var newDate = new Date(ms);
        popupElement.parentElement.setAttribute("time", newDate.getTime());
        $(popupElement.parentElement).data('onClosePicker')();
    } else
        event.target.parentElement.children[1].classList.add("show");
}


$(document).ready(function () {
    var pickerButtons = $(".weekly-appointment-picker");
    var days = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
    var days_he = ['ראשון', 'שני', 'שלישי', 'רביעי', 'חמישי', 'שישי', 'שבת'];
    var isHebrew = false;
    if (isHeb == 'True') {
        isHebrew = true;
    }
    pickerButtons.each(function (i, obj) {
        var $button = $(obj);
        $button.data('onClosePicker', function () {
        });

        $button.data('onSetDate', function (date) {
            var dayNum = date.getDay();
            var hoursNum = date.getHours();
            $button.find("select[name='day']").val(dayNum);
            $button.find("select[name='hour']").val(hoursNum);
            $button.attr("time", date.getTime());
            var dayOptionSelected = obj.children[1].children[0].querySelector("option:checked");
            var hourOptionSelected = obj.children[1].children[1].querySelector("option:checked");
            obj.children[0].innerText = dayOptionSelected.innerText + " " + hourOptionSelected.innerText;

        });

        var buttonName = "Pick Time";
        var name = $button.attr("name");
        if (name.length > 0) {
            buttonName = name;
        }
        var currentDay = days;
        if (isHebrew) {
            currentDay = days_he;
        }

        var daysOption = "";
        for (var i = 0; i < currentDay.length; i++) {
            daysOption = daysOption + "<option value='" + i + "'>" + currentDay[i] + "</option>";
        }
        
        $button.html(` 
            <a href="#" onclick="appointmentButton(event)" class="btn btn-primary">` + buttonName + `</a>
                <div class="popuptext" id="dayPopup">
                    <select class="days" name="day">` + daysOption +
                    `</select>
                    <select class="hour" name="hour">
                        <option value="0">0:00</option>
                        <option value="1">1:00</option>
                        <option value="2">2:00</option>
                        <option value="3">3:00</option>
                        <option value="4">4:00</option>
                        <option value="5">5:00</option>
                        <option value="6">6:00</option>
                        <option value="7">7:00</option>
                        <option value="8">8:00</option>
                        <option value="9">9:00</option>
                        <option value="10">10:00</option>
                        <option value="11">11:00</option>
                        <option value="12">12:00</option>
                        <option value="13">13:00</option>
                        <option value="14">14:00</option>
                        <option value="15">15:00</option>
                        <option value="16">16:00</option>
                        <option value="17">17:00</option>
                        <option value="18">18:00</option>
                        <option value="19">19:00</option>
                        <option value="20">20:00</option>
                        <option value="21">21:00</option>
                        <option value="22">22:00</option>
                        <option value="23">23:00</option>
                    </select>
                </div>
        `);
    });
});



