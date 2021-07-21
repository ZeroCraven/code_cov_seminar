const calenderVue = new Vue({
    el: "#newBookingRequest",
    name: "createBooking",
    data: {
        timePeriods: [{
            startTime: null,
            endTime: null,
        }],
        startSoC: 0,
        targetSoC: 100,
        location: document.querySelector("#selectLocation").value,
        carName: document.querySelector("#selectCar").value,
        selectedWeek: "",
        leftMousePressedOnCalender: false,
        rightMousePressedOnCalender: false,
        lastCalenderMouseEvent: null,
        errorMessage: "",
        success: false,
        bookings: [],
        moment: moment
    },
    computed: {
        filledTimePeriods: function () {
            return this.timePeriods.filter(x => (x && x.startTime && x.endTime));
        },
        currentWeekMoment: function () {
            let year = this.selectedWeek.split("-W")[0];
            let week = this.selectedWeek.split("-W")[1];
            return moment(weekDateToDate(year, week, 0));
        },
    },
    methods: {
        setTargetSoC: function (value) {
            this.targetSoC = parseFloat(value);
            this.startSoC = Math.min(this.targetSoC, this.startSoC)
        },
        setStartSoC: function (value) {
            this.startSoC = parseFloat(value);
            this.targetSoC = Math.max(this.targetSoC, this.startSoC)
        },
        addNewDateRow: function () {
            this.timePeriods.push({
                startTime: null,
                endTime: null
            });
        },
        deleteDateRow: function (index) {
            if (this.timePeriods.length === 1) {
                this.timePeriods = [{
                    startTime: null,
                    endTime: null,
                }]
                return;
            }
            this.timePeriods.splice(index, 1);
        },
        weekDateToDate: weekDateToDate,
        isInPast(day, hour, min) {
            const timeSectionStart = weekDateToDate(this.selectedWeek.split("-")[0], this.selectedWeek.split("W")[1], day);
            timeSectionStart.setHours(hour);
            timeSectionStart.setMinutes(min);
            return timeSectionStart.getTime() < Date.now();
        },
        handleMouseActionOnCalender: function (day, hour, min, isLeftMouseButtonPressed, isRightMouseButtonPressed) {

            const clickedTimeSectionStart = weekDateToDate(this.selectedWeek.split("-")[0], this.selectedWeek.split("W")[1], day);
            clickedTimeSectionStart.setHours(hour);
            clickedTimeSectionStart.setMinutes(min);

            if (isLeftMouseButtonPressed) {
                let timeSectionStartFromLastCalenderMouseEvent = clickedTimeSectionStart;
                if (this.lastCalenderMouseEvent && this.lastCalenderMouseEvent.leftClick && this.lastCalenderMouseEvent.clickedTimeSectionStart.getDay() === clickedTimeSectionStart.getDay()) {
                    timeSectionStartFromLastCalenderMouseEvent = this.lastCalenderMouseEvent.clickedTimeSectionStart;
                }
                this.executeForEveryTimeSectionInBetween(timeSectionStartFromLastCalenderMouseEvent, clickedTimeSectionStart, this.selectTimeSection)
                this.lastCalenderMouseEvent = {
                    leftClick: true,
                    clickedTimeSectionStart: clickedTimeSectionStart,
                };
                return;
            }
            if (isRightMouseButtonPressed) {
                let timeSectionStartFromLastCalenderMouseEvent = clickedTimeSectionStart;
                if (this.lastCalenderMouseEvent && !this.lastCalenderMouseEvent.leftClick && this.lastCalenderMouseEvent.clickedTimeSectionStart.getDay() === clickedTimeSectionStart.getDay()) {
                    timeSectionStartFromLastCalenderMouseEvent = this.lastCalenderMouseEvent.clickedTimeSectionStart;
                }
                this.executeForEveryTimeSectionInBetween(timeSectionStartFromLastCalenderMouseEvent, clickedTimeSectionStart, this.deselectTimeSection)
                this.lastCalenderMouseEvent = {
                    leftClick: false,
                    clickedTimeSectionStart: clickedTimeSectionStart,
                };
            }
        },
        executeForEveryTimeSectionInBetween: function (timeSectionStart1, timeSectionStart2, functionToExecute) {
            const earlierTimeSection = new Date(Math.min(timeSectionStart1.getTime(), timeSectionStart2.getTime()));
            const laterTimeSection = new Date(Math.max(timeSectionStart1.getTime(), timeSectionStart2.getTime()));
            for (let timeSectionInBetween = earlierTimeSection; timeSectionInBetween.getTime() <= laterTimeSection.getTime();
                 timeSectionInBetween = new Date(timeSectionInBetween.getTime() + 15 * 60 * 1000)) {
                functionToExecute(timeSectionInBetween, new Date(timeSectionInBetween.getTime() + 15 * 60 * 1000));
            }
        },
        selectTimeSection(timeSectionStart, timeSectionEnd) {
            if (timeSectionStart < Date.now()) return;
            if (this.isTimeSectionOnDateSelected(timeSectionStart, timeSectionEnd)) return;
            let timePeriodToExtendStartIndex = -1;
            let timePeriodToExtendEndIndex = -1;
            //try to find a timePeriod, on which we can append the timeSection (above the timePeriod)
            for (let index = 0; index < this.timePeriods.length; index++) {
                const timePeriod = this.timePeriods[index];
                if (!timePeriod.startTime || !timePeriod.endTime) continue;
                if (timeSectionStart.getTime() < timePeriod.startTime.getTime() && timeSectionEnd.getTime() >= timePeriod.startTime.getTime()) {
                    timePeriodToExtendStartIndex = index
                    break;
                }

            }
            //try to find a timePeriod, on which we can append the timeSection (below the timePeriod)
            for (let index = 0; index < this.timePeriods.length; index++) {
                const timePeriod = this.timePeriods[index];
                if (!timePeriod.startTime || !timePeriod.endTime) continue;
                if (timeSectionStart.getTime() <= timePeriod.endTime.getTime() && timeSectionEnd.getTime() > timePeriod.endTime.getTime()) {
                    timePeriodToExtendEndIndex = index
                    break;
                }
            }
            //if 2 timePeriods are found, merge them
            if (timePeriodToExtendEndIndex !== -1 && timePeriodToExtendStartIndex !== -1) {
                this.timePeriods[timePeriodToExtendEndIndex].endTime = this.timePeriods[timePeriodToExtendStartIndex].endTime;
                this.timePeriods.splice(timePeriodToExtendStartIndex, 1);
                return;
            }
            if (timePeriodToExtendStartIndex !== -1) {
                this.timePeriods[timePeriodToExtendStartIndex].startTime = timeSectionStart;
                return;
            }
            if (timePeriodToExtendEndIndex !== -1) {
                this.timePeriods[timePeriodToExtendEndIndex].endTime = timeSectionEnd;
                return;
            }
            //if no timePeriod is found, which we can extend, try to fill a not filled timePeriod
            for (let index = 0; index < this.timePeriods.length; index++) {
                const timePeriod = this.timePeriods[index];
                if (!timePeriod.startTime && !timePeriod.endTime) {
                    timePeriod.startTime = timeSectionStart;
                    timePeriod.endTime = timeSectionEnd;
                    return;
                }
            }
            //if we could not extend or fill a not filled timePeriod, create a new one
            this.timePeriods.push({
                startTime: timeSectionStart,
                endTime: timeSectionEnd
            });
        },
        deselectTimeSection(timeSectionStart, timeSectionEnd) {
            //if in middle of a timePeriod, split it up
            for (let index = 0; index < this.timePeriods.length; index++) {
                const timePeriod = this.timePeriods[index];
                if (!timePeriod.startTime || !timePeriod.endTime) continue;
                if (timePeriod.startTime.getTime() < timeSectionStart.getTime() && timePeriod.endTime > timeSectionEnd.getTime()) {
                    this.timePeriods.push({
                        startTime: timeSectionEnd,
                        endTime: timePeriod.endTime
                    })
                    timePeriod.endTime = timeSectionStart;
                    return;
                }
            }
            for (let index = 0; index < this.timePeriods.length; index++) {
                const timePeriod = this.timePeriods[index];
                if (!timePeriod.startTime || !timePeriod.endTime) continue;
                if (timePeriod.startTime.getTime() >= timePeriod.endTime.getTime()) continue;
                if (timePeriod.startTime.getTime() >= timeSectionStart.getTime() && timePeriod.startTime.getTime() <= timeSectionEnd.getTime()) {
                    timePeriod.startTime = timeSectionEnd;
                    if (timePeriod.startTime.getTime() === timePeriod.endTime.getTime()) {
                        if (this.timePeriods.length > 1) {
                            this.timePeriods.splice(index, 1);
                        } else {
                            timePeriod.startTime = null;
                            timePeriod.endTime = null;
                        }
                    }
                    return;
                }
            }
            for (let index = 0; index < this.timePeriods.length; index++) {
                const timePeriod = this.timePeriods[index];
                if (!timePeriod.startTime || !timePeriod.endTime) continue;
                if (timePeriod.startTime.getTime() >= timePeriod.endTime.getTime()) continue;
                if (timePeriod.endTime.getTime() >= timeSectionStart.getTime() && timePeriod.endTime.getTime() <= timeSectionEnd.getTime()) {
                    timePeriod.endTime = timeSectionStart;
                    if (timePeriod.startTime.getTime() === timePeriod.endTime.getTime()) {
                        if (this.timePeriods.length > 1) {
                            this.timePeriods.splice(index, 1);
                        } else {
                            timePeriod.startTime = null;
                            timePeriod.endTime = null;
                        }
                    }
                    return;
                }
            }
        },
        isTimeSectionOnDateSelected(timeSectionStart, timeSectionEnd) {
            for (let index = 0; index < this.filledTimePeriods.length; index++) {
                const timePeriod = this.filledTimePeriods[index];
                if ((timeSectionStart.getTime() >= timePeriod.startTime.getTime()) && (timePeriod.endTime.getTime() >= timeSectionEnd.getTime())) {
                    return true
                }
            }
            return false;
        },
        isSelected: function (day, hour, min) {
            const timeSectionStart = weekDateToDate(this.selectedWeek.split("-")[0], this.selectedWeek.split("W")[1], day);
            timeSectionStart.setHours(hour);
            timeSectionStart.setMinutes(min);
            const timeSectionEnd = new Date(timeSectionStart.getTime() + 15 * 60 * 1000);
            return this.isTimeSectionOnDateSelected(timeSectionStart, timeSectionEnd);
        },
        setStartTimeFromString: function (timePeriod, startTimeString) {
            let startTime = new Date(startTimeString);
            timePeriod.startTime = startTime
        },
        setEndTimeFromString: function (timePeriod, endTimeString) {
            let endTime = new Date(endTimeString);
            timePeriod.endTime = endTime;
        },
        submitNewBooking() {
            this.errorMessage = "";
            let timePeriodStartInStringFormat = [];
            let formData = {
                targetSoC: this.targetSoC,
                startSoC: this.startSoC,
                location: this.location,
                carName: this.carName,
                __RequestVerificationToken: document.querySelector("#newBookingRequest [name=__RequestVerificationToken]").value,
            }

            this.timePeriods.forEach((x, index) => {
                formData["timePeriods[" + index + "].startTime"] = moment(x.startTime).utc().format("YYYY-MM-DDTHH:mm");
                formData["timePeriods[" + index + "].endTime"] = moment(x.endTime).utc().format("YYYY-MM-DDTHH:mm");
            })

            $.ajax({
                url: "/booking/create",
                data: formData,
                method: "post"
            }).done(x => {
                this.success = true;
                this.timePeriods = [{
                    startTime: null,
                    endTime: null,
                }];
                this.fetchBookings();
            }).fail(x => {
                if (!x) {
                    this.errorMessage = "Technischer Fehler bei der Anfrage"
                } else {
                    this.errorMessage = x.responseText;
                }
            });
        },
        fetchBookings: fetchBookings,
    },
    mounted() {
        // calculate calendar cell width AFTER simplebar removed the native scrollbar
        initCalendarSimpleBar();
        this.$refs.calendarBookings.recalculateCalendarCellWidth();
        
        this.fetchBookings();
        this.selectedWeek = moment().format('YYYY[-W]ww');
    },
});

function stepToNextWeek() {
    let weekInput = $('input[name=week]')[0];
    weekInput.stepUp();
    calenderVue.selectedWeek = weekInput.value;
    event.preventDefault()
}

function stepToPreviousWeek() {
    let weekInput = $('input[name=week]')[0];
    weekInput.stepDown();
    calenderVue.selectedWeek = weekInput.value;
    event.preventDefault()
}