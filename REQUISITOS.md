# Sistema local de registro de préstamo de libros

Quiero que actúes como desarrollador principal de este proyecto. Yo actuaré como Project Manager.

Necesito que desarrolles una aplicación de escritorio completa siguiendo esta especificación. Antes de implementar cualquier funcionalidad, analiza todos los requisitos y diseña internamente la arquitectura necesaria.

El objetivo es obtener una primera versión completamente funcional de la aplicación, no solamente una demostración visual.

No agregues funcionalidades que no estén especificadas salvo aquellas estrictamente necesarias para que el sistema funcione correctamente.

---

# 1. Objetivo del proyecto

Desarrollar una aplicación de escritorio para Windows destinada al registro y control de préstamos de libros.

La aplicación será utilizada en una sola computadora por personal encargado de administrar préstamos.

Debe permitir:

* Registrar un préstamo.
* Consultar préstamos actualmente activos.
* Registrar la devolución de un libro sin volver a capturar los datos del préstamo.
* Detectar automáticamente préstamos que hayan excedido 3 semanas.
* Consultar préstamos vencidos.
* Consultar libros que ya fueron devueltos.
* Buscar préstamos mediante diferentes campos.
* Mantener toda la información almacenada localmente.

La aplicación deberá funcionar completamente offline.

No requiere conexión a Internet.

No requiere servidor.

No requiere cuentas de usuario.

No requiere servicios en la nube.

No requiere una base de datos externa.

---

# 2. Tecnología

Desarrolla el proyecto utilizando:

* C#
* .NET 8
* Windows Forms
* System.Text.Json para lectura y escritura de archivos JSON

Evita dependencias externas salvo que sean absolutamente necesarias.

El proyecto debe poder abrirse, compilarse, ejecutarse y modificarse posteriormente con Visual Studio o Visual Studio Code.

La aplicación final deberá poder publicarse como ejecutable para Windows.

---

# 3. Alcance

Esta aplicación sirve únicamente para controlar registros de préstamos.

No implementar:

* Sistema de usuarios.
* Inicio de sesión.
* Contraseñas.
* Permisos o roles.
* Sistema de multas.
* Notificaciones por correo.
* Integración con sistemas escolares.
* Sincronización en la nube.
* Inventario completo de biblioteca.
* Lectores de código de barras.
* Administración de ejemplares.
* Reservaciones.
* Servicios web.
* APIs.
* Base de datos SQL.

El diseño debe mantenerse deliberadamente sencillo.

---

# 4. Estructura de un préstamo

Cada préstamo deberá contener internamente un identificador único generado automáticamente.

Utilizar preferentemente un GUID.

Ejemplo:

`IdPrestamo`

El usuario no necesita introducir ni visualizar este identificador normalmente.

Cada registro deberá contener los siguientes campos:

## Datos del alumno

* Nombre completo.
* Licenciatura.
* Código de alumno.
* Semestre.
* Correo electrónico.

## Datos del libro

* Título del libro.

## Datos del préstamo

* Fecha de préstamo.
* Fecha límite de devolución.
* Persona que entrega el libro.

## Datos de devolución

* Fecha real de devolución.
* Persona que recibe el libro.

## Estado

El préstamo debe poder encontrarse en uno de estos estados:

* Activo
* Vencido
* Devuelto a tiempo
* Devuelto con retraso

---

# 5. Fechas

La fecha de préstamo deberá seleccionarse mediante un selector de fecha.

Por defecto deberá mostrar la fecha actual, aunque el usuario podrá modificarla si necesita registrar un préstamo realizado anteriormente.

La fecha límite de devolución NO será capturada manualmente.

Debe calcularse automáticamente:

`FechaLimite = FechaPrestamo + 21 días`

La fecha real de devolución solamente se asignará cuando se registre la entrega del libro.

---

# 6. Determinación del estado

Aplicar automáticamente las siguientes reglas.

### Activo

Un préstamo se considera activo cuando:

* Aún no tiene registrada una devolución.
* La fecha actual es menor o igual a la fecha límite.

### Vencido

Un préstamo se considera vencido cuando:

* Aún no tiene registrada una devolución.
* La fecha actual es posterior a la fecha límite.

