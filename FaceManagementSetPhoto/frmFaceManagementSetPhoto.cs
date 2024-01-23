using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Reflection;
using System.IO;

using Newtonsoft.Json;
using System.Net;
using System.Xml;
using System.Text.RegularExpressions;
using FaceManagementSetPhoto;
using System.Drawing.Text;
using System.Data.SqlClient;

namespace FaceManagement
{
    public partial class frmFaceManagementSetPhoto : Form
    {
        private ConfigReader configReader;
        public frmFaceManagementSetPhoto()
        {
            InitializeComponent();
            comboBoxLanguage.SelectedIndex = 0;

            configReader = new ConfigReader("xx.txt");
        }

        private string ActionISAPI(string szUrl, string szRequest, string szMethod)
        {
            string szResponse = string.Empty;
            if (AddDevice.struDeviceInfo == null)
            {
                MessageBox.Show("Please login device first!");
                return szResponse;
            }
            if (!AddDevice.struDeviceInfo.bIsLogin)
            {
                MessageBox.Show("Please login device first!");
                return szResponse;
            }

            if (!szUrl.Substring(0, 4).Equals("http"))
            {
                szUrl = "http://" + AddDevice.struDeviceInfo.strDeviceIP + ":" + AddDevice.struDeviceInfo.strHttpPort + szUrl;
            }
            HttpClient clHttpClient = new HttpClient();
            byte[] byResponse = { 0 };
            int iRet = 0;
            string szContentType = string.Empty;

            switch (szMethod)
            {
                case "GET":
                    iRet = clHttpClient.HttpRequest(AddDevice.struDeviceInfo.strUsername, AddDevice.struDeviceInfo.strPassword, szUrl, szMethod, ref byResponse, ref szContentType);
                    break;
                case "PUT":
                    iRet = clHttpClient.HttpPut(AddDevice.struDeviceInfo.strUsername, AddDevice.struDeviceInfo.strPassword, szUrl, szMethod, szRequest, ref szResponse);
                    break;
                case "POST":
                    iRet = clHttpClient.HttpPut(AddDevice.struDeviceInfo.strUsername, AddDevice.struDeviceInfo.strPassword, szUrl, szMethod, szRequest, ref szResponse);
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
                MessageBox.Show("Request failed! Error code:" + szCode + " Describe:" + szError + "\r\n");
            }
            else if (iRet == (int)HttpClient.HttpStatus.HttpTimeOut)
            {
                MessageBox.Show(szMethod + " " + szUrl + "error!Time out");
            }
            return szResponse;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AddDevice deviceAdd = new AddDevice();
            deviceAdd.ShowDialog();
            deviceAdd.Dispose();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = @"C:\Users\dsolares\Documents\Pruebas desarrollo\Evento Foto";
            openFileDialog.Filter = "Face file|*.jpg|All documents|*.*";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.FilterIndex = 1;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (pictureBoxFace.Image != null)
                {
                    pictureBoxFace.Image.Dispose();
                    pictureBoxFace.Image = null;
                }
                textBoxFilePath.Text = openFileDialog.FileName;
                pictureBoxFace.Image = Image.FromFile(textBoxFilePath.Text);
            }
        }

