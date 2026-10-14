using System;

namespace ArenaElemental.Modelos
{
    public abstract class Personaje
    {
        // Atributos encapsulados (PascalCase con prefijo Atributo para evitar colision con Propiedades)
        private int AtributoIdPersonaje;
        private string AtributoNombre;
        private Elemento AtributoFaccion;
        private int AtributoVidaMaxima;
        private int AtributoVidaActual;
        private int AtributoDefensaBase;
        private int AtributoEscudoActual;
        private int AtributoAtaqueBase;
        private int AtributoEnergiaActual;
        private int AtributoCargaEspecial;
        private int AtributoEmpatia;
        private string AtributoNombreEspecial;
        private string AtributoDescripcionEspecial;

        public const int EnergiaMaxima = 10;
        public const int CostoAtaque = 1;
        public const int CostoDefensa = 1;
        public const int CostoEspecial = 2;
        public const int CostoRelevo = 1;

        public int IdPersonaje
        {
            get { return AtributoIdPersonaje; }
            set { AtributoIdPersonaje = value > 0 ? value : 1; }
        }

        public string Nombre
        {
            get { return AtributoNombre; }
            set { AtributoNombre = !string.IsNullOrEmpty(value) ? value : "Guerrero Elemental"; }
        }

        public Elemento Faccion
        {
            get { return AtributoFaccion; }
            set { AtributoFaccion = value; }
        }

        public int VidaMaxima
        {
            get { return AtributoVidaMaxima; }
            protected set { AtributoVidaMaxima = value > 0 ? value : 1; }
        }

        public int VidaActual
        {
            get { return AtributoVidaActual; }
            set
            {
                if (value < 0)
                    AtributoVidaActual = 0;
                else if (value > AtributoVidaMaxima)
                    AtributoVidaActual = AtributoVidaMaxima;
                else
                    AtributoVidaActual = value;
            }
        }

        public int DefensaBase
        {
            get { return AtributoDefensaBase; }
            protected set { AtributoDefensaBase = value >= 0 ? value : 0; }
        }

        public int EscudoActual
        {
            get { return AtributoEscudoActual; }
            set
            {
                int TopeEscudo = AtributoDefensaBase * 2;
                if (value < 0)
                    AtributoEscudoActual = 0;
                else if (value > TopeEscudo)
                    AtributoEscudoActual = TopeEscudo;
                else
                    AtributoEscudoActual = value;
            }
        }

        public int AtaqueBase
        {
            get { return AtributoAtaqueBase; }
            protected set { AtributoAtaqueBase = value > 0 ? value : 1; }
        }

        public int EnergiaActual
        {
            get { return AtributoEnergiaActual; }
            set
            {
                if (value < 0)
                    AtributoEnergiaActual = 0;
                else if (value > EnergiaMaxima)
                    AtributoEnergiaActual = EnergiaMaxima;
                else
                    AtributoEnergiaActual = value;
            }
        }

        public int CargaEspecial
        {
            get { return AtributoCargaEspecial; }
            set
            {
                if (value < 0)
                    AtributoCargaEspecial = 0;
                else if (value > 100)
                    AtributoCargaEspecial = 100;
                else
                    AtributoCargaEspecial = value;
            }
        }

        public int Empatia
        {
            get { return AtributoEmpatia; }
            set
            {
                if (value < 0)
                    AtributoEmpatia = 0;
                else if (value > 100)
                    AtributoEmpatia = 100;
                else
                    AtributoEmpatia = value;
            }
        }

        public string NombreEspecial
        {
            get { return AtributoNombreEspecial; }
            set { AtributoNombreEspecial = value; }
        }

        public string DescripcionEspecial
        {
            get { return AtributoDescripcionEspecial; }
            set { AtributoDescripcionEspecial = value; }
        }

        public bool EstaVivo
        {
            get { return AtributoVidaActual > 0; }
        }

        public bool EspecialListo
        {
            get { return AtributoCargaEspecial >= 100 && AtributoEnergiaActual >= CostoEspecial; }
        }

