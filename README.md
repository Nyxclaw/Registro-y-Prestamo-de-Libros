# Registro y préstamo de libros

Aplicación de escritorio para Windows, escrita en C# con .NET 8 y Windows Forms. Permite registrar préstamos, localizar pendientes mediante filtros, registrar devoluciones y consultar vencidos e historial. Funciona sin Internet, servidor ni cuentas. La especificación está en [REQUISITOS.md](REQUISITOS.md).

## Desarrollo

Trabajar en la rama **`Develop`**, cuyo nombre distingue mayúsculas. Realizar commits después de pasar las comprobaciones correspondientes. No integrar en `main` sin comprobar el funcionamiento, incluido el flujo de escritorio en Windows.

Requisitos para desarrollar:

- SDK de .NET **8.0** (seleccionado por `global.json`).
- Windows 10/11 para ejecutar la interfaz.
- Visual Studio 2022 con desarrollo de escritorio .NET, o Visual Studio Code y el SDK.
- Internet durante la primera restauración o publicación para obtener los componentes oficiales de .NET. La aplicación publicada funciona offline.

Abrir `PrestamoLibros.sln`, seleccionar `PrestamoLibros.WinForms` como proyecto de inicio, o usar una terminal desde la raíz:

```powershell
dotnet restore PrestamoLibros.sln
dotnet build PrestamoLibros.sln -c Release --no-restore --warnaserror
dotnet run --project tests/PrestamoLibros.Tests -c Release --no-build
dotnet run --project src/PrestamoLibros.WinForms
```

En macOS/Linux se puede compilar con los componentes de referencia de Windows y ejecutar las pruebas del núcleo; Windows Forms solo se ejecuta en Windows. No hay paquetes de terceros. Las pruebas utilizan un ejecutor de consola propio: `dotnet run`, **no `dotnet test`**, ejecuta las comprobaciones y devuelve un código distinto de cero si alguna falla.

## Uso

1. **Préstamo:** completar los campos; la fecha límite se calcula automáticamente a 21 días. Los errores conservan los datos escritos.
2. **Entrega:** buscar por título, alumno, licenciatura, código, semestre, correo, fecha de préstamo o persona que entregó. Los filtros se combinan con AND; los textos admiten coincidencias parciales sin distinguir mayúsculas. Fecha desmarcada y semestre 0 significan sin filtro. Pulsar **Buscar** o **Limpiar filtros**.
3. Seleccionar una fila y pulsar **Registrar devolución** (también doble clic o Enter). Los datos originales aparecen para consulta; solo se capturan fecha y persona que recibe. Confirmar la devolución.
4. **Préstamos vencidos:** consultar pendientes que superaron su fecha límite y devolverlos desde el mismo detalle.
5. **Historial:** consultar devoluciones a tiempo y con retraso, con detalle de todos los datos. No permite editar.

Las listas se actualizan al registrar o devolver, al navegar y al cambiar el día (comprobación cada 30 segundos). **Actualizar** vuelve a leer los archivos. La app permite una sola instancia por sesión de Windows para evitar escrituras simultáneas desde dos ventanas.

## Datos locales y recuperación

La ruta está centralizada en `JsonStorageService.RutaPredeterminada`:

```text
%LocalAppData%/PrestamoLibros/Data/
├── PrestamosActivos/{GUID}.json
└── Historial/{GUID}.json
```

Las carpetas se crean automáticamente sin permisos de administrador. Cada préstamo tiene un GUID; alumnos o títulos repetidos son registros independientes. El código del alumno se guarda como texto y conserva ceros iniciales. Las fechas se guardan como `yyyy-MM-dd`, sin horas ni zonas horarias; en pantalla se muestran como `dd/MM/yyyy`.

