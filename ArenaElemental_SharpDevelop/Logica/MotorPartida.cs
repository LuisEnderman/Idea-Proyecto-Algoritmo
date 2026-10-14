using System;
using System.Collections.Generic;
using ArenaElemental.Modelos;

namespace ArenaElemental.Logica
{
    public class MotorPartida
    {
        public PerfilJugador JugadorUno { get; private set; }
        public PerfilJugador JugadorDos { get; private set; }

        public Personaje ActivoJugadorUno { get; private set; }
        public Personaje ActivoJugadorDos { get; private set; }

        // Estructuras de Datos Dinamicas: Colas (Queue<Personaje>) y Pila (Stack<string>)
        public Queue<Personaje> ColaRelevosJugadorUno { get; private set; }
        public Queue<Personaje> ColaRelevosJugadorDos { get; private set; }
        public Stack<string> PilaHistorialEventos { get; private set; }

        public List<Escenario> ListaEscenarios { get; private set; }
        public Escenario EscenarioActual { get; private set; }
        public bool EscenariosAlternantes { get; private set; }

        public int TurnoActualGlobal { get; private set; }
        public int NumeroJugadorEnTurno { get; private set; } // 1 o 2
        public bool PartidaFinalizada { get; private set; }
        public PerfilJugador GanadorPartida { get; private set; }
        public bool ContraCpu { get; private set; }

        private Random GeneradorAleatorio;

        public MotorPartida(
            PerfilJugador PerfilUno,
            PerfilJugador PerfilDos,
            List<Escenario> EscenariosDisponibles,
            Escenario EscenarioInicial,
            bool AlternarEscenarios,
            bool ModoContraCpu)
        {
            JugadorUno = PerfilUno;
            JugadorDos = PerfilDos;
            ListaEscenarios = EscenariosDisponibles;
            EscenarioActual = EscenarioInicial;
            EscenariosAlternantes = AlternarEscenarios;
            ContraCpu = ModoContraCpu;

            ColaRelevosJugadorUno = new Queue<Personaje>();
            ColaRelevosJugadorDos = new Queue<Personaje>();
            PilaHistorialEventos = new Stack<string>();
            GeneradorAleatorio = new Random();

            InicializarEquiposEnColas();
            IniciarCombate();
        }

        private void InicializarEquiposEnColas()
        {
            // El primer personaje (indice 0) entra como Activo; los otros 4 entran a la Cola de Relevos (FIFO)
            for (int I = 0; I < JugadorUno.MazoSeleccionado.Count; I++)
            {
                Personaje Clon = JugadorUno.MazoSeleccionado[I].ClonarParaBatalla();
                if (I == 0)
                    ActivoJugadorUno = Clon;
                else
                    ColaRelevosJugadorUno.Enqueue(Clon);
            }

            for (int I = 0; I < JugadorDos.MazoSeleccionado.Count; I++)
            {
                Personaje Clon = JugadorDos.MazoSeleccionado[I].ClonarParaBatalla();
                if (I == 0)
                    ActivoJugadorDos = Clon;
                else
                    ColaRelevosJugadorDos.Enqueue(Clon);
            }
        }

        private void IniciarCombate()
        {
            TurnoActualGlobal = 1;
            PartidaFinalizada = false;

            // El jugador inicial se decide mediante generador aleatorio (Enunciado Seccion 5)
            NumeroJugadorEnTurno = GeneradorAleatorio.Next(1, 3);

            // Regla de Inicio: Reserva inicial de 2 energias y habilidad especial en 0%
            ActivoJugadorUno.EnergiaActual = 2;
            ActivoJugadorUno.CargaEspecial = 0;
            ActivoJugadorDos.EnergiaActual = 2;
            ActivoJugadorDos.CargaEspecial = 0;

            RegistrarEnPila(string.Format("[INICIO] Combate en [{0}]. Jugador inicial por sorteo: {1}.",
                EscenarioActual.Nombre, ObtenerPerfilEnTurno().NombreUsuario));
        }

        public void RegistrarEnPila(string DescripcionEvento)
        {
            string LineaConTurno = string.Format("[T{0:00}] {1}", TurnoActualGlobal, DescripcionEvento);
            PilaHistorialEventos.Push(LineaConTurno);
        }

        public string ConsultarUltimoEventoPeek()
        {
            if (PilaHistorialEventos.Count > 0)
                return PilaHistorialEventos.Peek();
            return "Sin eventos registrados.";
        }

        public PerfilJugador ObtenerPerfilEnTurno()
        {
            return NumeroJugadorEnTurno == 1 ? JugadorUno : JugadorDos;
        }

