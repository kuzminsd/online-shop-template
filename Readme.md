# Проект для курса

## Создать миграцию: 
dotnet ef migrations add <MIGRATION_NAME> --startup-project ".\OnlineShop.Api\" --project ".\OnlineShop.Persistence\" 

## Обновить БД:
dotnet ef database update --startup-project ".\OnlineShop.Api\" --project ".\OnlineShop.Persistence\" --connection "Host=localhost;Port=5482;Username=postgres;Password=postgres;Database=OnlineShop"
