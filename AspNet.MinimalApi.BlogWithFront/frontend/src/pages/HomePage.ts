import { BaseComponent } from '../components/BaseComponent';
import { apiService } from '../services/ApiService';
import type { BlogPost } from '../types';

/**
 * Компонент головної сторінки
 */
export class HomePage extends BaseComponent {
  private posts: BlogPost[] = [];
  private loading = true;
  private errorMessage = '';

  constructor() {
    super();
  }

  connectedCallback(): void {
    super.connectedCallback();
    this.loadPosts();
  }

  protected render(): void {
    console.log('HomePage render викликано');
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
        margin: 0 0 1rem;
      }

      .posts {
        margin-top: 2rem;
      }

      .post-card {
        background: #f8f9fa;
        padding: 1rem;
        margin-bottom: 1rem;
        border-radius: 4px;
        border-left: 4px solid #007bff;
      }

      .post-title {
        font-size: 1.2rem;
        font-weight: bold;
        color: #333;
        margin: 0 0 0.5rem;
      }

      .post-meta {
        color: #666;
        font-size: 0.9rem;
        margin-bottom: 0.5rem;
      }

      .post-summary {
        color: #555;
        line-height: 1.5;
      }

      .post-card {
        cursor: pointer;
        transition: transform 0.2s, box-shadow 0.2s;
      }

      .post-card:hover {
        transform: translateY(-2px);
        box-shadow: 0 4px 8px rgba(0,0,0,0.15);
      }
    `);

    // Створюємо структуру сторінки
    const container = this.createElement('div', '', 'container');

    if (this.loading) {
      container.innerHTML = `
        <h1>📝 Останні пости</h1>
        <div style="text-align: center; padding: 3rem; color: #666;">
          ⏳ Завантаження постів...
        </div>
      `;
    } else if (this.errorMessage) {
      container.innerHTML = `
        <h1>📝 Останні пости</h1>
        <div style="background: #f8d7da; color: #721c24; padding: 1rem; border-radius: 4px; margin: 1rem 0;">
          ❌ ${this.errorMessage}
        </div>
        <button onclick="location.reload()" style="padding: 0.5rem 1rem; background: #007bff; color: white; border: none; border-radius: 4px; cursor: pointer;">
          Спробувати знову
        </button>
      `;
    } else if (this.posts.length === 0) {
      container.innerHTML = `
        <h1>📝 Останні пости</h1>
        <div style="text-align: center; padding: 3rem; color: #666;">
          📝 Поки що немає постів
        </div>
      `;
    } else {
      container.innerHTML = `
        <h1>📝 Останні пости</h1>
        <p>Ласкаво просимо до нашого блогу!</p>

        <div class="posts">
          ${this.posts.map(post => `
            <div class="post-card" data-post-id="${post.id}">
              <div class="post-title">${this.escapeHtml(post.title)}</div>
              <div class="post-meta">
                Автор: ${this.escapeHtml(post.authorName)} • ${this.formatDate(post.createdAt)}
                ${post.tags.length > 0 ? ` • ${post.tags.map(tag => `<span style="background: #e3f2fd; color: #1976d2; padding: 0.125rem 0.25rem; border-radius: 8px; font-size: 0.75rem;">${this.escapeHtml(tag)}</span>`).join(' ')}` : ''}
              </div>
              <div class="post-summary">${this.escapeHtml(post.summary || this.truncateText(post.content, 150))}</div>
            </div>
          `).join('')}
        </div>
      `;
    }

    this.shadow.appendChild(styles);
    this.shadow.appendChild(container);

    console.log('HomePage render завершено');
  }

  protected setupEventListeners(): void {
    console.log('HomePage setupEventListeners викликано');

    // Обробка кліків по картках постів
    this.shadow.addEventListener('click', (event) => {
      const target = event.target as HTMLElement;
      const postCard = target.closest('.post-card') as HTMLElement;

      if (postCard) {
        const postId = postCard.getAttribute('data-post-id');
        if (postId) {
          console.log('Перехід до поста:', postId);
          window.history.pushState(null, '', `/posts/${postId}`);
          window.dispatchEvent(new PopStateEvent('popstate'));
        }
      }
    });
  }

  /**
   * Завантаження постів з API
   */
  private async loadPosts(): Promise<void> {
    try {
      this.loading = true;
      this.errorMessage = '';
      this.render();

      const response = await apiService.getPosts(1, 10);
      this.posts = response.items;

      this.loading = false;
      this.render();
    } catch (error) {
      console.error('Помилка завантаження постів:', error);
      this.loading = false;
      this.errorMessage = error instanceof Error ? error.message : 'Помилка завантаження постів';
      this.render();
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
      day: 'numeric'
    });
  }

  /**
   * Екранування HTML
   */
  private escapeHtml(text: string): string {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
  }

  /**
   * Обрізання тексту
   */
  private truncateText(text: string, maxLength: number): string {
    if (text.length <= maxLength) return text;
    return text.substring(0, maxLength).trim() + '...';
  }
}

// Реєструємо компонент
customElements.define('home-page', HomePage);
