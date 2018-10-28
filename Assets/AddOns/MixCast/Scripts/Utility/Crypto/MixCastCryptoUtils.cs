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
using System;
using System.Collections.Generic;

using Org.BouncyCastle.Crypto;

namespace BlueprintReality.MixCast
{
    public class MixCastCryptoUtils
    {
        public static byte[] ProcessCipher(byte[] data, IAsymmetricBlockCipher cipher)
        {
            if (cipher == null || data == null) { return null; }

            List<byte> result = new List<byte>();
            int blockSize = cipher.GetInputBlockSize();

            for (int offset = 0; offset < data.Length; offset += blockSize) {
                int nextBlockSize = Math.Min(data.Length - offset, blockSize);
                byte[] processedBlock = cipher.ProcessBlock(data, offset, nextBlockSize);
                result.AddRange(processedBlock);
            }

            return result.ToArray();
        }

        public static bool BytesEqual(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) { return false; }

            for (int i = 0; i < a.Length; i++) {
                if (a[i] != b[i]) {
                    return false;
                }
            }

            return true;
        }
    }
}
#endif
