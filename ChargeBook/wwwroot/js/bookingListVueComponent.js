Vue.component('bookinglist', {
    name: "bookingList",
    props: ['displaybookings', 'showusercolumn'],
    data: function () {
        return {
            bookingStatusBadges: bookingStatusBadges
        };
    },
    computed: {
        displayBookingsWithDateRows() {
            let currentMoment = null;
            let bookingsWithDateRows = [];
            for (let i = 0; i < this.displaybookings.length; i++) {
                let bookingMoment = this.displaybookings[i].timePeriods[0].startTime.clone().hours(0).minutes(0).seconds(0).milliseconds(0);
                if (currentMoment == null || bookingMoment > currentMoment) {
                    currentMoment = bookingMoment;
                    bookingsWithDateRows.push({
                        type: "time",
                        data: bookingMoment.format('DD.MM.YYYY'),
                    });
                }
                bookingsWithDateRows.push({
                    type: "book",
                    data: this.displaybookings[i]
                })
            }
            return bookingsWithDateRows;
        }
    },
    template: `
        <div style="overflow: auto hidden;">
            <table class="table m-0 table-hover">
                <thead>
                    <tr>
                        <th scope="col">Status</th>
                        <th v-if="showusercolumn === 'true'" scope="col">Nutzer</th>
                        <th scope="col">Zeitraum</th>
                        <th scope="col">Ladepunkt</th>
                        <th scope="col">Standort</th>
                        <th scope="col">Ladevolumen</th>
                        <th scope="col"></th>
                    </tr>
                </thead>
                <simplebar style="overflow-y: auto;" class="d-flex flex-column" id="booking-list-scroll">
                    <tr v-for="renderObj in displayBookingsWithDateRows" v-bind:class="{'date-line-row': renderObj.type === 'time', 'booking-list-row': renderObj.type === 'book'}">
                        <th v-if="renderObj.type === 'time'">{{renderObj.data}}</th>
                        <td v-if="renderObj.type === 'book'">
                            <span class="badge" style="font-weight: 500; font-size: 0.8em;" v-bind:style="{ 'background-color': bookingStatusBadges[renderObj.data.status].color, 'color': bookingStatusBadges[renderObj.data.status].fontColor }"><i v-bind:class="bookingStatusBadges[renderObj.data.status].icon"></i> {{renderObj.data.status}}</span>
                        </td>
                        <td v-if="showusercolumn === 'true' && renderObj.type === 'book'"><span class="d-inline-block text-truncate">{{renderObj.data.user}}</span></td>
                        <td v-if="renderObj.type === 'book'" class="flex-row">
                            {{renderObj.data.timePeriods[0].startTime.format('HH:mm')}} - {{renderObj.data.timePeriods[0].endTime.format('HH:mm')}}
                            <button v-if="renderObj.data.timePeriods.length > 1" class="btn btn-link p-0 ml-1" style="font-size: 1.2em;" v-on:click="$emit('show-booking-details', renderObj.data)">...</button>
                        </td>
                        <td v-if="renderObj.type === 'book'">{{renderObj.data.fullChargePointString}}</td>
                        <td v-if="renderObj.type === 'book'">{{renderObj.data.location}}</td>
                        <td v-if="renderObj.type === 'book'" class="flex-column">
                            <div class="progress w-100" style="height: 20px;">
                                <div class="progress-bar" style="background-color: #dedede;" v-bind:style="{
                                    width: renderObj.data.startSoC + '%'
                                }"></div>
                                <div class="progress-bar progress-bar-striped bg-primary" v-bind:class="{'progress-bar-animated': renderObj.data.status === 'Ladevorgang begonnen'}" v-bind:style="{
                                    width: renderObj.data.targetSoC - renderObj.data.startSoC + '%'
                                }"></div>
                                <div class="progress-bar" style="background-color: #dedede;" v-bind:style="{
                                    width: 100 - renderObj.data.targetSoC + '%'
                                }"></div>
                            </div>
                            <span class="w-100 text-center" style="color: black;">{{renderObj.data.startSoC}}% - {{renderObj.data.targetSoC}}%</span>
                        </td>
                        <td v-if="renderObj.type === 'book'">
                            <button id="bookingDetails" class="mx-auto btn btn-sm btn-outline-secondary" v-on:click="$emit('show-booking-details', renderObj.data)"><i class="fas fa-info-circle"></i> Details</button>
                        </td>
                    </tr>
                    <tr v-if="displayBookingsWithDateRows.length === 0">
                        <td class="justify-content-around">
                            <span style="font-size: 1.2em;">Keine Buchungen vorhanden</span>
                        </td>
                    </tr>
                </simplebar>
            </table>
        </div>
    `
});