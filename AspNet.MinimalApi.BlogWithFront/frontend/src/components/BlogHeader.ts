import { BaseComponent } from './BaseComponent';

/**
 * Компонент хедера блогу
 */
export class BlogHeader extends BaseComponent {
  private isAuthenticated = false;
  private currentUser: any = null;

  constructor() {
    super();
  }

  static get observedAttributes() {
    return ['authenticated', 'user'];
  }

  protected render(): void {
    this.shadow.innerHTML = '';
    
    // Додаємо стилі
    const styles = this.createStyles(`
      :host {
        display: block;
      }

      .header-container {
        max-width: 1200px;
        margin: 0 auto;
        padding: 0 1rem;
        display: flex;
        justify-content: space-between;
        align-items: center;
        height: 60px;
      }

      .logo {
        font-size: 1.5rem;
        font-weight: bold;
        color: #333;
        text-decoration: none;
        display: flex;
        align-items: center;
        gap: 0.5rem;
      }

      .logo:hover {
        color: #007bff;
      }

      .nav {
        display: flex;
        align-items: center;
        gap: 1rem;
      }

      .nav-link {
        color: #666;
        text-decoration: none;
        padding: 0.5rem 1rem;
        border-radius: 4px;
        transition: all 0.2s;
      }

      .nav-link:hover {
        color: #007bff;
        background-color: #f8f9fa;
      }

      .nav-link.active {
        color: #007bff;
        background-color: #e3f2fd;
      }

      .user-menu {
        position: relative;
        display: inline-block;
      }

      .user-button {
        background: none;
        border: none;
        color: #666;
        cursor: pointer;
        padding: 0.5rem 1rem;
        border-radius: 4px;
        display: flex;
        align-items: center;
        gap: 0.5rem;
        transition: all 0.2s;
      }

      .user-button:hover {
        color: #007bff;
        background-color: #f8f9fa;
      }

      .dropdown {
        position: absolute;
        top: 100%;
        right: 0;
        background: white;
        border: 1px solid #e1e5e9;
        border-radius: 4px;
        box-shadow: 0 4px 6px rgba(0,0,0,0.1);
        min-width: 200px;
        z-index: 1000;
        display: none;
      }

      .dropdown.show {
        display: block;
      }

      .dropdown-item {
        display: block;
        width: 100%;
        padding: 0.75rem 1rem;
        color: #333;
        text-decoration: none;
        border: none;
        background: none;
        text-align: left;
        cursor: pointer;
        transition: background-color 0.2s;
      }

      .dropdown-item:hover {
        background-color: #f8f9fa;
      }

      .dropdown-divider {
        height: 1px;
        background-color: #e1e5e9;
        margin: 0.5rem 0;
      }

      .btn {
        padding: 0.5rem 1rem;
        border: none;
        border-radius: 4px;
        text-decoration: none;
        cursor: pointer;
        transition: all 0.2s;
        display: inline-block;
      }

      .btn-primary {
        background-color: #007bff;
        color: white;
      }

      .btn-primary:hover {
        background-color: #0056b3;
      }

      .btn-outline {
        background-color: transparent;
        color: #007bff;
        border: 1px solid #007bff;
      }

      .btn-outline:hover {
        background-color: #007bff;
        color: white;
      }

      @media (max-width: 768px) {
        .header-container {
          padding: 0 0.5rem;
        }
        
        .nav {
          gap: 0.5rem;
        }
        
        .nav-link {
          padding: 0.5rem;
          font-size: 0.9rem;
        }
      }
    `);

    // Створюємо структуру хедера
    const container = this.createElement('div', '', 'header-container');
    
    // Логотип
    const logo = document.createElement('a');
    logo.href = '/';
    logo.className = 'logo';
    logo.innerHTML = '📝 Блог';
    
    // Навігація
    const nav = this.createElement('nav', '', 'nav');
    
    if (this.isAuthenticated) {
      // Меню для авторизованих користувачів
      nav.appendChild(this.createNavLink('/', 'Головна'));
      nav.appendChild(this.createNavLink('/create-post', 'Створити пост'));
      
      // Меню користувача
      const userMenu = this.createUserMenu();
      nav.appendChild(userMenu);
    } else {
      // Меню для неавторизованих користувачів
      nav.appendChild(this.createNavLink('/', 'Головна'));
      nav.appendChild(this.createButton('/login', 'Вхід', 'btn-outline'));
      nav.appendChild(this.createButton('/register', 'Реєстрація', 'btn-primary'));
    }

    container.appendChild(logo);
    container.appendChild(nav);

    this.shadow.appendChild(styles);
    this.shadow.appendChild(container);
  }

