using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ArenaElemental.Modelos;

namespace ArenaElemental.Datos
{
    public class GestorBaseDatos
    {
        // Cadena de conexion para SQL Server Local / Express (ajustable segun servidor local)
        private string CadenaConexionSql = "Server=localhost;Database=ArenaElementalDB;Integrated Security=True;Connection Timeout=2;";
        public bool ModoConectadoSql { get; private set; }
        public string UltimoMensajeEstado { get; private set; }

        private List<PerfilJugador> JugadoresMemoria;
        private List<Personaje> CatalogoPersonajesMemoria;
        private List<Escenario> CatalogoEscenariosMemoria;

        public GestorBaseDatos()
        {
            ModoConectadoSql = false;
            InicializarCatalogoPorDefecto();
            ProbarConexionInicial();
        }

        private void InicializarCatalogoPorDefecto()
        {
            CatalogoPersonajesMemoria = new List<Personaje>();
            // 12 Cartas de Personaje (3 por faccion, todas suman 100 entre HP + DEF + ATK)
            CatalogoPersonajesMemoria.Add(new CartaFuego(1, "Ignis El Piromante", 45, 20, 35, 40, "Llamarada Solar", "Ignora escudo e inflige danio directo."));
            CatalogoPersonajesMemoria.Add(new CartaFuego(2, "Vulcano Acorazado", 55, 25, 20, 65, "Erupcion De Magma", "Funde el escudo enemigo y golpea."));
            CatalogoPersonajesMemoria.Add(new CartaFuego(3, "Kael Cuchilla Ignea", 40, 20, 40, 30, "Impacto De Ceniza", "Ataque igneo de maxima potencia."));

            CatalogoPersonajesMemoria.Add(new CartaAgua(4, "Nereida Del Abismo", 55, 25, 20, 85, "Marea Sanadora", "Cura 25% HP, roba 1 energia y ataca."));
            CatalogoPersonajesMemoria.Add(new CartaAgua(5, "Kraken Acorazado", 60, 25, 15, 75, "Vortice Abisal", "Drena energia rival y restaura salud."));
            CatalogoPersonajesMemoria.Add(new CartaAgua(6, "Siren Cazadora", 50, 20, 30, 60, "Lanza De Hielo", "Roba energia y golpea con hielo."));

            CatalogoPersonajesMemoria.Add(new CartaTierra(7, "Golem De Basalto", 60, 30, 10, 90, "Muralla Monolitica", "Sube escudo y aplasta con DEF+ATK."));
            CatalogoPersonajesMemoria.Add(new CartaTierra(8, "Terra Guardiana", 55, 25, 20, 80, "Raices Aplastantes", "Refuerza armadura y lanza golpe sismico."));
            CatalogoPersonajesMemoria.Add(new CartaTierra(9, "Rocco Rompemuros", 50, 25, 25, 70, "Martillo Tectonico", "Impacto pesado potenciado por defensa."));

            CatalogoPersonajesMemoria.Add(new CartaAire(10, "Cefiro Maestro", 45, 25, 30, 75, "Tormenta Cortante", "Ejecuta un impacto doble de viento."));
            CatalogoPersonajesMemoria.Add(new CartaAire(11, "Valkiria Celeste", 50, 20, 30, 80, "Rafaga Ascendente", "Doble golpe veloz consecutivo."));
            CatalogoPersonajesMemoria.Add(new CartaAire(12, "Boreas Silencioso", 40, 25, 35, 50, "Huracan Del Norte", "Rafaga doble critica de alta velocidad."));

            // 4 Cartas de Escenario
            CatalogoEscenariosMemoria = new List<Escenario>();
            CatalogoEscenariosMemoria.Add(new Escenario(1, "Volcan Activo", Elemento.Fuego, Elemento.Aire, 6, 4, 70, "Potencia a Fuego (+6) y sofoca a Aire (-4)."));
            CatalogoEscenariosMemoria.Add(new Escenario(2, "Abismo Oceanico", Elemento.Agua, Elemento.Fuego, 6, 4, 65, "Potencia a Agua (+6) y apaga a Fuego (-4)."));
            CatalogoEscenariosMemoria.Add(new Escenario(3, "Canion Rocoso", Elemento.Tierra, Elemento.Agua, 6, 4, 75, "Potencia a Tierra (+6) y contiene a Agua (-4)."));
            CatalogoEscenariosMemoria.Add(new Escenario(4, "Cumbre De Viento", Elemento.Aire, Elemento.Tierra, 6, 4, 70, "Potencia a Aire (+6) y erosiona a Tierra (-4)."));

            JugadoresMemoria = new List<PerfilJugador>();
            JugadoresMemoria.Add(new PerfilJugador(1, "JugadorUno", 4, 3, 1));
            JugadoresMemoria.Add(new PerfilJugador(2, "JugadorDos", 4, 1, 3));
        }

