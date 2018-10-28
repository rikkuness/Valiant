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

namespace BlueprintReality.MixCast
{
    public class MixCastPath
    {
        private const string COMPANY_AND_PRODUCT = @"Blueprint Reality\MixCast VR";

        public const string REGISTRY_PATH = @"SOFTWARE\" + COMPANY_AND_PRODUCT;

        public static string MyDocuments
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + COMPANY_AND_PRODUCT;
            }
        }

        public static string LocalApplicationData
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + COMPANY_AND_PRODUCT;
            }
        }
    }
}
#endif
