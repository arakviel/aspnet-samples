import { BaseComponent } from '../components/BaseComponent';

/**
 * Сторінка профілю користувача
 */
export class ProfilePage extends BaseComponent {
  private twoFactorEnabled = false;

  constructor() {
    super();
  }

  protected render(): void {
    console.log('ProfilePage render викликано');
    this.shadow.innerHTML = '';
    
    // Додаємо стилі
    const styles = this.createStyles(`
      :host {
        display: block;
        padding: 2rem;
      }

      .container {
        max-width: 800px;
        margin: 0 auto;
      }

      .profile-header {
        background: white;
        padding: 2rem;
        border-radius: 8px;
        box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        margin-bottom: 2rem;
        text-align: center;
      }

      .avatar {
        width: 100px;
        height: 100px;
        border-radius: 50%;
        background: #007bff;
        color: white;
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 2rem;
        margin: 0 auto 1rem;
      }

      .profile-name {
        font-size: 1.5rem;
        font-weight: bold;
        color: #333;
        margin: 0 0 0.5rem;
      }

      .profile-email {
        color: #666;
        margin: 0;
      }

      .profile-section {
        background: white;
        padding: 2rem;
        border-radius: 8px;
        box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        margin-bottom: 2rem;
      }

      .section-title {
        font-size: 1.25rem;
        font-weight: bold;
        color: #333;
        margin: 0 0 1rem;
        padding-bottom: 0.5rem;
        border-bottom: 1px solid #e9ecef;
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
        padding: 0.75rem 1.5rem;
        border: none;
        border-radius: 4px;
        font-size: 1rem;
        cursor: pointer;
        margin-right: 0.5rem;
        margin-bottom: 0.5rem;
      }

      .btn-primary {
        background: #007bff;
        color: white;
      }

      .btn-primary:hover {
        background: #0056b3;
      }

      .btn-success {
        background: #28a745;
        color: white;
      }

      .btn-success:hover {
        background: #218838;
      }

      .btn-danger {
        background: #dc3545;
        color: white;
      }

      .btn-danger:hover {
        background: #c82333;
      }

      .security-item {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 1rem;
        border: 1px solid #e9ecef;
        border-radius: 4px;
        margin-bottom: 1rem;
      }

      .security-info {
        flex: 1;
      }

      .security-title {
        font-weight: 500;
        color: #333;
        margin: 0 0 0.25rem;
      }

      .security-description {
        color: #666;
        font-size: 0.875rem;
        margin: 0;
      }

      .status-badge {
        padding: 0.25rem 0.75rem;
        border-radius: 12px;
        font-size: 0.875rem;
        font-weight: 500;
        margin-right: 1rem;
      }

      .status-enabled {
        background: #d4edda;
        color: #155724;
      }

      .status-disabled {
        background: #f8d7da;
        color: #721c24;
      }

      .qr-code {
        text-align: center;
        padding: 2rem;
        background: #f8f9fa;
        border-radius: 4px;
        margin: 1rem 0;
      }

      .qr-placeholder {
        width: 200px;
        height: 200px;
        background: white;
        border: 2px dashed #ddd;
        display: flex;
        align-items: center;
        justify-content: center;
        margin: 0 auto 1rem;
        color: #666;
      }

      .manual-key {
        background: #f8f9fa;
        padding: 0.75rem;
        border-radius: 4px;
        font-family: monospace;
        word-break: break-all;
        margin: 1rem 0;
      }
    `);

    // Створюємо структуру сторінки
    const container = this.createElement('div', '', 'container');
    
    // Заголовок профілю
    const header = this.createElement('div', '', 'profile-header');
    header.innerHTML = `
      <div class="avatar">👤</div>
      <h1 class="profile-name">Тестовий Користувач</h1>
      <p class="profile-email">test@example.com</p>
    `;
    
    // Секція особистої інформації
    const personalSection = this.createElement('div', '', 'profile-section');
    personalSection.innerHTML = `
      <h2 class="section-title">Особиста інформація</h2>
      <form id="personal-form">
        <div class="form-group">
          <label for="name">Ім'я</label>
          <input type="text" id="name" name="name" value="Тестовий Користувач">
        </div>
        
        <div class="form-group">
          <label for="email">Email</label>
          <input type="email" id="email" name="email" value="test@example.com" readonly>
        </div>
        
        <button type="submit" class="btn btn-primary">Зберегти зміни</button>
      </form>
    `;
    
    // Секція безпеки
    const securitySection = this.createElement('div', '', 'profile-section');
    securitySection.innerHTML = `
      <h2 class="section-title">Безпека</h2>
      
      <div class="security-item">
        <div class="security-info">
          <div class="security-title">Зміна пароля</div>
          <div class="security-description">Оновіть свій пароль для підвищення безпеки</div>
        </div>
        <button class="btn btn-primary" id="change-password-btn">Змінити пароль</button>
      </div>
      
      <div class="security-item">
        <div class="security-info">
          <div class="security-title">Двохфакторна аутентифікація</div>
          <div class="security-description">Додатковий рівень захисту для вашого акаунту</div>
        </div>
        <span class="status-badge ${this.twoFactorEnabled ? 'status-enabled' : 'status-disabled'}">
          ${this.twoFactorEnabled ? 'Увімкнено' : 'Вимкнено'}
        </span>
        <button class="btn ${this.twoFactorEnabled ? 'btn-danger' : 'btn-success'}" id="toggle-2fa-btn">
          ${this.twoFactorEnabled ? 'Вимкнути' : 'Увімкнути'}
        </button>
      </div>
      
      <div id="2fa-setup" style="display: ${this.twoFactorEnabled ? 'none' : 'none'};">
        <div class="qr-code">
          <h4>Налаштування двохфакторної аутентифікації</h4>
          <div class="qr-placeholder">QR код буде тут</div>
          <p>Або введіть цей ключ вручну:</p>
          <div class="manual-key">ABCD EFGH IJKL MNOP QRST UVWX YZ12 3456</div>
          
          <div class="form-group">
            <label for="verification-code">Код підтвердження</label>
            <input type="text" id="verification-code" placeholder="000000" maxlength="6">
          </div>
          
          <button class="btn btn-success" id="verify-2fa-btn">Підтвердити</button>
          <button class="btn btn-secondary" id="cancel-2fa-btn">Скасувати</button>
        </div>
      </div>
    `;

    container.appendChild(header);
    container.appendChild(personalSection);
    container.appendChild(securitySection);

    this.shadow.appendChild(styles);
    this.shadow.appendChild(container);
    
    console.log('ProfilePage render завершено');
  }

