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
using System;

namespace BlueprintReality.MixCast
{
    public class FileTextureLoader : MonoBehaviour
    {
        public class Job
        {
            public string filepath;
            public Action<Texture2D> OnJobDone;
            public Texture2D texture;

            public void InvokeCallback()
            {
                if (OnJobDone != null)
                {
                    OnJobDone(texture);
                }
            }
        }
        
        public bool IsJobInProgress { get; private set; }

        private Queue<Job> jobs = new Queue<Job>();

        void Update()
        {
            if (!IsJobInProgress
                && jobs.Count > 0)
            {
                StartJob(jobs.Dequeue());
            }    
        }

        public void AddJob(string filepath, Action<Texture2D> callback)
        {
            jobs.Enqueue(new Job()
            {
                filepath = filepath,
                OnJobDone = callback
            });
        }

        private void StartJob(Job job)
        {
            StartCoroutine(LoadImageAsync(job));
        }

        private IEnumerator LoadImageAsync(Job job)
        {
            if (string.IsNullOrEmpty(job.filepath))
            {
                Debug.LogError("No filepath set for watermark texture!");

                job.InvokeCallback();
                
                yield break;
            }

            IsJobInProgress = true;

            // a third / character is now required to access root /Users
            WWW www = new WWW("file:///" + job.filepath);

            yield return www;

            //Debug.Log(www.error);

            IsJobInProgress = false;

            if (string.IsNullOrEmpty(www.error)
                && www.texture != null)
            {
                job.texture = www.texture;
            }
            
            job.InvokeCallback();
        }
    }
}
#endif
