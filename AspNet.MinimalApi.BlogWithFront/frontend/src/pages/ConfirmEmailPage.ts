import { BaseComponent } from '../components/BaseComponent';

/**
 * Сторінка підтвердження email
 */
export class ConfirmEmailPage extends BaseComponent {
  private isLoading = false;
  private isConfirming = false;
  private errorMessage = '';
  private successMessage = '';
  private email = '';

  constructor() {
    super();
  }

  connectedCallback(): void {
    super.connectedCallback();
    
    // Перевіряємо, чи є токен підтвердження в URL
    const urlParams = new URLSearchParams(window.location.search);
    const token = urlParams.get('token');
    const userId = urlParams.get('userId');
    
    if (token && userId) {
      this.confirmEmail(userId, token);
    }
  }

  protected render(): void {
    this.shadow.innerHTML = '';
    
    // Додаємо стилі
    const styles = this.createStyles(`
      :host {
        display: block;
        max-width: 500px;
        margin: 2rem auto;
        padding: 0 1rem;
      }

      .confirm-card {
        background: white;
        border-radius: 8px;
        box-shadow: 0 4px 6px rgba(0,0,0,0.1);
        padding: 2rem;
        text-align: center;
      }

      .icon {
        font-size: 4rem;
        margin-bottom: 1rem;
      }

      .title {
        font-size: 1.75rem;
        font-weight: bold;
        color: #333;
        margin: 0 0 1rem;
      }

      .subtitle {
        color: #666;
        margin: 0 0 2rem;
        line-height: 1.6;
      }

      .email-highlight {
        color: #007bff;
        font-weight: 500;
      }

      .btn {
        padding: 0.75rem 1.5rem;
        border: none;
        border-radius: 4px;
        font-size: 1rem;
        font-weight: 500;
        cursor: pointer;
        transition: background-color 0.2s;
        margin: 0.5rem;
        text-decoration: none;
        display: inline-block;
      }

      .btn-primary {
        background-color: #007bff;
        color: white;
      }

      .btn-primary:hover:not(:disabled) {
        background-color: #0056b3;
      }

      .btn-secondary {
        background-color: #6c757d;
        color: white;
      }

      .btn-secondary:hover:not(:disabled) {
        background-color: #545b62;
      }

      .btn:disabled {
        opacity: 0.6;
        cursor: not-allowed;
      }

      .error-message {
        background-color: #f8d7da;
        color: #721c24;
        padding: 1rem;
        border-radius: 4px;
        margin: 1rem 0;
        border: 1px solid #f5c6cb;
      }

      .success-message {
        background-color: #d4edda;
        color: #155724;
        padding: 1rem;
        border-radius: 4px;
        margin: 1rem 0;
        border: 1px solid #c3e6cb;
      }

      .loading-spinner {
        display: inline-block;
        width: 2rem;
        height: 2rem;
        border: 3px solid #f3f3f3;
        border-radius: 50%;
        border-top-color: #007bff;
        animation: spin 1s ease-in-out infinite;
        margin: 1rem 0;
      }

      @keyframes spin {
        to { transform: rotate(360deg); }
      }

      .resend-section {
        margin-top: 2rem;
        padding-top: 2rem;
        border-top: 1px solid #e9ecef;
      }

      .resend-text {
        color: #666;
        margin-bottom: 1rem;
      }

      .countdown {
        color: #007bff;
        font-weight: 500;
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
    `);

    // Створюємо структуру сторінки
    const card = this.createElement('div', '', 'confirm-card');
    
    if (this.isConfirming) {
      // Стан підтвердження
      card.innerHTML = `
        <div class="icon">⏳</div>
        <h1 class="title">Підтвердження email...</h1>
        <div class="loading-spinner"></div>
        <p class="subtitle">Будь ласка, зачекайте</p>
      `;
    } else if (this.successMessage) {
      // Успішне підтвердження
      card.innerHTML = `
        <div class="icon">✅</div>
        <h1 class="title">Email підтверджено!</h1>
        <div class="success-message">${this.successMessage}</div>
        <a href="/login" class="btn btn-primary">Увійти в систему</a>
      `;
    } else if (this.errorMessage) {
      // Помилка підтвердження
      card.innerHTML = `
        <div class="icon">❌</div>
        <h1 class="title">Помилка підтвердження</h1>
        <div class="error-message">${this.errorMessage}</div>
        <div class="resend-section">
          <p class="resend-text">Спробуйте надіслати лист повторно:</p>
          <button class="btn btn-primary" id="resend-btn" ${this.isLoading ? 'disabled' : ''}>
            ${this.isLoading ? 'Надсилання...' : 'Надіслати повторно'}
          </button>
        </div>
        <a href="/login" class="btn btn-secondary">Повернутися до входу</a>
      `;
    } else {
      // Початковий стан - очікування підтвердження
      card.innerHTML = `
        <div class="icon">📧</div>
        <h1 class="title">Підтвердіть свій email</h1>
        <p class="subtitle">
          Ми надіслали лист з підтвердженням на адресу 
          <span class="email-highlight">${this.email || 'вашу пошту'}</span>
        </p>
        
        <div class="instructions">
          <h4>Що робити далі:</h4>
          <ul>
            <li>Перевірте свою поштову скриньку</li>
            <li>Знайдіть лист від нашого сервісу</li>
            <li>Натисніть на посилання підтвердження</li>
            <li>Якщо листа немає, перевірте папку "Спам"</li>
          </ul>
        </div>
        
        <div class="resend-section">
          <p class="resend-text">Не отримали лист?</p>
          <button class="btn btn-primary" id="resend-btn" ${this.isLoading ? 'disabled' : ''}>
            ${this.isLoading ? 'Надсилання...' : 'Надіслати повторно'}
          </button>
          <div id="countdown" class="countdown" style="display: none;"></div>
        </div>
        
        <a href="/login" class="btn btn-secondary">Повернутися до входу</a>
      `;
    }

    this.shadow.appendChild(styles);
    this.shadow.appendChild(card);
  }

