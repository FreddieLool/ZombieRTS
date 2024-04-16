using UnityEngine;
using UnityEngine.EventSystems;


// To manage interactions and differentiate between mouse on UI or game.
public class MouseOverUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private bool isMouseOver = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        isMouseOver = true;

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isMouseOver = false;

    }

    public bool IsMouseOver()
    {
        return isMouseOver;
    }

}

