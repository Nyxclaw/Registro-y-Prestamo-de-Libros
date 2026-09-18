using PrestamoLibros.Core.Services;
using PrestamoLibros.WinForms.Forms;

namespace PrestamoLibros.WinForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        // Una sola instancia evita que dos ventanas sobrescriban una devolución.
        using var instancia = new Mutex(true, @"Local\PrestamoLibros", out var primeraInstancia);
        if (!primeraInstancia)
        {
            MessageBox.Show("La aplicación ya está abierta. Utiliza la ventana existente.", "Préstamo de libros");
            return;
        }
        try { Application.Run(new MainForm(new PrestamoService(new JsonStorageService()))); }
        finally { instancia.ReleaseMutex(); }
    }
}
