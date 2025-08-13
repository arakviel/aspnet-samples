import { BaseComponent } from '../components/BaseComponent';
import type { ForgotPasswordRequest } from '../types';

/**
 * Сторінка відновлення пароля
 */
export class ForgotPasswordPage extends BaseComponent {
  private isLoading = false;
  private errorMessage = '';
  private emailSent = false;

  constructor() {
    super();
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

      .forgot-card {
        background: white;
        border-radius: 8px;
        box-shadow: 0 4px 6px rgba(0,0,0,0.1);
        padding: 2rem;
      }

      .forgot-header {
        text-align: center;
        margin-bottom: 2rem;
      }

      .forgot-title {
        font-size: 1.75rem;
        font-weight: bold;
        color: #333;
        margin: 0 0 0.5rem;
      }

      .forgot-subtitle {
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
        background-color: #007bff;
        color: white;
      }

      .btn-primary:hover:not(:disabled) {
        background-color: #0056b3;
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

      .success-content {
        text-align: center;
      }

      .success-icon {
        font-size: 3rem;
        margin-bottom: 1rem;
      }

      .instructions {
        background-color: #f8f9fa;
        border: 1px solid #e9ecef;
        border-radius: 4px;
        padding: 1rem;
        margin: 1rem 0;
        text-align: left;
      }

      .instructions h4 {
        margin: 0 0 0.5rem;
        color: #333;
      }

      .instructions ul {
        margin: 0.5rem 0 0;
        padding-left: 1.25rem;
      }

      .instructions li {
        margin-bottom: 0.25rem;
        color: #666;
      }

      .email-highlight {
        color: #007bff;
        font-weight: 500;
      }
    `);

    // Створюємо структуру сторінки
    const card = this.createElement('div', '', 'forgot-card');
    
    if (this.emailSent) {
      // Стан після надсилання email
      card.innerHTML = `
        <div class="success-content">
          <div class="success-icon">📧</div>
          <h1 class="forgot-title">Лист надіслано!</h1>
          <p class="forgot-subtitle">
            Ми надіслали інструкції для відновлення пароля на адресу 
            <span class="email-highlight">${this.getEmailFromForm()}</span>
          </p>
          
          <div class="instructions">
            <h4>Що робити далі:</h4>
            <ul>
              <li>Перевірте свою поштову скриньку</li>
              <li>Знайдіть лист з темою "Відновлення пароля"</li>
              <li>Натисніть на посилання в листі</li>
              <li>Створіть новий пароль</li>
              <li>Якщо листа немає, перевірте папку "Спам"</li>
            </ul>
          </div>
          
          <div class="links">
            <a href="/login" class="link">Повернутися до входу</a>
          </div>
        </div>
      `;
    } else {
      // Початкова форма
      const header = this.createElement('div', '', 'forgot-header');
      header.innerHTML = `
        <h1 class="forgot-title">🔑 Забули пароль?</h1>
        <p class="forgot-subtitle">
          Введіть свою email адресу і ми надішлемо вам інструкції для відновлення пароля
        </p>
      `;
      
      const form = document.createElement('form');
      form.id = 'forgot-form';
      
      // Повідомлення про помилку
      if (this.errorMessage) {
        const errorDiv = this.createElement('div', this.errorMessage, 'error-message');
        form.appendChild(errorDiv);
      }
      
      // Поле email
      form.innerHTML += `
        <div class="form-group">
          <label class="form-label" for="email">Email адреса</label>
          <input type="email" id="email" name="email" class="form-control" 
                 placeholder="your@email.com" required>
        </div>
      `;
      
      // Кнопка відправки
      const submitBtn = document.createElement('button');
      submitBtn.type = 'submit';
      submitBtn.className = 'btn btn-primary';
      submitBtn.disabled = this.isLoading;
      
      if (this.isLoading) {
        submitBtn.innerHTML = '<span class="loading-spinner"></span>Надсилання...';
      } else {
        submitBtn.textContent = 'Надіслати інструкції';
      }
      
      form.appendChild(submitBtn);
      
      // Посилання
      const links = this.createElement('div', '', 'links');
      links.innerHTML = `
        <a href="/login" class="link">← Повернутися до входу</a>
        <br><br>
        <span>Немає акаунту? </span>
        <a href="/register" class="link">Зареєструватися</a>
      `;

      card.appendChild(header);
      card.appendChild(form);
      card.appendChild(links);
    }

    this.shadow.appendChild(styles);
    this.shadow.appendChild(card);
  }

  protected setupEventListeners(): void {
    if (!this.emailSent) {
      // Обробка відправки форми
      this.shadow.addEventListener('submit', this.handleSubmit.bind(this));
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
      
      const forgotData: ForgotPasswordRequest = {
        email: formData.get('email') as string
      };
      
      // Валідація email
      if (!this.validateEmail(forgotData.email)) {
        this.errorMessage = 'Будь ласка, введіть коректну email адресу';
        return;
      }
      
      await this.sendResetEmail(forgotData);
      
    } catch (error) {
      this.errorMessage = error instanceof Error ? error.message : 'Помилка надсилання листа';
    } finally {
      this.isLoading = false;
      this.render();
    }
  }

  /**
   * Валідація email
   */
  private validateEmail(email: string): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }

  /**
   * Надсилання листа для відновлення пароля
   */
  private async sendResetEmail(forgotData: ForgotPasswordRequest): Promise<void> {
    // TODO: Замінити на реальний API виклик
    await new Promise(resolve => setTimeout(resolve, 1500));

    // Зберігаємо email для відображення
    this.storeEmailForDisplay(forgotData.email);
    
    this.emailSent = true;

    this.emit('password-reset-requested', { email: forgotData.email });
  }

  /**
   * Збереження email для відображення
   */
  private storeEmailForDisplay(email: string): void {
    // Зберігаємо в sessionStorage для використання на цій сторінці
    sessionStorage.setItem('resetEmail', email);
  }

  /**
   * Отримання email з форми або sessionStorage
   */
  private getEmailFromForm(): string {
    const storedEmail = sessionStorage.getItem('resetEmail');
    if (storedEmail) {
      return storedEmail;
    }
    
    const emailInput = this.querySelector('#email') as HTMLInputElement;
    return emailInput ? emailInput.value : '';
  }
}

// Реєструємо компонент
customElements.define('forgot-password-page', ForgotPasswordPage);
