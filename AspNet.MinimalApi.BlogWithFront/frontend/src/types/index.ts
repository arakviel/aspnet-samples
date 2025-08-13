// Типи для користувача
export interface User {
  id: string;
  email: string;
  name: string;
  roles: string[];
  emailConfirmed: boolean;
  twoFactorEnabled: boolean;
  createdAt: string;
}

// Типи для аутентифікації
export interface LoginRequest {
  email: string;
  password: string;
  rememberMe?: boolean;
  twoFactorCode?: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  confirmPassword: string;
  name: string;
}

export interface AuthResponse {
  token: string;
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: User;
  requiresTwoFactor?: boolean;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  email: string;
  token: string;
  password: string;
  confirmPassword: string;
}

export interface ConfirmEmailRequest {
  userId: string;
  token: string;
}

export interface TwoFactorSetupResponse {
  qrCodeUri: string;
  manualEntryKey: string;
}

export interface EnableTwoFactorRequest {
  code: string;
}

// Типи для блогу
export interface BlogPost {
  id: string;
  title: string;
  content: string;
  summary?: string;
  authorId: string;
  authorName: string;
  createdAt: string;
  updatedAt: string;
  isPublished: boolean;
  tags: string[];
  commentsCount: number;
}

export interface CreatePostRequest {
  title: string;
  content: string;
  summary?: string;
  isPublished: boolean;
  tags: string[];
}

export interface UpdatePostRequest extends CreatePostRequest {
  id: string;
}

export interface Comment {
  id: string;
  postId: string;
  authorId: string;
  authorName: string;
  content: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateCommentRequest {
  postId: string;
  content: string;
}

// Типи для API відповідей
export interface ApiResponse<T = any> {
  success: boolean;
  data?: T;
  message?: string;
  errors?: string[];
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

// Типи для роутера
export interface Route {
  path: string;
  component: string;
  title?: string;
  requiresAuth?: boolean;
  requiresEmailConfirmed?: boolean;
  roles?: string[];
}

export interface RouteParams {
  [key: string]: string;
}

// Типи для подій
export interface NavigationEvent extends CustomEvent {
  detail: {
    path: string;
    params?: RouteParams;
  };
}

export interface AuthStateChangeEvent extends CustomEvent {
  detail: {
    isAuthenticated: boolean;
    user?: User;
  };
}

// Типи для форм
export interface FormField {
  name: string;
  type: 'text' | 'email' | 'password' | 'textarea' | 'checkbox' | 'select';
  label: string;
  placeholder?: string;
  required?: boolean;
  validation?: (value: string) => string | null;
  options?: { value: string; label: string }[];
}

export interface FormData {
  [key: string]: string | boolean;
}

// Типи для повідомлень
export type MessageType = 'success' | 'error' | 'warning' | 'info';

export interface Message {
  id: string;
  type: MessageType;
  text: string;
  duration?: number;
}
