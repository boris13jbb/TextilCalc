# TextilCalc — .NET MAUI (C#)

Aplicación multiplataforma para Android y Windows orientada a cálculos textiles y calculadora científica.

## Funciones

- Cálculo de **gramatura** (resultado principal GSM en g/m²) con entrada manual.
- Cálculo de **metraje** (metros lineales disponibles) con entrada manual.
- Marca **VICUNHA jeansidentity** en Inicio (`vicunha_logo.png` por compatibilidad MAUI; logos de referencia en `DOCUMENTACION/branding/`).
- **Calculadora científica** con memoria, historial persistente, DEG/RAD/GRAD y teclado físico.
- Persistencia local de formularios, estado de la calculadora e historial.
- Tema claro / oscuro / sistema.
- Tutorial inicial de navegación.

> El catálogo de telas, LiteDB y la importación/exportación Excel se eliminaron a propósito. Gramatura y metraje usan solo valores manuales.

## Estructura

- `TextilCalc.Core`: fórmulas textiles, evaluador de expresiones y formateo numérico (sin UI).
- `TextilCalc.App`: aplicación .NET MAUI con MVVM y pantallas.
- `TextilCalc.Core.Tests`: pruebas unitarias xUnit.
- `installer/windows`: script Inno Setup para empaquetado Windows.
- `DOCUMENTACION/`: auditoría, cambios, pruebas y manual.

## Requisitos

- SDK de .NET 10.
- Cargas de trabajo MAUI (`maui-windows`, `maui-android` según plataforma).
- Android SDK para compilar o ejecutar Android.
- Windows 10 versión 1809 o posterior para la aplicación Windows.

## Compilación

```powershell
dotnet restore TextilCalc.slnx
dotnet test TextilCalc.Core.Tests\TextilCalc.Core.Tests.csproj
dotnet build TextilCalc.App\TextilCalc.App.csproj -f net10.0-windows10.0.19041.0
dotnet build TextilCalc.App\TextilCalc.App.csproj -f net10.0-android
```

Si faltan cargas de trabajo:

```powershell
dotnet workload restore TextilCalc.App\TextilCalc.App.csproj
```

## CI (GitHub Actions)

- Workflow: `.github/workflows/ci.yml`
- Trigger: `pull_request` hacia `main` (y push a la rama feature documentada)
- Ejecuta tests de `TextilCalc.Core.Tests` con .NET 10
- No publica instaladores ni usa secretos
- Builds MAUI Windows/Android: solo locales mientras no haya runners MAUI reproducibles (ver `DOCUMENTACION/PENDIENTES_VERIFICACION.md`)

## Instalador de Windows

```powershell
winget install --id JRSoftware.InnoSetup --exact
powershell -ExecutionPolicy Bypass -File installer\windows\Build-Installer.ps1
```

El resultado se genera en `artifacts\windows\TextilCalc-Setup-<versión>-x64.exe`.

## Datos y seguridad

- Funciona sin conexión; no solicita permisos de red.
- Preferencias e historial se guardan en el almacén privado de la aplicación.
- No se incluyen credenciales, endpoints ni servicios externos.
- AppId: `com.gramatura.app` · versión de visualización: `1.1.0`.