        protected Personaje(int Id, string NombreCarta, Elemento FaccionCarta, int Vida, int Defensa, int Ataque, int ValorEmpatia, string Especial, string Descripcion)
        {
            ValidarBalanceCien(Vida, Defensa, Ataque);

            IdPersonaje = Id;
            Nombre = NombreCarta;
            Faccion = FaccionCarta;
            VidaMaxima = Vida;
            VidaActual = Vida;
            DefensaBase = Defensa;
            EscudoActual = Defensa;
            AtaqueBase = Ataque;
            Empatia = ValorEmpatia;
            NombreEspecial = Especial;
            DescripcionEspecial = Descripcion;

            // Regla del Enunciado: Reserva inicial de 2 energias y habilidad especial descargada (0%)
            EnergiaActual = 2;
            CargaEspecial = 0;
        }

        private void ValidarBalanceCien(int Vida, int Defensa, int Ataque)
        {
            int SumaTotal = Vida + Defensa + Ataque;
            if (SumaTotal != 100)
            {
                throw new ArgumentException(string.Format(
                    "Error de balance en {0}: HP({1}) + DEF({2}) + ATK({3}) = {4}. Debe sumar exactamente 100.",
                    Nombre, Vida, Defensa, Ataque, SumaTotal));
            }
        }

        // Triangulo Elemental: Agua > Fuego > Aire > Tierra > Agua
        public double ObtenerMultiplicadorElemental(Elemento ElementoRival)
        {
            if (this.Faccion == ElementoRival)
                return 1.0;

            bool TieneVentaja =
                (this.Faccion == Elemento.Agua && ElementoRival == Elemento.Fuego) ||
                (this.Faccion == Elemento.Fuego && ElementoRival == Elemento.Aire) ||
                (this.Faccion == Elemento.Aire && ElementoRival == Elemento.Tierra) ||
                (this.Faccion == Elemento.Tierra && ElementoRival == Elemento.Agua);

            bool TieneDesventaja =
                (this.Faccion == Elemento.Fuego && ElementoRival == Elemento.Agua) ||
                (this.Faccion == Elemento.Aire && ElementoRival == Elemento.Fuego) ||
                (this.Faccion == Elemento.Tierra && ElementoRival == Elemento.Aire) ||
                (this.Faccion == Elemento.Agua && ElementoRival == Elemento.Tierra);

            if (TieneVentaja) return 1.5;
            if (TieneDesventaja) return 0.75;
            return 1.0;
        }

        // Funcion No Lineal de Calculo: Factor de Rendimiento Energetico Logaritmico
        // F(E) = 1 + 0.08 * ln(1 + EnergiaActual)
        public double CalcularFactorNoLinealCalculo()
        {
            return 1.0 + (0.08 * Math.Log(1.0 + this.EnergiaActual));
        }

        public virtual int RecibirDanio(int DanioBruto)
        {
            if (DanioBruto <= 0)
                return 0;

            int DanioRestante = DanioBruto;

            if (EscudoActual > 0)
            {
                if (EscudoActual >= DanioRestante)
                {
                    EscudoActual -= DanioRestante;
                    return 0;
                }
                else
                {
                    DanioRestante -= EscudoActual;
                    EscudoActual = 0;
                }
            }

            VidaActual -= DanioRestante;
            return DanioRestante;
        }

        public virtual int Defender()
        {
            if (EnergiaActual < CostoDefensa)
                throw new InvalidOperationException("No tienes suficiente energia para Defender.");

            EnergiaActual -= CostoDefensa;
            int EscudoGanado = (int)Math.Round(DefensaBase * 0.45);
            EscudoActual += EscudoGanado;
            return EscudoGanado;
        }

        public void RecargarInicioTurno(int TurnoActualGlobal)
        {
            EnergiaActual += 1;
            // La habilidad especial no puede estar lista en el Turno 1 y tarda 2 turnos en cargarse (50% por turno)
            if (TurnoActualGlobal > 1)
            {
                CargaEspecial += 50;
            }
        }

        public abstract Personaje ClonarParaBatalla();
        public abstract int CalcularDanio(Personaje Objetivo, int BonoEscenario);
        public abstract int Atacar(Personaje Objetivo, int BonoEscenario);
        public abstract string UsarHabilidadEspecial(Personaje Objetivo);

        public override string ToString()
        {
            return string.Format("{0} [{1}] (HP:{2}/{3} | ESC:{4} | ATK:{5})",
                Nombre, Faccion, VidaActual, VidaMaxima, EscudoActual, AtaqueBase);
        }
    }
}
