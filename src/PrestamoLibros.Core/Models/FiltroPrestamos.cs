namespace PrestamoLibros.Core.Models;

public sealed record FiltroPrestamos
{
    public string NombreAlumno { get; init; } = "";
    public string Licenciatura { get; init; } = "";
    public string CodigoAlumno { get; init; } = "";
    public int? Semestre { get; init; }
    public string CorreoElectronico { get; init; } = "";
    public string TituloLibro { get; init; } = "";
    public DateOnly? FechaPrestamo { get; init; }
    public string ResponsableEntrega { get; init; } = "";

    public bool Coincide(Prestamo p) =>
        Contiene(p.NombreAlumno, NombreAlumno) && Contiene(p.Licenciatura, Licenciatura) &&
        Contiene(p.CodigoAlumno, CodigoAlumno) && (!Semestre.HasValue || p.Semestre == Semestre) &&
        Contiene(p.CorreoElectronico, CorreoElectronico) && Contiene(p.TituloLibro, TituloLibro) &&
        (!FechaPrestamo.HasValue || p.FechaPrestamo == FechaPrestamo) &&
        Contiene(p.ResponsableEntrega, ResponsableEntrega);

    private static bool Contiene(string valor, string filtro) =>
        valor.Contains(filtro.Trim(), StringComparison.OrdinalIgnoreCase);
}
