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
using UnityEngine.UI;

namespace BlueprintReality.MixCast {
    public class SetFPSFromCameraContext: CameraComponent {
        [SerializeField] Dropdown fpsDropdown;

        string lastId = null;

        protected override void OnEnable() {
            base.OnEnable();

            fpsDropdown = GetComponent<Dropdown>();
            fpsDropdown.ClearOptions();
            fpsDropdown.AddOptions( new List<Dropdown.OptionData>()
            {
                new Dropdown.OptionData("30"),
                new Dropdown.OptionData("60"),
                new Dropdown.OptionData("90"),
            } );

            UpdateFramerateFromContext();

            fpsDropdown.onValueChanged.AddListener( HandleDropdownChanged );
            lastId = context.Data.id;
        }

        void UpdateFramerateFromContext() {
            string fps = GetValue().ToString();

            int index = Utility.Find.Index<Dropdown.OptionData>(fpsDropdown.options, op => op.text == fps);
            if( index >= 0 ) {
                fpsDropdown.value = index;
            }
        }

        protected override void OnDisable() {
            fpsDropdown.onValueChanged.RemoveListener( HandleDropdownChanged );
            base.OnDisable();
        }

        void HandleDropdownChanged( int index ) {
            SetValue( System.Convert.ToInt32( fpsDropdown.options[index].text ) );
        }

        protected virtual int GetValue() {
            if( context == null || context.Data == null ) {
                return 0;
            }
            return context.Data.outputFramerate;
        }

        protected virtual void SetValue( int newValue ) {
            if( context == null || context.Data == null ) {
                return;
            }
            context.Data.outputFramerate = newValue;
        }

        private void Update() {
            if( lastId != context.Data.id ) {
                UpdateFramerateFromContext();
                lastId = context.Data.id;
            }
        }
    }
}
#endif
