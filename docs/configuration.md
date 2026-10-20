# Конфигурация Mayfly

## Сервер

| Параметр | Обязательный | По умолчанию | Описание |
|---|---|---|---|
| `Postgres:ConnectionString` | Да | | Строка подключения к PostgreSQL |
| `Storage:Provider` | Да | | Хранилище файлов: `Minio` или `YandexDisk` |
| `Storage:Minio:Endpoint` | Для MinIO | | Адрес MinIO, например `localhost:9000` |
| `Storage:Minio:AccessKey` | Для MinIO | | Ключ доступа MinIO |
| `Storage:Minio:SecretKey` | Для MinIO | | Секретный ключ MinIO |
| `Storage:Minio:UseSsl` | Нет | `false` | Подключаться к MinIO по HTTPS |
| `Storage:Minio:BucketName` | Для MinIO | | Имя существующего бакета |
| `Storage:YandexDisk:OAuthToken` | Для Яндекс Диска | | OAuth-токен с доступом к папке приложения |
| `Upload:MaxRequestBodySize` | Нет | `4294967296` (4 ГиБ) | Максимальный размер загружаемого файла в байтах |
| `Cleanup:Interval` | Нет | `00:01:00` | Интервал между удалениями файлов с истёкшим сроком хранения, `чч:мм:сс` |
| `Cleanup:Lease` | Нет | `00:05:00` | Время на обработку файлов одним экземпляром сервера, `чч:мм:сс` |
| `Cleanup:MaxAttempts` | Нет | `15` | Максимальное число попыток удалить файл |
| `PasswordRateLimit:TokenLimit` | Нет | `10` | Размер запаса запросов с паролем для одного адреса |
| `PasswordRateLimit:TokensPerPeriod` | Нет | `1` | Сколько запросов возвращается в запас за один период |
| `PasswordRateLimit:ReplenishmentPeriod` | Нет | `00:00:06` | Период пополнения запаса, `чч:мм:сс` |

```json
{
  "Postgres": {
    "ConnectionString": "Host=localhost;Port=5432;Database=mayfly;Username=postgres;Password=postgres"
  },
  "Storage": {
    "Provider": "Minio",
    "Minio": {
      "Endpoint": "localhost:9000",
      "AccessKey": "minioadmin",
      "SecretKey": "minioadmin",
      "BucketName": "mayfly"
    }
  }
}
```

### Хранилище файлов

`Storage:Provider` выбирает хранилище. Проверяются только настройки выбранного. Без провайдера сервер не запускается.

#### MinIO

Бакет должен существовать.

#### Яндекс Диск

[Получить токен](https://oauth.yandex.ru/authorize?response_type=token&client_id=a987531e91c84cfaa24a8427429fcab5). Токен не обновляется, после истечения его нужно заменить.

```json
{
  "Storage": {
    "Provider": "YandexDisk",
    "YandexDisk": {
      "OAuthToken": "<токен>"
    }
  }
}
```

## Консольное приложение

| Параметр | Обязательный | Описание |
|---|---|---|
| Адрес сервера | Да | Задаётся командой [`config set server`](terminal.md#config-set-server) |

## Клиентская библиотека

| Параметр | Обязательный | По умолчанию | Описание |
|---|---|---|---|
| `Mayfly:BaseAddress` | Да | | Адрес сервера |
| `Mayfly:Timeout` | Нет | Без ограничения | Максимальное время выполнения запроса, `чч:мм:сс` |

```json
{
  "Mayfly": {
    "BaseAddress": "http://localhost:5015"
  }
}
```
