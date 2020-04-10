using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientTurretManager : MonoBehaviour {
    [SerializeField] ClientTurretController turretPrefab;
    [SerializeField] Transform[] turretSpawns;

    private UnityClient unityClient;
    private ClientTurretController[] turrets;

    public void Init(UnityClient unityClient) {
        this.unityClient = unityClient;
        this.unityClient.MessageReceived += OnMessageReceived;

        this.turrets = new ClientTurretController[turretSpawns.Length];
        for (var i = 0; i < turrets.Length; ++i) {
            turrets[i] = Instantiate(
                turretPrefab,
                turretSpawns[i].position,
                turretSpawns[i].rotation);
            turrets[i].Manager = this;
        }
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e) {
        using (var message = e.GetMessage()) {
            switch (message.Tag) {
                case MsgTags.TurretSpawn:
                    this.OnTurretSpawnMessage(message);
                    break;
                case MsgTags.TurretDespawn:
                    this.OnTurretDespawnMessage(message);
                    break;
            }
        }
    }

    // Allows for multiple spawn events in one message
    private void OnTurretSpawnMessage(Message message) {
        using (var reader = message.GetReader()) {
            while (reader.Position < reader.Length) {
                var data = reader.ReadSerializable<TurretSpawnData>();
                if (data.Index >= 0 && data.Index < this.turrets.Length) {
                    this.turrets[data.Index].SpawnTurret(data.ID);
                } else {
                    Debug.Log($"Received an invalid turret index of {data.Index}", this);
                }
            }
        }
    }

    // Allows for multiple despawn events in one message
    private void OnTurretDespawnMessage(Message message) {
        using (var reader = message.GetReader()) {
            while (reader.Position < reader.Length) {
                var data = reader.ReadSerializable<TurretSpawnData>();
                if (data.Index >= 0 && data.Index < this.turrets.Length) {
                    this.turrets[data.Index].DespawnTurret();
                } else {
                    Debug.Log($"Received an invalid turret index of {data.Index}", this);
                }
            }
        }
    }
}
