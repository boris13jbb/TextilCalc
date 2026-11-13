# Pruebas y resultados

Fecha de esta pasada (instaladores locales + validación): **2026-11-13** (hora local)
Rama: `feature/textilcalc-refactor-ui-calculadora`
SDK: .NET **10.0.404** · workloads: `maui-windows`, `android` · Inno Setup **6.7.3** · JDK **17**
Dispositivo Android: `1b520528` (Redmi / amethyst_global)
Versión: **1.1.0** (ApplicationVersion **2**)

## Comandos ejecutados

```powershell
dotnet test TextilCalc.Core.Tests/TextilCalc.Core.Tests.csproj
powershell -File installer/windows/Build-Installer.ps1
dotnet build TextilCalc.App/TextilCalc.App.csproj -f net10.0-android -c Debug -p:EmbedAssembliesIntoApk=true
# Install Windows: TextilCalc-Setup-1.1.0-x64.exe /VERYSILENT
# Install Android: adb install -r artifacts/android/TextilCalc-1.1.0-android-debug-embedded-Signed.apk
git status --short --branch
```

Detalle de rutas/tamaños/limitaciones: `DOCUMENTACION/INSTALADORES_LOCAL.md`.

## Resultados exactos

### `dotnet test TextilCalc.Core.Tests`

- Pruebas totales: **45** · Correctas: **45** · Incorrectas: **0** · exit **0**
- Duración reportada: ~79 ms

### Builds / instaladores

| Target | Resultado |
|--------|-----------|
| Windows publish + Inno Setup | OK · `artifacts\windows\TextilCalc-Setup-1.1.0-x64.exe` · **59.91 MB** · **NO FIRMADO** |
| Android + `EmbedAssembliesIntoApk=true` | OK · APK Signed **81.39 MB** en `artifacts\android\` |
| Warning XamlC XC0045 (ReuseHistoryCommand binding) | Presente en publish Windows; no bloquea build |

### Smoke Windows (app **instalada** desde el `.exe`)

| Caso | Resultado |
|------|-----------|
| Install silent + arranque desde LocalAppData\Programs\TextilCalc | OK · proceso Inicio creado |
| Logo Inicio | VICUNHA visible |
| Gramatura 50 / 1.8 / 35 | **388.89 g/m²** |
| Metraje 80 / 200 / 1.60 | **250.00 metros** |
| Calculadora / Historial / DEG | OK |
| Desinstalación | No ejecutada (conservar datos de usuario) |

### Smoke Android (APK instalado con `-r`, sin wipe)

| Caso | Resultado |
|------|-----------|
| Install APK debug embedded Signed | OK · versionName 1.1.0 / versionCode 2 |
| Logo Inicio | VICUNHA (`content-desc`) |
| Gramatura 50 / 1.8 / 35 | **388.89 g/m²** |
| Metraje 80 / 200 / 1.60 | **250.00 metros** |
| Calculadora + Historial | OK |

**Nota de despliegue Android:** un `adb install -r` incremental del APK Debug sin assemblies embebidos aborta con
`No assemblies found ... Assuming this is part of Fast Deployment`.
Para smoke en dispositivo usar build con `-p:EmbedAssembliesIntoApk=true` (o Release) e instalación limpia.

### Expectativas UI (presentación)

| Caso | Esperado / observado |
|------|----------------------|
| Gramatura | Solo **GRAMATURA: 388.89 g/m²** |
| Metraje | Solo **METRAJE DISPONIBLE: 250.00 metros** |
| Logo | `Resources/Images/vicunha_logo.png` (= fuente `vicunha_jeansidentity_transparente_HD.png`) |

### Git

- Rama: `feature/textilcalc-refactor-ui-calculadora`
- `git diff --check`: sin marcadores de conflicto
- **Sin commit / push** (restricción explícita)

## Búsqueda catálogo / deps obsoletas

- Sin refs funcionales a `TelasPage`, `TelaRepository`, LiteDB, OpenXML o FileSaver en código de App.
- Texto informativo «sin catálogo» en Inicio (intencional).

## Capturas regeneradas (`DOCUMENTACION/capturas/`)

| Archivo | Contenido |
|---------|-----------|
| `01-inicio-windows.png` | Inicio oscuro + logo |
| `01b-inicio-tema-claro.png` | Inicio claro + logo (banner oscuro) |
| `02-gramatura-windows.png` / `03-gramatura-resultado.png` | Gramatura 388.89 |
| `03-metraje-windows.png` / `04-metraje-resultado-windows.png` | Metraje 250.00 |
| `05-android-inicio.png` | Inicio Android + logo |
| `02-android-gramatura-resultado.png` | Gramatura Android 388.89 |
| `06-android-metraje.png` / `07-android-metraje-resultado.png` | Metraje Android 250.00 |
| `10-windows-oscuro-inicio.png` / `12-windows-oscuro-metraje-resultado.png` | Oscuro actualizado |

Eliminadas capturas obsoletas (webp antiguas y Windows/Android que aún mostraban Área o calculadora previa).

## Recursos logo

- App: un solo asset `TextilCalc.App/Resources/Images/vicunha_logo.png`
- Raíz del repo (fuentes útiles, conservadas): SVG/PNG transparente y fondo oscuro HD
- Sin duplicados temporales dentro de `Resources/Images`
