using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CollapsingFloors : ObstacleBase
{
    [Header("Initial Ownership")]
    public bool solidForPlayer1Initially = true;

    [Header("Fall-through Settings")]
    public float fallThroughSeconds = 2f;
    public float autoResetSeconds = 0f;

    [Header("Player References")]
    public GameObject player1;
    public GameObject player2;

    private Collider2D platformCollider;

    private Collider2D p1Col, p2Col;
    private PlayerStats p1Stats, p2Stats;

    // live solidity
    private bool solidForP1;
    private bool solidForP2;

    // per-player standing timers
    private Dictionary<GameObject, float> standTimers = new Dictionary<GameObject, float>();

    // respawn detection helpers
    private Vector2 p1LastPos, p2LastPos;
    private bool hasP1Stats = false, hasP2Stats = false;

    // respawn detection thresholds
    private const float TELEPORT_DIST = 1.0f;
    private const float RESPAWN_EPSILON = 0.2f;
    private Coroutine autoResetRoutine;


    private void Awake()
    {
        CacheComponents();
        InitializeOwnership();
    }

    private void Start()
    {
        ApplyInitialCollisionState();
    }

    private void Update()
    {
        UpdateStandingTimers();
        DetectRespawnTeleport();
    }

    // Platform is not meant to damage or knockback → empty
    public override void AffectPlayer(GameObject player) { }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        HandleEnter(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        HandleStay(collision.collider);
    }

    protected override void OnCollisionExit2D(Collision2D collision)
    {
        HandleExit(collision.collider);
    }

    private void CacheComponents()
    {
        platformCollider = GetComponent<Collider2D>();

        if (player1)
        {
            p1Col = player1.GetComponent<Collider2D>();
            p1Stats = player1.GetComponent<PlayerStats>();
            hasP1Stats = p1Stats != null;
            p1LastPos = player1.transform.position;
        }

        if (player2)
        {
            p2Col = player2.GetComponent<Collider2D>();
            p2Stats = player2.GetComponent<PlayerStats>();
            hasP2Stats = p2Stats != null;
            p2LastPos = player2.transform.position;
        }
    }

    private void InitializeOwnership()
    {
        solidForP1 = solidForPlayer1Initially;
        solidForP2 = !solidForPlayer1Initially;
    }

    private void ApplyInitialCollisionState()
    {
        ApplyCollisionState(player1, p1Col, solidForP1);
        ApplyCollisionState(player2, p2Col, solidForP2);
    }

    private void HandleEnter(Collider2D col)
    {
        if (col == p1Col)
            OnPlayerEnter(player1, ref solidForP1, ref solidForP2, p1Col, p2Col);

        else if (col == p2Col)
            OnPlayerEnter(player2, ref solidForP2, ref solidForP1, p2Col, p1Col);
    }

    private void HandleStay(Collider2D col)
    {
        if (col == p1Col && solidForP1)
            EnsureTimerExists(player1);

        else if (col == p2Col && solidForP2)
            EnsureTimerExists(player2);
    }

    private void HandleExit(Collider2D col)
    {
        if (col == p1Col) standTimers.Remove(player1);
        else if (col == p2Col) standTimers.Remove(player2);
    }


    private void OnPlayerEnter(
        GameObject player,
        ref bool solidForThisPlayer,
        ref bool solidForOtherPlayer,
        Collider2D thisCol,
        Collider2D otherCol)
    {
        if (!solidForThisPlayer)
        {
            Physics2D.IgnoreCollision(thisCol, platformCollider, true);
            return;
        }

        // Owner lands → make platform solid for both
        if (solidForThisPlayer && !solidForOtherPlayer)
        {
            solidForOtherPlayer = true;
            ApplyCollisionState(GetOtherPlayer(player), otherCol, true);
        }

        ResetOrStartTimer(player);
    }

    private void UpdateStandingTimers()
    {
        if (standTimers.Count == 0) return;

        var keys = new List<GameObject>(standTimers.Keys);

        foreach (var p in keys)
        {
            standTimers[p] += Time.deltaTime;

            if (standTimers[p] >= fallThroughSeconds)
            {
                TriggerFallThrough(p);
                standTimers.Remove(p);

                if (autoResetSeconds > 0)
                {
                    if (autoResetRoutine != null) StopCoroutine(autoResetRoutine);
                    autoResetRoutine = StartCoroutine(AutoReset(autoResetSeconds));
                }
            }
        }
    }

    private void TriggerFallThrough(GameObject player)
    {
        if (player == player1 && solidForP1)
        {
            solidForP1 = false;
            ApplyCollisionState(player1, p1Col, false);
        }
        else if (player == player2 && solidForP2)
        {
            solidForP2 = false;
            ApplyCollisionState(player2, p2Col, false);
        }
    }

    private void EnsureTimerExists(GameObject player)
    {
        if (!standTimers.ContainsKey(player))
            standTimers.Add(player, 0f);
    }

    private void ResetOrStartTimer(GameObject player)
    {
        standTimers[player] = 0f;
    }

    private void ApplyCollisionState(GameObject playerObj, Collider2D playerCol, bool solid)
    {
        if (!playerObj || !playerCol) return;

        // solid → collisions allowed (ignore=false)
        Physics2D.IgnoreCollision(playerCol, platformCollider, !solid);
    }

    private void DetectRespawnTeleport()
    {
        if (hasP1Stats) CheckRespawn(player1, ref p1LastPos, p1Stats.respawnPoint);
        if (hasP2Stats) CheckRespawn(player2, ref p2LastPos, p2Stats.respawnPoint);
    }

    private void CheckRespawn(GameObject player, ref Vector2 lastPos, Vector2 respawnPoint)
    {
        Vector2 current = player.transform.position;

        float moved = Vector2.Distance(current, lastPos);
        float distToRespawn = Vector2.Distance(current, respawnPoint);

        if (moved > TELEPORT_DIST && distToRespawn <= RESPAWN_EPSILON)
        {
            ResetToOriginal();
        }

        lastPos = current;
    }

    public void ResetToOriginal()
    {
        standTimers.Clear();

        solidForP1 = solidForPlayer1Initially;
        solidForP2 = !solidForPlayer1Initially;

        ApplyCollisionState(player1, p1Col, solidForP1);
        ApplyCollisionState(player2, p2Col, solidForP2);

        if (autoResetRoutine != null)
        {
            StopCoroutine(autoResetRoutine);
            autoResetRoutine = null;
        }
    }

    private IEnumerator AutoReset(float time)
    {
        yield return new WaitForSeconds(time);
        ResetToOriginal();
    }

    private GameObject GetOtherPlayer(GameObject p)
    {
        return p == player1 ? player2 : player1;
    }

    public override void ToggleActive(bool state)
    {
        gameObject.SetActive(state);
    }
}
