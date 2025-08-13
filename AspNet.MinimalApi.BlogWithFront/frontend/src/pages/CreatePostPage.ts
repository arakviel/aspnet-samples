import { BaseComponent } from '../components/BaseComponent';
import { apiService } from '../services/ApiService';
import { authService } from '../services/AuthService';

/**
 * Сторінка створення нового поста
 */
export class CreatePostPage extends BaseComponent {
  private isLoading = false;
  private errorMessage = '';

  constructor() {
    super();
  }

  connectedCallback(): void {
    super.connectedCallback();

    // Перевіряємо аутентифікацію
    if (!authService.getIsAuthenticated()) {
      // Перенаправляємо на сторінку входу
      window.history.pushState(null, '', '/login');
      window.dispatchEvent(new PopStateEvent('popstate'));
      return;
    }
  }

  protected render(): void {
    console.log('CreatePostPage render викликано');
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
        background: white;
        padding: 2rem;
        border-radius: 8px;
        box-shadow: 0 2px 4px rgba(0,0,0,0.1);
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
        border-color: #28a745;
        box-shadow: 0 0 0 3px rgba(40, 167, 69, 0.1);
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
      }

      .btn-primary {
        background: #28a745;
        color: white;
      }

      .btn-primary:hover {
        background: #218838;
      }

      .btn-secondary {
        background: #6c757d;
        color: white;
      }

      .btn-secondary:hover {
        background: #545b62;
      }

      .btn-outline {
        background: transparent;
        color: #007bff;
        border: 1px solid #007bff;
      }

