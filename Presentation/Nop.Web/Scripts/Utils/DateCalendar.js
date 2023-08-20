var clickCount = 0;
$(function () {

    Date.prototype.dateFormat = function (format) {
        //if (format == "H:i")
        //    return moment(this).format("HH:mm");

        return moment(this).format(format);
    };
    buildCalendar();
});
//#region Build DateTime Picker
function parseTime(timeString) {
    if (timeString == '') return null;

    var time = timeString.match(/(\d+)(:(\d\d))?\s*(p?)/i);
    if (time == null) return null;

    var hours = parseInt(time[1],10);
    if (hours == 12 && !time[4]) {
        hours = 0;
    }
    else {
        hours += (hours < 12 && time[4])? 12 : 0;
    }
    var d = new Date();
    d.setHours(hours);
    d.setMinutes(parseInt(time[3],10) || 0);
    d.setSeconds(0, 0);
    return d;
}


function getNearestHalfHourTimeString(date) {
    var now = date;
    var hour = now.getHours();
    var minutes = now.getMinutes();

    var ampm = "AM";
    if (minutes < 15) {
        minutes = "15";
    }else if (minutes < 30){
        minutes = "30";
    }else if (minutes < 45){
        minutes = "45";
    } else {
        minutes = "00";
        ++hour;
    }
    if (hour > 23) {
        hour = 12;
    } else if (hour > 12) {
        hour = hour - 12;
        ampm = "PM";
    } else if (hour == 12) {
        ampm = "PM";
    } else if (hour == 0) {
        hour = 12;
    }

    return(hour + ":" + minutes + " " + ampm);
}

function getTimeToUse(allowedTimes,minDate) {
    var earliestAllowedTime = parseTime(allowedTimes[0]);
    earliestAllowedTime.setMonth(minDate.getMonth());
    earliestAllowedTime.setYear(minDate.getFullYear());
    earliestAllowedTime.setDate(minDate.getDate());

    var nearestHalfHourToNow = parseTime(getNearestHalfHourTimeString(minDate));
    nearestHalfHourToNow.setMonth(minDate.getMonth());
    nearestHalfHourToNow.setYear(minDate.getFullYear());
    nearestHalfHourToNow.setDate(minDate.getDate());

    var timeToUse = "";

    if(nearestHalfHourToNow > earliestAllowedTime)
    {
        timeToUse = getNearestHalfHourTimeString(minDate);
    }
    else
    {
        timeToUse = allowedTimes[0];

    }
    return timeToUse;

}


function GetNextAvailableDate(blockedDates, date) {

    for (i =0; i < blockedDates.length; i++) {
        var dateParts = blockedDates[i].split("-");
        var dateClosed = new Date(dateParts[0], (dateParts[1] - 1), dateParts[2]);
        var minDateZeroTime = new Date(date.getFullYear(), (date.getMonth()), date.getDate());
        //alert(date);

        //alert(minDateZeroTime + " " + dateClosed);
        if (minDateZeroTime.getTime() == dateClosed.getTime()) {
            date.setDate(date.getDate() + 1);
            GetNextAvailableDate(blockedDates, date)
        }
    }

    return date;

}