No es necesario modificar físicamente el JSON cada día para cambiar el estado.

El programa puede determinar si está vencido comparando las fechas cada vez que se inicia o actualiza la interfaz.

### Devuelto a tiempo

Un préstamo será considerado devuelto a tiempo cuando:

`FechaDevolucion <= FechaLimite`

### Devuelto con retraso

Un préstamo será considerado devuelto con retraso cuando:

`FechaDevolucion > FechaLimite`

Un préstamo devuelto, aunque haya sido entregado tarde, deja de aparecer en los préstamos activos y vencidos.

---

# 7. Navegación principal

La interfaz deberá contar con dos secciones principales:

1. Préstamo
2. Entrega

Además deberá existir acceso secundario a:

* Préstamos vencidos.
* Historial de devoluciones.

Los apartados secundarios no deben tener el mismo peso visual que Préstamo y Entrega.

Puede utilizarse una barra lateral, barra superior o navegación equivalente.

El flujo debe ser claro incluso para una persona que no tenga conocimientos técnicos.

---

# 8. Apartado "Préstamo"

Este apartado estará destinado exclusivamente al registro de nuevos préstamos.

Debe mostrar un formulario con los siguientes campos:

* Nombre completo.
* Licenciatura.
* Código de alumno.
* Semestre.
* Correo electrónico.
* Título del libro.
* Fecha de préstamo.
* Persona que entrega el libro.

La fecha límite debe calcularse automáticamente a partir de la fecha de préstamo.

No mostrar campos correspondientes a la devolución.

Al final del formulario deberá existir un botón:

`Registrar préstamo`

---

# 9. Validación del formulario de préstamo

Antes de guardar un préstamo se deberá comprobar que los campos obligatorios tengan información válida.

Todos los campos anteriores serán obligatorios.

No aceptar cadenas compuestas únicamente por espacios.

El correo electrónico debe tener al menos un formato básico válido.

El semestre deberá aceptar únicamente números enteros positivos razonables.

El código de alumno deberá almacenarse como texto, no como número, para evitar problemas con códigos que puedan contener ceros iniciales u otros caracteres.

Si existe algún error:

* No guardar el préstamo.
* Indicar claramente qué campo necesita corregirse.
* Conservar la información que el usuario ya había introducido.

Cuando el préstamo se registre correctamente:

* Guardarlo.
* Mostrar una confirmación.
* Limpiar el formulario.
* Actualizar inmediatamente las demás secciones de la aplicación.

---

# 10. Apartado "Entrega"

Este apartado mostrará únicamente préstamos que aún no hayan sido devueltos.

Debe facilitar localizar rápidamente el préstamo correspondiente.

La vista inicial mostrará una lista o conjunto de tarjetas.

Cada elemento deberá mostrar como información principal:

`Título del libro`

También puede mostrar de manera secundaria:

* Nombre del alumno.
* Código del alumno.
* Fecha del préstamo.
* Fecha límite.

Los préstamos vencidos pueden incluir un indicador visual discreto para diferenciarlos.

Ejemplo:

`VENCIDO`

No es necesario utilizar colores específicos; mantén un diseño coherente y legible.

---

# 11. Seleccionar un préstamo

Cuando el usuario seleccione un préstamo dentro del apartado Entrega, mostrar el detalle completo del registro.

Los datos originales deben mostrarse automáticamente.

No solicitar nuevamente:

* Nombre.
* Licenciatura.
* Código.
* Semestre.
* Correo.
* Título.
* Fecha de préstamo.
* Responsable que entregó el libro.

Estos datos deben recuperarse directamente del registro previamente guardado.

Solamente deberán solicitarse los datos correspondientes a la devolución:

* Fecha de devolución.
* Persona que recibe el libro.

La fecha de devolución deberá mostrar inicialmente la fecha actual.

El usuario podrá modificarla.

Incluir un botón:

`Registrar devolución`

---

# 12. Registrar devolución

Antes de completar la devolución:

* Verificar que exista una persona que recibe el libro.
* Verificar que exista una fecha válida.
* Mostrar una confirmación para evitar devoluciones accidentales.

Ejemplo conceptual:

`¿Deseas registrar la devolución de "Título del libro"?`

