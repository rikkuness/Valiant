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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlueprintReality.MixCast {
	public class MakeCopyOfInputProjection : CameraComponent {
        public Transform copyGroup;
        public GameObject projectionObjectOverride;

        public void Run()
        {
            if (context.Data == null)
                return;
            InputFeedProjection projection = InputFeedProjection.FindProjection(context);
            if (projection == null)
                return;

            GameObject prefab = projectionObjectOverride;
            if (prefab == null)
                prefab = projection.gameObject;

            GameObject copyObj = Instantiate(prefab);
            copyObj.transform.SetParent(copyGroup);
            copyObj.transform.position = projection.transform.position;
            copyObj.transform.rotation = projection.transform.rotation;

            if (projectionObjectOverride == null)
            {
                InputFeedProjection copyProjection = copyObj.GetComponent<InputFeedProjection>();
                SetTransformFromCameraSettings copyMovement = copyObj.GetComponent<SetTransformFromCameraSettings>();
                Destroy(copyProjection);
                Destroy(copyMovement);
            }

            MeshFilter origFilter = projection.GetComponent<MeshFilter>();
            MeshRenderer copyRenderer = copyObj.GetComponent<MeshRenderer>();
            MeshFilter copyFilter = copyObj.GetComponent<MeshFilter>();

            //Copy Texture
            RenderTexture originalTex = projection.FindFeed().ProcessedTexture;
            RenderTexture copyTex = new RenderTexture(originalTex.width, originalTex.height, 0, originalTex.format);
            Graphics.Blit(originalTex, copyTex);

            //Copy Material
            Material originalMat = copyRenderer.sharedMaterial; //allow override to replace material
            Material copyMat = new Material(originalMat);
            copyMat.name += " Copy";
            copyMat.mainTexture = copyTex;
            copyRenderer.material = copyMat;
            
            //Copy Mesh
            Mesh originalMesh = origFilter.sharedMesh;
            Mesh copyMesh = new Mesh();
            copyMesh.SetVertices(new List<Vector3>(originalMesh.vertices));
            copyMesh.SetTriangles(originalMesh.triangles, 0, true);
            copyMesh.SetUVs(0, new List<Vector2>(originalMesh.uv));
            copyMesh.RecalculateNormals();
            copyMesh.UploadMeshData(false);
            copyFilter.mesh = copyMesh;

            copyRenderer.enabled = true;
            copyObj.SetActive(true);
        }
    }
}
#endif
