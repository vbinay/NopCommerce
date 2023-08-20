/*
Monthly 2.2.0 by Kevin Thornbloom is licensed under a Creative Commons Attribution-ShareAlike 4.0 International License.
*/

(function ($) {
    "use strict";
    $.fn.extend({
        monthly: function (customOptions) {

            // These are overridden by options declared in footer
            var defaults = {
                disablePast: false,
                eventList: true,
                events: "",
                getDataURL: "",
                linkCalendarToEventUrl: false,
                maxWidth: false,
                mode: "event",
                setWidth: false,
                showTrigger: "",
                startHidden: false,
                stylePast: false,
                target: "",
                useIsoDateFormat: false,
                weekStart: 0,	// Sunday
                xmlUrl: ""
            };

            var options = $.extend(defaults, customOptions),
				uniqueId = $(this).attr("id"),
				parent = "#" + uniqueId,
				currentDate = new Date(),
				currentMonth = currentDate.getMonth() + 1,
				currentYear = currentDate.getFullYear(),
				currentDay = currentDate.getDate(),
				locale = (options.locale || defaultLocale()).toLowerCase(),
				monthNameFormat = options.monthNameFormat || "short",
				weekdayNameFormat = options.weekdayNameFormat || "short",
				monthNames = options.monthNames || defaultMonthNames(),
				dayNames = options.dayNames || defaultDayNames(),
				markupBlankDay = '<div class="m-d monthly-day-blank"><div class="monthly-day-number"></div></div>',
				weekStartsOnMonday = options.weekStart === "Mon" || options.weekStart === 1 || options.weekStart === "1",
				primaryLanguageCode = locale.substring(0, 2).toLowerCase();

            if (options.maxWidth !== false) {
                $(parent).css("maxWidth", options.maxWidth);
            }
            if (options.setWidth !== false) {
                $(parent).css("width", options.setWidth);
            }

            if (options.startHidden) {
                $(parent).addClass("monthly-pop").css({
                    display: "none",
                    position: "absolute"
                });
                $(document).on("focus", String(options.showTrigger), function (event) {
                    $(parent).show();
                    event.preventDefault();
                });
                $(document).on("click", String(options.showTrigger) + ", .monthly-pop", function (event) {
                    event.stopPropagation();
                    event.preventDefault();
                });
                $(document).on("click", function () {
                    $(parent).hide();
                });
            }

            // Add Day Of Week Titles
            _appendDayNames(weekStartsOnMonday);

            // Add CSS classes for the primary language and the locale. This allows for CSS-driven
            // overrides of the language-specific header buttons. Lowercased because locale codes
            // are case-insensitive but CSS is not.
            $(parent).addClass("monthly-locale-" + primaryLanguageCode + " monthly-locale-" + locale);

            // Add Header & event list markup
            $(parent).prepend('<div class="monthly-header"><div class="monthly-header-title"><a href="#" class="monthly-header-title-date" onclick="return false"></a></div><a href="#" class="monthly-prev"></a><a href="#" class="monthly-next"></a></div>').append('<div class="monthly-event-list"></div>');

            // Set the calendar the first time
            setMonthly(currentMonth, currentYear);

            // How many days are in this month?
            function daysInMonth(month, year) {
                return month === 2 ? (year & 3) || (!(year % 25) && year & 15) ? 28 : 29 : 30 + (month + (month >> 3) & 1);
            }

            // Build the month
            function setMonthly(month, year) {
                $(parent).data("setMonth", month).data("setYear", year);

                // Get number of days
                var index = 0,
                    dayQty = daysInMonth(month, year),
                    // Get day of the week the first day is
                    mZeroed = month - 1,
                    firstDay = new Date(year, mZeroed, 1, 0, 0, 0, 0).getDay(),
                    settingCurrentMonth = month === currentMonth && year === currentYear;

                // Remove old days
                $(parent + " .monthly-day, " + parent + " .monthly-day-blank").remove();
                $(parent + " .monthly-event-list, " + parent + " .monthly-day-wrap").empty();
                // Print out the days
                for (var dayNumber = 1; dayNumber <= dayQty; dayNumber++) {
                    // Check if it's a day in the past
                    var isInPast = options.stylePast && (
                        year < currentYear
                        || (year === currentYear && (
                            month < currentMonth
                            || (month === currentMonth && dayNumber < currentDay)
                        ))),
                        innerMarkup = '<div class="monthly-day-number">' + dayNumber + '</div><div class="monthly-indicator-wrap"></div>';
                    if (options.mode === "event") {
                        var thisDate = new Date(year, mZeroed, dayNumber, 0, 0, 0, 0);
                        $(parent + " .monthly-day-wrap").append("<div"
                            + attr("class", "m-d monthly-day monthly-day-event"
                                + (isInPast ? " monthly-past-day" : "")
                                + " dt" + thisDate.toISOString().slice(0, 10)
                                )
                            + attr("data-number", dayNumber)
                            + attr("date-value", thisDate.toISOString().slice(0, 10))
                            + ">" + innerMarkup + "</div>");
                        $(parent + " .monthly-event-list").append("<div"
                            + attr("class", "monthly-list-item")
                            + attr("id", uniqueId + "day" + dayNumber)
                            + attr("data-number", dayNumber)
                            + '><div class="monthly-event-list-date">' + dayNames[thisDate.getDay()] + "<br>" + dayNumber + "</div></div>");
                    } else {
                        $(parent + " .monthly-day-wrap").append("<a"
                            + attr("href", "#")
                            + attr("class", "m-d monthly-day monthly-day-pick" + (isInPast ? " monthly-past-day" : ""))
                            + attr("data-number", dayNumber)
                            + ">" + innerMarkup + "</a>");
                    }
                }

                if (settingCurrentMonth) {
                    $(parent + ' *[data-number="' + currentDay + '"]').addClass("monthly-today");
                }

                // Reset button
                $(parent + " .monthly-header-title").html(
                    '<a href="#" class="monthly-header-title-date" onclick="return false">' + monthNames[month - 1] + " " + year + "</a>"
                    + (settingCurrentMonth ? "" : '<a href="#" class="monthly-reset"></a>'));

                // Account for empty days at start
                if (weekStartsOnMonday) {
                    if (firstDay === 0) {
                        _prependBlankDays(6);
                    } else if (firstDay !== 1) {
                        _prependBlankDays(firstDay - 1);
                    }
                } else if (firstDay !== 7) {
                    _prependBlankDays(firstDay);
                }

                // Account for empty days at end
                var numdays = $(parent + " .monthly-day").length,
                    numempty = $(parent + " .monthly-day-blank").length,
                    totaldays = numdays + numempty,
                    roundup = Math.ceil(totaldays / 7) * 7,
                    daysdiff = roundup - totaldays;
                if (totaldays % 7 !== 0) {
                    for (index = 0; index < daysdiff; index++) {
                        $(parent + " .monthly-day-wrap").append(markupBlankDay);
                    }
                }

                // Events
                if (options.mode === "event") {
                    addEvents(month, year);
                }
                var divs = $(parent + " .m-d");
                for (index = 0; index < divs.length; index += 7) {
                    divs.slice(index, index + 7).wrapAll('<div class="monthly-week"></div>');
                }
            }

            function addEvent(event, setMonth, setYear) {
                // Year [0]   Month [1]   Day [2]
                var fullStartDate = _getEventDetail(event, "startdate"),
                    fullEndDate = _getEventDetail(event, "enddate"),
                    startArr = fullStartDate.split("-"),
                    startYear = parseInt(startArr[0], 10),
                    startMonth = parseInt(startArr[1], 10),
                    startDay = parseInt(startArr[2], 10),
                    startDayNumber = startDay,
                    endDayNumber = startDay,
                    showEventTitleOnDay = startDay,
                    startsThisMonth = startMonth === setMonth && startYear === setYear,
                    happensThisMonth = startsThisMonth;

                if (!happensThisMonth) {
                    return;
                }

                for (var index = startDayNumber; index <= endDayNumber; index++) {
                    // Add to calendar view
                    $(parent + ' *[data-number="' + index + '"] .monthly-indicator-wrap').parent().addClass("monthly-day-selected");
                }
            }

            function getThisMonth(month, year) {
                //var thisMonth = moment().format("YYYY-MM-DD");
                var thisDate = year + "-" + month + "-1";

                var thisMonth = moment(thisDate, "YYYY-M-D").format("YYYY-MM-DD");


                var data = {
                    date: JSON.stringify(thisMonth)
                };
                addAntiForgeryToken(data);

                return (data);
            }

            function addEvents(month, year) {
                if (options.events) {
                    // Prefer local events if provided
                    addEventsFromString(options.events, month, year);
                } else {
                    var remoteUrl = options.getDataURL;
                    if (remoteUrl) {
                        // Replace variables for month and year to load from dynamic sources
                        var url = String(remoteUrl).replace("{month}", month).replace("{year}", year);
                        $.post(url, getThisMonth(month, year), function (data) {
                            addEventsFromString(data, month, year);
                        }, "json").fail(function () {
                            console.error("Monthly.js failed to import " + remoteUrl + ". Please check for the correct path and json syntax.");
                        });
                    }
                }
            }

            function addEventsFromString(events, setMonth, setYear) {
                console.log(events, setMonth, setYear);
                $.each(events, function (index, event) {
                    addEvent(event, setMonth, setYear);
                });
            }

            function attr(name, value) {
                var parseValue = String(value);
                var newValue = "";
                for (var index = 0; index < parseValue.length; index++) {
                    switch (parseValue[index]) {
                        case "'": newValue += "&#39;"; break;
                        case "\"": newValue += "&quot;"; break;
                        case "<": newValue += "&lt;"; break;
                        case ">": newValue += "&gt;"; break;
                        default: newValue += parseValue[index];
                    }
                }
                return " " + name + "=\"" + newValue + "\"";
            }

            function _appendDayNames(startOnMonday) {
                var offset = startOnMonday ? 1 : 0,
                    dayName = "",
                    dayIndex = 0;
                for (dayIndex = 0; dayIndex < 6; dayIndex++) {
                    dayName += "<div>" + dayNames[dayIndex + offset] + "</div>";
                }
                dayName += "<div>" + dayNames[startOnMonday ? 0 : 6] + "</div>";
                $(parent).append('<div class="monthly-day-title-wrap">' + dayName + '</div><div class="monthly-day-wrap"></div>');
            }

            // Detect the user's preferred language
            function defaultLocale() {
                if (navigator.languages && navigator.languages.length) {
                    return navigator.languages[0];
                }
                return navigator.language || navigator.browserLanguage;
            }

            // Use the user's locale if possible to obtain a list of short month names, falling back on English
            function defaultMonthNames() {
                if (typeof Intl === "undefined") {
                    return ["Jan", "Feb", "Mar", "Apr", "May", "June", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
                }
                var formatter = new Intl.DateTimeFormat(locale, { month: monthNameFormat });
                var names = [];
                for (var monthIndex = 0; monthIndex < 12; monthIndex++) {
                    var sampleDate = new Date(2017, monthIndex, 1, 0, 0, 0);
                    names[monthIndex] = formatter.format(sampleDate);
                }
                return names;
            }

            function formatDate(year, month, day) {
                if (options.useIsoDateFormat) {
                    return new Date(year, month - 1, day, 0, 0, 0).toISOString().substring(0, 10);
                }
                if (typeof Intl === "undefined") {
                    return month + "/" + day + "/" + year;
                }
                return new Intl.DateTimeFormat(locale).format(new Date(year, month - 1, day, 0, 0, 0));
            }

            // Use the user's locale if possible to obtain a list of short weekday names, falling back on English
            function defaultDayNames() {
                if (typeof Intl === "undefined") {
                    return ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
                }
                var formatter = new Intl.DateTimeFormat(locale, { weekday: weekdayNameFormat }),
                    names = [],
                    dayIndex = 0,
                    sampleDate = null;
                for (dayIndex = 0; dayIndex < 7; dayIndex++) {
                    // 2017 starts on a Sunday, so use it to capture the locale's weekday names
                    sampleDate = new Date(2017, 0, dayIndex + 1, 0, 0, 0);
                    names[dayIndex] = formatter.format(sampleDate);
                }
                return names;
            }

            function _prependBlankDays(count) {
                var wrapperEl = $(parent + " .monthly-day-wrap"),
                    index = 0;
                for (index = 0; index < count; index++) {
                    wrapperEl.prepend(markupBlankDay);
                }
            }

            function _getEventDetail(event, nodeName) {
                return event[nodeName];
            }

            // Returns a 12-hour format hour/minute with period. Opportunity for future localization.
            function formatTime(value) {
                var timeSplit = value.split(":");
                var hour = parseInt(timeSplit[0], 10);
                var period = "AM";
                if (hour > 12) {
                    hour -= 12;
                    period = "PM";
                } else if (hour === 0) {
                    hour = 12;
                }
                return hour + ":" + String(timeSplit[1]) + " " + period;
            }

            function setNextMonth() {
                var setMonth = $(parent).data("setMonth"),
                    setYear = $(parent).data("setYear"),
                    newMonth = setMonth === 12 ? 1 : setMonth + 1,
                    newYear = setMonth === 12 ? setYear + 1 : setYear;
                setMonthly(newMonth, newYear);
                viewToggleButton();
            }

            function setPreviousMonth() {
                var setMonth = $(parent).data("setMonth"),
                    setYear = $(parent).data("setYear"),
                    newMonth = setMonth === 1 ? 12 : setMonth - 1,
                    newYear = setMonth === 1 ? setYear - 1 : setYear;
                setMonthly(newMonth, newYear);
                viewToggleButton();
            }

            // Function to go back to the month view
            function viewToggleButton() {
                if ($(parent + " .monthly-event-list").is(":visible")) {
                    $(parent + " .monthly-cal").remove();
                    $(parent + " .monthly-header-title").prepend('<a href="#" class="monthly-cal"></a>');
                }
            }

            // Advance months
            $(document.body).on("click", parent + " .monthly-next", function (event) {
                setNextMonth();
                event.preventDefault();
            });

            // Go back in months
            $(document.body).on("click", parent + " .monthly-prev", function (event) {
                setPreviousMonth();
                event.preventDefault();
            });

            // Reset Month
            $(document.body).on("click", parent + " .monthly-reset", function (event) {
                $(this).remove();
                setMonthly(currentMonth, currentYear);
                viewToggleButton();
                event.preventDefault();
                event.stopPropagation();
            });

            // Back to month view
            $(document.body).on("click", parent + " .monthly-cal", function (event) {
                $(this).remove();
                $(parent + " .monthly-event-list").css("transform", "scale(0)");
                setTimeout(function () {
                    $(parent + " .monthly-event-list").hide();
                }, 250);
                event.preventDefault();
            });

            // Click A Day
            $(document.body).on("click touchstart", parent + " .monthly-day", function (event) {
                // If events, show events list
                var whichDay = $(this).data("number");
                var whichDate = $(this).attr("date-value");
                if (whichDay.toString().length < 2)
                    whichDate = whichDate.slice(0, -2) + 0 + whichDay;
                else
                    whichDate = whichDate.slice(0, -2) + whichDay;
                if (options.mode === "event" && options.eventList) {
                    var theList = $(parent + " .monthly-event-list"),
                        myElement = document.getElementById(uniqueId + "day" + whichDay),
                        topPos = myElement.offsetTop;

                    if ($(this).hasClass("monthly-day-selected")) {
                        $(this).removeClass("monthly-day-selected");

                    } else {
                        $(this).addClass("monthly-day-selected");
                    }

                    var updateDate = {
                        date: JSON.stringify(whichDate)
                    };
                    addAntiForgeryToken(updateDate);

                    $.ajax({
                        url: options.saveEventURL,
                        dataType: 'json',
                        data: updateDate,
                        type: 'POST',
                        error: function () {
                            console.log('there was an error while fetching events!');
                        },
                        success: function (data) {
                        }
                    });

                    /*
                    theList.show();
                    theList.css("transform");
                    theList.css("transform", "scale(1)");
                    $(parent + ' .monthly-list-item[data-number="' + whichDay + '"]').show();
                    theList.scrollTop(topPos);
                    viewToggleButton();
                    if(!options.linkCalendarToEventUrl) {
                        event.preventDefault();
                    }
                    */
                    // If picker, pick date
                } else if (options.mode === "picker") {
                    var setMonth = $(parent).data("setMonth"),
                        setYear = $(parent).data("setYear");
                    // Should days in the past be disabled?
                    if ($(this).hasClass("monthly-past-day") && options.disablePast) {
                        // If so, don't do anything.
                        event.preventDefault();
                    } else {
                        // Otherwise, select the date ...
                        $(String(options.target)).val(formatDate(setYear, setMonth, whichDay));
                        // ... and then hide the calendar if it started that way
                        if (options.startHidden) {
                            $(parent).hide();
                        }
                    }
                    event.preventDefault();
                }
            });

            // Clicking an event within the list
            $(document.body).on("click", parent + " .listed-event", function (event) {
                var href = $(this).attr("href");
                // If there isn't a link, don't go anywhere
                if (!href) {
                    event.preventDefault();
                }
            });

        }
    });
}(jQuery));
