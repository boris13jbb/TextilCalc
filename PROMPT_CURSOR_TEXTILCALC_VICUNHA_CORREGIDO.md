# CURSOR — AJUSTE PUNTUAL TEXTILCALC: LOGO Y RESULTADOS

Trabaja en el workspace local existente de TextilCalc, en la rama `feature/textilcalc-refactor-ui-calculadora`.
**No reconstruyas el proyecto ni repitas el refactor. No hagas commit, push ni merge.** Respeta los cambios locales del usuario.

## 1. Logo VICUNHA jeansidentity

Usa el nuevo archivo `vicunha_jeansidentity_transparente.svg` (logo vectorial de alta nitidez, trazados sin dependencia de fuentes). Si el pipeline de .NET MAUI admite este recurso como `MauiImage`, cópialo a `TextilCalc.App/Resources/Images/` y utilízalo desde XAML como imagen con `AspectFit`. En caso de que el SVG dé problemas en el sistema destino, usa `vicunha_jeansidentity_transparente_HD.png` como imagen PNG alternativa. También se facilita versión `fondo_oscuro` para fondos claros.

Preserva proporción, márgenes y legibilidad en Windows y Android. Incorpóralo con prudencia en Inicio y, donde corresponda, en cabeceras. No confundir este recurso con el ícono cuadrado de la app: modificar el ícono no está solicitado. Este diseño es una reconstrucción visual del logo enviado, no un archivo vectorial oficial de la marca.

## 2. Gramatura: SOLO el resultado principal

En `TextilCalc.App/Views/GramaturaPage.xaml`, elimina completamente de la interfaz los dos renglones, etiquetas, valores y contenedores correspondientes a:

- `Área total` (ejemplo `90.0000 m²`).
- `Rendimiento` (ejemplo `2.5714 m²/kg`).

**NO deben verse en la pantalla de Gramatura** y no debe quedar espacio vacío.

Conserva el bloque principal `GRAMATURA` y muéstralo con **exactamente dos decimales**: para metraje 50 m, ancho 1.8 m y peso 35 kg debe mostrar `388.89 g/m²` (o coma decimal según la región). Cambia el formato de la propiedad de presentación `GsmDisplay` en `TextilCalc.App/ViewModels/GramaturaViewModel.cs` de N4 a N2, o equivalente robusto. No redondees los valores matemáticos internos.

**IMPORTANTE**: No elimines el cálculo interno de `AreaM2` ni `Rendimiento` de `TextileCalculator` ni los rompas en modelos/pruebas. Este pedido cambia únicamente la presentación visual. Si hay propiedades de presentación usadas solo por los bloques eliminados, revisa si es seguro retirarlas.

## 3. Metraje: SOLO metraje disponible

En `TextilCalc.App/Views/MetrajePage.xaml` elimina de la interfaz el bloque `Área total` y su valor (`400.00 m²` en el ejemplo), sin dejar espacios vacíos.

Debe mostrarse `METRAJE DISPONIBLE: 250.00 metros` para peso 80 kg, gramatura 200 g/m² y ancho 1.60 m.

**No elimines** el cálculo interno de `AreaM2` ni modifiques la fórmula del metraje.

## 4. Control final

- Mantén intacto el módulo de Calculadora científica, el historial, los temas y el resto de funciones.
- Verifica que el logo no se recorte ni pixele, en modo claro y oscuro.
- Ajusta espacios y tarjetas de resultado para que queden visualmente equilibradas.
- Ejecuta `dotnet test TextilCalc.Core.Tests/TextilCalc.Core.Tests.csproj` y compilaciones Windows y Android, si están disponibles.
- Intenta ejecutar la app en Windows/Android para validar las vistas actualizadas. Si no puedes, no afirmes que las verificaste visualmente.
- Comprueba `git diff --check` y `git status --short --branch`.
- Documenta los cambios y el resultado real.

**No commit / no push / no merge.** Entrega resumen conciso de archivos cambiados, captura(s) si son reales, tests y builds, pendientes concretos.
