using System;
using System.Collections.Generic;
using System.IO;
using ArenaElemental.Modelos;

namespace ArenaElemental.Datos
{
    public class GestorArchivosLog
    {
        public static string ExportarBitacoraTxt(
            PerfilJugador JugadorUno,
            PerfilJugador JugadorDos,
            PerfilJugador Ganador,
            Escenario EscenarioFinal,
            int TotalTurnos,
            Stack<string> PilaEventos)
        {
            string NombreArchivo = string.Format("Bitacora_ArenaElemental_{0}.txt", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            string RutaCompleta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, NombreArchivo);

            try
            {
                using (StreamWriter Escritor = new StreamWriter(RutaCompleta, false, System.Text.Encoding.UTF8))
                {
                    Escritor.WriteLine("======================================================================");
                    Escritor.WriteLine("          BITACORA OFICIAL DE COMBATE - ARENA ELEMENTAL               ");
                    Escritor.WriteLine("======================================================================");
                    Escritor.WriteLine("Fecha y Hora      : " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                    Escritor.WriteLine("Jugador 1         : " + JugadorUno.NombreUsuario);
                    Escritor.WriteLine("Jugador 2         : " + JugadorDos.NombreUsuario);
                    Escritor.WriteLine("Vencedor          : " + Ganador.NombreUsuario);
                    Escritor.WriteLine("Escenario Final   : " + EscenarioFinal.Nombre);
                    Escritor.WriteLine("Total de Turnos   : " + TotalTurnos);
                    Escritor.WriteLine("----------------------------------------------------------------------");
                    Escritor.WriteLine("HISTORIAL CRONOLOGICO DE ACCIONES (DESDE EL INICIO HASTA EL FINAL):");
                    Escritor.WriteLine("----------------------------------------------------------------------");

                    // Convertimos la Pila (LIFO) en arreglo e invertimos para imprimir en orden cronologico
                    string[] ArregloEventos = PilaEventos.ToArray();
                    Array.Reverse(ArregloEventos);

                    for (int Indice = 0; Indice < ArregloEventos.Length; Indice++)
                    {
                        Escritor.WriteLine(ArregloEventos[Indice]);
                    }

                    Escritor.WriteLine("======================================================================");
                    Escritor.WriteLine("ESTADISTICAS ACTUALIZADAS:");
                    Escritor.WriteLine(string.Format(" - {0}: PJ={1} | V={2} | D={3} | Exito={4}%",
                        JugadorUno.NombreUsuario, JugadorUno.PartidasJugadas, JugadorUno.Victorias, JugadorUno.Derrotas, JugadorUno.PorcentajeExito));
                    Escritor.WriteLine(string.Format(" - {0}: PJ={1} | V={2} | D={3} | Exito={4}%",
                        JugadorDos.NombreUsuario, JugadorDos.PartidasJugadas, JugadorDos.Victorias, JugadorDos.Derrotas, JugadorDos.PorcentajeExito));
                    Escritor.WriteLine("======================================================================");
                }
            }
            catch (Exception Ex)
            {
                return "Error al guardar el archivo TXT: " + Ex.Message;
            }

            return RutaCompleta;
        }
    }
}
