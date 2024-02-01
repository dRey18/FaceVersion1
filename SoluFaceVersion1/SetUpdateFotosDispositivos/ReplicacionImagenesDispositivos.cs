using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;

using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
//using System.Windows.Forms;

namespace SetUpdateFotosDispositivos
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
            timer1.Interval = int.Parse(ConfigurationManager.AppSettings["Timer"].ToString());
        }

        protected override void OnStart(string[] args)
        {
            //timer1.Start();
            Timer timer = new Timer();
            timer.Interval = 60000; // 60 seconds
            timer.Elapsed += new ElapsedEventHandler(this.timer1_Tick);
            timer.Start();

            EventLog.WriteEntry("Inicio de proceso de Actualizacion de Biometros", EventLogEntryType.Information);

        }

        protected override void OnStop()
        {
            timer1.Stop();
            EventLog.WriteEntry("Se detuvo el proceso de Actualizacion de Biometros", EventLogEntryType.Information);
        }

        private List<string> ObtenerCodigoEmpleadoReplicar()
        {

            List<string> codigosEmpleado = new List<string>();

            try
            {
                //cadena de conn
                string Servidor = ConfigurationManager.AppSettings["Servidor"].ToString();
                string BaseDatos = ConfigurationManager.AppSettings["BaseDatos"].ToString();
                string Usuario_Login_DB = ConfigurationManager.AppSettings["Usuario_Login_DB"].ToString();
                string Password_DB = ConfigurationManager.AppSettings["Password_DB"].ToString();

                string connectionString = ObtenerConnectionStringDesdeArchivo(Servidor, BaseDatos, Usuario_Login_DB, Password_DB);

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string qry = "SELECT CodigoEmpleado FROM REP_ImagenEmpleadoRondaSeg";

                    using (SqlCommand command = new SqlCommand(qry, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string codigo = reader.GetString(reader.GetOrdinal("CodigoEmpleado"));
                                codigosEmpleado.Add(codigo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {


                EventLog.WriteEntry("Error al obtener el codigo de empleado: " + ex.Message, EventLogEntryType.Error);
            }
            return codigosEmpleado;

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //timer1.Stop();

            try
            {
                EventLog.WriteEntry("Inicio de proceso de Actualizacion de Biometros", EventLogEntryType.Information);

                //Obtener el listado de los dispositivos desde la DB
                var ListaDispositivos = GetDeviceList();
                // Obtener la lista de códigos de empleado desde la base de datos
                List<string> codigosEmpleado = ObtenerCodigoEmpleadoReplicar();


                //Iteramos sobre cada uno de los relojes disponibles
                for (int dispositivoBiometrico = 0; dispositivoBiometrico < ListaDispositivos.Count; dispositivoBiometrico++)
                {
                    // Iterar sobre cada código de empleado y crear usuarios en el reloj
                    foreach (string codigoEmpleado in codigosEmpleado)
                    {
                        crearUsuarioEnReloj(ListaDispositivos[dispositivoBiometrico].DireccionIP, ListaDispositivos[dispositivoBiometrico].Puerto.ToString(),
                            ListaDispositivos[dispositivoBiometrico].Usuario, ListaDispositivos[dispositivoBiometrico].Clave, codigoEmpleado, "NombreGeneral");
                    }

                    //Actualizar la imagen de cada uno de los empleados a cada dispositivo 
                    List<string> file_list = new List<string>();
                    //hacemos uso de la funcion "searchDirectory, para hacer referencia la ruta completa
                    SearchDirectory(ConfigurationManager.AppSettings["DirectorioPicture"].ToString(), file_list);


                    //iteramos sobre la lista de dispositivos pasandole cada una de las imagenes disponibles
                    for (int imagenLocal = 0; imagenLocal < file_list.Count; imagenLocal++)
                    {
                        FileInfo file = new FileInfo(file_list[imagenLocal]);
                        Actualizar_ImagenesDispositivos(ListaDispositivos[dispositivoBiometrico].DireccionIP, ListaDispositivos[dispositivoBiometrico].Puerto.ToString(),
                            ListaDispositivos[dispositivoBiometrico].Usuario, ListaDispositivos[dispositivoBiometrico].Clave, file.Name.Replace(".jpg", ""), file_list[imagenLocal]);
                    }
                    EventLog.WriteEntry("Fin de proceso de Actualizacion de Biometros", EventLogEntryType.Information);

                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ex.Message, EventLogEntryType.Information);
            }
            // timer1.Start();
            EventLog.WriteEntry("Se actualizo correctamente la replicacion de imagenes", EventLogEntryType.Information);
        }


        private List<DispositivoBiometrico> GetDeviceList()
        {
            //Obtenemos la lista de dispositivos
            List<DispositivoBiometrico> deviceList = new List<DispositivoBiometrico>();

            try
            {
                //cadena de conn
                string Servidor = ConfigurationManager.AppSettings["Servidor"].ToString();
                string BaseDatos = ConfigurationManager.AppSettings["BaseDatos"].ToString();
                string Usuario_Login_DB = ConfigurationManager.AppSettings["Usuario_Login_DB"].ToString();
                string Password_DB = ConfigurationManager.AppSettings["Password_DB"].ToString();

                string connectionString = ObtenerConnectionStringDesdeArchivo(Servidor, BaseDatos, Usuario_Login_DB, Password_DB);

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string selectQry = "SELECT * FROM EQ_DispositivoBiometricoRondaSeg";

                    using (SqlCommand command = new SqlCommand(selectQry, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Guid idDispositivo = reader.GetGuid(reader.GetOrdinal("IdDispositivo"));
                                string nombre = reader.GetString(reader.GetOrdinal("Nombre"));
                                string direccionIP = reader.GetString(reader.GetOrdinal("DireccionIP"));
                                int puerto = reader.GetInt32(reader.GetOrdinal("Puerto"));
                                int noPuerta = reader.GetInt32(reader.GetOrdinal("NoPuerta"));
                                string noPlan = reader.GetString(reader.GetOrdinal("NoPlan"));
                                string usuario = reader.GetString(reader.GetOrdinal("Usuario"));
                                string clave = reader.GetString(reader.GetOrdinal("Clave"));
                                string tipoUsuario = reader.GetString(reader.GetOrdinal("TipoUsuario"));
                                bool asistencia = reader.GetBoolean(reader.GetOrdinal("Asistencia"));

                                DispositivoBiometrico device = new DispositivoBiometrico
                                {
                                    IdDispositivo = idDispositivo,
                                    Nombre = nombre,
                                    DireccionIP = direccionIP,
                                    Puerto = puerto,
                                    Usuario = usuario,
                                    Clave = clave
                                };

                                deviceList.Add(device);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Error al obtener la lista de dispositivos: " + ex.Message, EventLogEntryType.Error);
            }

            return deviceList;
        }

        private string ObtenerConnectionStringDesdeArchivo(string G_Servidor, string G_BaseDatos, string G_Usuario_Login_DB, string G_Password_DB)
        {
            string ConectionString = @"Data Source = " + G_Servidor + "; Initial Catalog = " + G_BaseDatos + "; User ID = " + G_Usuario_Login_DB + "; Password = " + G_Password_DB;


            EventLog.WriteEntry("Se obtuvo correctamente la base de datos", EventLogEntryType.Information);
            return ConectionString;

        }


        private void crearUsuarioEnReloj(string pIPDispositivo, string pPuertoDispositivo, string pUsuarioDispositivo, string pContraseñaDispositivo, string pCodigoEmpleado, string pNombreEmpleado)
        {
            try
            {
                string szUrl = "/ISAPI/AccessControl/UserInfo/SetUp?format=json";
                string szRequest = "{\"UserInfo\":{\"employeeNo\":\"" + pCodigoEmpleado +
                    "\",\"name\":\"" + pNombreEmpleado +
                    "\",\"userType\":\"normal\",\"Valid\":{\"enable\":true,\"beginTime\":\"2017-08-01T17:30:08\",\"endTime\":\"2020-08-01T17:30:08\"},\"doorRight\": \"1\",\"RightPlan\":[{\"doorNo\":1,\"planTemplateNo\":\"" + "1" + "\"}]}}";
                string szMethod = "PUT";

                string szResponse = ActionISAPI(pIPDispositivo, pPuertoDispositivo, pUsuarioDispositivo, pContraseñaDispositivo, szUrl, szRequest, szMethod);

                if (szResponse != string.Empty)
                {
                    ResponseStatus rs = JsonConvert.DeserializeObject<ResponseStatus>(szResponse);
                    if (1 == rs.statusCode)
                    {
                        EventLog.WriteEntry("Set UserInfo Succ!");
                    }
                    else
                    {
                        EventLog.WriteEntry("No se pudo crear el usuario!", EventLogEntryType.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Error en la replicacion de usuario: " + ex.Message, EventLogEntryType.Information);
            }
        }


        /// <summary>
        /// Parametros que se utilizan para guardar la informaicon en cada reloj
        /// </summary>
        /// <param name="pIPDispositivo"> Id del reloj actual</param>
        /// <param name="pPuertoDispositivo">puerto para establecer la conn</param>
        /// <param name="pUsuarioDispositivo">usuario del reloj</param>
        /// <param name="pContraseñaDispositivo">contraseña del reloj</param>
        /// <param name="pEmployeeNo">numero de empleado(imagen a cargar)</param>
        /// <param name="pFilePath">ruta de imagen</param>
        private void Actualizar_ImagenesDispositivos(string pIPDispositivo, string pPuertoDispositivo, string pUsuarioDispositivo, string pContraseñaDispositivo, string pEmployeeNo, string pFilePath)
        {

            try
            {
                string szUrl = "/ISAPI/AccessControl/UserInfo/Search?format=json";
                string szResponse = string.Empty;
                string szRequest = "{\"UserInfoSearchCond\":{\"searchID\":\"1\",\"searchResultPosition\":0,\"maxResults\":30,\"EmployeeNoList\":[{\"employeeNo\":\"" 
                    + pEmployeeNo + "\"}]}}";
                string szMethod = "POST";

                //查询是否存在工号
                //Agregamos los parametros para establecer conexion con cada relj
                szResponse = ActionISAPI(pIPDispositivo, pPuertoDispositivo, pUsuarioDispositivo, pContraseñaDispositivo, szUrl, szRequest, szMethod);

                if (szResponse != string.Empty)
                {
                    UserInfoSearchRoot us = JsonConvert.DeserializeObject<UserInfoSearchRoot>(szResponse);
                    if (0 == us.UserInfoSearch.totalMatches)
                    {
                        EventLog.WriteEntry("Employee No isn't found!", EventLogEntryType.Error);
                        return;
                    }
                }

                //szUrl = "/ISAPI/Intelligent/FDLib?format=json&FDID=" + textBoxFDID.Text + "&faceLibType=" + comboBoxFaceType.SelectedItem.ToString();
                szUrl = "/ISAPI/Intelligent/FDLib?format=json&FDID=" + "1" + "&faceLibType=" + "blackFD";
                szResponse = string.Empty;
                szMethod = "GET";

                //查询FaceLib是否存在
                szResponse = ActionISAPI(pIPDispositivo, pPuertoDispositivo, pUsuarioDispositivo, pContraseñaDispositivo, szUrl, szRequest, szMethod);
                if (szResponse != string.Empty)
                {
                    FaceLib fb = JsonConvert.DeserializeObject<FaceLib>(szResponse);
                    if (fb == null || fb.statusCode != 1)
                    {
                        EventLog.WriteEntry("FaceLib isn't existed!", EventLogEntryType.Error);
                        return;
                    }
                }


                //查询是否已有图片，若有则删除
                szUrl = "/ISAPI/Intelligent/FDLib/FDSearch?format=json";
                szResponse = string.Empty;
                szRequest = "{\"searchResultPosition\":0,\"maxResults\":30,\"faceLibType\":\"" + "blackFD" +
                    "\",\"FDID\":\"" + "1" +
                    "\",\"FPID\":\"" + pEmployeeNo + "\"}";
                szMethod = "POST";

                szResponse = ActionISAPI(pIPDispositivo, pPuertoDispositivo, pUsuarioDispositivo, pContraseñaDispositivo, szUrl, szRequest, szMethod);
                if (szResponse != string.Empty)
                {
                    Root rt = JsonConvert.DeserializeObject<Root>(szResponse);
                    if (rt.statusCode == 1)
                    {
                        if (rt.totalMatches != 0)
                        {
                            szUrl = "/ISAPI/Intelligent/FDLib/FDSearch/Delete?format=json&FDID=" + "1" + "&faceLibType=" + "blackFD" + "";
                            szResponse = string.Empty;
                            szRequest = "{\"FPID\":[{\"value\":\"" + pEmployeeNo + "\"}]}";
                            szMethod = "PUT";

                            szResponse = ActionISAPI(pIPDispositivo, pPuertoDispositivo, pUsuarioDispositivo, pContraseñaDispositivo, szUrl, szRequest, szMethod);
                            if (szResponse != string.Empty)
                            {
                                ResponseStatus rs = JsonConvert.DeserializeObject<ResponseStatus>(szResponse);
                                if (!rs.statusCode.Equals("1"))
                                {
                                    return;
                                }
                            }
                        }
                    }
                }


                szUrl = "/ISAPI/Intelligent/FDLib/FaceDataRecord?format=json";

                if (!szUrl.Substring(0, 4).Equals("http"))
                {
                    szUrl = "http://" + pIPDispositivo + ":" + pPuertoDispositivo + szUrl;
                }
                HttpClient clHttpClient = new HttpClient();
                szResponse = string.Empty;
                szRequest = "{\"faceLibType\":\"" + "blackFD" +
                    "\",\"FDID\":\"" + "1" +
                    "\",\"FPID\":\"" + pEmployeeNo + "\"}";

                string filePath = pFilePath;
                szResponse = clHttpClient.HttpPostData(pUsuarioDispositivo, pContraseñaDispositivo, szUrl, filePath, szRequest);
                ResponseStatus res = JsonConvert.DeserializeObject<ResponseStatus>(szResponse);
                if (res != null && res.statusCode.Equals("1"))
                {
                    EventLog.WriteEntry("Set Picture Succ!", EventLogEntryType.Information);

                    //Obtener el idDispositivo desde la información del dispositivo actual
                    Guid idDispositivo = ObtenerIdDispositivoDesdeLista(pIPDispositivo);

                    if (idDispositivo != Guid.Empty)
                    {
                        InsertarInformacionEnDB(idDispositivo, pEmployeeNo, Environment.UserName);
                    }
                }
            }

            catch (Exception ex)
            {
                EventLog.WriteEntry("Error en la actualización de imágenes: " + ex.Message, EventLogEntryType.Information);
            }
        }

        private Guid ObtenerIdDispositivoDesdeLista(string pIPDispositivo)
        {
            // Implementa la lógica para obtener el IdDispositivo desde la lista de dispositivos según la dirección IP.
            DispositivoBiometrico dispositivo = GetDeviceList().FirstOrDefault(d => d.DireccionIP == pIPDispositivo);

            return dispositivo?.IdDispositivo ?? Guid.Empty;
        }

        private void InsertarInformacionEnDB(Guid idDispositivo, string codigoEmpleado, string creadoPor)
        {
            try
            {
                string Servidor = ConfigurationManager.AppSettings["Servidor"].ToString();
                string BaseDatos = ConfigurationManager.AppSettings["BaseDatos"].ToString();
                string Usuario_Login_DB = ConfigurationManager.AppSettings["Usuario_Login_DB"].ToString();
                string Password_DB = ConfigurationManager.AppSettings["Password_DB"].ToString();

                string connectionString = ObtenerConnectionStringDesdeArchivo(Servidor, BaseDatos, Usuario_Login_DB, Password_DB);

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();


                    // Verificar si ya existe el registro en la base de datos
                    string checkIfExistsQuery = "SELECT COUNT(*) FROM REP_DispositivoBiometricoFacialImagenEmpleadoRondaSeg " +
                                                "WHERE IdDispositivo = @IdDispositivo AND CodigoEmpleado = @CodigoEmpleado";

                    using (SqlCommand checkCommand = new SqlCommand(checkIfExistsQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@IdDispositivo", idDispositivo);
                        checkCommand.Parameters.AddWithValue("@CodigoEmpleado", codigoEmpleado);

                        int existingRecords = (int)checkCommand.ExecuteScalar();

                        // Si ya existe, no hacemos nada
                        if (existingRecords > 0)
                        {
                            EventLog.WriteEntry($"Registro para IdDispositivo: {idDispositivo}, CodigoEmpleado: {codigoEmpleado} ya existe.", EventLogEntryType.Information);
                            return;
                        }
                    }

                    string qry = "INSERT INTO REP_DispositivoBiometricoFacialImagenEmpleadoRondaSeg (IdDispositivo, CodigoEmpleado, CreadoPor, FechaCreacion) " +
                        "VALUES (@IdDispositivo, @CodigoEmpleado, @CreadoPor, GETDATE())";

                    using (SqlCommand command = new SqlCommand(qry, connection))
                    {
                        command.Parameters.AddWithValue("@IdDispositivo", idDispositivo);
                        command.Parameters.AddWithValue("@CodigoEmpleado", codigoEmpleado);
                        command.Parameters.AddWithValue("@CreadoPor", "sistema");

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Error al insertar en la base de datos: " + ex.StackTrace, EventLogEntryType.Information);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="IP"> le pasamos la IP</param>
        /// <param name="Puerto"> puerto de cada reloj</param>
        /// <param name="strUsername">usuario del reloj</param>
        /// <param name="strPassword">contraseña del reloj</param>
        /// <param name="szUrl"></param>
        /// <param name="szRequest"></param>
        /// <param name="szMethod"></param>
        /// <returns></returns>
        private string ActionISAPI(string IP, string Puerto, string strUsername, string strPassword, string szUrl, string szRequest, string szMethod)
        {
            string szResponse = string.Empty;


            if (!szUrl.Substring(0, 4).Equals("http"))
            {
                szUrl = "http://" + IP + ":" + Puerto + szUrl;
            }
            HttpClient clHttpClient = new HttpClient();
            byte[] byResponse = { 0 };
            int iRet = 0;
            string szContentType = string.Empty;

            switch (szMethod)
            {
                case "GET":
                    iRet = clHttpClient.HttpRequest(strUsername, strPassword, szUrl, szMethod, ref byResponse, ref szContentType);
                    break;
                case "PUT":
                    iRet = clHttpClient.HttpPut(strUsername, strPassword, szUrl, szMethod, szRequest, ref szResponse);
                    break;
                case "POST":
                    iRet = clHttpClient.HttpPut(strUsername, strPassword, szUrl, szMethod, szRequest, ref szResponse);
                    break;
                default:
                    break;
            }

            if (iRet == (int)HttpClient.HttpStatus.Http200)
            {
                if ((!szMethod.Equals("GET")) || (szContentType.IndexOf("application/xml") != -1))
                {
                    if (szResponse != string.Empty)
                    {
                        return szResponse;
                    }

                    if (szMethod.Equals("GET"))
                    {
                        szResponse = Encoding.Default.GetString(byResponse);
                        return szResponse;
                    }
                }
                else
                {
                    if (byResponse.Length != 0)
                    {
                        szResponse = Encoding.Default.GetString(byResponse);
                        return szResponse;
                    }
                }
            }
            else if (iRet == (int)HttpClient.HttpStatus.HttpOther)
            {
                string szCode = string.Empty;
                string szError = string.Empty;
                clHttpClient.ParserResponseStatus(szResponse, ref szCode, ref szError);
                EventLog.WriteEntry("Request failed! Error code:" + szCode + " Describe:" + szError + "\r\n", EventLogEntryType.Error);
            }
            else if (iRet == (int)HttpClient.HttpStatus.HttpTimeOut)
            {
                EventLog.WriteEntry(szMethod + " " + szUrl + "error!Time out", EventLogEntryType.Error);
            }
            return szResponse;
        }

        //funcion para obterner la ruta del directorio donde  se encuentran las imagenes
        private void SearchDirectory(string path, List<string> file_list)
        {
            //instanciamos la clase DirectoryInfo para obtener la ruta
            DirectoryInfo dir_info = new DirectoryInfo(path);
            try
            {
                foreach (DirectoryInfo subdir_info in dir_info.GetDirectories())
                {
                    SearchDirectory(subdir_info.FullName, file_list);
                }
            }
            catch (Exception ex)
            {
            }
            try
            {
                //obtenemos todas las imagenes que tengan extension .jpg
                foreach (FileInfo file_info in dir_info.GetFiles())
                {
                    if (file_info.Extension == ".jpg")
                        file_list.Add(file_info.FullName);
                }
            }
            catch
            {

                EventLog.WriteEntry("No se encontro el directorio", EventLogEntryType.Information);

            }
        }
    }
}
