# MirraAdminDashboard

Простой демонстрационный админ‑дашборд, состоящий из backend (ASP.NET Core 8 + PostgreSQL) и frontend (React + Vite).

## 🚀 Описание

### ✅ Что реализовано

* ✅ ASP.NET Core 8 minimal API + PostgreSQL (файл + сиды: 3 клиента, 5 платежей, курс = 10)
* ✅ React + Vite фронтенд с авторизацией и блоком информации
* ✅ Коммуникация frontend ↔ backend по REST

Идея кода: показать минимальный снапшот умения строить REST API на .NET и связывать с простым React-интерфейсом.

## 📦 Требования

- .NET 8 SDK
- NodeJS (>=16) и npm
- Docker Desktop (для запуска через Docker Compose)

## 🧩 Быстрый запуск

### Через Docker Compose

1. Убедитесь, что Docker Desktop запущен.
2. Перейдите в папку `backend/AdminDashboard`.
3. В Visual Studio выберите конфигурацию `Docker Compose` (значок шестерёнки).
4. Нажмите ▶ Run Docker Compose — дождитесь, пока соберутся все контейнеры.
5. Откройте браузер: `http://localhost:5173`

## 🔐 Авторизация

* Email: `admin@mirra.dev`
* Пароль: `admin123`

## 🖥 Frontend

* `/login` — форма логина
* `/dashboard` — виджет курса токенов + таблица клиентов (Name, Email, BalanceT); можно обновлять курс
