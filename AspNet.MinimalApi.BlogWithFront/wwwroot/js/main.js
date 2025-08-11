/**
 * Головний файл додатку
 * Ініціалізує компоненти та керує навігацією
 */

import { api } from './api.js';
import { router } from './router.js';
import {
  formatDate,
  showMessage,
  showLoading,
  isTokenValid,
  getUserRoles,
  getUserName,
  hasRole,
  escapeHtml,
  truncateText
} from './utils.js';

/**
 * Головний клас додатку
 */
class BlogApp {
  constructor() {
    this.currentUser = null;
    this.currentPage = 'home';
    this.modal = null;

    this.setupRoutes();
    this.init();
  }

  /**
   * Налаштування маршрутів
   */
  setupRoutes() {
    router.register('/', () => this.showHomePage());
    router.register('/posts/:id', (params) => this.showPostPage(params.id));
    router.register('/admin', () => this.showAdminPage());
    router.register('/admin/users', () => this.showAdminPage('users'));
    router.register('/admin/posts', () => this.showAdminPage('posts'));
  }

  /**
   * Ініціалізація додатку
   */
  init() {
    this.setupEventListeners();
    this.checkAuth();
    this.renderAuthWidget();

    // Ініціалізуємо роутер після реєстрації всіх маршрутів
    router.init();
  }

  /**
   * Налаштування обробників подій
   */
  setupEventListeners() {
    // Навігація через роутер
    document.addEventListener('click', (e) => {
      const navLink = e.target.closest('.nav-link');
      if (navLink) {
        e.preventDefault();
        const page = navLink.dataset.page;

        if (page === 'home') {
          router.navigate('/');
        } else if (page === 'admin') {
          router.navigate('/admin');
        }
      }
    });

    // Модальні вікна
    this.modal = document.getElementById('modal-overlay');
    document.querySelector('.modal-close').addEventListener('click', () => {
      this.hideModal();
    });
    
    this.modal.addEventListener('click', (e) => {
      if (e.target === this.modal) {
        this.hideModal();
      }
    });

    // Кнопка створення посту
    document.getElementById('create-post-btn').addEventListener('click', () => {
      this.showCreatePostModal();
    });

    // Адмін вкладки
    document.addEventListener('click', (e) => {
      const tabBtn = e.target.closest('.tab-btn');
      if (tabBtn) {
        this.switchAdminTab(tabBtn.dataset.tab);
      }
    });
  }

  /**
   * Перевіряє авторизацію користувача
   */
  checkAuth() {
    const token = api.getToken();
    if (token && isTokenValid(token)) {
      this.currentUser = {
        name: getUserName(token),
        roles: getUserRoles(token),
        token: token
      };
    } else {
      this.currentUser = null;
      api.setToken(null);
    }
  }

  /**
   * Рендерить віджет авторизації
   */
  renderAuthWidget() {
    const container = document.getElementById('auth-container');
    
    if (this.currentUser) {
      container.innerHTML = `
        <div class="auth-user-info">
          <span class="user-name">👤 ${escapeHtml(this.currentUser.name)}</span>
          <button class="btn btn-secondary btn-small" id="logout-btn">Вийти</button>
        </div>
      `;
      
      document.getElementById('logout-btn').addEventListener('click', () => {
        this.logout();
      });

      // Показуємо адмін посилання для адмінів
      const adminLink = document.getElementById('admin-link');
      if (hasRole(this.currentUser.token, 'Admin')) {
        adminLink.style.display = 'block';
      } else {
        adminLink.style.display = 'none';
      }

      // Показуємо кнопку створення посту для адмінів
      const createBtn = document.getElementById('create-post-btn');
      if (hasRole(this.currentUser.token, 'Admin')) {
        createBtn.style.display = 'block';
      } else {
        createBtn.style.display = 'none';
      }
    } else {
      container.innerHTML = `
        <form class="auth-form" id="login-form">
          <div class="form-group">
            <input type="text" class="form-input" name="username" placeholder="Email або ім'я" required>
          </div>
          <div class="form-group">
            <input type="password" class="form-input" name="password" placeholder="Пароль" required>
          </div>
          <button type="submit" class="btn btn-primary">Увійти</button>
          <button type="button" class="btn btn-secondary" id="register-btn">Реєстрація</button>
        </form>
      `;

      document.getElementById('login-form').addEventListener('submit', (e) => {
        this.handleLogin(e);
      });

      document.getElementById('register-btn').addEventListener('click', () => {
        this.showRegisterModal();
      });

      // Приховуємо адмін елементи
      document.getElementById('admin-link').style.display = 'none';
      document.getElementById('create-post-btn').style.display = 'none';
    }
  }

