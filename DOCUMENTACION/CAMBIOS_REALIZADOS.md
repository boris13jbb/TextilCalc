# Cambios realizados

## Fase 1 — Eliminación del catálogo de telas

Eliminados físicamente:

- `Views/TelasPage.xaml` / `.cs`
- `ViewModels/TelasViewModel.cs`
- `Models/Tela.cs`
- `Data/DefaultTelas.cs`
- `Services/TelaRepository.cs` / `ITelaRepository.cs`
- `Services/ExcelService.cs` / `IExcelService.cs`

Actualizados:

- `HomePage.xaml` / `HomeViewModel.cs` (sin Gestión de telas)
- `AppShell.xaml.cs` (sin ruta Telas)
- `MauiProgram.cs` (sin DI de catálogo/Excel/FileSaver)
- `NavigationTutorialService.cs`
- `TextilCalc.App.csproj` (sin LiteDB ni OpenXML)
- `README.md`

No se borraron bases de datos locales ni datos privados del usuario.

## Fase 2 — Calculadora científica

- `ExpressionEvaluator`: funciones, constantes π/e, notación científica, DEG/RAD/GRAD, errores en español.
- `NumberFormatter`: decimales significativos + notación científica.
- `CalculadoraViewModel`: máquina de estados, `%` contextual, AC/DEL, memoria, historial.
- `CalculatorHistoryService`: últimas 50 operaciones persistentes.
- Teclado físico Windows (`KeyDown`/`CharacterReceived`) y Android (`MainActivity.DispatchKeyEvent`).

## Fase 3 — UI/UX

- Nueva paleta oscura (`#10152F`…) y clara en `Colors.xaml`.
- Home rediseñado (3 herramientas + tema).
- Calculadora con display, panel científico, historial y teclado.
- Preferencia de tema Sistema / Claro / Oscuro.

## Fase 4 — Gramatura y Metraje

- Fórmulas verificadas (80/200/1.60 → 400 m² / 250 m).
- UI alineada; etiqueta “Datos de entrada” en Metraje.
- Validaciones y persistencia existentes mantenidas.

## Fase 5-7 — Tests y build

- Suite xUnit ampliada (textiles + evaluador + formateo).
- Builds Windows y Android validados tras instalar SDK/workloads .NET 10.

## Fase final — Validación real (2026-11-12)

Correcciones aplicadas en la pasada funcional:

- `HomePage.xaml`: navegación de tarjetas con **Button transparente** encima del card (en Windows, `TapGestureRecognizer` + Labels no recibía el clic).
- `CalculadoraPage.xaml`: reutilizar historial con el mismo patrón de overlay Button.
- `GramaturaViewModel` / `MetrajeViewModel`: invalidar resultados al editar entradas (evita valores obsoletos); flag `_isInitializing` para no borrar estado restaurado.
- `MetrajePage` / `GramaturaPage`: `Clicked` + `Unfocus` de Entries antes de calcular (Android confirma binding al perder foco).
- `UnitTest1.cs`: caso `√81` adicional.
- Documentación y capturas reales en `DOCUMENTACION/` (Windows + Android dispositivo `1b520528`).

## Cierre de calidad pre-publicación (2026-11-12, misma noche)

- `MetrajePage.xaml` / `GramaturaPage.xaml`: layout `Grid *,Auto` con pie fijo «Volver» / «Abrir calculadora» (siempre usable con resultado, error o ventana reducida).
- `MetrajePage.xaml.cs`: tras calcular, `ScrollToAsync` al card de resultado (conserva Unfocus).
- `Styles.xaml`: contraste oscuro en `SecondaryAction`, `TextilInput`, `ErrorBanner`, `SuccessBanner`, `DangerAction`.
- Capturas Oscuro Windows reales: `DOCUMENTACION/capturas/10`–`14`.
- Docs actualizadas con resultados reales de restore/test/build y UIA.

## Ajuste puntual logo + resultados (2026-11-12)

- Logo **VICUNHA jeansidentity**: `Resources/Images/vicunha_logo.png` (PNG HD transparente) usado en `HomePage.xaml` sobre fondo `#10152F` para contraste en claro/oscuro. Los SVG/PNG de referencia están en `DOCUMENTACION/branding/`; no se incluyen en `Resources/Images` junto al PNG de app porque Resizetizer exige nombres de salida únicos y MAUI no renderiza SVG de forma fiable en `Image` en todas las plataformas.
- **Gramatura UI**: se eliminaron de la presentación «Área total» y «Rendimiento»; solo se muestra `GRAMATURA` con formato `N2` (`GsmDisplay`). Cálculos internos de área/rendimiento intactos en Core/VM.
- **Metraje UI**: se eliminó «Área total» de la presentación; solo `METRAJE DISPONIBLE` (`N2`). `AreaM2` sigue calculándose internamente.
- Textos de Inicio/subtítulos y `MANUAL_USUARIO.md` alineados con la UI.
