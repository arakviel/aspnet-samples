/**
 * Інтерфейс для товару
 */
export interface Product {
    id: number;
    name: string;
    description?: string;
    price: number;
    stock: number;
    createdAt: string;
    updatedAt: string;
}

/**
 * Інтерфейс для створення нового товару (без id та дат)
 */
export interface CreateProductRequest {
    name: string;
    description?: string;
    price: number;
    stock: number;
}

/**
 * Інтерфейс для оновлення товару
 */
export interface UpdateProductRequest {
    name: string;
    description?: string;
    price: number;
    stock: number;
}

/**
 * Інтерфейс для відповіді з помилкою
 */
export interface ErrorResponse {
    error: string;
}
