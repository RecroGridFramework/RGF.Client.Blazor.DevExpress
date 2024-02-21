/*!
* recrovit-rgf-blazor-devexpress.js v1.0.1
*/

window.Recrovit = window.Recrovit || {};
window.Recrovit.RGF = window.Recrovit.RGF || {};
window.Recrovit.RGF.Blazor = window.Recrovit.RGF.Blazor || {};
var Blazor = window.Recrovit.RGF.Blazor;

Blazor.DevExpress = {
    Grid: {
        initializeTable: function (dotNetObj, entityDomId) {
            var entity = document.querySelector(`#${entityDomId}`);
            var table = entity.querySelector('table.dxbl-grid-table');
            table.classList.add('recro-grid');
        }
    }
};