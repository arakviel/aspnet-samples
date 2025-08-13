import { BaseComponent } from '../components/BaseComponent';
import type { RegisterRequest } from '../types';

/**
 * Сторінка реєстрації
 */
export class RegisterPage extends BaseComponent {
  private isLoading = false;
  private errorMessage = '';
  private successMessage = '';

  constructor() {
    super();
  }

  protected render(): void {
    this.shadow.innerHTML = '';
    
    // Додаємо стилі (використовуємо ті ж стилі, що й для LoginPage)
    const styles = this.createStyles(`
      :host {
        display: block;
        max-width: 400px;
        margin: 2rem auto;
        padding: 0 1rem;
      }

      .register-card {
        background: white;
        border-radius: 8px;
        box-shadow: 0 4px 6px rgba(0,0,0,0.1);
        padding: 2rem;
      }

      .register-header {
        text-align: center;
        margin-bottom: 2rem;
      }

      .register-title {
        font-size: 1.75rem;
        font-weight: bold;
        color: #333;
        margin: 0 0 0.5rem;
      }

      .register-subtitle {
        color: #666;
        margin: 0;
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

      .field-error {
        color: #dc3545;
        font-size: 0.875rem;
        margin-top: 0.25rem;
      }
    `);

    // Створюємо структуру сторінки
    const card = this.createElement('div', '', 'register-card');
    
    // Заголовок
    const header = this.createElement('div', '', 'register-header');
    header.innerHTML = `
      <h1 class="register-title">📝 Реєстрація</h1>
      <p class="register-subtitle">Створіть новий акаунт</p>
    `;
    
    // Форма
    const form = document.createElement('form');
    form.id = 'register-form';
    
    // Повідомлення
    if (this.errorMessage) {
      const errorDiv = this.createElement('div', this.errorMessage, 'error-message');
      form.appendChild(errorDiv);
    }
    
    if (this.successMessage) {
      const successDiv = this.createElement('div', this.successMessage, 'success-message');
      form.appendChild(successDiv);
    }
    
    // Поля форми
    form.innerHTML += `
      <div class="form-group">
        <label class="form-label" for="name">Ім'я</label>
        <input type="text" id="name" name="name" class="form-control" required>
        <div class="field-error" id="name-error"></div>
      </div>
      
      <div class="form-group">
        <label class="form-label" for="email">Email</label>
        <input type="email" id="email" name="email" class="form-control" required>
        <div class="field-error" id="email-error"></div>
      </div>
      
      <div class="form-group">
        <label class="form-label" for="password">Пароль</label>
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
    
    // Кнопка реєстрації
    const submitBtn = document.createElement('button');
    submitBtn.type = 'submit';
    submitBtn.className = 'btn btn-primary';
    submitBtn.disabled = this.isLoading;
    
    if (this.isLoading) {
      submitBtn.innerHTML = '<span class="loading-spinner"></span>Реєстрація...';
    } else {
      submitBtn.textContent = 'Зареєструватися';
    }
    
    form.appendChild(submitBtn);
    
    // Посилання
    const links = this.createElement('div', '', 'links');
    links.innerHTML = `
      <p>Вже маєте акаунт? <a href="/login" class="link">Увійти</a></p>
    `;

    card.appendChild(header);
    card.appendChild(form);
    card.appendChild(links);

    this.shadow.appendChild(styles);
    this.shadow.appendChild(card);
  }

  protected setupEventListeners(): void {
    // Обробка відправки форми
    this.shadow.addEventListener('submit', this.handleSubmit.bind(this));
    
    // Валідація пароля в реальному часі
    this.shadow.addEventListener('input', this.handleInput.bind(this));
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
    
    // Очищуємо попередні помилки
    this.clearFieldErrors();
    
    try {
      this.isLoading = true;
      this.errorMessage = '';
      this.successMessage = '';
      this.render();
      
      const registerData: RegisterRequest = {
        name: formData.get('name') as string,
        email: formData.get('email') as string,
        password: formData.get('password') as string,
        confirmPassword: formData.get('confirmPassword') as string
      };
      
      // Валідація на клієнті
      if (!this.validateForm(registerData)) {
        return;
      }
      
      await this.performRegistration(registerData);
      
    } catch (error) {
      this.errorMessage = error instanceof Error ? error.message : 'Помилка реєстрації';
    } finally {
      this.isLoading = false;
      this.render();
    }
  }

  /**
   * Валідація форми
   */
  private validateForm(data: RegisterRequest): boolean {
    let isValid = true;
    
    // Валідація імені
    if (!data.name.trim()) {
      this.showFieldError('name', 'Ім\'я обов\'язкове');
      isValid = false;
    }
    
    // Валідація email
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(data.email)) {
      this.showFieldError('email', 'Невірний формат email');
      isValid = false;
    }
    
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
   * Очистити помилки полів
   */
  private clearFieldErrors(): void {
    const errorElements = this.querySelectorAll('.field-error');
    const inputElements = this.querySelectorAll('.form-control');
    
    errorElements.forEach(element => {
      element.textContent = '';
    });
    
    inputElements.forEach(element => {
      element.classList.remove('error');
    });
  }

  /**
   * Виконання реєстрації
   */
  private async performRegistration(registerData: RegisterRequest): Promise<void> {
    // TODO: Замінити на реальний API виклик
    await new Promise(resolve => setTimeout(resolve, 1500));
    
    // Симуляція відповіді сервера
    const mockResponse = {
      success: true,
      message: 'Реєстрація успішна! Перевірте свою пошту для підтвердження email.'
    };
    
    this.successMessage = mockResponse.message;
    this.emit('registration-success', { email: registerData.email });
    
    // Перенаправлення на сторінку підтвердження email через 3 секунди
    setTimeout(() => {
      window.history.pushState(null, '', '/confirm-email');
      document.dispatchEvent(new CustomEvent('navigation', {
        detail: { path: '/confirm-email' }
      }));
    }, 3000);
  }
}

// Реєструємо компонент
customElements.define('register-page', RegisterPage);
