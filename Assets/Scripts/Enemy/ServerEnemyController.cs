using System;
using System.Collections;
using System.Collections.Generic;
using DarkRift.Server;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ServerEnemyController : MonoBehaviour {
    public uint Id { get; private set; }
    public Vector3 TargetPosition { get; private set; }

    public DarkRiftServer Server { get; set; }
    public ServerEnemyManager Manager { get; set; }

    private NavMeshAgent navAgent;

    private void Awake() {
        this.navAgent = this.GetComponent<NavMeshAgent>();
    }

    public void Setup(uint id, Vector3 target) {
        this.Id = id;
        this.TargetPosition = target;
        this.navAgent.SetDestination(this.TargetPosition);
    }

    private void Update() {
        if (Vector3.Distance(this.TargetPosition, this.transform.position) < 0.5f) {
            this.Manager.DespawnEnemy(this);
            this.Manager.InvokeEnemyReachedTarget();
        }
    }

    public void Damage() {
        this.Manager.DespawnEnemy(this);
        this.Manager.InvokeEnemyDestroyed();
    }
}
