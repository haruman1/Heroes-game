using UnityEngine;
using System.Collections.Generic;
using Unity.Cinemachine;
public class CameraManager : MonoBehaviour
{
  // Start is called once before the first execution of Update after the MonoBehaviour is created

  static List<CinemachineCamera> cameras = new List<CinemachineCamera>();
  public static CinemachineCamera ActiveCamera = null;

  public static bool IsActiveCamera(CinemachineCamera camera){
    return camera == ActiveCamera;
  }
  public static void RegisterCamera(CinemachineCamera camera){
    cameras.Add(camera);
    if(ActiveCamera == null)
    {   
        ActiveCamera = camera;
    }
  }
  public static void SwitchCamera(CinemachineCamera newCamera){
    //penting untuk kamera menjadi prioritas utama
    newCamera.Priority = 10; 
    ActiveCamera = newCamera;
    foreach (CinemachineCamera cam in cameras)
    {
        if(cam != newCamera)
        cam.Priority = 0;
    }
  }
  public static void UnRegisterCamera(CinemachineCamera camera){
    cameras.Remove(camera);
  }
  
 
    
  
}
