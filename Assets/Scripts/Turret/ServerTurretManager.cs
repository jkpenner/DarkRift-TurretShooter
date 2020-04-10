using DarkRift;
using DarkRift.Server;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerTurretManager : MonoBehaviour {
    [SerializeField] ServerTurretController turretPrefab;
    [SerializeField] Transform[] turretSpawns;

    private DarkRiftServer server;
    private ServerTurretController[] turrets;

    public int MaxTurretCount => turretSpawns.Length;

    public void Init(DarkRiftServer server) {
        this.server = server;

        this.turrets = new ServerTurretController[turretSpawns.Length];
        for (var i = 0; i < turrets.Length; ++i) {
            turrets[i] = Instantiate(
                turretPrefab,
                turretSpawns[i].position,
                turretSpawns[i].rotation);
            turrets[i].Manager = this;
            turrets[i].Server = this.server;
        }
    }

    public IClient GetClientInTurret(int index) {
        if (index >= 0 && index < this.turrets.Length) {
            return this.turrets[index].Client;
        }
        return null;
    }

    public void SpawnTurret(IClient[] clients) {
        if (clients.Length != turrets.Length) {
            Debug.LogError("Amount of clients to spawn does not match turrets");
            return;
        }

        for (var i = 0; i < turrets.Length; i++) {
            this.turrets[i].SpawnTurret(clients[i]);
        }

        using (var writer = DarkRiftWriter.Create()) {
            for (ushort i = 0; i < turrets.Length; i++) {
                writer.Write(new TurretSpawnData
                {
                    ID = turrets[i].Client.ID,
                    Index = i
                });
            }

            using (var spawnMessage = Message.Create(MsgTags.TurretSpawn, writer)) {
                foreach (var client in this.server.ClientManager.GetAllClients()) {
                    client.SendMessage(spawnMessage, SendMode.Reliable);
                }
            }
        }
    }

    public void DespawnTurret() {
        for (var i = 0; i < turrets.Length; i++) {
            this.turrets[i].DespawnTurret();
        }

        using (var writer = DarkRiftWriter.Create()) {
            for (ushort i = 0; i < turrets.Length; i++) {
                writer.Write(new TurretSpawnData
                {
                    ID = 0,
                    Index = i
                });
            }

            using (var spawnMessage = Message.Create(MsgTags.TurretDespawn, writer)) {
                foreach (var client in this.server.ClientManager.GetAllClients()) {
                    client.SendMessage(spawnMessage, SendMode.Reliable);
                }
            }
        }
    }
}
