using UnityEngine;
using Unity.Mathematics;
using System;

public class Turret : MonoBehaviour {
    [SerializeField] float minRotation = -45f;
    [SerializeField] float maxRotation = 45f;
    [SerializeField] float rotationSpeed = 90f;

    [Header("Projectile")]
    [SerializeField] Projectile projPrefab;
    [SerializeField] float fireRate = 0.5f;
    [SerializeField] Transform[] projSpawnpoints;

    private float rotation = 0f;
    private float fireCounter = 0f;

    public bool CanFire => this.fireCounter <= 0f;

    public int SpawnIndex { get; private set; } = 0;

    public Vector3 Position => this.transform.position;
    public float Rotation => this.transform.eulerAngles.y;

    private void Update() {
        this.fireCounter -= Time.deltaTime;
    }

    public void StartFireCooldown() {
        this.fireCounter = this.fireRate;
    }

    public void Rotate(float input) {
        float rotChange = math.clamp(input, -1f, 1f) * rotationSpeed * Time.deltaTime;
        this.SetRotation(this.rotation + rotChange);

        this.rotation = math.clamp(this.rotation, this.minRotation, this.maxRotation);
        this.transform.eulerAngles = new Vector3(0f, this.rotation, 0f);
    }

    public void IncrementSpawnIndex() {
        // Move to the next spawn in the array, wraps arround back to the begining of the array
        this.SpawnIndex = (this.SpawnIndex + 1) % this.projSpawnpoints.Length;
    }

    public void SetActiveSpawnIndex(int index) {
        this.SpawnIndex = index % this.projSpawnpoints.Length;
    }

    public void SetRotation(float newRotation) {
        /* Ensure the new rotation is clamped between min / max
        // Changes values into range [-360, 360]
        float val = newRotation % 360f;
        float min = this.minRotation % 360f;
        float max = this.maxRotation % 360f;

        // Wrap negative numbers to [0, 360]
        if (val < 0f) val += 360f;
        if (min < 0f) min += 360f;
        if (max < 0f) max += 360f;

        if (max < min) {
            if (val > max && val < min) {
                this.rotation = Mathf.Clamp(val, min, max + 360f) % 360f;
            }
        } else {
            this.rotation = Mathf.Clamp(val, min, max);
        }
        */
        this.rotation = newRotation;
        this.transform.eulerAngles = new Vector3(0f, this.rotation, 0f);
    }

    public Projectile FireProjectile() {
        var spawn = this.projSpawnpoints[this.SpawnIndex];

        var proj = SpawnProjectile(spawn.position, spawn.rotation.eulerAngles);

        this.IncrementSpawnIndex();

        return proj;
    }

    public Projectile SpawnProjectile(Vector3 position, Vector3 rotation) {
        Projectile proj;
        if (this.projPrefab != null) {
            proj = Instantiate(projPrefab, position, Quaternion.Euler(rotation));
        } else {
            // Create a place holder projectile
            proj = new GameObject("TempProjectile").AddComponent<Projectile>();
            proj.transform.position = position;
            proj.transform.eulerAngles = rotation;
            Debug.LogWarning("No projectile prefab assigned, Using subsitute projectile.");
        }
        return proj;
    }
}
