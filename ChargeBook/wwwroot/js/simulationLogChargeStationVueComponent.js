Vue.component('simulation-log-charge-station-modal', {
    name: "SimulationLogChargeStationModal",
    props: {
        stationdata: Object,
        bookings: Object
    },
    data() {
        return {
            possibleConnectorTypes: window.possibleConnectorTypes,
            possibleConnectorTypeIndices: possibleConnectorTypeIndices
        }
    },
    mounted() {

    },
    template: `
        <div class="modal fade" id="chargeStationModal">
          <div class="modal-dialog" role="document">
            <div class="modal-content">
              <div class="modal-header">
                <h5 class="modal-title" id="exampleModalLabel"><i class="fas fa-charging-station"></i> {{stationdata.name}}</h5>
                <button type="button" class="close" data-dismiss="modal">
                  <span>&times;</span>
                </button>
              </div>
              <div class="modal-body">
                  <div class="d-flex justify-content-between">
                      <span>{{stationdata.chargeStation.modelName}}</span>
                      <div>
                        <span v-if="stationdata.snapshotVersion.usedChargePower" class="text-primary font-weight-bold">{{stationdata.snapshotVersion.usedChargePower / 1000}}</span>
                        <span v-else>0</span>
                        / {{stationdata.chargeStation.maxChargePower / 1000}} kW
                      </div>
                  </div>
                  <div v-for="(chargePoint, pointIndex) in stationdata.chargeStation.chargePoints">
                      <hr>
                      <div class="text-center mb-2">
                        Ladepunkt {{pointIndex}}
                      </div>
                      <div v-for="(maxConnectorPower, connectorType) in chargePoint.connectors" class="mb-3">
                        <div class="d-flex justify-content-between">
                            <span>{{possibleConnectorTypes[connectorType]}}</span>
                            <div>
                            <span v-if=
                                "stationdata.snapshotVersion.chargePoints[pointIndex].usedPower &&
                                possibleConnectorTypeIndices[stationdata.snapshotVersion.chargePoints[pointIndex].usedConnector] === connectorType"
                            class="text-primary font-weight-bold">
                            {{bookings[stationdata.snapshotVersion.chargePoints[pointIndex].bookingId].car}}</span><span v-if=
                                "stationdata.snapshotVersion.chargePoints[pointIndex].usedPower &&
                                possibleConnectorTypeIndices[stationdata.snapshotVersion.chargePoints[pointIndex].usedConnector] === connectorType"> mit </span>
                            <span v-if="stationdata.snapshotVersion.chargePoints[pointIndex].usedPower && possibleConnectorTypeIndices[stationdata.snapshotVersion.chargePoints[pointIndex].usedConnector] === connectorType" class="text-primary font-weight-bold">{{stationdata.snapshotVersion.chargePoints[pointIndex].usedPower / 1000}}</span>
                            <span v-else>0</span>
                             / {{maxConnectorPower / 1000}} kW
                            </div>
                        </div>
                        <div class="progress" v-if=
                                "stationdata.snapshotVersion.chargePoints[pointIndex].usedPower &&
                                possibleConnectorTypeIndices[stationdata.snapshotVersion.chargePoints[pointIndex].usedConnector] === connectorType">
                          <div class="progress-bar progress-bar-striped progress-bar-animated bg-primary" v-bind:style="{ width: stationdata.snapshotVersion.chargePoints[pointIndex].usedPower / stationdata.chargeStation.maxChargePower * 100 + '%' }"></div>
                          <div class="progress-bar progress-bar-striped" style="background-color: #878787;" v-bind:style="{ width: (maxConnectorPower - stationdata.snapshotVersion.chargePoints[pointIndex].usedPower) / stationdata.chargeStation.maxChargePower * 100 + '%' }"></div>
                        </div>
                        <div class="progress" v-else>
                          <div class="progress-bar progress-bar-striped" style="background-color: #878787;" v-bind:style="{ width: maxConnectorPower / stationdata.chargeStation.maxChargePower * 100 + '%' }"></div>
                        </div>
                      </div>
                  </div>
              </div>
              <div class="modal-footer">
                <button type="button" class="btn btn-primary" data-dismiss="modal">Schließen</button>
              </div>
            </div>
          </div>
        </div>
    `
});