  protected setupEventListeners(): void {
    // Обробка кліків
    this.shadow.addEventListener('click', this.handleClick.bind(this));
    
    // Отримуємо email з localStorage або URL параметрів
    this.email = localStorage.getItem('registrationEmail') || '';
  }

  /**
   * Обробка кліків
   */
  private handleClick(event: Event): void {
    const target = event.target as HTMLElement;
    
    if (target.id === 'resend-btn' && !this.isLoading) {
      this.resendConfirmationEmail();
    }
  }

  /**
   * Підтвердження email
   */
  private async confirmEmail(_userId: string, _token: string): Promise<void> {
    try {
      this.isConfirming = true;
      this.render();
      
      // TODO: Замінити на реальний API виклик
      await new Promise(resolve => setTimeout(resolve, 2000));
      
      // Симуляція відповіді сервера
      const mockResponse = {
        success: true,
        message: 'Ваш email успішно підтверджено! Тепер ви можете увійти в систему.'
      };
      
      this.successMessage = mockResponse.message;
      
      // Очищуємо localStorage
      localStorage.removeItem('registrationEmail');
      
    } catch (error) {
      this.errorMessage = error instanceof Error ? error.message : 'Помилка підтвердження email';
    } finally {
      this.isConfirming = false;
      this.render();
    }
  }

  /**
   * Повторне надсилання листа підтвердження
   */
  private async resendConfirmationEmail(): Promise<void> {
    try {
      this.isLoading = true;
      this.errorMessage = '';
      this.render();
      
      // TODO: Замінити на реальний API виклик
      await new Promise(resolve => setTimeout(resolve, 1500));

      // Показуємо повідомлення про успіх
      this.showTemporaryMessage('Лист надіслано повторно! Перевірте свою пошту.', 'success');
      
      // Запускаємо таймер для повторного надсилання
      this.startResendCountdown();
      
    } catch (error) {
      this.errorMessage = error instanceof Error ? error.message : 'Помилка надсилання листа';
      this.render();
    } finally {
      this.isLoading = false;
      this.render();
    }
  }

  /**
   * Показати тимчасове повідомлення
   */
  private showTemporaryMessage(message: string, type: 'success' | 'error'): void {
    const messageDiv = this.createElement('div', message, `${type}-message`);
    const card = this.querySelector('.confirm-card');
    
    if (card) {
      card.insertBefore(messageDiv, card.firstChild);
      
      // Видаляємо повідомлення через 5 секунд
      setTimeout(() => {
        if (messageDiv.parentNode) {
          messageDiv.parentNode.removeChild(messageDiv);
        }
      }, 5000);
    }
  }

  /**
   * Запуск таймера для повторного надсилання
   */
  private startResendCountdown(): void {
    const resendBtn = this.querySelector('#resend-btn') as HTMLButtonElement;
    const countdownDiv = this.querySelector('#countdown') as HTMLElement;
    
    if (!resendBtn || !countdownDiv) return;
    
    let seconds = 60;
    resendBtn.disabled = true;
    countdownDiv.style.display = 'block';
    
    const interval = setInterval(() => {
      countdownDiv.textContent = `Повторне надсилання буде доступне через ${seconds} секунд`;
      seconds--;
      
      if (seconds < 0) {
        clearInterval(interval);
        resendBtn.disabled = false;
        countdownDiv.style.display = 'none';
        resendBtn.textContent = 'Надіслати повторно';
      }
    }, 1000);
  }
}

// Реєструємо компонент
customElements.define('confirm-email-page', ConfirmEmailPage);
