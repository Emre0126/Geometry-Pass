using UnityEngine;

public class SpherePositionChecker : MonoBehaviour
{
    public GameObject targetObject;
    private Vector3 initialPosition;
    public Vector3 targetDifference;
    public float tolerance = 0.1f;

    private bool isInTargetPosition;

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
            targetObject.SetActive(true);
            isInTargetPosition = true;
        }
        else
        {
            targetObject.SetActive(false);
            isInTargetPosition = false;
        }
    }

    public bool IsInTargetPosition()
    {
        return isInTargetPosition;
    }
}
