import { ProductApi } from './api';
import type {Product, CreateProductRequest, UpdateProductRequest} from './types';

/**
 * Головний клас для управління CRUD операціями з товарами
 */
class ProductManager {
    private api: ProductApi;
    private currentEditingId: number | null = null;

    // DOM елементи
    private form: HTMLFormElement;
    private formTitle: HTMLHeadingElement;
    private messageContainer: HTMLDivElement;
    private loadingElement: HTMLDivElement;
    private productsGrid: HTMLDivElement;
    private submitBtn: HTMLButtonElement;
    private cancelBtn: HTMLButtonElement;

    // Поля форми
    private productIdInput: HTMLInputElement;
    private nameInput: HTMLInputElement;
    private descriptionInput: HTMLTextAreaElement;
    private priceInput: HTMLInputElement;
    private stockInput: HTMLInputElement;

    constructor() {
        this.api = new ProductApi();

        // Ініціалізація DOM елементів
        this.form = document.getElementById('product-form') as HTMLFormElement;
        this.formTitle = document.getElementById('form-title') as HTMLHeadingElement;
        this.messageContainer = document.getElementById('message-container') as HTMLDivElement;
        this.loadingElement = document.getElementById('loading') as HTMLDivElement;
        this.productsGrid = document.getElementById('products-grid') as HTMLDivElement;
        this.submitBtn = document.getElementById('submit-btn') as HTMLButtonElement;
        this.cancelBtn = document.getElementById('cancel-btn') as HTMLButtonElement;

        // Поля форми
        this.productIdInput = document.getElementById('product-id') as HTMLInputElement;
        this.nameInput = document.getElementById('product-name') as HTMLInputElement;
        this.descriptionInput = document.getElementById('product-description') as HTMLTextAreaElement;
        this.priceInput = document.getElementById('product-price') as HTMLInputElement;
        this.stockInput = document.getElementById('product-stock') as HTMLInputElement;

        this.initializeEventListeners();
        this.loadProducts();
    }

    /**
     * Ініціалізує обробники подій
     */
    private initializeEventListeners(): void {
        this.form.addEventListener('submit', this.handleFormSubmit.bind(this));
        this.cancelBtn.addEventListener('click', this.handleCancelEdit.bind(this));
    }

    /**
     * Завантажує та відображає всі товари
     */
    private async loadProducts(): Promise<void> {
        try {
            this.showLoading(true);
            const products = await this.api.getAllProducts();
            this.displayProducts(products);
        } catch (error) {
            this.showMessage(`Помилка завантаження товарів: ${error}`, 'error');
        } finally {
            this.showLoading(false);
        }
    }

    /**
     * Відображає товари в сітці
     */
    private displayProducts(products: Product[]): void {
        if (products.length === 0) {
            this.productsGrid.innerHTML = '<p class="loading">Товарів поки що немає. Додайте перший товар!</p>';
            this.productsGrid.style.display = 'block';
            return;
        }

        const productsHtml = products.map(product => this.createProductCard(product)).join('');
        this.productsGrid.innerHTML = productsHtml;
        this.productsGrid.style.display = 'grid';

        // Додаємо обробники подій для кнопок
        this.attachProductEventListeners();
    }

    /**
     * Створює HTML картку товару
     */
    private createProductCard(product: Product): string {
        const formattedPrice = product.price.toLocaleString('uk-UA', {
            style: 'currency',
            currency: 'UAH'
        });

        const createdDate = new Date(product.createdAt).toLocaleDateString('uk-UA');

        return `
            <div class="product-card" data-id="${product.id}">
                <div class="product-name">${this.escapeHtml(product.name)}</div>
                ${product.description ? `<div class="product-description">${this.escapeHtml(product.description)}</div>` : ''}
                <div class="product-details">
                    <span class="product-price">${formattedPrice}</span>
                    <span class="product-stock">На складі: ${product.stock} шт.</span>
                </div>
                <div class="product-meta">
                    <small>Створено: ${createdDate}</small>
                </div>
                <div class="product-actions">
                    <button class="btn btn-warning edit-btn" data-id="${product.id}">Редагувати</button>
                    <button class="btn btn-danger delete-btn" data-id="${product.id}">Видалити</button>
                </div>
            </div>
        `;
    }

