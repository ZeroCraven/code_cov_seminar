$(function () {
    document.getElementById("tickLengthValue").innerHTML = document.getElementById("tickLength").value;
    document.getElementById("tickLength").oninput = function () {
        document.getElementById("tickLengthValue").innerHTML = document.getElementById("tickLength").value;
    }

});

function submitGeneralSettingsAndStartSimulation() {
    $.ajax({
        url: '/simulationadmin/setgeneralsimulationsettings',
        method: 'post',
        data: {
            __RequestVerificationToken: $('input[name=__RequestVerificationToken]').val(),
            tickLength: $('#settings-form #tickLength').val(),
            timePeriod: {
                startTime: $('#settings-form #simulationBegin').val(),
                endTime: $('#settings-form #simulationEnd').val()
            },
            name: $('#settings-form #name').val(),
            seed: $('#settings-form #seed').val()
        }
    }).done(() => {
        $.ajax({
            url: '/simulationadmin/startSimulation',
            method: 'post',
            data: {
                __RequestVerificationToken: $('input[name=__RequestVerificationToken]').val()
            }
        }).done(data => {
            let id = JSON.parse(data).simulationId;
            window.location = '/homeAdmin/simulationLog?id=' + id;
        })
    }).fail(err => {
    })
}
