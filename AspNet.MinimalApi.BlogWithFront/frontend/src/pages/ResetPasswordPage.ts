import { BaseComponent } from '../components/BaseComponent';
import type { ResetPasswordRequest } from '../types';

/**
 * Сторінка скидання пароля
 */
export class ResetPasswordPage extends BaseComponent {
  private isLoading = false;
  private errorMessage = '';
  private successMessage = '';
  private token = '';
  private email = '';

  constructor() {
    super();
  }

  connectedCallback(): void {
    super.connectedCallback();
    
    // Отримуємо токен та email з URL параметрів
    const urlParams = new URLSearchParams(window.location.search);
    this.token = urlParams.get('token') || '';
    this.email = urlParams.get('email') || '';
    
    // Якщо немає токена або email, перенаправляємо на сторінку забутого пароля
    if (!this.token || !this.email) {
      this.errorMessage = 'Невірне посилання для скидання пароля';
    }
  }

  protected render(): void {
    this.shadow.innerHTML = '';
    
    // Додаємо стилі
    const styles = this.createStyles(`
      :host {
        display: block;
        max-width: 400px;
        margin: 2rem auto;
        padding: 0 1rem;
      }

      .reset-card {
        background: white;
        border-radius: 8px;
        box-shadow: 0 4px 6px rgba(0,0,0,0.1);
        padding: 2rem;
      }

      .reset-header {
        text-align: center;
        margin-bottom: 2rem;
      }

      .reset-title {
        font-size: 1.75rem;
        font-weight: bold;
        color: #333;
        margin: 0 0 0.5rem;
      }

      .reset-subtitle {
        color: #666;
        margin: 0;
        line-height: 1.5;
      }

      .form-group {
        margin-bottom: 1.5rem;
      }

      .form-label {
        display: block;
        margin-bottom: 0.5rem;
        font-weight: 500;
        color: #333;
      }

      .form-control {
        width: 100%;
        padding: 0.75rem;
        border: 1px solid #ddd;
        border-radius: 4px;
        font-size: 1rem;
        transition: border-color 0.2s, box-shadow 0.2s;
      }

      .form-control:focus {
        outline: none;
        border-color: #007bff;
        box-shadow: 0 0 0 3px rgba(0, 123, 255, 0.1);
      }

      .form-control.error {
        border-color: #dc3545;
      }

      .btn {
        width: 100%;
        padding: 0.75rem;
        border: none;
        border-radius: 4px;
        font-size: 1rem;
        font-weight: 500;
        cursor: pointer;
        transition: background-color 0.2s;
        margin-bottom: 1rem;
      }

      .btn-primary {
        background-color: #28a745;
        color: white;
      }

      .btn-primary:hover:not(:disabled) {
        background-color: #218838;
      }

      .btn:disabled {
        opacity: 0.6;
        cursor: not-allowed;
      }

      .error-message {
        background-color: #f8d7da;
        color: #721c24;
        padding: 0.75rem;
        border-radius: 4px;
        margin-bottom: 1rem;
        border: 1px solid #f5c6cb;
      }

      .success-message {
        background-color: #d4edda;
        color: #155724;
        padding: 0.75rem;
        border-radius: 4px;
        margin-bottom: 1rem;
        border: 1px solid #c3e6cb;
      }

      .links {
        text-align: center;
        margin-top: 1.5rem;
      }

      .link {
        color: #007bff;
        text-decoration: none;
        margin: 0 0.5rem;
      }

      .link:hover {
        text-decoration: underline;
      }

      .loading-spinner {
        display: inline-block;
        width: 1rem;
        height: 1rem;
        border: 2px solid #ffffff;
        border-radius: 50%;
        border-top-color: transparent;
        animation: spin 1s ease-in-out infinite;
        margin-right: 0.5rem;
      }

      @keyframes spin {
        to { transform: rotate(360deg); }
      }

      .password-requirements {
        background-color: #f8f9fa;
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

      .field-error {
        color: #dc3545;
        font-size: 0.875rem;
        margin-top: 0.25rem;
      }

      .success-content {
        text-align: center;
      }

      .success-icon {
        font-size: 3rem;
        margin-bottom: 1rem;
      }

      .email-info {
        background-color: #e3f2fd;
        color: #1976d2;
        padding: 0.75rem;
        border-radius: 4px;
        margin-bottom: 1.5rem;
        font-size: 0.9rem;
      }
    `);

    // Створюємо структуру сторінки
    const card = this.createElement('div', '', 'reset-card');
    
    if (this.successMessage) {
      // Стан успішного скидання
      card.innerHTML = `
        <div class="success-content">
          <div class="success-icon">✅</div>
          <h1 class="reset-title">Пароль змінено!</h1>
          <div class="success-message">${this.successMessage}</div>
          <a href="/login" class="btn btn-primary">Увійти в систему</a>
        </div>
      `;
    } else if (this.errorMessage && (!this.token || !this.email)) {
      // Помилка з посиланням
      card.innerHTML = `
        <div class="reset-header">
          <h1 class="reset-title">❌ Помилка</h1>
          <div class="error-message">${this.errorMessage}</div>
          <div class="links">
            <a href="/forgot-password" class="link">Запросити нове посилання</a>
            <br><br>
            <a href="/login" class="link">Повернутися до входу</a>
          </div>
        </div>
      `;
    } else {
      // Форма скидання пароля
      const header = this.createElement('div', '', 'reset-header');
      header.innerHTML = `
        <h1 class="reset-title">🔑 Новий пароль</h1>
        <p class="reset-subtitle">Створіть новий пароль для свого акаунту</p>
      `;
      
      // Інформація про email
      const emailInfo = this.createElement('div', '', 'email-info');
      emailInfo.innerHTML = `📧 Скидання пароля для: <strong>${this.email}</strong>`;
      
      const form = document.createElement('form');
      form.id = 'reset-form';
      
      // Повідомлення про помилку
      if (this.errorMessage) {
        const errorDiv = this.createElement('div', this.errorMessage, 'error-message');
        form.appendChild(errorDiv);
      }
      
      // Поля форми
      form.innerHTML += `
        <div class="form-group">
          <label class="form-label" for="password">Новий пароль</label>
          <input type="password" id="password" name="password" class="form-control" required>
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
          <label class="form-label" for="confirmPassword">Підтвердження пароля</label>
          <input type="password" id="confirmPassword" name="confirmPassword" class="form-control" required>
          <div class="field-error" id="confirmPassword-error"></div>
        </div>
      `;
      
      // Кнопка скидання
      const submitBtn = document.createElement('button');
      submitBtn.type = 'submit';
      submitBtn.className = 'btn btn-primary';
      submitBtn.disabled = this.isLoading;
      
      if (this.isLoading) {
        submitBtn.innerHTML = '<span class="loading-spinner"></span>Збереження...';
      } else {
        submitBtn.textContent = 'Змінити пароль';
      }
      
      form.appendChild(submitBtn);
      
      // Посилання
      const links = this.createElement('div', '', 'links');
      links.innerHTML = `
        <a href="/login" class="link">← Повернутися до входу</a>
      `;

      card.appendChild(header);
      card.appendChild(emailInfo);
      card.appendChild(form);
      card.appendChild(links);
    }

    this.shadow.appendChild(styles);
    this.shadow.appendChild(card);
  }

