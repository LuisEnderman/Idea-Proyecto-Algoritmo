using System;
using System.Windows.Forms;
using ArenaElemental.Vistas;

namespace ArenaElemental
{
    internal sealed class Program
    {
        [STAThread]
        private static void Main(string[] Argumentos)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMenuPreJuego());
        }
    }
}
