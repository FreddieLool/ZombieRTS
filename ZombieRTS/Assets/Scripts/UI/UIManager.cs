using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject buildingUI;


    private void Update()
    {
        if (buildingUI.activeSelf == false)
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                buildingUI.SetActive(true);
            }
        }
        else if (buildingUI.activeSelf == true)
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                buildingUI.SetActive(false);
            }
        }
    }
}