        public void ProbarConexionInicial()
        {
            try
            {
                using (SqlConnection Conexion = new SqlConnection(CadenaConexionSql))
                {
                    Conexion.Open();
                    ModoConectadoSql = true;
                    UltimoMensajeEstado = "Conectado a Base de Datos SQL Server (ArenaElementalDB).";
                }
            }
            catch (Exception Ex)
            {
                ModoConectadoSql = false;
                UltimoMensajeEstado = "Modo Local Activo (Respaldo en Memoria sin servidor SQL activo: " + Ex.Message + ")";
            }
        }

        public List<Personaje> ObtenerCatalogoPersonajes()
        {
            if (ModoConectadoSql)
            {
                try
                {
                    List<Personaje> ListaDesdeBd = new List<Personaje>();
                    using (SqlConnection Conexion = new SqlConnection(CadenaConexionSql))
                    {
                        Conexion.Open();
                        string Consulta = "SELECT IdPersonaje, Nombre, Faccion, VidaBase, DefensaBase, AtaqueBase, Empatia, NombreEspecial, DescripcionEspecial FROM Personajes";
                        using (SqlCommand Comando = new SqlCommand(Consulta, Conexion))
                        using (SqlDataReader Lector = Comando.ExecuteReader())
                        {
                            while (Lector.Read())
                            {
                                int Id = Convert.ToInt32(Lector["IdPersonaje"]);
                                string Nombre = Lector["Nombre"].ToString();
                                string FaccionTexto = Lector["Faccion"].ToString();
                                int Vida = Convert.ToInt32(Lector["VidaBase"]);
                                int Defensa = Convert.ToInt32(Lector["DefensaBase"]);
                                int Ataque = Convert.ToInt32(Lector["AtaqueBase"]);
                                int Empatia = Convert.ToInt32(Lector["Empatia"]);
                                string Especial = Lector["NombreEspecial"].ToString();
                                string Desc = Lector["DescripcionEspecial"].ToString();

                                if (FaccionTexto == "Fuego")
                                    ListaDesdeBd.Add(new CartaFuego(Id, Nombre, Vida, Defensa, Ataque, Empatia, Especial, Desc));
                                else if (FaccionTexto == "Agua")
                                    ListaDesdeBd.Add(new CartaAgua(Id, Nombre, Vida, Defensa, Ataque, Empatia, Especial, Desc));
                                else if (FaccionTexto == "Tierra")
                                    ListaDesdeBd.Add(new CartaTierra(Id, Nombre, Vida, Defensa, Ataque, Empatia, Especial, Desc));
                                else
                                    ListaDesdeBd.Add(new CartaAire(Id, Nombre, Vida, Defensa, Ataque, Empatia, Especial, Desc));
                            }
                        }
                    }
                    if (ListaDesdeBd.Count > 0)
                        return ListaDesdeBd;
                }
                catch (Exception)
                {
                    ModoConectadoSql = false;
                }
            }
            return new List<Personaje>(CatalogoPersonajesMemoria);
        }

        public List<Escenario> ObtenerCatalogoEscenarios()
        {
            return new List<Escenario>(CatalogoEscenariosMemoria);
        }

        public PerfilJugador AutenticarORegistrarJugador(string NombreUsuario, string ClaveAcceso)
        {
            if (string.IsNullOrEmpty(NombreUsuario))
                NombreUsuario = "JugadorAnonimo";

            if (ModoConectadoSql)
            {
                try
                {
                    using (SqlConnection Conexion = new SqlConnection(CadenaConexionSql))
                    {
                        Conexion.Open();
                        string ConsultaBuscar = "SELECT IdJugador, PartidasJugadas, Victorias, Derrotas FROM Jugadores WHERE NombreUsuario = @Usuario";
                        using (SqlCommand Comando = new SqlCommand(ConsultaBuscar, Conexion))
                        {
                            Comando.Parameters.AddWithValue("@Usuario", NombreUsuario);
                            using (SqlDataReader Lector = Comando.ExecuteReader())
                            {
                                if (Lector.Read())
                                {
                                    return new PerfilJugador(
                                        Convert.ToInt32(Lector["IdJugador"]),
                                        NombreUsuario,
                                        Convert.ToInt32(Lector["PartidasJugadas"]),
                                        Convert.ToInt32(Lector["Victorias"]),
                                        Convert.ToInt32(Lector["Derrotas"]));
                                }
                            }
                        }

                        string ConsultaInsertar = "INSERT INTO Jugadores (NombreUsuario, ClaveAcceso, PartidasJugadas, Victorias, Derrotas, PorcentajeExito) OUTPUT INSERTED.IdJugador VALUES (@Usuario, @Clave, 0, 0, 0, 0.00)";
                        using (SqlCommand ComandoIns = new SqlCommand(ConsultaInsertar, Conexion))
                        {
                            ComandoIns.Parameters.AddWithValue("@Usuario", NombreUsuario);
                            ComandoIns.Parameters.AddWithValue("@Clave", ClaveAcceso);
                            int NuevoId = Convert.ToInt32(ComandoIns.ExecuteScalar());
                            return new PerfilJugador(NuevoId, NombreUsuario, 0, 0, 0);
                        }
                    }
                }
                catch (Exception)
                {
                    ModoConectadoSql = false;
                }
            }

            foreach (PerfilJugador Jugador in JugadoresMemoria)
            {
                if (string.Equals(Jugador.NombreUsuario, NombreUsuario, StringComparison.OrdinalIgnoreCase))
                    return Jugador;
            }

            PerfilJugador Nuevo = new PerfilJugador(JugadoresMemoria.Count + 1, NombreUsuario, 0, 0, 0);
            JugadoresMemoria.Add(Nuevo);
            return Nuevo;
        }

