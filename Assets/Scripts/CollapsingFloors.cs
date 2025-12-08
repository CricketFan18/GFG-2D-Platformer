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
    private Collider2D p1Collider, p2Collider;
    private PlayerStats p1Stats, p2Stats;

    private bool solidForP1;
    private bool solidForP2;
    private Dictionary<Collider2D, float> standTimers = new Dictionary<Collider2D, float>();
    private Coroutine autoResetRoutine;

    private Vector2 p1LastPos, p2LastPos;
    private const float TELEPORT_DIST = 1.0f;
    private const float RESPAWN_EPSILON = 0.2f;

    private void Awake()
    {
        platformCollider = GetComponent<Collider2D>();
        CachePlayerComponents();
        ResetStateVariables();
    }

    private void Start()
    {
        SetCollision(p1Collider, solidForP1);
        SetCollision(p2Collider, solidForP2);
    }

    private void Update()
    {
        ProcessStandTimers();
        CheckRespawnTeleport();
    }

    public override void AffectPlayer(GameObject player) { }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        Collider2D col = collision.collider;

        // Logic: When the owner lands, the platform becomes solid for the OTHER player too
        if (col == p1Collider)
        {
            if (solidForP1 && !solidForP2)
            {
                solidForP2 = true;
                SetCollision(p2Collider, true);
            }
            standTimers[col] = 0f; // Start timer
        }
        else if (col == p2Collider)
        {
            if (solidForP2 && !solidForP1)
            {
                solidForP1 = true;
                SetCollision(p1Collider, true);
            }
            standTimers[col] = 0f; // Start timer
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Ensure timer exists while standing
        if ((collision.collider == p1Collider && solidForP1) || (collision.collider == p2Collider && solidForP2))
        {
            if (!standTimers.ContainsKey(collision.collider))
                standTimers[collision.collider] = 0f;
        }
    }

    protected override void OnCollisionExit2D(Collision2D collision)
    {
        // Stop tracking time if they leave
        if (standTimers.ContainsKey(collision.collider))
        {
            standTimers.Remove(collision.collider);
        }
    }

    private void ProcessStandTimers()
    {
        if (standTimers.Count == 0) return;
        var activeColliders = new List<Collider2D>(standTimers.Keys);

        foreach (var col in activeColliders)
        {
            standTimers[col] += Time.deltaTime;

            if (standTimers[col] >= fallThroughSeconds)
            {
                DisableForPlayer(col);
                standTimers.Remove(col);

                if (autoResetSeconds > 0)
                {
                    if (autoResetRoutine != null) StopCoroutine(autoResetRoutine);
                    autoResetRoutine = StartCoroutine(AutoResetRoutine());
                }
            }
        }
    }

    private void DisableForPlayer(Collider2D col)
    {
        if (col == p1Collider)
        {
            solidForP1 = false;
            SetCollision(p1Collider, false);
        }
        else if (col == p2Collider)
        {
            solidForP2 = false;
            SetCollision(p2Collider, false);
        }
    }
    private void SetCollision(Collider2D playerCol, bool isSolid)
    {
        if (playerCol != null)
        {
            Physics2D.IgnoreCollision(playerCol, platformCollider, !isSolid);
        }
    }

    public void ResetToOriginal()
    {
        standTimers.Clear();
        ResetStateVariables();

        SetCollision(p1Collider, solidForP1);
        SetCollision(p2Collider, solidForP2);

        if (autoResetRoutine != null)
        {
            StopCoroutine(autoResetRoutine);
            autoResetRoutine = null;
        }
    }

    private void ResetStateVariables()
    {
        solidForP1 = solidForPlayer1Initially;
        solidForP2 = !solidForPlayer1Initially;
    }

    private IEnumerator AutoResetRoutine()
    {
        yield return new WaitForSeconds(autoResetSeconds);
        ResetToOriginal();
    }

    private void CheckRespawnTeleport()
    {
        CheckSinglePlayerRespawn(player1, p1Stats, ref p1LastPos);
        CheckSinglePlayerRespawn(player2, p2Stats, ref p2LastPos);
    }

    private void CheckSinglePlayerRespawn(GameObject player, PlayerStats stats, ref Vector2 lastPos)
    {
        if (player == null || stats == null) return;

        Vector2 currentPos = player.transform.position;
        float distMoved = Vector2.Distance(currentPos, lastPos);
        float distToRespawn = Vector2.Distance(currentPos, stats.respawnPoint);

        // If moved significantly in one frame AND landed exactly at respawn point
        if (distMoved > TELEPORT_DIST && distToRespawn <= RESPAWN_EPSILON)
        {
            ResetToOriginal();
        }

        lastPos = currentPos;
    }

    private void CachePlayerComponents()
    {
        if (player1)
        {
            p1Collider = player1.GetComponent<Collider2D>();
            p1Stats = player1.GetComponent<PlayerStats>();
            p1LastPos = player1.transform.position;
        }

        if (player2)
        {
            p2Collider = player2.GetComponent<Collider2D>();
            p2Stats = player2.GetComponent<PlayerStats>();
            p2LastPos = player2.transform.position;
        }
    }

    public override void ToggleActive(bool state)
    {
        gameObject.SetActive(state);
    }
}