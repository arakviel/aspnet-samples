const template = document.createElement('template');
template.innerHTML = `
  <style>
    :host {
      display: block;
      border: 1px solid #ddd;
      border-radius: 8px;
      overflow: hidden;
      background-color: #fff;
      box-shadow: 0 2px 5px rgba(0,0,0,0.1);
    }
    img {
      width: 100%;
      height: 280px;
      object-fit: cover;
      background-color: #eee;
    }
    .info {
      padding: 15px;
    }
    h3 {
      margin: 0 0 10px;
      font-size: 1.1em;
    }
    p {
      margin: 0 0 15px;
      color: #666;
    }
    button {
      width: 100%;
      padding: 10px;
      background-color: transparent;
      color: #007bff;
      border: 1px solid #007bff;
      border-radius: 4px;
      cursor: pointer;
      transition: all 0.2s;
    }
    button:hover {
      background-color: #007bff;
      color: white;
    }
  </style>
  <article>
    <img src="" alt="Poster">
    <div class="info">
      <h3></h3>
      <p></p>
      <button>Деталі</button>
    </div>
  </article>
`;

export class MovieItem extends HTMLElement {
    constructor() {
        super();
        this.attachShadow({mode: 'open'});
        this.shadowRoot.appendChild(template.content.cloneNode(true));
    }

    set movie(data) {
        this._movie = data;
        this.render();
    }

    render() {
        // Підтримуємо обидва формати - з великої та маленької літери
        const poster = (this._movie.Poster || this._movie.poster) === 'N/A' || !(this._movie.Poster || this._movie.poster) ? 'https://via.placeholder.com/300x444?text=No+Image' : (this._movie.Poster || this._movie.poster);
        const title = this._movie.Title || this._movie.title || 'Невідома назва';
        const year = this._movie.Year || this._movie.year || '';
        const imdbId = this._movie.ImdbID || this._movie.imdbID || this._movie.imdbId;

        this.shadowRoot.querySelector('img').src = poster;
        this.shadowRoot.querySelector('img').alt = title;
        this.shadowRoot.querySelector('h3').textContent = title;
        this.shadowRoot.querySelector('p').textContent = year;
        this.shadowRoot.querySelector('button').addEventListener('click', () => {
            this.dispatchEvent(new CustomEvent('show-details', {
                bubbles: true,
                composed: true,
                detail: {movieId: imdbId}
            }));
        });
    }
}
