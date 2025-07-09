import {MovieService} from '../services/MovieService.js';

const template = document.createElement('template');
template.innerHTML = `
    <style>
        .container {
            max-width: 1200px;
            margin: auto;
        }
    </style>
    <div class="container">
        <h1>Пошук фільмів</h1>
        <movie-search-form></movie-search-form>
        <movie-list></movie-list>
        <movie-modal></movie-modal>
    </div>
`;

export class MovieApp extends HTMLElement {
    constructor() {
        super();
        this.attachShadow({mode: 'open'});
        this.shadowRoot.appendChild(template.content.cloneNode(true));
        this.movieService = new MovieService();

        this.movieList = this.shadowRoot.querySelector('movie-list');
        this.modal = this.shadowRoot.querySelector('movie-modal');

        this.state = {
            currentPage: 1,
            query: ''
        };
    }

    connectedCallback() {
        this.addEventListener('search', this.handleSearch.bind(this));
        this.addEventListener('load-more', this.handleLoadMore.bind(this));
        this.addEventListener('show-details', this.handleShowDetails.bind(this));
    }

    async handleSearch(e) {
        this.state.query = e.detail.query;
        this.state.currentPage = 1;
        this.movieList.setLoading(true);

        try {
            const data = await this.movieService.search(this.state.query, this.state.currentPage);
            console.log(data)

            this.movieList.update({
                movies: data.search,
                append: false,
                totalResults: data.totalResults,
                currentPage: this.state.currentPage
            });
        } catch (error) {
            console.error('Помилка пошуку:', error);
            this.movieList.update({
                movies: [],
                append: false,
                totalResults: "0",
                currentPage: this.state.currentPage
            });
        }
    }

    async handleLoadMore() {
        this.state.currentPage++;
        this.movieList.setLoading(true, true);

        try {
            const data = await this.movieService.search(this.state.query, this.state.currentPage);

            this.movieList.update({
                movies: data.search,
                append: true,
                totalResults: data.totalResults,
                currentPage: this.state.currentPage
            });
        } catch (error) {
            console.error('Помилка завантаження додаткових фільмів:', error);
        }

        this.movieList.setLoading(false, true);
    }

    async handleShowDetails(e) {
        const movieId = e.detail.movieId;
        this.modal.open();
        this.modal.setLoading();

        try {
            const movieDetails = await this.movieService.getMovie(movieId);
            this.modal.render(movieDetails);
        } catch (error) {
            console.error('Помилка завантаження деталей фільму:', error);
            this.modal.render({
                Title: 'Помилка',
                Year: '',
                Plot: 'Не вдалося завантажити деталі фільму',
                Poster: 'N/A',
                Released: '',
                Genre: '',
                Director: '',
                Actors: ''
            });
        }
    }
}
