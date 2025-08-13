const CACHE_NAME = 'push-notifications-v1';
const urlsToCache = [
    '/',
    '/index.html',
    '/styles.css',
    '/app.js',
    '/manifest.json',
    '/icon-192x192.svg',
    '/icon-512x512.svg',
    '/badge-72x72.svg'
];

// Встановлення Service Worker
self.addEventListener('install', event => {
    console.log('Service Worker: Installing...');
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then(cache => {
                console.log('Service Worker: Caching files');
                return cache.addAll(urlsToCache);
            })
            .catch(err => console.log('Service Worker: Cache failed', err))
    );
});

// Активація Service Worker
self.addEventListener('activate', event => {
    console.log('Service Worker: Activating...');
    event.waitUntil(
        caches.keys().then(cacheNames => {
            return Promise.all(
                cacheNames.map(cacheName => {
                    if (cacheName !== CACHE_NAME) {
                        console.log('Service Worker: Deleting old cache', cacheName);
                        return caches.delete(cacheName);
                    }
                })
            );
        })
    );
});

// Обробка запитів (стратегія Cache First)
self.addEventListener('fetch', event => {
    event.respondWith(
        caches.match(event.request)
            .then(response => {
                // Повертаємо кешовану версію або завантажуємо з мережі
                return response || fetch(event.request);
            })
            .catch(() => {
                // Якщо немає мережі і немає в кеші, повертаємо офлайн сторінку
                if (event.request.destination === 'document') {
                    return caches.match('/index.html');
                }
            })
    );
});

// Обробка пуш-повідомлень
self.addEventListener('push', event => {
    console.log('Service Worker: Push event received', event);
    
    let notificationData = {
        title: 'Нове повідомлення',
        body: 'У вас є нове повідомлення!',
        icon: '/icon-192x192.svg',
        badge: '/badge-72x72.svg',
        tag: 'default',
        requireInteraction: false,
        actions: [
            {
                action: 'open',
                title: 'Відкрити',
                icon: '/icon-192x192.svg'
            },
            {
                action: 'close',
                title: 'Закрити'
            }
        ],
        data: {
            url: '/'
        }
    };

    if (event.data) {
        try {
            const payload = event.data.json();
            notificationData = { ...notificationData, ...payload };
        } catch (error) {
            console.error('Service Worker: Error parsing push data', error);
            notificationData.body = event.data.text();
        }
    }

    const promiseChain = self.registration.showNotification(
        notificationData.title,
        {
            body: notificationData.body,
            icon: notificationData.icon,
            badge: notificationData.badge,
            tag: notificationData.tag,
            requireInteraction: notificationData.requireInteraction,
            silent: notificationData.silent,
            vibrate: notificationData.vibrate,
            data: notificationData.data,
            actions: notificationData.actions
        }
    );

    event.waitUntil(promiseChain);
});

// Обробка кліків по нотифікаціям
self.addEventListener('notificationclick', event => {
    console.log('Service Worker: Notification click received', event);
    
    event.notification.close();

    let urlToOpen = '/';
    
    // Перевіряємо, чи є URL в даних нотифікації
    if (event.notification.data && event.notification.data.url) {
        urlToOpen = event.notification.data.url;
    }

    // Обробляємо дії нотифікації
    if (event.action === 'close') {
        return; // Просто закриваємо нотифікацію
    }

    const promiseChain = clients.matchAll({
        type: 'window',
        includeUncontrolled: true
    }).then(windowClients => {
        // Шукаємо відкриту вкладку з нашим додатком
        for (let i = 0; i < windowClients.length; i++) {
            const client = windowClients[i];
            if (client.url.includes(self.location.origin)) {
                // Фокусуємося на існуючій вкладці
                return client.focus().then(() => {
                    // Переходимо на потрібну сторінку
                    return client.navigate(urlToOpen);
                });
            }
        }
        
        // Якщо відкритої вкладки немає, відкриваємо нову
        return clients.openWindow(urlToOpen);
    });

    event.waitUntil(promiseChain);
});

// Обробка закриття нотифікацій
self.addEventListener('notificationclose', event => {
    console.log('Service Worker: Notification closed', event);
    
    // Тут можна відправити аналітику про закриття нотифікації
    // analytics.track('notification_closed', {
    //     tag: event.notification.tag,
    //     timestamp: Date.now()
    // });
});

// Обробка помилок пуш-підписки
self.addEventListener('pushsubscriptionchange', event => {
    console.log('Service Worker: Push subscription changed', event);

    // Отримуємо актуальний VAPID ключ та оновлюємо підписку
    const promiseChain = fetch('/api/vapid-public-key')
        .then(response => response.json())
        .then(data => {
            return self.registration.pushManager.subscribe({
                userVisibleOnly: true,
                applicationServerKey: urlBase64ToUint8Array(data.publicKey)
            });
        })
        .then(subscription => {
            return fetch('/api/subscribe', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    endpoint: subscription.endpoint,
                    keys: {
                        p256dh: btoa(String.fromCharCode.apply(null, new Uint8Array(subscription.getKey('p256dh')))),
                        auth: btoa(String.fromCharCode.apply(null, new Uint8Array(subscription.getKey('auth'))))
                    }
                })
            });
        });

    event.waitUntil(promiseChain);
});

// Допоміжна функція для конвертації VAPID ключа
function urlBase64ToUint8Array(base64String) {
    const padding = '='.repeat((4 - base64String.length % 4) % 4);
    const base64 = (base64String + padding)
        .replace(/-/g, '+')
        .replace(/_/g, '/');

    const rawData = atob(base64);
    const outputArray = new Uint8Array(rawData.length);

    for (let i = 0; i < rawData.length; ++i) {
        outputArray[i] = rawData.charCodeAt(i);
    }
    return outputArray;
}
