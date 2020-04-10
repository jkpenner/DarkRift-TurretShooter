using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientManager : MonoBehaviour {
    [SerializeField] ClientTurretManager turretManager;

    private UnityClient unityClient;

    public PlayerData PlayerData { get; private set; }

    public int Score { get; private set; }
    public int LifeRemaining { get; private set; }

    public event Action EventPlayerStateChange;
    public event Action EventStatsChanged;

    public event Action EventStartCounter;
    public event Action EventGameStart;
    public event Action EventGameComplete;


    private void Awake() {
        this.unityClient = FindObjectOfType<UnityClient>();
        this.unityClient.MessageReceived += OnMessageReceived;

        this.turretManager.Init(this.unityClient);
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e) {
        using (var message = e.GetMessage()) {
            switch (e.Tag) {
                case MsgTags.JoinMessage:
                    this.OnJoinMessage(message);
                    break;
                case MsgTags.PlayerUpdate:
                    this.OnPlayerUpdate(message);
                    break;
                case MsgTags.StartGameCountdown:
                    this.OnGameCountdownStart(message);
                    break;
                case MsgTags.GameStart:
                    this.OnGameStart(message);
                    break;
                case MsgTags.GameComplete:
                    this.OnGameComplete(message);
                    break;
                case MsgTags.GameStatsUpdate:
                    this.OnGameStatsUpdate(message);
                    break;
            }
        }
    }

    private void OnGameStatsUpdate(Message message) {
        Debug.Log("Stats Update");
        using (var reader = message.GetReader()) {
            Debug.Log("Reader Length: " + reader.Length);
            this.LifeRemaining = reader.ReadInt32();
            this.Score = reader.ReadInt32();

            this.EventStatsChanged?.Invoke();
        }
    }

    private void OnJoinMessage(Message message) {
        using (var reader = message.GetReader()) {
            var data = reader.ReadSerializable<PlayerData>();

            if (this.PlayerData != null) {
                Debug.Log("Received Join Message after an Update Message. Ignoring.");
                return;
            }

            Debug.Log($"Received Player Data Id: {data.ID}, Num: {data.Number}, isActive: {data.IsActive}", this);

            this.PlayerData = data;
            this.EventPlayerStateChange?.Invoke();

            // Tell the server that this client is ready
            using (Message readyMessage = Message.CreateEmpty(MsgTags.PlayerReady)) {
                this.unityClient.SendMessage(readyMessage, SendMode.Reliable);
            }
        }
    }

    private void OnPlayerUpdate(Message message) {
        using (var reader = message.GetReader()) {
            var data = reader.ReadSerializable<PlayerData>();

            if (this.PlayerData == null) {
                Debug.Log($"Received Player Data Id: {data.ID}, Num: {data.Number}, isActive: {data.IsActive}", this);

                this.PlayerData = data;
                this.EventPlayerStateChange?.Invoke();
                return;
            }



            var changed = data.IsActive != this.PlayerData.IsActive;
            changed = changed || data.Number != this.PlayerData.Number;

            this.PlayerData = data;

            if (changed) {
                Debug.Log($"Updated Player Data Id: {data.ID}, Num: {data.Number}, isActive: {data.IsActive}", this);
                this.EventPlayerStateChange?.Invoke();
            }
        }
    }


    private void OnGameComplete(Message message) {
        Debug.Log("Game Complete Message");
        this.EventGameComplete?.Invoke();
    }

    private void OnGameStart(Message message) {
        Debug.Log("Game Start Message");
        this.EventGameStart?.Invoke();
    }

    private void OnGameCountdownStart(Message message) {
        Debug.Log("Game Countdown Start Message");
        this.EventStartCounter?.Invoke();
    }
}
