using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossMissile : Bullet
{
    public Transform Target;
    public int missileHealth;
    NavMeshAgent nav;


    void Awake()
    {
        nav = GetComponent<NavMeshAgent>();
    }
    void OnTriggerEnter(Collider other) {
        if (!isRock && !isMelee && other.gameObject.tag == "Wall")
        {
            Destroy(gameObject);
        }

        if (other.tag == "Bullet")
        {
            if (missileHealth <= 0)
                Destroy(gameObject);
            Bullet bullet = other.GetComponent<Bullet>();
            missileHealth -= bullet.damage;
            Debug.Log("missileHealth : " + missileHealth);
        }
    }
    void Update()
    {
        nav.SetDestination(Target.position);
    }

   
}
