'use strict';
// fast toggle sections
for (let btn of document.querySelectorAll("._fasttoggle")) {
    let bindName = btn.id.split(":")[1];
    let target = $g(`_fasttoggle-bind:${bindName}`);
    btn.addEventListener('click', () => {
        target.hidden = !target.hidden;
    });
}

upstream.browserReady();

'use strict';
// Code area ===============================

// recursive
function setAllElementsDisabled(element, disable){
    for (let child of element.children){
        child.disabled = disable;
        if (child.children.length > 0) setAllElementsDisabled(child, disable)
    }
}

function sendPostRequest(url, body, onSuccess=null, onFail=null){
    // load captcha

    var exec = function () {
        body._tsToken = window._tsToken;
        if (window._tsWidgetId) turnstile.reset(window._tsWidgetId);

        fetch(url, {
            method: "POST",
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8'
            },
            body: Object.entries(body).map(([k,v])=>{return k+'='+v}).join('&')

        }).then(
            (response) => response.json().then(data => {
                if (onSuccess)
                try{
                    onSuccess(JSON.parse(data));
                }catch{
                    onSuccess({});
                }

            })
        ).catch(
            (error) => {
                if (onFail != null) {
                    onFail(error)
                }
            }
        )
    }

    exec();
}

function parseDateString(date){
    try{
        const offset = new Date().getTimezoneOffset()
        var ret = new Date(new Date(date).getTime() - (offset*60*1000))
        return `${ret.toISOString().split('T')[0]} ${ret.toISOString().split('T')[1].split('.')[0]}`
    }catch{
        return date;
    }
}

function Clone (parent, element) {
    var cloned = element.cloneNode(true);
    cloned.hidden = false;
    parent.appendChild(cloned);
 
    return cloned;
}

function _(id, content){
    var element = document.getElementById(id);
    if (element != null) {
        element.innerHTML = content;
        return true;
    } else return false;
}

function _a(id, content){
    for (let element of document.getElementsByClassName(id)){
        if (element != null) {
            element.innerHTML = content;
        }
    }
}

function $g(id){
    return document.getElementById(id);
}

function $ga(id){
    return document.getElementsByClassName(id);
}

function $v(id, visible){
    var ele = $g(id);
    if (ele != null) {
        ele.hidden = !visible;
        return true;
    } else return false;
}

function clipboardWrite(content, suc, bad) {
    if (!navigator.clipboard) {
        bad();
        return;
    }

    navigator.clipboard
    .writeText(content)
    .then(
        () => {
            suc();
        }, () => {
            bad();
        }
    );
}

function generateRandomString(length = 64) {
    let characters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz';
    let randomString = '';
    for (let i = 0; i < length; i++) {
      randomString += characters.charAt(Math.floor(Math.random() * characters.length));
    }
    
    return randomString;
}

function $eq(t1, t2) {
    return JSON.stringify(t1) == JSON.stringify(t2);
}

document.body.style.webkitUserSelect = 'none';
document.body.style.userSelect = 'none';
document.body.setAttribute('spellcheck', false);
document.body.setAttribute('contentEditable', false);