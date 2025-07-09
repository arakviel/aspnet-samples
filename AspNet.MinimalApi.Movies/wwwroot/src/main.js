import { MovieApp } from './components/MovieApp.js';
import { LoadingSpinner } from './components/LoadingSpinner.js';
import { MovieSearchForm } from './components/MovieSearchForm.js';
import { MovieList } from './components/MovieList.js';
import { MovieItem } from './components/MovieItem.js';
import { MovieModal } from './components/MovieModal.js';

customElements.define('loading-spinner', LoadingSpinner);
customElements.define('movie-search-form', MovieSearchForm);
customElements.define('movie-item', MovieItem);
customElements.define('movie-list', MovieList);
customElements.define('movie-modal', MovieModal);
customElements.define('movie-app', MovieApp);
