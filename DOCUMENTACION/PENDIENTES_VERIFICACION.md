# Pendientes de verificación

| Pendiente | Causa | Impacto | Recomendación |
|-----------|-------|---------|---------------|
| Firma Authenticode Windows | Sin certificado de código en entorno | Medio (SmartScreen) | Firmar instalador fuera del repo |
| APK Release firmado | Sin keystore Release en repo (intencional) | Medio para distribución | Crear keystore propio + build `-c Release`; no versionar secretos |
| Builds MAUI en CI (Windows/Android) | Runners GitHub sin workloads MAUI reproducibles en este flujo | Bajo (CI) | Validar localmente; no fallar CI inventando soporte |
| Merge a `main` / tags / Releases | PR #1 en **Draft**; restricción explícita | N/A | Autorización expresa e independiente |
| Migración LiteDB antigua en dispositivo/PC | Catálogo eliminado; posibles residuales locales | Bajo | Borrado manual opcional del usuario |
| Harness UI tests MAUI (Appium) | No existe en el repo | Bajo | Opcional a futuro |
| Publicación store / sideload | Solo validación local de instaladores | N/A | Autorización de publicación independiente |

## Hecho en GitHub (rama `feature/textilcalc-refactor-ui-calculadora`)

| Ítem | Evidencia |
|------|-----------|
| Commit inicial de la feature | `2aa2535` · `feat: moderniza TextilCalc e integra identidad VICUNHA` |
| Push a origin | rama `feature/textilcalc-refactor-ui-calculadora` |
| Pull Request | [#1](https://github.com/boris13jbb/TextilCalc/pull/1) · **Draft** · base `main` |
| CI de tests Core | `.github/workflows/ci.yml` · trigger `pull_request` → `main` (+ push a la feature) · .NET 10 · `TextilCalc.Core.Tests` · sin secretos ni publicación de instaladores |

## Resuelto en revisión técnica unarios + CI

| Ítem | Evidencia |
|------|-----------|
| Unarios ExpressionEvaluator | `-(2+3)=-5`, `-sin(30)=-0.5` (DEG), `2×(-3)=-6`, `2^(-3)=0.125`, `(-2)^2=4`, `-2^2=-4` |
| Tests Core | Ampliados; ver salida de `dotnet test` en esta pasada |
| Logos de referencia | Movidos a `DOCUMENTACION/branding/` |
| Prompt temporal de raíz | Eliminado `PROMPT_CURSOR_TEXTILCALC_VICUNHA_CORREGIDO.md` |

## Resuelto en generación de instaladores 2026-11-13 (local)

| Ítem | Evidencia |
|------|-----------|
| Instalador Windows Inno x64 | `artifacts\windows\TextilCalc-Setup-1.1.0-x64.exe` · local · **NO FIRMADO** · no versionado |
| APK Android embedded | `artifacts\android\...` · debug · no versionado |
| Detalle | `DOCUMENTACION\INSTALADORES_LOCAL.md` |

## Resuelto en validación funcional previa

| Ítem | Evidencia |
|------|-----------|
| Smoke Windows / Android logo + Gramatura / Metraje | Capturas en `DOCUMENTACION/capturas/` |
| Sin refs funcionales a catálogo | Búsqueda en App limpia |
| Fórmulas Gramatura/Metraje | Sin cambios de negocio en esta revisión |

## Nota CI vs builds MAUI

El workflow de GitHub Actions ejecuta **solo** `TextilCalc.Core.Tests` en `ubuntu-latest` con .NET 10.

Los builds:

```powershell
dotnet build TextilCalc.App\TextilCalc.App.csproj -f net10.0-windows10.0.19041.0
dotnet build TextilCalc.App\TextilCalc.App.csproj -f net10.0-android
```

se validan en máquina de desarrollo con workloads MAUI instalados. No se incluyen en CI hasta disponer de runners preparados y un flujo reproducible; incluirlos ahora haría fallar el pipeline sin aportar señal fiable.
