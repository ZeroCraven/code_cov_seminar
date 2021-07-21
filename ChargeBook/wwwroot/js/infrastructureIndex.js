$(function () {
    const sliders = document.querySelectorAll('.custom-range');;

    for (let i = 0; i < sliders.length; i++) {
        var output = document.getElementById(sliders[i].getAttribute("id") + "Value");
        output.innerHTML = sliders[i].value;
    }

    for (let i = 0; i < sliders.length; i++) {
        sliders[i].oninput = function () {
            var output = document.getElementById(sliders[i].getAttribute("id") + "Value");
            output.innerHTML = this.value;
        }
    }

    $(document).on("click", ".open-AddChargeStation", function () {
        var myChargeGroup = $(this).data('id');
        $(" #chargeGroupName").val(myChargeGroup);
    });
})

function changeDetailsButton(detailsId) {
    if (!document.getElementById(detailsId).classList.contains("show") && !document.getElementById(detailsId).classList.contains("collapsing")) {
        document.getElementById(detailsId + "ButtonSymbol").classList.replace("fa-chevron-down", "fa-chevron-up");
    } else if (document.getElementById(detailsId).classList.contains("show")) {
        document.getElementById(detailsId + "ButtonSymbol").classList.replace("fa-chevron-up", "fa-chevron-down");
    }
}