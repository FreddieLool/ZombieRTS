using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(UnitController))]
public class Unit : MonoBehaviour
{
    public string unitName;
    public int health;
    public int maxHealth;
    public int attackDamage;
    public float movementSpeed;

    // Initialize the unit's stats
    public void Initialize(string name, int hp, int attack, float moveSpeed)
    {
        unitName = name;
        maxHealth = hp;
        health = hp;
        attackDamage = attack;
        movementSpeed = moveSpeed;

        // Configure the NavMeshAgent
        var agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = movementSpeed;
        }
    }
}
