import { BaseComponent } from '../components/BaseComponent';

/**
 * Сторінка відновлення пароля (спрощена версія)
 */
export class ForgotPasswordPage extends BaseComponent {
  private emailSent = false;

  constructor() {
    super();
  }

  protected render(): void {
    console.log('ForgotPasswordPage render викликано');
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
        margin-top: 1.5rem;
      }

      .links a {
        color: #007bff;
        text-decoration: none;
        margin: 0 0.5rem;
      }

      .links a:hover {
        text-decoration: underline;
      }

      .success-content {
        text-align: center;
      }

      .success-icon {
        font-size: 3rem;
        margin-bottom: 1rem;
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
    `);

    // Створюємо структуру сторінки
    const container = this.createElement('div', '', 'container');
    
    if (this.emailSent) {
      // Стан після надсилання email
      container.innerHTML = `
        <div class="success-content">
          <div class="success-icon">📧</div>
          <h1>Лист надіслано!</h1>
          <p class="subtitle">
            Ми надіслали інструкції для відновлення пароля на вашу email адресу
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
            <a href="/login">Повернутися до входу</a>
          </div>
        </div>
      `;
    } else {
      // Початкова форма
      container.innerHTML = `
        <h1>🔑 Забули пароль?</h1>
        <p class="subtitle">
          Введіть свою email адресу і ми надішлемо вам інструкції для відновлення пароля
        </p>
        
        <form id="forgot-form">
          <div class="form-group">
            <label for="email">Email адреса</label>
            <input type="email" id="email" name="email" placeholder="your@email.com" required>
          </div>
          
          <button type="submit" class="btn">Надіслати інструкції</button>
        </form>
        
        <div class="links">
          <a href="/login">← Повернутися до входу</a>
          <br><br>
          <span>Немає акаунту? </span>
          <a href="/register">Зареєструватися</a>
        </div>
      `;
    }

    this.shadow.appendChild(styles);
    this.shadow.appendChild(container);
    
    console.log('ForgotPasswordPage render завершено');
  }

  protected setupEventListeners(): void {
    if (!this.emailSent) {
      // Обробка відправки форми
      this.shadow.addEventListener('submit', (event) => {
        event.preventDefault();
        console.log('Форма відновлення пароля відправлена');
        
        const form = event.target as HTMLFormElement;
        const formData = new FormData(form);
        const email = formData.get('email') as string;
        
        console.log('Email:', email);
        
        // Валідація email
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(email)) {
          alert('Будь ласка, введіть коректну email адресу');
          return;
        }
        
        // Змінюємо стан на "лист надіслано"
        this.emailSent = true;
        this.render();
      });
    }
  }
}

// Реєструємо компонент
customElements.define('forgot-password-page', ForgotPasswordPage);