  /**
   * Обробляє вхід користувача
   */
  async handleLogin(e) {
    e.preventDefault();
    const formData = new FormData(e.target);
    const username = formData.get('username');
    const password = formData.get('password');

    try {
      await api.login(username, password);
      this.checkAuth();
      this.renderAuthWidget();
      showMessage('Успішний вхід!', 'success');
      this.loadPosts(); // Перезавантажуємо пости для показу кнопок
    } catch (error) {
      showMessage('Помилка входу. Перевірте дані.', 'error');
    }
  }

  /**
   * Виходить з системи
   */
  logout() {
    api.logout();
    this.currentUser = null;
    this.renderAuthWidget();
    this.showPage('home');
    this.loadPosts();
    showMessage('Ви вийшли з системи', 'success');
  }

  /**
   * Показує головну сторінку
   */
  showHomePage() {
    this.showPage('home');
    this.loadPosts();
  }

  /**
   * Показує сторінку окремого посту
   */
  async showPostPage(postId) {
    this.showPage('home'); // Використовуємо головну сторінку як базу

    try {
      const post = await api.getPost(postId);
      this.renderSinglePost(post);
    } catch (error) {
      showMessage('Помилка завантаження посту', 'error');
      router.navigate('/');
    }
  }

  /**
   * Показує адмін сторінку
   */
  showAdminPage(tab = 'users') {
    if (!this.currentUser || !hasRole(this.currentUser.token, 'Admin')) {
      showMessage('Доступ заборонено', 'error');
      router.navigate('/');
      return;
    }

    this.showPage('admin');
    this.switchAdminTab(tab);
  }

  /**
   * Показує сторінку
   */
  showPage(pageName) {
    // Оновлюємо навігацію
    document.querySelectorAll('.nav-link').forEach(link => {
      link.classList.remove('active');
      if (link.dataset.page === pageName) {
        link.classList.add('active');
      }
    });

    // Показуємо потрібну сторінку
    document.querySelectorAll('.page').forEach(page => {
      page.classList.remove('active');
    });

    const targetPage = document.getElementById(`${pageName}-page`);
    if (targetPage) {
      targetPage.classList.add('active');
      this.currentPage = pageName;
    }
  }

  /**
   * Показує модальне вікно
   */
  showModal(title, content) {
    document.getElementById('modal-title').textContent = title;
    document.getElementById('modal-body').innerHTML = content;
    this.modal.classList.add('active');
  }

  /**
   * Приховує модальне вікно
   */
  hideModal() {
    this.modal.classList.remove('active');
  }

  /**
   * Показує модальне вікно реєстрації
   */
  showRegisterModal() {
    const content = `
      <form id="register-form">
        <div class="form-group">
          <label class="form-label">Ім'я користувача:</label>
          <input type="text" class="form-input" name="username" required>
        </div>
        <div class="form-group">
          <label class="form-label">Email:</label>
          <input type="email" class="form-input" name="email" required>
        </div>
        <div class="form-group">
          <label class="form-label">Пароль:</label>
          <input type="password" class="form-input" name="password" required>
        </div>
        <div class="form-group">
          <button type="submit" class="btn btn-primary">Зареєструватися</button>
        </div>
      </form>
    `;

    this.showModal('Реєстрація', content);

    document.getElementById('register-form').addEventListener('submit', async (e) => {
      e.preventDefault();
      const formData = new FormData(e.target);
      
      try {
        await api.register(
          formData.get('username'),
          formData.get('email'),
          formData.get('password')
        );
        this.hideModal();
        showMessage('Реєстрація успішна! Тепер можете увійти.', 'success');
      } catch (error) {
        showMessage('Помилка реєстрації. Спробуйте ще раз.', 'error');
      }
    });
  }

