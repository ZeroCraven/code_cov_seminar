Vue.component("admin-create-booking-modal", {
    name: "adminCreateBooking",
    props: {
        locations: Array,
        defaultlocation: String
    },
    data: function () {
        return {
            booking: null,
            errorMessage: ""
            
        };
    },
    methods: {
        showModal() {
            this.errorMessage = ""
            this.booking = {
                car: "",
                status: "",
                location: this.defaultlocation,
                user: "",
                timePeriods: [
                    {
                        startTime: null,
                        endTime: null
                    }
                ],
                chargeGroupName: "",
                chargeStationName: "",
                chargePointIndex: "",
                connectorType: "",
                startSoC: 0,
                targetSoC: 0,
            }
            this.$nextTick(() => {
                this.$refs.adminBookingForm.reset();
                $('#createBookingModal').modal('show');
            });
        },
        handleBookingChange() {
            this.$emit('booking-change')
            $('#createBookingModal').modal('hide');
        }
    },
    template: `
<div class="modal fade" id="createBookingModal" tabindex="-1" role="dialog" aria-labelledby="exampleModalLabel" aria-hidden="true">
    <div class="modal-dialog modal-lg" role="document">
        <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Buchung erstellen</h5>
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                        <span aria-hidden="true">&times;</span>
                    </button>
                </div>
                <div class="modal-body">
                    <div class="form-group m-0">
                        <admin-booking-form ref="adminBookingForm" v-if="booking"
                            v-bind:locations="locations"
                            v-bind:defaultlocation="defaultlocation"
                            v-bind:booking="booking"
                            v-bind:bookingaction="'adminCreate'"
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