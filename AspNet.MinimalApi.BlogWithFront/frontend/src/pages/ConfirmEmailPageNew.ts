import { BaseComponent } from '../components/BaseComponent';

/**
 * Сторінка підтвердження email (спрощена версія)
 */
export class ConfirmEmailPage extends BaseComponent {
  constructor() {
    super();
  }

  protected render(): void {
    console.log('ConfirmEmailPage render викликано');
    this.shadow.innerHTML = '';
    
    // Додаємо стилі
    const styles = this.createStyles(`
      :host {
        display: block;
        padding: 2rem;
      }

      .container {
        max-width: 500px;
        margin: 0 auto;
        background: white;
        padding: 2rem;
        border-radius: 8px;
        box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        text-align: center;
      }

      .icon {
        font-size: 4rem;
        margin-bottom: 1rem;
      }

      h1 {
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

      .instructions {
        background: #f8f9fa;
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

      .btn {
        padding: 0.75rem 1.5rem;
        background: #007bff;
        color: white;
        border: none;
        border-radius: 4px;
        font-size: 1rem;
        cursor: pointer;
        text-decoration: none;
        display: inline-block;
        margin: 0.5rem;
      }

      .btn:hover {
        background: #0056b3;
      }

      .btn-secondary {
        background: #6c757d;
      }

      .btn-secondary:hover {
        background: #545b62;
      }

      .success-message {
        background: #d4edda;
        color: #155724;
        padding: 1rem;
        border-radius: 4px;
        margin: 1rem 0;
        border: 1px solid #c3e6cb;
      }

      .countdown {
        color: #007bff;
        font-weight: 500;
        margin-top: 1rem;
      }
    `);

    // Перевіряємо, чи є токен підтвердження в URL
    const urlParams = new URLSearchParams(window.location.search);
    const token = urlParams.get('token');
    const userId = urlParams.get('userId');

    // Створюємо структуру сторінки
    const container = this.createElement('div', '', 'container');
    
    if (token && userId) {
      // Показуємо успішне підтвердження
      container.innerHTML = `
        <div class="icon">✅</div>
        <h1>Email підтверджено!</h1>
        <div class="success-message">
          Ваш email успішно підтверджено! Тепер ви можете увійти в систему.
        </div>
        <a href="/login" class="btn">Увійти в систему</a>
      `;
    } else {
      // Показуємо інструкції для підтвердження
      container.innerHTML = `
        <div class="icon">📧</div>
        <h1>Підтвердіть свій email</h1>
        <p class="subtitle">
          Ми надіслали лист з підтвердженням на вашу пошту
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
        
        <button class="btn" id="resend-btn">Надіслати повторно</button>
        <a href="/login" class="btn btn-secondary">Повернутися до входу</a>
        
        <div class="countdown" id="countdown" style="display: none;"></div>
      `;
    }

    this.shadow.appendChild(styles);
    this.shadow.appendChild(container);
    
    console.log('ConfirmEmailPage render завершено');
  }

  protected setupEventListeners(): void {
    // Обробка кнопки повторного надсилання
    this.shadow.addEventListener('click', (event) => {
      const target = event.target as HTMLElement;
      
      if (target.id === 'resend-btn') {
        console.log('Повторне надсилання листа');
        
        // Показуємо повідомлення
        const container = this.shadowQuery('.container');
        if (container) {
          const successDiv = document.createElement('div');
          successDiv.className = 'success-message';
          successDiv.innerHTML = '📧 Лист надіслано повторно! Перевірте свою пошту.';
          container.insertBefore(successDiv, container.firstChild);
          
          // Запускаємо таймер
          this.startCountdown();
          
          // Видаляємо повідомлення через 5 секунд
          setTimeout(() => {
            if (successDiv.parentNode) {
              successDiv.parentNode.removeChild(successDiv);
            }
          }, 5000);
        }
      }
    });
  }

  /**
   * Запуск таймера для повторного надсилання
   */
  private startCountdown(): void {
    const resendBtn = this.shadowQuery('#resend-btn') as HTMLButtonElement;
    const countdownDiv = this.shadowQuery('#countdown') as HTMLElement;
    
    if (!resendBtn || !countdownDiv) return;
    
    let seconds = 60;
    resendBtn.disabled = true;
    resendBtn.textContent = 'Надсилання...';
    countdownDiv.style.display = 'block';
    
    const interval = setInterval(() => {
      countdownDiv.textContent = `Повторне надсилання буде доступне через ${seconds} секунд`;
      seconds--;
      
      if (seconds < 0) {
        clearInterval(interval);
        resendBtn.disabled = false;
        resendBtn.textContent = 'Надіслати повторно';
        countdownDiv.style.display = 'none';
      }
    }, 1000);
  }
}

// Реєструємо компонент
customElements.define('confirm-email-page', ConfirmEmailPage);
