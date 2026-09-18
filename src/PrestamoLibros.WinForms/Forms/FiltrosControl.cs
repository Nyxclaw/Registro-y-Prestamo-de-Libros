using PrestamoLibros.Core.Models;

namespace PrestamoLibros.WinForms.Forms;

internal sealed class FiltrosControl : UserControl
{
    private readonly Dictionary<string, TextBox> _textos = new();
    private readonly NumericUpDown _semestre = new() { Minimum = 0, Maximum = 20, Width = 150 };
    private readonly DateTimePicker _fecha = Ui.Fecha();
    internal event EventHandler? Filtrar;
    internal FiltrosControl()
    {
        Dock = DockStyle.Top;
        AutoSize = true;
        var flujo = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true };
        foreach (var (campo, etiqueta) in new[]
        {
            (nameof(FiltroPrestamos.TituloLibro), "Título del libro"),
            (nameof(FiltroPrestamos.NombreAlumno), "Nombre del alumno"),
            (nameof(FiltroPrestamos.Licenciatura), "Licenciatura"),
            (nameof(FiltroPrestamos.CodigoAlumno), "Código de alumno"),
            (nameof(FiltroPrestamos.CorreoElectronico), "Correo electrónico"),
            (nameof(FiltroPrestamos.ResponsableEntrega), "Persona que entregó")
        })
        {
            var texto = new TextBox { Width = 185 };
            _textos.Add(campo, texto);
            flujo.Controls.Add(Envolver(etiqueta, texto));
        }
        flujo.Controls.Add(Envolver("Semestre (0 = todos)", _semestre));
        _fecha.ShowCheckBox = true;
        _fecha.Checked = false;
        _fecha.Width = 185;
        flujo.Controls.Add(Envolver("Fecha de préstamo (opcional)", _fecha));
        var buscar = Ui.Boton("Buscar");
        buscar.Click += (_, _) => Filtrar?.Invoke(this, EventArgs.Empty);
        var limpiar = Ui.Boton("Limpiar filtros");
        limpiar.Click += (_, _) =>
        {
            foreach (var texto in _textos.Values) texto.Clear();
            _semestre.Value = 0;
            _fecha.Checked = false;
            Filtrar?.Invoke(this, EventArgs.Empty);
        };
        flujo.Controls.Add(buscar);
        flujo.Controls.Add(limpiar);
        Controls.Add(flujo);
    }

    private static Control Envolver(string etiqueta, Control control)
    {
        var panel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown, AutoSize = true, WrapContents = false,
            Margin = new Padding(0, 0, 16, 8)
        };
        panel.Controls.Add(new Label { Text = etiqueta, AutoSize = true });
        panel.Controls.Add(control);
        return panel;
    }

    internal FiltroPrestamos Valor => new()
    {
        TituloLibro = _textos[nameof(FiltroPrestamos.TituloLibro)].Text,
        NombreAlumno = _textos[nameof(FiltroPrestamos.NombreAlumno)].Text,
        Licenciatura = _textos[nameof(FiltroPrestamos.Licenciatura)].Text,
        CodigoAlumno = _textos[nameof(FiltroPrestamos.CodigoAlumno)].Text,
        CorreoElectronico = _textos[nameof(FiltroPrestamos.CorreoElectronico)].Text,
        ResponsableEntrega = _textos[nameof(FiltroPrestamos.ResponsableEntrega)].Text,
        Semestre = _semestre.Value == 0 ? null : (int)_semestre.Value,
        FechaPrestamo = _fecha.Checked ? DateOnly.FromDateTime(_fecha.Value) : null
    };
}
