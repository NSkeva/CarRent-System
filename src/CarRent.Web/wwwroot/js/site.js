(function () {
    const locale = (navigator.language || "hr-HR").toLowerCase().startsWith("hr") ? "hr-HR" : "en-US";

    function debounce(fn, wait) {
        let t;
        return (...args) => {
            clearTimeout(t);
            t = setTimeout(() => fn(...args), wait);
        };
    }

    function initBlurValidation(root) {
        root.querySelectorAll("[data-val='true']").forEach((input) => {
            input.addEventListener("blur", () => {
                if (input.checkValidity()) {
                    input.classList.remove("input-invalid");
                    input.classList.add("input-valid");
                } else {
                    input.classList.add("input-invalid");
                    input.classList.remove("input-valid");
                }
            });
        });
    }

    function initAjaxTables() {
        document.querySelectorAll("[data-ajax-search]").forEach((input) => {
            const tableBody = document.querySelector(input.dataset.target);
            const url = input.dataset.ajaxSearch;
            if (!tableBody || !url) return;

            const run = debounce(async () => {
                const q = input.value.trim();
                const res = await fetch(`${url}?q=${encodeURIComponent(q)}`);
                if (!res.ok) return;
                tableBody.innerHTML = await res.text();
                tableBody.classList.add("fade-in");
                setTimeout(() => tableBody.classList.remove("fade-in"), 350);
            }, 250);

            input.addEventListener("input", run);
        });
    }

    function initAutocomplete(root) {
        root.querySelectorAll("[data-autocomplete]").forEach((wrap) => {
            const input = wrap.querySelector("[data-autocomplete-input]");
            const hidden = wrap.querySelector("[data-autocomplete-id]");
            const list = wrap.querySelector("[data-autocomplete-list]");
            const url = wrap.dataset.autocomplete;
            if (!input || !hidden || !list || !url) return;

            const setOpen = (open) => {
                list.classList.toggle("open", open);
                wrap.classList.toggle("is-open", open);
            };

            const render = (items) => {
                list.innerHTML = "";
                items.forEach((item) => {
                    const li = document.createElement("li");
                    li.textContent = item.label;
                    li.dataset.id = item.id;
                    li.addEventListener("click", () => {
                        hidden.value = item.id;
                        input.value = item.label;
                        setOpen(false);
                    });
                    list.appendChild(li);
                });
                setOpen(items.length > 0);
            };

            const search = debounce(async () => {
                const q = input.value.trim();
                const res = await fetch(`${url}?q=${encodeURIComponent(q)}`);
                if (!res.ok) return;
                render(await res.json());
            }, 220);

            input.addEventListener("input", search);
            input.addEventListener("focus", search);
            document.addEventListener("click", (e) => {
                if (!wrap.contains(e.target)) setOpen(false);
            });
        });
    }

    function parseDisplayDate(value) {
        if (!value) return null;
        const parts = value.split(/[.\s:/-]/).filter(Boolean);
        if (parts.length < 3) return null;
        const d = parseInt(parts[0], 10);
        const m = parseInt(parts[1], 10) - 1;
        const y = parseInt(parts[2], 10);
        const hh = parts.length > 3 ? parseInt(parts[3], 10) : 0;
        const mm = parts.length > 4 ? parseInt(parts[4], 10) : 0;
        return new Date(y, m, d, hh, mm);
    }

    function formatDisplayDate(date) {
        const pad = (n) => String(n).padStart(2, "0");
        if (locale.startsWith("hr")) {
            return `${pad(date.getDate())}.${pad(date.getMonth() + 1)}.${date.getFullYear()} ${pad(date.getHours())}:${pad(date.getMinutes())}`;
        }
        return `${pad(date.getMonth() + 1)}/${pad(date.getDate())}/${date.getFullYear()} ${pad(date.getHours())}:${pad(date.getMinutes())}`;
    }

    function initDateTimePickers(root) {
        root.querySelectorAll("[data-datetime-picker]").forEach((wrap) => {
            const display = wrap.querySelector("[data-datetime-display]");
            const hidden = wrap.querySelector("[data-datetime-hidden]");
            const cal = wrap.querySelector("[data-datetime-calendar]");
            if (!display || !hidden || !cal) return;

            const selected = hidden.value ? new Date(hidden.value) : new Date();
            let view = new Date(selected.getFullYear(), selected.getMonth(), 1);

            const syncHidden = (dt) => {
                hidden.value = dt.toISOString();
                display.value = formatDisplayDate(dt);
            };

            if (hidden.value) display.value = formatDisplayDate(new Date(hidden.value));

            const draw = () => {
                cal.innerHTML = "";
                const head = document.createElement("div");
                head.className = "dt-head";
                head.textContent = view.toLocaleString(locale, { month: "long", year: "numeric" });
                cal.appendChild(head);

                const grid = document.createElement("div");
                grid.className = "dt-grid";
                for (let d = 1; d <= 31; d++) {
                    const cell = document.createElement("button");
                    cell.type = "button";
                    cell.textContent = String(d);
                    cell.addEventListener("click", () => {
                        const dt = new Date(view.getFullYear(), view.getMonth(), d, selected.getHours(), selected.getMinutes());
                        syncHidden(dt);
                        setCalOpen(false);
                    });
                    grid.appendChild(cell);
                }
                cal.appendChild(grid);

                const timeRow = document.createElement("div");
                timeRow.className = "dt-time";
                timeRow.innerHTML = `<label>Sat</label><input type="number" min="0" max="23" value="${selected.getHours()}" data-dt-hour />
                                   <label>Min</label><input type="number" min="0" max="59" value="${selected.getMinutes()}" data-dt-minute />`;
                cal.appendChild(timeRow);

                timeRow.querySelector("[data-dt-hour]").addEventListener("change", (e) => {
                    selected.setHours(parseInt(e.target.value, 10) || 0);
                    syncHidden(selected);
                });
                timeRow.querySelector("[data-dt-minute]").addEventListener("change", (e) => {
                    selected.setMinutes(parseInt(e.target.value, 10) || 0);
                    syncHidden(selected);
                });
            };

            const setCalOpen = (open) => {
                cal.classList.toggle("open", open);
                wrap.classList.toggle("is-open", open);
            };

            display.addEventListener("click", (e) => {
                e.stopPropagation();
                const willOpen = !cal.classList.contains("open");
                setCalOpen(willOpen);
                if (willOpen) draw();
            });

            display.addEventListener("blur", () => {
                const parsed = parseDisplayDate(display.value);
                if (parsed) syncHidden(parsed);
            });

            document.addEventListener("click", (e) => {
                if (!wrap.contains(e.target)) setCalOpen(false);
            });
        });
    }

    function initCardAnimations() {
        document.querySelectorAll(".glass-card, .fleet-card, .panel").forEach((el, i) => {
            el.style.animationDelay = `${i * 40}ms`;
            el.classList.add("rise-in");
        });
    }

    function initGlobalSearch() {
        const modal = document.querySelector("[data-global-search-modal]");
        const backdrop = document.querySelector("[data-global-search-backdrop]");
        const input = document.querySelector("[data-global-search-input]");
        const results = document.querySelector("[data-global-search-results]");
        const openBtns = document.querySelectorAll("[data-global-search-open]");
        if (!modal || !input || !results) return;

        let items = [];
        let activeIndex = -1;
        let debounceTimer;

        const setOpen = (open) => {
            modal.hidden = !open;
            if (backdrop) backdrop.hidden = !open;
            document.body.classList.toggle("global-search-open", open);
            if (open) {
                input.value = "";
                results.innerHTML = "";
                items = [];
                activeIndex = -1;
                setTimeout(() => input.focus(), 0);
                fetchResults("");
            }
        };

        const render = (list) => {
            items = list;
            activeIndex = list.length ? 0 : -1;
            results.innerHTML = "";
            list.forEach((item, idx) => {
                const a = document.createElement("a");
                a.href = item.url;
                a.className = "global-search-item" + (idx === activeIndex ? " active" : "");
                a.innerHTML = `<span class="gs-type">${item.type}</span><strong>${item.title}</strong><span class="gs-sub">${item.subtitle || ""}</span>`;
                a.addEventListener("mouseenter", () => {
                    activeIndex = idx;
                    results.querySelectorAll(".global-search-item").forEach((el, i) => el.classList.toggle("active", i === idx));
                });
                results.appendChild(a);
            });
        };

        const fetchResults = async (q) => {
            const res = await fetch(`/api/search?q=${encodeURIComponent(q)}`);
            if (!res.ok) return;
            render(await res.json());
        };

        const scheduleSearch = () => {
            clearTimeout(debounceTimer);
            debounceTimer = setTimeout(() => fetchResults(input.value.trim()), 200);
        };

        openBtns.forEach((btn) => btn.addEventListener("click", () => setOpen(true)));
        backdrop?.addEventListener("click", () => setOpen(false));
        input.addEventListener("input", scheduleSearch);
        input.addEventListener("keydown", (e) => {
            if (e.key === "Escape") { setOpen(false); return; }
            if (!items.length) return;
            if (e.key === "ArrowDown") {
                e.preventDefault();
                activeIndex = Math.min(items.length - 1, activeIndex + 1);
            } else if (e.key === "ArrowUp") {
                e.preventDefault();
                activeIndex = Math.max(0, activeIndex - 1);
            } else if (e.key === "Enter" && activeIndex >= 0) {
                e.preventDefault();
                window.location.href = items[activeIndex].url;
                return;
            }
            results.querySelectorAll(".global-search-item").forEach((el, i) => el.classList.toggle("active", i === activeIndex));
        });

        document.addEventListener("keydown", (e) => {
            if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === "k") {
                if (!openBtns.length) return;
                e.preventDefault();
                setOpen(true);
            }
        });
    }

    function initMobileNav() {
        const toggle = document.querySelector("[data-mobile-nav-toggle]");
        const nav = document.getElementById("siteNav");
        if (!toggle || !nav) return;

        toggle.addEventListener("click", (e) => {
            e.stopPropagation();
            nav.classList.toggle("mobile-open");
            document.body.classList.toggle("mobile-nav-open");
        });

        document.addEventListener("click", (e) => {
            if (!e.target.closest("#siteNav") && !e.target.closest("[data-mobile-nav-toggle]")) {
                nav.classList.remove("mobile-open");
                document.body.classList.remove("mobile-nav-open");
            }
        });
    }

    function initNavDropdowns() {
        const closeDelayMs = 420;

        document.querySelectorAll(".nav-group").forEach((group) => {
            let closeTimer;

            const open = () => {
                clearTimeout(closeTimer);
                document.querySelectorAll(".nav-group").forEach((g) => {
                    if (g !== group) g.classList.remove("open");
                });
                group.classList.add("open");
            };

            const scheduleClose = () => {
                clearTimeout(closeTimer);
                closeTimer = setTimeout(() => group.classList.remove("open"), closeDelayMs);
            };

            group.addEventListener("mouseenter", open);
            group.addEventListener("mouseleave", scheduleClose);
            group.addEventListener("focusin", open);
            group.addEventListener("focusout", (e) => {
                if (!group.contains(e.relatedTarget)) scheduleClose();
            });
        });

        document.querySelectorAll(".nav-group-btn").forEach((btn) => {
            btn.addEventListener("click", (e) => {
                e.stopPropagation();
                const group = btn.closest(".nav-group");
                if (!group) return;
                const isOpen = group.classList.contains("open");
                document.querySelectorAll(".nav-group").forEach((g) => g.classList.remove("open"));
                if (!isOpen) group.classList.add("open");
            });
        });

        document.addEventListener("click", (e) => {
            if (!e.target.closest(".nav-group")) {
                document.querySelectorAll(".nav-group").forEach((g) => g.classList.remove("open"));
            }
        });
    }

    document.addEventListener("DOMContentLoaded", () => {
        initBlurValidation(document);
        initAjaxTables();
        initAutocomplete(document);
        initDateTimePickers(document);
        initCardAnimations();
        initGlobalSearch();
        initMobileNav();
        initNavDropdowns();
    });
})();
