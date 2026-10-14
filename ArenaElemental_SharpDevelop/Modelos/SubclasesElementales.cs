using System;

namespace ArenaElemental.Modelos
{
    public class CartaFuego : Personaje
    {
        public CartaFuego(int Id, string NombreCarta, int Vida, int Defensa, int Ataque, int ValorEmpatia, string Especial, string Descripcion)
            : base(Id, NombreCarta, Elemento.Fuego, Vida, Defensa, Ataque, ValorEmpatia, Especial, Descripcion)
        {
        }

        public override Personaje ClonarParaBatalla()
        {
            return new CartaFuego(IdPersonaje, Nombre, VidaMaxima, DefensaBase, AtaqueBase, Empatia, NombreEspecial, DescripcionEspecial);
        }

        public override int CalcularDanio(Personaje Objetivo, int BonoEscenario)
        {
            double Multiplicador = ObtenerMultiplicadorElemental(Objetivo.Faccion);
            double FactorCalculo = CalcularFactorNoLinealCalculo();
            double FactorFuria = (VidaActual < (VidaMaxima * 0.5)) ? 1.15 : 1.0;
            int DanioTotal = (int)Math.Round((AtaqueBase * Multiplicador * FactorCalculo * FactorFuria) + BonoEscenario);
            return Math.Max(0, DanioTotal);
        }

        public override int Atacar(Personaje Objetivo, int BonoEscenario)
        {
            if (EnergiaActual < CostoAtaque)
                throw new InvalidOperationException("Energia insuficiente para Atacar.");

            EnergiaActual -= CostoAtaque;
            int DanioBruto = CalcularDanio(Objetivo, BonoEscenario);
            return Objetivo.RecibirDanio(DanioBruto);
        }

        public override string UsarHabilidadEspecial(Personaje Objetivo)
        {
            if (!EspecialListo)
                throw new InvalidOperationException("La Habilidad Especial requiere 100% de carga y 2 de Energia.");

            EnergiaActual -= CostoEspecial;
            CargaEspecial = 0;

            double Multiplicador = ObtenerMultiplicadorElemental(Objetivo.Faccion);
            int DanioPerforante = (int)Math.Round(AtaqueBase * 1.45 * Multiplicador);
            Objetivo.VidaActual -= DanioPerforante;

            return string.Format("{0} desata [{1}]! Ignora el escudo y causa {2} de danio perforante a {3}.",
                Nombre, NombreEspecial, DanioPerforante, Objetivo.Nombre);
        }
    }

    public class CartaAgua : Personaje
    {
        public CartaAgua(int Id, string NombreCarta, int Vida, int Defensa, int Ataque, int ValorEmpatia, string Especial, string Descripcion)
            : base(Id, NombreCarta, Elemento.Agua, Vida, Defensa, Ataque, ValorEmpatia, Especial, Descripcion)
        {
        }

        public override Personaje ClonarParaBatalla()
        {
            return new CartaAgua(IdPersonaje, Nombre, VidaMaxima, DefensaBase, AtaqueBase, Empatia, NombreEspecial, DescripcionEspecial);
        }

        public override int CalcularDanio(Personaje Objetivo, int BonoEscenario)
        {
            double Multiplicador = ObtenerMultiplicadorElemental(Objetivo.Faccion);
            double FactorCalculo = CalcularFactorNoLinealCalculo();
            int DanioTotal = (int)Math.Round((AtaqueBase * Multiplicador * FactorCalculo) + BonoEscenario);
            return Math.Max(0, DanioTotal);
        }

        public override int Atacar(Personaje Objetivo, int BonoEscenario)
        {
            if (EnergiaActual < CostoAtaque)
                throw new InvalidOperationException("Energia insuficiente para Atacar.");

            EnergiaActual -= CostoAtaque;
            int DanioBruto = CalcularDanio(Objetivo, BonoEscenario);
            return Objetivo.RecibirDanio(DanioBruto);
        }

        public override string UsarHabilidadEspecial(Personaje Objetivo)
        {
            if (!EspecialListo)
                throw new InvalidOperationException("La Habilidad Especial requiere 100% de carga y 2 de Energia.");

            EnergiaActual -= CostoEspecial;
            CargaEspecial = 0;

            int EnergiaRobada = Objetivo.EnergiaActual > 0 ? 1 : 0;
            Objetivo.EnergiaActual -= EnergiaRobada;
            this.EnergiaActual += EnergiaRobada;

            int Curacion = (int)Math.Round(VidaMaxima * 0.25);
            this.VidaActual += Curacion;

            int DanioCausado = Objetivo.RecibirDanio(AtaqueBase);

            return string.Format("{0} invoca [{1}]! Roba {2} de energia, se cura +{3} HP y causa {4} de danio a {5}.",
                Nombre, NombreEspecial, EnergiaRobada, Curacion, DanioCausado, Objetivo.Nombre);
        }
    }

