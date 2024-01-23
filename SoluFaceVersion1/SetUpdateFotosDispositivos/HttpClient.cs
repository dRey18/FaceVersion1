﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Specialized;
using System.Drawing;

namespace SetUpdateFotosDispositivos
{
    class HttpClient
    {
        public delegate bool ProcessLongLinkData(byte[] data, object UserData, string boundary);
        public delegate bool ProcessSendDate(ref byte[] byBuffer, object UserDate);
        public delegate void SaveDownloadVideo(byte[] data, int iLength, ListViewItem temp, float fPercent);

        public static int i = 0;

        public enum HttpStatus
        {
            Http200 = 0,
            HttpOther,
            HttpTimeOut
        }
        public class RequestState
        {
            // This class stores the State of the request.
            const int BUFFER_SIZE = 8 * 102400;
            public StringBuilder requestData;
            public byte[] BufferRead;
            public HttpWebRequest request;
            public HttpWebResponse response;
            public Stream streamResponse;
            public ProcessLongLinkData processLongLinkData;
            public object objectUserData;
            public string strBoundary;
            public ProcessSendDate processSendData;
            public WebException eStatus;

            public RequestState()
            {
                BufferRead = new byte[BUFFER_SIZE];
                requestData = new StringBuilder("");
                request = null;
                streamResponse = null;
                processLongLinkData = null;
                objectUserData = null;
                strBoundary = string.Empty;
                processSendData = null;
                eStatus = null;
            }
        }

        private static Dictionary<string, NetworkCredential> digest = new Dictionary<string, NetworkCredential>();
        private CredentialCache _credentialCache = null;
        private string strURL = string.Empty;
        public static int m_iHttpTimeOut = 500000;
        const int BUFFER_SIZE = 1024;
        public static bool bIsStopDownLoad = false;

        public static void SetHttpTimeOut(int ms)
        {
            m_iHttpTimeOut = ms;
        }

        public static int GetHttpTimeOut()
        {
            return m_iHttpTimeOut;
        }
        private CredentialCache GetCredentialCache(string sUrl, string strUserName, string strPassword)
        {
            if (_credentialCache == null)
            {
                _credentialCache = new CredentialCache();
                _credentialCache.Add(new Uri(sUrl), "Digest", new NetworkCredential(strUserName, strPassword));
                strURL = sUrl;
            }
            if (strURL != sUrl)
            {
                _credentialCache.Add(new Uri(sUrl), "Digest", new NetworkCredential(strUserName, strPassword));
                strURL = sUrl;
            }

            return _credentialCache;
        }

        private NetworkCredential GetNetworkCredential(string sUrl, string strUserName, string strPassword, string deviceIp)
        {
            foreach (string ip in digest.Keys)
            {
                if (ip == deviceIp)
                {
                    NetworkCredential tmp = new NetworkCredential();
                    tmp = digest[ip];
                    return tmp;
                }
            }

            NetworkCredential tmp2 = new NetworkCredential(strUserName, strPassword);
            digest.Add(deviceIp, tmp2);

            return tmp2;
        }

        private static void ReceiveData(IAsyncResult asyncResult)
        {
            try
            {
                RequestState myRequestState = (RequestState)asyncResult.AsyncState;
                Stream responseStream = myRequestState.streamResponse;
                int read = responseStream.EndRead(asyncResult);
                if (read > 0)
                {
                    if (myRequestState.processLongLinkData != null)
                    {
                        //                         Console.WriteLine("Recive Buffer:" + System.Text.Encoding.Default.GetString (myRequestState.BufferRead));
                        Byte[] pBuf = new Byte[read];
                        Array.Copy(myRequestState.BufferRead, pBuf, read);

                        if (!myRequestState.processLongLinkData(pBuf
                            , myRequestState.objectUserData, myRequestState.strBoundary))
                        {
                            responseStream.Close();
                            return;
                        }
                    }
                    IAsyncResult asynchronousResult = responseStream.BeginRead(myRequestState.BufferRead, 0, BUFFER_SIZE, new AsyncCallback(ReceiveData), myRequestState);
                    return;
                }
                else
                {
                    responseStream.Close();
                }

            }
            catch (WebException e)
            {
                Console.WriteLine("\nReadCallBack Exception raised!");
                Console.WriteLine("\nMessage:{0}", e.Message);
                Console.WriteLine("\nStatus:{0}", e.Status);
            }
            catch (IOException e)
            {
                Console.WriteLine("\nReadCallBack Exception raised!");
                Console.WriteLine("\nMessage:{0}", e.Message);
            }
        }

