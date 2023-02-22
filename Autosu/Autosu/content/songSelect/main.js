currentBeatmaps = [];
selected = {
    title: "",
    variation: ""
}

function selectFiles(data){
    data = JSON.parse(data);

    _("$current-beatmaps-path", data.path.replaceAll("\\\\", "\\"));
    currentBeatmaps = data.beatmaps;
    renderSongs("");
}

function renderSongs(filter){
    // get the candidates
    var candidates = [];
    for (let name in currentBeatmaps) {
        if (filter == "" || name.toLocaleLowerCase().includes(filter.toLocaleLowerCase())) candidates.push(name);
    }

    _("message", `Found <b>${Object.keys(currentBeatmaps).length}</b> Titles, Showing <b>${candidates.length}</b> Title(s)${filter == "" ? "" : ` that includes <code>${filter}</code>`}.`);

    // render the song list
    for (let ele of $g("beatmaps-container").querySelectorAll("a")) if (ele.id != "beatmap-listing-template") ele.remove();
    for (let title of candidates) {
        let ele = Clone($g("beatmaps-container"), $g("beatmap-listing-template"));
        ele.hidden = false;
        ele.id = "";
        let children = ele.querySelectorAll("span");

        children[0].innerHTML = title;
        children[1].innerHTML = `<span class="badge bg-primary rounded-pill" style="float:right">${currentBeatmaps[title].length}</span>`;

        ele.href = `javascript:renderDifficulties("${title}");`;

    }

}

function renderDifficulties(title){
    const startBtn = $g("@start-btn");

    // render the difficulties
    for (let ele of $g("difficulty-container").querySelectorAll("a")) if (ele.id != "difficulty-listing-template") ele.remove();
    for (let difficulty of currentBeatmaps[title]) {
        let ele = Clone($g("difficulty-container"), $g("difficulty-listing-template"));
        ele.hidden = false;
        ele.id = "";
        ele.innerHTML = difficulty;

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

function selectBeatmap() {

}

$g("!filedir-input").addEventListener("change", () => {

    upstream.selectBeatmapDirectory($g("!filedir-input").value).then((response) => {
        if (response != null) selectFiles(response);
    });
});
$g("!name-filter").addEventListener("keyup", () => {
    renderSongs($g("!name-filter").value);
})