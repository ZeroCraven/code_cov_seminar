const adminBookingListVue = new Vue({
    name: 'adminBookingList',
    el: '#admin-booking-list',
    data: {
        bookingStatusBadges: bookingStatusBadges,
        bookings: [],
        filter: {}
    },
    computed: {
        displayBookings() {
            return this.bookings.filter(booking => {
                // filter user
                if (this.filter.user && !booking.user.toLowerCase().includes(this.filter.user.toLowerCase())) {
                    return false;
                }
                // filter status
                if (this.filter.status && booking.status !== this.filter.status) {
                    return false;
                }
                // filter location
                if (this.filter.location && booking.location !== this.filter.location) {
                    return false;
                }
                // filter chargePoint
                if (this.filter.chargePoint && !booking.fullChargePointString.toLowerCase().includes(this.filter.chargePoint.toLowerCase())) {
                    return false;
                }
                // filter timePeriods
                let startTimeMoment = moment(this.filter.startTime);
                let endTimeMoment = moment(this.filter.endTime);
                if (endTimeMoment.isValid()) endTimeMoment.add(1, 'days'); // let the endTime filter be inclusive
                for (let timePeriod of booking.timePeriods) {
                    if ((!startTimeMoment.isValid() || timePeriod.startTime >= startTimeMoment)
                        && (!endTimeMoment.isValid() || timePeriod.endTime <= endTimeMoment)) {
                        return true;
                    }
                }
                return false;
            });
        }
    },
    methods: {
        clearFilter() {
            this.filter = {
                user: "",
                status: "",
                startTime: "",
                endTime: "",
                chargePoint: "",
                location: ""
            };
        },
        fetchBookings: fetchBookings
    },
    mounted() {
        this.clearFilter();
        this.fetchBookings(true);
    }
});