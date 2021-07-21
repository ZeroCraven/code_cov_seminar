const homeViewVue = new Vue({
    name: 'homeView',
    el: '#homeView',
    data: {
        currentWeek: "",
        bookingStatusBadges: bookingStatusBadges,
        bookings: [],
        showPastBookings: false,
        activeTab: 'list' //'list' or 'calendar'
    },
    computed: {
        displayBookings() {
            return this.bookings.filter(b => this.showPastBookings || (b.status !== "abgelaufen" && b.status !== "Ladevorgang beendet"));
        },
        currentWeekMoment() {
            let year = this.currentWeek.split("-W")[0];
            let week = this.currentWeek.split("-W")[1];
            return moment(weekDateToDate(year, week, 0));
        },
        startConfirmableBookings() {
            let now = moment();
            return this.bookings.filter(booking => {
                if (booking.status === "angenommen") {
                    if (booking.timePeriods[0].startTime <= now) {
                        return true;
                    }
                }
                return false;
            });
        },
        endConfirmableBookings() {
            return this.bookings.filter(booking => booking.status === "Ladevorgang begonnen");
        }
    },
    methods: {
        handleTabSwitch() {
            this.$refs.calendarBookings.recalculateCalendarCellWidth();
        },
        stepUpWeek() {
            let weekElem = $('input[name=week]');
            weekElem[0].stepUp();
            this.currentWeek = weekElem.val();
        },
        stepDownWeek() {
            let weekElem = $('input[name=week]');
            weekElem[0].stepDown();
            this.currentWeek = weekElem.val();
        },
        confirmChargeBegin(bookingId) {
            $.ajax({
                url: '/booking/confirmChargeBegin',
                method: 'post',
                data: {
                    bookingId: bookingId,
                    __RequestVerificationToken: $('[name=__RequestVerificationToken]').val()
                }
            }).done(() => {
                this.fetchBookings();
            });
        },
        confirmChargeEnd(bookingId) {
            $.ajax({
                url: '/booking/confirmChargeEnd',
                method: 'post',
                data: {
                    bookingId: bookingId,
                    __RequestVerificationToken: $('[name=__RequestVerificationToken]').val()
                }
            }).done(() => {
                this.fetchBookings();
            });
        },
        fetchBookings: fetchBookings
    },
    mounted() {
        // calculate calendar cell width AFTER simplebar removed the native scrollbar
        initCalendarSimpleBar();
        this.$refs.calendarBookings.recalculateCalendarCellWidth();

        this.fetchBookings();
        this.currentWeek = moment().format('YYYY[-W]ww');

        // timer that calls fetchBookings every full 5 minutes
        let currentMinutes = moment().minutes();
        let next5MinutesMoment = moment().startOf('minute').minutes(Math.ceil(currentMinutes / 5) * 5);
        setTimeout(() => {
            this.fetchBookings();
            setInterval(() => this.fetchBookings(), 1000 * 60 * 5);
        }, next5MinutesMoment.diff(moment()) + 1000);
    }
});