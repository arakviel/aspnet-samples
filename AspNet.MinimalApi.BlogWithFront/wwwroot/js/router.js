/**
 * Клієнтський роутер для SPA навігації
 * Забезпечує навігацію без перезавантаження сторінки
 */

export class Router {
  constructor() {
    this.routes = new Map();
    this.currentRoute = null;
    this.initialized = false;
  }

  /**
   * Ініціалізація роутера
   */
  init() {
    if (this.initialized) return;

    console.log('Initializing router...');

    // Обробляємо зміни в історії браузера
    window.addEventListener('popstate', (e) => {
      this.handleRoute(window.location.pathname + window.location.search);
    });

    // Перехоплюємо кліки по посиланнях
    document.addEventListener('click', (e) => {
      const link = e.target.closest('a[href]');
      if (link && this.isInternalLink(link.href)) {
        e.preventDefault();
        this.navigate(link.href);
      }
    });

    this.initialized = true;

    // Обробляємо поточний маршрут
    this.handleRoute(window.location.pathname + window.location.search);
  }

  /**
   * Перевіряє, чи є посилання внутрішнім
   */
  isInternalLink(href) {
    try {
      const url = new URL(href, window.location.origin);
      return url.origin === window.location.origin;
    } catch {
      return false;
    }
  }

  /**
   * Реєструє маршрут
   * @param {string} path - Шлях маршруту (може містити параметри :id)
   * @param {Function} handler - Обробник маршруту
   */
  register(path, handler) {
    console.log('Registering route:', path);
    this.routes.set(path, handler);
  }

  /**
   * Навігація до нового маршруту
   * @param {string} path - Шлях для навігації
   * @param {boolean} replace - Замінити поточний запис в історії
   */
  navigate(path, replace = false) {
    if (replace) {
      window.history.replaceState(null, '', path);
    } else {
      window.history.pushState(null, '', path);
    }
    this.handleRoute(path);
  }

  /**
   * Обробляє маршрут
   * @param {string} path - Шлях для обробки
   */
  handleRoute(path) {
    try {
      const url = new URL(path, window.location.origin);
      const pathname = url.pathname;
      const searchParams = url.searchParams;

      console.log('Handling route:', pathname);

      // Знаходимо відповідний маршрут
      const route = this.findRoute(pathname);
      if (route) {
        this.currentRoute = pathname;
        console.log('Found route handler for:', pathname);
        route.handler(route.params, searchParams);
      } else {
        console.log('No route found for:', pathname, 'Using default handler');
        // Маршрут не знайдено - використовуємо обробник за замовчуванням (головна сторінка)
        const defaultRoute = this.routes.get('/');
        if (defaultRoute) {
          this.currentRoute = pathname;
          defaultRoute({}, searchParams);
        }
      }
    } catch (error) {
      console.error('Error handling route:', error);
      this.navigate('/', true);
    }
  }

  /**
   * Знаходить маршрут та витягує параметри
   * @param {string} pathname - Шлях для пошуку
   * @returns {Object|null} Об'єкт з обробником та параметрами
   */
  findRoute(pathname) {
    for (const [routePath, handler] of this.routes) {
      const params = this.matchRoute(routePath, pathname);
      if (params !== null) {
        return { handler, params };
      }
    }
    return null;
  }

  /**
   * Перевіряє відповідність маршруту та витягує параметри
   * @param {string} routePath - Шаблон маршруту
   * @param {string} pathname - Фактичний шлях
   * @returns {Object|null} Параметри маршруту або null
   */
  matchRoute(routePath, pathname) {
    // Перетворюємо шаблон маршруту в регулярний вираз
    const paramNames = [];
    const regexPattern = routePath.replace(/:([^/]+)/g, (match, paramName) => {
      paramNames.push(paramName);
      return '([^/]+)';
    });

    const regex = new RegExp(`^${regexPattern}$`);
    const match = pathname.match(regex);

    if (match) {
      const params = {};
      paramNames.forEach((name, index) => {
        params[name] = match[index + 1];
      });
      return params;
    }

    return null;
  }

  /**
   * Отримує поточний маршрут
   * @returns {string} Поточний маршрут
   */
  getCurrentRoute() {
    return this.currentRoute;
  }

  /**
   * Отримує параметри поточного маршруту
   * @returns {URLSearchParams} Параметри запиту
   */
  getSearchParams() {
    return new URLSearchParams(window.location.search);
  }
}

// Експортуємо глобальний екземпляр роутера
export const router = new Router();
