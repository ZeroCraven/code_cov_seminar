new Vue({
    el: '#simulation-history-vue',
    data: {
        moment: moment,
        filter: null,
        errorMessage: "",
        logs: []
    },
    computed: {
        filteredLogs() {
            return this.logs.filter(log => {
                if (!log.generalSettings.name.includes(this.filter.name)) {
                    return false;
                }
                if (!log.creatorEmail.includes(this.filter.user)) {
                    return false;
                }
                let logStartMoment = moment(log.startTime);
                let filterStartMoment = moment(this.filter.startedBetween.startTime);
                let filterEndMoment = moment(this.filter.startedBetween.endTime);
                if (filterEndMoment.isValid()) filterEndMoment.add(1, 'days'); //to make the filter inclusive
                if (filterStartMoment.isValid() && filterStartMoment > logStartMoment) {
                    return false;
                }
                if (filterEndMoment.isValid() && filterEndMoment < logStartMoment) {
                    return false;
                }
                return true;
            });
        }
    },
    watch: {
        filteredLogs() {
            this.$nextTick(() => this.initCharts());
        }
    },
    methods: {
        clearFilter() {
            this.filter = {
                name: "",
                user: "",
                startedBetween: {
                    startTime: "",
                    endTime: ""
                }
            }
        },
        deleteSimulationLog(id) {
            $.ajax({
                url: '/simulationadmin/deleteSimulationLog',
                method: 'post',
                data: {
                    id: id,
                    __RequestVerificationToken: $('input[name=__RequestVerificationToken]').val()
                }
            }).done(data => {
                let result = JSON.parse(data);
                if (result.success) {
                    this.logs = this.logs.filter(log => log.id !== id);
                    this.errorMessage = "";
                } else {
                    this.errorMessage = result.message;
                }
            }).fail(() => {
                this.errorMessage = "Es ist ein technischer Fehler aufgetreten";
            })
        },
        fetchLogs() {
            $.ajax({
                url: '/simulationadmin/historyApi',
                method: 'get'
            }).done(data => {
                this.logs = JSON.parse(data);
                this.$nextTick(() => this.initCharts());
            });
        },
        initCharts() {
            for (let log of this.filteredLogs) {
                log.chart = new Chart($('#history-chart-' + log.id), {
                    type: 'pie',
                    data: {
                        datasets: [
                            {
                                data: [log.statistics.countApprovedBookings, log.statistics.countDeniedBookings],
                                backgroundColor: [
                                    '#C1FF7D',
                                    '#FF6E68'
                                ]
                            }
                        ],
                        labels: [
                            'Angenommen',
                            'Abgelehnt'
                        ]
                    },
                    options: {
                        maintainAspectRatio: false,
                        legend: {
                            display: false
                        }
                    }
                });
            }
        }
    },
    created() {
        this.clearFilter();
    },
    mounted() {
        this.fetchLogs();
    }
});