using PrestamoLibros.Core.Models;

namespace PrestamoLibros.Core.Services;

public sealed class PrestamoService(JsonStorageService storage, Func<DateOnly>? reloj = null)
{
    private readonly Func<DateOnly> _hoy = reloj ?? (() => DateOnly.FromDateTime(DateTime.Today));
    private readonly List<Prestamo> _prestamos = [];
    public event EventHandler? Cambios;
    public DateOnly Hoy => _hoy();
    public string RutaDatos => storage.Ruta;

    public IReadOnlyList<string> Cargar()
    {
        var resultado = storage.Cargar(Hoy);
        _prestamos.Clear();
        _prestamos.AddRange(resultado.Prestamos);
        Cambios?.Invoke(this, EventArgs.Empty);
        return resultado.Avisos;
    }

    public Prestamo Registrar(Prestamo datos)
    {
        var p = datos with
        {
            IdPrestamo = Guid.NewGuid(),
            NombreAlumno = datos.NombreAlumno.Trim(), Licenciatura = datos.Licenciatura.Trim(),
            CodigoAlumno = datos.CodigoAlumno.Trim(), CorreoElectronico = datos.CorreoElectronico.Trim(),
            TituloLibro = datos.TituloLibro.Trim(), ResponsableEntrega = datos.ResponsableEntrega.Trim(),
            FechaDevolucion = null, ResponsableRecepcion = null
        };
        ValidacionPrestamo.Comprobar(p, Hoy);
        storage.GuardarNuevo(p);
        _prestamos.Add(p);
        Cambios?.Invoke(this, EventArgs.Empty);
        return p;
    }

    public string? Devolver(Guid id, DateOnly fecha, string receptor)
    {
        var indice = _prestamos.FindIndex(p => p.IdPrestamo == id);
        if (indice < 0) throw new InvalidOperationException("No se encontró el préstamo. Actualiza la lista.");
        var original = _prestamos[indice];
        if (original.Devuelto) throw new InvalidOperationException("Este préstamo ya fue devuelto.");
        var devuelto = original with { FechaDevolucion = fecha, ResponsableRecepcion = receptor.Trim() };
        ValidacionPrestamo.Comprobar(devuelto, Hoy);
        var aviso = storage.GuardarDevolucion(devuelto);
        _prestamos[indice] = devuelto;
        Cambios?.Invoke(this, EventArgs.Empty);
        return aviso;
    }

    public IReadOnlyList<Prestamo> Pendientes(FiltroPrestamos? filtro = null) => _prestamos
        .Where(p => !p.Devuelto && (filtro is null || filtro.Coincide(p)))
        .OrderBy(p => p.FechaLimite).ThenBy(p => p.TituloLibro).ToArray();
    public IReadOnlyList<Prestamo> Vencidos() => Pendientes().Where(p => p.EstadoEn(Hoy) == EstadoPrestamo.Vencido).ToArray();
    public IReadOnlyList<Prestamo> Historial() => _prestamos.Where(p => p.Devuelto)
        .OrderByDescending(p => p.FechaDevolucion).ToArray();
}
