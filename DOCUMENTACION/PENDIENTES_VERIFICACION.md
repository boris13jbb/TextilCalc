# Pendientes de verificación

| Pendiente | Causa | Impacto | Recomendación |
|-----------|-------|---------|---------------|
| Firma Authenticode Windows | Sin certificado de código en entorno | Medio (SmartScreen) | Firmar `TextilCalc-Setup-1.1.0-x64.exe` fuera del repo |
| APK Release firmado | Sin keystore Release en repo (intencional) | Medio para distribución | Crear keystore propio + build `-c Release`; no versionar secretos |
| Commit / push / PR / tag / release | Restricción explícita del usuario | N/A | Autorización expresa e independiente |
| Migración LiteDB antigua en dispositivo/PC | Catálogo eliminado; posibles residuales locales | Bajo | Borrado manual opcional del usuario |
| Harness UI tests MAUI (Appium) | No existe en el repo | Bajo | Opcional a futuro |
| Publicación store / sideload | Solo validación local de instaladores | N/A | Autorización de publicación independiente |

## Resuelto en generación de instaladores 2026-11-13

| Ítem | Evidencia |
|------|-----------|
| Instalador Windows Inno x64 | `artifacts\windows\TextilCalc-Setup-1.1.0-x64.exe` · 59.91 MB · **NO FIRMADO** |
| Install + run Windows instalado | `%LOCALAPPDATA%\Programs\TextilCalc\` · Gramatura 388.89 · Metraje 250.00 · Calculadora |
| APK Android embedded | `artifacts\android\TextilCalc-1.1.0-android-debug-embedded-Signed.apk` · 81.39 MB · debug |
| Install + run Android | dispositivo `1b520528` · mismos cálculos · historial visible |
| Respaldo instalador previo | `artifacts\_backup_20261113-003636\` |
| Detalle | `DOCUMENTACION\INSTALADORES_LOCAL.md` |

## Resuelto en validación final 2026-11-13

| Ítem | Evidencia |
|------|-----------|
| Smoke Windows logo claro/oscuro | Capturas `01`, `01b`, `10` · banner oscuro fijo |
| Smoke Windows Gramatura 388.89 | UIA + `02`/`03` · sin Área/Rendimiento |
| Smoke Windows Metraje 250.00 | UIA + `04`/`12` · sin Área |
| Smoke Android logo + mismos cálculos | Dispositivo `1b520528` · capturas `05`, `02-android`, `06`/`07` |
| APK autónomo en dispositivo | Rebuild `-p:EmbedAssembliesIntoApk=true` (Fast Deployment vacío abortaba) |
| Capturas sin Área/Rendimiento | Regeneradas; obsoletas eliminadas |
| Tests 45/45 + builds Windows/Android | Comandos de cierre ejecutados |
| Sin refs funcionales a catálogo | Búsqueda en App limpia |

## Resuelto antes (misma rama)

- Pie fijo Metraje/Gramatura; contraste oscuro botones/campos
- Defensa Unfocus en botones Calcular (Metraje/Gramatura)
- Eliminación de catálogo / Excel / LiteDB de la app
