using PrestamoLibros.Core.Models;

namespace PrestamoLibros.WinForms.Forms;

internal static class Ui
{
    internal static readonly Color Tinta = Color.FromArgb(30, 49, 65);
    internal static readonly Color Principal = Color.FromArgb(31, 97, 105);

    internal static Label Titulo(string texto) => new()
    {
        Text = texto, AutoSize = true, Font = new Font("Segoe UI", 19, FontStyle.Bold),
        ForeColor = Tinta, Margin = new Padding(0, 0, 0, 10)
    };

    internal static Label Texto(string texto) => new()
    {
        Text = texto, AutoSize = true, MaximumSize = new Size(760, 0),
        Margin = new Padding(0, 0, 0, 12)
    };

    internal static Button Boton(string texto, bool principal = false) => new()
    {
        Text = texto, AutoSize = true, MinimumSize = new Size(150, 40),
        Padding = new Padding(10, 4, 10, 4), Margin = new Padding(0, 8, 12, 8),
        FlatStyle = FlatStyle.Flat, BackColor = principal ? Principal : Color.White,
        ForeColor = principal ? Color.White : Tinta, Cursor = Cursors.Hand
    };

    internal static TableLayoutPanel Formulario() => new()
    {
        AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
        ColumnCount = 2, Dock = DockStyle.Top, Padding = new Padding(0, 8, 28, 8)
    };

    internal static void Campo(TableLayoutPanel panel, string etiqueta, Control entrada)
    {
        var fila = panel.RowCount++;
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.Controls.Add(new Label
        {
            Text = etiqueta, AutoSize = true, Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 10, 20, 10)
        }, 0, fila);
        entrada.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        entrada.Margin = new Padding(0, 6, 12, 6);
        entrada.MinimumSize = new Size(220, 28);
        panel.Controls.Add(entrada, 1, fila);
    }

    internal static DateTimePicker Fecha() => new()
    {
        Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy",
        Value = DateTime.Today, Width = 260
    };

    internal static string Estado(Prestamo p, DateOnly hoy) => p.EstadoEn(hoy) switch
    {
        EstadoPrestamo.Activo => "Activo",
        EstadoPrestamo.Vencido => "Vencido",
        EstadoPrestamo.DevueltoATiempo => "Devuelto a tiempo",
        _ => "Devuelto con retraso"
    };

    internal static string FechaTexto(DateOnly fecha) => fecha.ToString("dd/MM/yyyy");
}
