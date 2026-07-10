(() => {
    'use strict';

    function initModalUi(root) {
        if (window.CarRentUi) {
            window.CarRentUi.initBlurValidation?.(root);
            window.CarRentUi.initAutocomplete?.(root);
            window.CarRentUi.initDateTimePickers?.(root);
        }
        const form = root.querySelector('[data-timeline-modal-form]');
        if (form && window.jQuery?.validator?.unobtrusive) {
            window.jQuery.validator.unobtrusive.parse(form);
        }
    }

    class TimelineModal {
        constructor(shell) {
            this.shell = shell;
            this.backdrop = shell.querySelector('[data-tl-modal-backdrop]');
            this.panel = shell.querySelector('[data-tl-modal-panel]');
            this.titleEl = shell.querySelector('[data-tl-modal-title]');
            this.body = shell.querySelector('[data-tl-modal-body]');
            this.backBtn = shell.querySelector('[data-tl-modal-back]');
            this.closeBtn = shell.querySelector('[data-tl-modal-close]');
            this.stack = [];
            this.onReload = null;

            this.backBtn?.addEventListener('click', () => this.back());
            this.closeBtn?.addEventListener('click', () => this.close());
            this.backdrop?.addEventListener('click', (e) => {
                if (e.target === this.backdrop) this.close();
            });

            this.body?.addEventListener('click', (e) => {
                const editBtn = e.target.closest('[data-tl-modal-edit]');
                if (editBtn) {
                    e.preventDefault();
                    this.openEdit(parseInt(editBtn.dataset.tlModalEdit, 10));
                    return;
                }
                if (e.target.closest('[data-tl-modal-close]')) {
                    e.preventDefault();
                    this.close();
                }
                if (e.target.closest('[data-tl-modal-cancel]')) {
                    e.preventDefault();
                    if (this.stack.length > 1) this.back();
                    else this.close();
                }
            });

            this.body?.addEventListener('submit', (e) => {
                const form = e.target.closest('[data-timeline-modal-form]');
                if (!form) return;
                e.preventDefault();
                this.submitForm(form);
            });

            document.addEventListener('keydown', (e) => {
                if (e.key === 'Escape' && !this.shell.hidden) this.close();
            });
        }

        setReloadHandler(fn) {
            this.onReload = fn;
        }

        titles = {
            create: 'Nova rezervacija',
            details: 'Detalji rezervacije',
            edit: 'Uredi rezervaciju'
        };

        async openCreate(params = {}) {
            const qs = new URLSearchParams();
            if (params.vehicleId) qs.set('vehicleId', String(params.vehicleId));
            if (params.startDate) qs.set('startDate', params.startDate);
            if (params.endDate) qs.set('endDate', params.endDate);
            const q = qs.toString();
            await this.push('create', `/raspored/mjesecni/modal/rezervacija/nova${q ? `?${q}` : ''}`);
        }

        async openDetails(id) {
            await this.push('details', `/raspored/mjesecni/modal/rezervacija/${id}`);
        }

        async openEdit(id) {
            const top = this.stack[this.stack.length - 1];
            if (top?.view !== 'details' || top.id !== id) {
                this.stack = [{ view: 'details', url: `/raspored/mjesecni/modal/rezervacija/${id}`, id }];
            }
            await this.push('edit', `/raspored/mjesecni/modal/rezervacija/${id}/uredi`, id);
        }

        async push(view, url, id = null) {
            this.stack.push({ view, url, id });
            await this.render();
        }

        async back() {
            if (this.stack.length <= 1) {
                this.close();
                return;
            }
            this.stack.pop();
            await this.render();
        }

        close() {
            this.stack = [];
            this.shell.hidden = true;
            document.body.classList.remove('tl-modal-open');
            this.body.innerHTML = '';
        }

        isOpen() {
            return !this.shell.hidden;
        }

        async render() {
            const current = this.stack[this.stack.length - 1];
            if (!current) return;

            this.shell.hidden = false;
            document.body.classList.add('tl-modal-open');
            this.titleEl.textContent = this.titles[current.view] ?? 'Rezervacija';
            this.backBtn.hidden = this.stack.length <= 1;
            this.body.innerHTML = '<div class="tl-modal-loading">Učitavanje…</div>';

            try {
                const res = await fetch(current.url, { credentials: 'same-origin' });
                if (!res.ok) throw new Error('load failed');
                this.body.innerHTML = await res.text();
                initModalUi(this.body);
            } catch {
                this.body.innerHTML = '<p class="tl-modal-error">Nije moguće učitati sadržaj.</p>';
            }
        }

        async submitForm(form) {
            const submitBtn = form.querySelector('[type="submit"]');
            submitBtn?.setAttribute('disabled', 'disabled');
            try {
                const body = new FormData(form);
                const res = await fetch(form.action, {
                    method: 'POST',
                    credentials: 'same-origin',
                    body
                });

                const contentType = res.headers.get('content-type') ?? '';
                if (contentType.includes('application/json')) {
                    const data = await res.json();
                    if (data.success) {
                        this.close();
                        if (this.onReload) this.onReload();
                        else window.location.reload();
                    }
                    return;
                }

                this.body.innerHTML = await res.text();
                initModalUi(this.body);
            } catch {
                this.body.insertAdjacentHTML('afterbegin', '<p class="tl-modal-error">Spremanje nije uspjelo.</p>');
            } finally {
                submitBtn?.removeAttribute('disabled');
            }
        }
    }

    window.TimelineModal = TimelineModal;

    document.addEventListener('DOMContentLoaded', () => {
        const shell = document.querySelector('[data-timeline-modal]');
        if (!shell) return;
        const modal = new TimelineModal(shell);
        window.__timelineModal = modal;

        document.querySelector('[data-tl-open-create]')?.addEventListener('click', () => modal.openCreate());

        document.querySelectorAll('[data-timeline-scheduler]').forEach((root) => {
            modal.setReloadHandler(() => window.location.reload());
            if (typeof window.TimelineScheduler === 'function') {
                new window.TimelineScheduler(root, modal);
            }
        });
    });
})();
