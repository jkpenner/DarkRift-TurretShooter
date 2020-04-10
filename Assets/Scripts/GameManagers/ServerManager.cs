using DarkRift;
using DarkRift.Server;
using DarkRift.Server.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Handle all incoming client connections
 * Set all clients as spectators
 * When enough clients are connected or when a game is finished find
 * two new clients to become active players.
 * send message to both clients of their state change and when the game starts
 * 
 * Game Loop
 * - Wait for x players to start.
 * - Assign player's to turrets
 * - Update all player's states
 * - Trigger Count Down Timer
 * - Start Game
 * - Wait for failure state or time limit
 * - Complete Game
 * - Restart Game Loop
 * 
 */


public class PlayerCollection {
    private List<IClient> queue;
    private Dictionary<IClient, ServerPlayerData> players;

    public IReadOnlyDictionary<IClient, ServerPlayerData> Players => this.players;

    public int ReadyCount {
        get {
            int count = 0;
            foreach (var player in players.Values) {
                if (player.IsReady) count++;
            }
            return count;
        }
    }

    public PlayerCollection() {
        this.queue = new List<IClient>();
        this.players = new Dictionary<IClient, ServerPlayerData>();
    }

    public void AddClient(IClient client) {
        this.players.Add(client, new ServerPlayerData
        {
            ID = client.ID,
            IsActive = false,
            Number = -1,
        });
    }

    public void RemoveClient(IClient client) {
        this.queue.Remove(client);
        this.players.Remove(client);
    }

    public IClient DequeueReadyPlayer() {
        if (this.queue.Count > 0) {
            var player = this.queue[0];
            this.queue.RemoveAt(0);

            return player;
        }
        return null;
    }

    public ServerPlayerData GetData(IClient client) {
        if (this.players.TryGetValue(client, out var data)) {
            return data;
        }
        return null;
    }

    public void ReadyClient(IClient client) {
        var data = GetData(client);
        if (data != null) {
            // Mark the client as ready.
            data.MarkIsReady();

            // Add them to the queue of player
            this.queue.Add(client);
        }
    }

    public void RequeueAllMissing() {
        foreach (var player in this.players) {
            if (player.Value.IsReady && !queue.Contains(player.Key)) {
                queue.Add(player.Key);
            }
        }
    }

    public void UpdatePlayerStates() {
        for (var i = 0; i < this.queue.Count; i++) {
            var data = this.GetData(this.queue[i]);
            if (data != null) {
                data.IsActive = false;
                data.Number = i + 1;
            } else {
                Debug.LogWarning("Unhandled player in player queue");
            }
        }
    }
}


public class ServerManager : MonoBehaviour {
    private XmlUnityServer unityServer;

    [SerializeField] int maxMissedEnemies = 10;

    [SerializeField] ServerTurretManager turretManager;
    [SerializeField] ServerEnemyManager enemyManager;

    private PlayerCollection players;
    private Coroutine startRoutine;

    public bool IsGameActive { get; private set; }

    public int Score { get; private set; }
    public int LifesRemaining { get; private set; }

    public DarkRiftServer Server {
        get => this.unityServer.Server;
    }

    public IClientManager ClientManager {
        get {
            if (this.Server != null)
                return this.Server.ClientManager;
            return null;
        }
    }

    private void Awake() {
        this.IsGameActive = false;

        this.players = new PlayerCollection();
        this.unityServer = FindObjectOfType<XmlUnityServer>();

        StartCoroutine(this.HandleWaitForServerStart());
    }

    private IEnumerator HandleWaitForServerStart() {
        yield return new WaitUntil(() => Server != null);

        this.turretManager.Init(this.Server);

        this.enemyManager.Init(this.Server);
        this.enemyManager.EventEnemyDestroyed += OnEnemyDestroyed;
        this.enemyManager.EventEnemyReachedTarget += OnEnemyReachedTarget;

        ClientManager.ClientConnected += OnClientConnect;
        ClientManager.ClientDisconnected += OnClientDisconnect;
    }

    private void OnEnemyDestroyed() {
        this.Score++;
        this.SendGameStatsUpdateMessage();
    }

    private void OnEnemyReachedTarget() {
        this.LifesRemaining--;

        this.SendGameStatsUpdateMessage();
        if (this.LifesRemaining <= 0) {
            CancelGame();
        }
    }

    private void OnClientConnect(object sender, ClientConnectedEventArgs e) {
        var client = e.Client;

        this.players.AddClient(client);

        // Send the new clients information back to them
        using (var writer = DarkRiftWriter.Create()) {
            writer.Write(this.players.GetData(client));

            using (var message = Message.Create(MsgTags.JoinMessage, writer)) {
                client.SendMessage(message, SendMode.Reliable);
            }
        }

        client.MessageReceived += OnMessageReceived;
    }

    private void OnClientDisconnect(object sender, ClientDisconnectedEventArgs e) {
        var data = this.players.GetData(e.Client);
        this.players.RemoveClient(e.Client);

        // What should happend when a player leaves?
        if (data.IsActive) {
            this.CancelGame();
        }
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e) {
        using (var message = e.GetMessage()) {
            switch (message.Tag) {
                case MsgTags.PlayerReady:
                    this.OnPlayerReady(e.Client, message);
                    break;
            }
        }
    }

