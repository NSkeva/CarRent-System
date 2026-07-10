(() => {
    'use strict';

    const statusEl = document.querySelector('[data-push-status]');
    const btnEnable = document.querySelector('[data-push-enable]');
    const btnDisable = document.querySelector('[data-push-disable]');

    if (!btnEnable) return;

    function setStatus(text, ok) {
        if (!statusEl) return;
        statusEl.textContent = text;
        statusEl.classList.toggle('chip-completed', ok === true);
        statusEl.classList.toggle('chip-draft', ok === false);
        statusEl.classList.toggle('chip-planned', ok === null);
    }

    function urlBase64ToUint8Array(base64String) {
        const padding = '='.repeat((4 - (base64String.length % 4)) % 4);
        const base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/');
        const raw = atob(base64);
        const arr = new Uint8Array(raw.length);
        for (let i = 0; i < raw.length; ++i) arr[i] = raw.charCodeAt(i);
        return arr;
    }

    async function getPublicKey() {
        const res = await fetch('/api/push/vapid-public-key', { credentials: 'same-origin' });
        if (!res.ok) throw new Error('VAPID ključ nije dostupan');
        const body = await res.json();
        return body.publicKey;
    }

    async function subscribe() {
        if (!('serviceWorker' in navigator) || !('PushManager' in window)) {
            setStatus('Browser ne podržava push', false);
            return;
        }

        const permission = await Notification.requestPermission();
        if (permission !== 'granted') {
            setStatus('Dozvola odbijena', false);
            return;
        }

        const reg = await navigator.serviceWorker.register('/sw.js');
        await navigator.serviceWorker.ready;
        const publicKey = await getPublicKey();
        const sub = await reg.pushManager.subscribe({
            userVisibleOnly: true,
            applicationServerKey: urlBase64ToUint8Array(publicKey)
        });

        const json = sub.toJSON();
        const res = await fetch('/api/push/subscribe', {
            method: 'POST',
            credentials: 'same-origin',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                endpoint: json.endpoint,
                keys: { p256dh: json.keys.p256dh, auth: json.keys.auth }
            })
        });

        if (!res.ok) throw new Error('Spremanje pretplate nije uspjelo');
        setStatus('Push uključen', true);
        btnEnable.hidden = true;
        if (btnDisable) btnDisable.hidden = false;
    }

    async function unsubscribe() {
        const reg = await navigator.serviceWorker.getRegistration('/sw.js');
        const sub = await reg?.pushManager.getSubscription();
        if (sub) {
            await fetch('/api/push/unsubscribe', {
                method: 'POST',
                credentials: 'same-origin',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ endpoint: sub.endpoint })
            });
            await sub.unsubscribe();
        }
        setStatus('Push isključen', null);
        btnEnable.hidden = false;
        if (btnDisable) btnDisable.hidden = true;
    }

    btnEnable.addEventListener('click', () => subscribe().catch((e) => setStatus(e.message, false)));
    if (btnDisable) btnDisable.addEventListener('click', () => unsubscribe().catch((e) => setStatus(e.message, false)));

    (async () => {
        try {
            if (!('serviceWorker' in navigator)) return;
            const reg = await navigator.serviceWorker.getRegistration('/sw.js');
            const sub = await reg?.pushManager.getSubscription();
            if (sub) {
                setStatus('Push uključen', true);
                btnEnable.hidden = true;
                if (btnDisable) btnDisable.hidden = false;
            } else {
                setStatus('Push nije uključen', null);
            }
        } catch {
            setStatus('Push nije dostupan', false);
        }
    })();
})();
