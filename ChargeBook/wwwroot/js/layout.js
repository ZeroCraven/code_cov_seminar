$(function () {

    function checkMatchingPasswords() {
        let val1 = $("#newPassword").val();
        let val2 = $('#newPasswordRepeated').val();
        if (val1 !== val2) {
            $('#newPassword, #newPasswordRepeated').addClass('is-invalid');
            $('#changepw-submit').prop("disabled", "disabled");
            $('#changepw-nomatch-error').removeClass('d-none');
        } else {
            $('#newPassword, #newPasswordRepeated').removeClass('is-invalid');
            $('#changepw-submit').removeAttr("disabled");
            $('#changepw-nomatch-error').addClass('d-none');
        }
    }

    // check matching passwords on keyup event of input elements
    $('#newPassword, #newPasswordRepeated').keyup(function () {
        checkMatchingPasswords();
    });

    // prevent the default form submit and make custom ajax request to handle error messages
    $('#changepw-form').submit(function (e) {
        e.preventDefault();

        let oldPassword = $('#oldPassword').val();
        let newPassword = $('#newPassword').val();
        let antiForgeryToken = $('[name=__RequestVerificationToken]').val();

        $('#changepw-wrongpw-error').addClass('d-none');
        $('#changepw-fallback-error').addClass('d-none');

        $.ajax({
            url: "/user/changepassword",
            data: {
                "oldPassword": oldPassword,
                "newPassword": newPassword,
                "__RequestVerificationToken": antiForgeryToken
            },
            method: "POST"
        }).done(function (res) {
            $('#changepw-modal').modal('hide');
            $('#changepw-success-modal').modal('show');
        }).fail(function (res) {
            if (res.status === 400) {
                $('#changepw-wrongpw-error').removeClass('d-none');
            } else {
                $('#changepw-fallback-error').removeClass('d-none');
            }
        }).always(function () {
            $('#oldPassword').val("");
        });

    });
    
    document.getElementById("notificationsEnabled").addEventListener('click', function(event) {
        $.ajax({
            url: "/user/setNotification",
            method: "post",
            data: {
                enabled: this.checked,
                __RequestVerificationToken: document.querySelector("#notificationsEnabledForm [name=__RequestVerificationToken]").value 
            }
        });
    });

    document.getElementById("defaultLocation").addEventListener('input', function(event) {
        $.ajax({
            url: "/user/setDefaultLocation",
            method: "post",
            data: {
                location: this.value,
                __RequestVerificationToken: document.querySelector("#defaultLocationForm [name=__RequestVerificationToken]").value
            }
        })
    }),

    // when the change password modal hides, make sure to clear all input fields and error messages
    $('#changepw-modal').on('hidden.bs.modal', function () {
        $('#changepw-wrongpw-error').addClass('d-none');
        $('#changepw-fallback-error').addClass('d-none');
        $('#oldPassword').val("");
        $('#newPassword').val("");
        $('#newPasswordRepeated').val("");
        checkMatchingPasswords();
    });
});

if($('#cars-modal-vue').length) {
    let carsModalVue = new Vue({
        el: '#cars-modal-vue',
        data: {},
        methods: {
            // make sure car changes in the edit cars modal component applies to the select field inside the create booking view
            notifyCreateViewForCarsChange(cars) {
                let select = $('#newBookingRequest #selectCar');
                if(select.length) {
                    let selection = select.val();
                    select.empty();
                    for(let car of cars) {
                        let option = $('<option></option>').attr('value', car.name).text(car.name);
                        select.append(option);
                    }
                    select.val(selection);
                }
            }
        }
    });
}


