import { BaseComponent } from '../components/BaseComponent';

/**
 * Сторінка редагування поста
 */
export class EditPostPage extends BaseComponent {
  private postId: string = '';
  private post: any = null;
  private loading = true;

  constructor() {
    super();
  }

  connectedCallback(): void {
    super.connectedCallback();
    
    // Отримуємо ID поста з URL
    const path = window.location.pathname;
    const match = path.match(/\/edit-post\/(.+)/);
    if (match) {
      this.postId = match[1];
      this.loadPost();
    }
  }

  protected render(): void {
    console.log('EditPostPage render викликано для поста:', this.postId);
    this.shadow.innerHTML = '';
    
    // Додаємо стилі (такі ж як у CreatePostPage)
    const styles = this.createStyles(`
      :host {
        display: block;
        padding: 2rem;
      }

      .container {
        max-width: 800px;
        margin: 0 auto;
        background: white;
        padding: 2rem;
        border-radius: 8px;
        box-shadow: 0 2px 4px rgba(0,0,0,0.1);
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

      h1 {
        color: #333;
        margin: 0 0 2rem;
      }

      .form-group {
        margin-bottom: 1.5rem;
      }

      label {
        display: block;
        margin-bottom: 0.5rem;
        font-weight: 500;
        color: #333;
      }

      input, textarea, select {
        width: 100%;
        padding: 0.75rem;
        border: 1px solid #ddd;
        border-radius: 4px;
        font-size: 1rem;
        box-sizing: border-box;
        font-family: inherit;
      }

      input:focus, textarea:focus, select:focus {
        outline: none;
        border-color: #007bff;
        box-shadow: 0 0 0 3px rgba(0, 123, 255, 0.1);
      }

      textarea {
        min-height: 200px;
        resize: vertical;
      }

      .content-editor {
        min-height: 300px;
        font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
        line-height: 1.6;
      }

      .form-row {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 1rem;
      }

      .btn {
        padding: 0.75rem 1.5rem;
        border: none;
        border-radius: 4px;
        font-size: 1rem;
        cursor: pointer;
        margin-right: 0.5rem;
        text-decoration: none;
        display: inline-block;
      }

      .btn-primary {
        background: #007bff;
        color: white;
      }

      .btn-primary:hover {
        background: #0056b3;
      }

      .btn-secondary {
        background: #6c757d;
        color: white;
      }

      .btn-secondary:hover {
        background: #545b62;
      }

      .btn-danger {
        background: #dc3545;
        color: white;
      }

      .btn-danger:hover {
        background: #c82333;
      }

      .actions {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-top: 2rem;
        padding-top: 1rem;
        border-top: 1px solid #e9ecef;
      }

      .tags-input {
        display: flex;
        flex-wrap: wrap;
        gap: 0.5rem;
        align-items: center;
        min-height: 2.5rem;
        padding: 0.5rem;
        border: 1px solid #ddd;
        border-radius: 4px;
        cursor: text;
      }

      .tag {
        background: #e3f2fd;
        color: #1976d2;
        padding: 0.25rem 0.5rem;
        border-radius: 12px;
        font-size: 0.875rem;
        display: flex;
        align-items: center;
        gap: 0.25rem;
      }

      .tag-remove {
        cursor: pointer;
        font-weight: bold;
      }

      .tag-input {
        border: none;
        outline: none;
        flex: 1;
        min-width: 100px;
        font-size: 1rem;
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
        .form-row {
          grid-template-columns: 1fr;
        }
        
        .actions {
          flex-direction: column;
          gap: 1rem;
        }
      }
    `);

    // Створюємо структуру сторінки
    const container = this.createElement('div', '', 'container');
    
    // Посилання назад
    const backLink = document.createElement('a');
    backLink.href = `/posts/${this.postId}`;
    backLink.className = 'back-link';
    backLink.innerHTML = '← Повернутися до поста';
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
      // Форма редагування
      container.appendChild(this.createEditForm());
    }

    this.shadow.appendChild(styles);
    this.shadow.appendChild(container);
    
