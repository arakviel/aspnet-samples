export class MovieService {
    constructor() {
        this.baseUrl = '/api/movies';
    }

    async search(title, page = 1) {
        const url = `${this.baseUrl}/search?s=${encodeURIComponent(title)}&page=${page}`;
        const response = await fetch(url);

        if (!response.ok) {
            throw new Error(`Помилка пошуку: ${response.status}`);
        }

        return response.json();
    }

    async getMovie(movieId) {
        const url = `${this.baseUrl}/${movieId}`;
        const response = await fetch(url);

        if (!response.ok) {
            throw new Error(`Помилка отримання деталей фільму: ${response.status}`);
        }

        return response.json();
    }
}
