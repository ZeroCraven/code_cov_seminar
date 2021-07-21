forgotPasswordVue = new Vue({
    el: "#forgotPasswordModal",
    data: {
        message: "",
        error: false,
        success: false,
        loading: false,
        email: ""
    },
    name: "forgotPasswordModal",
    methods: {
        onSubmit: function () {
            this.loading = true;
            $.ajax({
                url: "/user/forgotPassword",
                method: "post",
                data: {
                    email: this.email,
                    __RequestVerificationToken: document.querySelector("#forgotPasswordModal [name=__RequestVerificationToken]").value
                }
            }).done(() => {
                this.loading = false;
                this.success = true;
            }).fail((response) => {
                this.loading = false;
                this.error = true;
                if (response["responseText"] && response.responseText.length < 40) {
                    this.message = response.responseText;
                } else {
                    this.message = "Technischer Fehler bei der Anfrage"
                }
            });
        }
    }
})