  protected setupEventListeners(): void {
    // Обробка форми особистої інформації
    this.shadow.addEventListener('submit', (event) => {
      if ((event.target as HTMLElement).id === 'personal-form') {
        event.preventDefault();
        console.log('Збереження особистої інформації');
        alert('Особиста інформація збережена!');
      }
    });

    // Обробка кліків
    this.shadow.addEventListener('click', (event) => {
      const target = event.target as HTMLElement;
      
      if (target.id === 'change-password-btn') {
        console.log('Зміна пароля');
        alert('Функція зміни пароля буде реалізована пізніше');
      } else if (target.id === 'toggle-2fa-btn') {
        this.toggle2FA();
      } else if (target.id === 'verify-2fa-btn') {
        this.verify2FA();
      } else if (target.id === 'cancel-2fa-btn') {
        this.cancel2FASetup();
      }
    });
  }

  /**
   * Перемикання двохфакторної аутентифікації
   */
  private toggle2FA(): void {
    if (this.twoFactorEnabled) {
      // Вимкнути 2FA
      if (confirm('Ви впевнені, що хочете вимкнути двохфакторну аутентифікацію?')) {
        this.twoFactorEnabled = false;
        this.render();
        alert('Двохфакторна аутентифікація вимкнена');
      }
    } else {
      // Увімкнути 2FA - показати налаштування
      const setupDiv = this.shadowQuery('#2fa-setup') as HTMLElement;
      if (setupDiv) {
        setupDiv.style.display = 'block';
      }
    }
  }

  /**
   * Підтвердження налаштування 2FA
   */
  private verify2FA(): void {
    const codeInput = this.shadowQuery('#verification-code') as HTMLInputElement;
    const code = codeInput?.value;
    
    if (!code || code.length !== 6) {
      alert('Будь ласка, введіть 6-значний код');
      return;
    }
    
    // Тут буде перевірка коду
    console.log('Перевірка коду 2FA:', code);
    
    // Симуляція успішної перевірки
    this.twoFactorEnabled = true;
    this.render();
    alert('Двохфакторна аутентифікація успішно увімкнена!');
  }

  /**
   * Скасування налаштування 2FA
   */
  private cancel2FASetup(): void {
    const setupDiv = this.shadowQuery('#2fa-setup') as HTMLElement;
    if (setupDiv) {
      setupDiv.style.display = 'none';
    }
  }
}

// Реєструємо компонент
customElements.define('profile-page', ProfilePage);
