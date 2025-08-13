import type { Route, RouteParams } from '../types';

/**
 * Клієнтський роутер для SPA навігації
 */
export class Router {
  private routes: Map<string, Route> = new Map();
  private currentRoute: string | null = null;
  private initialized = false;

  constructor() {
    this.setupRoutes();
  }

  /**
   * Налаштування маршрутів
   */
  private setupRoutes(): void {
    const routes: Route[] = [
      // Публічні сторінки
      { path: '/', component: 'home-page', title: 'Головна' },
      { path: '/posts/:id', component: 'post-page', title: 'Пост' },
      
      // Аутентифікація
      { path: '/login', component: 'login-page', title: 'Вхід' },
      { path: '/register', component: 'register-page', title: 'Реєстрація' },
      { path: '/confirm-email', component: 'confirm-email-page', title: 'Підтвердження email' },
      { path: '/forgot-password', component: 'forgot-password-page', title: 'Забув пароль' },
      { path: '/reset-password', component: 'reset-password-page', title: 'Скидання пароля' },
      { path: '/two-factor', component: 'two-factor-page', title: 'Двохфакторна аутентифікація' },
      
      // Приватні сторінки
      { path: '/profile', component: 'profile-page', title: 'Профіль', requiresAuth: true, requiresEmailConfirmed: true },
      { path: '/create-post', component: 'create-post-page', title: 'Створити пост', requiresAuth: true, requiresEmailConfirmed: true },
      { path: '/edit-post/:id', component: 'edit-post-page', title: 'Редагувати пост', requiresAuth: true, requiresEmailConfirmed: true },
      
      // Адмін сторінки
      { path: '/admin', component: 'admin-page', title: 'Адміністрування', requiresAuth: true, requiresEmailConfirmed: true, roles: ['Admin'] },
      { path: '/admin/users', component: 'admin-users-page', title: 'Користувачі', requiresAuth: true, requiresEmailConfirmed: true, roles: ['Admin'] },
      { path: '/admin/posts', component: 'admin-posts-page', title: 'Пости', requiresAuth: true, requiresEmailConfirmed: true, roles: ['Admin'] }
    ];

    routes.forEach(route => {
      this.routes.set(route.path, route);
    });
  }

  /**
   * Ініціалізація роутера
   */
  init(): void {
    if (this.initialized) return;

    // Обробляємо зміни в історії браузера
    window.addEventListener('popstate', () => {
      this.handleRoute(window.location.pathname + window.location.search);
    });

    // Перехоплюємо кліки по посиланнях
    document.addEventListener('click', (e) => {
      const link = e.target as HTMLAnchorElement;
      if (link.tagName === 'A' && link.href && this.isInternalLink(link.href)) {
        e.preventDefault();
        this.navigate(link.getAttribute('href')!);
      }
    });

    this.initialized = true;
    
    // Обробляємо поточний маршрут
    this.handleRoute(window.location.pathname + window.location.search);
  }

  /**
   * Навігація до маршруту
   */
  navigate(path: string, replace = false): void {
    if (replace) {
      window.history.replaceState(null, '', path);
    } else {
      window.history.pushState(null, '', path);
    }
    this.handleRoute(path);
  }

  /**
   * Обробка маршруту
   */
  private handleRoute(path: string): void {
    const url = new URL(path, window.location.origin);
    const pathname = url.pathname;
    const searchParams = url.searchParams;

    console.log('Handling route:', pathname);

    // Знаходимо відповідний маршрут
    const routeMatch = this.findRoute(pathname);
    if (routeMatch) {
      const { route, params } = routeMatch;
      
      // Перевіряємо права доступу
      if (!this.canAccessRoute(route)) {
        this.navigate('/login');
        return;
      }

      this.currentRoute = pathname;
      
      // Оновлюємо заголовок сторінки
      if (route.title) {
        document.title = `${route.title} - Блог`;
      }

      // Емітуємо подію навігації
      this.emitNavigationEvent(pathname, params, searchParams);
    } else {
      console.log('No route found for:', pathname);
      // Перенаправляємо на головну сторінку
      this.navigate('/', true);
    }
  }

  /**
   * Пошук маршруту з параметрами
   */
  private findRoute(pathname: string): { route: Route; params: RouteParams } | null {
    for (const [routePath, route] of this.routes) {
      const params = this.matchRoute(routePath, pathname);
      if (params !== null) {
        return { route, params };
      }
    }
    return null;
  }

  /**
   * Перевірка відповідності маршруту
   */
  private matchRoute(routePath: string, pathname: string): RouteParams | null {
    const routeParts = routePath.split('/');
    const pathParts = pathname.split('/');

    if (routeParts.length !== pathParts.length) {
      return null;
    }

    const params: RouteParams = {};

    for (let i = 0; i < routeParts.length; i++) {
      const routePart = routeParts[i];
      const pathPart = pathParts[i];

      if (routePart.startsWith(':')) {
        // Параметр маршруту
        const paramName = routePart.slice(1);
        params[paramName] = pathPart;
      } else if (routePart !== pathPart) {
        // Частини не співпадають
        return null;
      }
    }

    return params;
  }

  /**
   * Перевірка прав доступу до маршруту
   */
  private canAccessRoute(_route: Route): boolean {
    // TODO: Реалізувати перевірку аутентифікації та ролей
    // Поки що повертаємо true для всіх маршрутів
    return true;
  }

  /**
   * Перевірка, чи є посилання внутрішнім
   */
  private isInternalLink(href: string): boolean {
    try {
      const url = new URL(href, window.location.origin);
      return url.origin === window.location.origin;
    } catch {
      return false;
    }
  }

  /**
   * Емітування події навігації
   */
  private emitNavigationEvent(path: string, params: RouteParams, searchParams: URLSearchParams): void {
    const event = new CustomEvent('navigation', {
      detail: { path, params, searchParams },
      bubbles: true,
      composed: true
    });
    document.dispatchEvent(event);
  }

  /**
   * Отримання поточного маршруту
   */
  getCurrentRoute(): string | null {
    return this.currentRoute;
  }
}

// Експортуємо єдиний екземпляр роутера
export const router = new Router();
