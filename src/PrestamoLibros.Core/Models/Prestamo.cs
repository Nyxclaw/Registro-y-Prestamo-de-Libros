using System.Text.Json.Serialization;

namespace PrestamoLibros.Core.Models;

public enum EstadoPrestamo { Activo, Vencido, DevueltoATiempo, DevueltoConRetraso }

public sealed record Prestamo
{
    public Guid IdPrestamo { get; init; } = Guid.NewGuid();
    public string NombreAlumno { get; init; } = "";
    public string Licenciatura { get; init; } = "";
    public string CodigoAlumno { get; init; } = "";
    public int Semestre { get; init; }
    public string CorreoElectronico { get; init; } = "";
    public string TituloLibro { get; init; } = "";
    public DateOnly FechaPrestamo { get; init; }
    public DateOnly FechaLimite => FechaPrestamo.AddDays(21);
    public string ResponsableEntrega { get; init; } = "";
    public DateOnly? FechaDevolucion { get; init; }
    public string? ResponsableRecepcion { get; init; }
    public EstadoPrestamo Estado => EstadoEn(DateOnly.FromDateTime(DateTime.Today));

    [JsonIgnore]
    public bool Devuelto => FechaDevolucion.HasValue;

    public EstadoPrestamo EstadoEn(DateOnly hoy) => FechaDevolucion is { } fecha
        ? fecha <= FechaLimite ? EstadoPrestamo.DevueltoATiempo : EstadoPrestamo.DevueltoConRetraso
        : hoy <= FechaLimite ? EstadoPrestamo.Activo : EstadoPrestamo.Vencido;

    public int DiasRetraso(DateOnly hoy) => Math.Max(0, ((FechaDevolucion ?? hoy).DayNumber - FechaLimite.DayNumber));
}
