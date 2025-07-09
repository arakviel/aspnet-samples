const template = document.createElement('template');
template.innerHTML = `
  <style>
    .list {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
      gap: 20px;
    }
    .actions {
      text-align: center;
      margin-top: 2rem;
    }
    .actions button {
        padding: 10px 30px;
        font-size: 16px;
        background-color: #28a745;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
    }
    .actions button:disabled {
      background-color: #aaa;
    }
    .hidden {
      display: none;
    }
    .message {
      text-align: center;
      font-size: 1.2rem;
      color: #777;
    }
  </style>
  <div class="message"></div>
  <div class="list"></div>
  <div class="actions">
      <loading-spinner class="hidden"></loading-spinner>
      <button id="load-more-btn" class="hidden">Більше</button>
  </div>
`;

export class MovieList extends HTMLElement {
  constructor() {
    super();
    this.attachShadow({ mode: 'open' });
    this.shadowRoot.appendChild(template.content.cloneNode(true));
    this.shadowRoot.getElementById('load-more-btn').addEventListener('click', () => {
        this.dispatchEvent(new CustomEvent('load-more', { bubbles: true, composed: true }));
    });
  }

  update(data) {
      const { movies, append, totalResults, currentPage } = data;
      const list = this.shadowRoot.querySelector('.list');
      const messageEl = this.shadowRoot.querySelector('.message');

      if (!append) {
          list.innerHTML = '';
      }

      if (!movies || movies.length === 0) {
          messageEl.textContent = 'Фільми не знайдено.';
      } else {
          messageEl.textContent = '';
          movies.forEach(movie => {
              const movieItem = document.createElement('movie-item');
              movieItem.movie = movie;
              list.appendChild(movieItem);
          });
      }

      const moreBtn = this.shadowRoot.getElementById('load-more-btn');
      moreBtn.classList.toggle('hidden', totalResults <= currentPage * 10 || !movies || movies.length === 0);
  }

  setLoading(isLoading, forMore = false) {
      const moreBtn = this.shadowRoot.getElementById('load-more-btn');
      const spinner = this.shadowRoot.querySelector('loading-spinner');
      if (forMore) {
          moreBtn.classList.toggle('hidden', isLoading);
          spinner.classList.toggle('hidden', !isLoading);
      } else {
          const list = this.shadowRoot.querySelector('.list');
          if (isLoading) {
              list.innerHTML = ''; // Очищуємо список, щоб показати головний спіннер
              const mainSpinner = document.createElement('loading-spinner');
              list.appendChild(mainSpinner);
          }
      }
  }
}
