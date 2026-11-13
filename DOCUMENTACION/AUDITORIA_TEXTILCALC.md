# Auditoría TextilCalc

Fecha: 2026-11-12
Rama: `feature/textilcalc-refactor-ui-calculadora`
Workspace: `d:\TextilCalc.Maui`

## Alcance revisado

- `TextilCalc.App` (MAUI / MVVM / pantallas)
- `TextilCalc.Core` (fórmulas y evaluador)
- `TextilCalc.Core.Tests` (xUnit)
- `installer/windows`
- `TextilCalc.slnx`

## Hallazgos principales

| Área | Hallazgo | Severidad | Acción |
|------|----------|-----------|--------|
| Catálogo de telas | Módulo completo (UI, LiteDB, Excel, DI, tutorial) activo | Alta (producto) | Eliminado físicamente |
| Calculadora | Evaluador solo aritmético; sin historial; `%` siempre `/100`; formato fijo a 8 decimales | Alta | Reescrito |
| Errores | Mensajes genéricos `"Error"` sin detalle en español | Media | Mensajes claros (p. ej. tan 90°, división por cero) |
| Tema | Paleta púrpura genérica, sin preferencia usuario | Media | Nueva paleta + ciclo Sistema/Claro/Oscuro |
| Entorno | Proyecto en `net10.0` sin SDK 10 ni workloads MAUI 10 al inicio | Bloqueante build | SDK 10.0.404 + workloads restaurados |
| Textiles | Caso 80 kg / 200 g/m² / 1.60 m → 400 m² / 250 m correcto | OK | Cubierto por tests |
| AppId / versión | `com.gramatura.app` / `1.1.0` | Info | Conservados |

## Línea base de entorno (Fase 0)

- Git: rama creada desde `main` limpia (sin cambios locales previos).
- SDK inicial: 8.0.426, 9.0.319, 9.0.320 (sin net10).
- Tras instalación: SDK 10.0.404.
- Workloads MAUI actualizados: `maui-windows`, `android`, `ios`, `maccatalyst`.

## Distinción código vs entorno

- **Código corregido:** catálogo, motor científico, UI/UX, historial, teclado, temas, tests.
- **Limitaciones de entorno documentadas:** prueba visual en emulador/dispositivo depende de hardware local; instalador Inno no se regeneró en esta pasada.