      .btn-outline:hover {
        background: #007bff;
        color: white;
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

      .preview-mode {
        background: #f8f9fa;
        padding: 1rem;
        border-radius: 4px;
        border: 1px solid #e9ecef;
        min-height: 300px;
      }

      .preview-mode h1, .preview-mode h2, .preview-mode h3 {
        margin-top: 0;
      }

      .preview-mode p {
        line-height: 1.6;
      }

      .toolbar {
        display: flex;
        gap: 0.5rem;
        margin-bottom: 0.5rem;
        padding: 0.5rem;
        background: #f8f9fa;
        border: 1px solid #e9ecef;
        border-bottom: none;
        border-radius: 4px 4px 0 0;
      }

      .toolbar button {
        padding: 0.25rem 0.5rem;
        border: 1px solid #ddd;
        background: white;
        border-radius: 3px;
        cursor: pointer;
        font-size: 0.875rem;
      }

      .toolbar button:hover {
        background: #e9ecef;
      }

      .char-count {
        font-size: 0.875rem;
        color: #666;
        text-align: right;
        margin-top: 0.25rem;
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
    
    container.innerHTML = `
      <h1>✍️ Створити новий пост</h1>
      
      <form id="create-post-form">
        <div class="form-group">
          <label for="title">Заголовок поста</label>
          <input type="text" id="title" name="title" placeholder="Введіть заголовок..." required>
        </div>
        
        <div class="form-row">
          <div class="form-group">
            <label for="category">Категорія</label>
            <select id="category" name="category" required>
              <option value="">Оберіть категорію</option>
              <option value="tech">Технології</option>
              <option value="lifestyle">Стиль життя</option>
              <option value="travel">Подорожі</option>
              <option value="food">Їжа</option>
              <option value="other">Інше</option>
            </select>
          </div>
          
          <div class="form-group">
            <label for="status">Статус</label>
            <select id="status" name="status" required>
              <option value="draft">Чернетка</option>
              <option value="published">Опублікувати</option>
            </select>
          </div>
        </div>
        
        <div class="form-group">
          <label for="summary">Короткий опис</label>
          <textarea id="summary" name="summary" placeholder="Короткий опис поста..." rows="3"></textarea>
          <div class="char-count" id="summary-count">0/200</div>
        </div>
        
        <div class="form-group">
          <label for="tags">Теги</label>
          <div class="tags-input" id="tags-input">
            <input type="text" class="tag-input" placeholder="Додайте тег і натисніть Enter...">
          </div>
        </div>
        
        <div class="form-group">
          <label for="content">Контент</label>
          <div class="toolbar">
            <button type="button" data-action="bold">B</button>
            <button type="button" data-action="italic">I</button>
            <button type="button" data-action="heading">H</button>
            <button type="button" data-action="link">🔗</button>
            <button type="button" data-action="preview" id="preview-btn">👁️ Попередній перегляд</button>
          </div>
          <textarea id="content" name="content" class="content-editor" 
                    placeholder="Напишіть ваш пост тут..." required></textarea>
          <div class="char-count" id="content-count">0 символів</div>
        </div>
        
        <div class="actions">
          <div>
            <button type="button" class="btn btn-outline" id="save-draft-btn">💾 Зберегти як чернетку</button>
          </div>
          <div>
            <button type="button" class="btn btn-secondary" id="cancel-btn">Скасувати</button>
            <button type="submit" class="btn btn-primary">🚀 Опублікувати</button>
          </div>
        </div>
      </form>
    `;

    this.shadow.appendChild(styles);
    this.shadow.appendChild(container);
    
    console.log('CreatePostPage render завершено');
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
      
      if (target.id === 'save-draft-btn') {
        this.saveDraft();
      } else if (target.id === 'cancel-btn') {
        this.cancel();
      } else if (target.id === 'preview-btn') {
        this.togglePreview();
      } else if (target.classList.contains('tag-remove')) {
        this.removeTag(target);
      } else if (target.dataset.action) {
        this.handleToolbarAction(target.dataset.action);
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

    // Підрахунок символів
    this.shadow.addEventListener('input', (event) => {
      const target = event.target as HTMLInputElement;
      
      if (target.id === 'summary') {
        this.updateCharCount('summary-count', target.value, 200);
      } else if (target.id === 'content') {
        this.updateCharCount('content-count', target.value);
      }
    });
  }

  /**
   * Обробка відправки форми
   */
  private async handleSubmit(event: Event): Promise<void> {
    if (this.isLoading) return;

    const form = event.target as HTMLFormElement;
    const formData = new FormData(form);

    try {
      this.isLoading = true;
      this.errorMessage = '';
      this.updateSubmitButton();

      // Збираємо теги
      const tags = this.getTags();

      const postData = {
        title: formData.get('title') as string,
        content: formData.get('content') as string,
        summary: formData.get('summary') as string,
        tags: tags,
        isPublished: formData.get('status') === 'published'
      };

      console.log('Створення поста:', postData);

      // Створюємо пост через API
      const createdPost = await apiService.createPost(postData);

      console.log('Пост створено:', createdPost);

      // Перенаправляємо на сторінку створеного поста
      window.history.pushState(null, '', `/posts/${createdPost.id}`);
      window.dispatchEvent(new PopStateEvent('popstate'));

    } catch (error) {
      console.error('Помилка створення поста:', error);
      this.errorMessage = error instanceof Error ? error.message : 'Помилка створення поста';
      this.showError();
    } finally {
      this.isLoading = false;
      this.updateSubmitButton();
    }
  }

  /**
   * Збереження як чернетка
   */
  private saveDraft(): void {
    console.log('Збереження чернетки');
    alert('Чернетка збережена!');
  }

  /**
   * Скасування створення
   */
  private cancel(): void {
    if (confirm('Ви впевнені, що хочете скасувати? Незбережені зміни будуть втрачені.')) {
      window.history.pushState(null, '', '/');
      window.dispatchEvent(new PopStateEvent('popstate'));
    }
  }

  /**
   * Перемикання попереднього перегляду
   */
  private togglePreview(): void {
    const contentTextarea = this.shadowQuery('#content') as HTMLTextAreaElement;
    const previewBtn = this.shadowQuery('#preview-btn') as HTMLButtonElement;
    
    if (contentTextarea.classList.contains('preview-mode')) {
      // Повернутися до редагування
      contentTextarea.classList.remove('preview-mode');
      contentTextarea.style.display = 'block';
      previewBtn.textContent = '👁️ Попередній перегляд';
    } else {
      // Показати попередній перегляд
      contentTextarea.classList.add('preview-mode');
      contentTextarea.style.display = 'none';
      previewBtn.textContent = '✏️ Редагувати';
      
      // Тут можна додати рендеринг markdown
      console.log('Попередній перегляд контенту');
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

  /**
   * Обробка дій панелі інструментів
   */
  private handleToolbarAction(action: string): void {
    console.log('Дія панелі інструментів:', action);
    // Тут можна додати функціональність форматування тексту
  }

  /**
   * Оновлення лічильника символів
   */
  private updateCharCount(counterId: string, text: string, maxLength?: number): void {
    const counter = this.shadowQuery(`#${counterId}`) as HTMLElement;
    if (counter) {
      if (maxLength) {
        counter.textContent = `${text.length}/${maxLength}`;
        if (text.length > maxLength) {
          counter.style.color = '#dc3545';
        } else {
          counter.style.color = '#666';
        }
      } else {
        counter.textContent = `${text.length} символів`;
      }
    }
  }

  /**
   * Оновлення кнопки відправки
   */
  private updateSubmitButton(): void {
    const submitBtn = this.shadowQuery('button[type="submit"]') as HTMLButtonElement;
    if (submitBtn) {
      submitBtn.disabled = this.isLoading;
      submitBtn.textContent = this.isLoading ? '⏳ Створення...' : '🚀 Опублікувати';
    }
  }

  /**
   * Показати помилку
   */
  private showError(): void {
    if (!this.errorMessage) return;

    const form = this.shadowQuery('#create-post-form');
    if (form) {
      // Видаляємо попередню помилку
      const existingError = form.querySelector('.error-message');
      if (existingError) {
        existingError.remove();
      }

      // Додаємо нову помилку
      const errorDiv = document.createElement('div');
      errorDiv.className = 'error-message';
      errorDiv.style.cssText = 'background: #f8d7da; color: #721c24; padding: 0.75rem; border-radius: 4px; margin-bottom: 1rem; border: 1px solid #f5c6cb;';
      errorDiv.textContent = this.errorMessage;

      form.insertBefore(errorDiv, form.firstChild);
    }
  }
}

// Реєструємо компонент
customElements.define('create-post-page', CreatePostPage);
