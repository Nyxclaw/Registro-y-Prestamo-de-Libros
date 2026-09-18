using PrestamoLibros.Core.Models;
using PrestamoLibros.Core.Services;

namespace PrestamoLibros.WinForms.Forms;

internal sealed class PrestamoForm : UserControl
{
    private readonly PrestamoService _service;
    private readonly Dictionary<string, Control> _campos = new();
    private readonly ErrorProvider _errores = new();
    private readonly DateTimePicker _fecha = Ui.Fecha();
    private readonly TextBox _semestre = new() { Text = "1" };
    private readonly Label _limite = new() { AutoSize = true };

    public PrestamoForm(PrestamoService service)
    {
        _service = service;
        Dock = DockStyle.Fill;
        AutoScroll = true;
        Padding = new Padding(24);
        _errores.ContainerControl = this;
        _errores.BlinkStyle = ErrorBlinkStyle.NeverBlink;
        var contenido = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1 };
        contenido.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        contenido.Controls.Add(Ui.Titulo("Registrar préstamo"));
        contenido.Controls.Add(Ui.Texto("Todos los campos son obligatorios. El plazo de devolución es de 21 días."));
        var formulario = Ui.Formulario();
        formulario.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        formulario.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        AgregarTexto(formulario, nameof(Prestamo.NombreAlumno), "Nombre completo");
        AgregarTexto(formulario, nameof(Prestamo.Licenciatura), "Licenciatura");
        AgregarTexto(formulario, nameof(Prestamo.CodigoAlumno), "Código de alumno");
        Agregar(formulario, nameof(Prestamo.Semestre), "Semestre", _semestre);
        AgregarTexto(formulario, nameof(Prestamo.CorreoElectronico), "Correo electrónico");
        AgregarTexto(formulario, nameof(Prestamo.TituloLibro), "Título del libro");
        Agregar(formulario, nameof(Prestamo.FechaPrestamo), "Fecha de préstamo", _fecha);
        Ui.Campo(formulario, "Fecha límite (automática)", _limite);
        AgregarTexto(formulario, nameof(Prestamo.ResponsableEntrega), "Persona que entrega");
        _fecha.ValueChanged += (_, _) => ActualizarLimite();
        ActualizarLimite();
        contenido.Controls.Add(formulario);
        var registrar = Ui.Boton("Registrar préstamo", true);
        registrar.Click += (_, _) => Registrar();
        contenido.Controls.Add(registrar);
        Controls.Add(contenido);
    }

    private void AgregarTexto(TableLayoutPanel panel, string campo, string etiqueta) =>
        Agregar(panel, campo, etiqueta, new TextBox { Width = 400 });

    private void Agregar(TableLayoutPanel panel, string campo, string etiqueta, Control control)
    {
        _campos.Add(campo, control);
        Ui.Campo(panel, etiqueta, control);
    }

    private void ActualizarLimite() => _limite.Text = Ui.FechaTexto(DateOnly.FromDateTime(_fecha.Value).AddDays(21));
    private string Texto(string campo) => _campos[campo].Text;

    private void Registrar()
    {
        _errores.Clear();
        // No redondear ni corregir silenciosamente un semestre decimal o inválido.
        if (!int.TryParse(_semestre.Text, out var semestre))
        {
            _errores.SetError(_semestre, "Escribe un número entero de semestre.");
            _semestre.Focus();
            return;
        }
        var p = new Prestamo
        {
            NombreAlumno = Texto(nameof(Prestamo.NombreAlumno)), Licenciatura = Texto(nameof(Prestamo.Licenciatura)),
            CodigoAlumno = Texto(nameof(Prestamo.CodigoAlumno)), Semestre = semestre,
            CorreoElectronico = Texto(nameof(Prestamo.CorreoElectronico)), TituloLibro = Texto(nameof(Prestamo.TituloLibro)),
            FechaPrestamo = DateOnly.FromDateTime(_fecha.Value), ResponsableEntrega = Texto(nameof(Prestamo.ResponsableEntrega))
        };
        try
        {
            _service.Registrar(p);
            foreach (var campo in _campos.Values.OfType<TextBox>()) campo.Clear();
            _semestre.Text = "1";
            _fecha.Value = DateTime.Today;
            MessageBox.Show(this, "Préstamo registrado correctamente.", "Préstamo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _campos[nameof(Prestamo.NombreAlumno)].Focus();
        }
        catch (ValidacionException ex)
        {
            foreach (var error in ex.Errores)
                if (_campos.TryGetValue(error.Key, out var control)) _errores.SetError(control, error.Value);
            MessageBox.Show(this, ex.Message, "Revisa los datos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            if (_campos.TryGetValue(ex.Errores.First().Key, out var primero)) primero.Focus();
        }
        catch (Exception ex) when (JsonStorageService.EsErrorArchivo(ex))
        {
            MessageBox.Show(this, "No se pudo guardar el préstamo. Los datos del formulario se conservaron.\n\n" + ex.Message,
                "Error al guardar", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _errores.Dispose();
        base.Dispose(disposing);
    }
}
