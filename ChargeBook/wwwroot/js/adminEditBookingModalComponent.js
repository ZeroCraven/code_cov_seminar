Vue.component("admin-edit-booking-modal", {
    name: "adminEditBooking",
    props: {
        locations: Array
    },
    data: function () {
        return {
            booking: null,
            errorMessage: ""
        };
    },
    methods: {
        showModal(booking) {
            this.errorMessage = ""
            this.booking = Vue.util.extend({}, booking)
            this.booking.timePeriods = this.booking.timePeriods.map(timePeriod => {
                return {
                    startTime: timePeriod.startTime.format("YYYY-MM-DDTHH:mm"),
                    endTime: timePeriod.endTime.format("YYYY-MM-DDTHH:mm"),
                }
            });
            if (booking.fullChargePointString) {
                Vue.set(this.booking, 'chargeGroupName', booking.fullChargePointString.split("-")[0]);
                Vue.set(this.booking, 'chargeStationName', booking.fullChargePointString.split("-")[1]);
                Vue.set(this.booking, 'chargePointIndex', parseInt(booking.fullChargePointString.split("-")[2]));
            } else {
                Vue.set(this.booking, 'chargeGroupName', "");
                Vue.set(this.booking, 'chargeStationName', "");
                Vue.set(this.booking, 'chargePointIndex', "");
            }
            this.$nextTick(x => {
                this.$refs.adminBookingForm.reset();
                $('#editBookingModal').modal('show');
            });
        },
        handleBookingChange() {
            this.$emit('booking-change')
            $('#editBookingModal').modal('hide');
        }
    },
    template: `
<div class="modal fade" id="editBookingModal" tabindex="-1" role="dialog" aria-labelledby="exampleModalLabel" aria-hidden="true">
    <div class="modal-dialog modal-lg" role="document">
        <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Buchung editieren</h5>
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                        <span aria-hidden="true">&times;</span>
                    </button>
                </div>
                <div class="modal-body">
                    <div class="form-group m-0">
                        <admin-booking-form ref="adminBookingForm" v-if="booking"
                            v-bind:locations="locations"
                            v-bind:booking="booking"
                            v-bind:bookingaction="'adminEdit'"
                            v-on:error-message="errorMessage = $event"
                            v-on:booking-change="handleBookingChange()">
                        </admin-booking-form>
                    </div>
                </div>
                <div class="modal-footer">
                    <span class="text-danger font-weight-bold mr-auto align-self-center">{{errorMessage}}</span>
                    <button class="btn btn-danger" data-dismiss="modal" type="button">Abbrechen</button>
                    <button class="btn btn-primary" v-on:click="$refs.adminBookingForm.handleSubmit()">Speichern</button>
                </div>
        </div>
    </div>
</div>
    `
});