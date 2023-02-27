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
                // Add leading zeros if the number is less than 100
                upstream.inputChange(input.id.replace("!", ""), newValue)
            }
        }
    });

    input.onkeydown = () => {
        return false;
    }

});

function setInputValue(ele, val){
    if (ele.disabled) return;

    ele.valueAsNumber = val;
    ele.value = val.toString().padStart(parseInt(ele.dataset.digits), '0');
}

setInterval(() => {
    upstream.requestInputValues().then(res => {
        res = JSON.parse(res);
        let inputs = res.inputs;
        
        setInputValue($g("!hitdelay-sel"), inputs.hnavDelayRef);
        setInputValue($g("!movedelay-sel"), inputs.mnavDelayRef);
        setInputValue($g("!minimum-acc"), inputs.minimumAcc);
        setInputValue($g("!targetloc-offset"), inputs.targetOffsetAmount);
        setInputValue($g("!spinner-rand"), inputs.spinnerRandomAmount);
        setInputValue($g("!targetloc-thresh"), inputs.targetOffsetThreshold);
        setInputValue($g("!sliderhalt-thresh"), inputs.sliderHaltThreshold);

        $g("!cal-offset").disabled = !res.enableCalib
        $g("!cal-offset").value = res.calib == 0 ? `STD` : `${res.calib >= 0 ? "+" : "-"}${Math.abs(res.calib).toString().padStart(3, '0')}`;

    });
}, 1);