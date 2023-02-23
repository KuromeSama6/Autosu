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

upstream.initAutopilot().then(res => {
    $g("preview-img").src = res.bgPath;
    _("$chosen-beatmap-title", res.bmTitle);
    _("$chosen-beatmap-desc", res.bmDifficulty);

});

upstream.openDev();

setInterval(() => {
    upstream.requestCursorPosition().then(pos => {
        _("$test-ident", `Mouse position: [${pos.x}, ${pos.y}]`)
    })
}, 1)