        public Personaje ObtenerPersonajeActivoEnTurno()
        {
            return NumeroJugadorEnTurno == 1 ? ActivoJugadorUno : ActivoJugadorDos;
        }

        public Personaje ObtenerPersonajeRival()
        {
            return NumeroJugadorEnTurno == 1 ? ActivoJugadorDos : ActivoJugadorUno;
        }

        public Queue<Personaje> ObtenerColaEnTurno()
        {
            return NumeroJugadorEnTurno == 1 ? ColaRelevosJugadorUno : ColaRelevosJugadorDos;
        }

        public string EjecutarAtaqueEnTurno()
        {
            if (PartidaFinalizada) return "La partida ya finalizo.";

            Personaje Atacante = ObtenerPersonajeActivoEnTurno();
            Personaje Defensor = ObtenerPersonajeRival();

            int BonoEscenario = EscenarioActual.ObtenerModificadorEscenario(Atacante);
            double MultElemental = Atacante.ObtenerMultiplicadorElemental(Defensor.Faccion);
            int EscudoAntes = Defensor.EscudoActual;

            int DanioNetoVida = Atacante.Atacar(Defensor, BonoEscenario);
            int EscudoAbsorbido = EscudoAntes - Defensor.EscudoActual;

            string Mensaje = string.Format("{0} ataca a {1} (Mult x{2:0.00}, Escenario {3:+#;-#;0}): Escudo absorbe {4}, Vida recibe -{5} HP.",
                Atacante.Nombre, Defensor.Nombre, MultElemental, BonoEscenario, EscudoAbsorbido, DanioNetoVida);

            RegistrarEnPila(Mensaje);
            VerificarDebilitamientoYVictoria();
            return Mensaje;
        }

        public string EjecutarDefensaEnTurno()
        {
            if (PartidaFinalizada) return "La partida ya finalizo.";

            Personaje Activo = ObtenerPersonajeActivoEnTurno();
            int EscudoGanado = Activo.Defender();

            string Mensaje = string.Format("{0} refuerza su postura defensiva (+{1} Escudo -> Total: {2}).",
                Activo.Nombre, EscudoGanado, Activo.EscudoActual);

            RegistrarEnPila(Mensaje);
            return Mensaje;
        }

        public string EjecutarHabilidadEspecialEnTurno()
        {
            if (PartidaFinalizada) return "La partida ya finalizo.";

            if (TurnoActualGlobal == 1)
                throw new InvalidOperationException("La Habilidad Especial esta deshabilitada en el Turno 1.");

            Personaje Atacante = ObtenerPersonajeActivoEnTurno();
            Personaje Defensor = ObtenerPersonajeRival();

            string Resultado = Atacante.UsarHabilidadEspecial(Defensor);
            RegistrarEnPila(Resultado);
            VerificarDebilitamientoYVictoria();
            return Resultado;
        }

        public string EjecutarRelevoTacticoEnTurno()
        {
            if (PartidaFinalizada) return "La partida ya finalizo.";

            Queue<Personaje> ColaActual = ObtenerColaEnTurno();
            Personaje Saliente = ObtenerPersonajeActivoEnTurno();

            if (ColaActual.Count == 0)
                throw new InvalidOperationException("No quedan personajes en la banca de relevos (Cola vacia).");

            if (Saliente.EnergiaActual < Personaje.CostoRelevo)
                throw new InvalidOperationException("Se requiere al menos 1 punto de Energia para relevar.");

            Saliente.EnergiaActual -= Personaje.CostoRelevo;

            // Operacion explicita en Cola (Queue): Enqueue al saliente vivo y Dequeue al entrante
            ColaActual.Enqueue(Saliente);
            Personaje Entrante = ColaActual.Dequeue();

            if (NumeroJugadorEnTurno == 1)
                ActivoJugadorUno = Entrante;
            else
                ActivoJugadorDos = Entrante;

            string Mensaje = string.Format("RELEVO: {0} regresa a la banca (Enqueue) e ingresa {1} al combate (Dequeue).",
                Saliente.Nombre, Entrante.Nombre);

            RegistrarEnPila(Mensaje);
            return Mensaje;
        }

