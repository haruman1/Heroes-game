using UnityEngine;
using Unity.Cinemachine;

public class CameraRegister : MonoBehaviour
{
    private void OnEnable(){
        CameraManager.RegisterCamera(GetComponent<CinemachineCamera>());
    }
    private void OnDisable(){
        CameraManager.UnRegisterCamera(GetComponent<CinemachineCamera>());
    }
    private void OnDestroy(){
        CameraManager.UnRegisterCamera(GetComponent<CinemachineCamera>());
    }
}