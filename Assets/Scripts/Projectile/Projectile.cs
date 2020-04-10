using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour {
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float lifetime = 2f;

    private new Rigidbody rigidbody;

    public delegate void HitEvent(Projectile proj, GameObject other);
    public event HitEvent EventCollision;

    private void Awake() {
        this.rigidbody = this.GetComponent<Rigidbody>();
        //this.rigidbody.isKinematic = true;

        Destroy(this.gameObject, this.lifetime);
    }

    private void Update() {
        this.rigidbody.velocity = this.transform.forward * moveSpeed;
    }

    private void OnTriggerEnter(Collider other) {
        Debug.Log($"Hit {other.name}");
        this.EventCollision?.Invoke(this, other.gameObject);
        
        var serverEnemy = other.GetComponent<ServerEnemyController>();
        if (serverEnemy != null) {
            serverEnemy.Damage();
        }

        Destroy(this.gameObject);
    }
}
