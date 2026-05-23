# Oracle e migrations

Este projeto esta preparado para usar Oracle com Entity Framework Core.

## Pacotes usados

- `Oracle.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Design`
- `Microsoft.EntityFrameworkCore.InMemory` apenas para desenvolvimento local

## Configurar credenciais

Abra `Olli.Api/appsettings.json` e troque o exemplo:

```json
"FiapOracle": "User Id=RM000000;Password=SUA_SENHA;Data Source=oracle.fiap.com.br:1521/ORCL;"
```

por seu RM e senha reais.

Antes de subir para o GitHub, volte esse campo para o exemplo acima ou use uma configuracao local fora do repositorio.

## Aplicar migrations no Oracle

```powershell
dotnet restore
dotnet ef database update --project Olli.Api --startup-project Olli.Api
```

O projeto usa `ApplyMigrationsOnStartup` como `false`. Assim, a API nao tenta criar ou atualizar tabelas automaticamente ao iniciar.

Se aparecer `ORA-28000`, a conta Oracle esta bloqueada e precisa ser desbloqueada pela FIAP/professor.

## Rodar localmente com banco em memoria

O arquivo `Olli.Api/appsettings.Development.json` define:

```json
"DatabaseProvider": "InMemory",
"SeedDatabase": true
```

Isso permite testar a API localmente sem depender do Oracle.

## Rodar usando Oracle

```powershell
$env:ASPNETCORE_ENVIRONMENT="Production"
dotnet run --project Olli.Api
```
