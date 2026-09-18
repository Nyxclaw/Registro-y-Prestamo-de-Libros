using System.Text.Json;
using System.Text.Json.Serialization;
using PrestamoLibros.Core.Models;
using PrestamoLibros.Core.Services;

// Ejecutor sin paquetes externos: cualquier fallo produce un código de salida distinto de cero.
var hoy = new DateOnly(2026, 9, 18);
var pruebas = new (string Nombre, Action Ejecutar)[]
{
    ("Límite de 21 días y los cuatro estados", () =>
    {
        var p = Datos(hoy.AddDays(-21));
        Igual(hoy, p.FechaLimite);
        Igual(EstadoPrestamo.Activo, p.EstadoEn(hoy));
        Igual(EstadoPrestamo.Vencido, p.EstadoEn(hoy.AddDays(1)));
        Igual(1, p.DiasRetraso(hoy.AddDays(1)));
        Igual(EstadoPrestamo.DevueltoATiempo, (p with { FechaDevolucion = hoy }).EstadoEn(hoy.AddDays(100)));
        Igual(EstadoPrestamo.DevueltoConRetraso, (p with { FechaDevolucion = hoy.AddDays(1) }).EstadoEn(hoy.AddDays(100)));
        Igual(new DateOnly(2024, 3, 1), Datos(new DateOnly(2024, 2, 9)).FechaLimite);
        Igual(new DateOnly(2027, 1, 10), Datos(new DateOnly(2026, 12, 20)).FechaLimite);
    }),
    ("Registro, reinicio, devolución e historial persistente", () => ConCarpeta(ruta =>
    {
        var service = Servicio(ruta);
        Igual(0, service.Cargar().Count);
        Verdadero(Directory.Exists(Path.Combine(ruta, "PrestamosActivos")));
        Verdadero(Directory.Exists(Path.Combine(ruta, "Historial")));
        var eventos = 0;
        service.Cambios += (_, _) => eventos++;
        var p = service.Registrar(Datos(hoy.AddDays(-5)));
        Igual(1, eventos);
        var activo = Path.Combine(ruta, "PrestamosActivos", p.IdPrestamo + ".json");
        using (var json = JsonDocument.Parse(File.ReadAllText(activo)))
        {
            Igual("001-A", json.RootElement.GetProperty("codigoAlumno").GetString()!);
            Igual("2026-09-13", json.RootElement.GetProperty("fechaPrestamo").GetString()!);
            Igual("2026-10-04", json.RootElement.GetProperty("fechaLimite").GetString()!);
        }
        service = Servicio(ruta);
        Igual(0, service.Cargar().Count);
        Igual(p, service.Pendientes().Single());
        Igual<string?>(null, service.Devolver(p.IdPrestamo, hoy, "  Ana Recepción  "));
        Igual(0, service.Pendientes().Count);
        Igual(0, service.Vencidos().Count);
        Verdadero(!File.Exists(activo));
        Verdadero(File.Exists(Path.Combine(ruta, "Historial", p.IdPrestamo + ".json")));
        service = Servicio(ruta);
        Igual(0, service.Cargar().Count);
        var devuelto = service.Historial().Single();
        Igual("Ana Recepción", devuelto.ResponsableRecepcion!);
        Igual(p, devuelto with { FechaDevolucion = null, ResponsableRecepcion = null });
        Igual(EstadoPrestamo.DevueltoATiempo, devuelto.EstadoEn(hoy));
        Falla<InvalidOperationException>(() => service.Devolver(p.IdPrestamo, hoy, "Otra persona"));
    })),
    ("Vencidos, cambio de día y devolución tardía", () => ConCarpeta(ruta =>
    {
        var fechaActual = hoy;
        var service = new PrestamoService(new JsonStorageService(ruta), () => fechaActual);
        service.Cargar();
        var p = service.Registrar(Datos(hoy.AddDays(-21)));
        Igual(0, service.Vencidos().Count);
        fechaActual = hoy.AddDays(1);
        Igual(1, service.Vencidos().Count);
        service.Devolver(p.IdPrestamo, fechaActual, "Receptor");
        Igual(0, service.Pendientes().Count);
        Igual(0, service.Vencidos().Count);
        Igual(EstadoPrestamo.DevueltoConRetraso, service.Historial().Single().EstadoEn(fechaActual));
    })),
    ("Identificadores únicos con personas y títulos repetidos", () => ConCarpeta(ruta =>
    {
        var service = Servicio(ruta);
        service.Cargar();
        var a = service.Registrar(Datos(hoy));
        var b = service.Registrar(Datos(hoy));
        Verdadero(a.IdPrestamo != b.IdPrestamo);
        service.Devolver(b.IdPrestamo, hoy, "Receptor");
        Igual(a.IdPrestamo, service.Pendientes().Single().IdPrestamo);
        Igual(b.IdPrestamo, service.Historial().Single().IdPrestamo);
    })),
    ("Búsqueda por los ocho campos, parciales y filtros combinados", () => ConCarpeta(ruta =>
    {
        var service = Servicio(ruta);
        service.Cargar();
        var p = service.Registrar(Datos(hoy));
        service.Registrar(Datos(hoy.AddDays(-1)) with
        {
            NombreAlumno = "Otra Persona", Licenciatura = "Historia", CodigoAlumno = "002-B", Semestre = 4,
            CorreoElectronico = "otro@escuela.mx", TituloLibro = "Otro libro", ResponsableEntrega = "Luis"
        });
        var filtros = new[]
        {
            new FiltroPrestamos { NombreAlumno = " mAR " }, new FiltroPrestamos { Licenciatura = "SIST" },
            new FiltroPrestamos { CodigoAlumno = "001-a" }, new FiltroPrestamos { Semestre = 2 },
            new FiltroPrestamos { CorreoElectronico = "MARIA@" }, new FiltroPrestamos { TituloLibro = "QUIJ" },
            new FiltroPrestamos { FechaPrestamo = hoy }, new FiltroPrestamos { ResponsableEntrega = "ELEN" },
            new FiltroPrestamos { TituloLibro = "quij", CodigoAlumno = "001", Semestre = 2 }
        };
        foreach (var filtro in filtros) Igual(p.IdPrestamo, service.Pendientes(filtro).Single().IdPrestamo);
        Igual(0, service.Pendientes(new FiltroPrestamos { TituloLibro = "quij", Semestre = 4 }).Count);
        Igual(2, service.Pendientes(new FiltroPrestamos()).Count);
        service.Devolver(p.IdPrestamo, hoy, "Receptor");
        Igual(0, service.Pendientes(new FiltroPrestamos { TituloLibro = "quij" }).Count);
    })),
    ("Validaciones impiden escrituras y conservan los originales", () => ConCarpeta(ruta =>
    {
        var service = Servicio(ruta);
        service.Cargar();
        foreach (var invalido in new[]
        {
            Datos(hoy) with { NombreAlumno = "   " }, Datos(hoy) with { Licenciatura = " " },
            Datos(hoy) with { CodigoAlumno = "" }, Datos(hoy) with { TituloLibro = "\t" },
            Datos(hoy) with { ResponsableEntrega = "" }, Datos(hoy) with { CorreoElectronico = "no-es-correo" },
            Datos(hoy) with { CorreoElectronico = "Nombre <nombre@escuela.mx>" },
            Datos(hoy) with { Semestre = 0 }, Datos(hoy) with { Semestre = 21 },
            Datos(hoy.AddDays(1)), Datos(default)
        }) Falla<ValidacionException>(() => service.Registrar(invalido));
        Igual(0, Directory.GetFiles(ruta, "*.json", SearchOption.AllDirectories).Length);
        var p = service.Registrar(Datos(hoy.AddDays(-3)));
        Falla<ValidacionException>(() => service.Devolver(p.IdPrestamo, hoy, "   "));
        Falla<ValidacionException>(() => service.Devolver(p.IdPrestamo, hoy.AddDays(-4), "Receptor"));
        Falla<ValidacionException>(() => service.Devolver(p.IdPrestamo, hoy.AddDays(1), "Receptor"));
        Igual(p, service.Pendientes().Single());
        var reinicio = Servicio(ruta);
        reinicio.Cargar();
        Igual(p, reinicio.Pendientes().Single());
        Igual(0, reinicio.Historial().Count);
    })),
    ("Un JSON dañado no impide recuperar los demás", () => ConCarpeta(ruta =>
    {
        var service = Servicio(ruta);
        service.Cargar();
        var p = service.Registrar(Datos(hoy));
        File.WriteAllText(Path.Combine(ruta, "PrestamosActivos", "roto.json"), "{incompleto");
        File.WriteAllText(Path.Combine(ruta, "PrestamosActivos", "nulo.json"), "null");
        File.WriteAllText(Path.Combine(ruta, "PrestamosActivos", "vacio.json"), "{}");
        File.WriteAllText(Path.Combine(ruta, "PrestamosActivos", "pendiente.tmp"), "{incompleto");
        Igual(3, service.Cargar().Count);
        Igual(p, service.Pendientes().Single());
        Verdadero(File.Exists(Path.Combine(ruta, "PrestamosActivos", "roto.json")));
    })),
    ("Fallo de disco no agrega registros fantasma", () => ConCarpeta(ruta =>
    {
        var bloqueo = Path.Combine(ruta, "archivo-en-vez-de-carpeta");
        File.WriteAllText(bloqueo, "bloqueo");
        var service = Servicio(bloqueo);
        Igual(1, service.Cargar().Count);
        Falla<IOException>(() => service.Registrar(Datos(hoy)));
        Igual(0, service.Pendientes().Count);
    })),
    ("Fallo al guardar devolución conserva préstamo pendiente", () => ConCarpeta(ruta =>
    {
        var service = Servicio(ruta);
        service.Cargar();
        var p = service.Registrar(Datos(hoy));
        Directory.Delete(Path.Combine(ruta, "Historial"));
        File.WriteAllText(Path.Combine(ruta, "Historial"), "bloqueo");
        Falla<IOException>(() => service.Devolver(p.IdPrestamo, hoy, "Receptor"));
        Igual(p, service.Pendientes().Single());
        Igual(0, service.Historial().Count);
        File.Delete(Path.Combine(ruta, "Historial"));
        Igual(0, service.Cargar().Count);
        Igual(p, service.Pendientes().Single());
    })),
    ("Devolución interrumpida se recupera sin duplicar archivos", () => ConCarpeta(ruta =>
    {
        var service = Servicio(ruta);
        service.Cargar();
        var p = service.Registrar(Datos(hoy.AddDays(-30)));
        var destino = Path.Combine(ruta, "Historial", p.IdPrestamo + ".json");
        // Una carpeta en el destino simula un fallo al mover después de guardar.
        Directory.CreateDirectory(destino);
        Verdadero(service.Devolver(p.IdPrestamo, hoy, "Receptor") is not null);
        Igual(0, service.Pendientes().Count);
        Igual(1, service.Historial().Count);
        Igual(1, Directory.GetFiles(ruta, "*.json", SearchOption.AllDirectories).Length);
        var reinicio = Servicio(ruta);
        Igual(1, reinicio.Cargar().Count);
        Igual(0, reinicio.Pendientes().Count);
        Igual(1, reinicio.Historial().Count);
        Directory.Delete(destino);
        Igual(0, reinicio.Cargar().Count);
        Verdadero(File.Exists(destino));
        Igual(0, Directory.GetFiles(Path.Combine(ruta, "PrestamosActivos"), "*.json").Length);
        Igual(EstadoPrestamo.DevueltoConRetraso, reinicio.Historial().Single().EstadoEn(hoy));
        Igual(0, Directory.GetFiles(ruta, "*.tmp", SearchOption.AllDirectories).Length);
    })),
    ("Archivos con identificador o datos inválidos se rechazan", () => ConCarpeta(ruta =>
    {
        var storage = new JsonStorageService(ruta);
        storage.Cargar(hoy);
        var p = Datos(hoy);
        var opciones = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };
        File.WriteAllText(Path.Combine(ruta, "PrestamosActivos", Guid.NewGuid() + ".json"), JsonSerializer.Serialize(p, opciones));
        File.WriteAllText(Path.Combine(ruta, "Historial", p.IdPrestamo + ".json"), JsonSerializer.Serialize(p, opciones));
        var resultado = storage.Cargar(hoy);
        Igual(2, resultado.Avisos.Count);
        Igual(0, resultado.Prestamos.Count);
    }))
};

