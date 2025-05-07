using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubePositionController : MonoBehaviour
{
    public List<GameObject> objectsToCheck = new List<GameObject>();
    private void Update()
    {
        CheckCubePosition();
    }
    private void CheckCubePosition()
    {
        Vector3 cubePosition = transform.position;
        foreach (var obj in objectsToCheck)
        {
            if (obj != null && IsObjectUnderCube(obj))
            {
                obj.SetActive(false);
            }
        }
    }
    private bool IsObjectUnderCube(GameObject obj)
    {
        Vector3 cubePosition = transform.position;
        Vector3 objPosition = obj.transform.position;
        return Mathf.Approximately(cubePosition.x, objPosition.x) &&
               Mathf.Approximately(cubePosition.z, objPosition.z);
    }
}
