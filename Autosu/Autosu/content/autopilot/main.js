//upstream.openDev();

// Find all number input fields on the page
const numberInputs = document.querySelectorAll('input[type="number"]');

// Add event listeners to each input field
numberInputs.forEach(input => {
    input.addEventListener('wheel', e => {
        // Only update the input field if the mouse is over it
        if (e.target === input) {
            // Prevent the default scroll behavior
            e.preventDefault();
            if (input.disabled) return;

            // Get the current value of the input field
            let currentValue = input.valueAsNumber;
            
            // Get the minimum and maximum values allowed by the input field
            const minValue = input.min ? parseInt(input.min) : null;
            const maxValue = input.max ? parseInt(input.max) : null;
            
            // Get the amount to increment or decrement the value by
            const step = input.step ? parseInt(input.step) : 1;
            const delta = e.deltaY > 0 ? -step : step;
            
            // Update the current value
            const newValue = currentValue + delta;
            
            // Ensure the new value is within the minimum and maximum values
            if ((minValue === null || newValue >= minValue) && (maxValue === null || newValue <= maxValue)) {
                currentValue = newValue;
                input.valueAsNumber = currentValue;
                
                // Add leading zeros if the number is less than 100
                input.value = currentValue.toString().padStart(parseInt(input.dataset.digits), '0');
            }
        }
    });

    input.onkeydown = () => {
        return false;
    }

});

function returnToMenu() {
    upstream.returnToMenu();
}

function setAnnuciator(ele, value){
    if (ele.classList.contains('clickable')) {
        if (value) ele.classList.add("amber");
        else ele.classList.remove("amber");

    } else {
        if (value == 0) {
            ele.classList.remove("green");
            ele.classList.remove("amber");
        } else if (value == 1){
            ele.classList.remove("green");
            ele.classList.add("amber");
        } else if (value == 2){
            ele.classList.remove("amber");
            ele.classList.add("green");
        }


    }
}

upstream.initAutopilot().then(res => {
    $g("preview-img").src = res.bgPath;
    _("$chosen-beatmap-title", res.bmTitle);
    _("$chosen-beatmap-desc", res.bmDifficulty);

});

$g("@disengage").nextElementSibling.addEventListener("click", () => returnToMenu());

upstream.openDev();

setInterval(() => {
    upstream.requestCursorPosition().then(pos => {
        _("$test-ident", `Mouse position: [${pos.x}, ${pos.y}]`)
    })

    $g("!hitdelay-sel").disabled = !getEnabled($g("toggle:hnav"));
    $g("!movedelay-sel").disabled = !getEnabled($g("toggle:mnav"));
    $g("!movedelay-sel").disabled = !getEnabled($g("toggle:mnav"));
    $g("!minimum-acc").disabled = !getEnabled($g("toggle:accsel"));
    $g("!targetloc-offset").disabled = !getEnabled($g("toggle:targetoffset"));
    $g("!spinner-rand").disabled = !getEnabled($g("toggle:spinrandom"));

    upstream.requestAnnunciatorStatus().then(res => {
        for (let key in res){
            let annc = $g(`$annc:${key.replaceAll("_", "-")}`);
            setAnnuciator(annc, res[key]);

        }
    })

    upstream.getNextObject().then(res => {
        if (!res) return;
        _("$next-object", `[${res.x}, ${res.y}] in ${res.time}ms. ${res.queueLength} MNAVs, ${res.sysLatency}ms System Clock Latency.`)
    });

}, 1)