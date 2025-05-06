using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShapeChanger : MonoBehaviour
{
    public GameObject cubeShape;
    public GameObject sphereShape;
    public GameObject triangleShape; // Cylinder burada temsil ediliyor
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

        // Baþlangýçta hepsi aktif
        cubeShape.SetActive(true);
        sphereShape.SetActive(true);
        triangleShape.SetActive(true);

        // 0.05 saniye sonra sadece Cube ve Triangle açýk, Sphere kapalý
        StartCoroutine(SetInitialStateAfterDelay());
    }

    IEnumerator SetInitialStateAfterDelay()
    {
        // Butonlar ve hareket kapansýn ama oyun donmasýn
        FindObjectOfType<CylinderMovement>().DisableButtons();
        FindObjectOfType<CubeMovement>().DisableButtons();

        // Tüm hareketleri manuel durdur (kendi CanMove'larýný kontrol et)
        var cubeMove = cubeShape.GetComponent<CubeMovement>();
        var cylMove = triangleShape.GetComponent<CylinderMovement>();
        var sphereMove = sphereShape.GetComponent<SphereMovement>();

        if (cubeMove != null) cubeMove.CanMove = false;
        if (cylMove != null) cylMove.CanMove = false;
        if (sphereMove != null) sphereMove.CanMove = false;

        yield return new WaitForSeconds(0.05f); // Bu sýrada butonlar týklanabilir

        // Ayarlarý yap
        cubeShape.SetActive(true);
        sphereShape.SetActive(false);
        triangleShape.SetActive(true);

        // Hareketleri tekrar aç
        if (cubeMove != null) cubeMove.CanMove = true;
        if (cylMove != null) cylMove.CanMove = true;
        if (sphereMove != null) sphereMove.CanMove = true;
        Time.timeScale = 1f;
    }


    void Update()
    {
        // Eðer hem Cube hem Cylinder hedefe ulaþtýysa sadece Sphere aktif olur
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
        yield return new WaitForSeconds(0.0001f); // 0.2 saniye bekle

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
        // Cube ve Cylinder (triangle) aktifse
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

        // Cube ve Sphere aktifse
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

        // Cylinder (triangle) ve Sphere aktifse
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