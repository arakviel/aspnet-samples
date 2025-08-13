// Додаємо базові стилі
import './styles/main.css';

// Імпортуємо сервіси
import './services/ApiService';
import './services/AuthService';

// Імпортуємо компоненти
import './components/BlogApp';
import './pages/HomePage';
import './pages/LoginPageNew';
import './pages/RegisterPageNew';
import './pages/ConfirmEmailPageNew';
import './pages/ForgotPasswordPageNew';
import './pages/ProfilePage';
import './pages/CreatePostPage';
import './pages/PostPage';
import './pages/ResetPasswordPageNew';
import './pages/EditPostPage';

console.log('🚀 Блог додаток запущено!');

// Ініціалізуємо додаток після завантаження DOM
document.addEventListener('DOMContentLoaded', () => {
  console.log('DOM завантажено, ініціалізуємо додаток');

  const app = document.getElementById('app');
  if (app) {
    console.log('Знайдено #app, створюємо blog-app');
    app.innerHTML = '<blog-app></blog-app>';
  } else {
    console.error('Не знайдено елемент #app');
  }
});