Si el usuario confirma:

1. Registrar la fecha de devolución.
2. Registrar la persona que recibe el libro.
3. Determinar automáticamente si se devolvió a tiempo o con retraso.
4. Quitar el préstamo de la lista de préstamos pendientes.
5. Guardarlo en el historial correspondiente.
6. Actualizar inmediatamente la interfaz.

Nunca solicitar nuevamente los datos originales.

---

# 13. Sistema de búsqueda

El apartado Entrega debe permitir buscar préstamos actualmente pendientes.

La búsqueda debe poder realizarse utilizando cualquiera de los siguientes campos:

* Nombre.
* Licenciatura.
* Código de alumno.
* Semestre.
* Correo electrónico.
* Título del libro.
* Fecha de préstamo.
* Persona que entregó el libro.

La búsqueda deberá ignorar diferencias entre mayúsculas y minúsculas en los campos de texto.

Para campos de texto deben aceptarse coincidencias parciales.

Ejemplo:

Buscar:

`quij`

puede encontrar:

`Don Quijote de la Mancha`

No es necesario que todos los filtros estén llenos.

Si se utilizan varios filtros simultáneamente, mostrar únicamente registros que cumplan con todos los filtros indicados.

Incluir un mecanismo sencillo para limpiar los filtros.

---

# 14. Préstamos vencidos

Debe existir un apartado secundario denominado:

`Préstamos vencidos`

Aquí aparecerán automáticamente todos los préstamos que:

* No hayan sido devueltos.
* Hayan superado los 21 días desde la fecha de préstamo.

Mostrar como mínimo:

* Título.
* Nombre del alumno.
* Código del alumno.
* Fecha del préstamo.
* Fecha límite.
* Días de retraso.

Calcular:

`DiasRetraso = FechaActual - FechaLimite`

Un préstamo vencido sigue siendo un préstamo pendiente.

Por ello debe poder registrarse su devolución normalmente.

Una devolución tardía deberá quedar almacenada posteriormente como:

`Devuelto con retraso`

---

# 15. Historial

Crear un apartado secundario llamado:

`Historial`

Aquí deberán mostrarse todos los préstamos que ya hayan sido devueltos.

Permitir distinguir entre:

* Devuelto a tiempo.
* Devuelto con retraso.

Mostrar información suficiente para identificar cada registro.

Como mínimo:

* Título.
* Alumno.
* Código.
* Fecha de préstamo.
* Fecha límite.
* Fecha de devolución.
* Estado.

Al seleccionar un registro, permitir consultar toda la información.

Los registros del historial son únicamente de consulta.

No es necesario implementar edición de registros en esta primera versión.

---

# 16. Almacenamiento

Toda la información deberá almacenarse localmente mediante archivos JSON.

La aplicación deberá crear automáticamente su estructura de carpetas si todavía no existe.

Utilizar una estructura similar a:

`Data/`

Dentro:

`Data/PrestamosActivos/`

`Data/Historial/`

Cada préstamo deberá guardarse preferentemente como un archivo JSON independiente utilizando su identificador único.

Ejemplo:

`Data/PrestamosActivos/550e8400-e29b-41d4-a716-446655440000.json`

Cuando se registre una devolución:

1. Actualizar el registro con la información correspondiente.
2. Guardarlo dentro de `Historial`.
3. Eliminar el archivo correspondiente de `PrestamosActivos`.

No mantener dos copias contradictorias del mismo préstamo.

---

# 17. Estructura conceptual del JSON

Cada registro deberá ser equivalente conceptualmente a:

```json
{
  "idPrestamo": "GUID",
  "nombreAlumno": "Nombre completo",
  "licenciatura": "Licenciatura",
  "codigoAlumno": "Código",
  "semestre": 1,
  "correoElectronico": "correo@ejemplo.com",
  "tituloLibro": "Título",
  "fechaPrestamo": "2026-09-18",
  "fechaLimite": "2026-10-09",
  "responsableEntrega": "Nombre",
  "fechaDevolucion": null,
  "responsableRecepcion": null,
  "estado": "Activo"
}
```

Al registrar la devolución:

