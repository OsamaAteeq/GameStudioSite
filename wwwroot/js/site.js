// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener('DOMContentLoaded', function () {
    var form = document.querySelector('.contact-form');
    if (!form) return;

    form.addEventListener('submit', function (e) {
        // jQuery Validation adds this helper; if the form's invalid, do nothing and let it show errors instead.
        if (window.jQuery && !$(form).valid()) {
            return;
        }

        var btn = document.getElementById('contact-submit-btn');
        var btnText = document.getElementById('btn-text');
        var btnSpinner = document.getElementById('btn-spinner');

        btnText.textContent = 'Sending...';
        btnSpinner.style.display = 'inline-block';
        btn.disabled = true;
    });
});

document.addEventListener('DOMContentLoaded', function () {
    var dropdown = document.getElementById('subject-dropdown');
    if (!dropdown) return;

    var trigger = document.getElementById('subject-trigger');
    var hiddenInput = document.getElementById('subject-hidden-input');
    var optionsList = dropdown.querySelector('.nested-select-options');
    var hasSubmenuItems = dropdown.querySelectorAll('.has-submenu');
    var isHoverCapable = window.matchMedia('(hover: hover) and (pointer: fine)').matches;

    trigger.addEventListener('click', function (e) {
        e.stopPropagation();
        dropdown.classList.toggle('open');
    });

    document.addEventListener('click', function () {
        dropdown.classList.remove('open');
        hasSubmenuItems.forEach(function (item) {
            item.classList.remove('submenu-open');
        });
    });

    optionsList.addEventListener('click', function (e) {
        var leaf = e.target.closest('li[data-value]');

        if (leaf) {
            e.stopPropagation();
            var value = leaf.getAttribute('data-value');
            hiddenInput.value = value;
            trigger.textContent = value;
            dropdown.classList.remove('open');
            hasSubmenuItems.forEach(function (i) {
                i.classList.remove('submenu-open');
            });
            if (window.jQuery) {
                $(hiddenInput).valid();
            }
            return;
        }

        var submenuParent = e.target.closest('.has-submenu');
        if (submenuParent) {
            e.stopPropagation();
            var wasOpen = submenuParent.classList.contains('submenu-open');
            hasSubmenuItems.forEach(function (i) {
                i.classList.remove('submenu-open');
            });
            if (!wasOpen) {
                submenuParent.classList.add('submenu-open');
            }
        }
    });

    if (isHoverCapable) {
        hasSubmenuItems.forEach(function (item) {
            var submenu = item.querySelector('.submenu');
            if (!submenu) return;

            item.addEventListener('mouseenter', function () {
                submenu.style.left = ''; // reset to CSS default (left: 100%) before measuring

                var rect = submenu.getBoundingClientRect();
                var margin = 12; // small gap from the viewport edge, purely aesthetic
                var overflowAmount = rect.right - (window.innerWidth - margin);

                if (overflowAmount > 0) {
                    // Pull the submenu left by exactly however much it overflowed,
                    // so its right edge lands just inside the viewport — it stays
                    // anchored on the "right side" of the item, just shifted inward
                    // enough to overlap the parent dropdown instead of clipping off-screen.
                    submenu.style.left = 'calc(100% - ' + overflowAmount + 'px)';
                }
            });
        });
    }
});