La escritura usa un archivo temporal en la misma carpeta, vaciado a disco y renombrado al terminar. En una devolución se reemplaza primero el archivo pendiente con los datos completos y luego se mueve al historial, sin mantener copias contradictorias. Si falla el traslado o se interrumpe la app entre ambos pasos, el registro ya devuelto se muestra en el historial y se reintenta el movimiento al cargar. Un archivo temporal incompleto no se interpreta como préstamo.

Un JSON ilegible o inválido produce un aviso con su nombre; los demás registros continúan cargándose. El archivo problemático se conserva para revisión. Los errores de acceso o escritura se muestran sin cerrar la aplicación. Antes de corregir archivos manualmente, cerrar la app y hacer una copia de toda la carpeta `Data`. El guardado atómico no reemplaza una copia de seguridad frente a daños del disco.

El estado se calcula a partir de las fechas al consultar; el valor `estado` del JSON es una instantánea del momento del guardado. No es necesario modificar diariamente los archivos.

## Estructura

```text
src/
  PrestamoLibros.Core/
    Models/                Préstamo, estados y filtros
    Services/              Validación, reglas y persistencia JSON
  PrestamoLibros.WinForms/
    Forms/                 Navegación, registro, listados y detalle
    Program.cs             Inicio y control de instancia
tests/
  PrestamoLibros.Tests/     Pruebas de reglas, persistencia y recuperación
```

El núcleo no depende de Windows Forms: los formularios recogen entradas y muestran resultados; los servicios validan, guardan y notifican los cambios. El reloj y la ruta se pueden sustituir en pruebas, que usan carpetas temporales y datos ficticios sin tocar los datos reales.

Decisiones menores de la primera versión:

- Semestres enteros de 1 a 20; el máximo está centralizado en `ValidacionPrestamo`.
- No se aceptan préstamos futuros ni devoluciones anteriores al préstamo o posteriores a hoy.
- El correo se valida con `MailAddress` y un dominio con punto; no se comprueba mediante Internet.
- No se guardan datos de demostración en la versión distribuida.

## Publicar un ejecutable para Windows

Desde la raíz:

```powershell
dotnet publish src/PrestamoLibros.WinForms -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o artifacts/win-x64
```

Distribuir `artifacts/win-x64/PrestamoLibros.WinForms.exe`. Incluye el runtime: el equipo destino no necesita instalar el SDK ni .NET. Los archivos `.pdb`, si se generan, son símbolos de depuración opcionales. No se requiere instalador MSI. Los datos se guardan en el perfil del usuario, separados del ejecutable.

El workflow `.github/workflows/validar.yml` prepara restauración, compilación, pruebas y publicación sobre Windows al enviar cambios a `Develop` o abrir un PR hacia `Develop`/`main`. No fusiona ni publica cambios en `main`. Su ejecución requiere que los cambios estén en GitHub y Actions habilitado; no sustituye la revisión visual.

## Comprobación antes de integrar en main

Además de las pruebas automatizadas, ejecutar en Windows el flujo de aceptación de `REQUISITOS.md`:

- Primera apertura sin datos: se crean las carpetas y aparecen estados vacíos claros.
- Registrar un préstamo; cerrar y abrir; comprobar todos sus datos y la fecha límite.
- Localizarlo con cada filtro y con varios filtros combinados; limpiar filtros.
- Abrir el detalle, cancelar una devolución y verificar que sigue pendiente.
- Registrar una devolución confirmada; comprobar que desaparece de pendientes y aparece en historial incluso después de reiniciar.
- Registrar un préstamo de hace 22 días; comprobar vencimiento, días de retraso y devolución tardía.
- Probar campos vacíos, correo inválido, semestre decimal y fechas incoherentes sin perder lo capturado.
- Comprobar dos alumnos/títulos iguales, navegación por teclado, ventanas redimensionadas y escala de pantalla 100 %/150 %.
- Probar el ejecutable publicado en un equipo Windows sin SDK y sin conexión a Internet.

La compilación cruzada y las pruebas del núcleo no acreditan por sí solas estas comprobaciones de escritorio.
