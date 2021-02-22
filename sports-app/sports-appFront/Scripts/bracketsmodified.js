/**
 * Created by alicamargo on 23/05/15.
 */
(function ($) {
    var get_html_team = function (team) {
        var winner = team.winner ? 'winner' : '';
        var ID = team.ID ? team.ID : '';
        var droptarget = team.droptarget ? ' droptarget' : '';
        var showNumber = team.number ? '<span class="ranking">' + team.number + '</span>' : '';
        var html_team = '<div data-index="' + team.number + '" class="team ' + winner + ' ' + droptarget + ' team-' + ID + '" data-id="' + ID + '">' + showNumber;

        if (team.url) {
            html_team += '        <a class="name" href="' + team.url + '">';
            html_team += '           ' + team.name;
            html_team += '        </a>';
        } else {
            html_team += '           <label class="teamname">' + team.name + '</label>';
            var scoreLength = team.score.length;
            if (scoreLength > 0) {
                for (var i = 0; i < team.score.length; i++)
                    html_team += '           <label class="score">' + (team.score[i]) + '</label>';
            } else {
                html_team += '           <label class="score">0</label>';
            }

        }

        html_team += '       </div>';

        return html_team;
    },
        get_html_match = function (match) {
            if (match.CycleId == undefined) {
                match.CycleId = -1;
            }
            var html_match = '   <div class="match" data_cycle="' + match.CycleId + '">';
            if (match.team1)
                html_match += get_html_team(match.team1);

            if (match.team2)
                html_match += get_html_team(match.team2);

            html_match += '   </div>';

            return html_match;
        },
        get_html_separator = function (r, total, extra, isDouble = false, tree_num) {
            total = total / 2;
            if (isDouble) {
                total = total * 2;
            }
            var html_separator = '<div class="separator-brackets rd-' + tree_num + '-' + extra + r + '">';
            for (var s = 1; s <= total; s++) {
                html_separator += '<div class="line"><div class="union"></div></div>';
            }
            html_separator += '</div>';

            return html_separator;

        },
        get_html_header = function (titles, opts) {
            var t1 = opts.rounds[0][0].team1;
            var roundsForEachstage = t1 ? t1.score.length - 1 : 0;
            var html_titles = '';
            if (titles) {
                html_titles += '<div class="brackets-header">';
                if (Array.isArray(titles)) {
                    $.each(titles, function (i, title) {
                        html_titles += '<div class="title" style="width:' + (opts.brackets_width + (roundsForEachstage * 30)) + 'px;">' + title + '</div>';
                    });
                }
                html_titles += '</div>';
            }
            return html_titles;
        },

        get_html = function (opts, titles) {
            var rounds = opts.rounds;
            var html = '';
            var columnMinus = 0;
            if (rounds[0].length == rounds[1].length) {
                titles = [titles[0] + " Losers"].concat(titles);
            }
            html += get_html_header(titles, opts);
            html += '<div class="container-brackets-' + opts.tree_num + '">';

            $.each(rounds, function (r, round) {
                var isDouble = false;
                var extra = "";
                var isExtraType = false;
                var isAdvanced = rounds[r].length > 3;
                if ((r == 0 || r == 2) && isAdvanced && rounds[r].length == rounds[r + 1].length && rounds.length-2 > r) {
                    isDouble = true;
                    extra = "extra-";
                    isExtraType = true;
                    if (r == 2) {
                        extra = "";
                        isDouble = false;
                    }
                }

                var t1 = { ID: null, droptarget: false, name: "--", number: -1, score: [0], winner: false };
                if (round != undefined && round[0] != undefined && round[0].team1 != undefined) {
                    t1 = round[0].team1;
                }
                var roundsForEachstage = t1 ? t1.score.length - 1 : 0;

                if (isExtraType && r == 0) {
                    html += get_html_separator((r + 1), rounds[1].length, "", true, opts.tree_num);
                }

                html += '<div style="width:' + (opts.brackets_width + (roundsForEachstage * 30)) + 'px;" class="round rd-' + opts.tree_num + '-' + extra + (r + 1 - columnMinus) + '">';

                $.each(round, function (r, match) {
                    html += get_html_match(match);
                });

                html += '</div>';
                var sepChange = 0;
                if (r == 2 && isAdvanced && rounds[r].length == rounds[r + 1].length) {
                    extra = "extra-";
                    sepChange = 1;
                }

                if (isExtraType && (r == 0 || r == 2)) {
                    if (rounds.length != (r + 1)) {
                        html += get_html_separator((r + 1 - sepChange - columnMinus), rounds[1].length, extra, isDouble, opts.tree_num);
                    }
                }
                else {
                    if (rounds.length != (r + 1)) {
                        html += get_html_separator((r + 1 - columnMinus), round.length, "", isDouble, opts.tree_num);
                    }
                }
                if (r == 2 && isAdvanced && rounds[r].length == rounds[r + 1].length) {
                    columnMinus = 1;
                }

            });
            html += '</div>';

            return html;
        },

        key = function (i) {
            return (23 * Math.pow(2, (i - 2))) + (20 * Math.pow(2, (i - 3))) + (40 * (Math.pow(2, (i - 3)) - 1));
        },

        get_style_brackets = function (max_brackets, tree_num = 0) {
            var css = '';
            var teamCellHeight = 21;
            var bracketsCellHeight = (teamCellHeight * 2);
            var serieStartNum = 0;
            var linesMrTopTotal = 0;
            var nr = 1;
            max_brackets = max_brackets ? max_brackets : 0;
            for (var i = 1; i < max_brackets; i++) {
                if (i > 1) {
                    serieStartNum = serieStartNum * 2 + 1;
                }
                var mrgnTopSubtraction = i > 2 ? 2 : i;
                var mtr = i === 1 ? 0 : bracketsCellHeight * serieStartNum;
                var mbp = bracketsCellHeight * (serieStartNum * 2 + 1);
                var hl = bracketsCellHeight * (nr + 1);
                linesMrTopTotal = linesMrTopTotal * 2 + 1;
                var brfrstLineMrgTop = (linesMrTopTotal * teamCellHeight + (i !== 1 ? 1 : 0));
                var extraMbp = mbp;
                var extraTop = (bracketsCellHeight * (serieStartNum * 2 + 1));
                extraMbp = mbp * 3;
                extraTop = (bracketsCellHeight * (serieStartNum * 2 + 1));
                css += '.container-brackets-' + tree_num + ' .round.rd-' + tree_num + '-' + i + '{ margin-top: ' + mtr + 'px; }';
                css += '.container-brackets-' + tree_num + ' .round.rd-' + tree_num + '-extra-' + i + '{ margin-top: ' + extraTop + 'px; }';
                css += '   .container-brackets-' + tree_num + ' .round.rd-' + tree_num + '-' + i + ' .match:not(:last-child) { margin-bottom: ' + mbp + 'px; }';
                css += '   .container-brackets-' + tree_num + ' .round.rd-' + tree_num + '-extra-' + i + ' .match:not(:last-child) { margin-bottom: ' + extraMbp + 'px; }';
                css += '.container-brackets-' + tree_num + ' .separator-brackets.rd-' + tree_num + '-' + i + ' .line{';
                css += '   height: ' + (hl - 1) + 'px;';
                css += '}';
                css += '.container-brackets-' + tree_num + ' .separator-brackets.rd-' + tree_num + '-extra-' + i + ' .line{';
                css += '   height: ' + (hl) + 'px;';
                css += '   border: 0px';
                css += '}';
                css += '.container-brackets-' + tree_num + ' .separator-brackets.rd-' + tree_num + '-' + i + ' .line:not(:last-child){';
                css += '   margin-bottom: ' + (hl - 1) + 'px;';
                css += '}';
                css += '.container-brackets-' + tree_num + ' .separator-brackets.rd-' + tree_num + '-extra-' + i + ' .line:not(:last-child){';
                css += '   margin-bottom: ' + (hl) + 'px;';
                css += '   border: 0px';
                css += '}';
                css += '   .container-brackets-' + tree_num + ' .separator-brackets.rd-' + tree_num + '-' + i + ' .line:first-child{ margin-top: ' + (brfrstLineMrgTop - mrgnTopSubtraction) + 'px; }';
                css += '.container-brackets-' + tree_num + ' .separator-brackets.rd-' + tree_num + '-' + i + ' .line .union{';
                //css += '   height: ' + ((hl / 2) - 1) + 'px;';
                css += '   height: ' + ((hl / 2)) + 'px;';
                css += '}';
                css += '   .container-brackets-' + tree_num + ' .separator-brackets.rd-' + tree_num + '-extra-' + i + ' .line:first-child{ margin-top: ' + (brfrstLineMrgTop - mrgnTopSubtraction + 1) + 'px; }';
                css += '.container-brackets-' + tree_num + ' .separator-brackets.rd-' + tree_num + '-extra-' + i + ' .line .union{';
                css += '   height: ' + ((hl / 2)) + 'px;';
                css += '   width: ' + (hl) + 'px;';
                css += '}';
                nr = nr * 2 + 1;
            }

            return css;

        },

        get_width_container = function (max_brackets) {
            return (max_brackets) * 150 + (max_brackets - 1) * 80 - (max_brackets - 1) * 75;
        },

        get_height_container = function (max_brackets) {
            return key(max_brackets + 1);
        },

        set_style = function (styles, rounds) {
            var n_brackets = rounds.length;
            var t1 = rounds[0][0].team1;
            var roundsForEachtgme = t1 ? t1.score.length - 1 : 0;
            var extraWdth = (roundsForEachtgme * 30) * n_brackets;

            var css,
                head = document.head || document.getElementsByTagName('head')[0],
                style = document.createElement('style');
            style.setAttribute('id', 'bracketsStyle-' + styles.tree_num );
            style.setAttribute('class', 'bracketsStyle');
            css = ".left{float: left}";
            css += ".right{float: right}";
            css += ".brackets-header{";
            css += "    height: 30px;";
            css += "}";
            css += ".brackets-header .title{";
            css += "    color: " + styles.color_title + " !important;";
            css += "    box-shadow: 0px 0px 0px 1px" + styles.border_color + ";";
            css += "}";
            css += ".brackets-header .title{ background: #f9f9f9;padding: 3px;float: left; width: " + styles.brackets_width + "px;  margin-right: 63px; text-align: center;}";
            css += ".brackets-header .title:last-child{ margin-right: 0px;}";
            css += ".container-brackets-" + styles.tree_num + " *,";
            css += ".container-brackets-" + styles.tree_num + " *:before,";
            css += ".container-brackets-" + styles.tree_num + " *:after {";
            css += "  -webkit-box-sizing: content-box !important;";
            css += "     -moz-box-sizing: content-box !important;";
            css += "          box-sizing: content-box !important;";
            css += "}";
            css += ".container-brackets-" + styles.tree_num + "{";
            css += "    position: relative;float:left;text-align: left;";
            //css += "    overflow: hidden;";
            css += "    margin-bottom: 10px; margin-top: 10px;";
            css += "    width: " + (get_width_container(n_brackets) + 200 + extraWdth) + "px !important;";
            //css += "    height: " + get_height_container(n_brackets) + "px !important;";
            css += "}";

            css += ".container-brackets-" + styles.tree_num + " .teamname{";
            css += "    width: " + (styles.brackets_width - 40) + "px !important;float:left;overflow: hidden;text-overflow: ellipsis; font-size:12px";
            css += "}";

            css += ".container-brackets-" + styles.tree_num + " .score{";
            css += "        text-align:center;overflow: hidden;float:left;width: 25px !important;border-left:1px solid " + styles.border_color + ";padding: 0px 2px;";
            css += "}";

            css += ".container-brackets-" + styles.tree_num + " .round{ margin-left: 26px; float: left;}";
            css += "    .container-brackets-" + styles.tree_num + " .round:first-child{ margin-left: 0; }";
            css += "    .container-brackets-" + styles.tree_num + " .round .match{color:" + styles.color_team + "}";
            css += "        .container-brackets-" + styles.tree_num + " .round .match:last-child{ margin-bottom: 0px;}";
            css += "        .container-brackets-" + styles.tree_num + " .round .match .team{";
            css += "            box-shadow: 0px 0px 0px 1px " + styles.border_color + ";";
            css += "            border-radius: " + styles.border_radius_team + ";";
            css += "            height: 21px;";
            css += "            padding: 0 5px;";
            css += "            line-height: 21px;";
            css += "            background: " + styles.bg_team + ";";
            css += "white-space: nowrap;";
            css += "overflow: hidden;";
            css += "text-overflow: ellipsis;";
            css += "        }";
            css += "        .container-brackets-" + styles.tree_num + " .round .match .team .ranking{";
            css += "position: absolute;";
            css += "left: -18px;";
            css += "text-align: right;";
            css += "width: 15px;";
            css += "        }";
            css += "            .container-brackets-" + styles.tree_num + " .round .match .team.hover{ background: " + styles.bg_team_hover + "; color: " + styles.color_team_hover + "!important;}";
            css += "                .container-brackets-" + styles.tree_num + " .round .match .team .name{";
            css += "                    text-align: center;";
            css += "                    text-decoration: none;";
            css += "                    cursor: pointer;";
            css += "                   width: 100%;";
            css += "                    margin-left: 10px;";
            css += "                    color: " + styles.color_team + ";";
            css += "                }";
            css += "                .container-brackets-" + styles.tree_num + " .round .match .team.hover a{";
            css += "                    color: " + styles.color_team_hover + " !important;";
            css += "                }";
            css += ".container-brackets-" + styles.tree_num + " .separator-brackets{ width: 38px; float: left;}";
            css += "    .container-brackets-" + styles.tree_num + " .separator-brackets .line{";
            css += "        position:relative;border: 1px solid " + styles.border_color + ";";
            css += "        border-left: none;";
            css += "        border-radius: 0 " + styles.border_radius_lines + " " + styles.border_radius_lines + " 0;";
            css += "    }";
            css += ".container-brackets-" + styles.tree_num + " .separator-brackets .line .union{";
            css += "height: 42px;";
            css += "border-bottom: 1px solid " + styles.border_color + ";";
            css += "position: absolute; ";
            css += "right: -26px; ";
            css += "top: -1px;";
            css += "width: 26px;";
            css += "    }";


            css += "        .container-brackets-" + styles.tree_num + " .separator-brackets .line:last-child{ margin-bottom: 0; }";
            css += "        .container-brackets-" + styles.tree_num + " .separator-brackets.rd-1 .line:last-child{ margin-bottom: 0; }";
            css += "        .container-brackets-" + styles.tree_num + " .round.rd-1 .match .team{ margin-bottom: 0px; }";
            css += "/*-- First lines --*/";
            css += ".container-brackets-" + styles.tree_num + " .separator-brackets.rd-1 .line{";
            css += "    height: 42px;";
            css += "    margin-bottom: 62px;";
            css += "}";
            css += ".container-brackets-" + styles.tree_num + " .round .match .team.winner{";
            css += "    color:" + styles.winner_color + ";";
            css += "}";
            css += "    .container-brackets-" + styles.tree_num + " .separator-brackets.rd-1 .line:first-child{ margin-top: 11px; }";

            css += get_style_brackets(n_brackets, styles.tree_num);

            style.type = 'text/css';

            if (style.styleSheet) {
                style.styleSheet.cssText = css;
            } else {
                style.appendChild(document.createTextNode(css));
            }
            //$('#bracketsStyle').remove();
            head.appendChild(style);
        };


    $.fn.brackets = function (options) {
        var opts = $.extend({}, $.fn.brackets.defaults, options);

        if (!opts.rounds) {
            console.error("Round not found :(");
            return false;
        }

        if (opts.tree_num == undefined) {
            opts.tree_num = 0;
        }

        if (this.length >= 1) {
            this.each(function () {
                var $this = $(this);

                //-- add html brackets
                var container_brackets = get_html(opts, opts.titles);
                $this.html(container_brackets);
                set_style(opts, opts.rounds);

                //-- add event hover winner

                $('.team', $this).on({
                    mouseover: function () {
                        var $this = $(this);
                        ID = $this.data('id');
                        $(".team[data-id='" + ID + "']").addClass('hover');
                    },
                    mouseout: function () {
                        $(".team").removeClass('hover');
                    }
                });

            });
        } else {
            console.error('Object not found :( ');
        }

    };

    $.fn.brackets.defaults = {
        rounds: false,
        titles: false,
        color_title: 'black',
        border_color: 'black',
        color_team: 'black',
        bg_team: 'white',
        color_team_hover: 'black',
        bg_team_hover: 'white',
        border_radius_team: '0px',
        border_radius_lines: '0px',
        brackets_width: 150
    };




})(jQuery);