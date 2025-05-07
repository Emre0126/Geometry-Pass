using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShapeChanger : MonoBehaviour
{
    public GameObject cubeShape;
    public GameObject sphereShape;
    public GameObject triangleShape;
    public GameObject justsphereShape;
    public Button cubeButton;
    public Button sphereButton;
    public Button triangleButton;
    public Button justsphereButton;
    public CylinderPositionChecker cylinderChecker;
    public CubePositionChecker cubeChecker;
    private Rigidbody cubeRb;
    private Rigidbody sphereRb;
    private Rigidbody triangleRb;
    private bool isMoving = true;
    void Start()
    {
        cubeRb = cubeShape.GetComponent<Rigidbody>();
        sphereRb = sphereShape.GetComponent<Rigidbody>();
        triangleRb = triangleShape.GetComponent<Rigidbody>();
        cubeShape.SetActive(true);
        sphereShape.SetActive(true);
        triangleShape.SetActive(true);
        StartCoroutine(SetInitialStateAfterDelay());
    }

    IEnumerator SetInitialStateAfterDelay()
    {
        FindObjectOfType<CylinderMovement>().DisableButtons();
        FindObjectOfType<CubeMovement>().DisableButtons();
        var cubeMove = cubeShape.GetComponent<CubeMovement>();
        var cylMove = triangleShape.GetComponent<CylinderMovement>();
        var sphereMove = sphereShape.GetComponent<SphereMovement>();
        if (cubeMove != null) cubeMove.CanMove = false;
        if (cylMove != null) cylMove.CanMove = false;
        if (sphereMove != null) sphereMove.CanMove = false;
        yield return new WaitForSeconds(0.05f);
        cubeShape.SetActive(true);
        sphereShape.SetActive(false);
        triangleShape.SetActive(true);
        if (cubeMove != null) cubeMove.CanMove = true;
        if (cylMove != null) cylMove.CanMove = true;
        if (sphereMove != null) sphereMove.CanMove = true;
        Time.timeScale = 1f;
    }
    void Update()
    {
        if (cylinderChecker.IsInTargetPosition() && cubeChecker.IsInTargetPosition())
        {
            var cubeMove = cubeShape.GetComponent<CubeMovement>();
            var cylMove = triangleShape.GetComponent<CylinderMovement>();
            
            if (cubeMove != null) cubeMove.CanMove = false;
            if (cylMove != null) cylMove.CanMove = false;
            StartCoroutine(DisableCubeAndCylinderAfterDelay());
        }
        HandleFallAndDisableMovement();
    }
    IEnumerator DisableCubeAndCylinderAfterDelay()
    {
        yield return new WaitForSeconds(0.0001f);
        cubeShape.SetActive(false);
        sphereShape.SetActive(true);
        triangleShape.SetActive(false);
        cubeButton.gameObject.SetActive(false);
        sphereButton.gameObject.SetActive(false);
        triangleButton.gameObject.SetActive(false);
        justsphereButton.gameObject.SetActive(true);
    }
    void HandleFallAndDisableMovement()
    {
        if (cubeShape.activeSelf && triangleShape.activeSelf)
        {
            if (!triangleRb.isKinematic || !cubeRb.isKinematic)
            {
                var cubeMove = cubeShape.GetComponent<CubeMovement>();
                var cylMove = triangleShape.GetComponent<CylinderMovement>();
                var sphereMove = sphereShape.GetComponent<SphereMovement>();
                if (cubeMove != null) cubeMove.CanMove = false;
                if (cylMove != null) cylMove.CanMove = false;
                if (sphereMove != null) sphereMove.CanMove = false;
                FindObjectOfType<CylinderMovement>().DisableButtons();
                FindObjectOfType<CubeMovement>().DisableButtons();
            }
        }
        else if (cubeShape.activeSelf && sphereShape.activeSelf)
        {
            if (!sphereRb.isKinematic || !cubeRb.isKinematic)
            {
                var cubeMove = cubeShape.GetComponent<CubeMovement>();
                var cylMove = triangleShape.GetComponent<CylinderMovement>();
                var sphereMove = sphereShape.GetComponent<SphereMovement>();

                if (cubeMove != null) cubeMove.CanMove = false;
                if (sphereMove != null) sphereMove.CanMove = false;
                FindObjectOfType<CylinderMovement>().DisableButtons();
                FindObjectOfType<CubeMovement>().DisableButtons();
                if (cylMove != null) cylMove.CanMove = false;
            }
        }
        else if (triangleShape.activeSelf && sphereShape.activeSelf)
        {
            if (!sphereRb.isKinematic || !triangleRb.isKinematic)
            {
                var cubeMove = cubeShape.GetComponent<CubeMovement>();
                var cylMove = triangleShape.GetComponent<CylinderMovement>();
                var sphereMove = sphereShape.GetComponent<SphereMovement>();
                if (cubeMove != null) cubeMove.CanMove = false;
                if (sphereMove != null) sphereMove.CanMove = false;
                FindObjectOfType<CylinderMovement>().DisableButtons();
                FindObjectOfType<CubeMovement>().DisableButtons();
                if (cylMove != null) cylMove.CanMove = false;
            }
        }
    }
    public void OnCubeButtonPressed()
    {
        cubeShape.SetActive(true);
        sphereShape.SetActive(false);
        triangleShape.SetActive(true);
    }
    public void OnSphereButtonPressed()
    {
        cubeShape.SetActive(true);
        sphereShape.SetActive(true);
        triangleShape.SetActive(false);
    }
    public void OnTriangleButtonPressed()
    {
        cubeShape.SetActive(false);
        sphereShape.SetActive(true);
        triangleShape.SetActive(true);
    }
    public void OnJustSphereButtonPressed()
    {
        cubeShape.SetActive(false);
        sphereShape.SetActive(true);
        triangleShape.SetActive(false);
    }
}
