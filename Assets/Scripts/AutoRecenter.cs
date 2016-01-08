/// OSVR-Unity
///
/// http://sensics.com/osvr
///
/// <copyright>
/// Copyright 2016 Sensics, Inc.
///
/// Licensed under the Apache License, Version 2.0 (the "License");
/// you may not use this file except in compliance with the License.
/// You may obtain a copy of the License at
///
///     http://www.apache.org/licenses/LICENSE-2.0
///
/// Unless required by applicable law or agreed to in writing, software
/// distributed under the License is distributed on an "AS IS" BASIS,
/// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
/// See the License for the specific language governing permissions and
/// limitations under the License.
/// </copyright>

using UnityEngine;
using System.Collections;

namespace OSVR
{
    namespace Unity
    {
        //This class recenters the room based on head position at Start() time
        //@todo replace this file by merging this functionality into SetRoomRotationUsingHead script
        public class AutoRecenter : MonoBehaviour
        {
            private ClientKit _clientKit;
            private DisplayController _displayController;
            private bool recentered = false;

            void Awake()
            {
                recentered = false;
                _clientKit = FindObjectOfType<ClientKit>();
                _displayController = FindObjectOfType<DisplayController>();
            }

            void Update()
            {
                if(!recentered)
                {
                    if (_displayController != null && _displayController.CheckDisplayStartup() && _displayController.UseRenderManager)
                    {
                        _displayController.RenderManager.SetRoomRotationUsingHead();
                        recentered = true;
                    }
                    else if (_displayController != null && _displayController.CheckDisplayStartup() && !_displayController.UseRenderManager)
                    {
                        _clientKit.context.SetRoomRotationUsingHead();
                        recentered = true;
                    }
                }
                
            }
        }
    }
}