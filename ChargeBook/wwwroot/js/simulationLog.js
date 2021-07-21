let workloadChart;
let bookingChart;
let adhocBookingChart;
let peakOccupancyChart;
let peakWorkloadChart;
let energySuppliedChart;

let simulationLogVue = new Vue({
    el: '#simulationLog',
    data: {
        id: -1,
        bookingStatusBadges: bookingStatusBadges,
        currentTick: 0,
        loadingProgress: 0,
        showLog: false,
        invalidId: false,
        showAdHocInBookingChart: false,
        viewedChargeStationData: null,
        chart: {
            workloadStartTick: 0,
            workloadEndTick: 0,
            connectorType: "",
            chargeGroup: "",
            chargeStation: ""
        },
        log: {
            creatorEmail: "",
            snapshots: [],
            viewSimulationBookings: {},
            scenario: {
                simulatedInfrastructure: {
                    chargeGroups: {}
                },
                simulationSettings: {
                    generalSettings: {
                        name: '',
                        totalTicks: 0,
                        seed: -1,
                        timePeriod: {
                            startTime: "",
                            endTime: ""
                        },
                        tickLength: 0
                    }
                }
            },
            statistics: {}
        },
        moment: moment
    },
    watch: {
        filteredOccupancyData() {
            if (workloadChart) {
                workloadChart.data.datasets[0].data = this.filteredOccupancyData;
                workloadChart.update();
            }
        },
        filteredWorkloadData() {
            if (workloadChart) {
                workloadChart.data.datasets[1].data = this.filteredWorkloadData;
                workloadChart.update();
            }
        },
        'chart.chargeGroup'() {
            this.chart.chargeStation = "";
        }
    },
    computed: {
        startMoment() {
            return moment(this.log.scenario.simulationSettings.generalSettings.timePeriod.startTime);
        },
        endMoment() {
            return moment(this.log.scenario.simulationSettings.generalSettings.timePeriod.endTime);
        },
        tickLengthMinutes() {
            return this.log.scenario.simulationSettings.generalSettings.tickLength;
        },
        totalTicks() {
            return this.log.scenario.simulationSettings.generalSettings.totalTicks;
        },
        sortedChargeGroups() {
            return Object.entries(this.log.scenario.simulatedInfrastructure.chargeGroups).sort(([key1, value1], [key2, value2]) => key1.localeCompare(key2));
        },
        maxOccupancy() {
            let count = 0;
            for (let chargeGroup of Object.values(this.log.scenario.simulatedInfrastructure.chargeGroups)) {
                for (let chargeStation of Object.values(chargeGroup.chargeStations)) {
                    count += chargeStation.chargePoints.length;
                }
            }
            return count;
        },
        maxWorkload() {
            let maxWorkload = 0;
            for (let chargeGroup of Object.values(this.log.scenario.simulatedInfrastructure.chargeGroups)) {
                maxWorkload += chargeGroup.maxChargePower;
            }
            return maxWorkload;
        },
        occupancyData() {
            let data = [];
            let startMomentCpy = this.startMoment.clone();
            for (let i = 0; i < this.totalTicks; i++) {
                let countChargePointsOccupied = 0;

                let snapshot = this.log.snapshots[i];
                for (let [groupName, chargeGroup] of Object.entries(snapshot.chargeGroups)) {
                    for (let [stationName, chargeStation] of Object.entries(chargeGroup.chargeStations)) {
                        countChargePointsOccupied += chargeStation.chargePoints.filter(p => {
                            if (p.usedPower <= 0) {
                                return false;
                            }
                            if (this.chart.connectorType && p.usedConnector !== parseInt(this.chart.connectorType)) {
                                return false;
                            }
                            if (this.chart.chargeGroup && groupName !== this.chart.chargeGroup) {
                                return false;
                            }
                            if (this.chart.chargeStation && stationName !== this.chart.chargeStation) {
                                return false;
                            }
                            return true;
                        }).length;
                    }
                }

                let occupancy = countChargePointsOccupied / this.maxOccupancy * 100;
                data.push({
                    x: startMomentCpy.clone(),
                    y: occupancy
                });
                startMomentCpy.add(this.tickLengthMinutes, 'minutes');
            }
            return data;
        },
        workloadData() {
            let data = [];
            let startMomentCpy = this.startMoment.clone();

            for (let i = 0; i < this.totalTicks; i++) {

                let usedWorkload = 0;

                let snapshot = this.log.snapshots[i];
                for (let [groupName, chargeGroup] of Object.entries(snapshot.chargeGroups)) {
                    for (let [stationName, chargeStation] of Object.entries(chargeGroup.chargeStations)) {
                        for (let chargePoint of chargeStation.chargePoints) {
                            if (this.chart.connectorType && chargePoint.usedConnector !== parseInt(this.chart.connectorType)) {
                                continue;
                            }
                            if (this.chart.chargeGroup && groupName !== this.chart.chargeGroup) {
                                continue;
                            }
                            if (this.chart.chargeStation && stationName !== this.chart.chargeStation) {
                                continue;
                            }
                            usedWorkload += chargePoint.usedPower;
                        }
                    }
                }

                let workload = usedWorkload / this.maxWorkload * 100;
                data.push({
                    x: startMomentCpy.clone(),
                    y: workload
                });
                startMomentCpy.add(this.tickLengthMinutes, 'minutes');
            }
            return data;
        },
        filteredOccupancyData() {
            return this.occupancyData.filter((data, index) => index >= this.chart.workloadStartTick && index <= this.chart.workloadEndTick);
        },
        filteredWorkloadData() {
            return this.workloadData.filter((data, index) => index >= this.chart.workloadStartTick && index <= this.chart.workloadEndTick);
        },
        currentMoment() {
            return this.startMoment.clone().add(this.tickLengthMinutes * this.currentTick, 'minutes');
        },
        currentSnapshot() {
            if (this.log.snapshots.length) {
                return this.log.snapshots[this.currentTick];
            } else {
                return {
                    deltaBookings: []
                }
            }
        }
    },
    methods: {
        viewChargeStation(chargeGroupName, chargeStationName) {
            this.viewedChargeStationData = {
                name: chargeGroupName + "-" + chargeStationName,
                chargeStation: this.log.scenario.simulatedInfrastructure.chargeGroups[chargeGroupName].chargeStations[chargeStationName],
                snapshotVersion: this.log.snapshots[this.currentTick].chargeGroups[chargeGroupName].chargeStations[chargeStationName]
            }
            this.$nextTick(() => {
                $('#chargeStationModal').modal('show'); 
            });
        },
        isChargePointOccupied(chargeGroupName, chargeStationName, chargePointIndex) {
            return this.currentSnapshot.chargeGroups[chargeGroupName].chargeStations[chargeStationName].chargePoints[chargePointIndex].usedPower > 0;
        },
        jumpToNextChange() {
            let tick = this.currentTick;
            while (++tick < this.totalTicks) {
                if (this.log.snapshots[tick].deltaBookings.length > 0) {
                    this.currentTick = tick;
                    return;
                }
            }
        },
        jumpToPreviousChange() {
            let tick = this.currentTick;
            while (--tick >= 0) {
                if (this.log.snapshots[tick].deltaBookings.length > 0) {
                    this.currentTick = tick;
                    return;
                }
            }
        },
        fetchLog() {
            let params = new URLSearchParams(window.location.search);
            this.id = params.get('id');
            if (this.id === null) {
                this.invalidId = true;
                return;
            }
            this.id = parseInt(this.id);
            let vue = this;
            $.ajax({
                xhr() {
                    let xhr = new window.XMLHttpRequest();
                    xhr.addEventListener('progress', evt => {
                        if (evt.lengthComputable) {
                            vue.loadingProgress = evt.loaded / evt.total * 100;
                        }
                    });
                    return xhr;
                },
                url: '/simulationadmin/getsimulationlog',
                method: 'get',
                data: {
                    id: this.id
                }
            }).done((data) => {
                this.loadingProgress = 100;
                setTimeout(() => {
                    this.showLog = true;
                    this.log = JSON.parse(data);
                    this.chart.workloadEndTick = this.totalTicks - 1;
                    this.$nextTick(() => {
                        $('#show-booking-change-radio').change(function () {
                            $(this).tab('show');
                            $(this).removeClass('active');
                        });
                        $('#show-infrastructure-radio').change(function () {
                            $(this).tab('show');
                            $(this).removeClass('active');
                        });
                        this.initBookingChart();
                        this.initAdhocBookingChart();
                        this.initPeakOccupancyChart();
                        this.initPeakWorkloadChart();
                        this.initEnergySuppliedChart();
                        this.initWorkloadChart();
                    });
                }, 700);
            }).fail(() => {
                this.invalidId = true;
            });
        },
        initBookingChart() {
            bookingChart = new Chart($('#booking-chart'), {
                type: 'pie',
                data: {
                    datasets: [
                        {
                            data: [this.log.statistics.countApprovedBookings, this.log.statistics.countDeniedBookings],
                            backgroundColor: [
                                '#28a745',
                                '#dc3545'
                            ],
                            hidden: false
                        }
                    ],
                    labels: [
                        'Angenommen',
                        'Abgelehnt'
                    ]
                },
                options: {
                    maintainAspectRatio: false
                }
            });
        },
        initAdhocBookingChart() {
            adhocBookingChart = new Chart($('#adhoc-booking-chart'), {
                type: 'pie',
                data: {
                    datasets: [
                        {
                            data: [
                                this.log.statistics.countApprovedAdHocBookings,
                                this.log.statistics.countDeniedAdHocBookings
                            ],
                            backgroundColor: [
                                '#28a745',
                                '#dc3545'
                            ]
                        }
                    ],
                    labels: [
                        'Angenommen',
                        'Abgelehnt'
                    ]
                },
                options: {
                    maintainAspectRatio: false
                }
            });
        },
        initWorkloadChart() {
            workloadChart = new Chart($('#workload-chart'), {
                type: 'line',
                data: {
                    datasets: [
                        {
                            label: 'Prozentuale Belegung',
                            data: this.filteredOccupancyData,
                            backgroundColor: 'rgba(77,84,255,0.2)',
                            borderColor: 'rgba(77,84,255, 1)',
                            borderWidth: 1,
                            pointRadius: 0
                        },
                        {
                            label: 'Prozentuale Auslastung',
                            data: this.filteredWorkloadData,
                            backgroundColor: 'rgba(255,99,132,0.2)',
                            borderColor: 'rgba(255, 99, 132, 1)',
                            borderWidth: 1,
                            pointRadius: 0
                        }
                    ]
                },
                options: {
                    scales: {
                        yAxes: [
                            {
                                ticks: {
                                    min: 0,
                                    max: 101,
                                    stepSize: 10,
                                    callback: function (value, index, values) {
                                        if (value === 101) {
                                            return "";
                                        }
                                        return value + '%';
                                    }
                                }
                            }
                        ],
                        xAxes: [
                            {
                                type: 'time',
                                time: {
                                    minUnit: 'minute',
                                    displayFormats: {
                                        minute: 'HH:mm',
                                        hour: 'HH:[00]',
                                        day: 'DD.MM'
                                    }
                                }
                            }
                        ]
                    },
                    maintainAspectRatio: false
                }
            });
        },
        initPeakOccupancyChart() {
            peakOccupancyChart = new Chart($('#peak-occupancy-chart'), {
                type: 'pie',
                data: {
                    datasets: [
                        {
                            data: [this.log.statistics.peakOccupancy, this.maxOccupancy - this.log.statistics.peakOccupancy],
                            backgroundColor: [
                                '#007bff',
                                '#6c757d'
                            ]
                        }
                    ],
                    labels: [
                        'Spitzenbelegung',
                        'Frei'
                    ]
                },
                options: {
                    legend: {
                        display: false
                    },
                    maintainAspectRatio: false
                }
            });
        },
        initPeakWorkloadChart() {
            peakWorkloadChart = new Chart($('#peak-workload-chart'), {
                type: 'pie',
                data: {
                    datasets: [
                        {
                            data: [this.log.statistics.peakWorkload, this.maxWorkload - this.log.statistics.peakWorkload],
                            backgroundColor: [
                                '#007bff',
                                '#6c757d'
                            ]
                        }
                    ],
                    labels: [
                        'Spitzenauslastung',
                        'Frei'
                    ]
                },
                options: {
                    legend: {
                        display: false
                    },
                    maintainAspectRatio: false
                }
            });
        },
        initEnergySuppliedChart() {
            energySuppliedChart = new Chart($('#energy-supplied-chart'), {
                type: 'pie',
                data: {
                    datasets: [
                        {
                            data: [this.log.statistics.suppliedEnergy, this.log.statistics.requestedEnergy - this.log.statistics.suppliedEnergy],
                            backgroundColor: [
                                '#007bff',
                                '#6c757d'
                            ]
                        }
                    ],
                    labels: [
                        'Geleistete Energie',
                        'Nicht geleistete Energie'
                    ]
                },
                options: {
                    legend: {
                        display: false
                    },
                    maintainAspectRatio: false
                }
            });
        }
    },
    mounted() {
        this.fetchLog();
    }
});