var fallos = 0;
foreach (var (nombre, ejecutar) in pruebas)
{
    try { ejecutar(); Console.WriteLine($"OK  {nombre}"); }
    catch (Exception ex) { fallos++; Console.Error.WriteLine($"ERROR  {nombre}\n{ex}"); }
}
Console.WriteLine($"{pruebas.Length - fallos}/{pruebas.Length} pruebas correctas.");
return fallos == 0 ? 0 : 1;

PrestamoService Servicio(string ruta) => new(new JsonStorageService(ruta), () => hoy);

static Prestamo Datos(DateOnly fecha) => new()
{
    NombreAlumno = "María Pérez", Licenciatura = "Sistemas", CodigoAlumno = "001-A", Semestre = 2,
    CorreoElectronico = "maria@escuela.mx", TituloLibro = "Don Quijote de la Mancha",
    FechaPrestamo = fecha, ResponsableEntrega = "Elena"
};

static void ConCarpeta(Action<string> prueba)
{
    var ruta = Path.Combine(Path.GetTempPath(), "PrestamoLibros.Tests", Guid.NewGuid().ToString());
    Directory.CreateDirectory(ruta);
    try { prueba(ruta); }
    finally { Directory.Delete(ruta, true); }
}

static void Igual<T>(T esperado, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(esperado, actual))
        throw new Exception($"Se esperaba {esperado}; se obtuvo {actual}.");
}
static void Verdadero(bool condicion) { if (!condicion) throw new Exception("La condición no se cumplió."); }
static void Falla<T>(Action accion) where T : Exception
{
    try { accion(); }
    catch (T) { return; }
    throw new Exception($"Se esperaba {typeof(T).Name}.");
}
