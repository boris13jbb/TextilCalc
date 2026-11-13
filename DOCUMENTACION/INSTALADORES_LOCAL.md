# Instaladores locales TextilCalc 1.1.0

Fecha: **2026-11-13**
Rama: `feature/textilcalc-refactor-ui-calculadora`
Versión app: **ApplicationDisplayVersion 1.1.0** · **ApplicationVersion 2**
Sin commit / push / release / publicación.

## Windows (Inno Setup)

| Ítem | Valor |
|------|--------|
| Script | `installer\windows\Build-Installer.ps1` + `TextilCalc.iss` |
| Salida | `D:\TextilCalc.Maui\artifacts\windows\TextilCalc-Setup-1.1.0-x64.exe` |
| Tamaño | **59.91 MB** (62 824 381 bytes) |
| Runtime | win-x64 self-contained Release |
| Firma | **NO FIRMADO** (`Get-AuthenticodeSignature` → `NotSigned`) |
| SmartScreen | Esperable aviso “Windows protegió su PC” / origen desconocido hasta firmar con certificado Authenticode de código |
| Instalación validada | `%LOCALAPPDATA%\Programs\TextilCalc\` (privilegios lowest, sin admin) |
| Accesos directos | Menú Inicio `TextilCalc.lnk` (escritorio opcional en el wizard) |
| Datos usuario al actualizar | Preferencias en almacenamiento de app; Inno usa `ignoreversion` sobre archivos de programa. **No se desinstaló** en esta pasada para no tocar datos locales existentes. |
| Respaldo previo | `artifacts\_backup_20261113-003636\windows\` |

Ajuste de empaquetado: `PublishReadyToRun=false` en `Build-Installer.ps1` (evita `NETSDK1094` con restore multi-TFM MAUI).

## Android (APK de prueba)

| Ítem | Valor |
|------|--------|
| Build | Debug + `-p:EmbedAssembliesIntoApk=true` (necesario; Fast Deployment vacío aborta en dispositivo) |
| APK firmado debug | `D:\TextilCalc.Maui\artifacts\android\TextilCalc-1.1.0-android-debug-embedded-Signed.apk` · **81.39 MB** |
| APK sin sufijo Signed | `D:\TextilCalc.Maui\artifacts\android\TextilCalc-1.1.0-android-debug-embedded.apk` · **81.22 MB** |
| Package | `com.gramatura.app` · versionName **1.1.0** · versionCode **2** |
| Firma | Keystore **debug** de Android (no es Release de producción) |
| Keystore Release | **No presente en el repo** (correcto). APK definitivo requiere keystore propio + passwords fuera del repositorio. |
| Dispositivo | `1b520528` · `adb install -r` sin uninstall (datos conservados) |

## Validación funcional (app instalada)

### Windows (exe instalado, no Debug)

| Caso | Resultado |
|------|-----------|
| Arranque desde `%LOCALAPPDATA%\Programs\TextilCalc\TextilCalc.App.exe` | OK |
| Logo VICUNHA en Inicio | OK |
| Gramatura / Metraje / Calculadora (navegación) | OK |
| Gramatura 50 / 1.8 / 35 | **388.89 g/m²** |
| Metraje 80 / 200 / 1.60 | **250.00 metros** |
| Calculadora + Historial / DEG | OK |
| Desinstalación | **No ejecutada** (preservar datos) |

### Android (APK instalado)

| Caso | Resultado |
|------|-----------|
| Arranque | OK · MainActivity en foreground |
| Logo VICUNHA | OK (`content-desc`) |
| Gramatura 50 / 1.8 / 35 | **388.89 g/m²** |
| Metraje 80 / 200 / 1.60 | **250.00 metros** |
| Calculadora + Historial | OK |

## Calidad

- `dotnet test TextilCalc.Core.Tests` → **45/45** OK
- Builds Windows Release publish + Android Debug embedded → OK
- `.gitignore` cubre `artifacts/`, `bin/`, `obj/`, `*.keystore`, `*.jks`, `*.p12`, `*.pfx`

## Pendientes (no bloquean smoke local)

1. Certificado Authenticode Windows (firma del `.exe` / SmartScreen).
2. Keystore Release Android + build `-c Release` firmado para sideload/store.
3. Autorización expresa para commit / push / PR / release.
