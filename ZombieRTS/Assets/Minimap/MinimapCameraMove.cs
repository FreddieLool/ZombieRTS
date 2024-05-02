using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MinimapCameraMove : MonoBehaviour, IPointerClickHandler
{
    public RectTransform minimapRect;
    public Camera minimapCamera;
    public CameraController CameraController;

    public Vector2 terrainMin;
    public Vector2 terrainMax;


    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 localClick;

        localClick.x = eventData.position.x;
        localClick.y = eventData.position.y;


        Vector3 worldPosition;
        worldPosition.x = Mathf.Lerp(terrainMin.x, terrainMax.x, localClick.x / (minimapRect.rect.width));
        worldPosition.z = Mathf.Lerp(terrainMin.y, terrainMax.y, localClick.y / (minimapRect.rect.height));


        CameraController.SetCameraXZPosition(worldPosition.x,worldPosition.z);

    }
}
