# Validación de la primera implementación

Fecha: 18 de septiembre de 2026. Rama: `Develop`.

## Comprobaciones realizadas

Entorno: macOS ARM64, SDK oficial .NET **8.0.425**, instalado temporalmente para respetar la versión solicitada.

- Restauración de `PrestamoLibros.sln`: correcta.
- Compilación Release de la solución, tratando advertencias como errores: **0 errores, 0 advertencias**.
- Ejecución de `tests/PrestamoLibros.Tests`: **11 de 11 pruebas correctas**.
- Publicación `win-x64`, autocontenida y de archivo único: correcta.

Comandos utilizados (con el ejecutable del SDK 8):

```text
dotnet restore PrestamoLibros.sln --disable-parallel
dotnet build PrestamoLibros.sln -c Release --no-restore --warnaserror --disable-build-servers -m:1 -p:UseSharedCompilation=false
dotnet run --project tests/PrestamoLibros.Tests -c Release --no-build
dotnet publish src/PrestamoLibros.WinForms -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:UseSharedCompilation=false --disable-build-servers -m:1 -o artifacts/win-x64
```

Se desactivaron los servidores de compilación tras bloquearse el intento dentro del entorno restringido. La compilación posterior terminó correctamente.

## Cobertura comprobada

1. Plazo de 21 días, frontera de vencimiento, año bisiesto, cambio de año y cuatro estados.
2. Registro, recarga mediante una nueva instancia, devolución, traslado al historial y segunda recarga, conservando todos los datos originales y ceros del código.
3. Cambio de día, vencimiento y devolución tardía.
4. Alumnos y títulos repetidos distinguidos por GUID.
5. Ocho campos de búsqueda, coincidencias parciales, mayúsculas, combinación AND y exclusión de devueltos.
6. Campos obligatorios, correo, semestre y fechas: las entradas inválidas no escriben ni alteran datos existentes.
7. JSON dañado, nulo o incompleto sin impedir la carga de los registros válidos.
8. Fallo de almacenamiento al registrar, sin crear préstamos solo en memoria.
9. Fallo al guardar una devolución, conservando el préstamo pendiente.
10. Fallo del traslado posterior al guardado: devolución preservada, sin duplicados y recuperable en la siguiente carga.
11. Identificadores inconsistentes y registros inválidos del historial rechazados de forma controlada.

## Pendiente antes de integrar en main

**No se ha ejecutado Windows Forms en Windows ni se ha validado visualmente la interfaz.** La compilación cruzada y las pruebas del núcleo no permiten certificar navegación, tamaños, escalado o interacción con diálogos.

Ejecutar la lista manual del README, el flujo de aceptación de `REQUISITOS.md` y el ejecutable publicado en Windows sin SDK y sin Internet. El workflow de GitHub Actions queda preparado, pero no se considera ejecutado por haberlo añadido al repositorio.

El ejecutable generado se encuentra en `artifacts/win-x64/PrestamoLibros.WinForms.exe`; los binarios y los datos locales están excluidos de Git.
