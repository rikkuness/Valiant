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
using BlueprintReality.MixCast.Profiling;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BlueprintReality.MixCast
{
    public class MixCastCameras : MonoBehaviour
    {
        public static MixCastCameras Instance { get; protected set; }

        public CameraConfigContext cameraPrefab;

        [Tooltip("Please keep this enable to avoid losing your camera feed")]
        public bool keepCameraWhenChangingScene = true;

        public Transform RoomTransform { get; protected set; }
        public List<CameraConfigContext> CameraInstances { get; protected set; }
        public PerformanceTracker Performance { get; protected set; }

        public event System.Action OnBeforeRender;

        private float nextRenderTime;
        private static BlitTexture brandingBlit;
        private static BlitTexture logoBlit;
        //Caching objects for memory management
        List<MixCastData.CameraCalibrationData> createCams = new List<MixCastData.CameraCalibrationData>();
        List<CameraConfigContext> destroyCams = new List<CameraConfigContext>();
        WaitForEndOfFrame waitForEndOfFrame;

        void Awake()
        {

            Performance = GetComponent<PerformanceTracker>();
            if (Performance == null)
            {
                Performance = gameObject.AddComponent<PerformanceTracker>();
            }
            waitForEndOfFrame = new WaitForEndOfFrame();
        }

        void Start()
        {
            InitWatermarks();
        }

        private void OnEnable()
        {

            // Handle this error to give users a more relevant message than a null ref if room setup is incorrect
            try
            {
                RoomTransform = (transform.parent != null) ? (transform.parent) : (Instance.RoomTransform);
            }
            catch
            {
                Debug.LogWarning("MixCast caught a null reference looking for the room transform, are your cameras set up correctly? Please see our documentation for setup details - https://mixcast.me/docs/develop/unity/?section=installing-the-sdk");
            }

            if (RoomTransform == null)
            {
                RoomTransform = transform;
            }

            if (Instance != null)
            {
                Instance.RoomTransform = RoomTransform;  //Update Cameras parent
                DestroyImmediate(gameObject);
                return;
            }

            Instance = this;
            //Move Cameras to the root so DontDestroyOnLoad works
            if (keepCameraWhenChangingScene)
            {
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }

            MixCast.MixCastEnabled += HandleMixCastEnabled;
            MixCast.MixCastDisabled += HandleMixCastDisabled;

            GenerateCameras();

            StartCoroutine(RenderUsedCameras());
            StartCoroutine(RenderSpareCameras());

            nextRenderTime = Time.unscaledTime + 1f / MixCast.Settings.global.targetFramerate;
        }

        private void OnDisable()
        {
            if (Instance != this) { return; }

            StopCoroutine("RenderUsedCameras");
            StopCoroutine("RenderSpareCameras");

            DestroyCameras();

            MixCast.MixCastEnabled -= HandleMixCastEnabled;
            MixCast.MixCastDisabled -= HandleMixCastDisabled;

            Instance = null;
        }

        private void Update()
        {

            createCams.AddRange(MixCast.Settings.cameras);
            destroyCams.AddRange(CameraInstances);
            for (int i = 0; i < CameraInstances.Count; i++)
            {
                MixCastData.CameraCalibrationData camData = CameraInstances[i].Data;
                for (int j = createCams.Count - 1; j >= 0; j--)
                    if (createCams[j] == camData)
                        createCams.RemoveAt(j);
            }
            for (int i = 0; i < MixCast.Settings.cameras.Count; i++)
            {
                for (int j = destroyCams.Count - 1; j >= 0; j--)
                    if (destroyCams[j].Data == MixCast.Settings.cameras[i])
                        destroyCams.RemoveAt(j);
            }

            for (int i = 0; i < destroyCams.Count; i++)
            {
                CameraInstances.Remove(destroyCams[i]);
                Destroy(destroyCams[i].gameObject);
            }

            for (int i = 0; i < createCams.Count; i++)
            {
                bool wasPrefabActive = cameraPrefab.gameObject.activeSelf;
                cameraPrefab.gameObject.SetActive(false);

                CameraConfigContext instance = Instantiate(cameraPrefab, transform, false);

                instance.Data = createCams[i];

                CameraInstances.Add(instance);

                cameraPrefab.gameObject.SetActive(wasPrefabActive);

                instance.gameObject.SetActive(MixCast.Active);
            }

            destroyCams.Clear();
            createCams.Clear();

            // Process LibAVStuff logs
            MixCastAV.LogOutput();
        }


        IEnumerator RenderUsedCameras()
        {
            while (isActiveAndEnabled)
            {
                UpdateTransform();

                if (Time.unscaledTime >= nextRenderTime)
                {
                    if (OnBeforeRender != null)
                        OnBeforeRender();

                    for (int i = 0; i < MixCastCamera.ActiveCameras.Count; i++)
                    {
                        MixCastCamera cam = MixCastCamera.ActiveCameras[i];
                        if (cam.IsInUse)
                            cam.RenderScene();
                    }

                    nextRenderTime += 1f / MixCast.Settings.global.targetFramerate;
                }

                yield return waitForEndOfFrame;
            }
        }

        IEnumerator RenderSpareCameras()
        {
            int lastSpareRenderedIndex = 0;
            while (isActiveAndEnabled)
            {
                UpdateTransform();

                if (MixCastCamera.ActiveCameras.Count > 0)
                {
                    int startIndex = lastSpareRenderedIndex;
                    lastSpareRenderedIndex++;
                    while (MixCastCamera.ActiveCameras[lastSpareRenderedIndex % MixCastCamera.ActiveCameras.Count].IsInUse && (lastSpareRenderedIndex - startIndex) <= MixCastCamera.ActiveCameras.Count)
                        lastSpareRenderedIndex++;

                    if (lastSpareRenderedIndex - startIndex <= MixCastCamera.ActiveCameras.Count)
                        MixCastCamera.ActiveCameras[lastSpareRenderedIndex % MixCastCamera.ActiveCameras.Count].RenderScene();
                }

                for (int i = 0; i < MixCast.Settings.global.framesPerSpareRender; i++)
                {
                    yield return waitForEndOfFrame;
                }
            }
        }

        void GenerateCameras()
        {
            CameraInstances = new List<CameraConfigContext>();

            bool wasPrefabActive = cameraPrefab.gameObject.activeSelf;
            cameraPrefab.gameObject.SetActive(false);
            for (int i = 0; i < MixCast.Settings.cameras.Count; i++)
            {
                CameraConfigContext instance = Instantiate(cameraPrefab, transform, false);

                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;

                instance.Data = MixCast.Settings.cameras[i];

                CameraInstances.Add(instance);
            }
            cameraPrefab.gameObject.SetActive(wasPrefabActive);

            SetCamerasActive(MixCast.Active);
        }
        void DestroyCameras()
        {
            for (int i = 0; i < CameraInstances.Count; i++)
            {
                Destroy(CameraInstances[i].gameObject);
            }

            CameraInstances.Clear();
            CameraInstances = null;
        }
        private void HandleMixCastEnabled()
        {
            SetCamerasActive(true);
        }
        private void HandleMixCastDisabled()
        {
            SetCamerasActive(false);
        }

        void SetCamerasActive(bool active)
        {
            if (CameraInstances == null) { return; }


            for (int i = 0; i < CameraInstances.Count; i++)
            {
                CameraInstances[i].gameObject.SetActive(active);
            }
        }

        void UpdateTransform()
        {
            if (RoomTransform != null)
            {
                transform.position = RoomTransform.position;
                transform.rotation = RoomTransform.rotation;
                transform.localScale = Vector3.one * RoomTransform.TransformVector(Vector3.forward).magnitude;  //Apply rig scaling
            }
        }

        private void InitWatermarks()
        {
            // catch the first time Studio is loaded, when MixCastManageApp has not yet assigned the path causing the watermark to fail to show
            if (string.IsNullOrEmpty(MixCast.Settings.persistentDataPath))
            {
                MixCast.Settings.persistentDataPath = Application.persistentDataPath;
            }

            var textureLoader = gameObject.AddComponent<FileTextureLoader>();

            // only show arcade branding if not free license type
            if (!MixCast.SecureSettings.IsFreeLicense)
            {
                brandingBlit = new BlitTexture();
                brandingBlit.SetTexturePosition(BlitTexture.Position.BottomRight);
                brandingBlit.Material = new Material(Shader.Find("Hidden/MixCast/Watermark"));

                // arcade branding logo
                string brandingFilepath = Path.Combine(MixCast.Settings.persistentDataPath, "branding.png");
                textureLoader.AddJob(brandingFilepath, (texture) =>
                {
                    brandingBlit.Texture = texture;
                });
            }

            // show our MixCast watermark
            logoBlit = new BlitTexture();
            logoBlit.SetTexturePosition(BlitTexture.Position.BottomLeft);
            logoBlit.Material = new Material(Shader.Find("Hidden/MixCast/Watermark"));
            logoBlit.Texture = Resources.Load<Texture2D>("MixCast_Logo");

            MixCastCamera.FrameEnded += ApplyWatermarks;
        }
        private void ApplyWatermarks(MixCastCamera cam)
        {
            if (brandingBlit != null)
            {
                brandingBlit.ApplyToFrame(cam);
            }

            if (logoBlit != null && (MixCast.SecureSettings.IsFreeLicense || MixCast.Desktop.ShowingUI))
            {
                logoBlit.ApplyToFrame(cam);
            }
        }
    }
}
#endif
