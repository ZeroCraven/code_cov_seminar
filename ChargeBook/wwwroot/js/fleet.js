new Vue({
    el: '#fleet',
    data: {
        moment: moment,
        models: [],
        fleet: [],
        errorMessage: ''
    },
    methods: {
        addModel() {
            this.fleet.push({
                model: '',
                count: 1,
                weekDistribution: [
                    true, //Mo
                    true, //Di
                    true, //Mi
                    true, //Do
                    true, //Fr
                    false, //Sa
                    false //So
                ],
                requestsPerWeek: 1,
                chargeEnergy: 80,
                timeBetweenRequestAndBooking: 1, //in days
                requestTimeSpans: [],
                priorityRole: ""
            });
            this.addRequestTimeSpan(this.fleet.length - 1);
        },
        handleCarModelChange(event, modelIndex) {
            let newIndex = event.target.selectedIndex;
            if (newIndex === this.models.length + 1) {
                event.target.selectedIndex = 0;
                this.fleet[modelIndex].model = '';
                $('#fleet-cars-modal').modal('show');
            } else {
                this.fleet[modelIndex].model = this.models[newIndex - 1].name;
            }
        },
        handleWeekDayClick(event, weekDay, modelIndex) {
            this.fleet[modelIndex].weekDistribution[weekDay] = !this.fleet[modelIndex].weekDistribution[weekDay];
        },
        addRequestTimeSpan(modelIndex) {
            this.fleet[modelIndex].requestTimeSpans.push(
                {
                    startTime: "08:00",
                    endTime: "17:00"
                }
            );
        },
        submit() {
            let viewModels = [];
            for (let fleetModel of this.fleet) {
                let modelObj = {
                    carName: fleetModel.model,
                    count: fleetModel.count,
                    perWeekRequestFrequency: fleetModel.requestsPerWeek,
                    chargedEnergyInPercent: Math.round(fleetModel.chargeEnergy),
                    requestTime: fleetModel.timeBetweenRequestAndBooking * 24,
                    priorityRole: fleetModel.priorityRole,
                    bookingDays: [0, 1, 2, 3, 4, 5, 6].filter(i => fleetModel.weekDistribution[i]).map(i => (i + 1) % 7),
                    requestedSimulationTimePeriods: fleetModel.requestTimeSpans.map(timeSpan => {
                        return {
                            minutesFromMidnightStart: parseInt(timeSpan.startTime.split(':')[0]) * 60 + parseInt(timeSpan.startTime.split(':')[1]),
                            minutesFromMidnightEnd: parseInt(timeSpan.endTime.split(':')[0]) * 60 + parseInt(timeSpan.endTime.split(':')[1])
                        }
                    })
                };
                viewModels.push(modelObj);
            }
            $.ajax({
                url: '/simulationadmin/setBookingGenerationSettings',
                method: 'post',
                data: {
                    __RequestVerificationToken: $('input[name=__RequestVerificationToken]').val(),
                    viewModels: viewModels
                }
            }).done(data => {
                let dataObj = JSON.parse(data);
                if (dataObj.success) {
                    this.errorMessage = "";
                    window.location = "/simulationAdmin/settings"
                } else {
                    if (dataObj.redirect) {
                        window.location = "/simulationAdmin/" + dataObj.redirect;
                    }
                    this.errorMessage = dataObj.message;
                }
            }).fail(err => {
                this.errorMessage = "Es ist ein technischer Fehler aufgetreten";
            });
        },
        fetchFleet() {
            $.ajax({
                url: '/simulationAdmin/bookingGenerationSettings',
                method: 'get',
                data: {}
            }).done(data => {
                let bookingGenerationSettings = JSON.parse(data);
                //empty fleet
                this.fleet.splice(0, this.fleet.length);
                for (let bookingGenerationSetting of bookingGenerationSettings) {
                    this.fleet.push({
                        model: bookingGenerationSetting.carName,
                        count: bookingGenerationSetting.count,
                        weekDistribution: [1, 2, 3, 4, 5, 6, 0].map(i => bookingGenerationSetting.bookingDays.includes(i)),
                        requestsPerWeek: bookingGenerationSetting.perWeekRequestFrequency,
                        chargeEnergy: bookingGenerationSetting.chargedEnergyInPercent,
                        timeBetweenRequestAndBooking: bookingGenerationSetting.requestTime / 24,
                        requestTimeSpans: bookingGenerationSetting.requestedSimulationTimePeriods.map(timePeriod => {
                            return {
                                startTime: moment().startOf('day').minutes(timePeriod.minutesFromMidnightStart).format('HH:mm'),
                                endTime: moment().startOf('day').minutes(timePeriod.minutesFromMidnightEnd).format('HH:mm')
                            }
                        }),
                        priorityRole: bookingGenerationSetting.priorityRole
                    });
                }
                if (this.fleet.length === 0) {
                    this.addModel();
                }
            })
        }
    },
    mounted() {
        this.fetchFleet();
    }
});