        public int StartHttpLongLink(string strUserName, string strPassword, string strUrl, string strHttpMethod
            , ProcessLongLinkData processLongLinkData, ref string strResponse, object UserData, bool bBlock = true)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(strUrl);
            request.Credentials = GetCredentialCache(strUrl, strUserName, strPassword);
            request.Method = strHttpMethod;

            try
            {
                RequestState myRequestState = new RequestState();
                myRequestState.request = request;
                myRequestState.processLongLinkData = processLongLinkData;
                myRequestState.objectUserData = UserData;
                IAsyncResult ret = request.BeginGetResponse(new AsyncCallback(RespCallback), myRequestState);

                if (bBlock)
                {
                    int nTimeoutLimit = m_iHttpTimeOut / 100;
                    int nTimeoutCount = 0;
                    while (!ret.IsCompleted && nTimeoutCount < nTimeoutLimit)
                    {
                        Thread.Sleep(100);
                        nTimeoutCount++;
                    }

                    if (nTimeoutCount == nTimeoutLimit)
                    {
                        request.Abort();
                    }

                    if (myRequestState.response != null && myRequestState.response.StatusCode == HttpStatusCode.OK)
                    {
                        return (int)HttpStatus.Http200;
                    }
                    else
                    {
                        if (myRequestState.eStatus != null)
                        {
                            if (myRequestState.eStatus.Response != null)
                            {
                                Stream st = myRequestState.eStatus.Response.GetResponseStream();
                                StreamReader sr = new StreamReader(st, System.Text.Encoding.Default);
                                strResponse = sr.ReadToEnd();
                                sr.Close();
                                st.Close();
                                return (int)HttpStatus.HttpOther;
                            }
                            else
                            {
                                strResponse = myRequestState.eStatus.Status.ToString();
                                return (int)HttpStatus.HttpTimeOut;
                            }

                        }
                        return (int)HttpStatus.HttpOther;
                    }
                }
                else
                {
                    return (int)HttpStatus.Http200;
                }
            }
            catch (WebException ex)
            {
                WebResponse wr = ex.Response;
                if (wr != null)
                {
                    return (int)HttpStatus.HttpOther;
                }
                else
                {
                    return (int)HttpStatus.HttpTimeOut;
                }
            }
        }

        public int StartHttpLongLink_Reuqest(string strUserName, string strPassword, string strUrl, string strHttpMethod
            , string strContentType, ref string strResponse, ProcessSendDate processSendData, object UserData)
        {
            HttpWebRequest request = WebRequest.CreateHttp(strUrl);
            request.Credentials = GetCredentialCache(strUrl, strUserName, strPassword);
            request.Method = strHttpMethod;
            request.Timeout = m_iHttpTimeOut;
            request.ContentType = strContentType;
            request.AllowWriteStreamBuffering = false;
            if (processSendData == null)
            {
                request.ContentLength = 0;
            }
            else
            {
                //set contentLength very big to send stream
                request.ContentLength = 0x7fffffffffffffff;
            }
            request.Proxy = null;

            try
            {
                RequestState myRequestState = new RequestState();
                myRequestState.request = request;
                myRequestState.processSendData = processSendData;
                myRequestState.objectUserData = UserData;
                IAsyncResult ret = request.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), myRequestState);

                int nTimeoutLimit = m_iHttpTimeOut / 100;
                int nTimeoutCount = 0;
                while (!ret.IsCompleted && nTimeoutCount < nTimeoutLimit)
                {
                    Thread.Sleep(100);
                    nTimeoutCount++;
                }

                if (nTimeoutCount == nTimeoutLimit)
                {
                    request.Abort();
                }


                if (myRequestState.eStatus == null)
                {
                    return (int)HttpStatus.Http200;
                }
                else
                {

                    if (myRequestState.eStatus.Response != null)
                    {
                        Stream st = myRequestState.eStatus.Response.GetResponseStream();
                        StreamReader sr = new StreamReader(st, System.Text.Encoding.Default);
                        strResponse = sr.ReadToEnd();
                        sr.Close();
                        st.Close();
                        return (int)HttpStatus.HttpOther;
                    }
                    else
                    {
                        strResponse = myRequestState.eStatus.Status.ToString();
                        return (int)HttpStatus.HttpTimeOut;
                    }
                }

