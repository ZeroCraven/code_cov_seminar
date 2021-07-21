
function card(index) {
    if (!mouseOnSelect && !cards[index].classList.contains("disabled")) {
        if (index == 0) {
            $("#empty-form").submit();
        } else if (index == 1) {
            $("#location-form").submit();
        } else if (index == 2) {
            document.getElementById("uploadedFile").click();
        }
    }
}

function setMouseOnSelect(bool) {
    mouseOnSelect = bool;
}

async function registerEvents() {
    if (document.getElementById("locationName") != null) {
        document.getElementById("locationName").addEventListener('mouseover', () => { mouseOnSelect = true; }, true);
        document.getElementById("locationName").addEventListener('mouseout', () => { mouseOnSelect = false; }, true);
    }
}

var mouseOnSelect;
var cards;

$(function () {
    mouseOnSelect = false;
    registerEvents();
    cards = document.getElementsByClassName("card");
    document.getElementById("uploadedFile").onchange = function () {
        if (document.getElementById("uploadedFile").value != "") {
        $("#file-form").submit();
        }
    };
})
