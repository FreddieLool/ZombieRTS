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
    public float attackRange = 2.0f;
    public float attackRate = 1.0f;
    private float lastAttackTime;


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

    public void ReceiveDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        //  disable the unit, play animation..
        gameObject.SetActive(false);
    }

    public bool CanAttack()
    {
        return Time.time - lastAttackTime > (1 / attackRate);
    }

    public void Attack(Unit target)
    {
        if (target && CanAttack())
        {
            target.ReceiveDamage(attackDamage);
            lastAttackTime = Time.time;
        }
    }

}
