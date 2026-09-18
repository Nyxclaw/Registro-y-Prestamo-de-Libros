using System.Text.Json;
using System.Text.Json.Serialization;
using PrestamoLibros.Core.Models;

namespace PrestamoLibros.Core.Services;

public sealed record ResultadoCarga(IReadOnlyList<Prestamo> Prestamos, IReadOnlyList<string> Avisos);

public sealed class JsonStorageService
{
    private static readonly JsonSerializerOptions Opciones = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };
    public static string RutaPredeterminada => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PrestamoLibros", "Data");
    public string Ruta { get; }
    private string Activos => Path.Combine(Ruta, "PrestamosActivos");
    private string Historial => Path.Combine(Ruta, "Historial");

    public JsonStorageService(string? ruta = null) => Ruta = ruta ?? RutaPredeterminada;

    private void CrearCarpetas()
    {
        Directory.CreateDirectory(Activos);
        Directory.CreateDirectory(Historial);
    }

    public ResultadoCarga Cargar(DateOnly hoy)
    {
        var registros = new Dictionary<Guid, Prestamo>();
        var avisos = new List<string>();
        try { CrearCarpetas(); }
        catch (Exception ex) when (EsErrorArchivo(ex))
        {
            return new([], [$"No se pudo abrir la carpeta de datos: {ex.Message}"]);
        }
        foreach (var carpeta in new[] { Historial, Activos })
        {
            string[] archivos;
            try { archivos = Directory.GetFiles(carpeta, "*.json"); }
            catch (Exception ex) when (EsErrorArchivo(ex))
            {
                avisos.Add($"No se pudo leer {carpeta}: {ex.Message}");
                continue;
            }
            foreach (var archivo in archivos)
            {
                try
                {
                    var p = JsonSerializer.Deserialize<Prestamo>(File.ReadAllText(archivo), Opciones)
                        ?? throw new JsonException("Registro vacío.");
                    ValidacionPrestamo.Comprobar(p, hoy);
                    if (Path.GetFileNameWithoutExtension(archivo) != p.IdPrestamo.ToString())
                        throw new JsonException("El nombre del archivo no coincide con su identificador.");
                    if (carpeta == Historial && !p.Devuelto)
                        throw new JsonException("El registro del historial no contiene una devolución.");
                    if (!registros.TryAdd(p.IdPrestamo, p))
                    {
                        avisos.Add($"Identificador duplicado en {Path.GetFileName(archivo)}. Se conserva la lectura del historial; revisa los archivos.");
                        continue;
                    }
                    // Una devolución interrumpida ya está guardada: solo queda mover su único archivo.
                    if (carpeta == Activos && p.Devuelto)
                    {
                        var aviso = Archivar(p.IdPrestamo);
                        if (aviso is not null) avisos.Add(aviso);
                    }
                }
                catch (Exception ex) when (EsErrorArchivo(ex) || ex is JsonException or ValidacionException or NotSupportedException)
                {
                    avisos.Add($"No se pudo cargar {Path.GetFileName(archivo)}: {ex.Message}");
                }
            }
        }
        return new(registros.Values.ToArray(), avisos);
    }

    public void GuardarNuevo(Prestamo prestamo)
    {
        CrearCarpetas();
        if (File.Exists(Archivo(Historial, prestamo.IdPrestamo)))
            throw new IOException("Ya existe un préstamo con ese identificador.");
        EscribirAtomico(Archivo(Activos, prestamo.IdPrestamo), prestamo, false);
    }

    public string? GuardarDevolucion(Prestamo prestamo)
    {
        CrearCarpetas();
        var destino = Archivo(Activos, prestamo.IdPrestamo);
        if (!File.Exists(destino) || File.Exists(Archivo(Historial, prestamo.IdPrestamo)))
            throw new IOException("El archivo original ya no está disponible o ya fue devuelto. Actualiza la lista.");
        // Primero reemplazar de forma atómica; después mover dentro del mismo volumen.
        // Si se interrumpe el proceso, nunca quedan dos versiones contradictorias.
        EscribirAtomico(destino, prestamo, true);
        return Archivar(prestamo.IdPrestamo);
    }

    private string? Archivar(Guid id)
    {
        try { File.Move(Archivo(Activos, id), Archivo(Historial, id)); return null; }
        catch (Exception ex) when (EsErrorArchivo(ex))
        {
            return $"La devolución {id} está guardada, pero no se pudo mover a la carpeta Historial. Se reintentará al actualizar: {ex.Message}";
        }
    }

    private static void EscribirAtomico(string destino, Prestamo prestamo, bool reemplazar)
    {
        var temporal = destino + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var stream = new FileStream(temporal, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                JsonSerializer.Serialize(stream, prestamo, Opciones);
                stream.Flush(flushToDisk: true);
            }
            File.Move(temporal, destino, reemplazar);
        }
        finally
        {
            try { if (File.Exists(temporal)) File.Delete(temporal); }
            catch (Exception ex) when (EsErrorArchivo(ex)) { /* El temporal no se carga como registro. */ }
        }
    }

    private static string Archivo(string carpeta, Guid id) => Path.Combine(carpeta, id + ".json");
    public static bool EsErrorArchivo(Exception ex) => ex is IOException or UnauthorizedAccessException or System.Security.SecurityException;
}