function buildCalendar() {//minDate, maxDate,dates
    //picker, minDate, maxDate, showTimePicker, dates, allowedTimes, leadTimeDays, leadTimeHours, leadTimeMinutes, warehouseId, isDelivery, showPicker, isUnbind
    var picker = 'reservationDate';
    
    var isDelivery = false;
    var showPicker = true;
    var showTimePicker = false;
    var isUnbind= false;
    var clickCount = 0;
    var dates = $(document.getElementById('Reservation_JsExcludedDatesStr')).val();
    var date = new Date();
    var minDate = new Date(date.getFullYear(), (date.getMonth()), date.getDate());
    var maxDate = new Date(date.getFullYear(), date.getMonth(), date.getDate() + 2);
    var leadTimeDays = '';
    var leadTimeHours = '';
    var leadTimeMinutes = '';  
    var product_id = $(document.getElementById('productDetails')).attr('data-productid');

    //var id = picker.split('__');
    var dateIcon = 'dateIcons';
    var timePickerId = 'timePicker';
    var pickupTimeText = 'pickupTimeText';


    var isPickup = false;

    if(isDelivery == true)
    {
        isPickup = false;
    }
    else
    {
        isPickup = true;
    }

    $("#pickupText").hide();
    $("#" + pickupTimeText).hide();
    $("#" + timePickerId).hide();

    if (!showPicker) {
        $("#" + dateIcon).hide();
        $("#reservationDate").hide();
        return;
    }
    else {
        $("#" + dateIcon).show();
        $("#reservationDate").show();
    }

    $("#mindate").val("");
    $("#showTimePicker").val("");
    $("#mindate").val(minDate);
    $("#showTimePicker").val(showTimePicker);

    var stringDate = (minDate.getFullYear()) + '-' + (minDate.getMonth() + 1) + '-' + minDate.getDate();//+ " " + timeToUse;
    
    var date = minDate.dateFormat('MM-DD-YYYY');
    $("#" + picker).val(date);

    if (isUnbind) {
        $("#" + picker).unbind();
        $("#" + dateIcon).unbind();
    }           


    $A.bind(window, 'load', function () {
        // Syntax : setCalendar( ID , TriggeringElement , TargetEditField , EnableComments , clickHandler , config )
        $A.setCalendar('reservationCal', document.getElementById(dateIcon), document.getElementById(picker), false, function (ev, dc, targ) {
            targ.value = dc.range.current.year + '-' + ('0' + (dc.range.current.month + 1)).slice(-2) + '-' + ('0' + dc.range.current.mDay).slice(-2);
            dc.close();
           onDateChange();
        },
            {

                ajax: function (dc, save) {

                    // Run before the datepicker renders

                    if (!dc.firstResetDate) {
                        // Store a variable in the dc object to ensure this only runs when the date picker first opens, and not every time such as when switching between months or years
                        dc.firstResetDate = true;

                        // Set current date variables
                        //var cur = new Date();

                        // Now configure a 'current' object that uses the date syntax within the datepicker JS instance
                        // This will be used to merge into the datepicker before it opens
                        var current =
                            {
                                day: minDate.getDate(),
                                month: minDate.getMonth(),
                                year: minDate.getFullYear(),
                                weekDay: minDate.getDay()
                            };

                        var lastdate =
                            {
                                day: minDate.getDate()+2,
                                month: minDate.getMonth(),
                                year: minDate.getFullYear(),
                                weekDay: minDate.getDay()
                            };

                        // Now adjust the default date that the date picker first opens with using the previously set date object
                        // Uses the 'current' object variables to set the dates within the calendar before it opens
                        dc.range.current.month = current.month;
                        dc.range.current.mDay = current.day;
                        dc.range.current.wDay = current.weekDay;
                        dc.range.current.year = current.year;

                        // Now set a custom variable to store the disabled date range starting point
                        dc.startDate =
                            {
                                day: minDate.getDate(),
                                month: minDate.getMonth(),
                                year: minDate.getFullYear(),
                                weekDay: minDate.getDay()
                            };
                    }

                    // Now dynamically adjust the disabled date range always starting with dc.startDate
                    var current = dc.startDate;

                    // Disable all dates prior to the current day
                    if (current.year > dc.range.current.year
                        || (current.year === dc.range.current.year && current.month > dc.range.current.month)) {
                        dc.range[dc.range.current.month].disabled[dc.range.current.year] =
                            [
                                1,
                                2,
                                3,
                                4,
                                5,
                                6,
                                7,
                                8,
                                9,
                                10,
                                11,
                                12,
                                13,
                                14,
                                15,
                                16,
                                17,
                                18,
                                19,
                                20,
                                21,
                                22,
                                23,
                                24,
                                25,
                                26,
                                27,
                                28,
                                29,
                                30,
                                31
                            ];
                    }

                    if (current.year === dc.range.current.year && current.month === dc.range.current.month) {
                        dc.range[dc.range.current.month].disabled[dc.range.current.year] = [];

                        for (var day = 1; day < current.day; day++) {
                            dc.range[dc.range.current.month].disabled[dc.range.current.year].push(day);
                        }
                    }

                    //disable excluded dates
                    var unavlblDates = dates.split(',');
                    if (unavlblDates.length > 0) {
                        for (i = 0; i < unavlblDates.length; i++) {
                            var dateval = unavlblDates[i];
                            var dateParts = dateval.split("-");
                            var year = dateParts[0].substring(1, dateParts[0].length);
                            var day = dateParts[2].slice(0, -1);
                            var excludeDate = new Date(year, (dateParts[1] - 1), day);
                            if (excludeDate.getFullYear() == dc.range.current.year && excludeDate.getMonth() == dc.range.current.month) {
                                if (!dc.range[dc.range.current.month].disabled[dc.range.current.year])
                                    dc.range[dc.range.current.month].disabled[dc.range.current.year] = [];
                                dc.range[dc.range.current.month].disabled[dc.range.current.year].push(excludeDate.getDate());
                            }
                        }
                    }

                    //set max year
                    var yearEnd = maxDate.getFullYear();
                    if (dc.range.current.year > yearEnd) {
                        dc.range[dc.range.current.month].disabled[dc.range.current.year] =
                            [
                                1,
                                2,
                                3,
                                4,
                                5,
                                6,
                                7,
                                8,
                                9,
                                10,
                                11,
                                12,
                                13,
                                14,
                                15,
                                16,
                                17,
                                18,
                                19,
                                20,
                                21,
                                22,
                                23,
                                24,
                                25,
                                26,
                                27,
                                28,
                                29,
                                30,
                                31
                            ];
                    }
                    dc.open();
                },

                //openOnFocus: true,
                openOnFocusHelpText: 'Press Down arrow to browse the calendar, or Escape to close.',
                inputDateFormat: 'MM-DD-YYYY',
                initialDate: minDate,
                // Always restore today's date as being selected when calendar is activated.
                //resetCurrent: false,
                highlightToday: true,
                //showEscBtn: true,
                //escBtnName: 'Close',
                //escBtnIcon: 'close',

                // Set CSS positioning calculation for the calendar
                autoPosition: 6,
                // Customize with positive or negative offsets
                offsetTop: 0,
                offsetLeft: 5,
                overrides:
                    {
                        allowCascade: true,
                        runAfter: function (dc) {
                            $A.remAttr($A.getEl('keyboardHints'), 'hidden');
                        },
                        runAfterClose: function (dc) {
                            $A.setAttr($A.getEl('keyboardHints'), 'hidden', true);

                        }
                    }
            });
    });



    
    }

