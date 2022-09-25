// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Products index

var updateInterval;
var importInfo;
var check;
var buttonImport;
var buttonUpdate;
var message;


function startUpdate(productId) {
    switchButtons(true);
    message = document.getElementById('importMessage');
    message.innerHTML = 'Update in progress <i class="fa-solid fa-spinner fa-spin"></i>';
    importInfo = document.getElementById('importInfo');
    importInfo.classList.toggle('show');
    importInfo.style.animationDuration = '1s';

    var url = window.location.origin + '/Products/AddSupplierInfo';
    var type = 'GET';
    var data = {};

    if (productId != null) {
        url = window.location.origin + '/Products/UpdateProductInfo';
        type = 'POST';
        data = { id: productId };
    }

    $.ajax({
        url: url,
        type: type,
        data: data
    }).done(function () {
        message.innerHTML = 'Done!<br />Refresh to see changes';
        switchButtons(false);
        setTimeout(function () {
            importInfo.classList.toggle('show');
        }, 3000);
    }).fail(function () {
        message.innerHTML = 'Something went wrong<br />Check your configuration';
        switchButtons(false);
        setTimeout(function () {
            importInfo.classList.toggle('show');
        }, 5000);
    });
}


function startImport() {
    switchButtons(true);
    message = document.getElementById('importMessage');
    message.innerHTML = 'Import in progress <i class="fa-solid fa-spinner fa-spin"></i>';
    importInfo = document.getElementById('importInfo');
    importInfo.classList.toggle('show');
    importInfo.style.animationDuration = '1s';
    Import();
}

function Import() {
    $.ajax({
        url: window.location.origin + '/Products/ImportProducts',
    }).done(function () {
        message.innerHTML = 'Done!';
        setTimeout(function () {
            importInfo.classList.toggle('show');
        }, 3000);

        switchButtons(false);
    }).fail(function () {
        message.innerHTML = 'Something went wrong<br />Check your configuration';
        switchButtons(false);
        setTimeout(function () {
            importInfo.classList.toggle('show');
        }, 5000);
    });
}

function switchButtons(disable) {
    buttonImport = document.getElementById('import-button');
    buttonImport.disabled = disable;
    buttonImport.classList.toggle('inactive-button');
    buttonUpdate = document.getElementById('update-button');
    buttonUpdate.disabled = disable;
    buttonUpdate.classList.toggle('inactive-button');
}

function zoomPhoto(id) {
    var popup = document.getElementById('photo-' + id);
    popup.classList.toggle('show');
}

window.updateSorting = function (e) {
    var url = window.location.href;
    var paramName = 'sortOrder';
    var pattern = new RegExp('\\b(' + paramName + '=).*?(&|#|$)');
    if (url.indexOf('?') > 0) {
        url = url.replace(pattern, paramName + '=' + e.value);
    } else {
        url += '?sortOrder=' + e.value;
    }
    window.location.href = url;
}

function clearFilters() {
    location.href = '//' + location.host + location.pathname;
}

// Products details

function indexTooltip(id) {
    var elementId = 'index';
    if (id != null) {
        elementId += '-' + id;
    }
    var popup = document.getElementById(elementId);
    popup.classList.toggle('show');
}

function copyIndex(id) {
    var elementId = 'indexValue';
    if (id != null) {
        elementId += '-' + id;
        document.getElementById('index-' + id).innerHTML = 'Copied!';
    } else {
        document.getElementById('index').innerHTML = 'Copied!';
    }
    var index = document.getElementById(elementId);
    navigator.clipboard.writeText(index.innerText);
}

function deleteProduct(productId) {
    $.ajax({
        url: window.location.origin + '/Products/Delete',
        type: 'POST',
        data: { id: productId}
    }).done(function () {
        window.location.href = document.referrer;
    })
}

// Notifications index

function startMark() {
    message = document.getElementById('importMessage');
    message.innerHTML = 'Operation in progress <i class="fa-solid fa-spinner fa-spin"></i>';
    importInfo = document.getElementById('importInfo');
    importInfo.classList.toggle('show');
    importInfo.style.animationDuration = '1s';

    $.ajax({
        url: window.location.origin + '/Notifications/MarkAllChangesAsRead',
    }).done(function() {
        message.innerHTML = 'Done!';
        setTimeout(function () {
            importInfo.classList.toggle('show');
        }, 3000);
    })
}

function readPriceNotification(changeId){
    $.ajax({
        url: window.location.origin + '/Notifications/MarkPriceChangeAsRead',
        type: 'POST',
        data: { id: changeId }
   })
    var row = document.getElementById('price-' + changeId);
    row.style.opacity = '0.4';
}

function readQuantityNotification(changeId){
    $.ajax({
        url: window.location.origin + '/Notifications/MarkQuantityChangeAsRead',
        type: 'POST',
        data: { id: changeId }
    })
    var row = document.getElementById('quantity-' + changeId);
    row.style.opacity = '0.4';
}

function showTooltip(element, id) {
    var popup = document.getElementById(element + id);
    popup.classList.toggle('show');
}
