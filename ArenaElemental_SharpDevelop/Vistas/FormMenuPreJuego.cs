using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ArenaElemental.Datos;
using ArenaElemental.Logica;
using ArenaElemental.Modelos;
using ArenaElemental.Red;

namespace ArenaElemental.Vistas
{
    public class FormMenuPreJuego : Form
    {
        private GestorBaseDatos GestorDatos;
        private GestorRedSockets GestorRed;
        private List<Personaje> CatalogoCartas;
        private List<Escenario> CatalogoEscenarios;

        private PerfilJugador PerfilUno;
        private PerfilJugador PerfilDos;

        // Maquina de Estados de Pantallas GBA (1 = Titulo, 2 = Modo y Escenario, 3 = Laboratorio de Equipos)
        private int EtapaActualMenu;
        private System.Windows.Forms.Timer TimerAnimacionMenu;
        private int FrameMenu;

        // Paneles de las 3 Vistas Separadas
        private PanelDobleBuffer PnlPantallaTitulo;
        private PanelDobleBuffer PnlPantallaModalidad;
        private PanelDobleBuffer PnlPantallaEquipos;

        // Controles Pantalla 2: Modalidad, Escenario y Red
        private RadioButton RbModoLocal1v1;
        private RadioButton RbModoContraCpu;
        private RadioButton RbModoRedLan;
        private ComboBox CmbEscenarioInicial;
        private CheckBox ChkEscenarioAlternante;
        private TextBox TxtIpServidor;
        private TextBox TxtPuertoRed;
        private Button BtnHospedarPartida;
        private Button BtnConectarPartida;
        private Label LblEstadoRed;
        private Button BtnVolverATitulo;
        private Button BtnIrALaboratorio;

        // Controles Pantalla 3: Perfiles y Seleccion de Equipo (5 Cartas)
        private TextBox TxtUsuarioUno;
        private TextBox TxtUsuarioDos;
        private Button BtnAutenticarJugadores;
        private Label LblEstadisticasUno;
        private Label LblEstadisticasDos;
        private ListBox LstCatalogoCartas;
        private ListBox LstMazoJugadorUno;
        private ListBox LstMazoJugadorDos;
        private Button BtnAgregarCartaJ1;
        private Button BtnLimpiarMazoJ1;
        private Button BtnMazoAleatorioJ1;
        private Button BtnAgregarCartaJ2;
        private Button BtnLimpiarMazoJ2;
        private Button BtnMazoAleatorioJ2;
        private Button BtnVolverAModalidad;
        private Button BtnIniciarCombate;

        public FormMenuPreJuego()
        {
            GestorDatos = new GestorBaseDatos();
            GestorRed = new GestorRedSockets();
            CatalogoCartas = GestorDatos.ObtenerCatalogoPersonajes();
            CatalogoEscenarios = GestorDatos.ObtenerCatalogoEscenarios();

            EtapaActualMenu = 1;
            FrameMenu = 0;

            InicializarContenedorPrincipal();
            ConstruirPantalla1TituloGba();
            ConstruirPantalla2ModalidadYEscenario();
            ConstruirPantalla3LaboratorioEquipos();

            CargarDatosInicialesEnControles();
            CambiarEtapaMenu(1);

            TimerAnimacionMenu = new System.Windows.Forms.Timer();
            TimerAnimacionMenu.Interval = 35;
            TimerAnimacionMenu.Tick += new EventHandler(TimerAnimacionMenu_Tick);
            TimerAnimacionMenu.Start();
        }

        private void InicializarContenedorPrincipal()
        {
            this.Text = "Arena Elemental GBA - Edicion Rojo Fuego (POO C# & SQL)";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(16, 20, 28);
            this.ForeColor = Color.WhiteSmoke;
            this.Font = new Font("Consolas", 9.5F, FontStyle.Bold);
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(FormMenuPreJuego_KeyDown);
        }

        private void CambiarEtapaMenu(int NuevaEtapa)
        {
            EtapaActualMenu = NuevaEtapa;
            PnlPantallaTitulo.Visible = (EtapaActualMenu == 1);
            PnlPantallaModalidad.Visible = (EtapaActualMenu == 2);
            PnlPantallaEquipos.Visible = (EtapaActualMenu == 3);

            if (EtapaActualMenu == 1) PnlPantallaTitulo.Invalidate();
            if (EtapaActualMenu == 2) PnlPantallaModalidad.Invalidate();
            if (EtapaActualMenu == 3) PnlPantallaEquipos.Invalidate();
        }

        private void TimerAnimacionMenu_Tick(object Sender, EventArgs Args)
        {
            FrameMenu += 1;
            if (EtapaActualMenu == 1)
                PnlPantallaTitulo.Invalidate();
            else if (EtapaActualMenu == 2)
                PnlPantallaModalidad.Invalidate();
            else if (EtapaActualMenu == 3)
                PnlPantallaEquipos.Invalidate();
        }

        private void FormMenuPreJuego_KeyDown(object Sender, KeyEventArgs Args)
        {
            if (EtapaActualMenu == 1 && (Args.KeyCode == Keys.Enter || Args.KeyCode == Keys.Space))
            {
                CambiarEtapaMenu(2);
            }
        }

