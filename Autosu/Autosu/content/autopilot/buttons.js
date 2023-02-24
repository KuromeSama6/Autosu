function getEnabled(ele){
    let enableIndicator = ele.nextElementSibling.querySelector(".d-flex").querySelector(".enable-indicator");
    return !enableIndicator.hidden; 
}

// register toggle buttons
for (let button of document.querySelectorAll(".togglebtn")){
    let keywords = button.id.split(":");
    let type = keywords[1];
    let enableIndicator = button.nextElementSibling.querySelector(".d-flex").querySelector(".enable-indicator");

    button.nextElementSibling.addEventListener("click", () => {
        // report to upstream
        upstream.featureControlChange(type, !enableIndicator.hidden).then((approved) => {
            if (approved) enableIndicator.hidden = !enableIndicator.hidden;
        });

    })
}


for (let button of document.querySelectorAll(".annunciator.clickable")){
    let keyword = button.id.split(":")[1];
    button.addEventListener("click", () => {
        upstream.suppressAnnunciator(keyword);
    });
}