                //                 return (int)HttpStatus.Http200;


            }
            catch (WebException ex)
            {
                WebResponse wr = ex.Response;
                if (wr != null)
                {
                    return (int)HttpStatus.HttpOther;
                }
                else
                {
                    return (int)HttpStatus.HttpTimeOut;
                }
            }
        }

        public int StartHttpLongLink_Subscribe(string strUserName, string strPassword, string strUrl, string strHttpMethod
            , string param, ProcessLongLinkData processLongLinkData, ref string strResponse, object UserData, bool bBlock = true)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(strUrl);
            request.Credentials = GetCredentialCache(strUrl, strUserName, strPassword);
            request.Method = strHttpMethod;

            byte[] bs = Encoding.ASCII.GetBytes(param);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = bs.Length;
            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(bs, 0, bs.Length);
            }


            try
            {
                RequestState myRequestState = new RequestState();
                myRequestState.request = request;
                myRequestState.processLongLinkData = processLongLinkData;
                myRequestState.objectUserData = UserData;
                IAsyncResult ret = request.BeginGetResponse(new AsyncCallback(RespCallback), myRequestState);

                if (bBlock)
                {
                    int nTimeoutLimit = m_iHttpTimeOut / 100;
                    int nTimeoutCount = 0;
                    while (!ret.IsCompleted && nTimeoutCount < nTimeoutLimit)
                    {
                        Thread.Sleep(100);
                        nTimeoutCount++;
                    }

                    if (nTimeoutCount == nTimeoutLimit)
                    {
                        request.Abort();
                    }

                    if (myRequestState.response != null && myRequestState.response.StatusCode == HttpStatusCode.OK)
                    {
                        return (int)HttpStatus.Http200;
                    }
                    else
                    {
                        if (myRequestState.eStatus != null)
                        {
                            if (myRequestState.eStatus.Response != null)
                            {
                                Stream st = myRequestState.eStatus.Response.GetResponseStream();
                                StreamReader sr = new StreamReader(st, System.Text.Encoding.Default);
                                strResponse = sr.ReadToEnd();
                                sr.Close();
                                st.Close();
                                return (int)HttpStatus.HttpOther;
                            }
                            else
                            {
                                strResponse = myRequestState.eStatus.Status.ToString();
                                return (int)HttpStatus.HttpTimeOut;
                            }

                        }
                        return (int)HttpStatus.HttpOther;
                    }
                }
                else
                {
                    return (int)HttpStatus.Http200;
                }
            }
            catch (WebException ex)
            {
                WebResponse wr = ex.Response;
                if (wr != null)
                {
                    return (int)HttpStatus.HttpOther;
                }
                else
                {
                    return (int)HttpStatus.HttpTimeOut;
                }
            }
        }

        public int HttpRequest(string strUserName, string strPassword, string strUrl, string strHttpMethod, ref string strResponse)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(strUrl);
            request.Credentials = GetCredentialCache(strUrl, strUserName, strPassword);
            request.Method = strHttpMethod;
            request.Timeout = m_iHttpTimeOut;
            try
            {
                WebResponse wr = request.GetResponse();
                strResponse = new StreamReader(wr.GetResponseStream()).ReadToEnd();
                return (int)HttpStatus.Http200;
            }
            catch (WebException ex)
            {
                WebResponse wr = ex.Response;
                if (wr != null)
                {
                    Stream st = wr.GetResponseStream();
                    StreamReader sr = new StreamReader(st, System.Text.Encoding.Default);
                    strResponse = sr.ReadToEnd();
                    sr.Close();
                    st.Close();
                    return (int)HttpStatus.HttpOther;
                }
                else
                {
                    strResponse = ex.Status.ToString();
                    return (int)HttpStatus.HttpTimeOut;
                }
            }
            finally
            {
                if (request != null)
                {
                    request.Abort();
                }
            }
        }

        public int HttpRequest(string strUserName, string strPassword, string strUrl, string strHttpMethod, ref byte[] szResponse)
        {
            string szContentType = string.Empty;
            return HttpRequest(strUserName, strPassword, strUrl, strHttpMethod, ref szResponse, ref szContentType);
        }

        public int HttpRequest2(string strUserName, string strPassword, string strUrl, string strHttpMethod, ref byte[] szResponse, ref string szContentType)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(strUrl);
            request.Credentials = GetCredentialCache(strUrl, strUserName, strPassword);
            request.Method = strHttpMethod;
            request.Timeout = m_iHttpTimeOut;
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
            request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8");
            request.Headers.Add("Accept-Encoding", "gzip, deflate");
            request.Headers.Add("Cache-Control", "max-age=0");
            request.Headers.Add("Upgrade-Insecure-Requests", "1");
            try
            {
                WebResponse wr = request.GetResponse();
                szContentType = wr.ContentType;
                szResponse = new byte[wr.ContentLength];
                byte[] buf = new byte[wr.ContentLength];
                int iRet = -1;
                int len = 0;
                while ((iRet = wr.GetResponseStream().Read(buf, 0, (int)wr.ContentLength - len)) > 0)
                {
                    Array.Copy(buf, 0, szResponse, len, iRet);
                    len += iRet;
                }
                return (int)HttpStatus.Http200;
            }
            catch (WebException ex)
            {
                WebResponse wr = ex.Response;
                if (wr != null)
                {
                    Stream st = wr.GetResponseStream();
                    StreamReader sr = new StreamReader(st, System.Text.Encoding.Default);
                    szResponse = System.Text.Encoding.Default.GetBytes(sr.ReadToEnd());
                    sr.Close();
                    st.Close();
                    return (int)HttpStatus.HttpOther;
                }
                else
                {
                    szResponse = System.Text.Encoding.Default.GetBytes(ex.Status.ToString());
                    return (int)HttpStatus.HttpTimeOut;
                }
            }
            finally
            {

            }
        }

        public int HttpRequest(string strUserName, string strPassword, string strUrl, string strHttpMethod, ref byte[] szResponse, ref string szContentType)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(strUrl);
            request.Credentials = GetCredentialCache(strUrl, strUserName, strPassword);
            request.Method = strHttpMethod;
            request.Timeout = m_iHttpTimeOut;

            try
            {
                WebResponse wr = request.GetResponse();
                szContentType = wr.ContentType;
                szResponse = new byte[wr.ContentLength];
                byte[] buf = new byte[wr.ContentLength];
                int iRet = -1;
                int len = 0;
                while ((iRet = wr.GetResponseStream().Read(buf, 0, (int)wr.ContentLength - len)) > 0)
                {
                    Array.Copy(buf, 0, szResponse, len, iRet);
                    len += iRet;
                }
                return (int)HttpStatus.Http200;
            }
            catch (WebException ex)
            {
                WebResponse wr = ex.Response;
                if (wr != null)
                {
                    Stream st = wr.GetResponseStream();
                    StreamReader sr = new StreamReader(st, System.Text.Encoding.Default);
                    szResponse = System.Text.Encoding.Default.GetBytes(sr.ReadToEnd());
                    sr.Close();
                    st.Close();
                    return (int)HttpStatus.HttpOther;
                }
                else
                {
                    szResponse = System.Text.Encoding.Default.GetBytes(ex.Status.ToString());
                    return (int)HttpStatus.HttpTimeOut;
                }
            }
            finally
            {

            }
        }

        public int HttpRequestVideoDownload(string strUserName, string strPassword, string strUrl, string strHttpMethod, string param, ref byte[] szResponse, ListViewItem temp, SaveDownloadVideo SavePlayBackVideo)
        {
            bIsStopDownLoad = false;
            temp.SubItems[4].Text = "0%";
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(strUrl);
            request.Credentials = GetCredentialCache(strUrl, strUserName, strPassword);
            request.Method = strHttpMethod;
            request.Timeout = 20000;
            //             request.KeepAlive = false;
            //             System.Net.ServicePointManager.DefaultConnectionLimit = 512;
            byte[] bs = Encoding.ASCII.GetBytes(param);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = bs.Length;
            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(bs, 0, bs.Length);
            }

            try
            {
                WebResponse wr = request.GetResponse();
                byte[] buf = new byte[10 * 10 * 1024];
                int recvLengh = 0;
                float fPercent = 0;
                while (wr.ContentLength > recvLengh)
                {
                    if (bIsStopDownLoad)
                    {
                        break;
                    }
                    else
                    {
                        int iRet = wr.GetResponseStream().Read(buf, 0, 10 * 10 * 1024);
                        fPercent = (float)recvLengh * 100 / (float)wr.ContentLength;
                        SavePlayBackVideo(buf, iRet, temp, fPercent);
                        recvLengh += iRet;
                    }
                }
                SavePlayBackVideo(buf, 0, temp, fPercent);
                if (wr != null)
                {
                    wr.Close();
                }
                if (request != null)
                {
                    request.Abort();
                }
                return (int)HttpStatus.Http200;
            }
            catch (WebException ex)
            {
                WebResponse wr = ex.Response;
                if (wr != null)
                {
                    Stream st = wr.GetResponseStream();
                    StreamReader sr = new StreamReader(st, System.Text.Encoding.Default);
                    szResponse = System.Text.Encoding.Default.GetBytes(sr.ReadToEnd());
                    sr.Close();
                    st.Close();
                    return (int)HttpStatus.HttpOther;
                }
                else
                {
                    szResponse = System.Text.Encoding.Default.GetBytes(ex.Status.ToString());
                    return (int)HttpStatus.HttpTimeOut;
                }
            }
            finally
            {

            }
        }


        public int HttpPut(string strUserName, string strPassword, string strUrl, string strHttpMethod, byte[] bs, ref string strResponse)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(strUrl);
            request.Credentials = GetCredentialCache(strUrl, strUserName, strPassword);
            request.Method = strHttpMethod;
            request.Timeout = m_iHttpTimeOut * 60;

            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = bs.Length;
            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(bs, 0, bs.Length);
            }

            try
            {
                WebResponse wr = request.GetResponse();
                strResponse = new StreamReader(wr.GetResponseStream()).ReadToEnd();

                wr.Close();
                return (int)HttpStatus.Http200;
            }
            catch (WebException ex)
            {
                WebResponse wr = ex.Response;
                if (wr != null)
                {
                    Stream st = wr.GetResponseStream();
                    StreamReader sr = new StreamReader(st, System.Text.Encoding.Default);
                    strResponse = sr.ReadToEnd();
                    sr.Close();
                    st.Close();
                    return (int)HttpStatus.HttpOther;
                }
                else
                {
                    strResponse = ex.Status.ToString();
                    return (int)HttpStatus.HttpTimeOut;
                }
            }
            finally
            {

            }
        }


        public string HttpPostData(string strUserName, string strPassword, string url,
            string filePath, string request)
        {
            string responseContent = string.Empty;
            var memStream = new MemoryStream();
            var webRequest = (HttpWebRequest)WebRequest.Create(url);
            // 边界符  
            var boundary = "---------------" + DateTime.Now.Ticks.ToString("x");
            // 边界符  
            var beginBoundary = Encoding.ASCII.GetBytes("--" + boundary + "\r\n");
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            // 最后的结束符  
            var endBoundary = Encoding.ASCII.GetBytes("--" + boundary + "--\r\n");

            // 设置属性  
            webRequest.Credentials = GetCredentialCache(url, strUserName, strPassword);
            webRequest.Timeout = m_iHttpTimeOut;
            webRequest.Method = "POST";
            webRequest.Accept = "text/html, application/xhtml+xml,";
            webRequest.Headers.Add("Accept-Language", "zh-CN");
            webRequest.ContentType = "multipart/form-data; boundary=" + boundary;
            webRequest.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)";
            webRequest.Headers.Add("Accept-Encoding", "gzip, deflate");
            webRequest.Headers.Add("Cache-Control", "no-cache");
            //写入JSON报文
            string header =
                "Content-Disposition: form-data; name=\"FaceDataRecord\";\r\n" +
                "Content-Type: application/json\r\n" +
                "Content-Length: " + request.Length + "\r\n\r\n";
            var headerBytes = Encoding.UTF8.GetBytes(header);
            var requestBytes = Encoding.UTF8.GetBytes(request);
            memStream.Write(beginBoundary, 0, beginBoundary.Length);
            memStream.Write(headerBytes, 0, headerBytes.Length);
            memStream.Write(requestBytes, 0, requestBytes.Length);

            var buffer = new byte[fileStream.Length];

            fileStream.Read(buffer, 0, buffer.Length);
            fileStream.Close();

            // 写入文件  
            header = "\r\n--" + boundary + "\r\n" +
                "Content-Disposition: form-data; name=\"FaceImage\";\r\n" +
                 "Content-Type: image/jpeg\r\n" +
                 "Content-Length: " + buffer.Length + "\r\n\r\n";
            headerBytes = Encoding.UTF8.GetBytes(header);

            memStream.Write(headerBytes, 0, headerBytes.Length);
            memStream.Write(buffer, 0, buffer.Length);
            var contentLine = Encoding.ASCII.GetBytes("\r\n");
            memStream.Write(contentLine, 0, contentLine.Length);

            // 写入最后的结束边界符  
            memStream.Write(endBoundary, 0, endBoundary.Length);

            webRequest.ContentLength = memStream.Length;

            var requestStream = webRequest.GetRequestStream();

            memStream.Position = 0;
            var tempBuffer = new byte[memStream.Length];
            memStream.Read(tempBuffer, 0, tempBuffer.Length);
            memStream.Close();

            requestStream.Write(tempBuffer, 0, tempBuffer.Length);
            requestStream.Close();

            try
            {
                var httpWebResponse = (HttpWebResponse)webRequest.GetResponse();

                using (var httpStreamReader = new StreamReader(httpWebResponse.GetResponseStream(),
                                                                Encoding.GetEncoding("utf-8")))
                {
                    responseContent = httpStreamReader.ReadToEnd();
                }
                httpWebResponse.Close();
            }
            catch (WebException ex)
            {
                MessageBox.Show(ex.ToString());
            }
            fileStream.Close();

            webRequest.Abort();

            return responseContent;
        }

        public string HttpCapData(string strUserName, string strPassword, string url,
            string filePath, string request)
        {
            string responseContent = string.Empty;
            var memStream = new MemoryStream();
            var webRequest = (HttpWebRequest)WebRequest.Create(url);

            // 设置属性  
            webRequest.Credentials = GetCredentialCache(url, strUserName, strPassword);
            webRequest.Timeout = m_iHttpTimeOut;
            webRequest.Method = "POST";
            webRequest.Accept = "text/html, application/xhtml+xml,";
            webRequest.Headers.Add("Accept-Language", "zh-CN");
            webRequest.ContentType = "application/xml";
            webRequest.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)";
            webRequest.Headers.Add("Accept-Encoding", "gzip, deflate");
            webRequest.Headers.Add("Cache-Control", "no-cache");

            var contentLine = Encoding.ASCII.GetBytes("\r\n");
            memStream.Write(contentLine, 0, contentLine.Length);
            //写入JSON报文
            var byXml = Encoding.UTF8.GetBytes(request);
            memStream.Write(byXml, 0, byXml.Length);
            memStream.Write(contentLine, 0, contentLine.Length);
            webRequest.ContentLength = memStream.Length;

            var requestStream = webRequest.GetRequestStream();

            memStream.Position = 0;
            var tempBuffer = new byte[memStream.Length];
            memStream.Read(tempBuffer, 0, tempBuffer.Length);
            memStream.Close();

            requestStream.Write(tempBuffer, 0, tempBuffer.Length);
            requestStream.Close();

            try
            {
                WebResponse wr = webRequest.GetResponse();
                string szContentType = wr.ContentType;
                byte[] szResponse = new byte[wr.ContentLength];
                byte[] buf = new byte[1024];
                int iRet = -1;
                int len = 0;
                while ((iRet = wr.GetResponseStream().Read(buf, 0, 1024)) > 0)
                {
                    Array.Copy(buf, 0, szResponse, len, iRet);
                    len += iRet;
                }

                int startIndex = 0, endIndex = 0;
                byte[] byTmp = new byte[17];
                byte[] byTmp2 = new byte[4];
                string szTmp = string.Empty;
                for (; endIndex + 17 < szResponse.Length; endIndex++)
                {
                    Buffer.BlockCopy(szResponse, endIndex, byTmp, 0, 17);
                    szTmp = Encoding.UTF8.GetString(byTmp);
                    if (szTmp.Equals("\r\n--MIME_boundary")) break;
                }
                endIndex += 17;
                //endIndex = szResponse.IndexOf("\r\n--MIME_boundary") + "\r\n--MIME_boundary".Length;
                //szResponse = szResponse.Substring(endIndex);
                string strpath = null;
                DateTime dt = DateTime.Now;
                strpath = string.Format("face.jpg", Environment.CurrentDirectory);
                try
                {
                    using (FileStream fs = new FileStream(strpath, FileMode.OpenOrCreate))
                    {
                        while (true)
                        {
                            for (; startIndex + 4 < szResponse.Length; startIndex++)
                            {
                                Buffer.BlockCopy(szResponse, startIndex, byTmp2, 0, 4);
                                szTmp = Encoding.UTF8.GetString(byTmp2);
                                if (szTmp.Equals("\r\n\r\n")) break;
                            }
                            startIndex += 4;
                            for (; endIndex + 17 < szResponse.Length; endIndex++)
                            {
                                Buffer.BlockCopy(szResponse, endIndex, byTmp, 0, 17);
                                szTmp = Encoding.UTF8.GetString(byTmp);
                                if (szTmp.Equals("\r\n--MIME_boundary")) break;
                            }
                            //endIndex = szResponse.IndexOf("\r\n--MIME_boundary");
                            if (startIndex >= szResponse.Length || endIndex >= szResponse.Length) break;
                            //byte[] by = Encoding.Unicode.GetBytes(szResponse.Substring(startIndex, endIndex - startIndex));
                            fs.Write(szResponse, startIndex, endIndex - startIndex);
                            endIndex += "\r\n--MIME_boundary".Length;
                            if (startIndex >= szResponse.Length || endIndex >= szResponse.Length) break;
                        }
                        fs.Close();
                    }
                }
                catch
                {
                    MessageBox.Show("capature data wrong", "Error", MessageBoxButtons.OK);
                }
                //var httpWebResponse = (HttpWebResponse)webRequest.GetResponse();

                //using (var httpStreamReader = new StreamReader(httpWebResponse.GetResponseStream(),
                //                                                Encoding.GetEncoding("utf-8")))
                //{
                //    responseContent = httpStreamReader.ReadToEnd();
                //}
                //httpWebResponse.Close();
            }
            catch (WebException ex)
            {
                MessageBox.Show(ex.ToString());
            }

            webRequest.Abort();

            return responseContent;
        }

        public void GetImage(string url)//添加下载图片url的方式
        {
            //using (WebClient client = new WebClient())
            //{
            //    client.Credentials = GetCredentialCache(url, AddDevice.struDeviceInfo.strDeviceNickName, AddDevice.struDeviceInfo.strPassword);
            //    WebHeaderCollection customerHeader = new WebHeaderCollection();
            //    customerHeader.Add(HttpRequestHeader.Accept, "application/xml");
            //    customerHeader.Add(HttpRequestHeader.ContentType, "application/json;charset=UTF-8");
            //    client.Headers = customerHeader;

            //    byte[] buffer = client.DownloadData(url);
            //    for (int i = 0; i < 100; ++i) ;

            //}
            //string szContentType = string.Empty;
            //HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            //request.Credentials = GetCredentialCache(url, AddDevice.struDeviceInfo.strDeviceNickName, AddDevice.struDeviceInfo.strPassword);
            //request.Timeout = 5000;
            ////request.ContentType = "application/json"
            //try
            //{
            //    byte[] byResponse = {0};
            //    szContentType = wr.ContentType;
            //    byResponse = new byte[wr.ContentLength];
            //    byte[] buf = new byte[wr.ContentLength];
            //    int iRet = -1;
            //    int len = 0;
            //    while ((iRet = wr.GetResponseStream().Read(buf, 0, (int)wr.ContentLength - len)) > 0)
            //    {
            //        Array.Copy(buf, 0, byResponse, len, iRet);
            //        len += iRet;
            //    }
            //    string szResponse = Encoding.UTF8.GetString(byResponse);
            //    using(System.IO.Stream stream = wr.GetResponseStream())
            //    {
            //        Image img = Image.FromStream(stream);
            //        //return img;
            //    }
            //}
            //catch (WebException ex)
            //{
            //    MessageBox.Show(ex.ToString());
            //    //return null;
            //}
        }

        public int HttpPut(string strUserName, string strPassword, string strUrl, string strHttpMethod, string param, ref string strResponse)
        {

            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(strUrl);
                request.Credentials = GetCredentialCache(strUrl, strUserName, strPassword);
                request.Method = strHttpMethod;
                request.Timeout = m_iHttpTimeOut;

                byte[] bs = Encoding.ASCII.GetBytes(param);
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = bs.Length;
                using (Stream reqStream = request.GetRequestStream())
                {
                    reqStream.Write(bs, 0, bs.Length);
                }

                WebResponse wr = request.GetResponse();
                strResponse = new StreamReader(wr.GetResponseStream()).ReadToEnd();

                return (int)HttpStatus.Http200;
            }
            catch (WebException ex)
            {
                WebResponse wr = ex.Response;
                if (wr != null)
                {
                    Stream st = wr.GetResponseStream();
                    StreamReader sr = new StreamReader(st, System.Text.Encoding.Default);
                    strResponse = sr.ReadToEnd();
                    sr.Close();
                    st.Close();
                    return (int)HttpStatus.HttpOther;
                }
                else
                {
                    strResponse = ex.Status.ToString();
                    return (int)HttpStatus.HttpTimeOut;
                }
            }
            finally
            {

            }
        }

        public int HttpPut(DeviceInfo struDeviceInfo, string strUserName, string strPassword, string strUrl, string strHttpMethod, string param, ref string strResponse)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(strUrl);
            request.Credentials = new NetworkCredential(strUserName, strPassword);
            request.Method = strHttpMethod;
            request.Timeout = m_iHttpTimeOut;

            byte[] bs = Encoding.ASCII.GetBytes(param);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = bs.Length;
            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(bs, 0, bs.Length);
            }

            try
            {
                WebResponse wr = request.GetResponse();
                strResponse = new StreamReader(wr.GetResponseStream()).ReadToEnd();
                return (int)HttpStatus.Http200;
            }
            catch (WebException ex)
            {
                WebResponse wr = ex.Response;
                if (wr != null)
                {
                    Stream st = wr.GetResponseStream();
                    StreamReader sr = new StreamReader(st, System.Text.Encoding.Default);
                    strResponse = sr.ReadToEnd();
                    sr.Close();
                    st.Close();
                    return (int)HttpStatus.HttpOther;
                }
                else
                {
                    strResponse = ex.Status.ToString();
                    return (int)HttpStatus.HttpTimeOut;
                }
            }
            finally
            {

            }
        }

        public int HttpPut(string strUrl, string strHttpMethod, string param, ref string strResponse)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(strUrl);
            request.Method = strHttpMethod;
            //             request.Timeout = m_iHttpTimeOut;

            byte[] bs = Encoding.ASCII.GetBytes(param);

            request.ContentType = "text/xml";
            request.ContentLength = bs.Length;

            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(bs, 0, bs.Length);
            }

            try
            {
                WebResponse wr = request.GetResponse();
                strResponse = new StreamReader(wr.GetResponseStream()).ReadToEnd();
                return (int)HttpStatus.Http200;
            }
            catch (WebException ex)
            {
                WebResponse wr = ex.Response;
                if (wr != null)
                {
                    Stream st = wr.GetResponseStream();
                    StreamReader sr = new StreamReader(st, System.Text.Encoding.Default);
                    strResponse = sr.ReadToEnd();
                    sr.Close();
                    st.Close();
                    return (int)HttpStatus.HttpOther;
                }
                else
                {
                    strResponse = ex.Status.ToString();
                    return (int)HttpStatus.HttpTimeOut;
                }
            }
            finally
            {

            }
        }

        public int HttpPut(string strUrl, string strHttpMethod, XmlDocument param, ref string strResponse)
        {
            string szRequest = FormatXmlString(param);
            return HttpPut(strUrl, strHttpMethod, szRequest, ref strResponse);
        }

        public int ParserUserCheck(string httpBody, ref string statusCode, ref string statusString)
        {
            try
            {
                if (httpBody != string.Empty)
                {
                    XmlDocument xml = new XmlDocument();
                    xml.LoadXml(httpBody);
                    if (xml.DocumentElement != null && xml.DocumentElement.Name == "userCheck")
                    {
                        XmlNodeList childNode = xml.DocumentElement.ChildNodes;
                        foreach (XmlNode node in childNode)
                        {
                            if (node.Name == "statusValue")
                            {
                                statusCode = node.InnerText;
                            }
                            if (node.Name == "statusString")
                            {
                                statusString = node.InnerText;
                            }
                        }
                    }
                }
            }
            catch
            {

            }

            return 0;
        }

        public int ParserResponseStatus(string httpBody, ref string statusCode, ref string statusString)
        {
            if (httpBody == "Timeout")
            {
                return 0;
            }

            try
            {

                if (httpBody != string.Empty)
                {
                    XmlDocument xml = new XmlDocument();
                    xml.LoadXml(httpBody);
                    if (xml.DocumentElement != null && xml.DocumentElement.Name == "ResponseStatus")
                    {
                        XmlNodeList childNode = xml.DocumentElement.ChildNodes;
                        foreach (XmlNode node in childNode)
                        {
                            if (node.Name == "statusCode")
                            {
                                statusCode = node.InnerText;
                            }
                            if (node.Name == "statusString")
                            {
                                statusString = node.InnerText;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                statusString = httpBody;
            }
            return 0;
        }

        public static string FormatXmlString(XmlDocument document)
        {
            MemoryStream stream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(stream, null);
            writer.Formatting = Formatting.Indented;
            document.Save(writer);

            StreamReader streamReader = new StreamReader(stream, System.Text.Encoding.UTF8);
            stream.Position = 0;

            string returnValue = streamReader.ReadToEnd();

            streamReader.Close();
            stream.Close();

            return returnValue;
        }

        private static void RespCallback(IAsyncResult asynchronousResult)
        {
            // State of request is asynchronous.
            RequestState myRequestState = (RequestState)asynchronousResult.AsyncState;
            try
            {
                HttpWebRequest myHttpWebRequest = myRequestState.request;
                myRequestState.response = (HttpWebResponse)myHttpWebRequest.EndGetResponse(asynchronousResult);

                string strBoundary = myRequestState.response.ContentType;
                int nIndex = strBoundary.IndexOf("boundary=");
                if (nIndex >= 0)
                {
                    strBoundary = strBoundary.Substring(nIndex + "boundary=".Length);
                    myRequestState.strBoundary = strBoundary;
                }

                // Read the response into a Stream object.
                Stream responseStream = myRequestState.response.GetResponseStream();
                myRequestState.streamResponse = responseStream;

                // Begin the Reading of the contents of the HTML page and print it to the console.
                IAsyncResult asynchronousInputRead = responseStream.BeginRead(myRequestState.BufferRead, 0, BUFFER_SIZE, new AsyncCallback(ReceiveData), myRequestState);
                return;
            }
            catch (WebException e)
            {
                myRequestState.eStatus = e;
            }
        }



        private void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            RequestState myRequestState = (RequestState)asynchronousResult.AsyncState;
            try
            {
                using (Stream postStream = myRequestState.request.EndGetRequestStream(asynchronousResult))
                {
                    // Convert the string into a byte array.
                    byte[] byteArray = null;

                    while (true)
                    {
                        if (myRequestState.processSendData == null)
                        {
                            postStream.Close();
                            break;
                        }

                        if (!myRequestState.processSendData(ref byteArray, myRequestState.objectUserData))
                        {
                            break;
                        }
                        if (byteArray != null)
                        {
                            postStream.Write(byteArray, 0, byteArray.Length);
                        }

                    }

                }

            }
            catch (WebException ex)
            {
                myRequestState.eStatus = ex;

            }
        }

    }
}