  protected setupEventListeners(): void {
    // Слухаємо кліки по посиланнях
    this.shadow.addEventListener('click', this.handleClick.bind(this));
    
    // Закриваємо dropdown при кліку поза ним
    document.addEventListener('click', this.closeDropdowns.bind(this));
  }

  protected onAttributeChanged(name: string, _oldValue: string, newValue: string): void {
    if (name === 'authenticated') {
      this.isAuthenticated = newValue === 'true';
    } else if (name === 'user') {
      try {
        this.currentUser = newValue ? JSON.parse(newValue) : null;
      } catch {
        this.currentUser = null;
      }
    }
    
    if (this.isConnected) {
      this.render();
    }
  }

  /**
   * Створення навігаційного посилання
   */
  private createNavLink(href: string, text: string): HTMLAnchorElement {
    const link = document.createElement('a');
    link.href = href;
    link.textContent = text;
    link.className = 'nav-link';
    
    // Позначаємо активне посилання
    if (window.location.pathname === href) {
      link.classList.add('active');
    }
    
    return link;
  }

  /**
   * Створення кнопки
   */
  private createButton(href: string, text: string, className: string): HTMLAnchorElement {
    const button = document.createElement('a');
    button.href = href;
    button.textContent = text;
    button.className = `btn ${className}`;
    return button;
  }

  /**
   * Створення меню користувача
   */
  private createUserMenu(): HTMLDivElement {
    const userMenu = this.createElement('div', '', 'user-menu') as HTMLDivElement;
    
    const userButton = document.createElement('button');
    userButton.className = 'user-button';
    userButton.innerHTML = `
      👤 ${this.currentUser?.name || 'Користувач'}
      <span style="margin-left: 0.25rem;">▼</span>
    `;
    
    const dropdown = this.createElement('div', '', 'dropdown');
    dropdown.innerHTML = `
      <a href="/profile" class="dropdown-item">👤 Профіль</a>
      <div class="dropdown-divider"></div>
      <button class="dropdown-item" data-action="logout">🚪 Вихід</button>
    `;
    
    userMenu.appendChild(userButton);
    userMenu.appendChild(dropdown);
    
    return userMenu;
  }

  /**
   * Обробка кліків
   */
  private handleClick(event: Event): void {
    const target = event.target as HTMLElement;
    
    // Обробка кліку по кнопці користувача
    if (target.classList.contains('user-button')) {
      event.stopPropagation();
      const dropdown = target.nextElementSibling as HTMLElement;
      dropdown.classList.toggle('show');
      return;
    }
    
    // Обробка дій в dropdown
    if (target.hasAttribute('data-action')) {
      const action = target.getAttribute('data-action');
      if (action === 'logout') {
        this.handleLogout();
      }
      return;
    }
  }

  /**
   * Закриття всіх dropdown меню
   */
  private closeDropdowns(): void {
    const dropdowns = this.querySelectorAll('.dropdown');
    dropdowns.forEach(dropdown => {
      dropdown.classList.remove('show');
    });
  }

  /**
   * Обробка виходу з системи
   */
  private handleLogout(): void {
    this.emit('logout');
  }
}

// Реєструємо компонент
customElements.define('blog-header', BlogHeader);
