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
using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace BlueprintReality.MixCast
{
    [RequireComponent(typeof(RawImage))]
    public class SetImageFromMixCastData : MonoBehaviour
    {
        public enum DataType { Mid, Range, Depth, CIELab, File }
        public DataType textureDataType = DataType.File;
        public string fileName;
        public int textureWidth = 1280;
        public int textureHeight = 720;
        public TextureFormat textureFormat = TextureFormat.RGB24;
        public bool setScale = false;

        private InputFeed feed;
        private RawImage image;

        void OnEnable()
        {
            image = GetComponent<RawImage>();
        }


        private void LateUpdate()
        {
            if (textureDataType == DataType.Mid)
            {
                image.texture = GetTexFromFile("static_mid");
            }
            else if (textureDataType == DataType.Range)
            {
                image.texture = GetTexFromFile("static_range");
            }
            else if (textureDataType == DataType.Depth)
            {
                image.texture = GetTexFromFile("static_depth");
            }
            else if (textureDataType == DataType.CIELab)
            {
                image.texture = GetTexFromFile("static_cielab");
            }
            else
            {
                image.texture = GetTexFromFile();
            }

            image.SetMaterialDirty();

            if (setScale && image.texture != null)
            {
                image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (float)image.texture.width / image.texture.height * image.rectTransform.rect.height);
            }
        }

        private Texture2D tex = null;
        private Texture2D GetTexFromFile() { return GetTexFromFile(fileName); }
        private Texture2D GetTexFromFile(string file)
        {
            if (tex == null)
            {
                string programDataFolder = new DirectoryInfo(Application.persistentDataPath).Parent.Parent.FullName;
                string folderPath = Path.Combine(programDataFolder, "Blueprint Reality/MixCast");
                string filePath = Path.Combine(folderPath, file);

                if (!File.Exists(filePath))
                {
                    return null;
                }
                
                tex = new Texture2D(textureWidth, textureHeight, textureFormat, false, false);
                byte[] mapBytes = File.ReadAllBytes(filePath);
                tex.LoadRawTextureData(mapBytes);
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.filterMode = FilterMode.Point;
                tex.Apply();
            }
            return tex;
        }
    }
}
#endif
