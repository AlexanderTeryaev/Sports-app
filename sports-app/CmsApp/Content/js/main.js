var cmsMainLib = (function ($, win, doc) {

    var currLang = "en";

    return {
        init: function (lang) {

            currLang = lang;

            setGlobals();
            setMenu();
        }
    }

    function setGlobals() {

        $(document).on("focus", ":input", function () {
            $(this).attr("autocomplete", "off");
        });

        $(".filters .combo").change(function () {
            $(".filters").submit();
        });

        $("body").on("hidden.bs.modal", ".modal", function () {
            $(this).removeData("bs.modal");
        });

        $(".btn-back").click(function () {
            win.history.back();
        });

        $("a.submit").click(function () {
            $(this).closest("form").submit();
        });

        $(".check-all").click(function () {
            var groupName = $(this).data("group");
            $("[name=" + groupName + "]").prop("checked", this.checked);
        });

        $(".active-check").click(function () {
            var url = $(this).data("act");
            $.post(url, { val: this.checked });
        });

        $(".top-tabs li").each(function (i) {
            var num = $(this).parent().data("val");
            if (i == num)
                $(this).addClass("active");
        });

        $("input:reset, button:reset").click(function () {

            var $form = $(this).closest("form");

            resetForm($form.id);
        });

        $("#search_opn_btn").click(function () {
            $(".search-panel").slideToggle();
        });

        $(".btn-submit").click(function () {

            var formId = $(this).data("form");
            var action = $(this).data("acturl");

            $("#" + formId).prop("action", action).submit();
        });

        // buttons titles
        $(".grid-mvc .btn-edit").attr("title", "עריכה");
        $(".grid-mvc .btn-delete").attr("title", "מחיקה");
        $(".grid-mvc .btn-open").attr("title", "צפה");

        $(doc).on("click", ".btn-act, .btn-delete", function (e) {

            var me = $(this);
            var message = me.data("alert");
            var formId = me.data("form");
            var action = me.data("acturl");

            if (message == null)
                message = "יש לאשר פעולה זאת";

            $("#pop_alert").text(message);

            $("#confirm").modal({ backdrop: "static" })
                .one("click", "#confirm", function (e) {

                    if (formId == "link") {
                        var url = me.attr("href");
                        window.location.href = url;
                    }
                    else if (formId == "ajaxlink") {
                        var updateId = me.data("updateid");
                        $.post(me.attr("href"), function (data) {
                            $("#" + updateId).html(data);
                        });
                    }
                    else {
                        $("#" + formId).prop("action", action);
                        $("#" + formId).submit();
                    }

                    $("#confirm").modal("hide");
                });
            return false;
        });

        // validators // ------------------------------------------------------------

        // validate password
        $.validator.unobtrusive.adapters.addBool("pass");
        $.validator.addMethod("pass", function (value, element, params) {

            var passed = validatePassword($(element).val(), {
                length: [8, Infinity],
                alpha: 1,
                numeric: 1,
                badSequenceLength: 4
            });

            return passed;
        }, "");

        // validate file name
        //$.validator.unobtrusive.adapters.addBool('validfile');
        //$.validator.addMethod('validfile', function (value, el) {

        //    var extArr = $(el).data('rule-files').split('|');
        //    var ext = value.toLowerCase().split('.').pop();

        //    return this.optional(el) || extArr.indexOf(ext) > -1;

        //}, function (params, el) {
        //    return $(el).data('msg-files') + " (" + $(el).data('rule-files') + ")"
        //});

        // validate email
        $.validator.unobtrusive.adapters.addBool("custemail");
        $.validator.addMethod("custemail", function (value, element) {

            var re = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
            return this.optional(element) || re.test(value);
        }, $.validator.messages.email);
        
        //validate date
        $.validator.methods.date = function (value, element) {

            var bits = value.match(/([0-9]+)/gi), str;
            if (!bits)
                return this.optional(element) || false;
            str = bits[1] + "/" + bits[0] + "/" + bits[2];
            return this.optional(element) || !/Invalid|NaN/.test(new Date(str));
        };

        //validate ID
        $.validator.unobtrusive.adapters.addBool("custid");
        $.validator.addMethod("custid", function (value, element) {
            var re = /^([0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9])$/;
            return this.optional(element) || re.test(value);
        }, $.validator.messages.iddigits);

        // validate only numbers
        $.validator.unobtrusive.adapters.addBool("custpassport");
        $.validator.addMethod("custpassport", function (value, element) {

            var re = /^\d+$/;
            return this.optional(element) || re.test(value);
        }, $.validator.messages.numberPassport);

        // validators end ------------------------------------------------------------


        // autocomplete --------------------------------------------------------------

        setAutoSearch();

        // --- file upload events -----------------------------------------------------

        $(doc).on("change", ".btn-file :file", function () {
            var input = $(this),
                numFiles = input.get(0).files ? input.get(0).files.length : 1,
                label = input.val().replace(/\\/g, "/").replace(/.*\//, "");
            input.trigger("fileselect", [numFiles, label]);
        });

        $(doc).on("fileselect", ".btn-file :file", function (event, numFiles, label) {

            var el = $(this).parent().find("span");
            var txt = el.data("label");

            if (label.length > 0)
                el.text(label);
            else
                el.text(txt);
        });

        $(doc).on("change", ".glyph-btn :file", function () {
            var input = $(this),
                label = input.val().replace(/\\/g, "/").replace(/.*\//, "");
            $("#" + this.name + "Name").html(label);
        });
    }

    function getNameFromUrl() {
        var url = window.location.pathname;
        var filename = url.substring(url.lastIndexOf("/") + 1);
        return filename;
    }

    function setMenu() {

        var fName = window.location.pathname.toLowerCase();
        var isFound = false;

        $(".nav-sidebar li").not(":first-child").each(function () {

            var aUrl = $("a", $(this)).attr("href");
            if (aUrl)
                aUrl = aUrl.toLowerCase();

            if (fName.indexOf(aUrl) > -1) {
                $(this).addClass("active");
                isFound = true;
                return false;
            }
        });

        if (!isFound) {
            $(".nav-sidebar li").eq(0).addClass("active");
        }
    }

})(jQuery, window, document);

function numbersonly(e, canX) {
    if (canX == undefined) {
        canX = false;
    }
    var unicode = e.charCode ? e.charCode : e.keyCode
    if ((unicode != 8 && !canX) || (canX && (unicode != 8 && unicode != 88 && unicode != 120)))  { //if the key isn't the backspace key (which we should allow) except when also X key allowed
        if (unicode < 48 || unicode > 57) //if not a number
            return false //disable key press
    }
}

function setAutoSearch() {
    $(".auto-search").each(function () {

        var me = $(this);
        var searchUrl = me.data("remote");
        var navUrl = me.data("link");
        var field = me.data("field");

        var items = new Bloodhound({
            datumTokenizer: function (datum) {
                return Bloodhound.tokenizers.whitespace(datum.Name);
            },
            queryTokenizer: Bloodhound.tokenizers.whitespace,
            remote: {
                url: searchUrl,
                prepare: function (query, sett) {
                    sett.type = "POST";
                    sett.contentType = "application/json; charset=UTF-8";
                    sett.data = JSON.stringify({
                        term: query
                    });
                    return sett;
                }
            }
        });

        me.typeahead({
            hint: false,
            highlight: true,
            minLength: 2
        },
            {
                name: "states",
                display: "Name",
                limit: 200,
                source: items
            });

        me.bind("typeahead:select",
            function (ev, res) {

                if (navUrl) {
                    window.location.href = navUrl +
                        "/" +
                        res.Id + "?" +
                        (res.SeasonId !== undefined && res.SeasonId !== null && res.SeasonId !== 0
                            ? "&seasonId=" + res.SeasonId
                            : "") +
                        (res.TeamId !== undefined && res.TeamId !== null && res.TeamId !== 0
                            ? "&teamId=" + res.TeamId
                            : "") +
                        (res.LeagueId !== undefined && res.LeagueId !== null && res.LeagueId !== 0
                            ? "&leagueId=" + res.LeagueId
                            : "") +
                        (res.ClubId !== undefined && res.ClubId !== null && res.ClubId !== 0
                            ? "&clubId=" + res.ClubId
                            : "");
                } else if (field) {
                    $("#" + field).val(res.Id);
                }
            });
    });

    $(".auto-search-players").each(function () {

        var me = $(this);
        var searchUrl = me.data("remote");
        var navUrl = me.data("link");
        var field = me.data("field");

        var items = new Bloodhound({
            datumTokenizer: function (datum) {
                return Bloodhound.tokenizers.whitespace(datum.Name);
            },
            queryTokenizer: Bloodhound.tokenizers.whitespace,
            remote: {
                url: searchUrl,
                prepare: function (query, sett) {
                    sett.type = "POST";
                    sett.contentType = "application/json; charset=UTF-8";
                    sett.data = JSON.stringify({
                        term: query
                    });
                    return sett;
                }
            }
        });

        me.typeahead({
            hint: false,
            highlight: true,
            minLength: 2
        },
            {
                name: "states",
                display: "Name",
                limit: 200,
                source: items
            });

        me.bind("typeahead:select",
            function (ev, res) {

                if (navUrl) {
                    window.location.href =
                        navUrl +
                        "/" +
                        res.Id +
                        "?seasonId=" +
                        res.SeasonId +
                        "&teamId=" +
                        res.TeamId +
                        (res.LeagueId !== undefined && res.LeagueId !== null && res.LeagueId !== 0
                            ? "&leagueId=" + res.LeagueId
                            : "") +
                        (res.ClubId !== undefined && res.ClubId !== null && res.ClubId !== 0
                            ? "&clubId=" + res.ClubId
                            : "");
                } else if (field) {
                    $("#" + field).val(res.Id);
                }
            });
    });
}
function setNavSearch() {
    $(".auto-search-players-nav-bar").each(function () {
        var me = $(this);
        var searchUrl = me.data("remote");
        var navUrl = me.data("link");
        var field = me.data("field");

        var items = new Bloodhound({
            datumTokenizer: function (datum) {
                return Bloodhound.tokenizers.whitespace(datum.Name);
            },
            queryTokenizer: Bloodhound.tokenizers.whitespace,
            remote: {
                url: searchUrl,
                rateLimitWait: 2000,
                prepare: function (query, sett) {
                    sett.type = "POST";
                    sett.contentType = "application/json; charset=UTF-8";
                    sett.data = JSON.stringify({
                        term: query
                    });
                    return sett;
                }
            }
        });

        me.typeahead({
            hint: false,
            highlight: true,
            minLength: 2
        },
            {
                name: "states",
                display: "Name",
                limit: 200,
                source: items
            });

        me.bind("typeahead:select",
            function (ev, res) {

                if (navUrl) {
                    //window.location.href
                    var target_url =
                        navUrl +
                        "/" +
                        res.Id +
                        "?seasonId=" +
                        res.SeasonId +
                        "&teamId=" +
                        res.TeamId +
                        (res.LeagueId !== undefined && res.LeagueId !== null && res.LeagueId !== 0
                            ? "&leagueId=" + res.LeagueId
                            : "") +
                        (res.ClubId !== undefined && res.ClubId !== null && res.ClubId !== 0
                            ? "&clubId=" + res.ClubId
                            : "");

                    window.open(target_url, '_blank');

                } else if (field) {
                    $("#" + field).val(res.Id);
                }
            });
    });
}

function resetForm(objId) {
    $form = $("#" + objId);

    $(":input", $form)
   .not(":button, :submit, :reset, :hidden")
   .val("")
   .removeAttr("checked");

    $("select", $form).prop("selectedIndex", 0);
}

function setFileUpload(formName, place) {

    var bar = $(".progress-bar");
    var percent = $(".progress-bar");
    var status = $(place);

    var targetId = $(formName).data("targetid");

    $(formName).ajaxForm({
        beforeSend: function () {
            var percentVal = "0%";
            bar.width(percentVal)
            percent.html(percentVal);
        },
        uploadProgress: function (event, position, total, percentComplete) {
            var percentVal = percentComplete + "%";
            bar.width(percentVal)
            percent.html(percentVal);
        },
        success: function (responseText, statusText, xhr, form) {
            var percentVal = "100%";
            bar.width(percentVal)
            percent.html(percentVal);

            var tarId = form.data("targetid");
            $("#" + tarId).html(responseText);
        },
        complete: function (xhr) {
            status.html(xhr.responseText);
        },
        clearForm: "true",
        beforeSubmit: validator
    });

    function validator() {
        return $(formName).valid();
    }
}

$.fn.loadWith = function (u) {
    var groupsRefreshUrl = u;
    var groupsRefreshUrl = groupsRefreshUrl.replace(/&amp;/g, "&");
    var c = $(this);
    $.get(groupsRefreshUrl, function (d) {
        c.empty();
        c.html(d);
    });
};

function doRefresh() {
    window.location.href = window.location.href;
}

function getCookie(cname) {
    var name = cname + "=";
    var decodedCookie = decodeURIComponent(document.cookie);
    var ca = decodedCookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}
function setCookie(cname, cvalue, exdays) {
    var d = new Date();
    d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
    var expires = "expires=" + d.toUTCString();
    document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
}


jQuery.extend(jQuery.fn.dataTableExt.oSort, {
    "date-eu-pre": function (date) {
        date = date.replace(" ", "");

        if (!date) {
            return 0;
        }

        var year;
        var eu_date = date.split(/[\.\-\/]/);

        /*year (optional)*/
        if (eu_date[2]) {
            year = eu_date[2];
        }
        else {
            year = 0;
        }

        /*month*/
        var month = eu_date[1];
        if (month.length == 1) {
            month = 0 + month;
        }

        /*day*/
        var day = eu_date[0];
        if (day.length == 1) {
            day = 0 + day;
        }

        return (year + month + day) * 1;
    },

    "date-eu-asc": function (a, b) {
        return ((a < b) ? -1 : ((a > b) ? 1 : 0));
    },

    "date-eu-desc": function (a, b) {
        return ((a < b) ? 1 : ((a > b) ? -1 : 0));
    },

    "input-inner-pre": function (data) {
        var doc = new DOMParser();
        var dataParsed = doc.parseFromString(data, 'text/html');
        var firstDiv = dataParsed.body.firstChild;
        var id = firstDiv.id;
        var value = $("#" + id).val();
        return value;
    },

    "input-inner-select-pre": function (data) {
        var doc = new DOMParser();
        var dataParsed = doc.parseFromString(data, 'text/html');
        var firstDiv = dataParsed.body.firstChild;
        var text = $(firstDiv).find("option:selected").text();
        return text;
    },


    "input-inner-asc": function (a, b) {
        if (b.length == 0 && a.length > 0) {
            return -1;
        }
        if (b.length > 0 && a.length == 0) {
            return 1;
        }
        if (!isNaN(a) && !isNaN(b)) {
            ap = Number(a);
            bp = Number(b);
            return ((ap < bp) ? -1 : ((ap > bp) ? 1 : 0));
        }
        return ((a < b) ? -1 : ((a > b) ? 1 : 0));
    },

    "input-inner-desc": function (a, b) {

        if (!isNaN(a) && !isNaN(b)) {
            ap = Number(a);
            bp = Number(b);
            return ((ap < bp) ? 1 : ((ap > bp) ? -1 : 0));
        }
        return ((a < b) ? 1 : ((a > b) ? -1 : 0));
    },

    "input_inner_num-asc": function (a, b) {

        var doc = new DOMParser();
        var dataParsed1 = doc.parseFromString(a, 'text/html');
        var firstDiv1 = dataParsed1.body.firstChild;
        var id1 = firstDiv1.id;
        var value1 = $("#" + id1).val();

        var dataParsed2 = doc.parseFromString(b, 'text/html');
        var firstDiv2 = dataParsed2.body.firstChild;
        var id2 = firstDiv2.id;
        var value2 = $("#" + id2).val();

        if (isNaN(parseFloat(value1))) {
            return 1;
        }
        if (isNaN(parseFloat(value2))) {
            return -1;
        }

        var aNum = parseFloat(value1);
        var bNum = parseFloat(value2);

        return ((aNum < bNum) ? -1 : ((aNum > bNum) ? 1 : 0));

    },
    "input_inner_num-desc": function (a, b) {
        var doc = new DOMParser();
        var dataParsed1 = doc.parseFromString(a, 'text/html');
        var firstDiv1 = dataParsed1.body.firstChild;
        var id1 = firstDiv1.id;
        var value1 = $("#" + id1).val();

        var dataParsed2 = doc.parseFromString(b, 'text/html');
        var firstDiv2 = dataParsed2.body.firstChild;
        var id2 = firstDiv2.id;
        var value2 = $("#" + id2).val();

        if (isNaN(parseFloat(value1))) {
            return 1;
        }
        if (isNaN(parseFloat(value2))) {
            return -1;
        }

        var aNum = parseFloat(value1);
        var bNum = parseFloat(value2);

        return ((aNum > bNum) ? -1 : ((aNum < bNum) ? 1 : 0));
    }

});


function validatePassword(pw, options) {
    var prev = "";
    var curr = "";
    var o = {
        lower: 0,
        upper: 0,
        alpha: 0,
        numeric: 0,
        special: 0,
        length: [0, Infinity],
        custom: [],
        badWords: [],
        badSequenceLength: 0,
        noQwertySequences: false,
        noSequential: false,
        noRepeatChars: false
    };

    for (var property in options)
        o[property] = options[property];
    var re = {
        lower: /[a-z]/g,
        upper: /[A-Z]/g,
        alpha: /[A-Z]/gi,
        numeric: /[0-9]/g,
        special: /[\W_]/g
    }, rule, i;

    if (o.noRepeatChars) {
        for (i = 0; i < pw.length; i++) {
            curr = pw[i];
            if (curr == prev) {
                //alert(pw[i]);
                return (false);
            }
            else {
                prev = curr;
            }
        }
    }
    if (pw.length < o.length[0] || pw.length > o.length[1])
        return false;
    for (rule in re) {
        if ((pw.match(re[rule]) || []).length < o[rule])
            return false;
    }
    for (i = 0; i < o.badWords.length; i++) {
        if (pw.toLowerCase().indexOf(o.badWords[i].toLowerCase()) > -1)
            return false;
    }
    if (o.noSequential && /([\S\s])\1/.test(pw))
        return false;
    if (o.badSequenceLength) {
        var lower = "abcdefghijklmnopqrstuvwxyz",
            upper = lower.toUpperCase(),
            numbers = "0123456789",
            qwerty = "qwertyuiopasdfghjklzxcvbnm",
            start = o.badSequenceLength - 1,
            seq = "_" + pw.slice(0, start);
        for (i = start; i < pw.length; i++) {
            seq = seq.slice(1) + pw.charAt(i);
            if (
                lower.indexOf(seq) > -1 ||
                upper.indexOf(seq) > -1 ||
                numbers.indexOf(seq) > -1 ||
                (o.noQwertySequences && qwerty.indexOf(seq) > -1)
            ) {
                return false;
            }
        }
    }
    for (i = 0; i < o.custom.length; i++) {
        rule = o.custom[i];
        if (rule instanceof RegExp) {
            if (!rule.test(pw))
                return false;
        } else if (rule instanceof Function) {
            if (!rule(pw))
                return false;
        }
    }
    return true;
}

function applyFormatFieldsRestriction() {
    $(document).on('focusout', 'input[format]', function (e) {
        var input = $(this);
        var recordId = input.data("recordid");
        var inputVal = input.val();
        var format = input.attr("format");
        if (inputVal.length > 0) {
            if (format == 6 || format == 10) {
                var zeroResult = "0.00";
                if (inputVal.length < zeroResult.length) {
                    var restFormat = zeroResult.substring(inputVal.length);
                    inputVal = inputVal + restFormat;
                }
            }
            if (format == 11 || format == 7 || format == 1) {
                var zeroResult = "00.00";
                if (inputVal.length < zeroResult.length) {
                    var restFormat = zeroResult.substring(inputVal.length);
                    inputVal = inputVal + restFormat;
                }
            }
            if (format == 8) {
                var zeroResult = "0,000";
                if (inputVal.length < zeroResult.length) {
                    var restFormat = zeroResult.substring(inputVal.length);
                    inputVal = inputVal + restFormat;
                }
            }
            if (format == 9) {
                var zeroResult = "00:00";
                if (inputVal.length < zeroResult.length) {
                    var restFormat = zeroResult.substring(inputVal.length);
                    inputVal = inputVal + restFormat;
                }
            }
            if (format == 5) {
                var zeroResult = "00:00:00";
                if (inputVal.length < zeroResult.length) {
                    var restFormat = zeroResult.substring(inputVal.length);
                    inputVal = inputVal + restFormat;
                }
            }
            if (format == 4) {
                var zeroResult = "00:00:00.00";
                if (inputVal.length < zeroResult.length) {
                    var restFormat = zeroResult.substring(inputVal.length);
                    inputVal = inputVal + restFormat;
                }
            }
            if (format == 3) {
                var zeroResult = "00:00.00";
                if (inputVal.length < zeroResult.length) {
                    var restFormat = zeroResult.substring(inputVal.length);
                    inputVal = inputVal + restFormat;
                }
            }
            if (format == 2) {
                var zeroResult = "0:00.00";
                if (inputVal.length < zeroResult.length) {
                    var restFormat = zeroResult.substring(inputVal.length);
                    inputVal = inputVal + restFormat;
                }
            }
        }
        input.val(inputVal);
        $("#submit_save_record_button_" + recordId).removeAttr("disabled");
    });

    $(document).on('keydown', 'input[format]', function (e) {
        var input = $(this);
        var oldVal = input.val();
        var format = input.attr("format");

        if (format == 6 || format == 10) {
            var pattern = /^\d{0,1}(\.\d{0,2})?$/g;
            var regex = new RegExp(pattern);
            setTimeout(function () {
                var newVal = input.val();
                if (!regex.test(newVal)) {
                    input.val(oldVal);
                } else {
                    var n = newVal.indexOf(".");
                    if (n == 0) {
                        newVal = "0" + newVal;
                        input.val(newVal);
                    }
                    if (n == -1 && newVal.length == 1) {
                        newVal = newVal + ".";
                        input.val(newVal);
                    }
                }
            }, 0);
        }
        if (format == 11 || format == 7 || format == 1) {
            var pattern = /^\d{0,2}(\.\d{0,2})?$/g;
            var regex = new RegExp(pattern);

            setTimeout(function () {
                var newVal = input.val();
                if (!regex.test(newVal)) {
                    input.val(oldVal);
                } else {

                    var n = newVal.indexOf(".");
                    if (n == 0) {
                        newVal = "00" + newVal;
                        input.val(newVal);
                    }
                    else if (n == 1) {
                        newVal = "0" + newVal;
                        input.val(newVal);
                    }
                    if (n == -1 && newVal.length == 2) {
                        newVal = newVal + ".";
                        input.val(newVal);
                    }
                }
            }, 0);
        }
        if (format == 8) {
            var pattern = /^\d{0,1}(\,\d{0,3})?$/g;
            var regex = new RegExp(pattern);
            setTimeout(function () {
                var newVal = input.val();
                if (!regex.test(newVal)) {
                    input.val(oldVal);
                } else {
                    var n = newVal.indexOf(".");
                    if (n == 0) {
                        newVal = "0" + newVal;
                        input.val(newVal);
                    }
                    if (n == -1 && newVal.length == 1) {
                        newVal = newVal + ",";
                        input.val(newVal);
                    }
                }
            }, 0);
        }
        if (format == 9) {
            var pattern = /^\d{0,2}(\:\d{0,2})?$/g;
            var regex = new RegExp(pattern);

            setTimeout(function () {
                var newVal = input.val();
                if (!regex.test(newVal)) {
                    input.val(oldVal);
                } else {

                    var n = newVal.indexOf(":");
                    if (n == 0) {
                        newVal = "00" + newVal;
                        input.val(newVal);
                    }
                    else if (n == 1) {
                        newVal = "0" + newVal;
                        input.val(newVal);
                    }
                    if (n == -1 && newVal.length == 2) {
                        newVal = newVal + ":";
                        input.val(newVal);
                    }
                }
            }, 0);
        }
        if (format == 5) {
            var pattern = /^\d{0,2}(\:\d{0,2})?(\:\d{0,2})?$/g;
            var regex = new RegExp(pattern);

            setTimeout(function () {
                var newVal = input.val();
                if (!regex.test(newVal)) {
                    input.val(oldVal);
                } else {

                    var n = newVal.indexOf(":");
                    var ln = newVal.lastIndexOf(":");
                    if (n == 0) {
                        newVal = "00" + newVal;
                        input.val(newVal);
                    }
                    else if (n == 1) {
                        newVal = "0" + newVal;
                        input.val(newVal);
                    }
                    if (n == -1 && newVal.length == 2) {
                        newVal = newVal + ":";
                        input.val(newVal);
                    }
                    if (ln == n && newVal.length == 5) {
                        newVal = newVal + ":";
                        input.val(newVal);
                    }
                }
            }, 0);
        }
        if (format == 4) {
            var pattern = /^\d{0,2}(\:\d{0,2})?(\:\d{0,2})?(\.\d{0,2})?$/g;
            var regex = new RegExp(pattern);

            setTimeout(function () {
                var newVal = input.val();
                if (!regex.test(newVal)) {
                    input.val(oldVal);
                } else {

                    var n = newVal.indexOf(":");
                    var ln = newVal.lastIndexOf(":");
                    var nn = newVal.indexOf(".");
                    if (n == 0) {
                        newVal = "00" + newVal;
                        input.val(newVal);
                    }
                    else if (n == 1) {
                        newVal = "0" + newVal;
                        input.val(newVal);
                    }
                    if (n == -1 && newVal.length == 2) {
                        newVal = newVal + ":";
                        input.val(newVal);
                    }
                    if (ln == n && newVal.length == 5) {
                        newVal = newVal + ":";
                        input.val(newVal);
                    }
                    if (nn == -1 && newVal.length == 8) {
                        newVal = newVal + ".";
                        input.val(newVal);
                    }
                }
            }, 0);
        }
        if (format == 3) {
            var pattern = /^\d{0,2}(\:\d{0,2})?(\.\d{0,2})?$/g;
            var regex = new RegExp(pattern);

            setTimeout(function () {
                var newVal = input.val();
                if (!regex.test(newVal)) {
                    input.val(oldVal);
                } else {
                    var n = newVal.indexOf(":");
                    var nn = newVal.indexOf(".");
                    if (n == 0) {
                        newVal = "00" + newVal;
                        input.val(newVal);
                    }
                    else if (n == 1) {
                        newVal = "0" + newVal;
                        input.val(newVal);
                    }
                    if (n == -1 && newVal.length == 2) {
                        newVal = newVal + ":";
                        input.val(newVal);
                    }
                    if (nn == -1 && newVal.length == 5) {
                        newVal = newVal + ".";
                        input.val(newVal);
                    }
                }
            }, 0);
        }
        if (format == 2) {
            var pattern = /^\d{0,1}(\:\d{0,2})?(\.\d{0,2})?$/g;
            var regex = new RegExp(pattern);

            setTimeout(function () {
                var newVal = input.val();
                if (!regex.test(newVal)) {
                    input.val(oldVal);
                } else {
                    var n = newVal.indexOf(":");
                    var nn = newVal.indexOf(".");
                    if (n == 0) {
                        newVal = "0" + newVal;
                        input.val(newVal);
                    }
                    if (n == -1 && newVal.length == 1) {
                        newVal = newVal + ":";
                        input.val(newVal);
                    }
                    if (nn == -1 && newVal.length == 4) {
                        newVal = newVal + ".";
                        input.val(newVal);
                    }
                }
            }, 0);
        }
    });


}

$(document).ready(function () {
    applyFormatFieldsRestriction();
});