    console.log('EditPostPage render завершено');
  }

  protected setupEventListeners(): void {
    // Обробка відправки форми
    this.shadow.addEventListener('submit', (event) => {
      event.preventDefault();
      this.handleSubmit(event);
    });

    // Обробка кліків
    this.shadow.addEventListener('click', (event) => {
      const target = event.target as HTMLElement;
      
      if (target.id === 'delete-btn') {
        this.handleDelete();
      } else if (target.id === 'cancel-btn') {
        this.handleCancel();
      } else if (target.classList.contains('tag-remove')) {
        this.removeTag(target);
      }
    });

    // Обробка введення тегів
    this.shadow.addEventListener('keydown', (event) => {
      const target = event.target as HTMLElement;
      
      if (target.classList.contains('tag-input') && (event as KeyboardEvent).key === 'Enter') {
        event.preventDefault();
        this.addTag((target as HTMLInputElement).value);
        (target as HTMLInputElement).value = '';
      }
    });
  }

  /**
   * Завантаження поста
   */
  private async loadPost(): Promise<void> {
    try {
      this.loading = true;
      this.render();
      
      // TODO: Замінити на реальний API виклик
      await new Promise(resolve => setTimeout(resolve, 1000));
      
      // Тимчасові дані для демонстрації
      this.post = {
        id: this.postId,
        title: 'Як створити сучасний веб-додаток',
        category: 'tech',
        status: 'published',
        summary: 'Короткий опис про створення веб-додатків',
        content: 'Створення сучасного веб-додатку - це захоплюючий процес, який вимагає знання багатьох технологій та підходів.',
        tags: ['веб-розробка', 'javascript', 'typescript', 'frontend']
      };
      
      this.loading = false;
      this.render();
    } catch (error) {
      console.error('Помилка завантаження поста:', error);
      this.loading = false;
      this.post = null;
      this.render();
    }
  }

  /**
   * Створення форми редагування
   */
  private createEditForm(): HTMLElement {
    const form = document.createElement('form');
    form.id = 'edit-post-form';
    
    form.innerHTML = `
      <h1>✏️ Редагувати пост</h1>
      
      <div class="form-group">
        <label for="title">Заголовок поста</label>
        <input type="text" id="title" name="title" value="${this.post.title}" required>
      </div>
      
      <div class="form-row">
        <div class="form-group">
          <label for="category">Категорія</label>
          <select id="category" name="category" required>
            <option value="">Оберіть категорію</option>
            <option value="tech" ${this.post.category === 'tech' ? 'selected' : ''}>Технології</option>
            <option value="lifestyle" ${this.post.category === 'lifestyle' ? 'selected' : ''}>Стиль життя</option>
            <option value="travel" ${this.post.category === 'travel' ? 'selected' : ''}>Подорожі</option>
            <option value="food" ${this.post.category === 'food' ? 'selected' : ''}>Їжа</option>
            <option value="other" ${this.post.category === 'other' ? 'selected' : ''}>Інше</option>
          </select>
        </div>
        
        <div class="form-group">
          <label for="status">Статус</label>
          <select id="status" name="status" required>
            <option value="draft" ${this.post.status === 'draft' ? 'selected' : ''}>Чернетка</option>
            <option value="published" ${this.post.status === 'published' ? 'selected' : ''}>Опублікувати</option>
          </select>
        </div>
      </div>
      
      <div class="form-group">
        <label for="summary">Короткий опис</label>
        <textarea id="summary" name="summary" rows="3">${this.post.summary}</textarea>
      </div>
      
      <div class="form-group">
        <label for="tags">Теги</label>
        <div class="tags-input" id="tags-input">
          ${this.post.tags.map((tag: string) => `<span class="tag">${tag} <span class="tag-remove">×</span></span>`).join('')}
          <input type="text" class="tag-input" placeholder="Додайте тег і натисніть Enter...">
        </div>
      </div>
      
      <div class="form-group">
        <label for="content">Контент</label>
        <textarea id="content" name="content" class="content-editor" required>${this.post.content}</textarea>
      </div>
      
      <div class="actions">
        <div>
          <button type="button" class="btn btn-danger" id="delete-btn">🗑️ Видалити пост</button>
        </div>
        <div>
          <button type="button" class="btn btn-secondary" id="cancel-btn">Скасувати</button>
          <button type="submit" class="btn btn-primary">💾 Зберегти зміни</button>
        </div>
      </div>
    `;
    
    return form;
  }

  /**
   * Обробка відправки форми
   */
  private handleSubmit(event: Event): void {
    const form = event.target as HTMLFormElement;
    const formData = new FormData(form);
    
    // Збираємо теги
    const tags = this.getTags();
    
    const postData = {
      id: this.postId,
      title: formData.get('title'),
      category: formData.get('category'),
      status: formData.get('status'),
      summary: formData.get('summary'),
      content: formData.get('content'),
      tags: tags
    };
    
    console.log('Оновлення поста:', postData);
    
    // Тут буде API виклик
    alert('Пост оновлено успішно!');
    
    // Перенаправляємо на сторінку поста
    window.history.pushState(null, '', `/posts/${this.postId}`);
    window.dispatchEvent(new PopStateEvent('popstate'));
  }

  /**
   * Обробка видалення поста
   */
  private handleDelete(): void {
    if (confirm('Ви впевнені, що хочете видалити цей пост? Цю дію неможливо скасувати.')) {
      console.log('Видалення поста:', this.postId);
      
      // Тут буде API виклик
      alert('Пост видалено!');
      
      // Перенаправляємо на головну
      window.history.pushState(null, '', '/');
      window.dispatchEvent(new PopStateEvent('popstate'));
    }
  }

  /**
   * Обробка скасування
   */
  private handleCancel(): void {
    if (confirm('Ви впевнені, що хочете скасувати? Незбережені зміни будуть втрачені.')) {
      window.history.pushState(null, '', `/posts/${this.postId}`);
      window.dispatchEvent(new PopStateEvent('popstate'));
    }
  }

  /**
   * Додавання тегу
   */
  private addTag(tagText: string): void {
    const trimmedTag = tagText.trim();
    if (!trimmedTag) return;
    
    const tagsContainer = this.shadowQuery('#tags-input');
    const tagInput = tagsContainer?.querySelector('.tag-input') as HTMLInputElement;
    
    if (tagsContainer && tagInput) {
      const tagElement = document.createElement('span');
      tagElement.className = 'tag';
      tagElement.innerHTML = `${trimmedTag} <span class="tag-remove">×</span>`;
      
      tagsContainer.insertBefore(tagElement, tagInput);
    }
  }

  /**
   * Видалення тегу
   */
  private removeTag(removeBtn: HTMLElement): void {
    const tag = removeBtn.parentElement;
    if (tag) {
      tag.remove();
    }
  }

  /**
   * Отримання всіх тегів
   */
  private getTags(): string[] {
    const tags: string[] = [];
    const tagElements = this.shadow.querySelectorAll('.tag');
    
    tagElements.forEach(tag => {
      const text = tag.textContent?.replace('×', '').trim();
      if (text) {
        tags.push(text);
      }
    });
    
    return tags;
  }
}

// Реєструємо компонент
customElements.define('edit-post-page', EditPostPage);