function formatAMPM(date) {
    var hours = date.getHours();
    var minutes = date.getMinutes();
    var ampm = hours >= 12 ? 'PM' : 'AM';
    hours = hours % 12;
    hours = hours ? hours : 12; 
    minutes = minutes < 10 ? '0' + minutes : minutes;
    var strTime = hours + ':' + minutes + ' ' + ampm;
    return strTime;
    return strTime;
}

function onDateChange() {  
    $("#dateIcons").attr('aria-pressed', true);
    var minDate = new Date($("#mindate").val());
    var showTimePicker = $("#showTimePicker").val();
    var inputId = "reservationDate";
    var timePickerId = "timePicker";
    clickCount++;
    var current_date = $("#" + inputId).val();
    $('#TimeslotsConfigured').trigger('change');
    //displayAjaxLoading(true);

    //$.ajax({
    //    cache: false,
    //    type: "POST",
    //    url: '@Html.Raw(Url.Action("Calendar_DayChange", "Product"))',
    //    data: { "date": current_time, "warehouseId": warehouseId, "pickup": true, "delivery": false },
    //    success: function (data) {
    //        if (data.availableTimes) {
                
    //        }
    //        else {
               
    //        }
    //        if (clickCount) {
    //            clickCount--;
    //        }
    //        if (clickCount == 0) {
    //            displayAjaxLoading();
    //        }
    //    },
    //    error: function (xhr, ajaxOptions, thrownError) {
    //        if (clickCount) {
    //            clickCount--;
    //        }
    //        if (clickCount == 0) {
    //            displayAjaxLoading();
    //        }
    //    }
    //});
}