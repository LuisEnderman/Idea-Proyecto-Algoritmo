using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ArenaElemental.Red
{
    public delegate void DelegadoComandoRedRecibido(string ComandoRecibido);

    public class GestorRedSockets
    {
        private TcpListener ServidorEscucha;
        private TcpClient ClienteConectado;
        private NetworkStream FlujoRed;
        private Thread HiloRecepcion;
        private volatile bool EscuchandoActivo;

        public bool EstaConectado { get; private set; }
        public bool EsAnfitrionServidor { get; private set; }
        public event DelegadoComandoRedRecibido AlRecibirComando;

        public string IniciarServidor(int Puerto)
        {
            try
            {
                DetenerConexion();
                EsAnfitrionServidor = true;
                EscuchandoActivo = true;
                ServidorEscucha = new TcpListener(IPAddress.Any, Puerto);
                ServidorEscucha.Start();

                HiloRecepcion = new Thread(BucleAceptarYRecibirServidor);
                HiloRecepcion.IsBackground = true;
                HiloRecepcion.Start();

                return string.Format("Servidor TCP escuchando en el puerto {0}. Esperando jugador remoto...", Puerto);
            }
            catch (Exception Ex)
            {
                EscuchandoActivo = false;
                return "Error al iniciar servidor Socket: " + Ex.Message;
            }
        }

        public string ConectarAServidor(string DireccionIp, int Puerto)
        {
            try
            {
                DetenerConexion();
                EsAnfitrionServidor = false;
                ClienteConectado = new TcpClient();
                ClienteConectado.Connect(DireccionIp, Puerto);
                FlujoRed = ClienteConectado.GetStream();
                EstaConectado = true;
                EscuchandoActivo = true;

                HiloRecepcion = new Thread(BucleLecturaDatos);
                HiloRecepcion.IsBackground = true;
                HiloRecepcion.Start();

                return string.Format("Conectado exitosamente al servidor {0}:{1}.", DireccionIp, Puerto);
            }
            catch (Exception Ex)
            {
                EstaConectado = false;
                return "Error al conectar al servidor Socket: " + Ex.Message;
            }
        }

        private void BucleAceptarYRecibirServidor()
        {
            try
            {
                ClienteConectado = ServidorEscucha.AcceptTcpClient();
                FlujoRed = ClienteConectado.GetStream();
                EstaConectado = true;

                if (AlRecibirComando != null)
                {
                    AlRecibirComando("CONEXION_ESTABLECIDA");
                }

                BucleLecturaDatos();
            }
            catch (Exception)
            {
                EstaConectado = false;
            }
        }

        private void BucleLecturaDatos()
        {
            byte[] BufferLectura = new byte[1024];
            try
            {
                while (EscuchandoActivo && ClienteConectado != null && ClienteConectado.Connected)
                {
                    int BytesLeidos = FlujoRed.Read(BufferLectura, 0, BufferLectura.Length);
                    if (BytesLeidos <= 0)
                        break;

                    string Mensaje = Encoding.UTF8.GetString(BufferLectura, 0, BytesLeidos).Trim();
                    if (!string.IsNullOrEmpty(Mensaje) && AlRecibirComando != null)
                    {
                        AlRecibirComando(Mensaje);
                    }
                }
            }
            catch (Exception)
            {
                EstaConectado = false;
            }
        }

        public bool EnviarComandoAccion(string Comando)
        {
            try
            {
                if (EstaConectado && FlujoRed != null && FlujoRed.CanWrite)
                {
                    byte[] Datos = Encoding.UTF8.GetBytes(Comando + "\n");
                    FlujoRed.Write(Datos, 0, Datos.Length);
                    FlujoRed.Flush();
                    return true;
                }
            }
            catch (Exception)
            {
                EstaConectado = false;
            }
            return false;
        }

        public void DetenerConexion()
        {
            try
            {
                EscuchandoActivo = false;
                EstaConectado = false;
                if (FlujoRed != null) FlujoRed.Close();
                if (ClienteConectado != null) ClienteConectado.Close();
                if (ServidorEscucha != null) ServidorEscucha.Stop();
            }
            catch (Exception)
            {
            }
        }
    }
}