```json
{
  "idPrestamo": "GUID",
  "nombreAlumno": "Nombre completo",
  "licenciatura": "Licenciatura",
  "codigoAlumno": "Código",
  "semestre": 1,
  "correoElectronico": "correo@ejemplo.com",
  "tituloLibro": "Título",
  "fechaPrestamo": "2026-09-18",
  "fechaLimite": "2026-10-09",
  "responsableEntrega": "Nombre",
  "fechaDevolucion": "2026-10-03",
  "responsableRecepcion": "Nombre",
  "estado": "DevueltoATiempo"
}
```

Puedes modificar los nombres internos de las propiedades si mejora la consistencia del código, pero mantén toda la información requerida.

---

# 18. Seguridad de los archivos

La aplicación no debe perder toda la información si encuentra un único archivo JSON dañado.

Al cargar los datos:

* Leer cada registro individualmente.
* Manejar errores mediante excepciones controladas.
* Si un archivo no puede leerse, continuar cargando los demás.
* Informar del problema de forma comprensible.

Al guardar:

* Evitar escribir archivos incompletos siempre que sea posible.
* Manejar errores de permisos o acceso al disco.

La aplicación nunca deberá cerrarse abruptamente debido a un error común de lectura o escritura.

---

# 19. Ubicación de los datos

No guardar datos dentro de una ubicación que pueda requerir permisos de administrador.

Preferentemente utilizar una carpeta de datos de usuario apropiada para Windows, por ejemplo mediante:

`Environment.SpecialFolder.LocalApplicationData`

Crear una carpeta propia de la aplicación.

Ejemplo conceptual:

`%LocalAppData%/PrestamoLibros/Data/`

Centraliza la ruta de almacenamiento para que pueda modificarse fácilmente posteriormente.

---

# 20. Arquitectura del código

Evita colocar toda la lógica directamente dentro de los formularios de Windows Forms.

Separar como mínimo:

## Models

Representación de los datos.

Ejemplo:

`Prestamo.cs`

## Services

Lógica del sistema.

Ejemplos:

`PrestamoService.cs`

`JsonStorageService.cs`

## Forms

Interfaces gráficas.

Ejemplos conceptuales:

`MainForm`

`PrestamoForm`

`EntregaForm`

`VencidosForm`

`HistorialForm`

Los nombres pueden variar si existe una estructura más coherente.

Priorizar:

* Código legible.
* Responsabilidades claras.
* Métodos pequeños.
* Eliminación de duplicación innecesaria.
* Comentarios solamente cuando aporten información útil.

---

# 21. Interfaz gráfica

Crear una interfaz sencilla, limpia y profesional.

No necesito una interfaz excesivamente elaborada.

Priorizar:

* Buena jerarquía visual.
* Formularios fáciles de entender.
* Campos correctamente alineados.
* Botones claramente identificables.
* Espaciado consistente.
* Tipografía legible.
* Navegación sencilla.
* Estados vacíos comprensibles.

Ejemplo:

Si no existen préstamos activos:

`No hay préstamos pendientes.`

Si no existen préstamos vencidos:

`No hay préstamos vencidos.`

La aplicación debe poder utilizarse en resoluciones comunes de computadoras de oficina.

Evitar elementos demasiado pequeños.

---

# 22. Confirmaciones

Solicitar confirmación antes de acciones importantes como registrar una devolución.

No saturar al usuario con mensajes innecesarios.

Después de registrar correctamente una acción, mostrar retroalimentación breve.

Ejemplos:

`Préstamo registrado correctamente.`

`Devolución registrada correctamente.`

---

# 23. Actualización de información

Las diferentes vistas deben mantenerse sincronizadas durante la ejecución.

Ejemplo:

Si desde Entrega se devuelve un libro:

* Debe desaparecer inmediatamente de Entrega.
* Debe desaparecer de Vencidos si estaba vencido.
* Debe aparecer inmediatamente en Historial.

No requerir reiniciar la aplicación.

---

# 24. Casos especiales

El sistema deberá soportar correctamente:

* Dos personas con el mismo nombre.
* Dos préstamos del mismo título.
* Una misma persona solicitando varios libros.
* Diferentes alumnos solicitando libros con el mismo título.
* Devoluciones realizadas tarde.
* Registros realizados con fechas anteriores.
* Reiniciar la aplicación conservando todos los datos.

