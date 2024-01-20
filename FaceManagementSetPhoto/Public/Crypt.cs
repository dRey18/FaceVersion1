using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FaceManagementSetPhoto
{
    class Crypt
    {
        /// <summary>
        /// generate RSA public key
        /// </summary>
        /// <param name="hHandle"></param>
        /// <param name="iLen"></param>
        /// <returns></returns>
        public static byte[] fnMKRsaPublickey(IntPtr hHandle, ref int iLen)
        {
            IntPtr iPublicKey = Marshal.AllocHGlobal(256);
            //generate RSA public key buffer
            DllInterface.SSL_GenerateRSAPublicKey(hHandle, iPublicKey, ref iLen);
            byte[] byPublicKey = new byte[iLen];
            Marshal.Copy(iPublicKey, byPublicKey, 0, iLen);

            Marshal.FreeHGlobal(iPublicKey);
            return byPublicKey;
        }

        /// <summary>
        /// RSA decryption
        /// </summary>
        /// <param name="hHandle"></param>
        /// <param name="bySrc"></param>
        /// <param name="iSrcLen"></param>
        /// <param name="iDstLen"></param>
        /// <returns></returns>
        public static byte[] fnRsaDecrypt(IntPtr hHandle, byte[] bySrc, int iSrcLen, ref int iDstLen)
        {
            IntPtr pEncodeChallenge = Marshal.AllocHGlobal(iSrcLen);
            Marshal.Copy(bySrc, 0, pEncodeChallenge, iSrcLen);

            IntPtr pOut = Marshal.AllocHGlobal(1024);
            iDstLen = DllInterface.SSL_DecryptByPrivateKey(hHandle, iSrcLen, pEncodeChallenge, pOut);

            Byte[] byChallenge = new byte[iDstLen];
            Marshal.Copy(pOut, byChallenge, 0, iDstLen);
            Marshal.FreeHGlobal(pEncodeChallenge);
            Marshal.FreeHGlobal(pOut);

            return byChallenge;
        }

        /// <summary>
        /// AES encryption
        /// </summary>
        /// <param name="byKey"></param>
        /// <param name="bySrc"></param>
        /// <returns></returns>
        public static byte[] fnAESEncrypt(byte[] byKey, byte[] bySrc)
        {
            IntPtr pKey = Marshal.AllocHGlobal(byKey.Length);
            Marshal.Copy(byKey, 0, pKey, byKey.Length);
            int iDstLen = (bySrc.Length % 16 == 0) ? bySrc.Length : (bySrc.Length / 16 + 1) * 16;
            byte[] bySrcTmp = new byte[iDstLen];
            Array.Copy(bySrc, bySrcTmp, bySrc.Length);

            IntPtr pSrc = Marshal.AllocHGlobal(iDstLen);
            Marshal.Copy(bySrcTmp, 0, pSrc, bySrcTmp.Length);

            IntPtr pDst = Marshal.AllocHGlobal(iDstLen);
            DllInterface.SSL_AESEncrypt(pKey, pSrc, pDst);
            byte[] byAesEn = new byte[iDstLen];
            Marshal.Copy(pDst, byAesEn, 0, byAesEn.Length);

            Marshal.FreeHGlobal(pKey);
            Marshal.FreeHGlobal(pSrc);
            Marshal.FreeHGlobal(pDst);
            return byAesEn;
        }


        /// <summary>
        /// AES encryption
        /// </summary>
        /// <param name="byKey"></param>
        /// <param name="bySrc"></param>
        /// <returns></returns>
        public static byte[] fnAESDecrypt(byte[] byKey, byte[] bySrc)
        {
            IntPtr pKey = Marshal.AllocHGlobal(byKey.Length);
            Marshal.Copy(byKey, 0, pKey, byKey.Length);
            IntPtr pSrc = Marshal.AllocHGlobal(bySrc.Length);
            Marshal.Copy(bySrc, 0, pSrc, bySrc.Length);

            //             int iDstLen = (bySrc.Length % 16 == 0) ? bySrc.Length : (bySrc.Length / 16 + 1) * 16;
            IntPtr pDst = Marshal.AllocHGlobal(bySrc.Length);
            DllInterface.SSL_AESDecrypt(pKey, pSrc, pDst);
            byte[] byAesEn = new byte[bySrc.Length];
            Marshal.Copy(pDst, byAesEn, 0, byAesEn.Length);

            Marshal.FreeHGlobal(pKey);
            Marshal.FreeHGlobal(pSrc);
            Marshal.FreeHGlobal(pDst);

            return byAesEn;
        }

        /// <summary>
        /// convert from Hex to byte，for example:"0x11" to 0x11
        /// </summary>
        /// <param name="chstr"></param>
        /// <returns></returns>
        public static byte hexToBinary(byte chstr)
        {
            char crtn = '\0';
            if (('0' <= chstr) && ('9' >= chstr))
            {
                crtn = (char)(chstr & 0x0F);
            }
            else if (('A' <= chstr) && ('F' >= chstr))
            {
                crtn = (char)(chstr - 'A' + 10);
            }
            else if (('a' <= chstr) && ('f' >= chstr))
            {
                crtn = (char)(chstr - 'a' + 10);
            }
            return (byte)crtn;
        }

        /// <summary>
        /// convert from Hex to byte,for example:"0x11" to 0x11
        /// </summary>
        /// <param name="pSrc"></param>
        /// <param name="nSrcLen"></param>
        /// <returns></returns>
        public static byte[] convertCharArrayToByteArray(byte[] pSrc, int nSrcLen)
        {
            byte[] byChallengeDst2 = new byte[nSrcLen / 2];

            //         if (nSrcLen > (1024 * 2))
            //         {
            //             return null;
            //         }

            for (int i = 0; i < nSrcLen; i = i + 2)
            {
                byChallengeDst2[i / 2] = (byte)(hexToBinary(pSrc[i]) << 4);
                byChallengeDst2[i / 2] += (byte)hexToBinary(pSrc[i + 1]);
            }

            return byChallengeDst2;
        }

        /// <summary>
        /// from byte[] to Hex,for example:0x11 to "0x11"
        /// </summary>
        /// <param name="pSrc"></param>
        /// <param name="nSrcLen"></param>
        /// <returns></returns>
        public static byte[] converByteArrayToCharArray(byte[] pSrc, int nSrcLen)
        {
            StringBuilder strB = new StringBuilder();
            for (int i = 0; i < nSrcLen; i++)
            {
                strB.Append(pSrc[i].ToString("X2"));
            }
            return System.Text.Encoding.Default.GetBytes(strB.ToString());
        }


        /// <summary>
        /// generate base64 encoded RSA public key 
        /// </summary>
        /// <param name="hHandle"></param>
        /// <returns></returns>
        public static string fnMKRsaPublickey(IntPtr hHandle)
        {
            int iLen = 0;
            byte[] byPublicKey = fnMKRsaPublickey(hHandle, ref iLen);
            //convert the public key buffer to hex 
            byte[] byStrPublicKey = converByteArrayToCharArray(byPublicKey, iLen);
            //base64转换
            return Convert.ToBase64String(byStrPublicKey);
        }

        /// <summary>
        /// Decrypt challenge
        /// </summary>
        /// <param name="hHandle"></param>
        /// <param name="szChallenge"></param>
        /// <param name="iLen"></param>
        /// <returns></returns>
        public static byte[] fnDecryptChallenge(IntPtr hHandle, string szChallenge, ref int iLen)
        {
            //decode base64 string 
            byte[] byEncoder = Convert.FromBase64String(szChallenge);
            //the challenge need to convert from hex to byte array
            byte[] byChallengeEncoder = convertCharArrayToByteArray(byEncoder, byEncoder.Length);


            //calculate byChallenge length
            int iSize = byEncoder.Length / 2;
            //decrypt use RSA
            byte[] byChallenge = fnRsaDecrypt(hHandle, byChallengeEncoder, iSize, ref iLen);

            return byChallenge;
        }
    }
}
