using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ArenaElemental.Datos;
using ArenaElemental.Logica;
using ArenaElemental.Modelos;
using ArenaElemental.Red;

namespace ArenaElemental.Vistas
{
    public class PanelDobleBuffer : Panel
    {
        public PanelDobleBuffer()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
        }
    }

    public class FormTableroBatalla : Form
    {
        private MotorPartida Motor;
        private GestorBaseDatos GestorDatos;
        private GestorRedSockets GestorRed;
        private bool ModoRedActivo;

        // Timers de Reloj de Turno y Motor de Animacion GBA (60 FPS)
        private System.Windows.Forms.Timer RelojTurno;
        private System.Windows.Forms.Timer TimerAnimacionGba;
        private int SegundosRestantesTurno;
        private const int TiempoLimitePorTurno = 20;

        // Variables de Animacion GBA y Maquina de Estados Visual
        private PanelDobleBuffer PnlLienzoGba;
        private int FrameGlobal;
        private int OffsetAtaqueXJugadorUno;
        private int OffsetAtaqueXJugadorDos;
        private int OffsetEntradaRelevoUno;
        private int OffsetEntradaRelevoDos;
        private int SacudidaPantallaX;
        private int SacudidaPantallaY;
        private int TicksParpadeoDanioUno;
        private int TicksParpadeoDanioDos;
        private int TicksEfectoEspecial;
        private int TicksSacudidaTerremoto;

        // Valores interpolados suaves para las barras de vida GBA
        private float VidaVisualJ1;
        private float VidaVisualJ2;

        // Efecto Typewriter (Texto letra por letra estilo GBA)
        private string TextoNarradorCompleto;
        private string TextoNarradorVisible;
        private int IndiceLetraNarrador;

        // Controles Superiores e Inferiores
        private Label LblBannerEscenario;
        private Label LblCronometro;
        private ProgressBar PbTiempoTurno;

        // Controles de Colas y Pilas (Requerimiento Academico Explicito)
        private ListBox LstColaRelevosJ1;
        private ListBox LstColaRelevosJ2;
        private ListBox LstHistorialPila;
        private Label LblUltimoEventoPeek;

        // Botones de Comando Estilo Menu GBA (4 cuadrantes + Pasar)
        private Button BtnAccionAtacar;
        private Button BtnAccionDefender;
        private Button BtnAccionEspecial;
        private Button BtnAccionRelevar;
        private Button BtnPasarTerminarTurno;

        public FormTableroBatalla(MotorPartida MotorJuego, GestorBaseDatos BaseDatos, GestorRedSockets RedSockets, bool EnRed)
        {
            Motor = MotorJuego;
            GestorDatos = BaseDatos;
            GestorRed = RedSockets;
            ModoRedActivo = EnRed;
            SegundosRestantesTurno = TiempoLimitePorTurno;

            VidaVisualJ1 = Motor.ActivoJugadorUno.VidaActual;
            VidaVisualJ2 = Motor.ActivoJugadorDos.VidaActual;
            OffsetEntradaRelevoUno = -140;
            OffsetEntradaRelevoDos = 140;

            TextoNarradorCompleto = "";
            TextoNarradorVisible = "";
            IndiceLetraNarrador = 0;

            InicializarComponentesTablero();

            if (ModoRedActivo && GestorRed != null)
            {
                GestorRed.AlRecibirComando += new DelegadoComandoRedRecibido(ProcesarComandoRemotoSocket);
            }

            IniciarTextoNarradorGba(Motor.ConsultarUltimoEventoPeek());
            ActualizarInterfazCompleta();

            RelojTurno.Start();
            TimerAnimacionGba.Start();

            VerificarEjecucionCpu();
        }

        private void InicializarComponentesTablero()
        {
            this.Text = "Arena Elemental GBA - Simulador de Combate (Estilo Pokemon Rojo Fuego)";
            this.Size = new Size(1040, 740);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(20, 24, 34);
            this.ForeColor = Color.WhiteSmoke;
            this.Font = new Font("Consolas", 9.5F, FontStyle.Bold);

            // 1. Barra Superior de Estado y Reloj
            Panel PnlSuperior = new Panel { Bounds = new Rectangle(15, 10, 995, 48), BackColor = Color.FromArgb(36, 42, 58) };
            this.Controls.Add(PnlSuperior);

            LblBannerEscenario = new Label
            {
                Bounds = new Rectangle(12, 12, 620, 24),
                Font = new Font("Consolas", 10.5F, FontStyle.Bold),
                ForeColor = Color.Gold
            };

            LblCronometro = new Label
            {
                Bounds = new Rectangle(640, 6, 340, 20),
                Font = new Font("Consolas", 10.5F, FontStyle.Bold),
                ForeColor = Color.OrangeRed,
                TextAlign = ContentAlignment.MiddleRight
            };

            PbTiempoTurno = new ProgressBar
            {
                Bounds = new Rectangle(640, 28, 340, 14),
                Maximum = TiempoLimitePorTurno,
                Value = TiempoLimitePorTurno
            };

            PnlSuperior.Controls.Add(LblBannerEscenario);
            PnlSuperior.Controls.Add(LblCronometro);
            PnlSuperior.Controls.Add(PbTiempoTurno);

            // 2. Lienzo Central de Batalla Estilo Game Boy Advance (Doble Buffer)
            PnlLienzoGba = new PanelDobleBuffer();
            PnlLienzoGba.Bounds = new Rectangle(15, 65, 680, 420);
            PnlLienzoGba.Paint += new PaintEventHandler(PnlLienzoGba_Paint);
            this.Controls.Add(PnlLienzoGba);

            // 3. Panel Derecho: Bancas de Relevos FIFO (Queue<Personaje>)
            GroupBox GrpColasGba = new GroupBox
            {
                Text = "PARTY / COLA DE RELEVOS (Queue<Personaje>)",
                Bounds = new Rectangle(705, 65, 305, 420),
                ForeColor = Color.Gold,
                Font = new Font("Consolas", 9F, FontStyle.Bold)
            };
            this.Controls.Add(GrpColasGba);

            Label LblSubCola1 = new Label
            {
                Text = "Banca J1 (" + Motor.JugadorUno.NombreUsuario + "):",
                Bounds = new Rectangle(12, 24, 280, 18),
                ForeColor = Color.LightGreen
            };
            LstColaRelevosJ1 = new ListBox
            {
                Bounds = new Rectangle(12, 45, 280, 165),
                BackColor = Color.FromArgb(28, 36, 48),
                ForeColor = Color.LightGreen,
                Font = new Font("Consolas", 8.5F, FontStyle.Regular)
            };

            Label LblSubCola2 = new Label
            {
                Text = "Banca J2 (" + Motor.JugadorDos.NombreUsuario + "):",
                Bounds = new Rectangle(12, 222, 280, 18),
                ForeColor = Color.LightCoral
            };
            LstColaRelevosJ2 = new ListBox
            {
                Bounds = new Rectangle(12, 243, 280, 165),
                BackColor = Color.FromArgb(28, 36, 48),
                ForeColor = Color.LightCoral,
                Font = new Font("Consolas", 8.5F, FontStyle.Regular)
            };

            GrpColasGba.Controls.Add(LblSubCola1);
            GrpColasGba.Controls.Add(LstColaRelevosJ1);
            GrpColasGba.Controls.Add(LblSubCola2);
            GrpColasGba.Controls.Add(LstColaRelevosJ2);

            // 4. Caja de Comandos Estilo Menu GBA (Derecha Inferior, como LUCHA / MOCHILA / POKEMON / HUIR)
            GroupBox GrpComandosGba = new GroupBox
            {
                Text = "¿QUE DEBERIA HACER TU CARTA? (MENU GBA)",
                Bounds = new Rectangle(520, 492, 490, 198),
                ForeColor = Color.Cyan,
                BackColor = Color.FromArgb(32, 36, 48)
            };
            this.Controls.Add(GrpComandosGba);

            BtnAccionAtacar = new Button
            {
                Text = "► LUCHAR / ATACAR (1 EN)",
                Bounds = new Rectangle(15, 28, 225, 48),
                BackColor = Color.FromArgb(200, 50, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            BtnAccionAtacar.Click += new EventHandler(BtnAccionAtacar_Click);

            BtnAccionDefender = new Button
            {
                Text = "► ESCUDO / DEFENDER (1 EN)",
                Bounds = new Rectangle(250, 28, 225, 48),
                BackColor = Color.FromArgb(40, 100, 195),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            BtnAccionDefender.Click += new EventHandler(BtnAccionDefender_Click);

            BtnAccionEspecial = new Button
            {
                Text = "► HAB. ESPECIAL (2 EN)",
                Bounds = new Rectangle(15, 86, 225, 48),
                BackColor = Color.FromArgb(145, 50, 185),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            BtnAccionEspecial.Click += new EventHandler(BtnAccionEspecial_Click);

            BtnAccionRelevar = new Button
            {
                Text = "► CAMBIO / RELEVO (1 EN)",
                Bounds = new Rectangle(250, 86, 225, 48),
                BackColor = Color.FromArgb(35, 145, 95),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            BtnAccionRelevar.Click += new EventHandler(BtnAccionRelevar_Click);

            BtnPasarTerminarTurno = new Button
            {
                Text = "► TERMINAR TURNO / AHORRAR ENERGIA >>",
                Bounds = new Rectangle(15, 144, 460, 40),
                BackColor = Color.FromArgb(215, 135, 20),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            BtnPasarTerminarTurno.Click += new EventHandler(BtnPasarTerminarTurno_Click);

            GrpComandosGba.Controls.Add(BtnAccionAtacar);
            GrpComandosGba.Controls.Add(BtnAccionDefender);
            GrpComandosGba.Controls.Add(BtnAccionEspecial);
            GrpComandosGba.Controls.Add(BtnAccionRelevar);
            GrpComandosGba.Controls.Add(BtnPasarTerminarTurno);

            // 5. Historial en Vivo de la Pila LIFO (Stack<string>)
            GroupBox GrpPilaGba = new GroupBox
            {
                Text = "HISTORIAL DE EVENTOS EN PILA (Stack<string> - LIFO)",
                Bounds = new Rectangle(15, 492, 495, 198),
                ForeColor = Color.Gold
            };
            this.Controls.Add(GrpPilaGba);

            LblUltimoEventoPeek = new Label
            {
                Bounds = new Rectangle(10, 22, 475, 20),
                ForeColor = Color.LightSkyBlue,
                Font = new Font("Consolas", 8.5F, FontStyle.Bold)
            };

            LstHistorialPila = new ListBox
            {
                Bounds = new Rectangle(10, 45, 475, 142),
                BackColor = Color.FromArgb(14, 18, 24),
                ForeColor = Color.WhiteSmoke,
                Font = new Font("Consolas", 8.5F, FontStyle.Regular)
            };

            GrpPilaGba.Controls.Add(LblUltimoEventoPeek);
            GrpPilaGba.Controls.Add(LstHistorialPila);

            // Configuracion del Reloj de Turno (1 segundo)
            RelojTurno = new System.Windows.Forms.Timer();
            RelojTurno.Interval = 1000;
            RelojTurno.Tick += new EventHandler(RelojTurno_Tick);

            // Configuracion del Motor de Animacion GBA (~33 FPS suaves)
            TimerAnimacionGba = new System.Windows.Forms.Timer();
            TimerAnimacionGba.Interval = 30;
            TimerAnimacionGba.Tick += new EventHandler(TimerAnimacionGba_Tick);
        }

        private void IniciarTextoNarradorGba(string NuevoMensaje)
        {
            TextoNarradorCompleto = NuevoMensaje;
            TextoNarradorVisible = "";
            IndiceLetraNarrador = 0;
        }

        // Bucle de Animacion en Tiempo Real (Estilo Game Boy Advance)
        private void TimerAnimacionGba_Tick(object Sender, EventArgs Args)
        {
            FrameGlobal += 1;

            // 1. Interpolacion suave de las Barras de Vida (bajan poco a poco como en Pokemon)
            float MetaVida1 = Motor.ActivoJugadorUno.VidaActual;
            if (Math.Abs(VidaVisualJ1 - MetaVida1) > 0.4f)
                VidaVisualJ1 += (MetaVida1 - VidaVisualJ1) * 0.22f;
            else
                VidaVisualJ1 = MetaVida1;

            float MetaVida2 = Motor.ActivoJugadorDos.VidaActual;
            if (Math.Abs(VidaVisualJ2 - MetaVida2) > 0.4f)
                VidaVisualJ2 += (MetaVida2 - VidaVisualJ2) * 0.22f;
            else
                VidaVisualJ2 = MetaVida2;

            // 2. Deslizamiento de entrada al hacer Relevo (Slide-in)
            if (OffsetEntradaRelevoUno < 0)
                OffsetEntradaRelevoUno = Math.Min(0, OffsetEntradaRelevoUno + 18);
            if (OffsetEntradaRelevoDos > 0)
                OffsetEntradaRelevoDos = Math.Max(0, OffsetEntradaRelevoDos - 18);

            // 3. Retorno de embestida de ataque (Tackle Lunge)
            if (OffsetAtaqueXJugadorUno > 0)
                OffsetAtaqueXJugadorUno = Math.Max(0, OffsetAtaqueXJugadorUno - 8);
            if (OffsetAtaqueXJugadorDos < 0)
                OffsetAtaqueXJugadorDos = Math.Min(0, OffsetAtaqueXJugadorDos + 8);

            // 4. Contadores de parpadeo de impacto y destello especial
            if (TicksParpadeoDanioUno > 0) TicksParpadeoDanioUno -= 1;
            if (TicksParpadeoDanioDos > 0) TicksParpadeoDanioDos -= 1;
            if (TicksEfectoEspecial > 0) TicksEfectoEspecial -= 1;

            // 5. Sacudida de camara en el evento Terremoto
            if (TicksSacudidaTerremoto > 0)
            {
                TicksSacudidaTerremoto -= 1;
                SacudidaPantallaX = ((FrameGlobal % 2 == 0) ? 7 : -7);
                SacudidaPantallaY = ((FrameGlobal % 3 == 0) ? 5 : -5);
            }
            else
            {
                SacudidaPantallaX = 0;
                SacudidaPantallaY = 0;
            }

            // 6. Efecto Maquina de Escribir (Typewriter) en la Caja de Dialogo GBA
            if (IndiceLetraNarrador < TextoNarradorCompleto.Length)
            {
                int Avance = Math.Min(2, TextoNarradorCompleto.Length - IndiceLetraNarrador);
                IndiceLetraNarrador += Avance;
                TextoNarradorVisible = TextoNarradorCompleto.Substring(0, IndiceLetraNarrador);
            }

            PnlLienzoGba.Invalidate();
        }

        // Renderizado Grafico GDI+ Estilo Pokemon Rojo Fuego (GBA)
        private void PnlLienzoGba_Paint(object Sender, PaintEventArgs Args)
        {
            Graphics G = Args.Graphics;
            G.SmoothingMode = SmoothingMode.AntiAlias;
            G.TranslateTransform(SacudidaPantallaX, SacudidaPantallaY);

            int Ancho = PnlLienzoGba.Width;
            int AltoArena = 325;

            // 1. Fondo del Escenario Elemental con bandas retro GBA
            Color ColorCieloArriba;
            Color ColorSueloAbajo;
            Color ColorPlataforma;
            ObtenerPaletaEscenarioGba(Motor.EscenarioActual.FaccionDominante, out ColorCieloArriba, out ColorSueloAbajo, out ColorPlataforma);

            using (LinearGradientBrush BrochaFondo = new LinearGradientBrush(
                new Rectangle(0, 0, Ancho, AltoArena), ColorCieloArriba, ColorSueloAbajo, LinearGradientMode.Vertical))
            {
                G.FillRectangle(BrochaFondo, 0, 0, Ancho, AltoArena);
            }

            // Lineas de scanline sutiles estilo pantalla GBA
            using (Pen LapizScanline = new Pen(Color.FromArgb(22, 0, 0, 0), 1))
            {
                for (int Y = 0; Y < AltoArena; Y += 4)
                {
                    G.DrawLine(LapizScanline, 0, Y, Ancho, Y);
                }
            }

            // Destello de Habilidad Especial si esta activo
            if (TicksEfectoEspecial > 0 && (TicksEfectoEspecial % 2 == 0))
            {
                using (SolidBrush BrochaFlash = new SolidBrush(Color.FromArgb(95, 255, 255, 180)))
                {
                    G.FillRectangle(BrochaFlash, 0, 0, Ancho, AltoArena);
                }
            }

            // 2. Plataformas Ovaladas de Combate (Suelo del Rival arriba-derecha, Suelo del Jugador 1 abajo-izquierda)
            DibujarPlataformaGba(G, 405, 145, 230, 56, ColorPlataforma);
            DibujarPlataformaGba(G, 45, 258, 250, 62, ColorPlataforma);

            // Calculo de flotacion senoidal suave (Bobbing)
            int FlotacionJ1 = (int)(Math.Sin(FrameGlobal * 0.14) * 4);
            int FlotacionJ2 = (int)(Math.Cos(FrameGlobal * 0.14) * 4);

            // 3. Dibujar Carta y Sprite Elemental del Jugador 2 (Arriba a la Derecha)
            bool OcultarPorParpadeoJ2 = (TicksParpadeoDanioDos > 0) && (TicksParpadeoDanioDos % 2 == 1);
            if (!OcultarPorParpadeoJ2)
            {
                int XCartaJ2 = 465 + OffsetAtaqueXJugadorDos + OffsetEntradaRelevoDos;
                int YCartaJ2 = 30 + FlotacionJ2;
                DibujarCartaSpriteGba(G, Motor.ActivoJugadorDos, XCartaJ2, YCartaJ2, Motor.NumeroJugadorEnTurno == 2);
            }

            // 4. Dibujar Carta y Sprite Elemental del Jugador 1 (Abajo a la Izquierda)
            bool OcultarPorParpadeoJ1 = (TicksParpadeoDanioUno > 0) && (TicksParpadeoDanioUno % 2 == 1);
            if (!OcultarPorParpadeoJ1)
            {
                int XCartaJ1 = 115 + OffsetAtaqueXJugadorUno + OffsetEntradaRelevoUno;
                int YCartaJ1 = 140 + FlotacionJ1;
                DibujarCartaSpriteGba(G, Motor.ActivoJugadorUno, XCartaJ1, YCartaJ1, Motor.NumeroJugadorEnTurno == 1);
            }

            // 5. Cajas de Estado HUD Estilo Pokemon GBA (Crema con borde oscuro, barra HP verde/amarilla/roja y Pokebolas de equipo)
            int TotalVivosJ2 = (Motor.ActivoJugadorDos.EstaVivo ? 1 : 0) + Motor.ColaRelevosJugadorDos.Count;
            int TotalVivosJ1 = (Motor.ActivoJugadorUno.EstaVivo ? 1 : 0) + Motor.ColaRelevosJugadorUno.Count;

            // HUD Jugador 2 (Arriba a la Izquierda, como el rival en Pokemon GBA)
            DibujarHudEstadoPokemonGba(G, 20, 18, Motor.JugadorDos.NombreUsuario, Motor.ActivoJugadorDos, VidaVisualJ2, TotalVivosJ2, Motor.NumeroJugadorEnTurno == 2);

            // HUD Jugador 1 (Abajo a la Derecha, como tu Pokemon en GBA)
            DibujarHudEstadoPokemonGba(G, 365, 195, Motor.JugadorUno.NombreUsuario, Motor.ActivoJugadorUno, VidaVisualJ1, TotalVivosJ1, Motor.NumeroJugadorEnTurno == 1);

            // 6. Caja de Texto Narrador Inferior Estilo GBA (Borde rojo/dorado y fondo azul marino oscuro)
            Rectangle RectCajaTexto = new Rectangle(0, AltoArena, Ancho - 1, PnlLienzoGba.Height - AltoArena - 1);
            using (SolidBrush BrochaMarco = new SolidBrush(Color.FromArgb(210, 55, 45)))
            {
                G.FillRectangle(BrochaMarco, RectCajaTexto);
            }

            Rectangle RectInteriorTexto = new Rectangle(6, AltoArena + 6, Ancho - 13, PnlLienzoGba.Height - AltoArena - 13);
            using (SolidBrush BrochaInterior = new SolidBrush(Color.FromArgb(24, 40, 56)))
            using (Pen LapizBordeInterno = new Pen(Color.FromArgb(240, 225, 160), 3))
            {
                G.FillRectangle(BrochaInterior, RectInteriorTexto);
                G.DrawRectangle(LapizBordeInterno, RectInteriorTexto);
            }

            // Texto con efecto Typewriter y cursor parpadeante ▼
            string CursorGba = ((FrameGlobal / 8) % 2 == 0) ? " ▼" : "";
            string EncabezadoTurno = string.Format("[TURNO #{0} - JUEGA: {1}]",
                Motor.TurnoActualGlobal, Motor.ObtenerPerfilEnTurno().NombreUsuario.ToUpper());

            using (Font FuenteEncabezado = new Font("Consolas", 9.5F, FontStyle.Bold))
            using (Font FuenteDialogo = new Font("Consolas", 11F, FontStyle.Bold))
            {
                G.DrawString(EncabezadoTurno, FuenteEncabezado, Brushes.Gold, new PointF(18, AltoArena + 14));
                RectangleF AreaTexto = new RectangleF(18, AltoArena + 36, Ancho - 36, 52);
                G.DrawString(TextoNarradorVisible + CursorGba, FuenteDialogo, Brushes.White, AreaTexto);
            }
        }

        private void ObtenerPaletaEscenarioGba(Elemento ElementoEscenario, out Color ColorArriba, out Color ColorAbajo, out Color ColorPlataforma)
        {
            if (ElementoEscenario == Elemento.Fuego)
            {
                ColorArriba = Color.FromArgb(135, 38, 28);
                ColorAbajo = Color.FromArgb(220, 110, 45);
                ColorPlataforma = Color.FromArgb(165, 65, 35);
            }
            else if (ElementoEscenario == Elemento.Agua)
            {
                ColorArriba = Color.FromArgb(25, 75, 145);
                ColorAbajo = Color.FromArgb(85, 175, 225);
                ColorPlataforma = Color.FromArgb(45, 125, 185);
            }
            else if (ElementoEscenario == Elemento.Tierra)
            {
                ColorArriba = Color.FromArgb(115, 95, 55);
                ColorAbajo = Color.FromArgb(195, 165, 105);
                ColorPlataforma = Color.FromArgb(140, 110, 65);
            }
            else
            {
                ColorArriba = Color.FromArgb(65, 145, 135);
                ColorAbajo = Color.FromArgb(165, 230, 210);
                ColorPlataforma = Color.FromArgb(95, 185, 155);
            }
        }

        private void DibujarPlataformaGba(Graphics G, int X, int Y, int Ancho, int Alto, Color ColorBase)
        {
            using (SolidBrush BrochaSombra = new SolidBrush(Color.FromArgb(90, 0, 0, 0)))
            using (SolidBrush BrochaInterior = new SolidBrush(ColorBase))
            using (Pen LapizBorde = new Pen(Color.FromArgb(230, 240, 210), 3))
            {
                G.FillEllipse(BrochaSombra, X - 6, Y + 6, Ancho + 12, Alto);
                G.FillEllipse(BrochaInterior, X, Y, Ancho, Alto);
                G.DrawEllipse(LapizBorde, X, Y, Ancho, Alto);
            }
        }

        private void DibujarCartaSpriteGba(Graphics G, Personaje Carta, int X, int Y, bool EsTurnoActivo)
        {
            int AnchoCarta = 110;
            int AltoCarta = 142;

            Color ColorMarcoFaccion;
            if (Carta.Faccion == Elemento.Fuego) ColorMarcoFaccion = Color.FromArgb(220, 60, 40);
            else if (Carta.Faccion == Elemento.Agua) ColorMarcoFaccion = Color.FromArgb(40, 120, 220);
            else if (Carta.Faccion == Elemento.Tierra) ColorMarcoFaccion = Color.FromArgb(150, 105, 50);
            else ColorMarcoFaccion = Color.FromArgb(55, 175, 145);

            // Sombra proyectada sobre la plataforma
            using (SolidBrush Sombra = new SolidBrush(Color.FromArgb(80, 0, 0, 0)))
            {
                G.FillRectangle(Sombra, X + 6, Y + 6, AnchoCarta, AltoCarta);
            }

            // Resplandor dorado si es el personaje en turno
            if (EsTurnoActivo)
            {
                using (Pen LapizTurno = new Pen(Color.Gold, 4))
                {
                    G.DrawRectangle(LapizTurno, X - 3, Y - 3, AnchoCarta + 6, AltoCarta + 6);
                }
            }

            // Cuerpo de la Carta
            Rectangle RectCarta = new Rectangle(X, Y, AnchoCarta, AltoCarta);
            using (SolidBrush BrochaCarta = new SolidBrush(Color.FromArgb(30, 34, 44)))
            using (Pen LapizCarta = new Pen(ColorMarcoFaccion, 3))
            {
                G.FillRectangle(BrochaCarta, RectCarta);
                G.DrawRectangle(LapizCarta, RectCarta);
            }

            // Cabecera de Facción
            Rectangle RectCabecera = new Rectangle(X + 3, Y + 3, AnchoCarta - 6, 22);
            using (SolidBrush BrochaCab = new SolidBrush(ColorMarcoFaccion))
            using (Font FuenteCab = new Font("Consolas", 8.5F, FontStyle.Bold))
            {
                G.FillRectangle(BrochaCab, RectCabecera);
                G.DrawString(Carta.Faccion.ToString().ToUpper(), FuenteCab, Brushes.White, X + 8, Y + 6);
                G.DrawString("ATK:" + Carta.AtaqueBase, FuenteCab, Brushes.Yellow, X + 58, Y + 6);
            }

            // Emblema Elemental Geométrico Central (Estilo Sprite Retro)
            int CentroX = X + (AnchoCarta / 2);
            int CentroY = Y + 68;
            using (SolidBrush BrochaSprite = new SolidBrush(ColorMarcoFaccion))
            using (Pen LapizBrillo = new Pen(Color.White, 2))
            {
                if (Carta.Faccion == Elemento.Fuego)
                {
                    Point[] PuntosLlama = new Point[] {
                        new Point(CentroX, CentroY - 28),
                        new Point(CentroX + 26, CentroY + 18),
                        new Point(CentroX, CentroY + 8),
                        new Point(CentroX - 26, CentroY + 18)
                    };
                    G.FillPolygon(BrochaSprite, PuntosLlama);
                    G.DrawPolygon(LapizBrillo, PuntosLlama);
                }
                else if (Carta.Faccion == Elemento.Agua)
                {
                    G.FillEllipse(BrochaSprite, CentroX - 24, CentroY - 22, 48, 48);
                    G.DrawEllipse(LapizBrillo, CentroX - 24, CentroY - 22, 48, 48);
                }
                else if (Carta.Faccion == Elemento.Tierra)
                {
                    Rectangle RectRoca = new Rectangle(CentroX - 24, CentroY - 22, 48, 46);
                    G.FillRectangle(BrochaSprite, RectRoca);
                    G.DrawRectangle(LapizBrillo, RectRoca);
                }
                else
                {
                    Point[] PuntosRombo = new Point[] {
                        new Point(CentroX, CentroY - 28),
                        new Point(CentroX + 28, CentroY),
                        new Point(CentroX, CentroY + 26),
                        new Point(CentroX - 28, CentroY)
                    };
                    G.FillPolygon(BrochaSprite, PuntosRombo);
                    G.DrawPolygon(LapizBrillo, PuntosRombo);
                }
            }

            // Pie de la Carta: Escudo y Energia
            Rectangle RectPie = new Rectangle(X + 4, Y + 106, AnchoCarta - 8, 32);
            using (SolidBrush BrochaPie = new SolidBrush(Color.FromArgb(20, 22, 28)))
            using (Font FuentePie = new Font("Consolas", 7.5F, FontStyle.Bold))
            {
                G.FillRectangle(BrochaPie, RectPie);
                G.DrawString("ESC:" + Carta.EscudoActual + " EN:" + Carta.EnergiaActual, FuentePie, Brushes.Cyan, X + 8, Y + 109);
                G.DrawString("ESP:" + Carta.CargaEspecial + "%", FuentePie, Brushes.Gold, X + 8, Y + 123);
            }
        }

        private void DibujarHudEstadoPokemonGba(
            Graphics G,
            int X,
            int Y,
            string NombreEntrenador,
            Personaje Carta,
            float VidaAnimada,
            int CartasVivasEquipo,
            bool EnTurno)
        {
            int AnchoHud = 295;
            int AltoHud = 104;

            // Sombra y Caja color crema clasica de Pokemon Rojo Fuego
            using (SolidBrush Sombra = new SolidBrush(Color.FromArgb(100, 0, 0, 0)))
            using (SolidBrush FondoCrema = new SolidBrush(Color.FromArgb(248, 244, 216)))
            using (Pen BordeOscuro = new Pen(Color.FromArgb(48, 56, 48), 3))
            {
                G.FillRectangle(Sombra, X + 5, Y + 5, AnchoHud, AltoHud);
                G.FillRectangle(FondoCrema, X, Y, AnchoHud, AltoHud);
                G.DrawRectangle(BordeOscuro, X, Y, AnchoHud, AltoHud);
            }

            // Indicador de Turno y Nombre de la Carta
            using (Font FuenteNombre = new Font("Consolas", 10F, FontStyle.Bold))
            using (Font FuenteStats = new Font("Consolas", 8.5F, FontStyle.Bold))
            using (SolidBrush TextoOscuro = new SolidBrush(Color.FromArgb(35, 40, 35)))
            {
                string PrefijoTurno = EnTurno ? "► " : "  ";
                G.DrawString(PrefijoTurno + Carta.Nombre.ToUpper(), FuenteNombre, TextoOscuro, X + 8, Y + 8);
                G.DrawString("[" + NombreEntrenador + "]", FuenteStats, Brushes.DarkSlateGray, X + 12, Y + 26);

                // Dibujar las 5 Esferas/Cartas de Equipo (estilo Pokebolas de estado)
                for (int I = 0; I < 5; I++)
                {
                    Brush ColorEsfera = (I < CartasVivasEquipo) ? Brushes.Crimson : Brushes.Silver;
                    G.FillEllipse(ColorEsfera, X + 195 + (I * 18), Y + 26, 13, 13);
                    G.DrawEllipse(Pens.Black, X + 195 + (I * 18), Y + 26, 13, 13);
                }

                // Caja negra para la Barra HP estilo GBA
                G.FillRectangle(Brushes.Black, X + 12, Y + 46, 270, 18);
                G.DrawString("HP", FuenteStats, Brushes.Gold, X + 16, Y + 48);

                float PorcentajeHp = Math.Max(0f, Math.Min(1f, VidaAnimada / (float)Carta.VidaMaxima));
                int AnchoBarraHp = (int)Math.Round(228 * PorcentajeHp);

                Color ColorBarraHp = Color.FromArgb(40, 205, 75); // Verde Pokemon (>50%)
                if (PorcentajeHp <= 0.25f)
                    ColorBarraHp = Color.FromArgb(225, 55, 45); // Rojo peligro (<=25%)
                else if (PorcentajeHp <= 0.50f)
                    ColorBarraHp = Color.FromArgb(245, 200, 35); // Amarillo precaucion (<=50%)

                using (SolidBrush BrochaHp = new SolidBrush(ColorBarraHp))
                {
                    G.FillRectangle(BrochaHp, X + 48, Y + 50, AnchoBarraHp, 10);
                }

                // Barras secundarias de Escudo (DEF) y Especial (ESP) + Numeros
                string ResumenNumerico = string.Format("HP:{0}/{1} | ESC:{2} | EN:{3} | ESP:{4}%",
                    (int)Math.Round(VidaAnimada), Carta.VidaMaxima, Carta.EscudoActual, Carta.EnergiaActual, Carta.CargaEspecial);
                G.DrawString(ResumenNumerico, FuenteStats, TextoOscuro, X + 12, Y + 68);

                // Mini barra de Escudo (Azul) y Carga Especial (Morado/Oro)
                int AnchoEscudo = Math.Min(125, (int)Math.Round((Carta.EscudoActual / (float)Math.Max(1, Carta.DefensaBase * 2)) * 125));
                G.FillRectangle(Brushes.LightGray, X + 12, Y + 88, 125, 8);
                G.FillRectangle(Brushes.DodgerBlue, X + 12, Y + 88, AnchoEscudo, 8);

                int AnchoEsp = (int)Math.Round((Carta.CargaEspecial / 100f) * 130);
                G.FillRectangle(Brushes.LightGray, X + 150, Y + 88, 130, 8);
                G.FillRectangle(Carta.EspecialListo ? Brushes.Gold : Brushes.MediumPurple, X + 150, Y + 88, AnchoEsp, 8);
            }
        }

        private void ActualizarInterfazCompleta()
        {
            LblBannerEscenario.Text = string.Format("ENTORNO: {0} (+{1} {2} / -{3} {4}) | Umbral Empatia: {5}",
                Motor.EscenarioActual.Nombre.ToUpper(),
                Motor.EscenarioActual.BonoAtaque,
                Motor.EscenarioActual.FaccionDominante,
                Motor.EscenarioActual.PenalizacionAtaque,
                Motor.EscenarioActual.FaccionPenalizada,
                Motor.EscenarioActual.UmbralEmpatia);

            LblCronometro.Text = string.Format("RELOJ DE TURNO: {0:00} SEG", SegundosRestantesTurno);
            PbTiempoTurno.Value = Math.Max(0, Math.Min(TiempoLimitePorTurno, SegundosRestantesTurno));

            // Actualizar ListBox de Colas de Relevos (Queue<Personaje>)
            LstColaRelevosJ1.Items.Clear();
            foreach (Personaje EnBanca in Motor.ColaRelevosJugadorUno)
            {
                LstColaRelevosJ1.Items.Add(EnBanca.ToString());
            }

            LstColaRelevosJ2.Items.Clear();
            foreach (Personaje EnBanca in Motor.ColaRelevosJugadorDos)
            {
                LstColaRelevosJ2.Items.Add(EnBanca.ToString());
            }

            // Habilitar / Deshabilitar botones del Menu GBA segun el turno y la energia
            Personaje ActivoTurno = Motor.ObtenerPersonajeActivoEnTurno();
            bool ControlesHabilitados = !Motor.PartidaFinalizada && !(Motor.ContraCpu && Motor.NumeroJugadorEnTurno == 2);

            BtnAccionAtacar.Enabled = ControlesHabilitados && (ActivoTurno.EnergiaActual >= Personaje.CostoAtaque);
            BtnAccionDefender.Enabled = ControlesHabilitados && (ActivoTurno.EnergiaActual >= Personaje.CostoDefensa);
            BtnAccionEspecial.Enabled = ControlesHabilitados && ActivoTurno.EspecialListo && (Motor.TurnoActualGlobal > 1);
            BtnAccionRelevar.Enabled = ControlesHabilitados && (Motor.ObtenerColaEnTurno().Count > 0) && (ActivoTurno.EnergiaActual >= Personaje.CostoRelevo);
            BtnPasarTerminarTurno.Enabled = ControlesHabilitados;

            // Actualizar Pila (Stack<string>) en la Bitacora en Vivo
            LblUltimoEventoPeek.Text = "Stack.Peek(): " + Motor.ConsultarUltimoEventoPeek();
            LstHistorialPila.Items.Clear();
            foreach (string Evento in Motor.PilaHistorialEventos)
            {
                LstHistorialPila.Items.Add(Evento);
            }

            PnlLienzoGba.Invalidate();
        }

        private void DispararAnimacionAtaque(int JugadorAtacante, bool EsEspecial)
        {
            if (JugadorAtacante == 1)
            {
                OffsetAtaqueXJugadorUno = 64;
                TicksParpadeoDanioDos = 10;
            }
            else
            {
                OffsetAtaqueXJugadorDos = -64;
                TicksParpadeoDanioUno = 10;
            }

            if (EsEspecial)
            {
                TicksEfectoEspecial = 14;
            }
        }

        private void RelojTurno_Tick(object Sender, EventArgs Args)
        {
            if (Motor.PartidaFinalizada)
            {
                RelojTurno.Stop();
                return;
            }

            SegundosRestantesTurno -= 1;
            if (SegundosRestantesTurno <= 0)
            {
                AvanzarTurnoConEfectos(true);
            }
            else
            {
                LblCronometro.Text = string.Format("RELOJ DE TURNO: {0:00} SEG", SegundosRestantesTurno);
                PbTiempoTurno.Value = SegundosRestantesTurno;
            }
        }

        private void AvanzarTurnoConEfectos(bool FuePorTiempoAgotado)
        {
            int TurnoAntes = Motor.TurnoActualGlobal;
            SegundosRestantesTurno = TiempoLimitePorTurno;
            Motor.FinalizarTurnoYAvanzar(FuePorTiempoAgotado);

            if (TurnoAntes % 3 == 0)
            {
                TicksSacudidaTerremoto = 20;
            }

            IniciarTextoNarradorGba(Motor.ConsultarUltimoEventoPeek());
            ActualizarInterfazCompleta();
            VerificarCierreDePartida();
            VerificarEjecucionCpu();
        }

        private void VerificarEjecucionCpu()
        {
            if (!Motor.PartidaFinalizada && Motor.ContraCpu && Motor.NumeroJugadorEnTurno == 2)
            {
                DispararAnimacionAtaque(2, Motor.ActivoJugadorDos.EspecialListo);
                Motor.EjecutarTurnoInteligenciaCpu();
                IniciarTextoNarradorGba(Motor.ConsultarUltimoEventoPeek());
                VerificarCierreDePartida();

                if (!Motor.PartidaFinalizada)
                {
                    SegundosRestantesTurno = TiempoLimitePorTurno;
                    Motor.FinalizarTurnoYAvanzar(false);
                }
                ActualizarInterfazCompleta();
            }
        }

        private void VerificarCierreDePartida()
        {
            if (Motor.PartidaFinalizada && Motor.GanadorPartida != null)
            {
                RelojTurno.Stop();
                ActualizarInterfazCompleta();

                PerfilJugador Ganador = Motor.GanadorPartida;
                PerfilJugador Perdedor = (Ganador == Motor.JugadorUno) ? Motor.JugadorDos : Motor.JugadorUno;

                string RutaLogTxt = GestorArchivosLog.ExportarBitacoraTxt(
                    Motor.JugadorUno,
                    Motor.JugadorDos,
                    Ganador,
                    Motor.EscenarioActual,
                    Motor.TurnoActualGlobal,
                    Motor.PilaHistorialEventos);

                GestorDatos.ActualizarEstadisticasPostBatalla(
                    Ganador,
                    Perdedor,
                    Motor.EscenarioActual.IdEscenario,
                    Motor.TurnoActualGlobal,
                    RutaLogTxt);

                string Salto = Environment.NewLine;
                string TextoFinal = string.Format(
                    "¡FIN DEL COMBATE EN LA ARENA ELEMENTAL!{0}{0}Vencedor: {1}{0}Total de Turnos: {2}{0}{0}Estadisticas actualizadas en Base de Datos.{0}Bitacora TXT generada en:{0}{3}",
                    Salto, Ganador.NombreUsuario, Motor.TurnoActualGlobal, RutaLogTxt);
                MessageBox.Show(TextoFinal, "Victoria Proclamada", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnAccionAtacar_Click(object Sender, EventArgs Args)
        {
            try
            {
                int Jugador = Motor.NumeroJugadorEnTurno;
                string NombreAntes1 = Motor.ActivoJugadorUno.Nombre;
                string NombreAntes2 = Motor.ActivoJugadorDos.Nombre;

                string Msg = Motor.EjecutarAtaqueEnTurno();
                DispararAnimacionAtaque(Jugador, false);

                if (Motor.ActivoJugadorUno.Nombre != NombreAntes1) OffsetEntradaRelevoUno = -140;
                if (Motor.ActivoJugadorDos.Nombre != NombreAntes2) OffsetEntradaRelevoDos = 140;

                IniciarTextoNarradorGba(Msg);
                if (ModoRedActivo) GestorRed.EnviarComandoAccion("ATACAR");
                ActualizarInterfazCompleta();
                VerificarCierreDePartida();
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message, "Accion Invalida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnAccionDefender_Click(object Sender, EventArgs Args)
        {
            try
            {
                string Msg = Motor.EjecutarDefensaEnTurno();
                TicksEfectoEspecial = 6;
                IniciarTextoNarradorGba(Msg);
                if (ModoRedActivo) GestorRed.EnviarComandoAccion("DEFENDER");
                ActualizarInterfazCompleta();
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message, "Accion Invalida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnAccionEspecial_Click(object Sender, EventArgs Args)
        {
            try
            {
                int Jugador = Motor.NumeroJugadorEnTurno;
                string Msg = Motor.EjecutarHabilidadEspecialEnTurno();
                DispararAnimacionAtaque(Jugador, true);
                IniciarTextoNarradorGba(Msg);
                if (ModoRedActivo) GestorRed.EnviarComandoAccion("ESPECIAL");
                ActualizarInterfazCompleta();
                VerificarCierreDePartida();
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message, "Accion Invalida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnAccionRelevar_Click(object Sender, EventArgs Args)
        {
            try
            {
                int Jugador = Motor.NumeroJugadorEnTurno;
                string Msg = Motor.EjecutarRelevoTacticoEnTurno();

                if (Jugador == 1)
                {
                    OffsetEntradaRelevoUno = -150;
                    VidaVisualJ1 = Motor.ActivoJugadorUno.VidaActual;
                }
                else
                {
                    OffsetEntradaRelevoDos = 150;
                    VidaVisualJ2 = Motor.ActivoJugadorDos.VidaActual;
                }

                IniciarTextoNarradorGba(Msg);
                if (ModoRedActivo) GestorRed.EnviarComandoAccion("RELEVAR");
                ActualizarInterfazCompleta();
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message, "Accion Invalida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnPasarTerminarTurno_Click(object Sender, EventArgs Args)
        {
            if (ModoRedActivo) GestorRed.EnviarComandoAccion("PASAR");
            AvanzarTurnoConEfectos(false);
        }

        private void ProcesarComandoRemotoSocket(string Comando)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new DelegadoComandoRedRecibido(ProcesarComandoRemotoSocket), new object[] { Comando });
                return;
            }

            try
            {
                int Jugador = Motor.NumeroJugadorEnTurno;
                if (Comando == "ATACAR")
                {
                    DispararAnimacionAtaque(Jugador, false);
                    IniciarTextoNarradorGba(Motor.EjecutarAtaqueEnTurno());
                }
                else if (Comando == "DEFENDER")
                {
                    IniciarTextoNarradorGba(Motor.EjecutarDefensaEnTurno());
                }
                else if (Comando == "ESPECIAL")
                {
                    DispararAnimacionAtaque(Jugador, true);
                    IniciarTextoNarradorGba(Motor.EjecutarHabilidadEspecialEnTurno());
                }
                else if (Comando == "RELEVAR")
                {
                    if (Jugador == 1) OffsetEntradaRelevoUno = -150;
                    else OffsetEntradaRelevoDos = 150;
                    IniciarTextoNarradorGba(Motor.EjecutarRelevoTacticoEnTurno());
                }
                else if (Comando == "PASAR")
                {
                    AvanzarTurnoConEfectos(false);
                    return;
                }

                ActualizarInterfazCompleta();
                VerificarCierreDePartida();
            }
            catch (Exception)
            {
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs Args)
        {
            if (RelojTurno != null) RelojTurno.Stop();
            if (TimerAnimacionGba != null) TimerAnimacionGba.Stop();
            base.OnFormClosing(Args);
        }
    }
}
