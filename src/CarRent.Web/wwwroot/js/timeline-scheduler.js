(() => {
    'use strict';

    const HALF_CELL = 22;
    const SLOTS_PER_DAY = 2;
    const DRAG_THRESHOLD = 4;

    const pad2 = (n) => String(n).padStart(2, '0');

    function parseIsoDate(iso) {
        const [y, m, d] = iso.split('-').map(Number);
        return new Date(y, m - 1, d);
    }

    function toIsoDate(date) {
        return `${date.getFullYear()}-${pad2(date.getMonth() + 1)}-${pad2(date.getDate())}`;
    }

    function toDayNumber(date) {
        return Math.floor(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate()) / 86400000);
    }

    function fromDayNumber(dayNumber) {
        const utc = new Date(dayNumber * 86400000);
        return new Date(utc.getUTCFullYear(), utc.getUTCMonth(), utc.getUTCDate());
    }

    function occupiedGlobalSlots(startDate, endDate) {
        const s = toDayNumber(startDate);
        const e = toDayNumber(endDate);
        if (e < s) return null;
        if (s === e) return [s * SLOTS_PER_DAY, s * SLOTS_PER_DAY + 1];
        return [s * SLOTS_PER_DAY + 1, e * SLOTS_PER_DAY];
    }

    function datesFromGlobalSlots(g0, g1) {
        let startDayNum;
        let endDayNum;
        if (g1 - g0 === 1 && g0 % 2 === 0) {
            startDayNum = g0 / 2;
            endDayNum = startDayNum;
        } else {
            startDayNum = (g0 - 1) / 2;
            endDayNum = g1 / 2;
        }
        return { start: fromDayNumber(startDayNum), end: fromDayNumber(endDayNum) };
    }

    function rangesOverlapGlobal(a0, a1, b0, b1) {
        return a0 <= b1 && b0 <= a1;
    }


    function monthBounds(monthIso) {
        const start = parseIsoDate(monthIso);
        const end = new Date(start.getFullYear(), start.getMonth() + 1, 0);
        return { start, end };
    }

    function visibleLayout(startIso, endIso, monthStart, monthEnd) {
        const start = parseIsoDate(startIso);
        const end = parseIsoDate(endIso);
        const occupied = occupiedGlobalSlots(start, end);
        if (!occupied) return null;
        const [g0, g1] = occupied;
        const m0 = toDayNumber(monthStart) * SLOTS_PER_DAY;
        const m1 = toDayNumber(monthEnd) * SLOTS_PER_DAY + 1;
        if (g1 < m0 || g0 > m1) return null;
        const visStart = Math.max(g0, m0);
        const visEnd = Math.min(g1, m1);
        return {
            startSlot: visStart - m0 + 1,
            spanSlots: visEnd - visStart + 1
        };
    }

    function ensureToastContainer() {
        let el = document.querySelector('.toast-container');
        if (!el) {
            el = document.createElement('div');
            el.className = 'toast-container';
            document.body.appendChild(el);
        }
        return el;
    }

    function showToast(title, message, type = 'success') {
        const container = ensureToastContainer();
        const toast = document.createElement('div');
        toast.className = `toast ${type}`;
        toast.innerHTML = `
            <div class="toast-icon">${type === 'success' ? '✓' : type === 'error' ? '!' : '⚠'}</div>
            <div class="toast-body">
                <div class="toast-title">${title}</div>
                ${message ? `<div class="toast-msg">${message}</div>` : ''}
            </div>
            <button type="button" class="toast-close" aria-label="Zatvori">×</button>`;
        const close = () => toast.remove();
        toast.querySelector('.toast-close')?.addEventListener('click', close);
        container.appendChild(toast);
        setTimeout(close, 4500);
    }

    class TimelineScheduler {
        constructor(root, modal = null) {
            this.root = root;
            this.modal = modal ?? window.__timelineModal ?? null;
            this.canEdit = root.dataset.canEdit === 'true';
            this.monthIso = root.dataset.month;
            const bounds = monthBounds(this.monthIso);
            this.monthStart = bounds.start;
            this.monthEnd = bounds.end;
            this.dayCount = parseInt(root.dataset.dayCount, 10);
            this.slotCount = parseInt(root.dataset.slotCount, 10) || this.dayCount * SLOTS_PER_DAY;
            this.monthSlotStart = toDayNumber(this.monthStart) * SLOTS_PER_DAY;
            this.createUrl = root.dataset.createUrl || '/Reservation/Create';
            this.returnUrl = root.dataset.returnUrl || window.location.pathname + window.location.search;

            this.mode = null;
            this.activeEvent = null;
            this.pointerId = null;
            this.origin = null;
            this.ghost = null;
            this.selectionEl = root.querySelector('.tl-create-selection');

            this.onPointerMove = this.onPointerMove.bind(this);
            this.onPointerUp = this.onPointerUp.bind(this);

            this.bind();
        }

        bind() {
            this.root.querySelectorAll('.tl-event[data-reservation-id]').forEach((el) => {
                el.addEventListener('click', (e) => this.onEventClick(e, el));
                el.addEventListener('dblclick', (e) => this.onEventDblClick(e, el));
            });

            if (this.canEdit) {
                this.root.querySelectorAll('.tl-event[data-editable="true"]').forEach((el) => {
                    el.addEventListener('pointerdown', (e) => this.onEventPointerDown(e, el));
                });
                this.root.querySelectorAll('.tl-track').forEach((track) => {
                    track.addEventListener('pointerdown', (e) => this.onTrackPointerDown(e, track));
                });
            }

            document.addEventListener('keydown', (e) => {
                if (e.key === 'Escape' && !this.modal?.isOpen?.()) this.clearSelection();
            });
        }

        getTrackFromPoint(x, y) {
            const el = document.elementFromPoint(x, y);
            return el?.closest('.tl-track') ?? null;
        }

        getRowFromTrack(track) {
            return track?.closest('.tl-row') ?? null;
        }

        getVehicleIdFromRow(row) {
            return row ? parseInt(row.dataset.vehicleId, 10) : null;
        }

        slotFromTrackX(track, clientX) {
            const rect = track.getBoundingClientRect();
            const x = Math.max(0, Math.min(rect.width - 1, clientX - rect.left));
            const slot = Math.floor(x / HALF_CELL) + 1;
            return Math.max(1, Math.min(this.slotCount, slot));
        }

        globalSlotsFromVisible(startSlot, spanSlots) {
            const g0 = this.monthSlotStart + startSlot - 1;
            return [g0, g0 + spanSlots - 1];
        }

        datesFromVisibleLayout(startSlot, spanSlots) {
            const [g0, g1] = this.globalSlotsFromVisible(startSlot, spanSlots);
            return datesFromGlobalSlots(g0, g1);
        }

        dateFromDay(day) {
            return new Date(this.monthStart.getFullYear(), this.monthStart.getMonth(), day);
        }

        applyLayout(el, startSlot, spanSlots) {
            el.style.setProperty('--s', String(startSlot));
            el.style.setProperty('--n', String(spanSlots));
        }

        readEventDates(el) {
            return {
                start: el.dataset.start,
                end: el.dataset.end,
                vehicleId: parseInt(el.dataset.vehicleId, 10),
                reservationId: parseInt(el.dataset.reservationId, 10)
            };
        }

        collectEventsOnVehicle(vehicleId, excludeId = 0) {
            return [...this.root.querySelectorAll('.tl-event[data-reservation-id]')]
                .filter((el) => parseInt(el.dataset.vehicleId, 10) === vehicleId)
                .filter((el) => parseInt(el.dataset.reservationId, 10) !== excludeId)
                .map((el) => ({
                    id: parseInt(el.dataset.reservationId, 10),
                    start: parseIsoDate(el.dataset.start),
                    end: parseIsoDate(el.dataset.end)
                }));
        }

        hasConflict(vehicleId, start, end, excludeId = 0) {
            const occupied = occupiedGlobalSlots(start, end);
            if (!occupied) return false;
            const [a0, a1] = occupied;
            const others = this.collectEventsOnVehicle(vehicleId, excludeId);
            return others.some((o) => {
                const other = occupiedGlobalSlots(o.start, o.end);
                if (!other) return false;
                return rangesOverlapGlobal(a0, a1, other[0], other[1]);
            });
        }

        setGhostConflict(conflict) {
            if (this.ghost) this.ghost.classList.toggle('is-conflict', conflict);
        }

        highlightDropRow(track) {
            this.root.querySelectorAll('.tl-row.is-drop-target').forEach((r) => r.classList.remove('is-drop-target'));
            const row = this.getRowFromTrack(track);
            row?.classList.add('is-drop-target');
        }

        clearDropHighlight() {
            this.root.querySelectorAll('.tl-row.is-drop-target').forEach((r) => r.classList.remove('is-drop-target'));
        }

        onEventClick(e, el) {
            if (this.mode) return;
            if (e.target.closest('.tl-resize-handle')) return;
            e.preventDefault();
            this.clearSelection();
            el.classList.add('is-selected');
            const id = parseInt(el.dataset.reservationId, 10);
            if (id && this.modal) this.modal.openDetails(id);
        }

        onEventDblClick(e, el) {
            e.preventDefault();
            const id = parseInt(el.dataset.reservationId, 10);
            if (id && this.modal) this.modal.openDetails(id);
        }

        clearSelection() {
            this.root.querySelectorAll('.tl-event.is-selected').forEach((n) => n.classList.remove('is-selected'));
        }

        onEventPointerDown(e, el) {
            if (!this.canEdit || el.dataset.editable !== 'true') return;
            const handle = e.target.closest('.tl-resize-handle');
            if (handle) {
                e.preventDefault();
                this.beginResize(e, el, handle.dataset.handle);
                return;
            }
            if (e.button !== 0) return;
            e.preventDefault();
            this.beginMove(e, el);
        }

        beginMove(e, el) {
            const dates = this.readEventDates(el);
            const layout = visibleLayout(dates.start, dates.end, this.monthStart, this.monthEnd);
            this.mode = 'move';
            this.activeEvent = el;
            this.pointerId = e.pointerId;
            this.origin = {
                x: e.clientX,
                y: e.clientY,
                track: el.closest('.tl-track'),
                startSlot: layout?.startSlot ?? parseInt(el.style.getPropertyValue('--s'), 10),
                spanSlots: layout?.spanSlots ?? parseInt(el.style.getPropertyValue('--n'), 10),
                dates
            };
            el.setPointerCapture(e.pointerId);
            el.classList.add('is-dragging');
            document.body.classList.add('tl-interacting');
            document.addEventListener('pointermove', this.onPointerMove);
            document.addEventListener('pointerup', this.onPointerUp);
            document.addEventListener('pointercancel', this.onPointerUp);
        }

        beginResize(e, el, handle) {
            const dates = this.readEventDates(el);
            const layout = visibleLayout(dates.start, dates.end, this.monthStart, this.monthEnd);
            this.mode = handle === 'w' ? 'resize-w' : 'resize-e';
            this.activeEvent = el;
            this.pointerId = e.pointerId;
            this.origin = {
                track: el.closest('.tl-track'),
                startSlot: layout?.startSlot ?? parseInt(el.style.getPropertyValue('--s'), 10),
                spanSlots: layout?.spanSlots ?? parseInt(el.style.getPropertyValue('--n'), 10),
                dates
            };
            el.setPointerCapture(e.pointerId);
            el.classList.add('is-resizing');
            document.body.classList.add('tl-interacting');
            document.addEventListener('pointermove', this.onPointerMove);
            document.addEventListener('pointerup', this.onPointerUp);
            document.addEventListener('pointercancel', this.onPointerUp);
        }

        onTrackPointerDown(e, track) {
            if (e.button !== 0 || e.target.closest('.tl-event')) return;
            this.clearSelection();
            this.mode = 'create';
            this.pointerId = e.pointerId;
            const slot = this.slotFromTrackX(track, e.clientX);
            this.origin = { track, slot, vehicleId: this.getVehicleIdFromRow(this.getRowFromTrack(track)) };
            track.setPointerCapture(e.pointerId);
            this.showSelection(track, slot, slot);
            document.body.classList.add('tl-interacting');
            document.addEventListener('pointermove', this.onPointerMove);
            document.addEventListener('pointerup', this.onPointerUp);
            document.addEventListener('pointercancel', this.onPointerUp);
        }

        showSelection(track, fromSlot, toSlot) {
            if (!this.selectionEl) return;
            const startSlot = Math.min(fromSlot, toSlot);
            const spanSlots = Math.abs(toSlot - fromSlot) + 1;
            const trackRect = track.getBoundingClientRect();
            const outerRect = this.root.getBoundingClientRect();
            this.selectionEl.hidden = false;
            this.selectionEl.style.left = `${trackRect.left - outerRect.left + (startSlot - 1) * HALF_CELL + 2}px`;
            this.selectionEl.style.top = `${trackRect.top - outerRect.top + 15}px`;
            this.selectionEl.style.width = `${spanSlots * HALF_CELL - 4}px`;
        }

        hideSelection() {
            if (this.selectionEl) this.selectionEl.hidden = true;
        }

        onPointerMove(e) {
            if (!this.mode || e.pointerId !== this.pointerId) return;

            if (this.mode === 'move') {
                const dx = e.clientX - this.origin.x;
                const dy = e.clientY - this.origin.y;
                if (!this.ghost && Math.hypot(dx, dy) < DRAG_THRESHOLD) return;

                if (!this.ghost) {
                    this.ghost = this.activeEvent.cloneNode(true);
                    this.ghost.classList.add('tl-event-ghost');
                    this.ghost.querySelectorAll('.tl-resize-handle').forEach((h) => h.remove());
                    this.root.appendChild(this.ghost);
                }

                const track = this.getTrackFromPoint(e.clientX, e.clientY) ?? this.origin.track;
                this.highlightDropRow(track);
                const slot = this.slotFromTrackX(track, e.clientX);
                const startSlot = Math.max(1, Math.min(this.slotCount - this.origin.spanSlots + 1, slot));
                this.applyLayout(this.ghost, startSlot, this.origin.spanSlots);

                const outerRect = this.root.getBoundingClientRect();
                const trackRect = track.getBoundingClientRect();
                this.ghost.style.left = `${trackRect.left - outerRect.left + (startSlot - 1) * HALF_CELL + 2}px`;
                this.ghost.style.top = `${trackRect.top - outerRect.top + 15}px`;
                this.ghost.style.width = `calc(${this.origin.spanSlots} * ${HALF_CELL}px - 4px)`;

                const vehicleId = this.getVehicleIdFromRow(this.getRowFromTrack(track));
                const { start: newStart, end: newEnd } = this.datesFromVisibleLayout(startSlot, this.origin.spanSlots);
                this.setGhostConflict(this.hasConflict(vehicleId, newStart, newEnd, this.origin.dates.reservationId));
                return;
            }

            if (this.mode === 'resize-w' || this.mode === 'resize-e') {
                const track = this.origin.track;
                const slot = this.slotFromTrackX(track, e.clientX);
                let startSlot = this.origin.startSlot;
                let endSlot = this.origin.startSlot + this.origin.spanSlots - 1;

                if (this.mode === 'resize-w') startSlot = Math.min(slot, endSlot);
                else endSlot = Math.max(slot, startSlot);

                startSlot = Math.max(1, startSlot);
                endSlot = Math.min(this.slotCount, endSlot);
                const spanSlots = Math.max(1, endSlot - startSlot + 1);
                this.applyLayout(this.activeEvent, startSlot, spanSlots);

                const { start: newStart, end: newEnd } = this.datesFromVisibleLayout(startSlot, spanSlots);
                const conflict = this.hasConflict(
                    this.origin.dates.vehicleId,
                    newStart,
                    newEnd,
                    this.origin.dates.reservationId
                );
                this.activeEvent.classList.toggle('is-conflict', conflict);
                return;
            }

            if (this.mode === 'create') {
                const slot = this.slotFromTrackX(this.origin.track, e.clientX);
                this.showSelection(this.origin.track, this.origin.slot, slot);
            }
        }

        async onPointerUp(e) {
            if (!this.mode || e.pointerId !== this.pointerId) return;

            try {
                if (this.mode === 'move') await this.finishMove(e);
                else if (this.mode === 'resize-w' || this.mode === 'resize-e') await this.finishResize(e);
                else if (this.mode === 'create') this.finishCreate(e);
            } finally {
                this.cleanupInteraction();
            }
        }

        async finishMove(e) {
            const el = this.activeEvent;
            if (!this.ghost) {
                this.onEventClick({ preventDefault() {} }, el);
                return;
            }

            const track = this.getTrackFromPoint(e.clientX, e.clientY) ?? this.origin.track;
            const vehicleId = this.getVehicleIdFromRow(this.getRowFromTrack(track));
            const slot = this.slotFromTrackX(track, e.clientX);
            const startSlot = Math.max(1, Math.min(this.slotCount - this.origin.spanSlots + 1, slot));
            const { start: newStart, end: newEnd } = this.datesFromVisibleLayout(startSlot, this.origin.spanSlots);

            if (this.hasConflict(vehicleId, newStart, newEnd, this.origin.dates.reservationId)) {
                showToast('Konflikt termina', 'Vozilo je već rezervirano u tom periodu.', 'error');
                this.revertEventLayout(el, this.origin.dates);
                return;
            }

            const samePlace =
                vehicleId === this.origin.dates.vehicleId &&
                toIsoDate(newStart) === this.origin.dates.start &&
                toIsoDate(newEnd) === this.origin.dates.end;

            if (samePlace) return;

            await this.patchSchedule(el, vehicleId, newStart, newEnd, track);
        }

        async finishResize(e) {
            const el = this.activeEvent;
            const track = this.origin.track;
            const startSlot = parseInt(el.style.getPropertyValue('--s'), 10);
            const spanSlots = parseInt(el.style.getPropertyValue('--n'), 10);
            const { start: newStart, end: newEnd } = this.datesFromVisibleLayout(startSlot, spanSlots);

            if (toIsoDate(newStart) === this.origin.dates.start && toIsoDate(newEnd) === this.origin.dates.end)
                return;

            if (this.hasConflict(this.origin.dates.vehicleId, newStart, newEnd, this.origin.dates.reservationId)) {
                showToast('Konflikt termina', 'Vozilo je već rezervirano u tom periodu.', 'error');
                this.revertEventLayout(el, this.origin.dates);
                return;
            }

            await this.patchSchedule(el, this.origin.dates.vehicleId, newStart, newEnd, track);
        }

        finishCreate(e) {
            const slot = this.slotFromTrackX(this.origin.track, e.clientX);
            const from = Math.min(this.origin.slot, slot);
            const to = Math.max(this.origin.slot, slot);
            const spanSlots = to - from + 1;
            const { start, end } = this.datesFromVisibleLayout(from, spanSlots);

            if (this.modal) {
                this.modal.openCreate({
                    vehicleId: this.origin.vehicleId,
                    startDate: toIsoDate(start),
                    endDate: toIsoDate(end)
                });
                return;
            }

            const params = new URLSearchParams({
                vehicleId: String(this.origin.vehicleId),
                startDate: toIsoDate(start),
                endDate: toIsoDate(end),
                returnUrl: this.returnUrl
            });
            window.location.href = `${this.createUrl}?${params}`;
        }

        revertEventLayout(el, dates) {
            const layout = visibleLayout(dates.start, dates.end, this.monthStart, this.monthEnd);
            if (layout) this.applyLayout(el, layout.startSlot, layout.spanSlots);
        }

        async patchSchedule(el, vehicleId, startDate, endDate, targetTrack) {
            const reservationId = parseInt(el.dataset.reservationId, 10);
            const prev = { ...this.readEventDates(el), track: el.closest('.tl-track') };

            el.classList.add('is-saving');
            try {
                const res = await fetch(`/api/timeline/reservation/${reservationId}/schedule`, {
                    method: 'PATCH',
                    headers: { 'Content-Type': 'application/json' },
                    credentials: 'same-origin',
                    body: JSON.stringify({
                        vehicleId,
                        startDate: toIsoDate(startDate),
                        endDate: toIsoDate(endDate)
                    })
                });

                const body = await res.json().catch(() => ({}));
                if (!res.ok) {
                    const msg = body.error ?? 'Spremanje nije uspjelo.';
                    showToast('Raspored', msg, 'error');
                    this.revertEventLayout(el, prev);
                    if (prev.track && prev.track !== el.closest('.tl-track')) prev.track.appendChild(el);
                    return;
                }

                const startIso = toIsoDate(parseIsoDate(body.startDate?.split('T')[0] ?? toIsoDate(startDate)));
                const endIso = toIsoDate(parseIsoDate(body.endDate?.split('T')[0] ?? toIsoDate(endDate)));
                el.dataset.start = startIso;
                el.dataset.end = endIso;
                el.dataset.vehicleId = String(body.vehicleId ?? vehicleId);

                const layout = visibleLayout(startIso, endIso, this.monthStart, this.monthEnd);
                if (!layout) {
                    el.remove();
                    showToast('Raspored', 'Rezervacija je premještena izvan prikazanog mjeseca.', 'warn');
                    return;
                }

                this.applyLayout(el, layout.startSlot, layout.spanSlots);
                if (targetTrack && el.parentElement !== targetTrack) targetTrack.appendChild(el);

                if (body.status) {
                    const css = statusToClass(body.status);
                    const editable = ['Draft', 'Confirmed', 'Active'].includes(body.status);
                    el.className = `tl-event tl-res ${css}${editable && this.canEdit ? ' editable' : ''}`;
                    el.dataset.editable = editable && this.canEdit ? 'true' : 'false';
                }

                showToast('Raspored', 'Rezervacija je ažurirana.');
                el.classList.add('is-selected');
            } catch {
                showToast('Raspored', 'Mrežna greška pri spremanju.', 'error');
                this.revertEventLayout(el, prev);
            } finally {
                el.classList.remove('is-saving');
            }
        }

        cleanupInteraction() {
            if (this.activeEvent?.hasPointerCapture?.(this.pointerId))
                this.activeEvent.releasePointerCapture(this.pointerId);
            if (this.origin?.track?.hasPointerCapture?.(this.pointerId))
                this.origin.track.releasePointerCapture(this.pointerId);

            this.activeEvent?.classList.remove('is-dragging', 'is-resizing', 'is-conflict');
            this.ghost?.remove();
            this.ghost = null;
            this.clearDropHighlight();
            this.hideSelection();
            document.body.classList.remove('tl-interacting');

            this.mode = null;
            this.activeEvent = null;
            this.pointerId = null;
            this.origin = null;

            document.removeEventListener('pointermove', this.onPointerMove);
            document.removeEventListener('pointerup', this.onPointerUp);
            document.removeEventListener('pointercancel', this.onPointerUp);
        }
    }

    function statusToClass(status) {
        const map = {
            Draft: 'draft',
            Confirmed: 'confirmed',
            Active: 'active',
            Completed: 'completed',
            Cancelled: 'cancelled'
        };
        return map[status] ?? 'draft';
    }

    window.TimelineScheduler = TimelineScheduler;
})();
