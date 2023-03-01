//upstream.openDev();

// Find all number input fields on the page

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

function setToggle(name, value){
    var button = $g(`toggle:${name}`);
    var enableIndicator = button.nextElementSibling.querySelector(".d-flex").querySelector(".enable-indicator");
    enableIndicator.hidden = !value;
}

upstream.initAutopilot().then(res => {
    $g("preview-img").src = res.bgPath;
    _("$chosen-beatmap-title", res.bmTitle);
    _("$chosen-beatmap-desc", res.bmDifficulty);
    _("$timing-boundary", `${res.timingBoundary}ms`);

});

$g("@disengage").nextElementSibling.addEventListener("click", () => returnToMenu());

//upstream.openDev();

$g("@profile-read").addEventListener("click", () => {
    var name = $g("!profile-load").value || $g("!profile-load").placeholder;

    upstream.readProfile(name).then(res => {
        $g("!profile-load").value = "";
        if (!res.ok) {
            $g("!profile-load").value = res.msg;
            setTimeout(() => $g("!profile-load").value = "", 1000);
        } else {
            $g("!profile-load").placeholder = name;
        }
    });
});

$g("@profile-write").addEventListener("click", () => {
    var name = $g("!profile-load").value || $g("!profile-load").placeholder;

    upstream.writeProfile(name).then(msg => {
        $g("!profile-load").placeholder = name;
        $g("!profile-load").value = msg;
        setTimeout(() => $g("!profile-load").value = "", 1000);
    });
});

setInterval(() => {
    upstream.requestCursorPosition().then(pos => {
        _("$test-ident", `Mouse position: [${pos.x}, ${pos.y}]`)
    })

    $g("!hitdelay-sel").disabled = !(getEnabled($g("toggle:hnav")) && getEnabled($g("toggle:delayhit")) && !getEnabled($g("toggle:accsel")));
    $g("!movedelay-sel").disabled = !(getEnabled($g("toggle:mnav")) && getEnabled($g("toggle:delaymove")) && !getEnabled($g("toggle:accsel")));
    $g("!minimum-acc").disabled = !getEnabled($g("toggle:accsel"));
    $g("!targetloc-offset").disabled = !getEnabled($g("toggle:targetoffset"));
    $g("!targetloc-thresh").disabled = !getEnabled($g("toggle:targetoffset"));
    $g("!spinner-rand").disabled = !getEnabled($g("toggle:spinrandom"));
    $g("!sliderhalt-thresh").disabled = !getEnabled($g("toggle:sliderhalt"));

    upstream.requestAnnunciatorStatus().then(res => {
        for (let key in res){
            let annc = $g(`$annc:${key.replaceAll("_", "-")}`);
            setAnnuciator(annc, res[key]);

        }
    })

    upstream.requestTogglebtnStatus().then(res => {
        res = JSON.parse(res);
        var f = res.features;
        
        setToggle("cmd", res.cmdEnable);
        setToggle("hnav", f.hnav);
        setToggle("mnav", f.mnav);
        setToggle("n1", f.n1);
        setToggle("delayhit", f.hitDelay);
        setToggle("accsel", f.accuracySelect);
        setToggle("delaymove", f.moveDelay);
        setToggle("targetoffset", f.targetOffset);
        setToggle("blankmouse", f.blankAddMouse);
        setToggle("slidermvnt", f.humanSlider);
        setToggle("sliderhalt", f.shortSliderHalt);
        setToggle("spinoffset", f.spinnerOffset);
        setToggle("spinrandom", f.spinnerRandom);
        setToggle("humanendur", f.humanEndurance);
        setToggle("humannerv", f.humanNervous);
        setToggle("humandetent", f.humanDistraction);
        setToggle("autoswitch", f.autoSwitch);
        setToggle("tkovrstby", f.takeoverStandby);
        setToggle("detentstby", f.panic);

    });

    upstream.getNextObject().then(res => {
        if (!res) return;
        _("$next-object", `[${res.x}, ${res.y}] in ${res.time}ms. ${res.queueLength} MNAVs. #${res.extraData}.`)
    });

}, 1)