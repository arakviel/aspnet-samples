const template = document.createElement('template');
template.innerHTML = `
  <style>
    :host([open]) .overlay {
      display: flex;
    }
    .overlay {
      display: none;
      position: fixed;
      top: 0; left: 0;
      width: 100%; height: 100%;
      background-color: rgba(0,0,0,0.6);
      justify-content: center;
      align-items: center;
      z-index: 1000;
    }
    .modal {
      background: white;
      padding: 30px;
      border-radius: 8px;
      max-width: 700px;
      width: 90%;
      position: relative;
    }
    .close-btn {
      position: absolute;
      top: 10px; right: 20px;
      font-size: 28px;
      cursor: pointer;
    }
    .content {
      display: flex;
      gap: 20px;
    }
    .content img {
      max-width: 250px;
      border-radius: 4px;
    }
    table { width: 100%; border-collapse: collapse; }
    td { padding: 8px; border-bottom: 1px solid #eee; }
    td:first-child { font-weight: bold; width: 100px; }
  </style>
  <div class="overlay">
    <div class="modal">
      <span class="close-btn">×</span>
      <div class="modal-body"></div>
    </div>
  </div>
`;

export class MovieModal extends HTMLElement {
    constructor() {
        super();
        this.attachShadow({mode: 'open'});
        this.shadowRoot.appendChild(template.content.cloneNode(true));
        this.shadowRoot.querySelector('.close-btn').addEventListener('click', this.close.bind(this));
        this.shadowRoot.querySelector('.overlay').addEventListener('click', (e) => {
            if (e.target === this.shadowRoot.querySelector('.overlay')) {
                this.close();
            }
        });
    }

    open() {
        this.setAttribute('open', '');
    }

    close() {
        this.removeAttribute('open');
    }

    setLoading() {
        this.shadowRoot.querySelector('.modal-body').innerHTML = '<loading-spinner></loading-spinner>';
    }

    render(movie) {
        // Підтримуємо обидва формати - з великої та маленької літери
        const poster = (movie.Poster || movie.poster) === 'N/A' || !(movie.Poster || movie.poster) ? 'https://via.placeholder.com/250x370?text=No+Image' : (movie.Poster || movie.poster);
        const plot = movie.Plot || movie.plot || 'Опис недоступний';
        const released = movie.Released || movie.released || 'Невідомо';
        const genre = movie.Genre || movie.genre || 'Невідомо';
        const director = movie.Director || movie.director || 'Невідомо';
        const actors = movie.Actors || movie.actors || 'Невідомо';
        const rating = (movie.ImdbRating || movie.imdbRating) ? `${movie.ImdbRating || movie.imdbRating}/10` : 'Невідомо';
        const runtime = movie.Runtime || movie.runtime || 'Невідомо';
        const language = movie.Language || movie.language || 'Невідомо';
        const country = movie.Country || movie.country || 'Невідомо';
        const title = movie.Title || movie.title || 'Невідома назва';
        const year = movie.Year || movie.year || '';

        this.shadowRoot.querySelector('.modal-body').innerHTML = `
      <div class="content">
          <img src="${poster}" alt="${title}">
          <div>
            <h3>${title} (${year})</h3>
            <p>${plot}</p>
            <table>
              <tr><td>Released:</td><td>${released}</td></tr>
              <tr><td>Runtime:</td><td>${runtime}</td></tr>
              <tr><td>Genre:</td><td>${genre}</td></tr>
              <tr><td>Director:</td><td>${director}</td></tr>
              <tr><td>Actors:</td><td>${actors}</td></tr>
              <tr><td>Language:</td><td>${language}</td></tr>
              <tr><td>Country:</td><td>${country}</td></tr>
              <tr><td>IMDB Rating:</td><td>${rating}</td></tr>
            </table>
          </div>
      </div>
    `;
    }
}
