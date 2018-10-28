/**********************************************************************************
* Blueprint Reality Inc. CONFIDENTIAL
* 2018 Blueprint Reality Inc.
* All Rights Reserved.
*
* NOTICE:  All information contained herein is, and remains, the property of
* Blueprint Reality Inc. and its suppliers, if any.  The intellectual and
* technical concepts contained herein are proprietary to Blueprint Reality Inc.
* and its suppliers and may be covered by Patents, pending patents, and are
* protected by trade secret or copyright law.
*
* Dissemination of this information or reproduction of this material is strictly
* forbidden unless prior written permission is obtained from Blueprint Reality Inc.
***********************************************************************************/

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Security;

namespace BlueprintReality.MixCast
{
    public class MixCastDecrypter
    {
        private Pkcs1Encoding _Cipher;

        public MixCastDecrypter(byte[] privateKey)
        {
            Init(privateKey);
        }

        public void Init(byte[] privateKey)
        {
            var key = PrivateKeyFactory.CreateKey(privateKey);
            _Cipher = new Pkcs1Encoding(new RsaEngine());
            _Cipher.Init(false, key);
        }

        public byte[] Decrypt(byte[] data)
        {
            return MixCastCryptoUtils.ProcessCipher(data, _Cipher);
        }
    }
}
#endif
