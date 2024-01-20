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
            timer1.Start();

        }

        protected override void OnStop()
        { 
            timer1.Stop();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            try
            {
                EventLog.WriteEntry("Inicio de proceso de Actualizacion de Biometros", EventLogEntryType.Information);
                //Obtener el listado de los dispositivos
                var ListaDispositivos = GetDeviceList();

                for (int i = 0; i < ListaDispositivos.Count; i++)
                {
                    //Actualizar la imagen de cada uno de los empleados a cada dispositivo 
                    List<string> file_list = new List<string>();
                    SearchDirectory(ConfigurationManager.AppSettings["DirectorioPicture"].ToString(), file_list);

                    for (int j = 0; j < file_list.Count; j++)
                    {
                        FileInfo file = new FileInfo (file_list[j]);
                        Actualizar_ImagenesDispositivos(ListaDispositivos[i].DireccionIP, ListaDispositivos[i].Puerto.ToString(), ListaDispositivos[i].Usuario, ListaDispositivos[i].Clave, file.Name.Replace(".jpg", ""), file_list[j]);

                    }
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
            }
            timer1.Start();
        }


        private List<DispositivoBiometrico> GetDeviceList()
        {
            List<DispositivoBiometrico> deviceList = new List<DispositivoBiometrico>();

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
            return ConectionString;
        }

        private void Actualizar_ImagenesDispositivos(string pIPDispositivo, string pPuertoDispositivo, string pUsuarioDispositivo, string pContraseñaDispositivo, string pEmployeeNo, string pFilePath) 
        {
            string szUrl = "/ISAPI/AccessControl/UserInfo/Search?format=json";
            string szResponse = string.Empty;
            string szRequest = "{\"UserInfoSearchCond\":{\"searchID\":\"1\",\"searchResultPosition\":0,\"maxResults\":30,\"EmployeeNoList\":[{\"employeeNo\":\"" + pEmployeeNo + "\"}]}}";
            string szMethod = "POST";

            //查询是否存在工号
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
                return;
            }
            EventLog.WriteEntry("Set Picture Failed!", EventLogEntryType.Information); 

        }

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

        private void SearchDirectory(string path, List<string> file_list)
        {
            DirectoryInfo dir_info = new DirectoryInfo(path);
            try
            {
                foreach (DirectoryInfo subdir_info in dir_info.GetDirectories())
                {
                    SearchDirectory(subdir_info.FullName, file_list);
                }
            }
            catch
            {
            }
            try
            {
                foreach (FileInfo file_info in dir_info.GetFiles())
                {
                    if (file_info.Extension == ".jpg")
                        file_list.Add(file_info.FullName);
                }
            }
            catch
            {
            }
        }


    }
}
