using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;

[RequireComponent(typeof(Turret))]
public class ClientTurretController : MonoBehaviour {
    private UnityClient unityClient;
    private Turret turret;

    public ClientTurretManager Manager { get; set; }

    public int ClientId { get; set; } = -1;
    public bool IsLocalPlayer { get; set; } = false;

    private void Awake() {
        this.turret = this.GetComponent<Turret>();
        this.unityClient = FindObjectOfType<UnityClient>();
        this.unityClient.MessageReceived += OnMessageReceived;
    }

    private void Update() {
        if (this.IsLocalPlayer) {
            this.turret.Rotate(Input.GetAxisRaw("Horizontal"));
            this.SendTurretUpdateMessage();

            if (Input.GetKey(KeyCode.Space) && this.turret.CanFire) {
                this.turret.StartFireCooldown();
                this.SendFireMessage();


                // Spawn fire partical effect here


                this.turret.IncrementSpawnIndex();
            }
        }
    }

    public void SpawnTurret(int clientId) {
        this.ClientId = clientId;
        this.IsLocalPlayer = this.unityClient.ID == clientId;
    }

    public void DespawnTurret() {
        this.ClientId = -1;
        this.IsLocalPlayer = false;
    }

    private void SendTurretUpdateMessage() {
        using (var writer = DarkRiftWriter.Create()) {
            writer.Write(new TurretUpdateData
            {
                Rotation = this.turret.Rotation
            });

            using (var fireMessage = Message.Create(MsgTags.TurretUpdate, writer)) {
                this.unityClient.SendMessage(fireMessage, SendMode.Unreliable);
            }
        }
    }

    private void SendFireMessage() {
        using (var writer = DarkRiftWriter.Create()) {
            writer.Write(new TurretFireData
            {
                Barrel = (ushort)this.turret.SpawnIndex,
                Rotation = this.turret.Rotation
            });

            using (var fireMessage = Message.Create(MsgTags.TurretFire, writer)) {
                this.unityClient.SendMessage(fireMessage, SendMode.Reliable);
            }
        }
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e) {
        using (var message = e.GetMessage()) {
            switch (message.Tag) {
                case MsgTags.TurretUpdate:
                    this.OnTurretUpdateMessage(message);
                    break;
                case MsgTags.ProjectileSpawn:
                    this.OnProjectileSpawn(message);
                    break;
            }
        }
    }

    private void OnTurretUpdateMessage(Message message) {
        using (var reader = message.GetReader()) {
            var data = reader.ReadSerializable<TurretUpdateData>();
            if (!this.IsLocalPlayer && data.ClientId == this.ClientId) {
                this.turret.SetRotation(data.Rotation);
            }
        }
    }

    private void OnProjectileSpawn(Message message) {
        using (var reader = message.GetReader()) {
            var data = reader.ReadSerializable<ProjectileSpawnData>();
            this.turret.SpawnProjectile(data.Position, data.Rotation);
        }
    }
}