        public void ActualizarEstadisticasPostBatalla(PerfilJugador Ganador, PerfilJugador Perdedor, int IdEscenario, int TotalTurnos, string RutaArchivoLog)
        {
            Ganador.RegistrarResultado(true);
            Perdedor.RegistrarResultado(false);

            if (ModoConectadoSql)
            {
                try
                {
                    using (SqlConnection Conexion = new SqlConnection(CadenaConexionSql))
                    {
                        Conexion.Open();
                        string SqlUpdate = @"UPDATE Jugadores 
                                             SET PartidasJugadas = @Partidas, Victorias = @Victorias, Derrotas = @Derrotas, PorcentajeExito = @Porcentaje 
                                             WHERE IdJugador = @IdJugador";

                        using (SqlCommand CmdGanador = new SqlCommand(SqlUpdate, Conexion))
                        {
                            CmdGanador.Parameters.AddWithValue("@Partidas", Ganador.PartidasJugadas);
                            CmdGanador.Parameters.AddWithValue("@Victorias", Ganador.Victorias);
                            CmdGanador.Parameters.AddWithValue("@Derrotas", Ganador.Derrotas);
                            CmdGanador.Parameters.AddWithValue("@Porcentaje", Ganador.PorcentajeExito);
                            CmdGanador.Parameters.AddWithValue("@IdJugador", Ganador.IdJugador);
                            CmdGanador.ExecuteNonQuery();
                        }

                        using (SqlCommand CmdPerdedor = new SqlCommand(SqlUpdate, Conexion))
                        {
                            CmdPerdedor.Parameters.AddWithValue("@Partidas", Perdedor.PartidasJugadas);
                            CmdPerdedor.Parameters.AddWithValue("@Victorias", Perdedor.Victorias);
                            CmdPerdedor.Parameters.AddWithValue("@Derrotas", Perdedor.Derrotas);
                            CmdPerdedor.Parameters.AddWithValue("@Porcentaje", Perdedor.PorcentajeExito);
                            CmdPerdedor.Parameters.AddWithValue("@IdJugador", Perdedor.IdJugador);
                            CmdPerdedor.ExecuteNonQuery();
                        }

                        string SqlInsertPartida = @"INSERT INTO EstadisticasPartidas (IdJugadorUno, IdJugadorDos, IdGanador, IdEscenario, TotalTurnos, ArchivoLog)
                                                    VALUES (@J1, @J2, @Ganador, @Escenario, @Turnos, @Log)";
                        using (SqlCommand CmdPartida = new SqlCommand(SqlInsertPartida, Conexion))
                        {
                            CmdPartida.Parameters.AddWithValue("@J1", Ganador.IdJugador);
                            CmdPartida.Parameters.AddWithValue("@J2", Perdedor.IdJugador);
                            CmdPartida.Parameters.AddWithValue("@Ganador", Ganador.IdJugador);
                            CmdPartida.Parameters.AddWithValue("@Escenario", IdEscenario);
                            CmdPartida.Parameters.AddWithValue("@Turnos", TotalTurnos);
                            CmdPartida.Parameters.AddWithValue("@Log", RutaArchivoLog);
                            CmdPartida.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception)
                {
                    ModoConectadoSql = false;
                }
            }
        }

        public List<PerfilJugador> ObtenerRankingJugadores()
        {
            return new List<PerfilJugador>(JugadoresMemoria);
        }
    }
}
