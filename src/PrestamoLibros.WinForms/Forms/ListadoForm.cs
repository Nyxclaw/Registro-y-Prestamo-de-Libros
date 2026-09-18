using PrestamoLibros.Core.Models;
using PrestamoLibros.Core.Services;

namespace PrestamoLibros.WinForms.Forms;

internal enum TipoListado { Entrega, Vencidos, Historial }

internal sealed class ListadoForm : UserControl
{
    private readonly PrestamoService _service;
    private readonly TipoListado _tipo;
    private readonly FiltrosControl? _filtros;
    private readonly DataGridView _tabla = new()
    {
        Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
        MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        AutoGenerateColumns = false, RowHeadersVisible = false,
        BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
        ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
    };
    private readonly Label _resumen = Ui.Texto("");
    private readonly Button _abrir;

    internal ListadoForm(PrestamoService service, TipoListado tipo)
    {
        _service = service;
        _tipo = tipo;
        Dock = DockStyle.Fill;
        Padding = new Padding(24);
        var contenido = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5 };
        contenido.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        contenido.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        contenido.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        contenido.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        contenido.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        contenido.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        contenido.Controls.Add(Ui.Titulo(tipo switch
        {
            TipoListado.Entrega => "Entrega de libros", TipoListado.Vencidos => "Préstamos vencidos", _ => "Historial"
        }), 0, 0);
        if (tipo == TipoListado.Entrega)
        {
            _filtros = new FiltrosControl();
            _filtros.Filtrar += (_, _) => Actualizar();
            contenido.Controls.Add(_filtros, 0, 1);
        }
        else contenido.Controls.Add(Ui.Texto(tipo == TipoListado.Vencidos
            ? "Préstamos pendientes con más de 21 días. Selecciona un registro para devolverlo."
            : "Devoluciones registradas. Selecciona un registro para consultar sus datos."), 0, 1);
        contenido.Controls.Add(_resumen, 0, 2);
        Columna("Título del libro", "Titulo", 190, 180);
        Columna("Alumno", "Alumno", 150, 130);
        Columna("Código", "Codigo", 85, 75);
        Columna("Préstamo", "FechaPrestamo", 95, 80);
        Columna("Fecha límite", "FechaLimite", 95, 80);
        if (tipo == TipoListado.Vencidos) Columna("Días de retraso", "DiasRetraso", 75, 65);
        if (tipo == TipoListado.Historial) Columna("Devolución", "FechaDevolucion", 95, 80);
        Columna("Estado", "Estado", 145, 115);
        _tabla.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        _tabla.DefaultCellStyle.Padding = new Padding(6);
        _tabla.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 249);
        _tabla.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) Abrir(); };
        _tabla.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { e.Handled = true; e.SuppressKeyPress = true; Abrir(); } };
        contenido.Controls.Add(_tabla, 0, 3);
        _abrir = Ui.Boton(tipo == TipoListado.Historial ? "Consultar detalle" : "Registrar devolución", true);
        _abrir.Click += (_, _) => Abrir();
        contenido.Controls.Add(_abrir, 0, 4);
        Controls.Add(contenido);
        _service.Cambios += Cambios;
        Actualizar();
    }

    private void Columna(string titulo, string propiedad, int peso, int minimo) => _tabla.Columns.Add(new DataGridViewTextBoxColumn
    {
        HeaderText = titulo, DataPropertyName = propiedad, FillWeight = peso, MinimumWidth = minimo,
        SortMode = DataGridViewColumnSortMode.NotSortable
    });

    private void Cambios(object? sender, EventArgs e) => Actualizar();

    internal void Actualizar()
    {
        var seleccion = (_tabla.CurrentRow?.DataBoundItem as FilaPrestamo)?.Prestamo.IdPrestamo;
        var registros = _tipo switch
        {
            TipoListado.Entrega => _service.Pendientes(_filtros?.Valor),
            TipoListado.Vencidos => _service.Vencidos(), _ => _service.Historial()
        };
        _tabla.DataSource = registros.Select(p => new FilaPrestamo(p, _service.Hoy)).ToList();
        _abrir.Enabled = registros.Count > 0;
        _resumen.Text = registros.Count > 0 ? $"{registros.Count} registro(s). Selecciona una fila para ver el detalle." : _tipo switch
        {
            TipoListado.Entrega => _service.Pendientes().Count == 0 ? "No hay préstamos pendientes." : "No hay préstamos que coincidan con los filtros.",
            TipoListado.Vencidos => "No hay préstamos vencidos.", _ => "Todavía no hay devoluciones en el historial."
        };
        foreach (DataGridViewRow fila in _tabla.Rows)
            if ((fila.DataBoundItem as FilaPrestamo)?.Prestamo.IdPrestamo == seleccion)
            {
                _tabla.CurrentCell = fila.Cells[0];
                break;
            }
    }

    private void Abrir()
    {
        if (_tabla.CurrentRow?.DataBoundItem is not FilaPrestamo fila) return;
        using var detalle = new DetalleForm(fila.Prestamo, _service);
        detalle.ShowDialog(this);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _service.Cambios -= Cambios;
        base.Dispose(disposing);
    }

    private sealed class FilaPrestamo(Prestamo prestamo, DateOnly hoy)
    {
        public Prestamo Prestamo { get; } = prestamo;
        public string Titulo => Prestamo.TituloLibro;
        public string Alumno => Prestamo.NombreAlumno;
        public string Codigo => Prestamo.CodigoAlumno;
        public string FechaPrestamo => Ui.FechaTexto(Prestamo.FechaPrestamo);
        public string FechaLimite => Ui.FechaTexto(Prestamo.FechaLimite);
        public string FechaDevolucion => Prestamo.FechaDevolucion is { } f ? Ui.FechaTexto(f) : "";
        public int DiasRetraso => Prestamo.DiasRetraso(hoy);
        public string Estado => Ui.Estado(Prestamo, hoy);
    }
}
