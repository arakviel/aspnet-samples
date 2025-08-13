import { BaseComponent } from '../components/BaseComponent';
import { authService } from '../services/AuthService';

/**
 * Сторінка входу в систему
 */
export class LoginPage extends BaseComponent {
  private isLoading = false;
  private errorMessage = '';
  private showTwoFactor = false;

  constructor() {
    super();
  }

  protected render(): void {
    console.log('LoginPage render викликано');
    this.shadow.innerHTML = '';
    
    // Додаємо стилі
    const styles = this.createStyles(`
      :host {
        display: block;
        padding: 2rem;
      }

      .container {
        max-width: 400px;
        margin: 0 auto;
        background: white;
        padding: 2rem;
        border-radius: 8px;
        box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      }

      h1 {
        text-align: center;
        color: #333;
        margin: 0 0 2rem;
      }

      .form-group {
        margin-bottom: 1rem;
      }

      label {
        display: block;
        margin-bottom: 0.5rem;
        font-weight: 500;
        color: #333;
      }

      input {
        width: 100%;
        padding: 0.75rem;
        border: 1px solid #ddd;
        border-radius: 4px;
        font-size: 1rem;
        box-sizing: border-box;
      }

      input:focus {
        outline: none;
        border-color: #007bff;
        box-shadow: 0 0 0 3px rgba(0, 123, 255, 0.1);
      }

      .btn {
        width: 100%;
        padding: 0.75rem;
        background: #007bff;
        color: white;
        border: none;
        border-radius: 4px;
        font-size: 1rem;
        cursor: pointer;
        margin-top: 1rem;
      }

      .btn:hover {
        background: #0056b3;
      }

      .links {
        text-align: center;
        margin-top: 1rem;
      }

      .links a {
        color: #007bff;
        text-decoration: none;
        margin: 0 0.5rem;
      }

      .links a:hover {
        text-decoration: underline;
      }

      .checkbox-group {
        display: flex;
        align-items: center;
        gap: 0.5rem;
        margin: 1rem 0;
      }

      .checkbox-group input[type="checkbox"] {
        width: auto;
      }
    `);

    // Створюємо структуру сторінки
    const container = this.createElement('div', '', 'container');

    let formContent = '';

    if (!this.showTwoFactor) {
      // Основна форма входу
      formContent = `
        <div class="form-group">
          <label for="email">Email</label>
          <input type="email" id="email" name="email" required>
        </div>

        <div class="form-group">
          <label for="password">Пароль</label>
          <input type="password" id="password" name="password" required>
        </div>

        <div class="checkbox-group">
          <input type="checkbox" id="rememberMe" name="rememberMe">
          <label for="rememberMe">Запам'ятати мене</label>
        </div>
      `;
    } else {
      // Форма двохфакторної аутентифікації
      formContent = `
        <div class="form-group">
          <label for="twoFactorCode">Код аутентифікації</label>
          <input type="text" id="twoFactorCode" name="twoFactorCode"
                 placeholder="000000" maxlength="6" required>
          <div style="font-size: 0.875rem; color: #666; margin-top: 0.25rem;">
            Введіть код з вашого додатку аутентифікації
          </div>
        </div>
      `;
    }

    container.innerHTML = `
      <h1>🔐 ${this.showTwoFactor ? 'Двохфакторна аутентифікація' : 'Вхід'}</h1>

      ${this.errorMessage ? `<div style="background: #f8d7da; color: #721c24; padding: 0.75rem; border-radius: 4px; margin-bottom: 1rem; border: 1px solid #f5c6cb;">${this.errorMessage}</div>` : ''}

      <form id="login-form">
        ${formContent}

        <button type="submit" class="btn" ${this.isLoading ? 'disabled' : ''}>
          ${this.isLoading ? '⏳ Завантаження...' : (this.showTwoFactor ? 'Підтвердити' : 'Увійти')}
        </button>
      </form>

      ${!this.showTwoFactor ? `
        <div class="links">
          <a href="/forgot-password">Забули пароль?</a>
          <a href="/register">Реєстрація</a>
        </div>
      ` : `
        <div class="links">
          <button type="button" id="back-btn" style="background: none; border: none; color: #007bff; cursor: pointer; text-decoration: underline;">
            ← Повернутися до входу
          </button>
        </div>
      `}
    `;

    this.shadow.appendChild(styles);
    this.shadow.appendChild(container);
    
    console.log('LoginPage render завершено');
  }

  protected setupEventListeners(): void {
    // Обробка відправки форми
    this.shadow.addEventListener('submit', (event) => {
      event.preventDefault();
      this.handleSubmit(event);
    });

    // Обробка кнопки "Повернутися"
    this.shadow.addEventListener('click', (event) => {
      const target = event.target as HTMLElement;
      if (target.id === 'back-btn') {
        this.showTwoFactor = false;
        this.errorMessage = '';
        this.render();
      }
    });
  }

  /**
   * Обробка відправки форми
   */
  private async handleSubmit(event: Event): Promise<void> {
    if (this.isLoading) return;

    const form = event.target as HTMLFormElement;
    const formData = new FormData(form);

    try {
      this.isLoading = true;
      this.errorMessage = '';
      this.render();

      if (!this.showTwoFactor) {
        // Основний вхід
        const email = formData.get('email') as string;
        const password = formData.get('password') as string;
        const rememberMe = formData.has('rememberMe');

        const result = await authService.login(email, password, rememberMe);

        if (result.requiresTwoFactor) {
          this.showTwoFactor = true;
          this.render();
          return;
        }

        // Успішний вхід
        this.handleSuccessfulLogin();
      } else {
        // Двохфакторна аутентифікація
        const twoFactorCode = formData.get('twoFactorCode') as string;

        // Отримуємо збережені дані з попереднього кроку
        const email = sessionStorage.getItem('loginEmail') || '';
        const password = sessionStorage.getItem('loginPassword') || '';
        const rememberMe = sessionStorage.getItem('loginRememberMe') === 'true';

        await authService.login(email, password, rememberMe, twoFactorCode);

        // Очищуємо тимчасові дані
        sessionStorage.removeItem('loginEmail');
        sessionStorage.removeItem('loginPassword');
        sessionStorage.removeItem('loginRememberMe');

        this.handleSuccessfulLogin();
      }
    } catch (error) {
      this.errorMessage = error instanceof Error ? error.message : 'Помилка входу в систему';
      console.error('Помилка входу:', error);
    } finally {
      this.isLoading = false;
      this.render();
    }
  }

  /**
   * Обробка успішного входу
   */
  private handleSuccessfulLogin(): void {
    console.log('Успішний вхід в систему');

    // Перенаправляємо на головну сторінку
    window.history.pushState(null, '', '/');
    window.dispatchEvent(new PopStateEvent('popstate'));
  }
}

// Реєструємо компонент
customElements.define('login-page', LoginPage);
