import { BaseComponent } from '../components/BaseComponent';
import { authService } from '../services/AuthService';

/**
 * Сторінка реєстрації
 */
export class RegisterPage extends BaseComponent {
  private isLoading = false;
  private errorMessage = '';

  constructor() {
    super();
  }

  protected render(): void {
    console.log('RegisterPage render викликано');
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
        border-color: #28a745;
        box-shadow: 0 0 0 3px rgba(40, 167, 69, 0.1);
      }

      .btn {
        width: 100%;
        padding: 0.75rem;
        background: #28a745;
        color: white;
        border: none;
        border-radius: 4px;
        font-size: 1rem;
        cursor: pointer;
        margin-top: 1rem;
      }

      .btn:hover {
        background: #218838;
      }

      .links {
        text-align: center;
        margin-top: 1rem;
      }

      .links a {
        color: #007bff;
        text-decoration: none;
      }

      .links a:hover {
        text-decoration: underline;
      }

      .password-hint {
        font-size: 0.875rem;
        color: #666;
        margin-top: 0.25rem;
      }

      .success-message {
        background: #d4edda;
        color: #155724;
        padding: 1rem;
        border-radius: 4px;
        margin-bottom: 1rem;
        border: 1px solid #c3e6cb;
      }
    `);

    // Створюємо структуру сторінки
    const container = this.createElement('div', '', 'container');
    
    container.innerHTML = `
      <h1>📝 Реєстрація</h1>

      ${this.errorMessage ? `<div style="background: #f8d7da; color: #721c24; padding: 0.75rem; border-radius: 4px; margin-bottom: 1rem; border: 1px solid #f5c6cb;">${this.errorMessage}</div>` : ''}

      <form id="register-form">
        <div class="form-group">
          <label for="name">Ім'я</label>
          <input type="text" id="name" name="name" required>
        </div>

        <div class="form-group">
          <label for="email">Email</label>
          <input type="email" id="email" name="email" required>
        </div>

        <div class="form-group">
          <label for="password">Пароль</label>
          <input type="password" id="password" name="password" required>
          <div class="password-hint">Мінімум 8 символів, включаючи цифри та літери</div>
        </div>

        <div class="form-group">
          <label for="confirmPassword">Підтвердження пароля</label>
          <input type="password" id="confirmPassword" name="confirmPassword" required>
        </div>

        <button type="submit" class="btn" ${this.isLoading ? 'disabled' : ''}>
          ${this.isLoading ? '⏳ Реєстрація...' : 'Зареєструватися'}
        </button>
      </form>

      <div class="links">
        <p>Вже маєте акаунт? <a href="/login">Увійти</a></p>
      </div>
    `;

    this.shadow.appendChild(styles);
    this.shadow.appendChild(container);
    
    console.log('RegisterPage render завершено');
  }

  protected setupEventListeners(): void {
    // Обробка відправки форми
    this.shadow.addEventListener('submit', (event) => {
      event.preventDefault();
      this.handleSubmit(event);
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

      const name = formData.get('name') as string;
      const email = formData.get('email') as string;
      const password = formData.get('password') as string;
      const confirmPassword = formData.get('confirmPassword') as string;

      // Валідація на клієнті
      if (password !== confirmPassword) {
        this.errorMessage = 'Паролі не співпадають!';
        return;
      }

      if (password.length < 8) {
        this.errorMessage = 'Пароль повинен містити мінімум 8 символів';
        return;
      }

      // Реєстрація через API
      await authService.register(name, email, password, confirmPassword);

      // Показуємо повідомлення про успіх
      const container = this.shadowQuery('.container');
      if (container) {
        const successDiv = document.createElement('div');
        successDiv.className = 'success-message';
        successDiv.innerHTML = '✅ Реєстрація успішна! Перевірте свою пошту для підтвердження email.';
        container.insertBefore(successDiv, container.firstChild);

        // Перенаправляємо на сторінку підтвердження через 3 секунди
        setTimeout(() => {
          window.history.pushState(null, '', '/confirm-email');
          window.dispatchEvent(new PopStateEvent('popstate'));
        }, 3000);
      }

    } catch (error) {
      this.errorMessage = error instanceof Error ? error.message : 'Помилка реєстрації';
      console.error('Помилка реєстрації:', error);
    } finally {
      this.isLoading = false;
      this.render();
    }
  }
}

// Реєструємо компонент
customElements.define('register-page', RegisterPage);
