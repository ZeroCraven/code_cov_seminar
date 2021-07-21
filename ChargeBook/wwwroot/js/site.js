// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

moment.locale('de');

$(function () {
    //enable tooltips everywhere on the page
    $('[data-toggle="tooltip"]').tooltip();

    $('#calendarView').change(function () {
        $(this).tab('show');
        $(this).removeClass('active');
    });
    $('#listView').change(function () {
        $(this).tab('show');
        $(this).removeClass('active');
    });

    // every "collapse-chevron-btn" in the whole page should flip its chevron arrow when the collapse process is complete
    $('.collapse-chevron-btn').each(function () {
        let elem = $(this);
        let collapse = $(elem.data('target'));
        collapse.on('shown.bs.collapse', function () {
            elem.find('i').removeClass('fa-chevron-down');
            elem.find('i').addClass('fa-chevron-up');
        });
        collapse.on('hidden.bs.collapse', function () {
            elem.find('i').removeClass('fa-chevron-up');
            elem.find('i').addClass('fa-chevron-down');
        });
    });
});

//Convert Iso Week Date to Date
const getZeroBasedIsoWeekDay = date => (date.getDay() + 6) % 7
const getIsoWeekDay = date => getZeroBasedIsoWeekDay(date) + 1

function weekDateToDate(year, week, weekDay) {
    year = parseInt(year);
    week = parseInt(week);
    weekDay = parseInt(weekDay);
    const zeroBasedWeek = week - 1
    const zeroBasedWeekDay = weekDay
    let days = (zeroBasedWeek * 7) + zeroBasedWeekDay

    // Dates start at 2017-01-01 and not 2017-01-00
    days += 1

    const firstDayOfYear = new Date(year, 0, 1)
    const firstIsoWeekDay = getIsoWeekDay(firstDayOfYear)
    const zeroBasedFirstIsoWeekDay = getZeroBasedIsoWeekDay(firstDayOfYear)

    // If year begins with W52 or W53
    if (firstIsoWeekDay > 4) days += 8 - firstIsoWeekDay
    // Else begins with W01
    else days -= zeroBasedFirstIsoWeekDay

    return new Date(year, 0, days)
}

//Convert the String Object of a Date into yyyy-MM-ddThh:mm:ss
function convertToDateFormat(date) {
    if (!date) return;
    let month;
    date = date.toString();
    switch (date.split(' ')[1]) {
        case "Jan":
            month = "01";
            break;
        case "Feb":
            month = "02";
            break;
        case "Mar":
            month = "03";
            break;
        case "Apr":
            month = "04";
            break;
        case "May":
            month = "05";
            break;
        case "Jun":
            month = "06";
            break;
        case "Jul":
            month = "07";
            break;
        case "Aug":
            month = "08";
            break;
        case "Sep":
            month = "09";
            break;
        case "Oct":
            month = "10";
            break;
        case "Nov":
            month = "11";
            break;
        case "Dec":
            month = "12";
            break;
    }
    return date.split(' ')[3] + "-" + month + "-" + date.split(' ')[2] + "T" + date.split(' ')[4];
}

//Convert the yyyy-MM-ddThh:mm:ss into a Date object
function convertFromDateFormat(dateTemp) {
    let year = dateTemp.split('-')[0];
    let month = parseInt(dateTemp.split('-')[1]);
    //Shift months from 1-12 to 0-11
    month = month - 1;
    let day = dateTemp.split('-')[2].split('T')[0];
    let hour = dateTemp.split('T')[1].split(':')[0];
    let minutes = dateTemp.split('T')[1].split(':')[1]
    return new Date(year, month, day, hour, minutes);
}

function datesAreOnSameDay(first, second) {
    return first.getFullYear() === second.getFullYear() &&
        first.getMonth() === second.getMonth() &&
        first.getDate() === second.getDate();
}

function fetchBookings(all = false) {
    $.ajax({
        url: all ? '/booking/listAll' : '/booking/list',
        method: 'get'
    }).done(data => {
        let rawBookings = JSON.parse(data);
        for (let booking of rawBookings) {
            booking.timePeriods = booking.timePeriods.map(period => {
                return {
                    startTime: moment.utc(period.startTime).local(),
                    endTime: moment.utc(period.endTime).local()
                }
            });
        }
        this.bookings = rawBookings;
    });
}