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
using System.Collections.Generic;
using UnityEngine;

namespace BlueprintReality.MixCast
{ 
    public class EventCenter : MonoBehaviour
    {
        public enum Result {
            Started,
            Stopped,
            Success,
            Error
        }

        // holds some default string categories.  Using string instead of enum so the system is easier to extend
        // across projects.
        public class Category {
            public static readonly string General   = "General";
            public static readonly string Streaming = "Streaming";
            public static readonly string Network   = "Network";
            public static readonly string Input     = "Input";
            public static readonly string Recording = "Recording";
            public static readonly string Saving    = "Saving";
            public static readonly string Loading   = "Loading";
            public static readonly string Sensor = "Sensor";
            public static readonly string WildKey = "WildKey";
        }

        public class EventData {
            public string category;
            public Result result;
            public string locMsg;

            public EventData(string c, Result r, string msg) {
                category = c;
                result = r;
                locMsg = msg;
            }
        }

        static EventCenter _instance = null;

        Dictionary<string, System.Action<Result, string>> eventCallbacks;
        List<EventData>                                   events;

        static bool shuttingDown = false;

        public static void HandleEvent( string category, Result result, string msg = "", bool localize = true) {
            if( Instance != null && shuttingDown == false ) {
                string loc = (localize ? Text.Localization.Get( msg ) : msg);
                Instance.events.Add( new EventData( category, result, loc ) );
            }
        }

        public static void AddListener( string category, System.Action<Result, string> listener) {
            if( Instance != null ) {
                System.Action<Result, string> callbackList;
                Instance.eventCallbacks.TryGetValue( category, out callbackList );
                if( callbackList == null ) {
                    callbackList = Instance.BaseCallback;
                }
                callbackList += listener;
                Instance.eventCallbacks.Remove( category );
                Instance.eventCallbacks.Add( category, callbackList );
            }
        }

        public static void RemoveListener( string category, System.Action<Result, string> listener ) {
            if( Instance != null ) {
                System.Action<Result, string> callbackList;
                Instance.eventCallbacks.TryGetValue( category, out callbackList );
                if( callbackList != null ) {
                    callbackList -= listener;
                    Instance.eventCallbacks.Remove( category );
                    Instance.eventCallbacks.Add( category, callbackList );
                }
            }
        }

        // call Initialize() when the app loads.  This will create the singleton game object.
        public static void Initialize() {
            if(_instance == null) {
                GameObject go = new GameObject( "EventCenter" );
                go.AddComponent<EventCenter>(); // will finish initialization when Start() is called.
                shuttingDown = false;
                UnityEngine.SceneManagement.SceneManager.sceneLoaded += _instance.OnSceneLoaded;
            }
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode) {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= _instance.OnSceneLoaded;
            MixCastRegistry.ShowRegistryMismatchWarningIfNeeded();
        }

        public static void Disable() {
            if(_instance != null) {
                Destroy( _instance.gameObject );
                shuttingDown = true;
                _instance = null;
            }
        }

        /// <summary>
        /// PRIVATE FUNCTIONS
        /// </summary>

        private static EventCenter Instance { // use this in Public functions
            get {
                if(_instance == null && shuttingDown == false) {
                    Debug.LogError( "EventCenter::Instance -- This script must be attached to a global game object" );
                }
                return _instance;
            }
        }

        void InitInternal() {
            events = new List<EventData>();
            eventCallbacks = new Dictionary<string, System.Action<Result, string>>();
            shuttingDown = false;
        }

        void LateUpdate() {
            foreach(var e in Instance.events) {
                System.Action<Result, string> callbackList;
                Instance.eventCallbacks.TryGetValue( e.category, out callbackList );
                if(callbackList != null) {
                    callbackList( e.result, e.locMsg );
                }
            }
            Instance.events.Clear();
        }

        void OnEnable() { // Update() can get called before Start()
            if(_instance != null && _instance != this) {
                GameObject.Destroy( gameObject );
                return;
            }
            if(_instance == null) {
                _instance = this;
                _instance.InitInternal();
                DontDestroyOnLoad( _instance.gameObject );
            }
        }

        void OnDestroy() {
            if(this == _instance) {
                _instance = null;
                events.Clear();
                eventCallbacks.Clear();
            }
        }

        void BaseCallback(Result type, string locMsg) { }

        private void OnApplicationQuit() {
            shuttingDown = true;
        }
    }
}
#endif
