Vue.component('edit-cars-modal', {
    name: 'Edit-Cars-Modal',
    props: [
        'modalid',
        'controllerurl',
        'carsingular',
        'carplural'
    ],
    data: function () {
        return {
            cars: [],
            possibleConnectorTypes: window.possibleConnectorTypes,
            connectors: [
                {
                    connectorType: "Bitte auswählen",
                    power: ""
                }
            ],
            name: "",
            batteryCapacity: "",
            hasError: false,
            validationMessage: ""
        }
    },
    computed: {
        freeConnectorTypes() {
            return Object.keys(this.possibleConnectorTypes).filter((s, i) => {
                return !this.connectors.some(obj => obj.connectorType === s);
            })
        }
    },
    methods: {
        addConnectorLine() {
            this.connectors.push({
                connectorType: "Bitte auswählen",
                power: ""
            });
        },
        removeConnectorLine(index) {
            this.connectors.splice(index, 1);
        },
        submit() {
            let data = {
                name: this.name,
                batteryCapacity: Math.round(this.batteryCapacity * 1000),
                __RequestVerificationToken: $('input[name=__RequestVerificationToken]').val()
            };
            this.connectors.filter(conn => conn.connectorType !== "Bitte auswählen").forEach(conn => data["connectors[" + conn.connectorType + "]"] = Math.round(conn.power * 1000));

            $.ajax({
                url: "/" + this.controllerurl + "/createCar",
                method: "post",
                data: data
            }).done(data => {
                let responseJson = JSON.parse(data);
                this.hasError = !responseJson.success;
                this.validationMessage = responseJson.message;
                if (responseJson.success) {
                    this.resetForm();
                }
                this.fetchCars();
            }).fail(() => {
                this.hasError = true;
                this.validationMessage = "Es gab einen technischen Fehler bei der Anfrage";
            });
        },
        resetForm() {
            this.name = "";
            this.connectors = [];
            this.addConnectorLine();
            this.batteryCapacity = "";
        },
        deleteCar(carName) {
            $.ajax({
                url: "/" + this.controllerurl + "/deleteCar",
                method: "post",
                data: {
                    carName: carName,
                    __RequestVerificationToken: $('input[name=__RequestVerificationToken]').val()
                }
            }).done(data => {
                let responseJson = JSON.parse(data);
                this.hasError = !responseJson.success;
                this.validationMessage = responseJson.message;
                this.fetchCars();
            }).fail(() => {
                this.hasError = true;
                this.validationMessage = "Es gab einen technischen Fehler bei der Anfrage";
            });
        },
        fetchCars() {
            $.ajax({
                url: "/" + this.controllerurl + "/cars",
                method: 'get'
            }).done(data => {
                let responseJson = JSON.parse(data);
                this.cars = responseJson.cars;
                this.$emit('cars-change', this.cars);
            }).fail(() => {
                this.hasError = true;
                this.validationMessage = "Es gab einen technischen Fehler beim Aktualisieren der Fahrzeuge";
            });
        }
    },
    mounted: function () {
        this.fetchCars();
    },
    template: `
    <div class="modal fade" v-bind:id="modalid">
        <div class="modal-dialog modal-lg">
            <div class="modal-content">
                <div class="modal-body">
                    <h4 class="mb-4 float-left">Meine {{carplural}}</h4>
                    <button type="button" class="close float-right" data-dismiss="modal">
                        <span>&times;</span>
                    </button>
                    <div class="clearfix"></div>
                    <div class="table-responsive">
                        <table class="table">
                            <thead>
                                <tr>
                                    <th>Name</th>
                                    <th>Batteriekapazität</th>
                                    <th colspan="2">Stecker</th>
                                    <th></th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr v-for="car in cars">
                                    <td>{{car.name}}</td>
                                    <td>{{car.batteryCapacity / 1000}} kWh</td>
                                    <td style="min-width: 7em;">
                                        <ul class="list-unstyled">
                                            <li v-for="(connector, index) in Object.keys(car.connectors)">
                                                {{possibleConnectorTypes[connector]}}:
                                            </li>
                                        </ul>
                                    </td>
                                    <td style="min-width: 5em;">
                                        <ul class="list-unstyled">
                                            <li v-for="(connector, index) in Object.keys(car.connectors)">
                                                {{car.connectors[connector] / 1000}} kW
                                            </li>
                                        </ul>
                                    </td>
                                    <td>
                                        <form asp-controller="User" asp-action="deleteCar">
                                            <input type="hidden" name="carName" v-bind:value="car.name" />
                                            <button type="submit" class="btn btn-danger btn-sm" v-on:click.prevent="deleteCar(car.name)">
                                                <i class="fas fa-trash-alt"></i>
                                            </button>
                                        </form>
                                    </td>
                                </tr>
                                <tr v-if="cars.length === 0">
                                    <td colspan="4" class="text-center">Noch keine {{carplural}} vorhanden</td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                    <form asp-action="createCar" asp-controller="User" class="mr-3" id="edit-cars-form">
                        <h4 class="mb-4">Neues {{carsingular}} anlegen</h4>
                        <div class="form-group row">
                            <label class="col-4 col-lg-3 col-form-label text-break">Name:</label>
                            <input class="col form-control" type="text" name="carName" placeholder="Name" v-model:value="name" />
                        </div>
                        <div class="form-group row">
                            <label class="col-4 col-lg-3 col-form-label text-break">Batteriekapazität:</label>
                            <div class="col input-group px-0 align-items-center">
                                <input class="form-control" type="number" name="carCapacity" placeholder="Batteriekapazität" v-model:value="batteryCapacity" />
                                <div class="input-group-append">
                                    <span class="input-group-text">kWh</span>
                                </div>
                            </div>
                        </div>
                        <div class="form-group row" id="plugs-form-group">
                            <label class="col-4 col-lg-3 col-form-label text-break">Stecker:</label>
                            <div class="col-8 col-lg-9">
                                <div class="row align-items-center mb-3" v-for="(connector, index) in connectors">
                                    <div class="col row pr-3 mr-0">
                                        <select class="col-12 col-md-6 form-control" type="text" v-model="connector.connectorType">
                                            <option selected v-if="connector.connectorType === 'Bitte auswählen'" v-bind:value="connector.connectorType">Bitte auswählen</option>
                                            <option selected v-else v-bind:value="connector.connectorType">{{possibleConnectorTypes[connector.connectorType]}}</option>
                                            <option v-for="freeConnectorType in freeConnectorTypes" v-bind:value="freeConnectorType">{{possibleConnectorTypes[freeConnectorType]}}</option>
                                        </select>
                                        <div class="col-12 col-md-6 input-group  align-items-center pl-0 pl-md-3 pr-0 mt-2 mt-md-0">
                                            <input class="form-control" type="number" placeholder="Steckerleistung" v-model:value="connector.power" />
                                            <div class="input-group-append">
                                                <span class="input-group-text">kW</span>
                                            </div>
                                        </div>
                                    </div>
                                    <button class="col-auto btn btn-lg text-danger p-0" v-on:click.prevent="removeConnectorLine(index)" v-bind:disabled="connectors.length === 1">
                                        <i class="fas fa-minus"></i>
                                    </button>
                                </div>
                            </div>
                            <button v-if="Object.keys(possibleConnectorTypes).length > connectors.length" class="btn btn-lg btn-link p-0 text-success align-self-center ml-auto" v-on:click.prevent="addConnectorLine()">
                                <i class="fas fa-plus"></i>
                            </button>
                        </div>
                        <input class="btn btn-primary float-left" type="submit" v-bind:value="carsingular + ' anlegen'" v-on:click.prevent="submit()" />
                        <span class="float-left ml-3 mt-2 font-weight-bold" v-bind:class="{'text-danger': hasError, 'text-success': !hasError}">{{validationMessage}}</span>
                    </form>
                </div>
            </div>
        </div>
    </div>
    `
});