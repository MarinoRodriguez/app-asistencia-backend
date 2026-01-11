# agents.md - Contexto del Proyecto: Sistema de Gestión de Asistencia

## 1. Visión General
Desarrollo de una aplicación web responsiva ("Full Stack C#") diseñada para gestionar la asistencia de personas a eventos. El sistema debe resolver la dicotomía entre la gestión administrativa (configuración compleja) y la operatividad en campo (toma de asistencia rápida en dispositivos móviles).

### Actores Principales
* **Administrador (Admin):** Gestiona la base de datos de personas, grupos, crea eventos y analiza reportes. (UX: Desktop/Tablet).
* **Tomador de Asistencia (Staff):** Registra la presencia de los asistentes en tiempo real. (UX: Móvil, optimizado para velocidad).

---

## 2. Lógica de Negocio y Reglas

### 2.1. Entidades del Dominio
1.  **Persona:** Individuo que puede asistir. Puede ser un usuario registrado previamente o un "Externo" creado durante un evento.
2.  **Grupo:** Agrupación lógica (ej. "Jovenes", "Varones", "Clase 101"). **Relación:** Muchos a Muchos (Una persona puede estar en varios grupos).
3.  **Evento:** La actividad central.
4.  **Invitación:** Registro de la *intención* de asistencia (Lista esperada).
5.  **Asistencia:** Registro de la *realidad* (Quién vino, hora, estado).

### 2.2. Configuración del Evento (Flags)
Cada evento posee configuraciones críticas que alteran el comportamiento de los registros de asistencia previos:
* `PermiteNoInvitados` (Bool): ¿Se Puede buscar y marcar a una persona que existe en la BD pero no fue invitada explícitamente?
* `PermiteExternos` (Bool): ¿Se Puede registrar a una persona nueva (Guest) que no existe en la BD? y por consecuente marcarlo como asistido
* `AutoInicio` (Bool): ¿El sistema debe cambiar el estado del evento a "EN CURSO" automáticamente al llegar la hora programada?

### 2.3. Ciclo de Vida del Evento (Máquina de Estados)
1.  **BORRADOR:** Configuración inicial. No visible para el Staff.
2.  **PROGRAMADO:** Fecha futura definida. Esperando hora de inicio.
3.  **EN CURSO:** Evento activo. Se habilita la toma de asistencia.
    * *Activación:* Manual (Botón) o Automática (Worker Service).
4.  **FINALIZADO:** Cierre administrativo.
    * *Lógica de Cierre:* Al cerrar, el sistema convierte automáticamente todas las invitaciones pendientes a estado "AUSENTE".

---

## 3. Arquitectura Técnica

### Stack Tecnológico
* **Lenguaje:** C# (.NET 8).
* **Frontend:** Blazor WebAssembly (WASM) - Para capacidad PWA y Offline.
* **Backend:** ASP.NET Core Web API.
* **Base de Datos:** SQLite (Vía Entity Framework).

### Estructura de Datos (Modelo Relacional)
* **Personas:** `Id, Nombre, Email, EsExterno, Activo`.
* **Grupos:** `Id, Nombre`.
* **PersonaGrupo:** `PersonaId, GrupoId` (Tabla intermedia N:M).
* **Eventos:** `Id, Titulo, FechaInicio, Estado, ConfigFlags...`.
* **Invitaciones:** `EventoId, PersonaId`.
    * *Nota:* Al invitar un grupo, se expande a invitaciones individuales para mantener consistencia histórica.
* **Asistencias:** `EventoId, PersonaId, FechaHora, Estado (Presente/Tarde), RegistradoPor`.

---

## 4. Plan de Acción y Desarrollo

### Fase 1: Fundamentos (Backend)
- [ ] Inicializar Solución .NET (Shared, API, Client).
- [ ] Implementar Modelos en `Shared` (incluyendo relación N:M Persona-Grupo).
- [ ] Configurar `DbContext` y Migraciones EF Core.
- [ ] Implementar Repositorios/Servicios básicos (CRUD Personas y Grupos).

### Fase 2: Lógica del Evento (Core)
- [ ] API para Crear/Editar Eventos.
- [ ] Lógica de Invitaciones (Expansión de Grupos a Personas).
- [ ] Implementar `BackgroundService` (Worker) para el `AutoInicio` de eventos.
- [ ] Endpoint de "Cierre de Evento" (Conversión masiva de Pendientes a Ausentes).

### Fase 3: Interfaz de Usuario (Blazor)
- [ ] **Admin Panel:**
    - [ ] ABM de Personas y Grupos.
    - [ ] Creador de Eventos (Formulario con Flags).
- [ ] **Staff Mobile View (Prioridad UX):**
    - [ ] Listado de eventos activos.
    - [ ] Pantalla de "Tomar Lista": Buscador rápido + Botones grandes.
    - [ ] Manejo de "No Invitados" y "Externos".

### Fase 4: Refinamiento
- [ ] Seguridad (JWT/Roles).
- [ ] Validación Offline (PWA).
- [ ] Reportes de Asistencia.