  /**
   * Показує модальне вікно створення посту
   */
  showCreatePostModal() {
    const content = `
      <form id="create-post-form">
        <div class="form-group">
          <label class="form-label">Заголовок:</label>
          <input type="text" class="form-input" name="title" required>
        </div>
        <div class="form-group">
          <label class="form-label">Контент:</label>
          <textarea class="form-textarea" name="content" required></textarea>
        </div>
        <div class="form-group">
          <button type="submit" class="btn btn-primary">Створити пост</button>
        </div>
      </form>
    `;

    this.showModal('Створити новий пост', content);

    document.getElementById('create-post-form').addEventListener('submit', async (e) => {
      e.preventDefault();
      const formData = new FormData(e.target);
      
      try {
        await api.createPost(
          formData.get('title'),
          formData.get('content')
        );
        this.hideModal();
        this.loadPosts();
        showMessage('Пост успішно створено!', 'success');
      } catch (error) {
        showMessage('Помилка створення посту.', 'error');
      }
    });
  }

  /**
   * Завантажує та відображає пости
   */
  async loadPosts() {
    const container = document.getElementById('posts-container');
    showLoading(container);

    try {
      const posts = await api.getPosts();
      this.renderPosts(posts);
    } catch (error) {
      container.innerHTML = '<div class="alert alert-error">Помилка завантаження постів</div>';
    }
  }

  /**
   * Рендерить список постів
   */
  renderPosts(posts) {
    const container = document.getElementById('posts-container');

    if (posts.length === 0) {
      container.innerHTML = '<div class="alert alert-warning">Постів поки що немає</div>';
      return;
    }

    container.innerHTML = posts.map(post => `
      <article class="card">
        <div class="card-header">
          <div>
            <h3 class="card-title">
              <a href="/posts/${post.id}" class="post-link">${escapeHtml(post.title)}</a>
            </h3>
            <div class="card-meta">${formatDate(post.createdDate)}</div>
          </div>
          ${this.currentUser && hasRole(this.currentUser.token, 'Admin') ? `
            <div class="card-actions">
              <button class="btn btn-secondary btn-small edit-post-btn" data-id="${post.id}">✏️ Редагувати</button>
              <button class="btn btn-danger btn-small delete-post-btn" data-id="${post.id}">🗑️ Видалити</button>
            </div>
          ` : ''}
        </div>
        <div class="card-content">
          ${escapeHtml(truncateText(post.content, 300))}
          ${post.content.length > 300 ? `<br><a href="/posts/${post.id}" class="read-more">Читати далі...</a>` : ''}
        </div>
        <div class="stats">
          <div class="stat-item">
            <button class="btn btn-secondary btn-small like-btn" data-id="${post.id}" ${!this.currentUser ? 'disabled' : ''}>
              💙 ${post.likesCount}
            </button>
          </div>
          <div class="stat-item">
            <a href="/posts/${post.id}" class="comment-link">💬 ${post.commentsCount} коментарів</a>
          </div>
        </div>
      </article>
    `).join('');

    // Додаємо обробники подій
    this.setupPostEventListeners();
  }

