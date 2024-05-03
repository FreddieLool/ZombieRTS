using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class MinimapController : MonoBehaviour
{
    public Material cameraBoxMaterial;

    public Camera minimap;

    public float lineWidth;

    public Collider mapCollider;

    public LineRenderer lineRenderer;




    private void Update()
    {
        DrawRect();
    }


    private void DrawRect()
    {
        Vector3 topLeft = GetCameraFrustumPoint(new Vector3(0, 0));
        Vector3 topRight = GetCameraFrustumPoint(new Vector3(Screen.width, 0));
        Vector3 bottomRight = GetCameraFrustumPoint(new Vector3(Screen.width, Screen.height));
        Vector3 bottomLeft = GetCameraFrustumPoint(new Vector3(0, Screen.height));

        topLeft.y += 150f;
        topRight.y += 150f;
        bottomRight.y += 150f;
        bottomLeft.y += 150f;


        lineRenderer.SetPosition(0, topLeft);
        lineRenderer.SetPosition(1, topRight);
        lineRenderer.SetPosition(2, bottomRight);
        lineRenderer.SetPosition(3, bottomLeft);



    }

    private Vector3 GetCameraFrustumPoint(Vector3 position)
    {
        var positionRay = Camera.main.ScreenPointToRay(position);
        RaycastHit hit;
        Vector3 result = mapCollider.Raycast(positionRay, out hit, Camera.main.transform.position.y * 2) ? hit.point : new Vector3();

        return result;

    }

    private void OnValidate()
    {
        DrawRect();
    }

  
}
