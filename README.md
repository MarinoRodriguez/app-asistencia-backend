# App Asistencia - Backend (API)

Backend en **ASP.NET Core (.NET 8)** para la gestión de asistencia a eventos. Expone endpoints REST para personas, grupos, eventos, invitaciones y asistencia. Incluye **ASP.NET Identity + JWT** para autenticación y autorización por permisos.

## Stack
- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- SQLite
- Identity + JWT

## Configuración
Variables principales (appsettings o env):
- `ConnectionStrings__Default` → cadena SQLite
- `Jwt__Key` → clave de firma
- `Jwt__Issuer` → issuer
- `Jwt__Audience` → audience
- `Jwt__ExpiresMinutes` → expiración en minutos

Ejemplo `.env` (o variables de entorno):
```bash
ConnectionStrings__Default=Data Source=./Data/asistencia.db
Jwt__Key=CHANGE_ME_SUPER_SECRET
Jwt__Issuer=AssistantApp
Jwt__Audience=AssistantApp
Jwt__ExpiresMinutes=480
```

## Ejecutar en desarrollo
```bash
dotnet restore
 dotnet run --project AssistantApp.API
```

Por defecto escucha en `http://localhost:8080` (ver `Program.cs`).

## Autenticación
- Login: `POST /api/auth/login`
- Registro bootstrap (solo si no existe ningún usuario): `POST /api/auth/register`

El JWT incluye roles y permisos como claims `permission`.

## Permisos
Definidos en `AssistantApp.Shared/Permissions.cs`. Se aplican por policy con `[Authorize(Policy = ...)]`.

## Endpoints principales
- Personas: `/api/persons`
- Grupos: `/api/groups`
- Eventos: `/api/events`
- Asistencia: `/api/attendance`
- Usuarios: `/api/users`
- Roles: `/api/roles`

## Docker
Dockerfile en `AssistantApp.API/Dockerfile`. Multi‑arch con `make release`.

```bash
make -C AssistantApp.API release DOCKERHUB_USER=tu_usuario VERSION=1.0.0
```
