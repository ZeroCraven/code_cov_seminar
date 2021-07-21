Vue.component('calendarbookings', {
    name: "calendarBookings",
    props: ['currentweek', 'bookings'],
    data: function () {
        return {
            currentMoment: moment(),
            bookingStatusBadges: bookingStatusBadges,
            calendarCellWidth: 0,
            calendarHourHeight: 50
        };
    },
    computed: {
        currentWeekMoment() {
            let year = this.currentweek.split("-W")[0];
            let week = this.currentweek.split("-W")[1];
            return moment(weekDateToDate(year, week, 0));
        }
    },
    methods: {
        recalculateCalendarCellWidth() {
            this.calendarCellWidth = $('#calendar-section .calender-content td').prop('scrollWidth');
        },
        calculateTimePeriodHeight(timePeriod) {
            return (timePeriod.endTime - timePeriod.startTime) / 1000.0 / 60.0 / 60.0 * this.calendarHourHeight;
        },
        isTimePeriodVisible(timePeriod) {
            let weekStart = this.currentWeekMoment;
            let weekEnd = weekStart.clone().add(7, 'days');
            return timePeriod.startTime >= weekStart && timePeriod.endTime <= weekEnd;
        },
        isTimeLineVisible() {
            let weekStart = this.currentWeekMoment;
            let weekEnd = weekStart.clone().add(7, 'days');
            let now = moment();
            return now >= weekStart && now <= weekEnd;
        }
    },
    mounted() {
        $(window).resize(() => {
            this.recalculateCalendarCellWidth();
        });
        setInterval(() => {
            this.currentMoment = moment();
        }, 1000 * 60 * 5);
    },
    template: `
        <div>
            <div v-for="booking in bookings" v-on:click="$emit('show-booking-details', booking)">
                <div v-for="timePeriod in booking.timePeriods" v-if="isTimePeriodVisible(timePeriod)" class="booking-overlay" v-bind:data-bookinginfo-id="booking.id"
                    v-bind:style="{
                        width: calendarCellWidth + 'px',
                        height: calculateTimePeriodHeight(timePeriod) + 'px',
                        left: (timePeriod.startTime.weekday() * calendarCellWidth) + 'px',
                        top: ((timePeriod.startTime.hours() + timePeriod.startTime.minutes() / 60.0) * calendarHourHeight) + 'px'
                    }"
                    >
                    <!-- show more info when booking is bigger or equal 1.25 hours -->
                    <div v-if="calculateTimePeriodHeight(timePeriod) >= 62.5" v-bind:style="{ 'background-color': bookingStatusBadges[booking.status].lightColor, 'border-color': bookingStatusBadges[booking.status].color }" style="border-width: 3px;">
                        <span>{{timePeriod.startTime.format('HH:mm')}} - {{timePeriod.endTime.format('HH:mm')}}</span>
                        <div style="height: 1px;" v-bind:style="{ 'background-color': bookingStatusBadges[booking.status].color }"></div>
                        <ul class="list-unstyled text-left mx-2 my-1 overflow-hidden" style="font-size: 0.8em;">
                            <li><i class="fas fa-car"></i> {{booking.car}}</li>
                            <li v-if="booking.fullChargePointString.length"><i class="fas fa-charging-station"></i> {{booking.fullChargePointString}}</li>
                            <li v-if="booking.connectorType.length"><i class="fas fa-plug"></i> {{booking.connectorType}}</li>
                            <li><i class="fas fa-map-marker-alt"></i> {{booking.location}}</li>
                        </ul>
                        <div class="flex-grow-1"></div>
                        <div style="height: 1px;" v-bind:style="{ 'background-color': bookingStatusBadges[booking.status].color }"></div>
                        <div class="row justify-content-between">
                            <span class="col-auto mx-1">#{{booking.id}}</span>
                            <i v-bind:class="'col-auto mx-1 ' + bookingStatusBadges[booking.status].icon"></i>
                        </div>
                    </div>
                    <!-- only show basic booking info when booking is smaller than 1.25 hours -->
                    <div v-else v-bind:style="{ 'background-color': bookingStatusBadges[booking.status].lightColor, 'border-color': bookingStatusBadges[booking.status].color }" style="border-width: 3px;">
                        <div class="row justify-content-between align-items-center position-relative overflow-hidden h-100" style="font-size: 0.9em;">
                            <span class="col-auto mx-1">#{{booking.id}}</span>
                            <i v-bind:class="'col-auto mx-1 ' + bookingStatusBadges[booking.status].icon"></i>
                        </div>
                    </div>
                </div>
            </div>
            <!-- current time line -->
            <div v-if="isTimeLineVisible()" class="position-absolute calendar-current-time" style="height: 3px; z-index: 2; background: linear-gradient(to bottom, #fff, #f00 50%, #fff);" v-bind:style="{
                width: calendarCellWidth + 'px',
                left: calendarCellWidth * currentMoment.weekday() + 'px',
                top: (currentMoment.hours() + currentMoment.minutes() / 60.0) * 50 - 1 + 'px' 
            }"></div>
        </div>
    `
});