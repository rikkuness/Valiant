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

using UnityEngine;
using System.Collections.Generic;

namespace BlueprintReality.MixCast {
public class SetActiveFromActive : MonoBehaviour {
    public GameObject[] targets = new GameObject[0];
    public bool all = false;

    public List<GameObject> active = new List<GameObject>();
    public List<GameObject> inactive = new List<GameObject>();


    private bool lastState;

    void OnEnable()
    {
        SetState(CalculateNewState());
    }

    void Update()
    {
        bool newState = CalculateNewState();
        if (lastState != newState)
            SetState(newState);
    }

    bool CalculateNewState()
    {
        if (all) {
            
            foreach (var target in targets)
                if (target == null || !target.activeInHierarchy)
                    return false;

            return true;
        }
        else
        {
            foreach (var target in targets)
                if (target != null && target.activeInHierarchy)
                    return true;
            
            return false;
        }
    }

    void SetState(bool newState)
    {
        lastState = newState;

        foreach (var g in active)
            g.SetActive(newState);

        foreach (var g in inactive)
            g.SetActive(!newState);
    }
}
}
