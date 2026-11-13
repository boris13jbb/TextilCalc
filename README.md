# TextilCalc — migración C#/.NET MAUI

Aplicación empresarial multiplataforma para Android y Windows. Esta carpeta es independiente del proyecto original Kotlin y permite validar la migración sin modificarlo.

## Funciones migradas

- Cálculo de gramatura, área y rendimiento.
- Cálculo de metraje por valores manuales.
- Catálogo local de telas con búsqueda, alta, edición y eliminación.
- Persistencia embebida LiteDB y carga inicial del catálogo productivo existente.
- Cálculo de metraje desde una tela seleccionada.
- Importación y exportación real de archivos `.xlsx`.
- Calculadora básica y científica con memoria y unidades DEG/RAD/GRAD.
- Persistencia local de formularios y estado de la calculadora.
- Tema claro/oscuro, navegación y tutorial inicial.

## Estructura

- `TextilCalc.Core`: fórmulas, análisis numérico y evaluador de expresiones sin dependencias de UI.
- `TextilCalc.App`: aplicación .NET MAUI, MVVM, LiteDB, Excel y pantallas.
- `TextilCalc.Core.Tests`: pruebas unitarias de las reglas de negocio.

## Requisitos

- Visual Studio 2022 con las cargas de trabajo de .NET MAUI.
- SDK de .NET 10.
- Android SDK para compilar o ejecutar Android.
- Windows 10 versión 1809 o posterior para ejecutar la aplicación Windows.

## Compilación

```powershell
dotnet restore TextilCalc.slnx
dotnet test TextilCalc.Core.Tests\TextilCalc.Core.Tests.csproj
dotnet build TextilCalc.App\TextilCalc.App.csproj -f net10.0-windows10.0.19041.0
dotnet build TextilCalc.App\TextilCalc.App.csproj -f net10.0-android
```

## Instalador de Windows

El instalador `.exe` publica la aplicación como `win-x64` autocontenida, instala todos
los archivos requeridos y crea accesos directos y desinstalador.

```powershell
winget install --id JRSoftware.InnoSetup --exact
powershell -ExecutionPolicy Bypass -File installer\windows\Build-Installer.ps1
```

El resultado se genera en
`artifacts\windows\TextilCalc-Setup-<versión>-x64.exe`. El instalador no está
firmado digitalmente mientras no se configure un certificado de firma de código.

## Datos y seguridad

- La aplicación funciona sin conexión y no solicita permisos de red ni acceso general al almacenamiento.
- Android y Windows utilizan selectores nativos para abrir o guardar archivos Excel.
- La base LiteDB se crea dentro del directorio privado de la aplicación.
- No se incluyen credenciales, endpoints, mocks ni servicios externos.

## Consideración de migración

Android no permite que esta aplicación MAUI lea directamente la base privada creada por el APK Kotlin, aunque conserve el mismo identificador. Para conservar catálogos modificados, exporte las telas desde la aplicación Kotlin e importe el archivo en esta versión antes de retirar la anterior.
