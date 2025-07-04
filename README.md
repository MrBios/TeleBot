# TeleBot

TeleBot — WPF-приложение для работы с Telegram при помощи инструментов с простым интерфейсом.  
Каждый инструмент реализован как отдельная WPF-страница (Page).  
Проект пригоден для автоматизации базовых задач: рассылка сообщений, получение пользователей и прочее.

## Структура

- Инструменты оформлены как отдельные страницы (Page)
- Чтобы добавить новый инструмент, достаточно создать новую страницу и зарегистрировать её в приложении

## Реализованные инструменты

| Инструмент | Описание |
|-------------------------|----------------------------|
| Получение групп и каналов | Получение полного списка групп и каналов для получения ID |
| Получение чатов | Получение полного списка личных чатов с пользователями для получения ID |
| Массовая отправка сообщений пользователям и ботам | Массовая рассылка текста пользователям в сообщения по ID |

## Требования

- Windows 10/11
- .NET 8.0 или выше
- Telegram API ID и Hash

## Запуск

1. Клонируйте репозиторий
2. Откройте решение в Visual Studio
3. Соберите и запустите проект

## Примечания

- Для работы потребуется указать свои api_id и api_hash для Telegram

## Обратная связь

Вопросы и предложения — через issues на GitHub или в Telegram @mrbios9.