Nunca utilizar únicamente el título del libro como identificador.

Cada préstamo debe identificarse mediante su `IdPrestamo`.

---

# 25. Datos de prueba

Durante desarrollo puedes crear algunos registros ficticios para comprobar:

* Un préstamo activo.
* Un préstamo vencido.
* Una devolución realizada a tiempo.
* Una devolución realizada con retraso.

No dejar datos de prueba obligatorios dentro de la versión final.

---

# 26. Criterios de aceptación

La aplicación se considerará funcional cuando pueda completarse correctamente el siguiente flujo:

1. Ejecutar la aplicación por primera vez.
2. El programa crea automáticamente sus carpetas de almacenamiento.
3. Abrir Préstamo.
4. Introducir información válida.
5. Registrar un préstamo.
6. Cerrar completamente la aplicación.
7. Abrir nuevamente la aplicación.
8. El préstamo continúa almacenado.
9. Abrir Entrega.
10. Localizar el préstamo mediante el título o algún filtro.
11. Seleccionarlo.
12. Comprobar que los datos originales aparecen automáticamente.
13. Introducir únicamente fecha de devolución y persona que recibe.
14. Registrar la devolución.
15. El préstamo desaparece de los pendientes.
16. El préstamo aparece en Historial.
17. Cerrar y volver a abrir la aplicación.
18. La información continúa correctamente almacenada.

También deberá comprobarse que un préstamo con más de 21 días aparezca automáticamente en Préstamos vencidos.

---

# 27. Entregables del proyecto

Genera todos los archivos necesarios para tener un proyecto funcional.

Incluye:

* Código fuente completo.
* Archivo `.csproj`.
* Formularios necesarios.
* Modelos.
* Servicios.
* Sistema de persistencia JSON.
* Manejo de errores.
* Archivo `.gitignore`.
* README.md.

En el README explica:

* Objetivo de la aplicación.
* Requisitos.
* Cómo ejecutar el proyecto.
* Cómo compilarlo.
* Dónde se almacenan los JSON.
* Estructura básica del proyecto.
* Cómo generar una versión ejecutable para Windows.

---

# 28. Compilación

El proyecto deberá compilar correctamente.

Antes de considerar terminado el trabajo:

1. Ejecuta `dotnet restore`.
2. Ejecuta `dotnet build`.
3. Corrige cualquier error de compilación.
4. Si el entorno lo permite, ejecuta la aplicación o las pruebas disponibles.
5. Revisa advertencias importantes.

No presentes el proyecto como terminado mientras existan errores de compilación conocidos.

---

# 29. Publicación

Configura el proyecto para permitir posteriormente una publicación para Windows mediante un comando similar a:

```bash
dotnet publish -c Release -r win-x64 --self-contained true
```

Cuando sea viable, permitir generar un ejecutable independiente para que la computadora destino no necesite tener instalado manualmente el SDK de .NET.

No es necesario crear un instalador MSI en esta primera versión.

---

# 30. Forma de trabajar

Primero inspecciona la especificación completa.

Después:

1. Define la estructura del proyecto.
2. Crea los modelos.
3. Implementa el almacenamiento.
4. Implementa la lógica de negocio.
5. Construye la interfaz.
6. Conecta la interfaz con los servicios.
7. Implementa las validaciones.
8. Prueba los flujos principales.
9. Compila el proyecto.
10. Corrige errores encontrados.
11. Revisa el resultado completo.

No generes solamente fragmentos aislados de código.

Trabaja directamente sobre el proyecto hasta dejar una versión funcional.

Si encuentras una decisión técnica menor no especificada, toma una decisión razonable que mantenga la aplicación sencilla y documenta brevemente dicha decisión.

No detengas innecesariamente el desarrollo para preguntarme detalles menores que puedan resolverse mediante una decisión técnica convencional.

Sin embargo, no amplíes el alcance del sistema con funciones que no hayan sido solicitadas.

El objetivo final es obtener una aplicación pequeña, robusta, comprensible y fácil de mantener para registrar préstamos y devoluciones de libros de manera local.
