// register toggle buttons
for (let button of document.querySelectorAll(".togglebtn")){
    let keywords = button.id.split(":");
    let type = keywords[1];
    let enableIndicator = button.nextElementSibling.querySelector(".d-flex").querySelector(".enable-indicator");

    button.nextElementSibling.addEventListener("click", () => {
        enableIndicator.hidden = !enableIndicator.hidden;
        // report to upstream
        upstream.featureControlChanged(type, !enableIndicator.hidden).then((res) => {
            console.log(res);
        });

    })
}