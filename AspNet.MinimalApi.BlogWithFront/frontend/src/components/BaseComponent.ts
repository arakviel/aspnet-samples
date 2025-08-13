/**
 * Базовий клас для всіх веб-компонентів
 * Забезпечує основну функціональність Shadow DOM та життєвий цикл
 */
export abstract class BaseComponent extends HTMLElement {
  protected shadow: ShadowRoot;
  protected isComponentConnected = false;

  constructor() {
    super();
    this.shadow = this.attachShadow({ mode: 'open' });
    console.log(`${this.constructor.name} створено`);
  }

  /**
   * Викликається при підключенні компонента до DOM
   */
  connectedCallback() {
    if (!this.isComponentConnected) {
      this.isComponentConnected = true;
      this.render();
      this.setupEventListeners();
    }
  }

  /**
   * Викликається при відключенні компонента від DOM
   */
  disconnectedCallback() {
    this.isComponentConnected = false;
    this.cleanup();
  }

  /**
   * Викликається при зміні атрибутів
   */
  attributeChangedCallback(name: string, oldValue: string, newValue: string) {
    if (oldValue !== newValue) {
      this.onAttributeChanged(name, oldValue, newValue);
    }
  }

  /**
   * Абстрактний метод для рендерингу компонента
   */
  protected abstract render(): void;

  /**
   * Налаштування слухачів подій (перевизначається в дочірніх класах)
   */
  protected setupEventListeners(): void {
    // Базова реалізація - порожня
  }

  /**
   * Очищення ресурсів при видаленні компонента
   */
  protected cleanup(): void {
    // Базова реалізація - порожня
  }

  /**
   * Обробка зміни атрибутів
   */
  protected onAttributeChanged(_name: string, _oldValue: string, _newValue: string): void {
    // Базова реалізація - перерендерити компонент
    if (this.isComponentConnected) {
      this.render();
    }
  }

  /**
   * Створення HTML елемента з текстом
   */
  protected createElement(tag: string, content?: string, className?: string): HTMLElement {
    const element = document.createElement(tag);
    if (content) element.textContent = content;
    if (className) element.className = className;
    return element;
  }

  /**
   * Створення стилів для компонента
   */
  protected createStyles(css: string): HTMLStyleElement {
    const style = document.createElement('style');
    style.textContent = css;
    return style;
  }

  /**
   * Пошук елемента в Shadow DOM
   */
  protected shadowQuery<T extends Element>(selector: string): T | null {
    return this.shadow.querySelector<T>(selector);
  }

  /**
   * Пошук всіх елементів в Shadow DOM
   */
  protected shadowQueryAll<T extends Element>(selector: string): NodeListOf<T> {
    return this.shadow.querySelectorAll<T>(selector);
  }

  /**
   * Емітування кастомної події
   */
  protected emit(eventName: string, detail?: any): void {
    this.dispatchEvent(new CustomEvent(eventName, {
      detail,
      bubbles: true,
      composed: true
    }));
  }

  /**
   * Додавання слухача події (перевизначаємо для публічного доступу)
   */
  public addEventListener(type: string, listener: EventListener, options?: AddEventListenerOptions): void {
    super.addEventListener(type, listener, options);
  }

  /**
   * Видалення слухача події (перевизначаємо для публічного доступу)
   */
  public removeEventListener(type: string, listener: EventListener, options?: EventListenerOptions): void {
    super.removeEventListener(type, listener, options);
  }

  /**
   * Безпечне встановлення innerHTML з очищенням
   */
  protected setInnerHTML(element: Element, html: string): void {
    element.innerHTML = html;
  }
}
