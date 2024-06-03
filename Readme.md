# Проект для курса High-load-course

Для тестирования проекта вам необходимо:
1. Скачать docker-compose файл
   https://github.com/andrsuh/bombardier/blob/main/docker-compose.yml
2. Расскоментировать конфигурацию сервиса bombardier
3. Запустить docker-compose 
4. Перейти по адресу http://localhost:1234/swagger-ui/index.html
5. Выполнить метод POST /test/run с необходимой конфигурацией 
```json
{
  "serviceName": "test",
  "usersCount": 0, // количество пользователей
  "testCount": 0, // количество тестов для запуска
  "ratePerSecond": 0 // количество тестов, запускаемых в секунду
}
```
  


## Создать миграцию: 
dotnet ef migrations add <MIGRATION_NAME> --startup-project ".\OnlineShop.Api\" --project ".\OnlineShop.Persistence\" 

## Обновить БД:
dotnet ef database update --startup-project ".\OnlineShop.Api\" --project ".\OnlineShop.Persistence\" --connection "Host=localhost;Port=5482;Username=postgres;Password=postgres;Database=OnlineShop"
