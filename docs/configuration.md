# Конфигурация Mayfly

## Сервер

| Параметр | Обязательный | По умолчанию | Описание |
|---|---|---|---|
| `Postgres:ConnectionString` | Да | | Строка подключения к PostgreSQL |
| `Storage:Provider` | Нет | `S3` | Хранилище файлов: `S3` или `YandexDisk` |
| `Storage:S3:ServiceUrl` | Для S3 | | Адрес S3-совместимого хранилища, например `http://localhost:9000` |
| `Storage:S3:AccessKey` | Для S3 | | Ключ доступа |
| `Storage:S3:SecretKey` | Для S3 | | Секретный ключ |
| `Storage:S3:BucketName` | Для S3 | | Имя существующего бакета |
| `Storage:S3:Region` | Нет | `us-east-1` | Регион бакета |
| `Storage:S3:ForcePathStyle` | Нет | `true` | Указывать бакет в пути запроса, а не в имени хоста |
| `Storage:YandexDisk:OAuthToken` | Для Яндекс Диска | | OAuth-токен с доступом к папке приложения |
| `Upload:MaxRequestBodySize` | Нет | `4294967296` (4 ГиБ) | Максимальный размер загружаемого файла в байтах |
| `Retention:RequireExpiration` | Нет | `false` | Запретить загрузку файла без срока хранения |
| `Retention:MaxLifetime` | Нет | Без ограничения | Максимальный срок хранения файла от момента загрузки, `д.чч:мм:сс` |
| `Cleanup:Interval` | Нет | `00:01:00` | Как часто запускается удаление файлов, `чч:мм:сс` |
| `Cleanup:Lease` | Нет | `00:05:00` | Время, отведённое на обработку файлов при удалении, `чч:мм:сс` |
| `Cleanup:ExhaustedFileDelay` | Нет | `01:00:00` | Через сколько после последнего разрешённого скачивания файл удаляется, `чч:мм:сс` |
| `Cleanup:MaxAttempts` | Нет | `15` | Сколько раз пытаться удалить файл |
| `PasswordRateLimit:TokenLimit` | Нет | `10` | Максимальное число запросов с паролем подряд |
| `PasswordRateLimit:TokensPerPeriod` | Нет | `1` | Сколько запросов с паролем возвращается за один период |
| `PasswordRateLimit:ReplenishmentPeriod` | Нет | `00:00:06` | Период, через который возвращаются запросы с паролем, `чч:мм:сс` |
| `Cors:AllowedOrigins` | Нет | Пустой список | Список адресов веб-интерфейса, которым разрешены запросы из браузера |
| `ASPNETCORE_FORWARDEDHEADERS_ENABLED` | Нет | `false` | Учитывать заголовок `X-Forwarded-For` |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | Нет | | Адрес приёмника логов по OTLP, например `http://localhost:4317` |

```json
{
  "Postgres": {
    "ConnectionString": "Host=localhost;Port=5432;Database=mayfly;Username=postgres;Password=postgres"
  },
  "Storage": {
    "Provider": "S3",
    "S3": {
      "ServiceUrl": "http://localhost:9000",
      "AccessKey": "minioadmin",
      "SecretKey": "minioadmin",
      "BucketName": "mayfly"
    }
  }
}
```

### Хранилище файлов

`Storage:Provider` выбирает хранилище. Проверяются только настройки выбранного.

#### S3

Подходит любое S3-совместимое хранилище: AWS S3, Yandex Object Storage, Garage, SeaweedFS, MinIO и другие. Бакет должен существовать. Для AWS S3 нужно указать регион бакета.

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
