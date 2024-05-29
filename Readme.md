# Проект для курса

Код написан синхронно. На моем ноуте падает даже со следующими параметрами (rps всего-лишь 5 запросов!):
{
"usersCount": 5,
"testCount": 250,
"ratePerSecond": 5
}

## Создать миграцию: 
dotnet ef migrations add <MIGRATION_NAME> --startup-project ".\OnlineShop.Api\" --project ".\OnlineShop.Persistence\" 

## Обновить БД:
dotnet ef database update --startup-project ".\OnlineShop.Api\" --project ".\OnlineShop.Persistence\" --connection "Host=localhost;Port=5482;Username=postgres;Password=postgres;Database=OnlineShop"
