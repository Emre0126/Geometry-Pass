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

        // Listedeki her bir nesnenin x ve z koordinatlarýný kontrol et
        foreach (var obj in objectsToCheck)
        {
            if (obj != null && IsObjectUnderCube(obj))
            {
                obj.SetActive(false);
            }
        }
    }

    // Nesnenin küp ile ayný x ve z deðerlerine sahip olup olmadýðýný kontrol eder
    private bool IsObjectUnderCube(GameObject obj)
    {
        Vector3 cubePosition = transform.position;
        Vector3 objPosition = obj.transform.position;

        // float karþýlaþtýrmalarýnda Mathf.Approximately kullanmak hassasiyet saðlar
        return Mathf.Approximately(cubePosition.x, objPosition.x) &&
               Mathf.Approximately(cubePosition.z, objPosition.z);
    }
}