        private string ObtenerConnectionStringDesdeArchivo()
        {
            string[] lines = File.ReadAllLines("xx.txt");

            //separado por split y que al menos exista una linea en el archivo de texto
            if (lines.Length >= 1)
            {
                // Verificar si hay exactamente 5 partes en la línea
                string[] parameters = lines[0].Split('|'); //lectura del arreglo, las lineas del archivo
                if (parameters.Length == 5)
                {
                    return $"Data Source={parameters[0]};Initial Catalog={parameters[1]};User ID={parameters[2]};Password={parameters[3]}";
                }
            }
            throw new InvalidOperationException("Archivo de conexión incorrecto");
        }
        private void button6_Click(object sender, EventArgs e)
        {
            string szUrl = "/ISAPI/AccessControl/UserInfo/Search?format=json";
            string szResponse = string.Empty;
            string szRequest = "{\"UserInfoSearchCond\":{\"searchID\":\"1\",\"searchResultPosition\":0,\"maxResults\":30,\"EmployeeNoList\":[{\"employeeNo\":\"" + textBoxEmployeeNo.Text + "\"}]}}";
            string szMethod = "POST";

            //查询是否存在工号
            szResponse = ActionISAPI(szUrl, szRequest, szMethod);

            if (szResponse != string.Empty)
            {
                UserInfoSearchRoot us = JsonConvert.DeserializeObject<UserInfoSearchRoot>(szResponse);
                if (0 == us.UserInfoSearch.totalMatches)
                {
                    MessageBox.Show("Employee No isn't found!");
                    return;
                }
            }

            szUrl = "/ISAPI/Intelligent/FDLib?format=json&FDID=" + textBoxFDID.Text + "&faceLibType=" + comboBoxFaceType.SelectedItem.ToString();
            szResponse = string.Empty;
            szMethod = "GET";

            //查询FaceLib是否存在
            szResponse = ActionISAPI(szUrl, szRequest, szMethod);
            if (szResponse != string.Empty)
            {
                FaceLib fb = JsonConvert.DeserializeObject<FaceLib>(szResponse);
                if (fb == null || fb.statusCode != 1)
                {
                    MessageBox.Show("FaceLib isn't existed!");
                    return;
                }
            }
            //查询是否已有图片，若有则删除
            szUrl = "/ISAPI/Intelligent/FDLib/FDSearch?format=json";

            szResponse = string.Empty;
            szRequest = "{\"searchResultPosition\":0,\"maxResults\":30,\"faceLibType\":\"" + comboBoxFaceType.SelectedItem.ToString() +
                "\",\"FDID\":\"" + textBoxFDID.Text +
                "\",\"FPID\":\"" + textBoxEmployeeNo.Text + "\"}";
            szMethod = "POST";

            szResponse = ActionISAPI(szUrl, szRequest, szMethod);
            if (szResponse != string.Empty)
            {
                Root rt = JsonConvert.DeserializeObject<Root>(szResponse);
                if (rt.statusCode == 1)
                {
                    if (rt.totalMatches != 0)
                    {
                        szUrl = "/ISAPI/Intelligent/FDLib/FDSearch/Delete?format=json&FDID=" + textBoxFDID.Text + "&faceLibType=" + comboBoxFaceType.SelectedItem.ToString() + "";
                        szResponse = string.Empty;
                        szRequest = "{\"FPID\":[{\"value\":\"" + textBoxEmployeeNo.Text + "\"}]}";
                        szMethod = "PUT";

                        szResponse = ActionISAPI(szUrl, szRequest, szMethod);
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
                szUrl = "http://" + AddDevice.struDeviceInfo.strDeviceIP + ":" + AddDevice.struDeviceInfo.strHttpPort + szUrl;
            }
            HttpClient clHttpClient = new HttpClient();
            szResponse = string.Empty;
            szRequest = "{\"faceLibType\":\"" + comboBoxFaceType.SelectedItem.ToString() +
                "\",\"FDID\":\"" + textBoxFDID.Text +
                "\",\"FPID\":\"" + textBoxEmployeeNo.Text + "\"}";
            string filePath = textBoxFilePath.Text;
            szResponse = clHttpClient.HttpPostData(AddDevice.struDeviceInfo.strUsername, AddDevice.struDeviceInfo.strPassword, szUrl, filePath, szRequest);
            ResponseStatus res = JsonConvert.DeserializeObject<ResponseStatus>(szResponse);
            if (res != null && res.statusCode.Equals("1"))
            {
                MessageBox.Show("Set Picture Succ!");
                return;
            }
            MessageBox.Show("Set Picture Failed!");
        }


        private List<DispositivoBiometrico> GetDeviceList()
        {
            List<DispositivoBiometrico> deviceList = new List<DispositivoBiometrico>();

            try
            {
                string connectionString = ObtenerConnectionStringDesdeArchivo();

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
                Console.WriteLine("Error al obtener la lista de dispositivos: " + ex.Message);
            }

            return deviceList;
        }

        private void ReplicateImageToDevice(DispositivoBiometrico device)
        {
            string szUrl = "/ISAPI/Intelligent/FDLib/FaceDataRecord?format=json";
            szUrl = "http://" + device.DireccionIP + ":" + device.Puerto + szUrl;

            string szResponse = string.Empty;
            string szRequest = "{\"faceLibType\":\"" + comboBoxFaceType.SelectedItem.ToString() +
                                "\",\"FDID\":\"" + textBoxFDID.Text +
                                "\",\"FPID\":\"" + textBoxEmployeeNo.Text + "\"}";

            string filePath = textBoxFilePath.Text;

            // Utilizar la función original para realizar la replicación
            szResponse = ActionISAPI(szUrl, szRequest, "POST");

            // Asegurarse de manejar la respuesta según tus necesidades
            // Puedes verificar el valor de szResponse y tomar acciones en consecuencia
            // Puedes agregar más lógica aquí para manejar el resultado de la replicación en cada dispositivo
        }
        private void btnClear_Click(object sender, EventArgs e)
        {
            if (pictureBoxFace.Image != null)
            {
                pictureBoxFace.Image.Dispose();
                pictureBoxFace.Image = null;
            }
            textBoxFilePath.Text = "";
        }

        private string ReadFaceData()
        {
            string sFpData = null;

            if (File.Exists(textBoxFilePath.Text))
            {
                FileStream fs = File.OpenRead(textBoxFilePath.Text); // OpenRead
                int filelen = 0;
                filelen = (int)fs.Length;
                byte[] byteFp = new byte[filelen];
                fs.Read(byteFp, 0, filelen);
                fs.Close();
                try
                {
                    sFpData = Convert.ToBase64String(byteFp);
                }
                catch
                {
                    sFpData = null;
                }
            }
            return sFpData;

        }

        private string ActionISAPI2(string szUrl, string szRequest, string szMethod)
        {
            string szResponse = string.Empty;
            if (AddDevice.struDeviceInfo == null)
            {
                MessageBox.Show("Please login device first!");
                return szResponse;
            }
            if (!AddDevice.struDeviceInfo.bIsLogin)
            {
                MessageBox.Show("Please login device first!");
                return szResponse;
            }
            szUrl = szUrl.Substring(szUrl.IndexOf("/LOCALS"));
            if (!szUrl.Substring(0, 4).Equals("http"))
            {
                szUrl = "http://" + AddDevice.struDeviceInfo.strDeviceIP + ":" + AddDevice.struDeviceInfo.strHttpPort + szUrl;
            }
            HttpClient clHttpClient = new HttpClient();
            byte[] byResponse = { 0 };
            int iRet = 0;
            string szContentType = string.Empty;

            iRet = clHttpClient.HttpRequest2(AddDevice.struDeviceInfo.strUsername, AddDevice.struDeviceInfo.strPassword, szUrl, szMethod, ref byResponse, ref szContentType);

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
                        string szPath = string.Format("{0}\\outputFaceData.jpg", Environment.CurrentDirectory);
                        try
                        {
                            using (FileStream fs = new FileStream(szPath, FileMode.OpenOrCreate))
                            {
                                if (!File.Exists(szPath))
                                {
                                    MessageBox.Show("FaceData storage file create failed！");
                                }
                                BinaryWriter objBinaryWrite = new BinaryWriter(fs);
                                fs.Write(byResponse, 0, byResponse.Length);
                                fs.Close();
                                textBoxFilePath.Text = szPath;
                                pictureBoxFace.Image = Image.FromFile(szPath);
                                MessageBox.Show("Get face data success!");
                            }
                            MessageBox.Show("FaceData GET SUCCEED", "SUCCESSFUL", MessageBoxButtons.OK);
                        }
                        catch
                        {
                            MessageBox.Show("FaceData process failed");
                        }
                        return szResponse;
                    }
                }
            }
            else if (iRet == (int)HttpClient.HttpStatus.HttpOther)
            {
                string szCode = string.Empty;
                string szError = string.Empty;
                clHttpClient.ParserResponseStatus(szResponse, ref szCode, ref szError);
                MessageBox.Show("Request failed! Error code:" + szCode + " Describe:" + szError + "\r\n");
            }
            else if (iRet == (int)HttpClient.HttpStatus.HttpTimeOut)
            {
                MessageBox.Show(szMethod + " " + szUrl + "error!Time out");
            }
            return szResponse;
        }

        //获取单张人脸图片
        //private void btnGet_Click(object sender, EventArgs e)
        //{
        //    string szUrl = "/ISAPI/AccessControl/UserInfo/Search?format=json";
        //    string szResponse = string.Empty;
        //    string szRequest = "{\"UserInfoSearchCond\":{\"searchID\":\"1\",\"searchResultPosition\":0,\"maxResults\":30,\"EmployeeNoList\":[{\"employeeNo\":\"" + textBoxEmployeeNo.Text + "\"}]}}";
        //    string szMethod = "POST";


        //    //查询是否存在工号
        //    szResponse = ActionISAPI(szUrl, szRequest, szMethod);
        //   if(szResponse!=string.Empty)
        //   {
        //        UserInfoSearchRoot us = JsonConvert.DeserializeObject<UserInfoSearchRoot>(szResponse);
        //        if (0 == us.UserInfoSearch.totalMatches)
        //        {
        //            MessageBox.Show("Employee No isn't found!");
        //            return;
        //        }
        //    }

        //    //查询人脸库是否存在
        //    szUrl = "/ISAPI/Intelligent/FDLib?format=json&FDID=" + textBoxFDID.Text + "&faceLibType=" + comboBoxFaceType.SelectedItem.ToString();
        //    szResponse = string.Empty;
        //    szMethod = "GET";

        //    szResponse = ActionISAPI(szUrl, szRequest, szMethod);
        //    if(szResponse!=string.Empty)
        //    {
        //        FaceLib fb = JsonConvert.DeserializeObject<FaceLib>(szResponse);
        //        if (fb == null || fb.statusCode != 1)
        //        {
        //            MessageBox.Show("FaceLib isn't existed!");
        //            return;
        //        }
        //    }

        //    szUrl = "/ISAPI/Intelligent/FDLib/FDSearch?format=json";
        //    szResponse = string.Empty;
        //    szRequest = "{\"searchResultPosition\":0,\"maxResults\":5,\"faceLibType\":\"" + comboBoxFaceType.SelectedItem.ToString() +
        //        "\",\"FDID\":\""+textBoxFDID.Text+
        //        "\",\"FPID\":\""+textBoxEmployeeNo.Text+"\"}";
        //    szMethod = "POST";

        //    szResponse = ActionISAPI(szUrl, szRequest, szMethod);
        //    if (szResponse != string.Empty)
        //    {
        //        Root rt = JsonConvert.DeserializeObject<Root>(szResponse);
        //        if(rt.statusCode==1)
        //        {
        //            //MessageBox.Show("Get picture succ!");
        //            if(rt.totalMatches==1)
        //            {
        //                string picData = string.Empty;
        //                foreach(MatchListItem item in rt.MatchList)
        //                {
        //                    picData = item.modelData;
        //                    string strPath = string.Format("1.jpg");

        //                    if(pictureBoxFace.Image!=null)
        //                    {
        //                        pictureBoxFace.Image.Dispose();
        //                        pictureBoxFace.Image = null;
        //                    }
        //                    string url = item.faceURL;
        //                    string data = ActionISAPI2(url, szRequest, "GET");
        //                    //WriteFaceData(data);
        //                }
        //            }
        //            else
        //            {
        //                MessageBox.Show("Picture isn't found!");
        //            }
        //        }
        //    }
           
        //}


        //private void btnDel_Click(object sender, EventArgs e)
        //{
        //    string szUrl = "/ISAPI/Intelligent/FDLib/FDSearch/Delete?format=json&FDID="+textBoxFDID.Text+"&faceLibType="+comboBoxFaceType.SelectedItem.ToString()+"";
        //    string szResponse = string.Empty;
        //    string szRequest = "{\"FPID\":[{\"value\":\""+textBoxEmployeeNo.Text+"\"}]}";
        //    string szMethod = "PUT";

        //    szResponse = ActionISAPI(szUrl, szRequest, szMethod);
        //    if (szResponse != string.Empty)
        //    {
        //        ResponseStatus rs = JsonConvert.DeserializeObject<ResponseStatus>(szResponse);
        //        if(rs.statusCode.Equals("1"))
        //        {
        //            MessageBox.Show("Delete Picture Succ!");
        //        }
        //        else
        //        {
        //            MessageBox.Show("Delete face data failed! Error code:" + rs.subStatusCode);
        //        }
        //    }
        //}

        //private void WriteFaceData(string faceData)
        //{
        //    string szPath = null;
        //    DateTime dt = DateTime.Now;
        //    //byte[] byFaceData = Convert.FromBase64String(faceData);
        //    MessageBox.Show(faceData.Length.ToString());
        //    byte[] byFaceData = Encoding.Default.GetBytes(faceData);
        //    szPath = string.Format("{0}\\outputFaceData.jpg", Environment.CurrentDirectory);
        //    try
        //    {
        //        using (FileStream fs = new FileStream(szPath, FileMode.OpenOrCreate))
        //        {
        //            if (!File.Exists(szPath))
        //            {
        //                MessageBox.Show("FaceData storage file create failed！");
        //            }
        //            BinaryWriter objBinaryWrite = new BinaryWriter(fs);
        //            fs.Write(byFaceData, 0, byFaceData.Length);
        //            fs.Close();
        //        }
        //        MessageBox.Show("FaceData GET SUCCEED", "SUCCESSFUL", MessageBoxButtons.OK);
        //    }
        //    catch
        //    {
        //        MessageBox.Show("FaceData process failed");
        //    }
        //}

        //private string Reverse( string s )
        //{
        //    char[] charArray = s.ToCharArray();
        //    Array.Reverse( charArray );
        //    return new string( charArray );
        //}

 
    }
}