    public class CartaTierra : Personaje
    {
        public CartaTierra(int Id, string NombreCarta, int Vida, int Defensa, int Ataque, int ValorEmpatia, string Especial, string Descripcion)
            : base(Id, NombreCarta, Elemento.Tierra, Vida, Defensa, Ataque, ValorEmpatia, Especial, Descripcion)
        {
        }

        public override Personaje ClonarParaBatalla()
        {
            return new CartaTierra(IdPersonaje, Nombre, VidaMaxima, DefensaBase, AtaqueBase, Empatia, NombreEspecial, DescripcionEspecial);
        }

        public override int CalcularDanio(Personaje Objetivo, int BonoEscenario)
        {
            double Multiplicador = ObtenerMultiplicadorElemental(Objetivo.Faccion);
            double FactorCalculo = CalcularFactorNoLinealCalculo();
            int BonoRoca = (int)Math.Round(EscudoActual * 0.15);
            int DanioTotal = (int)Math.Round((AtaqueBase * Multiplicador * FactorCalculo) + BonoEscenario + BonoRoca);
            return Math.Max(0, DanioTotal);
        }

        public override int Atacar(Personaje Objetivo, int BonoEscenario)
        {
            if (EnergiaActual < CostoAtaque)
                throw new InvalidOperationException("Energia insuficiente para Atacar.");

            EnergiaActual -= CostoAtaque;
            int DanioBruto = CalcularDanio(Objetivo, BonoEscenario);
            return Objetivo.RecibirDanio(DanioBruto);
        }

        public override string UsarHabilidadEspecial(Personaje Objetivo)
        {
            if (!EspecialListo)
                throw new InvalidOperationException("La Habilidad Especial requiere 100% de carga y 2 de Energia.");

            EnergiaActual -= CostoEspecial;
            CargaEspecial = 0;

            this.EscudoActual += DefensaBase;
            int ImpactoTectonico = (int)Math.Round((AtaqueBase + DefensaBase) * ObtenerMultiplicadorElemental(Objetivo.Faccion));
            int DanioNeto = Objetivo.RecibirDanio(ImpactoTectonico);

            return string.Format("{0} activa [{1}]! Sube su escudo a {2} y aplasta a {3} causando {4} de danio neto.",
                Nombre, NombreEspecial, EscudoActual, Objetivo.Nombre, DanioNeto);
        }
    }

    public class CartaAire : Personaje
    {
        public CartaAire(int Id, string NombreCarta, int Vida, int Defensa, int Ataque, int ValorEmpatia, string Especial, string Descripcion)
            : base(Id, NombreCarta, Elemento.Aire, Vida, Defensa, Ataque, ValorEmpatia, Especial, Descripcion)
        {
        }

        public override Personaje ClonarParaBatalla()
        {
            return new CartaAire(IdPersonaje, Nombre, VidaMaxima, DefensaBase, AtaqueBase, Empatia, NombreEspecial, DescripcionEspecial);
        }

        public override int CalcularDanio(Personaje Objetivo, int BonoEscenario)
        {
            double Multiplicador = ObtenerMultiplicadorElemental(Objetivo.Faccion);
            double FactorCalculo = CalcularFactorNoLinealCalculo();
            int DanioTotal = (int)Math.Round((AtaqueBase * Multiplicador * FactorCalculo) + BonoEscenario);
            return Math.Max(0, DanioTotal);
        }

        public override int Atacar(Personaje Objetivo, int BonoEscenario)
        {
            if (EnergiaActual < CostoAtaque)
                throw new InvalidOperationException("Energia insuficiente para Atacar.");

            EnergiaActual -= CostoAtaque;
            int DanioBruto = CalcularDanio(Objetivo, BonoEscenario);
            return Objetivo.RecibirDanio(DanioBruto);
        }

        public override string UsarHabilidadEspecial(Personaje Objetivo)
        {
            if (!EspecialListo)
                throw new InvalidOperationException("La Habilidad Especial requiere 100% de carga y 2 de Energia.");

            EnergiaActual -= CostoEspecial;
            CargaEspecial = 0;

            int GolpeRafaga = (int)Math.Round(AtaqueBase * 0.85 * ObtenerMultiplicadorElemental(Objetivo.Faccion));
            int DanioGolpeUno = Objetivo.RecibirDanio(GolpeRafaga);
            int DanioGolpeDos = Objetivo.RecibirDanio(GolpeRafaga);

            return string.Format("{0} ejecuta [{1}]! Doble impacto aereo que inflige {2} HP en total a {3}.",
                Nombre, NombreEspecial, DanioGolpeUno + DanioGolpeDos, Objetivo.Nombre);
        }
    }
}