        // =========================================================================
        // PANTALLA 1: INTRO / PANTALLA DE PRE-CARGA ESTILO POKEMON ROJO FUEGO (GBA)
        // =========================================================================
        private void ConstruirPantalla1TituloGba()
        {
            PnlPantallaTitulo = new PanelDobleBuffer();
            PnlPantallaTitulo.Bounds = new Rectangle(0, 0, 994, 671);
            PnlPantallaTitulo.Paint += new PaintEventHandler(PnlPantallaTitulo_Paint);
            PnlPantallaTitulo.MouseClick += new MouseEventHandler(PnlPantallaTitulo_MouseClick);

            Button BtnPressStart = new Button
            {
                Text = "► PULSA START / COMENZAR AVENTURA ◄",
                Bounds = new Rectangle(310, 520, 375, 48),
                BackColor = Color.FromArgb(220, 55, 35),
                ForeColor = Color.Yellow,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Consolas", 12F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            BtnPressStart.FlatAppearance.BorderColor = Color.Gold;
            BtnPressStart.FlatAppearance.BorderSize = 3;
            BtnPressStart.Click += delegate { CambiarEtapaMenu(2); };

            PnlPantallaTitulo.Controls.Add(BtnPressStart);
            this.Controls.Add(PnlPantallaTitulo);
        }

        private void PnlPantallaTitulo_MouseClick(object Sender, MouseEventArgs Args)
        {
            CambiarEtapaMenu(2);
        }

        private void PnlPantallaTitulo_Paint(object Sender, PaintEventArgs Args)
        {
            Graphics G = Args.Graphics;
            G.SmoothingMode = SmoothingMode.AntiAlias;
            int W = PnlPantallaTitulo.Width;
            int H = PnlPantallaTitulo.Height;

            // Fondo degradado ardiente estilo Pokemon FireRed Intro
            using (LinearGradientBrush Fondo = new LinearGradientBrush(
                new Rectangle(0, 0, W, H),
                Color.FromArgb(18, 12, 34),
                Color.FromArgb(165, 35, 20),
                LinearGradientMode.Vertical))
            {
                G.FillRectangle(Fondo, 0, 0, W, H);
            }

            // Bandas horizontales de velocidad estilo GBA
            using (Pen LapizScan = new Pen(Color.FromArgb(28, 255, 200, 80), 2))
            {
                for (int Y = 0; Y < H; Y += 6)
                {
                    G.DrawLine(LapizScan, 0, Y, W, Y);
                }
            }

            // Barras negras cinematograficas arriba y abajo (Letterbox GBA)
            G.FillRectangle(Brushes.Black, 0, 0, W, 52);
            G.FillRectangle(Brushes.Black, 0, H - 55, W, 55);

            using (Font FuenteCopy = new Font("Consolas", 9.5F, FontStyle.Bold))
            {
                G.DrawString("GAME BOY ADVANCE EDITION  |  SIMULADOR DE COMBATE POO C# & SQL", FuenteCopy, Brushes.Gold, 22, 16);
                G.DrawString("© 2026 PROYECTO FINAL POO - ESTRUCTURAS DINAMICAS (LIST, QUEUE, STACK) & SOCKETS", FuenteCopy, Brushes.Silver, 165, H - 35);
            }

            // Orbes Elementales Orbitando en el Centro
            int CentroX = W / 2;
            int CentroY = 305;
            double AnguloBase = FrameMenu * 0.05;

            DibujarOrbeElementalIntro(G, CentroX + (int)(Math.Cos(AnguloBase) * 230), CentroY + (int)(Math.Sin(AnguloBase) * 65), Color.OrangeRed, "FUEGO");
            DibujarOrbeElementalIntro(G, CentroX + (int)(Math.Cos(AnguloBase + 1.57) * 230), CentroY + (int)(Math.Sin(AnguloBase + 1.57) * 65), Color.DodgerBlue, "AGUA");
            DibujarOrbeElementalIntro(G, CentroX + (int)(Math.Cos(AnguloBase + 3.14) * 230), CentroY + (int)(Math.Sin(AnguloBase + 3.14) * 65), Color.Peru, "TIERRA");
            DibujarOrbeElementalIntro(G, CentroX + (int)(Math.Cos(AnguloBase + 4.71) * 230), CentroY + (int)(Math.Sin(AnguloBase + 4.71) * 65), Color.MediumAquamarine, "AIRE");

            // Placa Central del Logo Estilo Pokemon
            Rectangle RectLogo = new Rectangle(175, 95, 645, 160);
            using (SolidBrush SombraLogo = new SolidBrush(Color.FromArgb(140, 0, 0, 0)))
            using (LinearGradientBrush FondoLogo = new LinearGradientBrush(RectLogo, Color.FromArgb(40, 50, 85), Color.FromArgb(20, 24, 42), LinearGradientMode.Vertical))
            using (Pen BordeOro = new Pen(Color.Gold, 5))
            {
                G.FillRectangle(SombraLogo, RectLogo.X + 8, RectLogo.Y + 8, RectLogo.Width, RectLogo.Height);
                G.FillRectangle(FondoLogo, RectLogo);
                G.DrawRectangle(BordeOro, RectLogo);
            }

            using (Font FuenteTitulo = new Font("Consolas", 34F, FontStyle.Bold))
            using (Font FuenteSub = new Font("Consolas", 13F, FontStyle.Bold))
            {
                G.DrawString("ARENA ELEMENTAL", FuenteTitulo, Brushes.Black, 219, 119);
                G.DrawString("ARENA ELEMENTAL", FuenteTitulo, Brushes.Gold, 215, 115);

                Rectangle RectCintaRoja = new Rectangle(285, 192, 425, 38);
                using (SolidBrush BrochaRoja = new SolidBrush(Color.FromArgb(215, 45, 35)))
                using (Pen LapizBlanco = new Pen(Color.White, 2))
                {
                    G.FillRectangle(BrochaRoja, RectCintaRoja);
                    G.DrawRectangle(LapizBlanco, RectCintaRoja);
                }
                G.DrawString("EDICION ROJO FUEGO - COMBATE DE CARTAS", FuenteSub, Brushes.White, 298, 201);
            }

            // Emblema Central de 4 Elementos
            int Flotacion = (int)(Math.Sin(FrameMenu * 0.12) * 6);
            Rectangle RectEmblema = new Rectangle(CentroX - 65, CentroY - 35 + Flotacion, 130, 130);
            using (SolidBrush BrochaCirculo = new SolidBrush(Color.FromArgb(245, 240, 220)))
            using (SolidBrush BrochaRoja = new SolidBrush(Color.FromArgb(215, 45, 35)))
            using (Pen BordeNegro = new Pen(Color.FromArgb(30, 30, 30), 5))
            {
                G.FillEllipse(BrochaCirculo, RectEmblema);
                G.FillPie(BrochaRoja, RectEmblema, 180, 180);
                G.DrawEllipse(BordeNegro, RectEmblema);
                G.DrawLine(BordeNegro, RectEmblema.Left, CentroY + 30 + Flotacion, RectEmblema.Right, CentroY + 30 + Flotacion);
                G.FillEllipse(Brushes.White, CentroX - 18, CentroY + 12 + Flotacion, 36, 36);
                G.DrawEllipse(BordeNegro, CentroX - 18, CentroY + 12 + Flotacion, 36, 36);
            }

            // Texto Parpadeante "PRESS START"
            if ((FrameMenu / 10) % 2 == 0)
            {
                using (Font FuenteBlink = new Font("Consolas", 13F, FontStyle.Bold))
                {
                    G.DrawString("¡HAZ CLIC O PRESIONA [ENTER] PARA CONTINUAR!", FuenteBlink, Brushes.White, 275, 480);
                }
            }
        }

        private void DibujarOrbeElementalIntro(Graphics G, int X, int Y, Color ColorElemento, string Etiqueta)
        {
            using (SolidBrush Sombra = new SolidBrush(Color.FromArgb(90, 0, 0, 0)))
            using (SolidBrush Brocha = new SolidBrush(ColorElemento))
            using (Pen Borde = new Pen(Color.White, 2))
            using (Font Fuente = new Font("Consolas", 8.5F, FontStyle.Bold))
            {
                G.FillEllipse(Sombra, X - 26, Y - 22, 56, 56);
                G.FillEllipse(Brocha, X - 28, Y - 26, 56, 56);
                G.DrawEllipse(Borde, X - 28, Y - 26, 56, 56);
                G.DrawString(Etiqueta, Fuente, Brushes.White, X - 22, Y - 6);
            }
        }

        // =========================================================================
        // PANTALLA 2: SELECCION DE MODALIDAD DE JUEGO, ESCENARIO Y RED (ESTILO GBA)
        // =========================================================================
        private void ConstruirPantalla2ModalidadYEscenario()
        {
            PnlPantallaModalidad = new PanelDobleBuffer();
            PnlPantallaModalidad.Bounds = new Rectangle(0, 0, 994, 671);
            PnlPantallaModalidad.Paint += new PaintEventHandler(PnlPantallaModalidad_Paint);

            // Caja 1: Modalidad de Combate
            GroupBox GrpModo = new GroupBox
            {
                Text = "1. ELIGE LA MODALIDAD DE COMBATE",
                Bounds = new Rectangle(55, 110, 425, 225),
                BackColor = Color.FromArgb(248, 244, 216),
                ForeColor = Color.FromArgb(40, 50, 40),
                Font = new Font("Consolas", 10.5F, FontStyle.Bold)
            };

            RbModoLocal1v1 = new RadioButton
            {
                Text = "► MODO DUELO LOCAL (Jugador 1 vs Jugador 2)",
                Bounds = new Rectangle(20, 42, 385, 32),
                Checked = true,
                ForeColor = Color.FromArgb(30, 35, 30)
            };

            RbModoContraCpu = new RadioButton
            {
                Text = "► MODO ENTRENAMIENTO (Jugador 1 vs CPU IA)",
                Bounds = new Rectangle(20, 95, 385, 32),
                ForeColor = Color.FromArgb(30, 35, 30)
            };

            RbModoRedLan = new RadioButton
            {
                Text = "► MODO CABLE LINK / RED LAN (Sockets TCP)",
                Bounds = new Rectangle(20, 148, 385, 32),
                ForeColor = Color.FromArgb(30, 35, 30)
            };

            GrpModo.Controls.Add(RbModoLocal1v1);
            GrpModo.Controls.Add(RbModoContraCpu);
            GrpModo.Controls.Add(RbModoRedLan);
            PnlPantallaModalidad.Controls.Add(GrpModo);

            // Caja 2: Seleccion de Escenario Elemental
            GroupBox GrpEscenario = new GroupBox
            {
                Text = "2. SELECCIONA EL ESCENARIO DE LA ARENA (4 CARTAS)",
                Bounds = new Rectangle(510, 110, 430, 225),
                BackColor = Color.FromArgb(248, 244, 216),
                ForeColor = Color.FromArgb(40, 50, 40),
                Font = new Font("Consolas", 10.5F, FontStyle.Bold)
            };

            Label LblSelEsc = new Label
            {
                Text = "Entorno Elemental Inicial:",
                Bounds = new Rectangle(20, 35, 380, 22),
                ForeColor = Color.FromArgb(40, 45, 40)
            };

            CmbEscenarioInicial = new ComboBox
            {
                Bounds = new Rectangle(20, 62, 385, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Consolas", 9.5F, FontStyle.Bold)
            };
            CmbEscenarioInicial.SelectedIndexChanged += delegate { PnlPantallaModalidad.Invalidate(); };

            ChkEscenarioAlternante = new CheckBox
            {
                Text = "Rotar Escenario automaticamente cada 2 turnos",
                Bounds = new Rectangle(20, 110, 390, 28),
                Checked = true,
                ForeColor = Color.FromArgb(35, 40, 35)
            };

            Label LblInfoTerremoto = new Label
            {
                Text = "* Nota: Cada 3 turnos ocurre el evento Terremoto. Si la Empatia supera el umbral del escenario, el Terremoto lo cura en vez de daniarlo.",
                Bounds = new Rectangle(20, 145, 395, 65),
                ForeColor = Color.DarkRed,
                Font = new Font("Consolas", 8.5F, FontStyle.Bold)
            };

            GrpEscenario.Controls.Add(LblSelEsc);
            GrpEscenario.Controls.Add(CmbEscenarioInicial);
            GrpEscenario.Controls.Add(ChkEscenarioAlternante);
            GrpEscenario.Controls.Add(LblInfoTerremoto);
            PnlPantallaModalidad.Controls.Add(GrpEscenario);

            // Caja 3: Conexion Cable Link TCP/IP
            GroupBox GrpCableLink = new GroupBox
            {
                Text = "3. CONFIGURACION DE SALA CABLE LINK (SOCKETS TCP/IP OPCIONAL)",
                Bounds = new Rectangle(55, 355, 885, 125),
                BackColor = Color.FromArgb(248, 244, 216),
                ForeColor = Color.FromArgb(40, 50, 40),
                Font = new Font("Consolas", 10F, FontStyle.Bold)
            };

            Label LblIp = new Label { Text = "Direccion IP:", Bounds = new Rectangle(20, 36, 115, 22), ForeColor = Color.Black };
            TxtIpServidor = new TextBox { Text = "127.0.0.1", Bounds = new Rectangle(140, 33, 140, 26) };

            Label LblPort = new Label { Text = "Puerto TCP:", Bounds = new Rectangle(300, 36, 95, 22), ForeColor = Color.Black };
            TxtPuertoRed = new TextBox { Text = "5000", Bounds = new Rectangle(400, 33, 85, 26) };

            BtnHospedarPartida = new Button
            {
                Text = "HOSPEDAR SALA (SERVIDOR)",
                Bounds = new Rectangle(510, 28, 175, 36),
                BackColor = Color.FromArgb(55, 95, 165),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Consolas", 8.5F, FontStyle.Bold)
            };
            BtnHospedarPartida.Click += new EventHandler(BtnHospedarPartida_Click);

            BtnConectarPartida = new Button
            {
                Text = "UNIRSE A SALA (CLIENTE)",
                Bounds = new Rectangle(695, 28, 170, 36),
                BackColor = Color.FromArgb(40, 140, 115),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Consolas", 8.5F, FontStyle.Bold)
            };
            BtnConectarPartida.Click += new EventHandler(BtnConectarPartida_Click);

            LblEstadoRed = new Label
            {
                Text = "Estado Cable Link: Modo Local activo. (Usa los botones de arriba solo si jugaras en Red LAN).",
                Bounds = new Rectangle(20, 78, 845, 24),
                ForeColor = Color.DarkSlateBlue
            };

            GrpCableLink.Controls.Add(LblIp);
            GrpCableLink.Controls.Add(TxtIpServidor);
            GrpCableLink.Controls.Add(LblPort);
            GrpCableLink.Controls.Add(TxtPuertoRed);
            GrpCableLink.Controls.Add(BtnHospedarPartida);
            GrpCableLink.Controls.Add(BtnConectarPartida);
            GrpCableLink.Controls.Add(LblEstadoRed);
            PnlPantallaModalidad.Controls.Add(GrpCableLink);

            // Botones de Navegacion de Etapa 2
            BtnVolverATitulo = new Button
            {
                Text = "<< VOLVER A PANTALLA DE TITULO",
                Bounds = new Rectangle(55, 585, 290, 46),
                BackColor = Color.FromArgb(95, 100, 115),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Consolas", 10F, FontStyle.Bold)
            };
            BtnVolverATitulo.Click += delegate { CambiarEtapaMenu(1); };

            BtnIrALaboratorio = new Button
            {
                Text = "SIGUIENTE: NOMBRES Y EQUIPO (5 CARTAS) >>",
                Bounds = new Rectangle(520, 585, 420, 46),
                BackColor = Color.FromArgb(215, 65, 35),
                ForeColor = Color.Yellow,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Consolas", 10.5F, FontStyle.Bold)
            };
            BtnIrALaboratorio.Click += delegate { CambiarEtapaMenu(3); };

            PnlPantallaModalidad.Controls.Add(BtnVolverATitulo);
            PnlPantallaModalidad.Controls.Add(BtnIrALaboratorio);
            this.Controls.Add(PnlPantallaModalidad);
        }

        private void PnlPantallaModalidad_Paint(object Sender, PaintEventArgs Args)
        {
            Graphics G = Args.Graphics;
            int W = PnlPantallaModalidad.Width;
            int H = PnlPantallaModalidad.Height;

            // Fondo estilo Menu de Opciones de Pokemon GBA
            using (LinearGradientBrush Fondo = new LinearGradientBrush(
                new Rectangle(0, 0, W, H), Color.FromArgb(28, 52, 78), Color.FromArgb(16, 30, 46), LinearGradientMode.Vertical))
            {
                G.FillRectangle(Fondo, 0, 0, W, H);
            }

            // Cabecera estilo GBA
            G.FillRectangle(new SolidBrush(Color.FromArgb(210, 55, 40)), 0, 0, W, 68);
            G.DrawLine(new Pen(Color.Gold, 4), 0, 68, W, 68);

            using (Font FuenteTit = new Font("Consolas", 16F, FontStyle.Bold))
            using (Font FuenteSub = new Font("Consolas", 9.5F, FontStyle.Bold))
            {
                G.DrawString("PASO 1 DE 2: MODALIDAD DE COMBATE Y ESCENARIO ELEMENTAL", FuenteTit, Brushes.White, 25, 14);
                G.DrawString("Persistencia SQL: " + GestorDatos.UltimoMensajeEstado, FuenteSub, Brushes.Yellow, 25, 42);
            }

            // Caja de Dialogo del Profesor en la parte inferior
            Rectangle RectDialogo = new Rectangle(55, 495, 885, 74);
            using (SolidBrush BrochaMarco = new SolidBrush(Color.FromArgb(210, 55, 45)))
            using (SolidBrush BrochaInt = new SolidBrush(Color.FromArgb(24, 40, 56)))
            using (Pen LapizBorde = new Pen(Color.FromArgb(240, 225, 160), 3))
            using (Font FuenteTexto = new Font("Consolas", 10.5F, FontStyle.Bold))
            {
                G.FillRectangle(BrochaMarco, RectDialogo);
                Rectangle IntRect = new Rectangle(RectDialogo.X + 5, RectDialogo.Y + 5, RectDialogo.Width - 10, RectDialogo.Height - 10);
                G.FillRectangle(BrochaInt, IntRect);
                G.DrawRectangle(LapizBorde, IntRect);

                string DescEscenario = CmbEscenarioInicial.SelectedItem != null ? CmbEscenarioInicial.SelectedItem.ToString() : "";
                G.DrawString("PROF. LUCENA: ¡Bienvenido a la Arena Elemental! Primero configura las reglas del encuentro.", FuenteTexto, Brushes.Gold, 72, 510);
                G.DrawString("Escenario seleccionado: " + DescEscenario + " ▼", FuenteTexto, Brushes.White, 72, 535);
            }
        }

        // =========================================================================
        // PANTALLA 3: PERSONALIZACION DE ENTRENADORES Y SELECCION DE 5 CARTAS
        // =========================================================================
        private void ConstruirPantalla3LaboratorioEquipos()
        {
            PnlPantallaEquipos = new PanelDobleBuffer();
            PnlPantallaEquipos.Bounds = new Rectangle(0, 0, 994, 671);
            PnlPantallaEquipos.Paint += new PaintEventHandler(PnlPantallaEquipos_Paint);

            // Seccion 1: Nombres de Entrenadores y Estadisticas SQL
            GroupBox GrpEntrenadores = new GroupBox
            {
                Text = "1. REGISTRO DE ENTRENADORES Y RANKING HISTORICO (BASE DE DATOS)",
                Bounds = new Rectangle(25, 80, 940, 100),
                BackColor = Color.FromArgb(248, 244, 216),
                ForeColor = Color.FromArgb(35, 45, 35),
                Font = new Font("Consolas", 9.5F, FontStyle.Bold)
            };

            Label LblN1 = new Label { Text = "Entrenador 1:", Bounds = new Rectangle(15, 28, 110, 22), ForeColor = Color.Black };
            TxtUsuarioUno = new TextBox { Text = "JugadorUno", Bounds = new Rectangle(125, 25, 155, 24) };
            LblEstadisticasUno = new Label { Text = "PJ:0 V:0 D:0", Bounds = new Rectangle(15, 62, 355, 22), ForeColor = Color.DarkGreen };

            Label LblN2 = new Label { Text = "Entrenador 2:", Bounds = new Rectangle(375, 28, 110, 22), ForeColor = Color.Black };
            TxtUsuarioDos = new TextBox { Text = "JugadorDos", Bounds = new Rectangle(485, 25, 155, 24) };
            LblEstadisticasDos = new Label { Text = "PJ:0 V:0 D:0", Bounds = new Rectangle(375, 62, 355, 22), ForeColor = Color.DarkRed };

            BtnAutenticarJugadores = new Button
            {
                Text = "GUARDAR / CARGAR NOMBRES",
                Bounds = new Rectangle(680, 24, 240, 36),
                BackColor = Color.FromArgb(45, 110, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            BtnAutenticarJugadores.Click += new EventHandler(BtnAutenticarJugadores_Click);

            GrpEntrenadores.Controls.Add(LblN1);
            GrpEntrenadores.Controls.Add(TxtUsuarioUno);
            GrpEntrenadores.Controls.Add(LblEstadisticasUno);
            GrpEntrenadores.Controls.Add(LblN2);
            GrpEntrenadores.Controls.Add(TxtUsuarioDos);
            GrpEntrenadores.Controls.Add(LblEstadisticasDos);
            GrpEntrenadores.Controls.Add(BtnAutenticarJugadores);
            PnlPantallaEquipos.Controls.Add(GrpEntrenadores);

            // Seccion 2: Catalogo de 12 Personajes con Vista Previa Estilo Pokedex
            GroupBox GrpPokedex = new GroupBox
            {
                Text = "2. CATALOGO ELEMENTAL (12 CARTAS: HP + DEF + ATK = 100)",
                Bounds = new Rectangle(25, 190, 380, 275),
                BackColor = Color.FromArgb(248, 244, 216),
                ForeColor = Color.FromArgb(35, 45, 35),
                Font = new Font("Consolas", 9.5F, FontStyle.Bold)
            };

            LstCatalogoCartas = new ListBox
            {
                Bounds = new Rectangle(14, 26, 352, 195),
                BackColor = Color.FromArgb(28, 34, 44),
                ForeColor = Color.White,
                Font = new Font("Consolas", 8.5F, FontStyle.Regular)
            };
            LstCatalogoCartas.SelectedIndexChanged += delegate { PnlPantallaEquipos.Invalidate(); };

            BtnAgregarCartaJ1 = new Button
            {
                Text = "+ A EQUIPO 1",
                Bounds = new Rectangle(14, 228, 170, 34),
                BackColor = Color.SeaGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            BtnAgregarCartaJ1.Click += new EventHandler(BtnAgregarCartaJ1_Click);

            BtnAgregarCartaJ2 = new Button
            {
                Text = "+ A EQUIPO 2",
                Bounds = new Rectangle(196, 228, 170, 34),
                BackColor = Color.Firebrick,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            BtnAgregarCartaJ2.Click += new EventHandler(BtnAgregarCartaJ2_Click);

            GrpPokedex.Controls.Add(LstCatalogoCartas);
            GrpPokedex.Controls.Add(BtnAgregarCartaJ1);
            GrpPokedex.Controls.Add(BtnAgregarCartaJ2);
            PnlPantallaEquipos.Controls.Add(GrpPokedex);

            // Seccion 3: Equipo Jugador 1 (5 Cartas)
            GroupBox GrpMazo1 = new GroupBox
            {
                Text = "EQUIPO JUGADOR 1 (5 CARTAS)",
                Bounds = new Rectangle(580, 190, 185, 275),
                BackColor = Color.FromArgb(248, 244, 216),
                ForeColor = Color.DarkGreen,
                Font = new Font("Consolas", 9F, FontStyle.Bold)
            };

            LstMazoJugadorUno = new ListBox
            {
                Bounds = new Rectangle(10, 26, 165, 195),
                BackColor = Color.FromArgb(24, 34, 28),
                ForeColor = Color.LightGreen,
                Font = new Font("Consolas", 8F, FontStyle.Regular)
            };

            BtnMazoAleatorioJ1 = new Button
            {
                Text = "AUTO 5",
                Bounds = new Rectangle(10, 228, 80, 34),
                BackColor = Color.DarkSlateGray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            BtnMazoAleatorioJ1.Click += new EventHandler(BtnMazoAleatorioJ1_Click);

            BtnLimpiarMazoJ1 = new Button
            {
                Text = "VACIAR",
                Bounds = new Rectangle(95, 228, 80, 34),
                BackColor = Color.DimGray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            BtnLimpiarMazoJ1.Click += new EventHandler(BtnLimpiarMazoJ1_Click);

            GrpMazo1.Controls.Add(LstMazoJugadorUno);
            GrpMazo1.Controls.Add(BtnMazoAleatorioJ1);
            GrpMazo1.Controls.Add(BtnLimpiarMazoJ1);
            PnlPantallaEquipos.Controls.Add(GrpMazo1);

            // Seccion 4: Equipo Jugador 2 (5 Cartas)
            GroupBox GrpMazo2 = new GroupBox
            {
                Text = "EQUIPO JUGADOR 2 (5 CARTAS)",
                Bounds = new Rectangle(780, 190, 185, 275),
                BackColor = Color.FromArgb(248, 244, 216),
                ForeColor = Color.DarkRed,
                Font = new Font("Consolas", 9F, FontStyle.Bold)
            };

            LstMazoJugadorDos = new ListBox
            {
                Bounds = new Rectangle(10, 26, 165, 195),
                BackColor = Color.FromArgb(36, 24, 28),
                ForeColor = Color.LightCoral,
                Font = new Font("Consolas", 8F, FontStyle.Regular)
            };

            BtnMazoAleatorioJ2 = new Button
            {
                Text = "AUTO 5",
                Bounds = new Rectangle(10, 228, 80, 34),
                BackColor = Color.DarkSlateGray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            BtnMazoAleatorioJ2.Click += new EventHandler(BtnMazoAleatorioJ2_Click);

            BtnLimpiarMazoJ2 = new Button
            {
                Text = "VACIAR",
                Bounds = new Rectangle(95, 228, 80, 34),
                BackColor = Color.DimGray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            BtnLimpiarMazoJ2.Click += new EventHandler(BtnLimpiarMazoJ2_Click);

            GrpMazo2.Controls.Add(LstMazoJugadorDos);
            GrpMazo2.Controls.Add(BtnMazoAleatorioJ2);
            GrpMazo2.Controls.Add(BtnLimpiarMazoJ2);
            PnlPantallaEquipos.Controls.Add(GrpMazo2);

            // Botones Inferiores de Paso 3
            BtnVolverAModalidad = new Button
            {
                Text = "<< CAMBIAR MODALIDAD / ESCENARIO",
                Bounds = new Rectangle(25, 590, 310, 48),
                BackColor = Color.FromArgb(95, 100, 115),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Consolas", 10F, FontStyle.Bold)
            };
            BtnVolverAModalidad.Click += delegate { CambiarEtapaMenu(2); };

            BtnIniciarCombate = new Button
            {
                Text = "¡ENTRAR A LA ARENA ELEMENTAL (INICIAR BATALLA)! >>",
                Bounds = new Rectangle(355, 590, 610, 48),
                BackColor = Color.FromArgb(220, 60, 30),
                ForeColor = Color.Yellow,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Consolas", 12F, FontStyle.Bold)
            };
            BtnIniciarCombate.Click += new EventHandler(BtnIniciarCombate_Click);

            PnlPantallaEquipos.Controls.Add(BtnVolverAModalidad);
            PnlPantallaEquipos.Controls.Add(BtnIniciarCombate);
            this.Controls.Add(PnlPantallaEquipos);
        }

        private void PnlPantallaEquipos_Paint(object Sender, PaintEventArgs Args)
        {
            Graphics G = Args.Graphics;
            G.SmoothingMode = SmoothingMode.AntiAlias;
            int W = PnlPantallaEquipos.Width;
            int H = PnlPantallaEquipos.Height;

            // Fondo Azul Laboratorio GBA
            using (LinearGradientBrush Fondo = new LinearGradientBrush(
                new Rectangle(0, 0, W, H), Color.FromArgb(24, 48, 72), Color.FromArgb(14, 26, 40), LinearGradientMode.Vertical))
            {
                G.FillRectangle(Fondo, 0, 0, W, H);
            }

            // Cabecera Roja GBA
            G.FillRectangle(new SolidBrush(Color.FromArgb(210, 55, 40)), 0, 0, W, 68);
            G.DrawLine(new Pen(Color.Gold, 4), 0, 68, W, 68);

            using (Font FuenteTit = new Font("Consolas", 15.5F, FontStyle.Bold))
            using (Font FuenteSub = new Font("Consolas", 9.5F, FontStyle.Bold))
            {
                G.DrawString("PASO 2 DE 2: PERSONALIZACION DE ENTRENADORES Y EQUIPO DE 5 CARTAS", FuenteTit, Brushes.White, 25, 14);
                G.DrawString("El 1er personaje elegido inicia como ACTIVO y los otros 4 entran a la Cola de Relevos (Queue<Personaje>).", FuenteSub, Brushes.Yellow, 25, 42);
            }

            // Vista Previa Holografica de la Carta Seleccionada en el Centro (entre el Catalogo y los Mazos)
            int IndiceSel = LstCatalogoCartas.SelectedIndex;
            if (IndiceSel >= 0 && IndiceSel < CatalogoCartas.Count)
            {
                Personaje CartaVista = CatalogoCartas[IndiceSel];
                DibujarVistaPreviaCartaGba(G, CartaVista, 418, 195);
            }

            // Caja Inferior de Descripcion de Habilidad Especial Estilo GBA
            Rectangle RectDialogo = new Rectangle(25, 478, 940, 96);
            using (SolidBrush BrochaMarco = new SolidBrush(Color.FromArgb(210, 55, 45)))
            using (SolidBrush BrochaInt = new SolidBrush(Color.FromArgb(24, 40, 56)))
            using (Pen LapizBorde = new Pen(Color.FromArgb(240, 225, 160), 3))
            using (Font FuenteTexto = new Font("Consolas", 9.5F, FontStyle.Bold))
            {
                G.FillRectangle(BrochaMarco, RectDialogo);
                Rectangle IntRect = new Rectangle(RectDialogo.X + 5, RectDialogo.Y + 5, RectDialogo.Width - 10, RectDialogo.Height - 10);
                G.FillRectangle(BrochaInt, IntRect);
                G.DrawRectangle(LapizBorde, IntRect);

                if (IndiceSel >= 0 && IndiceSel < CatalogoCartas.Count)
                {
                    Personaje C = CatalogoCartas[IndiceSel];
                    string Linea1 = string.Format("FICHA POKEDEX ELEMENTAL -> {0} [{1}]  |  Suma de Balance: HP({2}) + DEF({3}) + ATK({4}) = 100",
                        C.Nombre.ToUpper(), C.Faccion.ToString().ToUpper(), C.VidaMaxima, C.DefensaBase, C.AtaqueBase);
                    string Linea2 = string.Format("HABILIDAD ESPECIAL [{0}]: {1} (Empatia: {2})",
                        C.NombreEspecial, C.DescripcionEspecial, C.Empatia);
                    string Linea3 = string.Format("Estado de Equipos -> {0}: {1}/5 cartas listas   |   {2}: {3}/5 cartas listas",
                        PerfilUno.NombreUsuario, PerfilUno.MazoSeleccionado.Count, PerfilDos.NombreUsuario, PerfilDos.MazoSeleccionado.Count);

                    G.DrawString(Linea1, FuenteTexto, Brushes.Gold, 40, 490);
                    G.DrawString(Linea2, FuenteTexto, Brushes.White, 40, 515);
                    G.DrawString(Linea3, FuenteTexto, Brushes.LightGreen, 40, 540);
                }
            }
        }

        private void DibujarVistaPreviaCartaGba(Graphics G, Personaje Carta, int X, int Y)
        {
            int Ancho = 150;
            int Alto = 270;
            int Flotacion = (int)(Math.Sin(FrameMenu * 0.12) * 4);
            Y += Flotacion;

            Color ColorElemento;
            if (Carta.Faccion == Elemento.Fuego) ColorElemento = Color.FromArgb(220, 60, 40);
            else if (Carta.Faccion == Elemento.Agua) ColorElemento = Color.FromArgb(40, 120, 220);
            else if (Carta.Faccion == Elemento.Tierra) ColorElemento = Color.FromArgb(150, 105, 50);
            else ColorElemento = Color.FromArgb(55, 175, 145);

            Rectangle RectTarjeta = new Rectangle(X, Y, Ancho, Alto);
            using (SolidBrush Sombra = new SolidBrush(Color.FromArgb(110, 0, 0, 0)))
            using (SolidBrush Fondo = new SolidBrush(Color.FromArgb(30, 36, 48)))
            using (Pen Borde = new Pen(Color.Gold, 3))
            {
                G.FillRectangle(Sombra, X + 6, Y + 6, Ancho, Alto);
                G.FillRectangle(Fondo, RectTarjeta);
                G.DrawRectangle(Borde, RectTarjeta);
            }

            using (SolidBrush Cabecera = new SolidBrush(ColorElemento))
            using (Font FuenteCab = new Font("Consolas", 9.5F, FontStyle.Bold))
            {
                G.FillRectangle(Cabecera, X + 4, Y + 4, Ancho - 8, 28);
                G.DrawString(Carta.Faccion.ToString().ToUpper(), FuenteCab, Brushes.White, X + 10, Y + 10);
            }

            // Icono Elemental Central
            int CX = X + (Ancho / 2);
            int CY = Y + 90;
            using (SolidBrush BrochaIcono = new SolidBrush(ColorElemento))
            using (Pen BordeBlanco = new Pen(Color.White, 2))
            {
                G.FillEllipse(BrochaIcono, CX - 36, CY - 36, 72, 72);
                G.DrawEllipse(BordeBlanco, CX - 36, CY - 36, 72, 72);
            }

            using (Font FuenteStats = new Font("Consolas", 8.5F, FontStyle.Bold))
            {
                G.DrawString(Carta.Nombre, FuenteStats, Brushes.Gold, new RectangleF(X + 6, Y + 135, Ancho - 12, 34));
                G.DrawString("VIDA (HP) : " + Carta.VidaMaxima, FuenteStats, Brushes.LightGreen, X + 10, Y + 175);
                G.DrawString("ESCUDO    : " + Carta.DefensaBase, FuenteStats, Brushes.LightSkyBlue, X + 10, Y + 195);
                G.DrawString("ATAQUE    : " + Carta.AtaqueBase, FuenteStats, Brushes.Orange, X + 10, Y + 215);
                G.DrawString("EMPATIA   : " + Carta.Empatia, FuenteStats, Brushes.Plum, X + 10, Y + 235);
            }
        }

        // =========================================================================
        // LOGICA DE CARGA Y EVENTOS DE CONTROLES
        // =========================================================================
        private void CargarDatosInicialesEnControles()
        {
            LstCatalogoCartas.Items.Clear();
            foreach (Personaje Carta in CatalogoCartas)
            {
                LstCatalogoCartas.Items.Add(string.Format("#{0:00} {1} [{2}] ({3}/{4}/{5})",
                    Carta.IdPersonaje, Carta.Nombre, Carta.Faccion, Carta.VidaMaxima, Carta.DefensaBase, Carta.AtaqueBase));
            }
            if (LstCatalogoCartas.Items.Count > 0)
                LstCatalogoCartas.SelectedIndex = 0;

            CmbEscenarioInicial.Items.Clear();
            foreach (Escenario Esc in CatalogoEscenarios)
            {
                CmbEscenarioInicial.Items.Add(Esc);
            }
            if (CmbEscenarioInicial.Items.Count > 0)
                CmbEscenarioInicial.SelectedIndex = 0;

            AutenticarPerfilesActuales();
            GenerarMazoCincoCartas(PerfilUno, LstMazoJugadorUno, 0);
            GenerarMazoCincoCartas(PerfilDos, LstMazoJugadorDos, 3);
        }

        private void AutenticarPerfilesActuales()
        {
            PerfilUno = GestorDatos.AutenticarORegistrarJugador(TxtUsuarioUno.Text.Trim(), "1234");
            PerfilDos = GestorDatos.AutenticarORegistrarJugador(TxtUsuarioDos.Text.Trim(), "1234");
            ActualizarEtiquetasEstadisticas();
        }

        private void ActualizarEtiquetasEstadisticas()
        {
            LblEstadisticasUno.Text = string.Format("{0} -> PJ:{1} | V:{2} | D:{3} | Exito:{4}%",
                PerfilUno.NombreUsuario, PerfilUno.PartidasJugadas, PerfilUno.Victorias, PerfilUno.Derrotas, PerfilUno.PorcentajeExito);
            LblEstadisticasDos.Text = string.Format("{0} -> PJ:{1} | V:{2} | D:{3} | Exito:{4}%",
                PerfilDos.NombreUsuario, PerfilDos.PartidasJugadas, PerfilDos.Victorias, PerfilDos.Derrotas, PerfilDos.PorcentajeExito);
        }

        private void GenerarMazoCincoCartas(PerfilJugador Perfil, ListBox ListaVisual, int DesplazamientoInicial)
        {
            Perfil.MazoSeleccionado.Clear();
            ListaVisual.Items.Clear();

            for (int I = 0; I < 5; I++)
            {
                int Indice = (DesplazamientoInicial + (I * 2)) % CatalogoCartas.Count;
                Personaje Elegido = CatalogoCartas[Indice].ClonarParaBatalla();
                Perfil.MazoSeleccionado.Add(Elegido);
                string Rol = (I == 0) ? "[LIDER]" : "[COLA " + I + "]";
                ListaVisual.Items.Add(string.Format("{0} {1}", Rol, Elegido.Nombre));
            }
        }

        private void BtnAutenticarJugadores_Click(object Sender, EventArgs Args)
        {
            List<Personaje> MazoPrevio1 = new List<Personaje>(PerfilUno.MazoSeleccionado);
            List<Personaje> MazoPrevio2 = new List<Personaje>(PerfilDos.MazoSeleccionado);

            AutenticarPerfilesActuales();
            PerfilUno.MazoSeleccionado = MazoPrevio1;
            PerfilDos.MazoSeleccionado = MazoPrevio2;
            PnlPantallaEquipos.Invalidate();
        }

        private void BtnAgregarCartaJ1_Click(object Sender, EventArgs Args)
        {
            AgregarCartaSeleccionadaAMazo(PerfilUno, LstMazoJugadorUno);
        }

        private void BtnAgregarCartaJ2_Click(object Sender, EventArgs Args)
        {
            AgregarCartaSeleccionadaAMazo(PerfilDos, LstMazoJugadorDos);
        }

        private void AgregarCartaSeleccionadaAMazo(PerfilJugador Perfil, ListBox ListaVisual)
        {
            if (LstCatalogoCartas.SelectedIndex < 0) return;

            if (Perfil.MazoSeleccionado.Count >= 5)
            {
                MessageBox.Show("El equipo ya tiene el maximo de 5 cartas permitidas.", "Equipo Completo (5/5)", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Personaje CartaBase = CatalogoCartas[LstCatalogoCartas.SelectedIndex];
            Perfil.MazoSeleccionado.Add(CartaBase.ClonarParaBatalla());

            int Posicion = Perfil.MazoSeleccionado.Count;
            string EtiquetaPosicion = (Posicion == 1) ? "[LIDER]" : "[COLA " + (Posicion - 1) + "]";
            ListaVisual.Items.Add(string.Format("{0} {1}", EtiquetaPosicion, CartaBase.Nombre));
            PnlPantallaEquipos.Invalidate();
        }

        private void BtnLimpiarMazoJ1_Click(object Sender, EventArgs Args)
        {
            PerfilUno.MazoSeleccionado.Clear();
            LstMazoJugadorUno.Items.Clear();
            PnlPantallaEquipos.Invalidate();
        }

        private void BtnLimpiarMazoJ2_Click(object Sender, EventArgs Args)
        {
            PerfilDos.MazoSeleccionado.Clear();
            LstMazoJugadorDos.Items.Clear();
            PnlPantallaEquipos.Invalidate();
        }

        private void BtnMazoAleatorioJ1_Click(object Sender, EventArgs Args)
        {
            Random Rnd = new Random(Environment.TickCount);
            GenerarMazoCincoCartas(PerfilUno, LstMazoJugadorUno, Rnd.Next(0, CatalogoCartas.Count));
            PnlPantallaEquipos.Invalidate();
        }

        private void BtnMazoAleatorioJ2_Click(object Sender, EventArgs Args)
        {
            Random Rnd = new Random(Environment.TickCount + 77);
            GenerarMazoCincoCartas(PerfilDos, LstMazoJugadorDos, Rnd.Next(0, CatalogoCartas.Count));
            PnlPantallaEquipos.Invalidate();
        }

        private void BtnHospedarPartida_Click(object Sender, EventArgs Args)
        {
            int Puerto = 5000;
            int.TryParse(TxtPuertoRed.Text, out Puerto);
            RbModoRedLan.Checked = true;
            LblEstadoRed.Text = GestorRed.IniciarServidor(Puerto);
        }

        private void BtnConectarPartida_Click(object Sender, EventArgs Args)
        {
            int Puerto = 5000;
            int.TryParse(TxtPuertoRed.Text, out Puerto);
            RbModoRedLan.Checked = true;
            LblEstadoRed.Text = GestorRed.ConectarAServidor(TxtIpServidor.Text.Trim(), Puerto);
        }

        private void BtnIniciarCombate_Click(object Sender, EventArgs Args)
        {
            // Sincronizar nombres escritos en las cajas de texto antes de iniciar
            if (!string.IsNullOrEmpty(TxtUsuarioUno.Text.Trim()))
                PerfilUno.NombreUsuario = TxtUsuarioUno.Text.Trim();
            if (!string.IsNullOrEmpty(TxtUsuarioDos.Text.Trim()))
                PerfilDos.NombreUsuario = TxtUsuarioDos.Text.Trim();

            if (PerfilUno.MazoSeleccionado.Count != 5 || PerfilDos.MazoSeleccionado.Count != 5)
            {
                MessageBox.Show("Ambos entrenadores deben seleccionar exactamente 5 personajes en su equipo antes de entrar a la Arena.",
                    "Equipo Incompleto", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Escenario EscenarioElegido = (Escenario)CmbEscenarioInicial.SelectedItem;
            bool Alternar = ChkEscenarioAlternante.Checked;
            bool ContraCpu = RbModoContraCpu.Checked;

            MotorPartida NuevoMotor = new MotorPartida(
                PerfilUno,
                PerfilDos,
                CatalogoEscenarios,
                EscenarioElegido,
                Alternar,
                ContraCpu);

            TimerAnimacionMenu.Stop();
            FormTableroBatalla VentanaBatalla = new FormTableroBatalla(NuevoMotor, GestorDatos, GestorRed, RbModoRedLan.Checked);
            VentanaBatalla.ShowDialog();
            TimerAnimacionMenu.Start();

            ActualizarEtiquetasEstadisticas();
        }
    }
}
