using UnityEngine;
using System.Collections;

namespace OSVR
{
    namespace Unity
    {

        public class ResetYaw : MonoBehaviour
        {
            public KeyCode resetYawKey = KeyCode.R;
            public KeyCode undoKey = KeyCode.U;
            private ClientKit _clientKit;

            void Awake()
            {
                _clientKit = FindObjectOfType<ClientKit>();
            }
            // Update is called once per frame
            void FixedUpdate()
            {
                if(Input.GetKeyDown(resetYawKey))
                {
                    _clientKit.context.SetRoomRotationUsingHead();
                }
                if (Input.GetKeyDown(undoKey))
                {
                    _clientKit.context.ClearRoomToWorldTransform();
                }
            }
        }
    }
}

