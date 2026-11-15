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

    function closeSubmenus() {
        hasSubmenuItems.forEach(function (item) {
            item.classList.remove('submenu-open');
            var label = item.querySelector('.submenu-label');
            if (label) label.setAttribute('aria-expanded', 'false');
        });
    }

    function setOpen(open) {
        dropdown.classList.toggle('open', open);
        trigger.setAttribute('aria-expanded', open ? 'true' : 'false');
        if (!open) closeSubmenus();
    }

    // Keeps the submenu inside the viewport (hover devices only)
    function positionSubmenu(item) {
        var submenu = item.querySelector('.submenu');
        if (!submenu || !isHoverCapable) return;
        submenu.style.left = '';   // reset to the CSS default before measuring
        var rect = submenu.getBoundingClientRect();
        var overflowAmount = rect.right - (window.innerWidth - 12);
        if (overflowAmount > 0) {
            submenu.style.left = 'calc(100% - ' + overflowAmount + 'px)';
        }
    }

    function openSubmenu(item) {
        closeSubmenus();
        item.classList.add('submenu-open');
        item.querySelector('.submenu-label').setAttribute('aria-expanded', 'true');
        positionSubmenu(item);
    }

    function topItems() {
        return Array.from(optionsList.children).map(function (li) {
            return li.classList.contains('has-submenu') ? li.querySelector('.submenu-label') : li;
        });
    }

    function subItems(item) {
        return Array.from(item.querySelectorAll('.submenu > li'));
    }

    function focusItem(el) { if (el) el.focus(); }

    function selectItem(li) {
        var value = li.getAttribute('data-value');
        hiddenInput.value = value;
        trigger.textContent = value;
        setOpen(false);
        trigger.focus();
        if (window.jQuery) {
            $(hiddenInput).valid();
        }
    }

    // Trigger: click, Enter and Space all arrive as a click
    trigger.addEventListener('click', function (e) {
        e.stopPropagation();
        var open = !dropdown.classList.contains('open');
        setOpen(open);
        if (open && e.detail === 0) focusItem(topItems()[0]);   // detail 0 = opened by keyboard
    });

    trigger.addEventListener('keydown', function (e) {
        if (e.key === 'ArrowDown' || e.key === 'ArrowUp') {
            e.preventDefault();
            setOpen(true);
            var items = topItems();
            focusItem(e.key === 'ArrowDown' ? items[0] : items[items.length - 1]);
        } else if (e.key === 'Escape' && dropdown.classList.contains('open')) {
            e.preventDefault();
            setOpen(false);
        }
    });

    // Click outside closes everything
    document.addEventListener('click', function () {
        setOpen(false);
    });

    // Mouse and touch
    optionsList.addEventListener('click', function (e) {
        var leaf = e.target.closest('li[data-value]');
        if (leaf) {
            e.stopPropagation();
            selectItem(leaf);
            return;
        }

        var submenuParent = e.target.closest('.has-submenu');
        if (submenuParent) {
            e.stopPropagation();
            if (isHoverCapable) return;   // hovering already shows the submenu
            if (submenuParent.classList.contains('submenu-open')) {
                closeSubmenus();
            } else {
                openSubmenu(submenuParent);
            }
        }
    });

    // Keyboard inside the menu
    optionsList.addEventListener('keydown', function (e) {
        var current = document.activeElement;
        var submenuEl = current.closest('.submenu');
        var parentItem = submenuEl ? submenuEl.closest('.has-submenu') : null;
        var list = parentItem ? subItems(parentItem) : topItems();
        var index = list.indexOf(current);
        var isLabel = current.classList.contains('submenu-label');

        if (!parentItem && ['ArrowDown', 'ArrowUp', 'Home', 'End'].indexOf(e.key) > -1) {
            closeSubmenus();
        }

        switch (e.key) {
            case 'ArrowDown':
                e.preventDefault();
                focusItem(list[(index + 1) % list.length]);
                break;
            case 'ArrowUp':
                e.preventDefault();
                focusItem(list[(index - 1 + list.length) % list.length]);
                break;
            case 'Home':
                e.preventDefault();
                focusItem(list[0]);
                break;
            case 'End':
                e.preventDefault();
                focusItem(list[list.length - 1]);
                break;
            case 'ArrowRight':
                if (isLabel) {
                    e.preventDefault();
                    openSubmenu(current.parentElement);
                    focusItem(subItems(current.parentElement)[0]);
                }
                break;
            case 'ArrowLeft':
                if (parentItem) {
                    e.preventDefault();
                    closeSubmenus();
                    focusItem(parentItem.querySelector('.submenu-label'));
                }
                break;
            case 'Enter':
            case ' ':
                e.preventDefault();
                if (isLabel) {
                    openSubmenu(current.parentElement);
                    focusItem(subItems(current.parentElement)[0]);
                } else if (current.hasAttribute('data-value')) {
                    selectItem(current);
                }
                break;
            case 'Escape':
                e.preventDefault();
                if (parentItem) {
                    closeSubmenus();
                    focusItem(parentItem.querySelector('.submenu-label'));
                } else {
                    setOpen(false);
                    trigger.focus();
                }
                break;
            case 'Tab':
                trigger.focus();   // the default Tab then moves on from the trigger
                setOpen(false);
                break;
        }
    });

    if (isHoverCapable) {
        hasSubmenuItems.forEach(function (item) {
            item.addEventListener('mouseenter', function () { positionSubmenu(item); });
        });
    }
});