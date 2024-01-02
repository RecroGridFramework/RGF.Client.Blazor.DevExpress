﻿export function setTheme(theme, appRootUrl) {
    var body = document.getElementsByTagName('body')[0];
    body.style.display = 'none';
    body.classList.add(theme);
    document.getElementById('devexpress-theme').href = `${appRootUrl}_content/DevExpress.Blazor.Themes/${theme}.min.css`;
    setTimeout(function () {
        body.style.display = '';
    }, 500);
}
