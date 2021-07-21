userAdminVue = new Vue({
    el: "#userAdminDiv",
    data: {
        unverifiedUsers: [],
        verifiedUsers: [],
        email: "",
        isAdmin: false,
        priorityRole: "",
        errorMessages: [],
        hideUnverified: false,
        emailFilter: "",
        nameFilter: "",
        adminFilter: "noFilter",
        priorityFilter: "",
        locationFilter: "",
        currentlyEditedUser: "",
        editUserErrorMessage: "",
        deleteVerifiedUserErrorMessage: "",
        deleteUnverifiedUserErrorMessages: [],
        currentlyDeletedVerifiedUser: "",
        fetchErrorMessages: []
    },
    computed: {
        filteredUnverifiedUsers: function () {
            let filteredUnverUsers = this.unverifiedUsers;
            if (this.emailFilter !== "- nicht danach filtern -") {
                filteredUnverUsers = filteredUnverUsers.filter(x => x.email.toLowerCase().includes(this.emailFilter.toLowerCase()));
            }
            if (this.nameFilter !== "- nicht danach filtern -" && this.nameFilter !== "") {
                filteredUnverUsers = filteredUnverUsers.filter(() => false);
            }
            if (this.adminFilter !== "noFilter") {
                filteredUnverUsers = filteredUnverUsers.filter(x => x.isAdmin === ((this.adminFilter === "admin") ? true : false));
            }
            if (this.priorityFilter !== "") {
                filteredUnverUsers = filteredUnverUsers.filter(x => this.priorityFilter.toLowerCase() === x.priorityRole.toLowerCase());
            }
            if (this.locationFilter !== "") {
                filteredUnverUsers = filteredUnverUsers.filter(() => false);
            }
            return filteredUnverUsers.sort((a,b) => a.email.localeCompare(b.email));
        },
        filteredVerifiedUsers: function () {
            let filteredVerUsers = this.verifiedUsers;
            if (this.emailFilter !== "- nicht danach filtern -") {
                filteredVerUsers = filteredVerUsers.filter(x => x.email.toLowerCase().includes(this.emailFilter.toLowerCase()));
            }
            if (this.nameFilter !== "- nicht danach filtern -" && this.nameFilter !== "") {
                filteredVerUsers = filteredVerUsers.filter(x => x.firstName.concat(" " + x.lastName).toLowerCase().includes(this.nameFilter.toLowerCase()));
            }
            if (this.adminFilter !== "noFilter") {
                filteredVerUsers = filteredVerUsers.filter(x => x.isAdmin === ((this.adminFilter === "admin") ? true : false));
            }
            if (this.priorityFilter !== "") {
                filteredVerUsers = filteredVerUsers.filter(x => this.priorityFilter.toLowerCase() === x.priorityRole.toLowerCase());
            }
            if (this.locationFilter !== "") {
                filteredVerUsers = filteredVerUsers.filter(x => this.locationFilter.toLowerCase() === x.defaultLocation.toLowerCase());
            }
            return filteredVerUsers.sort((a, b) => a.email.localeCompare(b.email));
        }
    },
    methods: {
        onSubmit: function () {
            const newUser = {
                email: this.email,
                isAdmin: this.isAdmin,
                priorityRole: this.priorityRole,
                emailSent: false
            };
            this.email = "";
            this.isAdmin = false;
            this.priorityFilter = "";
            this.emailSent = "";
            this.unverifiedUsers = this.unverifiedUsers.filter(x => x.email !== newUser.email);
            this.unverifiedUsers.push(newUser);
            $.ajax({
                url: "/user/create",
                method: "post",
                data: {
                    email: newUser.email,
                    isAdmin: newUser.isAdmin,
                    priorityRole: newUser.priorityRole,
                    __RequestVerificationToken: document.querySelector("#addUser-collapse [name=__RequestVerificationToken]").value
                }
            }).done(() => {
                newUser.emailSent = true;
                this.fetchUsers();
            }).fail((response) => {
                this.unverifiedUsers = this.unverifiedUsers.filter(x => x !== newUser);
                this.fetchUsers();
                if (response["responseText"]) {
                    const newError = {
                        email: newUser.email,
                        message: response.responseText
                    };
                    this.errorMessages.push(newError)
                } else {
                    const newError = {
                        email: newUser.email,
                        message: "Technischer Fehler bei der Anfrage"
                    };
                    this.errorMessages.push(newError)
                }
            });
        },

        setCurrentlyEditedUser(user) {
            this.editUserErrorMessage = "";
            this.currentlyEditedUser = Vue.util.extend({}, user);
        },

        editUser() {
            this.editUserErrorMessage = "";
            $.ajax({
                url: "/user/edit",
                method: "post",
                data: {
                    email: this.currentlyEditedUser.email,
                    firstName: this.currentlyEditedUser.firstName,
                    lastName: this.currentlyEditedUser.lastName,
                    isAdmin: this.currentlyEditedUser.isAdmin,
                    priorityRole: this.currentlyEditedUser.priorityRole,
                    __RequestVerificationToken: document.querySelector("#editUser-modal [name=__RequestVerificationToken]").value
                }
            }).done(() => {
                this.fetchUsers();
                $('#editUser-modal').modal("hide");
            }).fail((response) => {
                this.fetchUsers();
                if (response["responseText"]) {
                    this.editUserErrorMessage = response.responseText;
                } else {
                    this.editUserErrorMessage = "Technischer Fehler bei der Anfrage";
                }
            });
        },

        setCurrentlyDeletedVerifiedUser(user) {
            this.deleteVerifiedUserErrorMessage = "";
            this.currentlyDeletedVerifiedUser = Vue.util.extend({}, user);
        },

        deleteUnverifiedUser(email) {
            $.ajax({
                url: "/user/deleteUnverifiedUser",
                method: "post",
                data: {
                    email: email,
                    __RequestVerificationToken: document.querySelector("#list-section-unver [name=__RequestVerificationToken]").value
                }
            }).done(() => {
                this.fetchUsers();
            }).fail((response) => {
                this.fetchUsers();
                if (response["responseText"]) {
                    this.deleteUnverifiedUserErrorMessages.push("Fehler beim Löschen des unverifizierten Nutzers " + email + ": " + response.responseText)
                } else {
                    this.deleteUnverifiedUserErrorMessages.push("Fehler beim Löschen des unverifizierten Nutzers " + email + ": Technischer Fehler bei der Anfrage")
                }
            });
        },

        deleteVerifiedUser(email) {
            this.deleteVerifiedUserErrorMessage = "";
            $.ajax({
                url: "/user/deleteVerifiedUser",
                method: "post",
                data: {
                    email: email,
                    __RequestVerificationToken: document.querySelector("#deleteVerifiedUser-modal [name=__RequestVerificationToken]").value
                }
            }).done(() => {
                this.fetchUsers();
                $('#deleteVerifiedUser-modal').modal("hide");
            }).fail((response) => {
                this.fetchUsers();
                if (response["responseText"]) {
                    this.deleteVerifiedUserErrorMessage = response.responseText;
                } else {
                    this.deleteVerifiedUserErrorMessage = "Technischer Fehler bei der Anfrage";
                }
            });
        },

        fetchUsers() {
            $.ajax({
                url: '/user/users',
                method: 'get'
            }).done(data => {
                let responseJson = JSON.parse(data);
                this.verifiedUsers = responseJson.verifiedUsers;
                this.unverifiedUsers = this.unverifiedUsers.filter(x => !(x.emailSent));
                const fetchedUsers = responseJson.unverifiedUsers;
                fetchedUsers.forEach(x => { x.emailSent = true; });
                this.unverifiedUsers = this.unverifiedUsers.concat(fetchedUsers);
            }).fail(() => {
                this.fetchErrorMessages.push("Es gab einen technischen Fehler beim Aktualisieren der Nutzer");
            });
        }
    },

    mounted: function () {
        this.fetchUsers();
    }
})