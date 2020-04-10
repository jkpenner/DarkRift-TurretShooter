using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientEnemyManager : MonoBehaviour {
    [SerializeField] ClientEnemyController enemyPrefab;

    private UnityClient unityClient;
    private Dictionary<uint, ClientEnemyController> enemies;

    private void Awake() {
        this.enemies = new Dictionary<uint, ClientEnemyController>();

        this.unityClient = FindObjectOfType<UnityClient>();
        this.unityClient.MessageReceived += OnMessageReceived;
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e) {
        if (e.Tag != MsgTags.EnemyUpdate && e.Tag != MsgTags.EnemyDespawn)
            return;

        using (var message = e.GetMessage())
        using (var reader = message.GetReader()) {
            switch (message.Tag) {
                case MsgTags.EnemyUpdate:
                    this.OnEnemyUpdate(reader);
                    break;
                case MsgTags.EnemyDespawn:
                    this.OnEnemyDespawn(reader);
                    break;
            }
        }
    }

    private void OnEnemyUpdate(DarkRiftReader reader) {
        while (reader.Position < reader.Length) {
            var data = reader.ReadSerializable<EnemyData>();
            if (this.enemies.ContainsKey(data.ID)) {
                var enemy = this.enemies[data.ID];
                enemy.transform.position = data.Position;
            } else {
                var enemy = Instantiate(enemyPrefab, data.Position, Quaternion.identity);
                this.enemies.Add(data.ID, enemy);
            }
        }
    }

    private void OnEnemyDespawn(DarkRiftReader reader) {
        while (reader.Position < reader.Length) {
            var data = reader.ReadSerializable<EnemyDespawnData>();
            if (this.enemies.ContainsKey(data.ID)) {
                Destroy(this.enemies[data.ID].gameObject);
                this.enemies.Remove(data.ID);
            }
        }
    }

}
