function validateAddLocation() {
    if ($("#locationName").val() == null || $("#locationName").val() == "") {
        if ($("#locationName-error").hasClass("text-hide")) {
            $("#locationName-error").removeClass("text-hide");
        }
        event.preventDefault();
        $("#locationName-error").text("Name des Standorts darf nicht leer sein!");
    } else if ($("#locationName").val().length > 50) {
        if ($("#locationName-error").hasClass("text-hide")) {
            $("#locationName-error").removeClass("text-hide");
        }
        event.preventDefault();
        $("#locationName-error").text("Name des Standorts darf höchstens 50 Zeichen lang sein!");
    } else {
        if (!$("#locationName-error").hasClass("text-hide")) {
            $("#locationName-error").addClass("text-hide");
        }
    }
}
