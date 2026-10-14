using System;
using System.Collections.Generic;

namespace ArenaElemental.Modelos
{
    public class PerfilJugador
    {
        public int IdJugador { get; set; }
        public string NombreUsuario { get; set; }
        public int PartidasJugadas { get; set; }
        public int Victorias { get; set; }
        public int Derrotas { get; set; }
        public decimal PorcentajeExito { get; set; }
        public List<Personaje> MazoSeleccionado { get; set; }

        public PerfilJugador(int Id, string Usuario, int Partidas, int Ganadas, int Perdidas)
        {
            IdJugador = Id;
            NombreUsuario = Usuario;
            PartidasJugadas = Partidas;
            Victorias = Ganadas;
            Derrotas = Perdidas;
            MazoSeleccionado = new List<Personaje>();
            RecalcularPorcentaje();
        }

        public void RegistrarResultado(bool EsVictoria)
        {
            PartidasJugadas += 1;
            if (EsVictoria)
                Victorias += 1;
            else
                Derrotas += 1;
            RecalcularPorcentaje();
        }

        public void RecalcularPorcentaje()
        {
            if (PartidasJugadas > 0)
                PorcentajeExito = Math.Round(((decimal)Victorias / (decimal)PartidasJugadas) * 100m, 2);
            else
                PorcentajeExito = 0.00m;
        }
    }
}