        private void VerificarDebilitamientoYVictoria()
        {
            if (!ActivoJugadorUno.EstaVivo)
            {
                RegistrarEnPila(string.Format("[DEBILITADO] {0} (J1) cayo a 0 HP y sale definitivamente del combate!", ActivoJugadorUno.Nombre));
                if (ColaRelevosJugadorUno.Count > 0)
                {
                    ActivoJugadorUno = ColaRelevosJugadorUno.Dequeue();
                    RegistrarEnPila(string.Format("[RELEVO FORZADO J1] Ingresa {0} desde la Cola de Relevos.", ActivoJugadorUno.Nombre));
                }
                else
                {
                    PartidaFinalizada = true;
                    GanadorPartida = JugadorDos;
                    RegistrarEnPila(string.Format("[FIN DE PARTIDA] El equipo de {0} fue eliminado! Vencedor: {1}!",
                        JugadorUno.NombreUsuario, JugadorDos.NombreUsuario));
                    return;
                }
            }

            if (!ActivoJugadorDos.EstaVivo)
            {
                RegistrarEnPila(string.Format("[DEBILITADO] {0} (J2) cayo a 0 HP y sale definitivamente del combate!", ActivoJugadorDos.Nombre));
                if (ColaRelevosJugadorDos.Count > 0)
                {
                    ActivoJugadorDos = ColaRelevosJugadorDos.Dequeue();
                    RegistrarEnPila(string.Format("[RELEVO FORZADO J2] Ingresa {0} desde la Cola de Relevos.", ActivoJugadorDos.Nombre));
                }
                else
                {
                    PartidaFinalizada = true;
                    GanadorPartida = JugadorUno;
                    RegistrarEnPila(string.Format("[FIN DE PARTIDA] El equipo de {0} fue eliminado! Vencedor: {1}!",
                        JugadorDos.NombreUsuario, JugadorUno.NombreUsuario));
                    return;
                }
            }
        }

        public void FinalizarTurnoYAvanzar(bool FuePorTiempoAgotado)
        {
            if (PartidaFinalizada) return;

            if (FuePorTiempoAgotado)
            {
                RegistrarEnPila(string.Format("[TIEMPO AGOTADO] {0} perdio el resto de su turno por limite de reloj.",
                    ObtenerPerfilEnTurno().NombreUsuario));
            }

            // Evento Algoritmico Terremoto: ocurre cada 3 turnos globales
            if (TurnoActualGlobal % 3 == 0)
            {
                string EfectoUno = EscenarioActual.AplicarTerremoto(ActivoJugadorUno, TurnoActualGlobal);
                RegistrarEnPila(EfectoUno);
                VerificarDebilitamientoYVictoria();
                if (PartidaFinalizada) return;

                string EfectoDos = EscenarioActual.AplicarTerremoto(ActivoJugadorDos, TurnoActualGlobal);
                RegistrarEnPila(EfectoDos);
                VerificarDebilitamientoYVictoria();
                if (PartidaFinalizada) return;
            }

            // Alternancia de Escenario si esta configurada
            if (EscenariosAlternantes && ListaEscenarios.Count > 1 && TurnoActualGlobal % 2 == 0)
            {
                int IndiceActual = ListaEscenarios.IndexOf(EscenarioActual);
                int NuevoIndice = (IndiceActual + 1) % ListaEscenarios.Count;
                EscenarioActual = ListaEscenarios[NuevoIndice];
                RegistrarEnPila(string.Format("[CAMBIO DE ESCENARIO] El entorno cambia a: {0}.", EscenarioActual.Nombre));
            }

            // Cambiar jugador activo y avanzar contador de turnos
            TurnoActualGlobal += 1;
            NumeroJugadorEnTurno = (NumeroJugadorEnTurno == 1) ? 2 : 1;

            // Recargar +1 Energia y +50% barra de Habilidad Especial al personaje que inicia turno
            Personaje ActivoNuevoTurno = ObtenerPersonajeActivoEnTurno();
            ActivoNuevoTurno.RecargarInicioTurno(TurnoActualGlobal);
        }

        public void EjecutarTurnoInteligenciaCpu()
        {
            if (PartidaFinalizada || NumeroJugadorEnTurno != 2) return;

            try
            {
                Personaje Cpu = ActivoJugadorDos;
                if (Cpu.EspecialListo && TurnoActualGlobal > 1)
                {
                    EjecutarHabilidadEspecialEnTurno();
                }

                if (!PartidaFinalizada && Cpu.EnergiaActual >= Personaje.CostoAtaque)
                {
                    EjecutarAtaqueEnTurno();
                }

                if (!PartidaFinalizada && Cpu.EnergiaActual >= Personaje.CostoDefensa && Cpu.VidaActual < (Cpu.VidaMaxima / 2))
                {
                    EjecutarDefensaEnTurno();
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
