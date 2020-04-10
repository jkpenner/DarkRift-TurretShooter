using DarkRift;
using DarkRift.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Turret))]
public class ServerTurretController : MonoBehaviour {
    public ServerTurretManager Manager { get; set; }
    public DarkRiftServer Server { get; set; }
    public IClient Client { get; private set; }
    public Turret Turret { get; private set; }

    private void Awake() {
        this.Turret = this.GetComponent<Turret>();
    }

    public void SpawnTurret(IClient client) {
        this.Client = client;
        this.Client.MessageReceived += OnMessageReceived;
    }

    public void DespawnTurret() {
        if (this.Client != null) {
            this.Client.MessageReceived -= OnMessageReceived;
            this.Client = null;
        }
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e) {
        using (var message = e.GetMessage()) {
            switch (message.Tag) {
                case MsgTags.TurretUpdate:
                    this.OnTurretUpdateMessage(message);
                    break;
                case MsgTags.TurretFire:
                    this.OnTurretFireMessage(message);
                    break;
            }
        }
    }

    private void OnTurretUpdateMessage(Message message) {
        using (var reader = message.GetReader()) {
            var data = reader.ReadSerializable<TurretUpdateData>();
            
            this.Turret.SetRotation(data.Rotation);

            using (var writer = DarkRiftWriter.Create()) {
                writer.Write(new TurretUpdateData
                {
                    ClientId = this.Client.ID,
                    Rotation = this.Turret.Rotation
                });

                using (var updateMessage = Message.Create(MsgTags.TurretUpdate, writer)) {
                    foreach (var client in this.Server.ClientManager.GetAllClients()) {
                        if (client == this.Client)
                            continue;

                        client.SendMessage(updateMessage, SendMode.Unreliable);
                    }
                }
            }
            
        }
    }

    private void OnTurretFireMessage(Message message) {
        using (var reader = message.GetReader()) {
            var data = reader.ReadSerializable<TurretFireData>();

            this.Turret.SetRotation(data.Rotation);
            this.Turret.SetActiveSpawnIndex(data.Barrel);

            using (var writer = DarkRiftWriter.Create()) {
                writer.Write(new TurretUpdateData
                {
                    ClientId = this.Client.ID,
                    Rotation = this.Turret.Rotation
                });

                using (var updateMessage = Message.Create(MsgTags.TurretUpdate, writer)) {
                    foreach (var client in this.Server.ClientManager.GetAllClients()) {
                        client.SendMessage(updateMessage, SendMode.Unreliable);
                    }
                }
            }

            var proj = this.Turret.FireProjectile();
            proj.EventCollision += OnProjectileCollision;
            //proj.Setup(this.Server);

            using (var writer = DarkRiftWriter.Create()) {
                writer.Write(new ProjectileSpawnData
                {
                    Position = proj.transform.position,
                    Rotation = proj.transform.eulerAngles,
                });

                using (var spawnProjMessage = Message.Create(MsgTags.ProjectileSpawn, writer)) {
                    foreach (var client in this.Server.ClientManager.GetAllClients()) {
                        client.SendMessage(spawnProjMessage, SendMode.Reliable);
                    }
                }
            }
        }
    }

    private void OnProjectileCollision(Projectile proj, GameObject other) {
        // Handle Collisions here
    }
}
