/* CarRent fleet push — service worker */
self.addEventListener('push', (event) => {
    let data = { title: 'CarRent', body: 'Nova obavijest flote' };
    try {
        if (event.data) data = event.data.json();
    } catch { /* default */ }

    event.waitUntil(
        self.registration.showNotification(data.title || 'CarRent', {
            body: data.body || '',
            icon: '/favicon.ico',
            badge: '/favicon.ico',
            tag: 'carrent-fleet'
        })
    );
});

self.addEventListener('notificationclick', (event) => {
    event.notification.close();
    event.waitUntil(clients.openWindow('/Notifications'));
});