  /**
   * Рендерить окремий пост з коментарями
   */
  renderSinglePost(post) {
    const container = document.getElementById('posts-container');

    container.innerHTML = `
      <article class="card">
        <div class="card-header">
          <div>
            <h1 class="card-title">${escapeHtml(post.title)}</h1>
            <div class="card-meta">
              ${formatDate(post.createdDate)} • Автор: ${escapeHtml(post.authorName)}
            </div>
          </div>
          ${this.currentUser && hasRole(this.currentUser.token, 'Admin') ? `
            <div class="card-actions">
              <button class="btn btn-secondary btn-small edit-post-btn" data-id="${post.id}">✏️ Редагувати</button>
              <button class="btn btn-danger btn-small delete-post-btn" data-id="${post.id}">🗑️ Видалити</button>
            </div>
          ` : ''}
        </div>
        <div class="card-content">
          ${escapeHtml(post.content).replace(/\n/g, '<br>')}
        </div>
        <div class="stats">
          <div class="stat-item">
            <button class="btn btn-secondary btn-small like-btn" data-id="${post.id}" ${!this.currentUser ? 'disabled' : ''}>
              💙 ${post.likesCount}
            </button>
          </div>
          <div class="stat-item">
            💬 ${post.comments.length} коментарів
          </div>
        </div>

        <!-- Секція коментарів -->
        <div class="comments-section">
          <div class="comments-header">
            <h3>Коментарі</h3>
            ${this.currentUser ? `
              <button class="btn btn-primary btn-small" id="add-comment-btn" data-post-id="${post.id}">
                ➕ Додати коментар
              </button>
            ` : ''}
          </div>
          <div class="comments-list">
            ${post.comments.map(comment => `
              <div class="comment">
                <div class="comment-header">
                  <span class="comment-author">${escapeHtml(comment.authorName)}</span>
                  <span class="comment-date">${formatDate(comment.createdDate)}</span>
                  ${this.currentUser && hasRole(this.currentUser.token, 'Admin') ? `
                    <div class="comment-actions">
                      <button class="btn btn-danger btn-small delete-comment-btn" data-id="${comment.id}">🗑️</button>
                    </div>
                  ` : ''}
                </div>
                <div class="comment-content">
                  ${escapeHtml(comment.content).replace(/\n/g, '<br>')}
                </div>
              </div>
            `).join('')}
          </div>
        </div>
      </article>

      <div class="navigation-back">
        <a href="/" class="btn btn-secondary">← Повернутися до списку постів</a>
      </div>
    `;

    // Додаємо обробники подій
    this.setupPostEventListeners();
    this.setupCommentEventListeners();
  }

  /**
   * Налаштовує обробники подій для постів
   */
  setupPostEventListeners() {
    // Лайки
    document.querySelectorAll('.like-btn').forEach(btn => {
      btn.addEventListener('click', async () => {
        if (!this.currentUser) {
          showMessage('Увійдіть, щоб ставити лайки', 'warning');
          return;
        }

        try {
          const result = await api.toggleLike(btn.dataset.id);
          btn.innerHTML = `💙 ${result.likes}`;
          showMessage(result.liked ? 'Лайк поставлено!' : 'Лайк знято!', 'success');
        } catch (error) {
          showMessage('Помилка при роботі з лайками', 'error');
        }
      });
    });

    // Редагування постів (тільки для адмінів)
    document.querySelectorAll('.edit-post-btn').forEach(btn => {
      btn.addEventListener('click', () => {
        this.showEditPostModal(btn.dataset.id);
      });
    });

    // Видалення постів (тільки для адмінів)
    document.querySelectorAll('.delete-post-btn').forEach(btn => {
      btn.addEventListener('click', () => {
        this.deletePost(btn.dataset.id);
      });
    });
  }

  /**
   * Показує модальне вікно редагування посту
   */
  async showEditPostModal(postId) {
    try {
      const post = await api.getPost(postId);
      
      const content = `
        <form id="edit-post-form">
          <div class="form-group">
            <label class="form-label">Заголовок:</label>
            <input type="text" class="form-input" name="title" value="${escapeHtml(post.title)}" required>
          </div>
          <div class="form-group">
            <label class="form-label">Контент:</label>
            <textarea class="form-textarea" name="content" required>${escapeHtml(post.content)}</textarea>
          </div>
          <div class="form-group">
            <button type="submit" class="btn btn-primary">Зберегти зміни</button>
          </div>
        </form>
      `;

      this.showModal('Редагувати пост', content);

      document.getElementById('edit-post-form').addEventListener('submit', async (e) => {
        e.preventDefault();
        const formData = new FormData(e.target);
        
        try {
          await api.updatePost(
            postId,
            formData.get('title'),
            formData.get('content')
          );
          this.hideModal();
          this.loadPosts();
          showMessage('Пост успішно оновлено!', 'success');
        } catch (error) {
          showMessage('Помилка оновлення посту.', 'error');
        }
      });
    } catch (error) {
      showMessage('Помилка завантаження посту для редагування.', 'error');
    }
  }

