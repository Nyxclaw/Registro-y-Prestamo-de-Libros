using PrestamoLibros.Core.Models;
using PrestamoLibros.Core.Services;

namespace PrestamoLibros.WinForms.Forms;

internal sealed class DetalleForm : Form
{
    private readonly Prestamo _prestamo;
    private readonly PrestamoService _service;
    private readonly DateTimePicker _fecha = Ui.Fecha();
    private readonly TextBox _receptor = new() { Width = 350 };

    internal DetalleForm(Prestamo prestamo, PrestamoService service)
    {
        _prestamo = prestamo;
        _service = service;
        Text = prestamo.Devuelto ? "Detalle de devolución" : "Registrar devolución";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(760, 780);
        MinimumSize = new Size(620, 480);
        Font = new Font("Segoe UI", 10);
        BackColor = Color.White;
        AutoScroll = true;
        Padding = new Padding(24);
        var contenido = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1 };
        contenido.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        contenido.Controls.Add(Ui.Titulo(prestamo.Devuelto ? "Detalle del historial" : "Entrega del libro"));
        contenido.Controls.Add(Ui.Texto("Los datos originales se muestran únicamente para consulta."));
        var detalle = Ui.Formulario();
        detalle.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        detalle.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        foreach (var (etiqueta, valor) in new[]
        {
            ("Título del libro", prestamo.TituloLibro), ("Nombre completo", prestamo.NombreAlumno),
            ("Licenciatura", prestamo.Licenciatura), ("Código de alumno", prestamo.CodigoAlumno),
            ("Semestre", prestamo.Semestre.ToString()), ("Correo electrónico", prestamo.CorreoElectronico),
            ("Fecha de préstamo", Ui.FechaTexto(prestamo.FechaPrestamo)),
            ("Fecha límite", Ui.FechaTexto(prestamo.FechaLimite)),
            ("Persona que entregó", prestamo.ResponsableEntrega), ("Estado", Ui.Estado(prestamo, service.Hoy))
        })
            Ui.Campo(detalle, etiqueta, new TextBox { Text = valor, ReadOnly = true, BorderStyle = BorderStyle.None, Width = 350 });
        if (prestamo.Devuelto)
        {
            Ui.Campo(detalle, "Fecha de devolución", new Label { Text = Ui.FechaTexto(prestamo.FechaDevolucion!.Value), AutoSize = true });
            Ui.Campo(detalle, "Persona que recibió", new TextBox { Text = prestamo.ResponsableRecepcion, ReadOnly = true, BorderStyle = BorderStyle.None });
        }
        else
        {
            Ui.Campo(detalle, "Fecha de devolución", _fecha);
            Ui.Campo(detalle, "Persona que recibe", _receptor);
        }
        contenido.Controls.Add(detalle);
        var acciones = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Top };
        if (!prestamo.Devuelto)
        {
            var devolver = Ui.Boton("Registrar devolución", true);
            devolver.Click += (_, _) => Devolver();
            acciones.Controls.Add(devolver);
        }
        var cerrar = Ui.Boton("Cerrar");
        cerrar.DialogResult = DialogResult.Cancel;
        CancelButton = cerrar;
        acciones.Controls.Add(cerrar);
        contenido.Controls.Add(acciones);
        Controls.Add(contenido);
    }

    private void Devolver()
    {
        var fecha = DateOnly.FromDateTime(_fecha.Value);
        try
        {
            ValidacionPrestamo.Comprobar(_prestamo with { FechaDevolucion = fecha, ResponsableRecepcion = _receptor.Text.Trim() }, _service.Hoy);
            if (MessageBox.Show(this,
                $"¿Deseas registrar la devolución de «{_prestamo.TituloLibro}»?\n\nAlumno: {_prestamo.NombreAlumno}\nCódigo: {_prestamo.CodigoAlumno}\nFecha: {Ui.FechaTexto(fecha)}\nRecibe: {_receptor.Text.Trim()}",
                "Confirmar devolución", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
            var aviso = _service.Devolver(_prestamo.IdPrestamo, fecha, _receptor.Text);
            MessageBox.Show(this, aviso ?? "Devolución registrada correctamente.", "Devolución", MessageBoxButtons.OK,
                aviso is null ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (ValidacionException ex)
        {
            MessageBox.Show(this, ex.Message, "Revisa la devolución", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex) when (JsonStorageService.EsErrorArchivo(ex) || ex is InvalidOperationException)
        {
            MessageBox.Show(this, "No se pudo registrar la devolución.\n\n" + ex.Message,
                "Error al guardar", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
