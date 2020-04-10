using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserInterfaceManager : MonoBehaviour {
    [SerializeField] ClientManager manager;

    [SerializeField] GameObject queue;
    [SerializeField] Text queueText;

    [SerializeField] GameObject stats;
    [SerializeField] Text statsText;

    [SerializeField] GameObject message;
    [SerializeField] Text messageText;

    private Coroutine routine;

    private void Awake() {
        this.queue.SetActive(false);
        this.message.SetActive(false);
        this.stats.SetActive(false);
    }

    private void OnEnable() {
        manager.EventGameComplete += OnGameComplete;
        manager.EventGameStart += OnGameStart;
        manager.EventStartCounter += OnStartCounter;

        manager.EventPlayerStateChange += OnPlayerStateChange;
        manager.EventStatsChanged += OnStatsChanged;
    }

    private void OnDisable() {
        if (manager != null) {
            manager.EventGameComplete -= OnGameComplete;
            manager.EventGameStart -= OnGameStart;
            manager.EventStartCounter -= OnStartCounter;

            manager.EventPlayerStateChange -= OnPlayerStateChange;
            manager.EventStatsChanged -= OnStatsChanged;
        }
    }

    private void OnStatsChanged() {
        this.stats.SetActive(true);
        this.statsText.text = string.Format(
            "Score: {0}, Lifes Remaining: {1}",
            this.manager.Score, this.manager.LifeRemaining);
    }

    private void OnPlayerStateChange() {
        this.queue.SetActive(
            !this.manager.PlayerData.IsActive);

        this.queueText.text = string.Format(
            "Waiting for turn. Queue #{0}", 
            this.manager.PlayerData.Number);
    }

    private void OnStartCounter() {
        if (routine != null) {
            StopCoroutine(routine);
        }

        routine = StartCoroutine(HandleStartCounter());
    }

    private IEnumerator HandleStartCounter() {
        var waitSecond = new WaitForSeconds(1f);

        message.SetActive(true);
        for (int i = 5; i >= 0; i--) {
            messageText.text = i.ToString();
            yield return waitSecond;
        }
        message.SetActive(true);
    }

    private void OnGameStart() {
        if (routine != null) {
            StopCoroutine(routine);
        }

        routine = StartCoroutine(HandleStartGame());
    }

    IEnumerator HandleStartGame() {
        message.SetActive(true);
        messageText.text = "Start";
        yield return new WaitForSeconds(1f);
        message.SetActive(false);
    }

    private void OnGameComplete() {
        if (routine != null) {
            StopCoroutine(routine);
        }

        routine = StartCoroutine(HandleCompleteGame());
    }

    IEnumerator HandleCompleteGame() {
        message.SetActive(true);
        messageText.text = "Complete";
        yield return new WaitForSeconds(1f);
        messageText.text = "Waiting For Match";
        message.SetActive(true);
    }
}
