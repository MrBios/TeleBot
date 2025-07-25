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
| Получение сообщений | Получение всех сообщений из чатов\групп\каналов вместе с медиа (сохранение в json) |
| Массовая отправка текстовых сообщений | Массовая рассылка текста в сообщения и каналы по ID |
| Массовая отправка сообщений из файлов | Массовая отправка сообщений (с медиа) из файлов, полученных из инструмента "Получение сообщений" |
| Проверка сообщений | Открытие сообщений из файлов для просмотра их содержимого и медиа в удобном формате |

## Требования

- Windows 10/11
- [.NET 8.0 или выше](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [Telegram API ID и Hash](https://my.telegram.org/auth?to=apps)

## Запуск

1. Клонируйте репозиторий
2. Откройте решение в Visual Studio
3. создайте в корне secret-config.json
4. добавьте в него свой api_id и api_hash
   ```
   {
      "api_id": "123123",
      "api_hash": "abcd123abcd"
   }
5. Соберите и запустите проект

## Примечания

- Для работы потребуется указать свои api_id и api_hash для Telegram

## Обратная связь

Вопросы и предложения — через issues на GitHub или в Telegram @mrbios9.
