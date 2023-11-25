using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossMissile : Bullets
{
    public Transform target;
    NavMeshAgent nav;
    void Awake()
    {
        nav = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        nav.SetDestination(target.position);
    }
}