    /**
     * Додає обробники подій для кнопок товарів
     */
    private attachProductEventListeners(): void {
        // Кнопки редагування
        const editButtons = this.productsGrid.querySelectorAll('.edit-btn');
        editButtons.forEach(button => {
            button.addEventListener('click', (e) => {
                const id = parseInt((e.target as HTMLButtonElement).dataset.id!);
                this.handleEditProduct(id);
            });
        });

        // Кнопки видалення
        const deleteButtons = this.productsGrid.querySelectorAll('.delete-btn');
        deleteButtons.forEach(button => {
            button.addEventListener('click', (e) => {
                const id = parseInt((e.target as HTMLButtonElement).dataset.id!);
                this.handleDeleteProduct(id);
            });
        });
    }

    /**
     * Обробляє відправку форми
     */
    private async handleFormSubmit(e: Event): Promise<void> {
        e.preventDefault();

        const productData: CreateProductRequest | UpdateProductRequest = {
            name: this.nameInput.value.trim(),
            description: this.descriptionInput.value.trim() || undefined,
            price: parseFloat(this.priceInput.value),
            stock: parseInt(this.stockInput.value) || 0
        };

        try {
            if (this.currentEditingId) {
                await this.api.updateProduct(this.currentEditingId, productData);
                this.showMessage('Товар успішно оновлено!', 'success');
            } else {
                await this.api.createProduct(productData);
                this.showMessage('Товар успішно створено!', 'success');
            }

            this.resetForm();
            this.loadProducts();
        } catch (error) {
            this.showMessage(`Помилка: ${error}`, 'error');
        }
    }

    /**
     * Обробляє редагування товару
     */
    private async handleEditProduct(id: number): Promise<void> {
        try {
            const product = await this.api.getProductById(id);
            this.fillFormForEdit(product);
        } catch (error) {
            this.showMessage(`Помилка завантаження товару: ${error}`, 'error');
        }
    }

    /**
     * Заповнює форму для редагування
     */
    private fillFormForEdit(product: Product): void {
        this.currentEditingId = product.id;
        this.productIdInput.value = product.id.toString();
        this.nameInput.value = product.name;
        this.descriptionInput.value = product.description || '';
        this.priceInput.value = product.price.toString();
        this.stockInput.value = product.stock.toString();

        this.formTitle.textContent = 'Редагувати товар';
        this.submitBtn.textContent = 'Оновити товар';
        this.submitBtn.className = 'btn btn-warning';
        this.cancelBtn.style.display = 'inline-block';

        // Прокрутка до форми
        this.form.scrollIntoView({ behavior: 'smooth' });
    }

    /**
     * Обробляє скасування редагування
     */
    private handleCancelEdit(): void {
        this.resetForm();
    }

    /**
     * Обробляє видалення товару
     */
    private async handleDeleteProduct(id: number): Promise<void> {
        if (!confirm('Ви впевнені, що хочете видалити цей товар?')) {
            return;
        }

        try {
            await this.api.deleteProduct(id);
            this.showMessage('Товар успішно видалено!', 'success');
            this.loadProducts();
        } catch (error) {
            this.showMessage(`Помилка видалення товару: ${error}`, 'error');
        }
    }

    /**
     * Скидає форму до початкового стану
     */
    private resetForm(): void {
        this.currentEditingId = null;
        this.form.reset();
        this.productIdInput.value = '';
        this.stockInput.value = '0';

        this.formTitle.textContent = 'Додати новий товар';
        this.submitBtn.textContent = 'Додати товар';
        this.submitBtn.className = 'btn btn-success';
        this.cancelBtn.style.display = 'none';
    }

    /**
     * Показує/приховує індикатор завантаження
     */
    private showLoading(show: boolean): void {
        this.loadingElement.style.display = show ? 'block' : 'none';
        this.productsGrid.style.display = show ? 'none' : 'grid';
    }

    /**
     * Показує повідомлення користувачу
     */
    private showMessage(message: string, type: 'success' | 'error'): void {
        this.messageContainer.innerHTML = `<div class="${type}">${message}</div>`;

        // Автоматично приховати повідомлення через 5 секунд
        setTimeout(() => {
            this.messageContainer.innerHTML = '';
        }, 5000);
    }

    /**
     * Екранує HTML символи для безпеки
     */
    private escapeHtml(text: string): string {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }
}

// Ініціалізація додатку після завантаження DOM
document.addEventListener('DOMContentLoaded', () => {
    new ProductManager();
});
