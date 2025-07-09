const template = document.createElement('template');
template.innerHTML = `
  <style>
    form {
      display: flex;
      gap: 10px;
      margin-bottom: 2rem;
    }

    input {
      flex-grow: 1;
      padding: 10px;
      font-size: 16px;
      border: 1px solid #ccc;
      border-radius: 4px;
    }

    button {
      padding: 10px 20px;
      font-size: 16px;
      background-color: #007bff;
      color: white;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      transition: background-color 0.2s;
    }

    button:hover {
      background-color: #0056b3;
    }
  </style>
  <form>
    <input type="text" placeholder="Введіть назву фільму..." required>
    <button type="submit">Пошук</button>
  </form>
`;

export class MovieSearchForm extends HTMLElement {
  constructor() {
    super();
    this.attachShadow({ mode: 'open' });
    this.shadowRoot.appendChild(template.content.cloneNode(true));


    this.shadowRoot.querySelector('form').addEventListener('submit', (e) => {
      e.preventDefault();

      const query = this.shadowRoot.querySelector('input').value;

      if (query) {
        this.dispatchEvent(new CustomEvent('search', {
          bubbles: true,
          composed: true,
          detail: { query }
        }));
      }
    });
  }
}
