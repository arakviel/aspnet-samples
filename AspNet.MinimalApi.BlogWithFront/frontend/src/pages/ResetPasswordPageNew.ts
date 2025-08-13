import { BaseComponent } from '../components/BaseComponent';

/**
 * Сторінка скидання пароля (спрощена версія)
 */
export class ResetPasswordPage extends BaseComponent {
  private token = '';
  private email = '';
  private isSuccess = false;

  constructor() {
    super();
  }

  connectedCallback(): void {
    super.connectedCallback();
    
    // Отримуємо токен та email з URL параметрів
    const urlParams = new URLSearchParams(window.location.search);
    this.token = urlParams.get('token') || '';
    this.email = urlParams.get('email') || '';
    
    console.log('ResetPasswordPage - token:', this.token, 'email:', this.email);
  }

  protected render(): void {
    console.log('ResetPasswordPage render викликано');
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
        margin: 0 0 1rem;
      }

      .subtitle {
        text-align: center;
        color: #666;
        margin: 0 0 2rem;
        line-height: 1.5;
      }

      .email-info {
        background: #e3f2fd;
        color: #1976d2;
        padding: 0.75rem;
        border-radius: 4px;
        margin-bottom: 1.5rem;
        font-size: 0.9rem;
        text-align: center;
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

      .password-requirements {
        background: #f8f9fa;
        border: 1px solid #e9ecef;
        border-radius: 4px;
        padding: 0.75rem;
        margin-top: 0.5rem;
        font-size: 0.875rem;
        color: #666;
      }

      .password-requirements ul {
        margin: 0.5rem 0 0;
        padding-left: 1.25rem;
      }

      .password-requirements li {
        margin-bottom: 0.25rem;
      }

      .requirement-met {
        color: #28a745;
      }

      .requirement-unmet {
        color: #dc3545;
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

      .error-message {
        background: #f8d7da;
        color: #721c24;
        padding: 0.75rem;
        border-radius: 4px;
        margin-bottom: 1rem;
        border: 1px solid #f5c6cb;
      }

      .success-content {
        text-align: center;
      }

      .success-icon {
        font-size: 3rem;
        margin-bottom: 1rem;
      }

      .success-message {
        background: #d4edda;
        color: #155724;
        padding: 1rem;
        border-radius: 4px;
        margin: 1rem 0;
        border: 1px solid #c3e6cb;
      }

      .field-error {
        color: #dc3545;
        font-size: 0.875rem;
        margin-top: 0.25rem;
      }
    `);

    // Створюємо структуру сторінки
    const container = this.createElement('div', '', 'container');
    
    if (!this.token || !this.email) {
      // Помилка з посиланням
      container.innerHTML = `
        <h1>❌ Помилка</h1>
        <div class="error-message">Невірне посилання для скидання пароля</div>
        <div class="links">
          <a href="/forgot-password">Запросити нове посилання</a>
          <br><br>
          <a href="/login">Повернутися до входу</a>
        </div>
      `;
    } else if (this.isSuccess) {
      // Успішне скидання
      container.innerHTML = `
        <div class="success-content">
          <div class="success-icon">✅</div>
          <h1>Пароль змінено!</h1>
          <div class="success-message">
            Пароль успішно змінено! Тепер ви можете увійти в систему з новим паролем.
          </div>
          <a href="/login" class="btn">Увійти в систему</a>
        </div>
      `;
    } else {
      // Форма скидання пароля
      container.innerHTML = `
        <h1>🔑 Новий пароль</h1>
        <p class="subtitle">Створіть новий пароль для свого акаунту</p>
        
        <div class="email-info">
          📧 Скидання пароля для: <strong>${this.email}</strong>
        </div>
        
        <form id="reset-form">
          <div class="form-group">
            <label for="password">Новий пароль</label>
            <input type="password" id="password" name="password" required>
            <div class="password-requirements">
              <strong>Вимоги до пароля:</strong>
              <ul>
                <li id="req-length" class="requirement-unmet">Мінімум 8 символів</li>
                <li id="req-uppercase" class="requirement-unmet">Одна велика літера</li>
                <li id="req-lowercase" class="requirement-unmet">Одна мала літера</li>
                <li id="req-number" class="requirement-unmet">Одна цифра</li>
                <li id="req-special" class="requirement-unmet">Один спеціальний символ</li>
              </ul>
            </div>
            <div class="field-error" id="password-error"></div>
          </div>
          
          <div class="form-group">
            <label for="confirmPassword">Підтвердження пароля</label>
            <input type="password" id="confirmPassword" name="confirmPassword" required>
            <div class="field-error" id="confirmPassword-error"></div>
          </div>
          
          <button type="submit" class="btn">Змінити пароль</button>
        </form>
        
        <div class="links">
          <a href="/login">← Повернутися до входу</a>
        </div>
      `;
    }

    this.shadow.appendChild(styles);
    this.shadow.appendChild(container);
    
    console.log('ResetPasswordPage render завершено');
  }

  protected setupEventListeners(): void {
    if (!this.isSuccess && this.token && this.email) {
      // Обробка відправки форми
      this.shadow.addEventListener('submit', (event) => {
        event.preventDefault();
        this.handleSubmit(event);
      });
      
      // Валідація пароля в реальному часі
      this.shadow.addEventListener('input', (event) => {
        const target = event.target as HTMLInputElement;
        
        if (target.name === 'password') {
          this.validatePassword(target.value);
        } else if (target.name === 'confirmPassword') {
          this.validatePasswordConfirmation(target.value);
        }
      });
    }
  }

  /**
   * Обробка відправки форми
   */
  private handleSubmit(event: Event): void {
    const form = event.target as HTMLFormElement;
    const formData = new FormData(form);
    
    const password = formData.get('password') as string;
    const confirmPassword = formData.get('confirmPassword') as string;
    
    // Валідація
    if (!this.isPasswordValid(password)) {
      this.showFieldError('password', 'Пароль не відповідає вимогам');
      return;
    }
    
    if (password !== confirmPassword) {
      this.showFieldError('confirmPassword', 'Паролі не співпадають');
      return;
    }
    
    console.log('Скидання пароля для:', this.email);
    console.log('Токен:', this.token);
    console.log('Новий пароль:', password);
    
    // Тут буде API виклик
    // Симулюємо успішне скидання
    this.isSuccess = true;
    this.render();
  }

  /**
   * Валідація пароля
   */
  private validatePassword(password: string): void {
    const requirements = {
      length: password.length >= 8,
      uppercase: /[A-Z]/.test(password),
      lowercase: /[a-z]/.test(password),
      number: /\d/.test(password),
      special: /[!@#$%^&*(),.?":{}|<>]/.test(password)
    };

    // Оновлюємо візуальні індикатори
    Object.entries(requirements).forEach(([key, met]) => {
      const element = this.shadowQuery(`#req-${key}`);
      if (element) {
        element.className = met ? 'requirement-met' : 'requirement-unmet';
      }
    });
  }

  /**
   * Валідація підтвердження пароля
   */
  private validatePasswordConfirmation(confirmPassword: string): void {
    const passwordInput = this.shadowQuery('#password') as HTMLInputElement;
    const errorElement = this.shadowQuery('#confirmPassword-error');
    
    if (passwordInput && errorElement) {
      if (confirmPassword && confirmPassword !== passwordInput.value) {
        errorElement.textContent = 'Паролі не співпадають';
      } else {
        errorElement.textContent = '';
      }
    }
  }

  /**
   * Перевірка валідності пароля
   */
  private isPasswordValid(password: string): boolean {
    return password.length >= 8 &&
           /[A-Z]/.test(password) &&
           /[a-z]/.test(password) &&
           /\d/.test(password) &&
           /[!@#$%^&*(),.?":{}|<>]/.test(password);
  }

  /**
   * Показати помилку поля
   */
  private showFieldError(fieldName: string, message: string): void {
    const errorElement = this.shadowQuery(`#${fieldName}-error`);
    const inputElement = this.shadowQuery(`#${fieldName}`) as HTMLInputElement;
    
    if (errorElement) {
      errorElement.textContent = message;
    }
    
    if (inputElement) {
      inputElement.style.borderColor = '#dc3545';
    }
  }
}

// Реєструємо компонент
customElements.define('reset-password-page', ResetPasswordPage);