  /**
   * Видаляє пост
   */
  async deletePost(postId) {
    if (!confirm('Ви впевнені, що хочете видалити цей пост?')) {
      return;
    }

    try {
      await api.deletePost(postId);

      // Якщо ми на сторінці окремого посту, перенаправляємо на головну
      if (router.getCurrentRoute().includes('/posts/')) {
        router.navigate('/');
      } else {
        this.loadPosts();
      }

      showMessage('Пост успішно видалено!', 'success');
    } catch (error) {
      showMessage('Помилка видалення посту.', 'error');
    }
  }

  /**
   * Налаштовує обробники подій для коментарів
   */
  setupCommentEventListeners() {
    // Додавання коментаря
    const addCommentBtn = document.getElementById('add-comment-btn');
    if (addCommentBtn) {
      addCommentBtn.addEventListener('click', () => {
        this.showAddCommentModal(addCommentBtn.dataset.postId);
      });
    }

    // Видалення коментарів
    document.querySelectorAll('.delete-comment-btn').forEach(btn => {
      btn.addEventListener('click', () => {
        this.deleteComment(btn.dataset.id);
      });
    });
  }

  /**
   * Показує модальне вікно додавання коментаря
   */
  showAddCommentModal(postId) {
    const content = `
      <form id="add-comment-form">
        <div class="form-group">
          <label class="form-label">Коментар:</label>
          <textarea class="form-textarea" name="content" required placeholder="Напишіть ваш коментар..."></textarea>
        </div>
        <div class="form-group">
          <button type="submit" class="btn btn-primary">Додати коментар</button>
        </div>
      </form>
    `;

    this.showModal('Додати коментар', content);

    document.getElementById('add-comment-form').addEventListener('submit', async (e) => {
      e.preventDefault();
      const formData = new FormData(e.target);

      try {
        await api.createComment(postId, formData.get('content'));
        this.hideModal();

        // Перезавантажуємо пост з коментарями
        const post = await api.getPost(postId);
        this.renderSinglePost(post);

        showMessage('Коментар успішно додано!', 'success');
      } catch (error) {
        showMessage('Помилка додавання коментаря.', 'error');
      }
    });
  }

  /**
   * Видаляє коментар
   */
  async deleteComment(commentId) {
    if (!confirm('Ви впевнені, що хочете видалити цей коментар?')) {
      return;
    }

    try {
      await api.deleteComment(commentId);

      // Перезавантажуємо поточний пост
      const currentRoute = router.getCurrentRoute();
      const postIdMatch = currentRoute.match(/\/posts\/(\d+)/);
      if (postIdMatch) {
        const post = await api.getPost(postIdMatch[1]);
        this.renderSinglePost(post);
      }

      showMessage('Коментар успішно видалено!', 'success');
    } catch (error) {
      showMessage('Помилка видалення коментаря.', 'error');
    }
  }

  /**
   * Завантажує контент адмін панелі
   */
  loadAdminContent() {
    this.switchAdminTab('users');
  }

  /**
   * Перемикає вкладку в адмін панелі
   */
  async switchAdminTab(tabName) {
    // Оновлюємо активну вкладку
    document.querySelectorAll('.tab-btn').forEach(btn => {
      btn.classList.remove('active');
      if (btn.dataset.tab === tabName) {
        btn.classList.add('active');
      }
    });

    const container = document.getElementById('admin-content');
    showLoading(container);

    try {
      if (tabName === 'users') {
        await this.loadUsersTab();
      } else if (tabName === 'posts') {
        await this.loadPostsTab();
      }
    } catch (error) {
      container.innerHTML = '<div class="alert alert-error">Помилка завантаження даних</div>';
    }
  }

