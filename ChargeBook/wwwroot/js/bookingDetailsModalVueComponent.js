Vue.component('booking-details-modal', {
    name: "bookingDetailsModal",
    data: function () {
        return {
            bookingStatusBadges: bookingStatusBadges,
            booking: null
        };
    },
    props: {
        admin: Boolean
    },
    methods: {
        showModal(booking) {
            this.booking = booking;
            this.$nextTick(() => $('#booking-info-modal').modal('show'));
        },
        deleteBooking() {
            $.ajax({
                url: "/booking/delete",
                method: "post",
                data: {
                    bookingId: this.booking.id,
                    __RequestVerificationToken: document.querySelector("[name=__RequestVerificationToken]").value
                }
            }).done(x => {
                this.$emit('booking-change');
                $('#booking-info-modal').modal('hide');
            })
        },
        edit() {
            $('#booking-info-modal').modal('hide');
            this.$emit('edit-booking', this.booking)
        }
    },
    template:
        `
<div class="modal" id="booking-info-modal">
    <div class="modal-dialog">
        <div class="modal-content" v-if="booking">
            <div class="modal-header">
                <span class="badge" style="font-weight: 500; font-size: 1.3em;" v-bind:style="{ 'background-color': bookingStatusBadges[booking.status].color, 'color': bookingStatusBadges[booking.status].fontColor }"><i v-bind:class="bookingStatusBadges[booking.status].icon"></i> {{booking.status}}</span>
                <button type="button" class="close" data-dismiss="modal">&times;</button>
            </div>

            <div class="modal-body">
                <div class="d-flex flex-column">
                    <label class="position-relative" id="bookingTime-label">
                        <ul class="list-group" id="bookingTime">
                            <li v-for="timePeriod in booking.timePeriods" class="list-group-item d-flex justify-content-center p-1">
                                <div>
                                    {{timePeriod.startTime.format('DD.MM.YYYY HH:mm [Uhr]')}}
                                    -
                                    {{timePeriod.endTime.format('DD.MM.YYYY HH:mm [Uhr]')}}
                                </div>
                            </li>
                        </ul>
                    </label>
                    <label class="d-flex flex-row" id="bookingId-label">
                        <div class="d-flex justify-content-start align-items-center" style="height: 25px; width: 25px">
                            <i class="fas fa-hashtag"></i>
                        </div>
                        <span id="bookingId">{{booking.id}}</span>
                    </label>
                    <label v-if="admin" class="d-flex flex-row" id="bookingUser-label">
                        <div class="d-flex justify-content-start align-items-center" style="height: 25px; width: 25px">
                            <i class="fas fa-user"></i>
                        </div>
                        <span id="bookingUser">{{booking.user}}</span>
                    </label>
                    <label class="d-flex flex-row" id="bookingLocation-label">
                        <div class="d-flex justify-content-start align-items-center" style="height: 25px; width: 25px">
                            <i class="fas fa-map-marker-alt"></i>
                        </div>
                        <span id="bookingLocation">{{booking.location}}</span>
                    </label>
                    <label class="d-flex flex-row" id="bookingChargeStation-label" v-if="booking.fullChargePointString">
                        <div class="d-flex justify-content-start align-items-center" style="height: 25px; width: 25px">
                            <i class="fas fa-charging-station"></i>
                        </div>
                        <span id="bookingChargeStation">{{booking.fullChargePointString}}</span>
                    </label>
                    <label class="d-flex flex-row" id="bookingConnectorType-label" v-if="booking.connectorType">
                        <div class="d-flex justify-content-start align-items-center" style="height: 25px; width: 25px">
                            <i class="fas fa-plug"></i>
                        </div>
                        <span id="bookingConnectorType">{{booking.connectorType}}</span>
                    </label>
                    <label class="d-flex flex-row" id="bookingCar-label">
                        <div class="d-flex justify-content-start align-items-center" style="height: 25px; width: 25px">
                            <i class="fas fa-car-alt"></i>
                        </div>
                        <span id="bookingCar">{{booking.car}}</span>
                    </label>
                    <div class="d-flex flex-row justify-content-start">
                        <label class="d-flex flex-row" id="bookingStartSoC-label">
                            <div class="d-flex justify-content-start align-items-center" style="height: 25px; width: 25px">
                                <i class="fas fa-battery-empty"></i>
                            </div>
                            <span id="bookingStartSoC">{{booking.startSoC}} %</span>
                        </label>
                        <label class="d-flex flex-row ml-2" id="bookingTargetSoC-label">
                            <div class="d-flex justify-content-start align-items-center" style="height: 25px; width: 25px">
                                <i class="fas fa-battery-full"></i>
                            </div>
                            <span id="bookingTargetSoC">{{booking.targetSoC}} %</span>
                        </label>
                    </div>
                    <!--<label class="d-flex flex-row" id="bookingStatus-label">
                    </label>-->
                </div>
            </div>

            <div class="modal-footer">
                <div class="d-flex" style="width: 100%">
                    <form class="mr-auto">
                        <button v-on:click.prevent="deleteBooking" class="btn btn-danger p-0" id="cancel-btn" data-toggle="tooltip" data-placement="top" title="Gedrückt halten" data-trigger="click">
                            <div class="progress position-relative" style="height: 40px; width: 100px; background-color: transparent;">
                                <div class="progress-bar progress-bar-striped bg-danger" style="width: 0; transition: width 0s linear;">

                                </div>
                                <span class="position-absolute" style="height: 40px; width: 100px; line-height: 40px; font-size: 1.3em;">
                                    <span v-if="booking.status === 'angenommen'">Stornieren</span>
                                    <span v-else>Löschen</span>
                                </span>
                            </div>
                        </button>
                    </form>
                    <button type="button" v-if="admin" v-on:click="edit()" class="btn btn-primary mx-1"><i class="fas fa-pencil-alt"></i></button>
                    <button type="button" class="btn btn-primary mx-1" data-dismiss="modal">Zurück</button>
                </div>
            </div>
        </div>
    </div>
</div>
    `
});
