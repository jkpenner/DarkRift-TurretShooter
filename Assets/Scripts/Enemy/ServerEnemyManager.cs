using DarkRift;
using DarkRift.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerEnemyManager : MonoBehaviour {
    [SerializeField] ServerEnemyController enemyPrefab;
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] Transform[] targetPoints;

    private DarkRiftServer server;
    private uint maxActiveEnemyId = 0;
    private Dictionary<uint, ServerEnemyController> enemies = new Dictionary<uint, ServerEnemyController>();

    private Coroutine waveSpawnRoutine;

    public event Action EventEnemyReachedTarget;
    public event Action EventEnemyDestroyed;

    public void InvokeEnemyReachedTarget() {
        this.EventEnemyReachedTarget?.Invoke();
    }

    public void InvokeEnemyDestroyed() {
        this.EventEnemyDestroyed?.Invoke();
    }

    public void Init(DarkRiftServer server) {
        this.server = server;
    }

    public void StartEnemyWave() {
        Debug.Log("Starting Wave");
        this.waveSpawnRoutine = StartCoroutine(HandleWaveSpawning());
    }

    private IEnumerator HandleWaveSpawning() {
        List<ServerEnemyController> spawned = new List<ServerEnemyController>();

        while (true) {
            for (var i = 0; i < UnityEngine.Random.Range(1, 6); i++) {
                spawned.Add(SpawnEnemy());
            }

            using (var writer = DarkRiftWriter.Create()) {
                foreach (var enemy in spawned) {
                    writer.Write(new EnemyData
                    {
                        ID = enemy.Id,
                        Position = enemy.transform.position,
                    });
                }

                using (var spawnMessage = Message.Create(MsgTags.EnemyUpdate, writer)) {
                    foreach (var client in this.server.ClientManager.GetAllClients()) {
                        client.SendMessage(spawnMessage, SendMode.Reliable);
                    }
                }
            }

            spawned.Clear();

            yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 1f));
        }
    }

    private void Update() {
        // Only send update messages if a wave is active
        if (this.waveSpawnRoutine == null)
            return;

        using (var writer = DarkRiftWriter.Create()) {
            foreach (var enemy in this.enemies.Values) {
                if (enemy == null)
                    continue;

                writer.Write(new EnemyData
                {
                    ID = enemy.Id,
                    Position = enemy.transform.position,
                });

            }

            if (writer.Position == 0)
                return;

            using (var updateMessage = Message.Create(MsgTags.EnemyUpdate, writer)) {
                foreach (var client in this.server.ClientManager.GetAllClients()) {
                    client.SendMessage(updateMessage, SendMode.Unreliable);
                }
            }
        }
    }

    public void StopEnemyWave() {
        Debug.Log("Wave Canceled");
        if (this.waveSpawnRoutine != null) {
            StopCoroutine(this.waveSpawnRoutine);
            this.DespawnAllEnemies();
        }
    }

    public ServerEnemyController SpawnEnemy() {
        var enemy = Instantiate(enemyPrefab, GetRandomSpawn(), Quaternion.identity);
        enemy.Server = this.server;
        enemy.Manager = this;
        enemy.Setup(this.ClaimAvailableId(), this.GetRandomTarget());
        this.enemies.Add(enemy.Id, enemy);
        return enemy;
    }

    public void DespawnEnemy(ServerEnemyController enemy) {
        if (this.enemies.ContainsKey(enemy.Id)) {
            this.enemies.Remove(enemy.Id);
        }

        using (var writer = DarkRiftWriter.Create()) {
            writer.Write(new EnemyDespawnData
            {
                ID = enemy.Id
            });

            using (var despawnMessage = Message.Create(MsgTags.EnemyDespawn, writer)) {
                foreach (var client in this.server.ClientManager.GetAllClients()) {
                    client.SendMessage(despawnMessage, SendMode.Reliable);
                }
            }
        }

        Destroy(enemy.gameObject);
    }

    public void DespawnAllEnemies() {
        using (var writer = DarkRiftWriter.Create()) {
            foreach (var enemy in this.enemies.Values) {
                if (enemy == null)
                    continue;

                writer.Write(new EnemyData
                {
                    ID = enemy.Id,
                });
                Destroy(enemy.gameObject);
            }

            // Check if anything was writen
            if (writer.Position == 0)
                return;

            using (var despawnMessage = Message.Create(MsgTags.EnemyDespawn, writer)) {
                foreach (var client in this.server.ClientManager.GetAllClients()) {
                    client.SendMessage(despawnMessage, SendMode.Unreliable);
                }
            }
        }
    }

    public uint ClaimAvailableId() {
        return ++this.maxActiveEnemyId;
    }

    public Vector3 GetRandomSpawn() {
        return spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)].position;
    }

    public Vector3 GetRandomTarget() {
        return targetPoints[UnityEngine.Random.Range(0, targetPoints.Length)].position;
    }
}