  /**
   * Завантажує вкладку користувачів
   */
  async loadUsersTab() {
    const users = await api.getUsers();
    const container = document.getElementById('admin-content');
    
    container.innerHTML = `
      <table class="table">
        <thead>
          <tr>
            <th>Ім'я користувача</th>
            <th>Email</th>
            <th>Ролі</th>
            <th>Дії</th>
          </tr>
        </thead>
        <tbody>
          ${users.map(user => `
            <tr>
              <td>${escapeHtml(user.userName || 'Не вказано')}</td>
              <td>${escapeHtml(user.email || 'Не вказано')}</td>
              <td>${user.roles.join(', ')}</td>
              <td>
                <button class="btn btn-secondary btn-small assign-role-btn" data-user-id="${user.id}">
                  Керувати ролями
                </button>
              </td>
            </tr>
          `).join('')}
        </tbody>
      </table>
    `;

    // Додаємо обробники подій
    document.querySelectorAll('.assign-role-btn').forEach(btn => {
      btn.addEventListener('click', () => {
        this.showRoleManagementModal(btn.dataset.userId);
      });
    });
  }

  /**
   * Завантажує вкладку постів для адміна
   */
  async loadPostsTab() {
    const posts = await api.getAdminPosts();
    const container = document.getElementById('admin-content');
    
    container.innerHTML = `
      <table class="table">
        <thead>
          <tr>
            <th>Заголовок</th>
            <th>Автор</th>
            <th>Дата створення</th>
            <th>Коментарі</th>
            <th>Лайки</th>
            <th>Дії</th>
          </tr>
        </thead>
        <tbody>
          ${posts.map(post => `
            <tr>
              <td>${escapeHtml(truncateText(post.title, 50))}</td>
              <td>${escapeHtml(post.authorName)}</td>
              <td>${formatDate(post.createdDate)}</td>
              <td>${post.commentsCount}</td>
              <td>${post.likesCount}</td>
              <td>
                <button class="btn btn-secondary btn-small edit-post-btn" data-id="${post.id}">Редагувати</button>
                <button class="btn btn-danger btn-small delete-post-btn" data-id="${post.id}">Видалити</button>
              </td>
            </tr>
          `).join('')}
        </tbody>
      </table>
    `;

    // Додаємо обробники подій
    document.querySelectorAll('.edit-post-btn').forEach(btn => {
      btn.addEventListener('click', () => {
        this.showEditPostModal(btn.dataset.id);
      });
    });

    document.querySelectorAll('.delete-post-btn').forEach(btn => {
      btn.addEventListener('click', () => {
        this.deletePost(btn.dataset.id);
      });
    });
  }

  /**
   * Показує модальне вікно керування ролями
   */
  showRoleManagementModal(userId) {
    const content = `
      <div class="form-group">
        <label class="form-label">Призначити роль:</label>
        <select class="form-input" id="role-select">
          <option value="User">Користувач</option>
          <option value="Admin">Адміністратор</option>
        </select>
        <button class="btn btn-primary" id="assign-role-btn" data-user-id="${userId}">
          Призначити роль
        </button>
      </div>
      <div class="form-group">
        <label class="form-label">Видалити роль:</label>
        <select class="form-input" id="remove-role-select">
          <option value="User">Користувач</option>
          <option value="Admin">Адміністратор</option>
        </select>
        <button class="btn btn-danger" id="remove-role-btn" data-user-id="${userId}">
          Видалити роль
        </button>
      </div>
    `;

    this.showModal('Керування ролями', content);

    document.getElementById('assign-role-btn').addEventListener('click', async () => {
      const role = document.getElementById('role-select').value;
      try {
        await api.assignRole(userId, role);
        this.hideModal();
        this.loadUsersTab();
        showMessage(`Роль ${role} призначена!`, 'success');
      } catch (error) {
        showMessage('Помилка призначення ролі.', 'error');
      }
    });

    document.getElementById('remove-role-btn').addEventListener('click', async () => {
      const role = document.getElementById('remove-role-select').value;
      try {
        await api.removeRole(userId, role);
        this.hideModal();
        this.loadUsersTab();
        showMessage(`Роль ${role} видалена!`, 'success');
      } catch (error) {
        showMessage('Помилка видалення ролі.', 'error');
      }
    });
  }
}

// Ініціалізуємо додаток після завантаження DOM
document.addEventListener('DOMContentLoaded', () => {
  new BlogApp();
});
