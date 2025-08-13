import { BaseComponent } from '../components/BaseComponent';

/**
 * Сторінка перегляду окремого поста
 */
export class PostPage extends BaseComponent {
  private postId: string = '';
  private post: any = null;
  private comments: any[] = [];
  private loading = true;

  constructor() {
    super();
  }

  connectedCallback(): void {
    super.connectedCallback();
    
    // Отримуємо ID поста з URL
    const path = window.location.pathname;
    const match = path.match(/\/posts\/(.+)/);
    if (match) {
      this.postId = match[1];
      this.loadPost();
    }
  }

  protected render(): void {
    console.log('PostPage render викликано для поста:', this.postId);
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

      .back-link {
        color: #007bff;
        text-decoration: none;
        margin-bottom: 2rem;
        display: inline-block;
      }

      .back-link:hover {
        text-decoration: underline;
      }

      .post-card {
        background: white;
        padding: 2rem;
        border-radius: 8px;
        box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        margin-bottom: 2rem;
      }

      .post-header {
        margin-bottom: 2rem;
        padding-bottom: 1rem;
        border-bottom: 1px solid #e9ecef;
      }

      .post-title {
        font-size: 2rem;
        font-weight: bold;
        color: #333;
        margin: 0 0 1rem;
        line-height: 1.3;
      }

      .post-meta {
        color: #666;
        font-size: 0.9rem;
        display: flex;
        align-items: center;
        gap: 1rem;
        margin-bottom: 1rem;
      }

      .post-author {
        display: flex;
        align-items: center;
        gap: 0.25rem;
      }

      .post-date {
        display: flex;
        align-items: center;
        gap: 0.25rem;
      }

      .post-tags {
        display: flex;
        flex-wrap: wrap;
        gap: 0.5rem;
        margin-top: 1rem;
      }

      .tag {
        background: #e3f2fd;
        color: #1976d2;
        padding: 0.25rem 0.5rem;
        border-radius: 12px;
        font-size: 0.8rem;
      }

      .post-content {
        line-height: 1.8;
        color: #333;
        font-size: 1.1rem;
      }

      .post-content h1, .post-content h2, .post-content h3 {
        margin-top: 2rem;
        margin-bottom: 1rem;
        color: #333;
      }

      .post-content p {
        margin-bottom: 1rem;
      }

      .post-content blockquote {
        border-left: 4px solid #007bff;
        padding-left: 1rem;
        margin: 1rem 0;
        font-style: italic;
        color: #666;
      }

      .post-actions {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 1rem 0;
        border-top: 1px solid #e9ecef;
        margin-top: 2rem;
      }

      .action-buttons {
        display: flex;
        gap: 0.5rem;
      }

      .btn {
        padding: 0.5rem 1rem;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        text-decoration: none;
        display: inline-flex;
        align-items: center;
        gap: 0.25rem;
      }

      .btn-primary {
        background: #007bff;
        color: white;
      }

      .btn-secondary {
        background: #6c757d;
        color: white;
      }

      .btn-outline {
        background: transparent;
        color: #007bff;
        border: 1px solid #007bff;
      }

      .comments-section {
        background: white;
        padding: 2rem;
        border-radius: 8px;
        box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      }

      .comments-header {
        font-size: 1.25rem;
        font-weight: bold;
        color: #333;
        margin: 0 0 1rem;
        padding-bottom: 0.5rem;
        border-bottom: 1px solid #e9ecef;
      }

      .comment-form {
        margin-bottom: 2rem;
        padding: 1rem;
        background: #f8f9fa;
        border-radius: 4px;
      }

      .comment-form textarea {
        width: 100%;
        padding: 0.75rem;
        border: 1px solid #ddd;
        border-radius: 4px;
        font-size: 1rem;
        box-sizing: border-box;
        resize: vertical;
        min-height: 100px;
        font-family: inherit;
      }

      .comment-form textarea:focus {
        outline: none;
        border-color: #007bff;
        box-shadow: 0 0 0 3px rgba(0, 123, 255, 0.1);
      }

      .comment {
        padding: 1rem;
        border-bottom: 1px solid #e9ecef;
      }

      .comment:last-child {
        border-bottom: none;
      }

      .comment-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 0.5rem;
      }

      .comment-author {
        font-weight: 500;
        color: #333;
      }

      .comment-date {
        color: #666;
        font-size: 0.875rem;
      }

      .comment-content {
        color: #555;
        line-height: 1.6;
      }

      .loading {
        text-align: center;
        padding: 3rem;
        color: #666;
      }

      .error {
        background: #f8d7da;
        color: #721c24;
        padding: 1rem;
        border-radius: 4px;
        margin: 1rem 0;
      }

