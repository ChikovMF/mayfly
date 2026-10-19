# Конфигурация Mayfly

## Сервер

| Параметр | Обязательный | По умолчанию | Описание |
|---|---|---|---|
| `Postgres:ConnectionString` | Да | | Строка подключения к PostgreSQL |
| `Storage:Provider` | Да | | Хранилище файлов: `Minio` |
| `Storage:Minio:Endpoint` | Для MinIO | | Адрес MinIO, например `localhost:9000` |
| `Storage:Minio:AccessKey` | Для MinIO | | Ключ доступа MinIO |
| `Storage:Minio:SecretKey` | Для MinIO | | Секретный ключ MinIO |
| `Storage:Minio:UseSsl` | Нет | `false` | Подключаться к MinIO по HTTPS |
| `Storage:Minio:BucketName` | Для MinIO | | Имя существующего бакета |
| `Upload:MaxRequestBodySize` | Нет | `134217728` (128 МиБ) | Максимальный размер загружаемого файла в байтах |
| `Cleanup:Interval` | Нет | `00:01:00` | Интервал между удалениями файлов с истёкшим сроком хранения, `чч:мм:сс` |
| `Cleanup:Lease` | Нет | `00:05:00` | Время на обработку файлов одним экземпляром сервера, `чч:мм:сс` |
| `Cleanup:MaxAttempts` | Нет | `15` | Максимальное число попыток удалить файл |

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
