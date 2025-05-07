using UnityEngine;
using System.Collections;
public class CubePositionChecker : MonoBehaviour
{
    public GameObject targetObject;
    private Vector3 initialPosition;
    public Vector3 targetDifference;
    public float tolerance = 0.1f;

    private bool isInTargetPosition;
    private Coroutine disableCoroutine;

    void Start()
    {
        initialPosition = transform.position;
    }

    void Update()
    {
        Vector3 positionDifference = transform.position - initialPosition;

        if (Mathf.Abs(positionDifference.x - targetDifference.x) < tolerance &&
            Mathf.Abs(positionDifference.y - targetDifference.y) < tolerance &&
            Mathf.Abs(positionDifference.z - targetDifference.z) < tolerance)
        {
            if (disableCoroutine != null)
            {
                StopCoroutine(disableCoroutine);
                disableCoroutine = null;
            }

            targetObject.SetActive(true);
            isInTargetPosition = true;
        }
        else
        {
            if (disableCoroutine == null)
            {
                disableCoroutine = StartCoroutine(DisableAfterDelay());
            }

            isInTargetPosition = false;
        }
    }

    private IEnumerator DisableAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        targetObject.SetActive(false);
    }

    public bool IsInTargetPosition()
    {
        return isInTargetPosition;
    }
}
