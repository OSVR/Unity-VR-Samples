# Unity-VR-Samples

This project demonstrates OSVR integration with the [Unity VR Samples project](https://www.assetstore.unity3d.com/#!/content/51519) available on the Unity Asset Store as of the Unity 5.3 release.

OSVR-Unity-v0.6.4-23-ge0de7c4 (build number 302) with OSVR-Core v0.6-807-gc9403f7 and RenderManager v0.6.38.

To convert the scenes to be OSVR compatible, the following steps were taken:

1) Uncheck "Virtual Reality Supported" in Player Settings -- Unity only supports Oculus with this feature at this time.

2) Add a ClientKit and DisplayController to the scene.

3) Add a VRViewer component to MainCamera. 

4) Replace instances of InputTracking.GetLocalRotation with the rotation of the MainCamera.