    private void OnPlayerReady(IClient client, Message message) {
        this.players.ReadyClient(client);

        if (this.CheckIfGameCanStart()) {
            this.StartGame();
        }
    }

    private bool CheckIfGameCanStart() {
        Debug.Log("CheckIfGameCanStart");
        if (this.IsGameActive)
            return false;

        if (this.players.ReadyCount < this.turretManager.MaxTurretCount)
            return false;

        return true;
    }

    private void StartGame() {
        Debug.Log("StartGame");
        this.startRoutine = StartCoroutine(this.HandleStartGame());
    }

    private void CancelGame() {
        Debug.Log("CancelGame");
        if (this.startRoutine != null) {
            StopCoroutine(this.startRoutine);
        }

        this.SendGameCanceledMessage();
        this.enemyManager.StopEnemyWave();

        if (this.CheckIfGameCanStart()) {
            this.StartGame();
        } else {
            this.RequeueAllActivePlayers();
            this.UpdateAllPlayerStates();
            this.SendPlayerStateUpdateMessage();
        }
    }

    private IEnumerator HandleStartGame() {
        Debug.Log("HandleStartGame");
        this.RequeueAllActivePlayers();
        this.DequeueAndAssignPlayers();
        this.UpdateAllPlayerStates();
        this.SendPlayerStateUpdateMessage();

        this.LifesRemaining = this.maxMissedEnemies;
        this.Score = 0;
        this.SendGameStatsUpdateMessage();

        // Temp delay between messages
        yield return new WaitForSeconds(1f);

        this.SendCountdownStartMessage();

        yield return new WaitForSeconds(5f);

        this.SendGameStartMessage();
        this.startRoutine = null;
        this.enemyManager.StartEnemyWave();
    }

    private void RequeueAllActivePlayers() {
        Debug.Log("RequeueAllActivePlayers");
        this.turretManager.DespawnTurret();
        this.players.RequeueAllMissing();
    }

    private void DequeueAndAssignPlayers() {
        var clients = new IClient[this.turretManager.MaxTurretCount];

        // Assign Active Players
        for (var i = 0; i < clients.Length; i++) {
            // Assign the client to the slot
            clients[i] = this.players.DequeueReadyPlayer();
        }

        this.turretManager.SpawnTurret(clients);
    }

    private void UpdateAllPlayerStates() {
        // Assign Active Players
        for (var i = 0; i < this.turretManager.MaxTurretCount; i++) {
            // Remove the client from the queue
            var client = this.turretManager.GetClientInTurret(i);

            // Skip empty slots
            if (client == null) continue;

            var player = this.players.GetData(client);
            if (player != null) {
                player.IsActive = true;
                player.Number = i + 1;
            } else {
                Debug.Log("Unhandled Player");
            }
        }

        // Update the player's in the queue
        this.players.UpdatePlayerStates();
    }

    #region Message Senders
    private void SendPlayerStateUpdateMessage() {
        using (var message = Message.CreateEmpty(MsgTags.PlayerUpdate)) {
            foreach (var player in this.players.Players) {
                var client = player.Key;
                var data = player.Value;

                using (var writer = DarkRiftWriter.Create()) {
                    writer.Write(data);
                    message.Serialize(writer);
                }

                client.SendMessage(message, SendMode.Reliable);
            }
        }
    }

    private void SendGameCanceledMessage() {
        using (var completeMessage = Message.CreateEmpty(MsgTags.GameCanceled)) {
            foreach (var client in this.Server.ClientManager.GetAllClients()) {
                client.SendMessage(completeMessage, SendMode.Reliable);
            }
        }
    }

    private void SendGameCompleteMessage() {
        using (var completeMessage = Message.CreateEmpty(MsgTags.GameComplete)) {
            foreach (var client in this.Server.ClientManager.GetAllClients()) {
                client.SendMessage(completeMessage, SendMode.Reliable);
            }
        }
    }

    private void SendCountdownStartMessage() {
        // Send start message
        using (var countdownMessage = Message.CreateEmpty(MsgTags.StartGameCountdown)) {
            foreach (var client in this.Server.ClientManager.GetAllClients()) {
                client.SendMessage(countdownMessage, SendMode.Reliable);
            }
        }
    }

    private void SendGameStartMessage() {
        using (var startMessage = Message.CreateEmpty(MsgTags.GameStart)) {
            foreach (var client in this.Server.ClientManager.GetAllClients()) {
                client.SendMessage(startMessage, SendMode.Reliable);
            }
        }
    }

    private void SendGameStatsUpdateMessage() {
        using (var writer = DarkRiftWriter.Create()) {
            writer.Write(this.LifesRemaining);
            writer.Write(this.Score);

            using (var startMessage = Message.Create(MsgTags.GameStatsUpdate, writer)) {
                foreach (var client in this.Server.ClientManager.GetAllClients()) {
                    client.SendMessage(startMessage, SendMode.Reliable);
                }
            }
        }
    }

    #endregion
}
