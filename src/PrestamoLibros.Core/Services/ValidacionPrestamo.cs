using System.Net.Mail;
using PrestamoLibros.Core.Models;

namespace PrestamoLibros.Core.Services;

public sealed class ValidacionException(IReadOnlyDictionary<string, string> errores)
    : Exception(string.Join(Environment.NewLine, errores.Values))
{
    public IReadOnlyDictionary<string, string> Errores { get; } = errores;
}

public static class ValidacionPrestamo
{
    public const int SemestreMaximo = 20;

    public static Dictionary<string, string> Revisar(Prestamo p, DateOnly hoy)
    {
        var errores = new Dictionary<string, string>();
        void Requerido(string campo, string? valor, string etiqueta)
        {
            if (string.IsNullOrWhiteSpace(valor)) errores[campo] = $"{etiqueta}: campo obligatorio.";
        }
        Requerido(nameof(p.NombreAlumno), p.NombreAlumno, "Nombre completo");
        Requerido(nameof(p.Licenciatura), p.Licenciatura, "Licenciatura");
        Requerido(nameof(p.CodigoAlumno), p.CodigoAlumno, "Código de alumno");
        Requerido(nameof(p.TituloLibro), p.TituloLibro, "Título del libro");
        Requerido(nameof(p.ResponsableEntrega), p.ResponsableEntrega, "Persona que entrega");
        if (p.Semestre is < 1 or > SemestreMaximo)
            errores[nameof(p.Semestre)] = $"Semestre: utiliza un entero entre 1 y {SemestreMaximo}.";
        if (string.IsNullOrWhiteSpace(p.CorreoElectronico) ||
            !MailAddress.TryCreate(p.CorreoElectronico, out var correo) ||
            correo.Address != p.CorreoElectronico || !correo.Host.Contains('.'))
            errores[nameof(p.CorreoElectronico)] = "Correo electrónico: escribe una dirección válida, por ejemplo nombre@escuela.mx.";
        if (p.FechaPrestamo == default || p.FechaPrestamo > hoy || p.FechaPrestamo > DateOnly.MaxValue.AddDays(-21))
            errores[nameof(p.FechaPrestamo)] = "Fecha de préstamo: selecciona una fecha válida que no sea futura.";
        if (p.IdPrestamo == Guid.Empty) errores[nameof(p.IdPrestamo)] = "El identificador del préstamo no es válido.";
        if (p.FechaDevolucion is { } fecha)
        {
            if (fecha < p.FechaPrestamo || fecha > hoy)
                errores[nameof(p.FechaDevolucion)] = "Fecha de devolución: debe estar entre el préstamo y hoy.";
            Requerido(nameof(p.ResponsableRecepcion), p.ResponsableRecepcion, "Persona que recibe");
        }
        else if (p.ResponsableRecepcion is not null)
            errores[nameof(p.ResponsableRecepcion)] = "La persona que recibe necesita una fecha de devolución.";
        return errores;
    }

    public static void Comprobar(Prestamo p, DateOnly hoy)
    {
        var errores = Revisar(p, hoy);
        if (errores.Count > 0) throw new ValidacionException(errores);
    }
}
