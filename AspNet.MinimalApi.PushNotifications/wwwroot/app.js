class PushNotificationManager {
    constructor() {
        this.vapidPublicKey = null;
        this.subscription = null;
        this.init();
    }

    async init() {
        this.setupEventListeners();
        await this.checkServiceWorkerSupport();
        await this.loadVapidKey();
        await this.checkSubscriptionStatus();
        await this.refreshStats();
        this.log('Додаток ініціалізовано', 'info');
    }

    setupEventListeners() {
        document.getElementById('subscribe-btn').addEventListener('click', () => this.subscribe());
        document.getElementById('unsubscribe-btn').addEventListener('click', () => this.unsubscribe());
        document.getElementById('notification-form').addEventListener('submit', (e) => this.sendNotification(e));
        document.getElementById('send-test-btn').addEventListener('click', (e) => this.sendTestNotification(e));
        document.getElementById('refresh-stats').addEventListener('click', () => this.refreshStats());
        document.getElementById('clear-logs').addEventListener('click', () => this.clearLogs());
    }

    async checkServiceWorkerSupport() {
        if (!('serviceWorker' in navigator)) {
            this.updateStatus('Service Worker не підтримується', 'error');
            this.log('Service Worker не підтримується в цьому браузері', 'error');
            return false;
        }

        if (!('PushManager' in window)) {
            this.updateStatus('Push API не підтримується', 'error');
            this.log('Push API не підтримується в цьому браузері', 'error');
            return false;
        }

        try {
            const registration = await navigator.serviceWorker.register('/sw.js');
            this.log('Service Worker зареєстровано успішно', 'success');
            return true;
        } catch (error) {
            this.updateStatus('Помилка реєстрації Service Worker', 'error');
            this.log(`Помилка реєстрації Service Worker: ${error.message}`, 'error');
            return false;
        }
    }

    async loadVapidKey() {
        try {
            const response = await fetch('/api/vapid-public-key');
            const data = await response.json();
            this.vapidPublicKey = data.publicKey;
            this.log('VAPID ключ завантажено', 'info');
        } catch (error) {
            this.log(`Помилка завантаження VAPID ключа: ${error.message}`, 'error');
        }
    }

    async checkSubscriptionStatus() {
        try {
            const registration = await navigator.serviceWorker.ready;
            this.subscription = await registration.pushManager.getSubscription();
            
            if (this.subscription) {
                this.updateStatus('Підписано на нотифікації', 'connected');
                this.enableButton('unsubscribe-btn');
                this.log('Знайдено активну підписку', 'success');
            } else {
                this.updateStatus('Не підписано на нотифікації', 'disconnected');
                this.enableButton('subscribe-btn');
                this.log('Активна підписка не знайдена', 'info');
            }
        } catch (error) {
            this.updateStatus('Помилка перевірки підписки', 'error');
            this.log(`Помилка перевірки підписки: ${error.message}`, 'error');
        }
    }

    async subscribe() {
        try {
            this.disableButtons();
            this.updateStatus('Підписка...', 'info');

            const registration = await navigator.serviceWorker.ready;
            
            this.subscription = await registration.pushManager.subscribe({
                userVisibleOnly: true,
                applicationServerKey: this.urlBase64ToUint8Array(this.vapidPublicKey)
            });

            await this.sendSubscriptionToServer(this.subscription);
            
            this.updateStatus('Підписано на нотифікації', 'connected');
            this.enableButton('unsubscribe-btn');
            this.log('Успішно підписано на нотифікації', 'success');
            await this.refreshStats();
        } catch (error) {
            this.updateStatus('Помилка підписки', 'error');
            this.enableButton('subscribe-btn');
            this.log(`Помилка підписки: ${error.message}`, 'error');
        }
    }

    async unsubscribe() {
        try {
            this.disableButtons();
            this.updateStatus('Відписка...', 'info');

            if (this.subscription) {
                await this.subscription.unsubscribe();
                await this.removeSubscriptionFromServer(this.subscription.endpoint);
                this.subscription = null;
            }

            this.updateStatus('Відписано від нотифікацій', 'disconnected');
            this.enableButton('subscribe-btn');
            this.log('Успішно відписано від нотифікацій', 'success');
            await this.refreshStats();
        } catch (error) {
            this.updateStatus('Помилка відписки', 'error');
            this.enableButton('unsubscribe-btn');
            this.log(`Помилка відписки: ${error.message}`, 'error');
        }
    }

    async sendSubscriptionToServer(subscription) {
        const response = await fetch('/api/subscribe', {
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

        if (!response.ok) {
            throw new Error('Помилка відправки підписки на сервер');
        }
    }

    async removeSubscriptionFromServer(endpoint) {
        const response = await fetch('/api/unsubscribe', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ endpoint })
        });

        if (!response.ok) {
            throw new Error('Помилка видалення підписки з сервера');
        }
    }

    async sendNotification(event) {
        event.preventDefault();

        const formData = new FormData(event.target);
        const notificationData = {
            title: formData.get('title'),
            body: formData.get('body'),
            url: formData.get('url') || null
        };

        try {
            const response = await fetch('/api/send-notification', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(notificationData)
            });

            const result = await response.json();

            if (response.ok) {
                this.log(`Нотифікацію відправлено: ${result.message}`, 'success');
                await this.refreshStats();
            } else {
                this.log(`Помилка відправки нотифікації: ${result.error}`, 'error');
            }
        } catch (error) {
            this.log(`Помилка відправки нотифікації: ${error.message}`, 'error');
        }
    }

    async sendTestNotification(event) {
        event.preventDefault();

        const form = document.getElementById('notification-form');
        const formData = new FormData(form);
        const notificationData = {
            title: formData.get('title') || 'Тестова нотифікація',
            body: formData.get('body') || 'Це тестове повідомлення!',
            url: formData.get('url') || null
        };

        try {
            const response = await fetch('/api/send-test-notification', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(notificationData)
            });

            const result = await response.json();

            if (response.ok) {
                this.log(`${result.message}`, 'info');
                this.log(`Деталі: ${JSON.stringify(result.notification, null, 2)}`, 'info');
            } else {
                this.log(`Помилка тестової нотифікації: ${result.error}`, 'error');
            }
        } catch (error) {
            this.log(`Помилка тестової нотифікації: ${error.message}`, 'error');
        }
    }

    async refreshStats() {
        try {
            const response = await fetch('/api/subscriptions/stats');
            const stats = await response.json();
            
            document.getElementById('subscription-count').textContent = stats.count;
            this.log(`Статистика оновлена: ${stats.count} активних підписок`, 'info');
        } catch (error) {
            this.log(`Помилка оновлення статистики: ${error.message}`, 'error');
        }
    }

    updateStatus(message, type) {
        const statusElement = document.getElementById('status-text');
        const indicatorElement = document.getElementById('status-indicator');
        const statusContainer = document.getElementById('subscription-status');
        
        statusElement.textContent = message;
        statusContainer.className = `status ${type}`;
        
        switch (type) {
            case 'connected':
                indicatorElement.textContent = '🟢';
                break;
            case 'disconnected':
                indicatorElement.textContent = '🟡';
                break;
            case 'error':
                indicatorElement.textContent = '🔴';
                break;
            default:
                indicatorElement.textContent = '⚪';
        }
    }

    enableButton(buttonId) {
        const button = document.getElementById(buttonId);
        button.disabled = false;
    }

    disableButtons() {
        document.getElementById('subscribe-btn').disabled = true;
        document.getElementById('unsubscribe-btn').disabled = true;
    }

    log(message, type = 'info') {
        const logsContainer = document.getElementById('logs');
        const timestamp = new Date().toLocaleTimeString();
        
        const logEntry = document.createElement('div');
        logEntry.className = 'log-entry';
        logEntry.innerHTML = `
            <span class="log-timestamp">[${timestamp}]</span>
            <span class="log-${type}">${message}</span>
        `;
        
        logsContainer.appendChild(logEntry);
        logsContainer.scrollTop = logsContainer.scrollHeight;
    }

    clearLogs() {
        document.getElementById('logs').innerHTML = '';
    }

    urlBase64ToUint8Array(base64String) {
        const padding = '='.repeat((4 - base64String.length % 4) % 4);
        const base64 = (base64String + padding)
            .replace(/-/g, '+')
            .replace(/_/g, '/');

        const rawData = window.atob(base64);
        const outputArray = new Uint8Array(rawData.length);

        for (let i = 0; i < rawData.length; ++i) {
            outputArray[i] = rawData.charCodeAt(i);
        }
        return outputArray;
    }
}

// Ініціалізуємо додаток після завантаження DOM
document.addEventListener('DOMContentLoaded', () => {
    new PushNotificationManager();
});