      @media (max-width: 768px) {
        :host {
          padding: 1rem;
        }
        
        .post-title {
          font-size: 1.5rem;
        }
        
        .post-meta {
          flex-direction: column;
          align-items: flex-start;
          gap: 0.5rem;
        }
        
        .post-actions {
          flex-direction: column;
          gap: 1rem;
        }
      }
    `);

    // Створюємо структуру сторінки
    const container = this.createElement('div', '', 'container');
    
    // Посилання назад
    const backLink = document.createElement('a');
    backLink.href = '/';
    backLink.className = 'back-link';
    backLink.innerHTML = '← Повернутися до списку постів';
    container.appendChild(backLink);
    
    if (this.loading) {
      // Стан завантаження
      const loading = this.createElement('div', '', 'loading');
      loading.innerHTML = '⏳ Завантаження поста...';
      container.appendChild(loading);
    } else if (!this.post) {
      // Пост не знайдено
      const error = this.createElement('div', '', 'error');
      error.innerHTML = '❌ Пост не знайдено';
      container.appendChild(error);
    } else {
      // Відображаємо пост
      container.appendChild(this.createPostCard());
      container.appendChild(this.createCommentsSection());
    }

    this.shadow.appendChild(styles);
    this.shadow.appendChild(container);
    
    console.log('PostPage render завершено');
  }

  protected setupEventListeners(): void {
    // Обробка відправки коментаря
    this.shadow.addEventListener('submit', (event) => {
      if ((event.target as HTMLElement).id === 'comment-form') {
        event.preventDefault();
        this.handleCommentSubmit(event);
      }
    });

    // Обробка кліків
    this.shadow.addEventListener('click', (event) => {
      const target = event.target as HTMLElement;
      
      if (target.id === 'like-btn') {
        this.handleLike();
      } else if (target.id === 'share-btn') {
        this.handleShare();
      }
    });
  }

  /**
   * Завантаження поста
   */
  private async loadPost(): Promise<void> {
    try {
      console.log('Завантаження поста з ID:', this.postId);
      this.loading = true;
      this.render();

      // Завантажуємо пост з API
      const apiService = (window as any).apiService;
      if (!apiService) {
        throw new Error('API Service не доступний');
      }

      console.log('Викликаємо apiService.getPost з ID:', this.postId);
      this.post = await apiService.getPost(this.postId);
      console.log('Пост завантажено успішно:', this.post);

      // Завантажуємо коментарі (якщо є в API відповіді)
      this.comments = this.post.comments || [];

      this.loading = false;
      this.render();
    } catch (error) {
      console.error('Помилка завантаження поста:', error);
      console.error('Тип помилки:', typeof error);
      console.error('Повідомлення помилки:', error instanceof Error ? error.message : String(error));
      this.loading = false;
      this.post = null;
      this.render();
    }
  }

  /**
   * Створення картки поста
   */
  private createPostCard(): HTMLElement {
    const card = this.createElement('article', '', 'post-card');
    
    const header = this.createElement('div', '', 'post-header');
    header.innerHTML = `
      <h1 class="post-title">${this.post.title}</h1>
      <div class="post-meta">
        <span class="post-author">👤 ${this.post.authorName}</span>
        <span class="post-date">📅 ${this.formatDate(this.post.createdDate)}</span>
        <span>💬 ${this.post.commentsCount} коментарів</span>
      </div>
      <div class="post-tags">
        ${this.post.tags.map((tag: string) => `<span class="tag">${tag}</span>`).join('')}
      </div>
    `;
    
    const content = this.createElement('div', '', 'post-content');
    content.innerHTML = this.post.content;
    
    const actions = this.createElement('div', '', 'post-actions');
    actions.innerHTML = `
      <div class="action-buttons">
        <button class="btn btn-outline" id="like-btn">👍 Подобається</button>
        <button class="btn btn-outline" id="share-btn">📤 Поділитися</button>
      </div>
      <div>
        <a href="/edit-post/${this.post.id}" class="btn btn-secondary">✏️ Редагувати</a>
      </div>
    `;
    
    card.appendChild(header);
    card.appendChild(content);
    card.appendChild(actions);
    
    return card;
  }

  /**
   * Створення секції коментарів
   */
  private createCommentsSection(): HTMLElement {
    const section = this.createElement('section', '', 'comments-section');
    
    const header = this.createElement('h2', `Коментарі (${this.comments.length})`, 'comments-header');
    
    // Форма додавання коментаря
    const form = document.createElement('form');
    form.id = 'comment-form';
    form.className = 'comment-form';
    form.innerHTML = `
      <textarea name="content" placeholder="Напишіть свій коментар..." required></textarea>
      <button type="submit" class="btn btn-primary" style="margin-top: 0.5rem;">Додати коментар</button>
    `;
    
    // Список коментарів
    const commentsList = this.createElement('div', '', 'comments-list');
    this.comments.forEach(comment => {
      const commentElement = this.createElement('div', '', 'comment');
      commentElement.innerHTML = `
        <div class="comment-header">
          <span class="comment-author">${comment.authorName}</span>
          <span class="comment-date">${this.formatDate(comment.createdAt)}</span>
        </div>
        <div class="comment-content">${comment.content}</div>
      `;
      commentsList.appendChild(commentElement);
    });
    
    section.appendChild(header);
    section.appendChild(form);
    section.appendChild(commentsList);
    
    return section;
  }

  /**
   * Обробка відправки коментаря
   */
  private handleCommentSubmit(event: Event): void {
    const form = event.target as HTMLFormElement;
    const formData = new FormData(form);
    const content = formData.get('content') as string;
    
    if (!content.trim()) return;
    
    console.log('Додавання коментаря:', content);
    
    // Додаємо новий коментар
    const newComment = {
      id: Date.now().toString(),
      authorName: 'Поточний користувач',
      content: content.trim(),
      createdAt: new Date().toISOString()
    };
    
    this.comments.unshift(newComment);
    this.post.commentsCount++;
    
    // Очищуємо форму та перерендерюємо
    form.reset();
    this.render();
  }

  /**
   * Обробка лайка
   */
  private handleLike(): void {
    console.log('Лайк поста');
    alert('Функція лайків буде реалізована пізніше');
  }

  /**
   * Обробка поділитися
   */
  private handleShare(): void {
    console.log('Поділитися постом');
    if (navigator.share) {
      navigator.share({
        title: this.post.title,
        url: window.location.href
      });
    } else {
      // Копіюємо URL в буфер обміну
      navigator.clipboard.writeText(window.location.href);
      alert('Посилання скопійовано в буфер обміну!');
    }
  }

  /**
   * Форматування дати
   */
  private formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('uk-UA', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }
}

// Реєструємо компонент
customElements.define('post-page', PostPage);
