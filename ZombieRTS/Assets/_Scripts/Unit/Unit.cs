using UnityEngine;

[RequireComponent(typeof(UnitController))]
public class Unit : MonoBehaviour
{
    // Automatically registers the unit with the UnitSelectionManager upon instantiation
    void Start()
    {
        // Adds this unit to the managed list of all units in the UnitSelectionManager
        //UnitSelectionManager.Instance.AddUnit(this.gameObject);
    }

    // Automatically deregisters the unit from the UnitSelectionManager upon destruction
    void OnDestroy()
    {
        // Removes this unit from the managed list of all units in the UnitSelectionManager
        //UnitSelectionManager.Instance.RemoveUnit(this.gameObject);
    }
}
