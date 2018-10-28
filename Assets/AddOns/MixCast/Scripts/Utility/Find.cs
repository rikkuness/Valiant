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

namespace BlueprintReality.MixCast.Utility
{
    public class Find
    {
        public delegate bool findFunc<T>( T a );

        public static int Index<T>( List<T> objList, findFunc<T> compareFunc ) {
            if( objList == null ) {
                return -1;
            }
            for( int i = 0; i < objList.Count; i++ ) {
                if( compareFunc( objList[ i ] ) ) {
                    return i;
                }
            }
            return -1;
        }

        public static T Object<T>( List<T> objList, findFunc<T> compareFunc ) {
            if( objList == null ) {
                return default( T );
            }
            for( int i = 0; i < objList.Count; i++ ) {
                if( compareFunc( objList[ i ] ) ) {
                    return objList[i];
                }
            }
            return default(T);
        }
    }
}
#endif
