// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Confirmation dialog for forms with data-confirm (e.g. paying fees).
// Same behaviour as the handler in _AdminLayout, which does not load this file.
document.addEventListener("submit", function (e) {
    var msg = e.target.getAttribute && e.target.getAttribute("data-confirm");
    if (msg && !confirm(msg)) e.preventDefault();
});
