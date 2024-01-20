using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace FaceManagementSetPhoto
{
    class Security
    {
        public static bool Login(DeviceInfo struDeviceInfo)
        {
            string strUsername = struDeviceInfo.strUsername;
            string strPassword = struDeviceInfo.strPassword;
            string strDeviceIp = struDeviceInfo.strDeviceIP;
            string strHttpMethod = "GET";
            string strUrl = "http://" + strDeviceIp + ":" + struDeviceInfo.strHttpPort + "/ISAPI/Security/userCheck";
            string strResponse = string.Empty;

            HttpClient client = new HttpClient();
            int iRet = client.HttpRequest(strUsername, strPassword, strUrl, strHttpMethod, ref strResponse);

            if (iRet == (int)HttpClient.HttpStatus.Http200)
            {
                if (strResponse != string.Empty)
                {
                    MessageBox.Show(struDeviceInfo.strDeviceNickName+" Login success");
                    return true;

//                     XmlDocument xml = new XmlDocument();
//                     xml.LoadXml(strResponse);
//                     if (xml.DocumentElement != null)
//                     {
//                         XmlNodeList childNodes = xml.DocumentElement.ChildNodes;
//                         foreach (XmlNode node in childNodes)
//                         {
//                             if (node.Name == "statusValue")
//                             {
//                                 if (node.InnerText == "200")
//                                 {
//                                     // user check success
//                                     MainForm.LogInfo(struDeviceInfo.strDeviceNickName, null, "Login success");
//                                     return true;
//                                 }
//                             }
//                         }
//                     }
                }
            }
            else if (iRet == (int)HttpClient.HttpStatus.HttpOther)
            {
                string statusCode = string.Empty;
                string statusString = string.Empty;
                client.ParserUserCheck(strResponse, ref statusCode, ref statusString);
                MessageBox.Show(struDeviceInfo.strDeviceNickName+" Login failed!  Error Code:" + statusCode + "  Describe:" + statusString);
            }
            else if (iRet == (int)HttpClient.HttpStatus.HttpTimeOut)
            {
                MessageBox.Show(struDeviceInfo.strDeviceNickName+" Login failed!  Describe: " + strResponse);
            }
            return false;
        }

        private static XmlDocument fnCreatActiveRequest(byte[] byChallenge, int iOutLen, string strPWD)
        {
            //aes encrypt
            byte[] byChallengeHex = Crypt.convertCharArrayToByteArray(byChallenge, iOutLen);

            //AES byChallenge use byChallengeHex
            byte[] byAesEn = Crypt.fnAESEncrypt(byChallengeHex, byChallenge);

            byte[] byAes16 = new byte[16];
            Array.Copy(byAesEn, byAes16, 16);

            byte[] byPassword = Encoding.Default.GetBytes(strPWD);
            //encryp Password using AES
            byte[] byAesPWD = Crypt.fnAESEncrypt(byChallengeHex, byPassword);


            byte[] byPack = byAes16.Concat(byAesPWD).ToArray();

            byte[] byHexAes = Crypt.converByteArrayToCharArray(byPack, byPack.Length);
            string szAesDecrypt = Convert.ToBase64String(byHexAes);

//             byte[] byTest = Convert.FromBase64String(szAesDecrypt);
//             byte[] byTestHex = Crypt.convertCharArrayToByteArray(byTest, byTest.Length);
//             byte[] byDecode = Crypt.fnAESDecrypt(byChallengeHex, byTestHex);
//             byte[] byDePass = byTestHex.Skip(16).Take(16).ToArray();
//             byte[] byPass = Crypt.fnAESDecrypt(byChallengeHex, byDePass);


            //create xml 
            XmlDocument xmlActive = new XmlDocument();
            XmlElement xmlNode = xmlActive.CreateElement("ActivateInfo");
            xmlNode.SetAttribute("version", "2.0");
            xmlNode.SetAttribute("xmlns", "http://www.isapi.org/ver20/XMLSchema");
            xmlActive.AppendChild(xmlNode);
            XmlElement xmlPWD = xmlActive.CreateElement("password");
            xmlPWD.InnerText = szAesDecrypt;
            xmlNode.AppendChild(xmlPWD);

            return xmlActive;
        }
        public static bool fnActive(string strHttpPort, string strDeviceIp, string strPWD)
        {
            IntPtr hHandle = DllInterface.SSL_CreateSSLEncrypt();
            if (hHandle == IntPtr.Zero)
            {
                return false;
            }

            string szPublicKey = Crypt.fnMKRsaPublickey(hHandle);

            //send frist http request to get challenge
            string strHttpMethod = "POST";
            string strUrl = "http://" + strDeviceIp + ":" + strHttpPort + "/ISAPI/Security/challenge";
            HttpClient client = new HttpClient();
            string strResponse = string.Empty;
            string szChallenge = null;
            string szRequest = "<PublicKey><key>" + szPublicKey + "</key></PublicKey>\r\n";
            int iRet = client.HttpPut(strUrl, strHttpMethod, szRequest, ref strResponse);

            if (iRet == (int)HttpClient.HttpStatus.Http200)
            {
                if (strResponse != string.Empty)
                {
                    XmlDocument xml = new XmlDocument();
                    xml.LoadXml(strResponse);
                    if (xml.DocumentElement != null)
                    {
                        XmlNodeList childNodes = xml.DocumentElement.ChildNodes;
                        foreach (XmlNode node in childNodes)
                        {
                            if (node.Name == "key")
                            {
                                szChallenge = node.InnerText;
                            }
                        }
                    }
                }
            }
            else if (iRet == (int)HttpClient.HttpStatus.HttpOther)
            {
                string statusCode = string.Empty;
                string statusString = string.Empty;
                client.ParserResponseStatus(strResponse, ref statusCode, ref statusString);
                MessageBox.Show("Activate failed!  Error Code:" + statusCode + "  Describe:" + statusString);
                DllInterface.SSL_DestroySSLEncrypt(hHandle);
                hHandle = IntPtr.Zero;
                return false;
            }
            else if (iRet == (int)HttpClient.HttpStatus.HttpTimeOut)
            {
                MessageBox.Show("device offline !  Describe: " + strResponse);
                DllInterface.SSL_DestroySSLEncrypt(hHandle);
                hHandle = IntPtr.Zero;
                return false;
            }

            int iOutLen = 0;
            //get challenge 
            byte[] byChallenge = Crypt.fnDecryptChallenge(hHandle, szChallenge, ref iOutLen);
            //destory RSA object
            DllInterface.SSL_DestroySSLEncrypt(hHandle);
            hHandle = IntPtr.Zero;

            //create second request xml
            XmlDocument xmlActive = fnCreatActiveRequest(byChallenge, iOutLen, strPWD);



            HttpClient clientActive = new HttpClient();
            string strActive = "http://" + strDeviceIp + ":" + strHttpPort + "/ISAPI/System/activate";
            iRet = client.HttpPut(strActive, "PUT", xmlActive, ref strResponse);

            if (iRet == (int)HttpClient.HttpStatus.Http200)
            {
                if (strResponse != string.Empty)
                {
                    XmlDocument xml = new XmlDocument();
                    xml.LoadXml(strResponse);
                    if (xml.DocumentElement != null)
                    {
                        XmlNodeList childNodes = xml.DocumentElement.ChildNodes;
                        foreach (XmlNode node in childNodes)
                        {
                            if (node.Name == "statusCode")
                            {
                                if (node.InnerText == "1")
                                {
                                    // user check success
                                    MessageBox.Show(strDeviceIp+" Active device success!");
                                    //this.Close();
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            else if (iRet == (int)HttpClient.HttpStatus.HttpOther)
            {
                string statusCode = string.Empty;
                string statusString = string.Empty;
                client.ParserResponseStatus(strResponse, ref statusCode, ref statusString);
                MessageBox.Show("Activate failed!  Error Code:" + statusCode + "  Describe:" + statusString);
                return false;
            }
            else if (iRet == (int)HttpClient.HttpStatus.HttpTimeOut)
            {
                MessageBox.Show("device offline !  Describe: " + strResponse);
                return false;
            }
            return true;
        }

    }
}
