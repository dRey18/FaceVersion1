using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace FaceManagementSetPhoto
{
    class DllInterface
    {
        public const int FFMPEG_NOERROR = 0;//no error
        public const int FFMPEG_DATA_ERROR = 1;//Wrong data length
        public const int FFMPEG_SDL_INIT_ERROR = 2;//SDL init failed
        public const int FFMPEG_FFMPEG_INIT_ERROR = 3;//FFmpeg init failed
        public const int FFMPEG_CREATE_THREAD_ERROR = 4;//create thread failed


        public enum DataTypeCode 
        {
            enumMediaInfo = 0,
            enumRtpPacket,
            enumMetadata,
            enumError,
            enumThermalData
        }

        public enum RtspReturn
        {
            enumRtspFalse = 0,
            enumRtspTrue
        }

        public enum AudioEncodeTypeEx
        {
            AUDIO_TYPE_PCM    = 0x00,
            AUDIO_TYPE_G711A  = 0x01,
            AUDIO_TYPE_G711U  = 0x02,
            AUDIO_TYPE_G722   = 0x03,
            AUDIO_TYPE_G726   = 0x04,
            AUDIO_TYPE_MPEG2  = 0x05,
            AUDIO_TYPE_AAC    = 0x06
        }

        public enum AudioEncodeType
        {
            AUDIO_TYPE_PCM_S16K    = 0x00,
            AUDIO_TYPE_G711A_S8K   = 0x01,
            AUDIO_TYPE_G711U_S8K   = 0x02,
            AUDIO_TYPE_G722_S16K   = 0x03,
            AUDIO_TYPE_G726_S8K    = 0x04,
            AUDIO_TYPE_MPEG2_S16K  = 0x05,
            AUDIO_TYPE_AAC_S32K    = 0x06,
            AUDIO_TYPE_PCM_S8K     = 0x07,
            AUDIO_TYPE_PCM_S32K    = 0x08,
            AUDIO_TYPE_AAC_S16K    = 0x09
        }

        public enum VideoCodecType
        {
            H264 = 0x00,
            H265 = 0x01,
            MPEG4 = 0x02
        }

        public delegate void DataCallBack(int code, IntPtr rtpPacket, int len);
        public delegate void OutputDataCallBack(IntPtr pstDataInfo, IntPtr pUser);

        // RTSP.dll
        [DllImport("RTSP.dll", EntryPoint = "CreateRtsp")]
        public static extern IntPtr CreateRtsp(bool store);

        [DllImport("RTSP.dll", EntryPoint = "DeleteRtsp")]
        public static extern IntPtr DeleteRtsp(IntPtr pRtsp);

        // Return:        if success return 1, otherwise return 0.
        [DllImport("RTSP.dll", EntryPoint = "StartGetRtspData")]
        public static extern int StartGetRtspData(IntPtr pRtsp, ref RtspDeviceInfo struRtspDeviceInfo, DataCallBack callback);

        // Return:        if success return 1, otherwise return 0.
        [DllImport("RTSP.dll", EntryPoint = "StopGetRtspData")]
        public static extern int StopGetRtspData(IntPtr pRtsp);

        [DllImport("RTSP.dll", EntryPoint = "SetScale")]
        public static extern int SetScale(IntPtr pRtsp, float scale);

        [DllImport("RTSP.dll", EntryPoint = "PauseGetRtspData")]
        public static extern int PauseGetRtspData(IntPtr pRtsp);

        [DllImport("RTSP.dll", EntryPoint = "ContinueGetRtspData")]
        public static extern int ContinueGetRtspData(IntPtr pRtsp);


        // PlayCtrl.dll
        [DllImport("PlayCtrl\\PlayCtrl.dll", EntryPoint = "PlayM4_GetPort")]
        public static extern int PlayM4_GetPort(ref int port);

        [DllImport("PlayCtrl\\PlayCtrl.dll", EntryPoint = "PlayM4_GetLastError")]
        public static extern uint PlayM4_GetLastError(int port);

        [DllImport("PlayCtrl\\PlayCtrl.dll", EntryPoint = "PlayM4_SetStreamOpenMode")]
        public static extern int PlayM4_SetStreamOpenMode(int nPort,uint nMode);

        [DllImport("PlayCtrl\\PlayCtrl.dll", EntryPoint = "PlayM4_OpenStream")]
        public static extern int PlayM4_OpenStream(int nPort, byte[] pFileHeadBuf, uint nSize, uint nBufPoolSize);

        [DllImport("PlayCtrl\\PlayCtrl.dll", EntryPoint = "PlayM4_SetDisplayBuf")]
        public static extern int PlayM4_SetDisplayBuf(int nPort, uint nNum);

        [DllImport("PlayCtrl\\PlayCtrl.dll", EntryPoint = "PlayM4_Play")]
        public static extern int PlayM4_Play(int nPort, IntPtr hWnd);

        [DllImport("PlayCtrl\\PlayCtrl.dll", EntryPoint = "PlayM4_Fast")]
        public static extern int PlayM4_Fast(int nPort);

        [DllImport("PlayCtrl\\PlayCtrl.dll", EntryPoint = "PlayM4_Slow")]
        public static extern int PlayM4_Slow(int nPort);

        [DllImport("PlayCtrl\\PlayCtrl.dll", EntryPoint = "PlayM4_InputData")]
        public static extern int PlayM4_InputData(int nPort, byte[] pBuf, uint nSize);

        [DllImport("PlayCtrl\\PlayCtrl.dll", EntryPoint = "PlayM4_Stop")]
        public static extern int PlayM4_Stop(int nPort);

        [DllImport("PlayCtrl\\PlayCtrl.dll", EntryPoint = "PlayM4_CloseStream")]
        public static extern int PlayM4_CloseStream(int nPort);

        [DllImport("PlayCtrl\\PlayCtrl.dll", EntryPoint = "PlayM4_FreePort")]
        public static extern int PlayM4_FreePort(int nPort);

        public delegate void DRAWFUN(int nPort, System.IntPtr hDc, int nUser);
        [DllImport("PlayCtrl\\PlayCtrl.dll", EntryPoint = "PlayM4_RegisterDrawFun")]
        public static extern bool PlayM4_RegisterDrawFun(int nPort, DRAWFUN DrawFun, int nUser);

        [DllImport("PlayCtrl\\PlayCtrl.dll", EntryPoint = "PlayM4_RenderPrivateData")]
        public static extern int PlayM4_RenderPrivateData(int nPort, uint nIntelType, int bTrue);

        [DllImport("PlayCtrl\\PlayCtrl.dll", EntryPoint = "PlayM4_GetSystemTime")]
        public static extern int PlayM4_GetSystemTime(int nPort, ref PLAYM4_SYSTEM_TIME pstTime);

        // nPause 1：pause，0：play 
        [DllImport("PlayCtrl\\PlayCtrl.dll", EntryPoint = "PlayM4_Pause")]
        public static extern int PlayM4_Pause(int nPort, int nPause);

        [DllImport("PlayCtrl\\PlayCtrl.dll", EntryPoint = "PlayM4_ReversePlay")]
        public static extern int PlayM4_ReversePlay(int nPort);

        [DllImport("PlayCtrl\\PlayCtrl.dll", EntryPoint = "PlayM4_OneByOne")]
        public static extern int PlayM4_OneByOne(int nPort);

        [DllImport("PlayCtrl\\PlayCtrl.dll", EntryPoint = "PlayM4_GetSourceBufferRemain")]
        public static extern int PlayM4_GetSourceBufferRemain(int nPort);

        [DllImport("PlayCtrl\\PlayCtrl.dll", EntryPoint = "PlayM4_SetSourceBufCallBack")]
        public static extern int PlayM4_SetSourceBufCallBack(int nPort, int nThreShold, IntPtr fSourceBufCallBackFun, int dvUser, IntPtr pReserved);

        [DllImport("PlayCtrl\\PlayCtrl.dll", EntryPoint = "PlayM4_GetBufferValue")]
        public static extern int PlayM4_GetBufferValue(int nPort, int nBufType);

        [DllImport("PlayCtrl\\PlayCtrl.dll", EntryPoint = "PlayM4_ResetSourceBufFlag")]
        public static extern int PlayM4_ResetSourceBufFlag(int nPort);

        [DllImport("PlayCtrl\\PlayCtrl.dll", EntryPoint = "PlayM4_GetDisplayBuf")]
        public static extern int PlayM4_GetDisplayBuf(int nPort);

        [DllImport("PlayCtrl\\PlayCtrl.dll", EntryPoint = "PlayM4_SetDisplayBuf")]
        public static extern int PlayM4_SetDisplayBuf(int nPort, int nNum);

        //Crypt
        [DllImport(@"./Crypt.dll", EntryPoint = "SSL_CreateSSLEncrypt")]
        public static extern IntPtr SSL_CreateSSLEncrypt();

        [DllImport(@"./Crypt.dll", EntryPoint = "SSL_DestroySSLEncrypt")]
        public static extern void SSL_DestroySSLEncrypt(IntPtr pInstance);

        [DllImport(@"./Crypt.dll", EntryPoint = "SSL_GenerateRSAPublicKey")]
        public static extern bool SSL_GenerateRSAPublicKey(IntPtr pInstance, IntPtr pKey, ref int len);

        [DllImport(@"./Crypt.dll", EntryPoint = "SSL_DecryptByPrivateKey")]
        public static extern int SSL_DecryptByPrivateKey(IntPtr pInstance, int iInputBufLen, IntPtr pInputBuf, IntPtr pOutputBuf);

        [DllImport(@"./Crypt.dll", EntryPoint = "SSL_AESEncrypt")]
        public static extern void SSL_AESEncrypt(IntPtr pUserKey, IntPtr pIn, IntPtr pOut);

        [DllImport(@"./Crypt.dll", EntryPoint = "SSL_AESDecrypt")]
        public static extern void SSL_AESDecrypt(IntPtr pUserKey, IntPtr pIn, IntPtr pOut);

        [DllImport(@"./Crypt.dll", EntryPoint = "AES_CreateAESEncrypt")]
        public static extern IntPtr AES_CreateAESEncrypt();

        [DllImport(@"./Crypt.dll", EntryPoint = "AES_DestroyAESEncrypt")]
        public static extern void AES_DestroyAESEncrypt(IntPtr pInstance);

        [DllImport(@"./Crypt.dll", EntryPoint = "AES_EncryptContent")]
        public static extern void AES_EncryptContent(IntPtr pClientAES, IntPtr pInitVector, IntPtr pUserName, int iNameLen, IntPtr pSalt, IntPtr pPassword, int iPswLen, IntPtr pSrcContent, IntPtr pOut);



        //ISAPITimeBarActiveX.ocx
        [DllImport("ISAPITimeBarActiveX.ocx")]
        public static extern int DllRegisterServer();

        [DllImport("ISAPITimeBarActiveX.ocx")]
        public static extern int DllUnregisterServer();

        //AudioIntercom.dll
        [DllImport("PlayCtrl\\AudioIntercom.dll", EntryPoint = "AUDIOCOM_GetSoundCardNum")]
        public static extern int AUDIOCOM_GetSoundCardNum(ref uint pdwDeviceNum);

        [DllImport("PlayCtrl\\AudioIntercom.dll", EntryPoint = "AUDIOCOM_GetLastError")]
        public static extern int AUDIOCOM_GetLastError(int nPort);

        [DllImport("PlayCtrl\\AudioIntercom.dll", EntryPoint = "AUDIOCOM_GetOneSoundCardInfo")]
        public static extern int AUDIOCOM_GetOneSoundCardInfo(uint dwDeviceIndex, ref SOUND_CARD_INFO pstDeviceInfo);

        [DllImport("PlayCtrl\\AudioIntercom.dll", EntryPoint = "AUDIOCOM_CreateCaptureHandle")]
        public static extern int AUDIOCOM_CreateCaptureHandle(ref int piCapturePort, string pDeviceName);

        [DllImport("PlayCtrl\\AudioIntercom.dll", EntryPoint = "AUDIOCOM_RegisterOutputDataCallBackEx")]
        public static extern int AUDIOCOM_RegisterOutputDataCallBackEx(int iCapturePort, ref AudioParam pstAudioParam, OutputDataCallBack pfnOutputDataCallBack, IntPtr pUser);

        [DllImport("PlayCtrl\\AudioIntercom.dll", EntryPoint = "AUDIOCOM_StartCapture")]
        public static extern int AUDIOCOM_StartCapture(int iCapturePort);

        [DllImport("PlayCtrl\\AudioIntercom.dll", EntryPoint = "AUDIOCOM_StopCapture")]
        public static extern int AUDIOCOM_StopCapture(int iCapturePort);

        [DllImport("PlayCtrl\\AudioIntercom.dll", EntryPoint = "AUDIOCOM_ReleaseCaptureHandle")]
        public static extern int AUDIOCOM_ReleaseCaptureHandle(int iCapturePort);

        [DllImport("PlayCtrl\\AudioIntercom.dll", EntryPoint = "AUDIOCOM_CreatePlayHandle")]
        public static extern int AUDIOCOM_CreatePlayHandle(ref int nPlayPort, string pDeviceName);

        [DllImport("PlayCtrl\\AudioIntercom.dll", EntryPoint = "AUDIOCOM_OpenStream")]
        public static extern int AUDIOCOM_OpenStream(int nPlayPort, AudioEncodeType enDataType);

        [DllImport("PlayCtrl\\AudioIntercom.dll", EntryPoint = "AUDIOCOM_InputStreamData")]
        public static extern int AUDIOCOM_InputStreamData(int nPlayPort, IntPtr pData, uint dwDataLen);

        [DllImport("PlayCtrl\\AudioIntercom.dll", EntryPoint = "AUDIOCOM_StartPlay")]
        public static extern int AUDIOCOM_StartPlay(int nPlayPort);

        [DllImport("PlayCtrl\\AudioIntercom.dll", EntryPoint = "AUDIOCOM_StopPlay")]
        public static extern int AUDIOCOM_StopPlay(int nPlayPort);

        [DllImport("PlayCtrl\\AudioIntercom.dll", EntryPoint = "AUDIOCOM_ReleasePlayHandle")]
        public static extern int AUDIOCOM_ReleasePlayHnadle(int nPlayPort);

        [DllImport("gdi32.dll")]  
        public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("gdi32.dll")]  
        public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        [DllImport("gdi32.dll")]  
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreatePen(int nPenStyle, int nWidth, Int32 crColor);

        [DllImport("gdi32.dll")]
        public static extern bool MoveToEx(IntPtr hDC, int x, int y, IntPtr lpPoint);

        [DllImport("gdi32.dll")]
        public static extern bool LineTo(IntPtr hDC, int x, int y);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        public static extern bool TextOutW(IntPtr hdc, int nXStart, int nYStart, string lpString, int nLength);

        //FFmpeg
        [DllImport("FFmpeg\\FFmpeg2.dll", EntryPoint = "CreateFFmpeg")]
        public static extern IntPtr CreateFFmpeg();

        [DllImport("FFmpeg\\FFmpeg2.dll", EntryPoint = "DeleteFFmpeg")]
        public static extern void DeleteFFmpeg(IntPtr pFFmpeg);

        [DllImport("FFmpeg\\FFmpeg2.dll", EntryPoint = "AddRtpPktToQueue")]
        public static extern int AddRtpPktToQueue(IntPtr pFFmpeg, int len, IntPtr data);

        [DllImport("FFmpeg\\FFmpeg2.dll", EntryPoint = "SetAudioAndVideoParam")]
        public static extern void SetAudioAndVideoParam(IntPtr pFFmpeg, FFmpegAudioAndVideoInfo param);

        [DllImport("FFmpeg\\FFmpeg2.dll", EntryPoint = "Start")]
        public static extern void Start(IntPtr pFFmpeg);

        [DllImport("FFmpeg\\FFmpeg2.dll", EntryPoint = "StreamCreateFFmpeg")]
        public static extern IntPtr StreamCreateFFmpeg();

        [DllImport("FFmpeg\\FFmpeg2.dll", EntryPoint = "StreamStart")]
        public static extern void StreamStart(IntPtr pFFmpeg, string path, IntPtr hwnd);

        [DllImport("FFmpeg\\FFmpeg2.dll", EntryPoint = "StreamDeleteFFmpeg")]
        public static extern void StreamDeleteFFmpeg(IntPtr pFFmpeg);
    }

    public struct FFmpegAudioAndVideoInfo
    {
        public int width;
        public int height;
        public float fps;
        public int videoCodecType;
        public int audioCodecType;
        public bool audioEnable;
    }



    public struct RtspDeviceInfo
    {
        public int iIP;
        public int iPort;
        public int iChannel;
        public int iChannelType;
        public string strUsername;
        public string strPassword;
        public string strUrl;
        public string strParam; // have ?
        public float scale;
        public bool bGetMetadata;
        public int nRtspConnectTimeout;//ms
        public int nRtspReceiveTimeout;//ms
    }

    public struct PLAYM4_SYSTEM_TIME
    {
        public int dwYear;
        public int dwMon;
        public int dwDay;
        public int dwHour;
        public int dwMin;
        public int dwSec;
        public int dwMs;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SOUND_CARD_INFO
    {
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 128)]
        public char[]   byDeviceName;     ///<设备名称
        public uint     dwFrequency;      ///<采集频率
        public uint     dwRefresh;        ///<刷新频率
        public uint     dwSync;           ///<同步
        public uint     dwMonoSources;    ///<单声道源数量
        public uint     dwStereoSources;  ///<多声道源数量
        public uint     dwMajorVersion;   ///<主版本号
        public uint     dwMinorVersion;   ///<次版本号
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 16, ArraySubType = UnmanagedType.U4)]
        public uint[]   dwReserved;       ///<保留参数
    }

    public struct AudioParam
    {
        public System.UInt16 nChannel;           ///<PCM声道数
        public System.UInt16 nBitWidth;          ///<PCM位宽
        public uint      nSampleRate;        ///<PCM采样率
        public uint      nBitRate;           ///<编码比特率
        public uint      enAudioEncodeTypeEx;///<编解码类别
    }

    public struct OutputDataInfo 
    {
        public IntPtr    pData;
        public uint      dwDataLen;
        public uint      enDataType;
    }
}
