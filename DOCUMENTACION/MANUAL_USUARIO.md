# Manual de usuario — TextilCalc

## Inicio

Al abrir la app verá tres herramientas:

1. **Calcular gramatura**
2. **Calcular metraje**
3. **Calculadora científica**

También puede:

- Cambiar el **tema** (Sistema / Claro / Oscuro).
- **Salir** de la aplicación (con confirmación).

## Gramatura

1. Ingrese metraje (m), ancho (m) y peso (kg). Acepte coma o punto decimal.
2. Pulse **Calcular gramatura**.
3. Verá el resultado principal **GRAMATURA** en g/m² (2 decimales).
4. **Limpiar** borra el formulario y el resultado guardado.

Fórmula: `GSM = (Peso × 1000) / (Metraje × Ancho)`.

## Metraje

1. Ingrese peso (kg), gramatura (g/m²) y ancho (m).
2. Pulse **Calcular metraje**.
3. Verá **METRAJE DISPONIBLE** en metros (2 decimales).

Ejemplo: 80 kg, 200 g/m², 1.60 m → **250.00 metros**.

Fórmula: `Metraje = (Peso × 1000) / (Gramatura × Ancho)`.

## Calculadora científica

### Básico

- Dígitos, `+ − × ÷`, decimal, paréntesis, `AC`, `DEL`, `=`.
- `%` contextual:
  - `200×10%` → 20
  - `200+10%` → 220
  - valor suelto → divide entre 100.

### Científico

Pulse **Mostrar científicas** para:

- sin/cos/tan y sus inversas
- sinh/cosh/tanh
- √ ∛ x² x³ xʸ
- 10ˣ 2ˣ eˣ
- log / ln / log₂
- factorial, 1/x, abs, floor/ceil/round
- π y e
- memoria MC / MR / M+ / M−
- unidad angular DEG → RAD → GRAD

### Historial

1. Pulse **Historial**.
2. Toque una entrada para reutilizar el resultado.
3. **Borrar** pide confirmación y limpia las últimas 50 operaciones.

### Teclado físico (Windows/Android)

Dígitos, `+ - * / . ,`, Enter/`=`, Backspace, Escape, `% ( ) ^` usan la misma lógica que los botones.

### Errores

La app no se cierra ante operaciones inválidas. Ejemplos:

- División entre cero
- Raíz de negativo
- Logaritmo de 0 o negativo
- `tan(90°)` no definida
