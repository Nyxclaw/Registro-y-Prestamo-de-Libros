using PrestamoLibros.Core.Services;

namespace PrestamoLibros.WinForms.Forms;

internal sealed class MainForm : Form
{
    private readonly PrestamoService _service;
    private readonly Panel _contenido = new() { Dock = DockStyle.Fill };
    private readonly List<UserControl> _vistas = [];
    private readonly System.Windows.Forms.Timer _reloj = new() { Interval = 30_000 };
    private DateOnly _ultimoDia;

    internal MainForm(PrestamoService service)
    {
        _service = service;
        _ultimoDia = service.Hoy;
        Text = "Registro y préstamo de libros";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1200, 800);
        MinimumSize = new Size(1000, 680);
        Font = new Font("Segoe UI", 10);
        BackColor = Color.White;
        var navegacion = new FlowLayoutPanel
        {
            Dock = DockStyle.Left, Width = 190, FlowDirection = FlowDirection.TopDown,
            WrapContents = false, Padding = new Padding(16), BackColor = Color.FromArgb(239, 244, 245)
        };
        navegacion.Controls.Add(new Label
        {
            Text = "PRÉSTAMO\nDE LIBROS", AutoSize = true, Font = new Font("Segoe UI", 13, FontStyle.Bold),
            ForeColor = Ui.Tinta, Margin = new Padding(0, 8, 0, 28)
        });
        AgregarVista(navegacion, "Préstamo", new PrestamoForm(service), true);
        AgregarVista(navegacion, "Entrega", new ListadoForm(service, TipoListado.Entrega), true);
        navegacion.Controls.Add(new Label { Text = "CONSULTAS", AutoSize = true, Margin = new Padding(0, 30, 0, 6) });
        AgregarVista(navegacion, "Préstamos vencidos", new ListadoForm(service, TipoListado.Vencidos), false);
        AgregarVista(navegacion, "Historial", new ListadoForm(service, TipoListado.Historial), false);
        var actualizar = Ui.Boton("Actualizar");
        actualizar.Margin = new Padding(0, 30, 0, 8);
        actualizar.Click += (_, _) => Cargar();
        navegacion.Controls.Add(actualizar);
        Controls.Add(_contenido);
        Controls.Add(navegacion);
        Mostrar(_vistas[0]);
        Shown += (_, _) => Cargar();
        _reloj.Tick += (_, _) =>
        {
            if (_ultimoDia == service.Hoy) return;
            _ultimoDia = service.Hoy;
            foreach (var vista in _vistas.OfType<ListadoForm>()) vista.Actualizar();
        };
        _reloj.Start();
    }

    private void AgregarVista(FlowLayoutPanel navegacion, string texto, UserControl vista, bool principal)
    {
        _vistas.Add(vista);
        _contenido.Controls.Add(vista);
        var boton = Ui.Boton(texto, principal);
        boton.Width = 156;
        if (principal) boton.Font = new Font(Font, FontStyle.Bold);
        boton.Click += (_, _) => Mostrar(vista);
        navegacion.Controls.Add(boton);
    }

    private void Mostrar(UserControl vista)
    {
        foreach (var otra in _vistas) otra.Visible = otra == vista;
        vista.BringToFront();
        if (vista is ListadoForm listado) listado.Actualizar();
    }

    private void Cargar()
    {
        var avisos = _service.Cargar();
        if (avisos.Count > 0)
            MessageBox.Show(this, "Se cargaron los registros que pudieron leerse. No se eliminaron archivos dañados.\n\n" +
                string.Join("\n\n", avisos.Take(8)) + (avisos.Count > 8 ? $"\n\nHay {avisos.Count - 8} avisos adicionales." : "") +
                "\n\nCarpeta de datos: " + _service.RutaDatos,
                "Revisar almacenamiento", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _reloj.Dispose();
        base.Dispose(disposing);
    }
}
