function scrollToCurrentTimeLine() {
    let calendarContent = $('#calendar-section .calender-content');
    let timeLine = $('#calendar-section .calendar-current-time');
    let calendarHeight = calendarContent.height();
    let scrollElement = SimpleBar.instances.get(calendarContent[0]).getScrollElement();
    scrollElement.scrollTop = parseInt(timeLine.css('top')) - calendarHeight / 2;
}

$(function () {
    // update the booking widths when the table column width updates
    $(window).resize(function () {
        //automatically switch to list view if breakpoint is too small
        if ($('#settings-section > .row > .col').is(':not(:visible)')) {
            if ($('#calendar-tab').is('.active')) {
                $('#listView').click();
            }
        }
    })
    scrollToCurrentTimeLine();
    $('#calendarView').on('shown.bs.tab', function (e) {
        scrollToCurrentTimeLine();
        homeViewVue.handleTabSwitch();
    });
    
});

function initCalendarSimpleBar() {
    // scroll the days and time row and column when the main content table scrolls
    let simpleBar = new SimpleBar($('#calendar-section .calender-content')[0]);
    simpleBar.getScrollElement().addEventListener('scroll', function () {
        $('#calendar-section .calender-days .table').css('margin-left', -$(this).scrollLeft());
        $('#calendar-section .calender-time .table').css('margin-top', -$(this).scrollTop() - 25);
    });
}