  protected setupEventListeners(): void {
    if (!this.successMessage && this.token && this.email) {
      // Обробка відправки форми
      this.shadow.addEventListener('submit', this.handleSubmit.bind(this));
      
      // Валідація пароля в реальному часі
      this.shadow.addEventListener('input', this.handleInput.bind(this));
    }
  }

  /**
   * Обробка введення в поля
   */
  private handleInput(event: Event): void {
    const target = event.target as HTMLInputElement;
    
    if (target.name === 'password') {
      this.validatePassword(target.value);
    } else if (target.name === 'confirmPassword') {
      this.validatePasswordConfirmation(target.value);
    }
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
      const element = this.querySelector(`#req-${key}`);
      if (element) {
        element.className = met ? 'requirement-met' : 'requirement-unmet';
      }
    });
  }

  /**
   * Валідація підтвердження пароля
   */
  private validatePasswordConfirmation(confirmPassword: string): void {
    const passwordInput = this.querySelector('#password') as HTMLInputElement;
    const errorElement = this.querySelector('#confirmPassword-error');
    
    if (passwordInput && errorElement) {
      if (confirmPassword && confirmPassword !== passwordInput.value) {
        errorElement.textContent = 'Паролі не співпадають';
      } else {
        errorElement.textContent = '';
      }
    }
  }

  /**
   * Обробка відправки форми
   */
  private async handleSubmit(event: Event): Promise<void> {
    event.preventDefault();
    
    if (this.isLoading) return;
    
    const form = event.target as HTMLFormElement;
    const formData = new FormData(form);
    
    try {
      this.isLoading = true;
      this.errorMessage = '';
      this.render();
      
      const resetData: ResetPasswordRequest = {
        email: this.email,
        token: this.token,
        password: formData.get('password') as string,
        confirmPassword: formData.get('confirmPassword') as string
      };
      
      // Валідація на клієнті
      if (!this.validateForm(resetData)) {
        return;
      }
      
      await this.performPasswordReset(resetData);
      
    } catch (error) {
      this.errorMessage = error instanceof Error ? error.message : 'Помилка скидання пароля';
    } finally {
      this.isLoading = false;
      this.render();
    }
  }

  /**
   * Валідація форми
   */
  private validateForm(data: ResetPasswordRequest): boolean {
    let isValid = true;
    
    // Валідація пароля
    if (data.password.length < 8) {
      this.showFieldError('password', 'Пароль повинен містити мінімум 8 символів');
      isValid = false;
    }
    
    // Валідація підтвердження пароля
    if (data.password !== data.confirmPassword) {
      this.showFieldError('confirmPassword', 'Паролі не співпадають');
      isValid = false;
    }
    
    return isValid;
  }

  /**
   * Показати помилку поля
   */
  private showFieldError(fieldName: string, message: string): void {
    const errorElement = this.querySelector(`#${fieldName}-error`);
    const inputElement = this.querySelector(`#${fieldName}`) as HTMLInputElement;
    
    if (errorElement) {
      errorElement.textContent = message;
    }
    
    if (inputElement) {
      inputElement.classList.add('error');
    }
  }

  /**
   * Виконання скидання пароля
   */
  private async performPasswordReset(resetData: ResetPasswordRequest): Promise<void> {
    // TODO: Замінити на реальний API виклик
    await new Promise(resolve => setTimeout(resolve, 1500));
    
    // Симуляція відповіді сервера
    const mockResponse = {
      success: true,
      message: 'Пароль успішно змінено! Тепер ви можете увійти в систему з новим паролем.'
    };
    
    this.successMessage = mockResponse.message;
    this.emit('password-reset-success', { email: resetData.email });
  }
}

// Реєструємо компонент
customElements.define('reset-password-page', ResetPasswordPage);
