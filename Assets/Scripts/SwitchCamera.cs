using UnityEngine;
using Unity.Cinemachine;
public class SwitchCamera : MonoBehaviour
{
  public CinemachineCamera cam1;
  public CinemachineCamera cam2;    

  void Update()
  {
    if (Input.GetKeyDown(KeyCode.C))
    {
      if (cam1.Priority > cam2.Priority)
      {
        cam1.Priority = 0;
        cam2.Priority = 10;
      }
      else
      {
        cam1.Priority = 10;
        cam2.Priority = 0;
      }
    }
  }
}
