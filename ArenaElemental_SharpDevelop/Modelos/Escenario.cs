using System;

namespace ArenaElemental.Modelos
{
    public class Escenario
    {
        public int IdEscenario { get; set; }
        public string Nombre { get; set; }
        public Elemento FaccionDominante { get; set; }
        public Elemento FaccionPenalizada { get; set; }
        public int BonoAtaque { get; set; }
        public int PenalizacionAtaque { get; set; }
        public int UmbralEmpatia { get; set; }
        public string Descripcion { get; set; }

        public Escenario(int Id, string NombreEscenario, Elemento Dominante, Elemento Penalizada, int Bono, int Penalizacion, int Umbral, string TextoDescripcion)
        {
            IdEscenario = Id;
            Nombre = NombreEscenario;
            FaccionDominante = Dominante;
            FaccionPenalizada = Penalizada;
            BonoAtaque = Bono;
            PenalizacionAtaque = Penalizacion;
            UmbralEmpatia = Umbral;
            Descripcion = TextoDescripcion;
        }

        public int ObtenerModificadorEscenario(Personaje Combatiente)
        {
            if (Combatiente.Faccion == FaccionDominante)
                return BonoAtaque;
            if (Combatiente.Faccion == FaccionPenalizada)
                return -PenalizacionAtaque;
            return 0;
        }

        // Evento Algoritmico Terremoto con Mecanica Oculta de Empatia
        public string AplicarTerremoto(Personaje Afectado, int TurnoActual)
        {
            // Funcion no lineal de magnitud sismica (Calculo): oscila entre 12% y 20% de la VidaMaxima
            double FactorSismico = 0.14 + (0.04 * Math.Abs(Math.Sin(TurnoActual)));
            int MagnitudSismica = (int)Math.Round(Afectado.VidaMaxima * FactorSismico);

            if (Afectado.Empatia >= UmbralEmpatia)
            {
                Afectado.VidaActual += MagnitudSismica;
                Afectado.EscudoActual += 6;
                return string.Format("[TERREMOTO INVERTIDO - Empatia {0}>={1}] {2} armoniza con {3}: +{4} HP y +6 Escudo!",
                    Afectado.Empatia, UmbralEmpatia, Afectado.Nombre, Nombre, MagnitudSismica);
            }
            else
            {
                if (Afectado.Faccion == Elemento.Tierra)
                {
                    MagnitudSismica = MagnitudSismica / 2;
                }
                Afectado.EscudoActual = 0;
                Afectado.VidaActual -= MagnitudSismica;
                return string.Format("[TERREMOTO CATASTROFICO] {0} pierde su escudo y sufre -{1} HP por el sismo!",
                    Afectado.Nombre, MagnitudSismica);
            }
        }

        public override string ToString()
        {
            return string.Format("{0} (+{1} {2} / -{3} {4})",
                Nombre, BonoAtaque, FaccionDominante, PenalizacionAtaque, FaccionPenalizada);
        }
    }
}
