currentBeatmaps = [];
currentDifficulties = [];
selected = {
    title: "",
    variation: ""
}

function selectFiles(data){
    data = JSON.parse(data);

    _("$current-beatmaps-path", data.path.replaceAll("\\\\", "\\"));
    currentBeatmaps = data.beatmaps;
    currentDifficulties = data.difficulties;
    renderSongs("");
}

function renderSongs(filter){
    $g("@reload-btn").hidden = false;

    // get the candidates
    var candidates = [];
    for (let name in currentBeatmaps) {
        if (filter == "" || name.toLocaleLowerCase().includes(filter.toLocaleLowerCase())) candidates.push(name);
    }

    _("message", `Loaded <b>${Object.keys(currentBeatmaps).length}</b> Beatmaps, Showing <b>${candidates.length}</b> Title(s)${filter == "" ? "" : ` that includes <code>${filter}</code>`}.`);

    // render the song list
    for (let ele of $g("beatmaps-container").querySelectorAll("a")) if (ele.id != "beatmap-listing-template") ele.remove();
    for (let title of candidates) {
        let ele = Clone($g("beatmaps-container"), $g("beatmap-listing-template"));
        ele.hidden = false;
        ele.id = "";
        let children = ele.querySelectorAll("span");

        children[0].innerHTML = title;
        children[1].innerHTML = `${currentBeatmaps[title].length} DIFFICULTIES`;

        ele.href = `javascript:renderDifficulties("${title}");`;

    }

}

function notifyFailedFiles(files){
    files = JSON.parse(files.replaceAll("\\", "\\\\"));
    
    for (let pth of files){
        let path = pth.replaceAll("\\\\", "\\");
        let name = path.split("\\")[path.split("\\").length - 1];

        let ele = Clone($g("beatmaps-container"), $g("beatmap-listing-template"));
        ele.hidden = false;
        ele.disabled = true;
        ele.id = "";
        ele.classList.remove("list-group-item-action");
        ele.classList.add("list-group-item-danger");
        let children = ele.querySelectorAll("span");

        children[0].innerHTML = name;
        children[1].innerHTML = `LOAD FAIL`;
        children[1].classList.add("bg-danger");
        children[1].classList.remove("bg-primary");

    }

    if (files.length > 0){
        _("message", `${$g("message").innerHTML}<br>${files.length} beatmaps failed to load because it is of a older format, or is corrupted.`);
    }

}

//upstream.openDev();

function renderDifficulties(title){
    const startBtn = $g("@start-btn");

    // render the difficulties
    for (let ele of $g("difficulty-container").querySelectorAll("a")) if (ele.id != "difficulty-listing-template") ele.remove();
    for (let difficulty of currentBeatmaps[title]) {
        let index = currentBeatmaps[title].indexOf(difficulty);
        let ele = Clone($g("difficulty-container"), $g("difficulty-listing-template"));
        ele.hidden = false;
        ele.id = "";
        ele.innerHTML = `${difficulty} [${currentDifficulties[title][index] / 10}]`;

        ele.addEventListener("click", () => {
            startBtn.hidden = true;

            upstream.loadBeatmapPreview(title, difficulty).then(res => {
                if (res == null) {
                    _("$chosen-beatmap-title", "UNABLE SELECT BEATMAP");
                    return;
                }

                startBtn.hidden = false;
                selected.title = title;
                selected.difficulty = difficulty;
                
                $g("preview-img").src = res.bgPath;
                _("$chosen-beatmap-title", title);
                _("$chosen-beatmap-desc", difficulty);

                $g("confirm-box").scrollIntoView({behavior: "smooth"});

            });

        });

    }
}


$g("!filedir-input").addEventListener("change", () => {

    upstream.selectBeatmapDirectory($g("!filedir-input").value).then((response) => {
        if (response != null) selectFiles(response);
    });
});
$g("!name-filter").addEventListener("keyup", () => {
    renderSongs($g("!name-filter").value);
})
$g("@start-btn").addEventListener("click", () => {
    upstream.startAutopilot(selected.title, selected.difficulty);
})

$g("@reload-btn").addEventListener('click', () => {
    for (let ele of $g("difficulty-container").querySelectorAll("a")) if (ele.id != "difficulty-listing-template") ele.remove();
    for (let ele of $g("beatmaps-container").querySelectorAll("a")) if (ele.id != "beatmap-listing-template") ele.remove();
    _("message", `Loading`);
    currentBeatmaps = [];
    currentDifficulties = [];
    selected = {
        title: "",
        variation: ""
    }
    $g("@start-btn").hidden = true;

    upstream.reloadAllSongs();

});
