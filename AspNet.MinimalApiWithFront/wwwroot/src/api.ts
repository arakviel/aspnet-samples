import type {Product, CreateProductRequest, UpdateProductRequest, ErrorResponse} from './types';

/**
 * Клас для роботи з API товарів
 */
export class ProductApi {
    private readonly baseUrl: string;

    constructor(baseUrl: string = '/api') {
        this.baseUrl = baseUrl;
    }

    /**
     * Отримує всі товари
     */
    async getAllProducts(): Promise<Product[]> {
        const response = await fetch(`${this.baseUrl}/products`);
        
        if (!response.ok) {
            throw new Error(`Помилка завантаження товарів: ${response.status}`);
        }
        
        return await response.json();
    }

    /**
     * Отримує товар за ідентифікатором
     */
    async getProductById(id: number): Promise<Product> {
        const response = await fetch(`${this.baseUrl}/products/${id}`);
        
        if (!response.ok) {
            if (response.status === 404) {
                throw new Error('Товар не знайдено');
            }
            throw new Error(`Помилка завантаження товару: ${response.status}`);
        }
        
        return await response.json();
    }

    /**
     * Створює новий товар
     */
    async createProduct(product: CreateProductRequest): Promise<Product> {
        const response = await fetch(`${this.baseUrl}/products`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(product),
        });

        if (!response.ok) {
            if (response.status === 400) {
                const errorData: ErrorResponse = await response.json();
                throw new Error(errorData.error);
            }
            throw new Error(`Помилка створення товару: ${response.status}`);
        }

        return await response.json();
    }

    /**
     * Оновлює існуючий товар
     */
    async updateProduct(id: number, product: UpdateProductRequest): Promise<Product> {
        const response = await fetch(`${this.baseUrl}/products/${id}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(product),
        });

        if (!response.ok) {
            if (response.status === 404) {
                throw new Error('Товар не знайдено');
            }
            if (response.status === 400) {
                const errorData: ErrorResponse = await response.json();
                throw new Error(errorData.error);
            }
            throw new Error(`Помилка оновлення товару: ${response.status}`);
        }

        return await response.json();
    }

    /**
     * Видаляє товар
     */
    async deleteProduct(id: number): Promise<void> {
        const response = await fetch(`${this.baseUrl}/products/${id}`, {
            method: 'DELETE',
        });

        if (!response.ok) {
            if (response.status === 404) {
                throw new Error('Товар не знайдено');
            }
            throw new Error(`Помилка видалення товару: ${response.status}`);
        }
    }
}
