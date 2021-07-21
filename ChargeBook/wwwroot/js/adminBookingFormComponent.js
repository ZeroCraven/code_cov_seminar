Vue.component("admin-booking-form", {
    name: "adminBookingForm",
    props: {
        locations: Array,
        booking: Object,
        bookingaction: String
    },
    data: function () {
        return {
            userErrorMessage: "",
            userFetching: false,
            success: false,
            carsOfUser: [],
            bookingStatusBadges: bookingStatusBadges,
            fetchedLocation: null,
            locationErrorMessage: "",
            serverFormErrorMessage: "",
            timePeriodGotInputOnce: false,
            possibleConnectorTypes: possibleConnectorTypes
        }
    },
    computed: {
        displayTimePeriods() {
            if (this.isApprovedBooking) {
                return [this.booking.timePeriods[0]];
            }
            return this.booking.timePeriods;
        },
        isApprovedBooking() {
            return this.booking.status && this.booking.status !== "angefordert" && this.booking.status !== "abgelehnt";
        },
        chargeGroupNames() {
            if (!this.fetchedLocation) {
                return [];
            }
            return Object.keys(this.fetchedLocation.infrastructure.chargeGroups);
        },
        chargeStationNames() {
            if (!this.chargeGroupNames.includes(this.booking.chargeGroupName)) {
                return [];
            }
            return Object.keys(this.fetchedLocation.infrastructure.chargeGroups[this.booking.chargeGroupName].chargeStations)
        },
        chargePointIndices() {
            if (!this.chargeStationNames.includes(this.booking.chargeStationName)) {
                return [];
            }
            return this.fetchedLocation.infrastructure.chargeGroups[this.booking.chargeGroupName].chargeStations[this.booking.chargeStationName].chargePoints
                .map((x, index) => index);
        },
        possibleConnectorTypesOfSelectedStation() {
            if (!this.chargePointIndices.includes(parseInt(this.booking.chargePointIndex))) {
                return [];
            }
            if (this.carsOfUser.filter(x => x.name === this.booking.car).length === 0) {
                return [];
            }
            let chargeStationConnectorTypes = Object.keys(this.fetchedLocation.infrastructure.chargeGroups[this.booking.chargeGroupName].chargeStations[this.booking.chargeStationName].chargePoints[this.booking.chargePointIndex].connectors);
            let carConnectorTypes = Object.keys(this.carsOfUser.filter(x => x.name === this.booking.car)[0].connectors);
            let chargeStationAndCarConnectorTypesIntersection = chargeStationConnectorTypes.filter(x => carConnectorTypes.includes(x));
            return chargeStationAndCarConnectorTypesIntersection.map(x => this.possibleConnectorTypes[x]);
        },
        displayTimePeriodMoments() {
            const timePeriodsAsMoments = [];
            for (let timePeriod of this.displayTimePeriods) {
                timePeriodsAsMoments.push({
                    startTime: moment(timePeriod.startTime),
                    endTime: moment(timePeriod.endTime)
                });
            }
            return timePeriodsAsMoments;
        },
        timePeriodErrorMessages() {
            const errorMessages = [];
            for (let [index, timePeriod] of this.displayTimePeriodMoments.entries()) {
                if (!this.timePeriodGotInputOnce) {
                    errorMessages.push("");
                    continue;
                }
                if (!timePeriod.startTime.isValid() || !timePeriod.endTime.isValid()) {
                    errorMessages.push("noch unvollständig");
                    continue;
                }
                if (timePeriod.startTime.format("mm") % 15 !== 0 || timePeriod.endTime.format("mm") % 15 !== 0) {
                    errorMessages.push("Die Zeit darf nur in 15 min Schritten angegeben werden");
                    continue;
                }
                if (timePeriod.endTime.isSame(timePeriod.startTime)) {
                    errorMessages.push("Es muss ein Zeitraum von mindestens 15 min ausgewählt werden");
                    continue;
                }
                if (timePeriod.startTime.isAfter(timePeriod.endTime)) {
                    errorMessages.push("Es muss ein positiver Zeitraum ausgewählt werden");
                    continue;
                }
                if (this.booking.status === "angefordert" && moment().isSameOrAfter(timePeriod.startTime)) {
                    errorMessages.push("Der Zeitraum bei einer angeforderten Buchung muss in der Zukunft liegen");
                    continue;
                }
                let errorMessage = "";
                for (let [innerIndex, comparedTimePeriod] of this.displayTimePeriodMoments.entries()) {
                    if (index === innerIndex) {
                        continue;
                    }
                    if (!comparedTimePeriod.startTime.isValid() || !comparedTimePeriod.endTime.isValid()) {
                        continue;
                    }
                    if (!(timePeriod.endTime.isBefore(comparedTimePeriod.startTime) || timePeriod.startTime.isAfter(comparedTimePeriod.endTime))) {
                        errorMessage = "Die Zeitspanne kollidiert mit einer anderen ausgewählten Zeitspanne";
                        break;
                    }
                }
                errorMessages.push(errorMessage);
            }
            return errorMessages;
        }
    },
    methods: {
        removeTimePeriod(index) {
            if (this.booking.timePeriods.length > 1) {
                this.booking.timePeriods.splice(index, 1);
            }
        },
        addTimePeriod() {
            this.booking.timePeriods.push({
                startTime: null,
                endTime: null
            });
        },
        setStartSoC(value) {
            this.booking.startSoC = value;
            this.booking.targetSoC = Math.max(this.booking.startSoC, this.booking.targetSoC);
        },
        setTargetSoC(value) {
            this.booking.targetSoC = value;
            this.booking.startSoC = Math.min(this.booking.startSoC, this.booking.targetSoC);
        },
        handleUserInputFocusOut() {
            this.userErrorMessage = "";
            this.success = false;
            if (!this.booking.user) {
                this.userErrorMessage = "Pflichtfeld"
                return;
            }
            this.userFetching = true;
            $.ajax({
                url: "/user/cars",
                method: "get",
                data: {
                    userEmail: this.booking.user.toLowerCase()
                }
            }).done(x => {
                this.userFetching = false;
                this.success = true
                this.carsOfUser = JSON.parse(x).cars;
            }).fail(x => {
                this.userFetching = false;
                if (x.responseText) {
                    this.userErrorMessage = "\"" + this.booking.user + "\" existiert nicht";
                } else {
                    this.userErrorMessage = "Server konnte nicht erreicht werden";
                }
            });
        },
        handleUserInput() {
            this.userErrorMessage = "";
            this.success = false;
            this.booking.car = "";
        },
        fetchInfrastructure() {
            this.locationErrorMessage = "";
            if (!this.booking.location) return;
            $.ajax({
                url: "/infrastructure/infrastructure",
                method: "get",
                data: {
                    locationName: this.booking.location
                }
            }).done(data => {
                this.fetchedLocation = {
                    name: this.booking.location,
                    infrastructure: JSON.parse(data).infrastructure
                }
            }).fail(x => {
                this.locationErrorMessage = "technischer Fehler beim Laden der Infrastruktur"
                if (x.responseText) {
                    this.locationErrorMessage = x.responseText;
                }
            });
        },
        handleLocationInput(value) {
            this.fetchedLocation = null;
            this.booking.location = value;
            this.booking.chargeGroupName = "";
            this.booking.chargeStationName = "";
            this.booking.chargePointIndex = "";
            this.booking.connectorType = "";
            this.fetchInfrastructure();
        },
        handleChargeGroupInput(value) {
            this.booking.chargeGroupName = value;
            this.booking.chargeStationName = "";
            this.booking.chargePointIndex = "";
            this.booking.connectorType = "";
        },
        handleChargeStationInput(value) {
            this.booking.chargeStationName = value;
            this.booking.chargePointIndex = "";
            this.booking.connectorType = "";
        },
        handleChargePointIndexInput(value) {
            this.booking.chargePointIndex = value;
            this.booking.connectorType = "";
        },
        handleSubmit() {
            this.$emit('error-message', "");
            this.timePeriodGotInputOnce = true;
            if (!this.userErrorMessage && !this.booking.user) {
                this.userErrorMessage = "Pflichtfeld";
            }
            if (this.timePeriodErrorMessages.filter(x => x).length > 0 || this.userErrorMessage || this.locationErrorMessage) {
                this.$emit('error-message', "Sie müssen zuerst alle Fehler ausbessern");
                return;
            }
            const data = Object.assign({}, this.booking);
            data.timePeriods = this.booking.timePeriods.map(x => {
                return {
                    startTime: moment(x.startTime).utc().format("YYYY-MM-DDTHH:mm"),
                    endTime: moment(x.endTime).utc().format("YYYY-MM-DDTHH:mm"),
                }
            });
            data["__RequestVerificationToken"] = document.querySelector("[name=__RequestVerificationToken]").value
            $.ajax({
                url: "/booking/" + this.bookingaction,
                data: data,
                method: "post"
            }).done(x => {
                this.$emit('booking-change');
            }).fail(x => {
                if (x.responseText) {
                    this.$emit('error-message', x.responseText);
                } else {
                    this.$emit('error-message', "Server konnte nicht erreicht werden");
                }
            })
        },
        handleTimePeriodInput() {
            this.timePeriodGotInputOnce = true;
        },
        reset() {
            this.serverFormErrorMessage = "";
            this.userErrorMessage = "";
            this.success = false;
            this.fetchInfrastructure();
            if (this.booking && this.booking.user) {
                this.handleUserInputFocusOut();
            }
        }
    },
    template: `
<form asp-action="create" v-on:submit.prevent="handleSubmit()" class="form" id="adminBookingForm" v-cloak>
    <div class="form-group">
        <label for="userEmail">Nutzeremail</label>
        <div class="row">
            <div class="col">
                <input name="user" v-model="booking.user" class="form-control" v-on:focusout="handleUserInputFocusOut()" v-on:input="handleUserInput()" id="userEmail" />
            </div>
            <div class="col-auto" v-if="userFetching">
                <span class="spinner-border" role="status" aria-hidden="true"></span>
            </div>
        </div>
        <span v-if="!success" class="text-danger">{{userErrorMessage}}</span>
        <span v-else="success" class="text-success">Nutzer "{{booking.user}}" konnte geladen werden</span>
    </div>
    <div class="form-group">
        <label>Buchungsstatus</label>
        <select name="status" v-model="booking.status" class="form-control">
            <option value="" v-if="!booking.status">-nicht ausgewählt-</option>
            <option v-for="(badge, statusText) in bookingStatusBadges" v-bind:value="statusText">
                {{statusText}}
            </option>
        </select>
        <span class="badge" v-if="booking.status" style="font-weight: 500; font-size: 0.8em;"
              v-bind:style="{ 'background-color': bookingStatusBadges[booking.status].color, 'color': bookingStatusBadges[booking.status].fontColor }">
            <i v-bind:class="bookingStatusBadges[booking.status].icon"></i>
            {{booking.status}}
        </span>
    </div>
    <div class="form-group">
        <label for="locationName">Standort</label>
        <div class="row">
            <div class="col">
                <select name="location" class="form-control" id="locationName" v-bind:value="booking.location"
                        v-on:input="handleLocationInput($event.target.value)">
                        <option v-for="locationName in locations">{{locationName}}</option>
                        <option v-if="locations.length === 0" value="">- noch nichts angelegt -</option>
                </select>
            </div>
            <div class="col-auto" v-if="!fetchedLocation && !locationErrorMessage && booking.location">
                <span class="spinner-border" role="status" aria-hidden="true"></span>
            </div>
        </div>
        <span v-if="!fetchedLocation && locationErrorMessage" class="text-danger">{{locationErrorMessage}}</span>
    </div>
    <div class="form-group">
        <label>Fahrzeug</label>
        <select name="car" v-model="booking.car" class="form-control">
            <option v-if="!success" value="">- geben sie zuerst einen Nutzer an -</option>
            <option v-else-if="booking.car && !carsOfUser.map(x => x.name).includes(booking.car)" v-bind:value="booking.car">- dieses Auto existiert nicht mehr -</option>
            <option v-else-if="carsOfUser.length === 0" value="">- noch kein Fahrzeug angelegt -</option>
            <option v-else-if="!booking.car" value="">- noch nicht ausgewählt -</option>
            <option v-if="success" v-for="car in carsOfUser">{{car.name}}</option>
        </select>
    </div>
    <div>
        <label>aktueller Ladestand:</label> <span class="text-primary font-weight-bold">{{booking.startSoC}}%</span>
        <input name="startSoC" class="custom-range" type="range" v-bind:value="booking.startSoC" v-on:input="setStartSoC($event.target.value)" />
    </div>
    <div>
        <label>gewünschter Ladestand:</label> <span class="text-primary font-weight-bold">{{booking.targetSoC}}%</span>
        <input name="targetSoC" class="custom-range" type="range" v-bind:value="booking.targetSoC" v-on:input="setTargetSoC($event.target.value)" />
    </div>
    <div class="form-group">
        <label v-if="isApprovedBooking">
            Zeitraum
        </label>
        <label v-else>
            Zeiträume
        </label>
        <div v-for="(timePeriod, index) in displayTimePeriods" class="mb-2">
            <div class="row">
                <div class="col">
                    <input v-model="timePeriod.startTime" class="form-control" type="datetime-local" v-on:input="handleTimePeriodInput()" />
                </div>
                <div class="col">
                    <input v-model="timePeriod.endTime" class="form-control" type="datetime-local" v-on:input="handleTimePeriodInput()" />
                </div>
                <div class="col-auto" v-if="!isApprovedBooking" >
                    <button type="button" v-bind:disabled="booking.timePeriods.length === 1" class="btn" v-on:click.prevent="removeTimePeriod(index)">
                        <i class="fas fa-minus text-danger"></i>
                    </button>
                </div>
            </div>
            <span class="text-danger">{{timePeriodErrorMessages[index]}}</span>
        </div>
        <div class="row" v-if="!isApprovedBooking">
            <div class="col-auto ml-auto"  >
                <button type="button" class="btn" v-on:click.prevent="addTimePeriod()">
                    <i class="fas fa-plus text-success"></i>
                </button>
            </div>
        </div>
    </div>
    <div class="row" v-if="isApprovedBooking">
        <div class="form-group col-3">
            <label>Ladegruppe</label>
            <select name="chargeGroupName" class="form-control" v-bind:value="booking.chargeGroupName" v-on:input="handleChargeGroupInput($event.target.value)">
                <option v-if="fetchedLocation === ''" value="">erst Ort auswählen</option>
                <option v-else-if="chargeGroupNames.length === 0" value="">- noch keine Ladegruppe vorhanden -</option>
                <option v-else-if="!booking.chargeGroupName" value="">- noch nicht ausgewählt -</option>
                <option v-for="chargeGroupName in chargeGroupNames">{{chargeGroupName}}</option>
            </select>
        </div>
        <div class="form-group col-3">
            <label>Ladestation</label>
            <select namer="chargeStationName" class="form-control" v-bind:value="booking.chargeStationName" v-on:input="handleChargeStationInput($event.target.value)">
                <option v-if="!booking.chargeGroupName === ''" value="">- erst Ladestation auswählen -</option>
                <option v-else-if="chargeStationNames.length === 0" value="">- noch keine Ladestation vorhanden -</option>
                <option v-else-if="!booking.chargeStationName" value="">- noch nicht ausgewählt -</option>
                <option v-for="chargeStationName in chargeStationNames">
                    {{chargeStationName}}
                </option>
            </select>
        </div>
        <div class="form-group col-3">
            <label>Ladepunkt</label>
            <select name="chargePointIndex" class="form-control" v-bind:value="booking.chargePointIndex" v-on:input="handleChargePointIndexInput($event.target.value)">
                <option v-if="booking.chargeStationName ===''" value="">- erst Ladegruppe auswählen -</option>
                <option v-else-if="chargePointIndices.length === 0" value="">- noch kein Ladepunkt vorhanden -</option>
                <option v-else-if="booking.chargePointIndex === ''" value="">- noch nicht ausgewählt -</option>
                <option v-for="chargePointName in chargePointIndices">{{chargePointName}}</option>
            </select>
        </div>
        <div class="form-group col-3">
            <label>Stecker</label>
            <select name="connectorType" class="form-control" v-model="booking.connectorType">
                <option v-if="booking.chargePointIndex === ''" value="">- erst Ladestation auswählen -</option>
                <option v-else-if="booking.car === ''" value="">- erst Fahrzeug auswählen -</option>
                <option v-else-if="carsOfUser.filter(x => x.name === booking.car).length === 0" v-bind:value="booking.connectorType">- das zugehörige Auto existiert nicht mehr -</option>
                <option v-else-if="!booking.connectorType" value="">- noch nicht ausgewählt -</option>
                <option v-else-if="possibleConnectorTypesOfSelectedStation.length === 0" value="">- keine passenden Stecker zu diesem Auto -</option>
                <option v-for="connectorType in possibleConnectorTypesOfSelectedStation">{{connectorType}}</option>
            </select>
        </div>
    </div>
</form>
    `
});