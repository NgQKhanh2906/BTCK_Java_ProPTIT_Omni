using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Cinemachine;

public class CamController : MonoBehaviour
{
    public static CamController Instance;

    [Header("Kéo các Object Cinemachine vào đây")]
    public CinemachineVirtualCamera vcam;
    public CinemachineTargetGroup targetGroup;
    public CinemachineConfiner2D confiner;

    [Header("Zoom Settings")]
    public float minCamSize = 5f;
    public float maxCamSize = 9f;

    [Header("Player & Teleport Settings")]
    public List<Transform> players = new List<Transform>();
    public float timeToTeleport = 1.2f;
    public GameObject teleportVFXPrefab;

    private float stuckTimer = 0f;
    private Camera mainCam;
    private Transform p1Ref;
    private Transform p2Ref;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
        
        mainCam = Camera.main;
    }

    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        players.Clear();
        GameObject p1 = GameObject.FindGameObjectWithTag("Player1");
        GameObject p2 = GameObject.FindGameObjectWithTag("Player2");
        
        p1Ref = p1 != null ? p1.transform : null;
        p2Ref = p2 != null ? p2.transform : null;

        if (p1Ref != null) players.Add(p1Ref);
        if (p2Ref != null) players.Add(p2Ref);

        UpdateTargetGroup();
    }

    void UpdateTargetGroup()
    {
        if (targetGroup == null) return;
        
        var targets = targetGroup.m_Targets;
        for (int i = targets.Length - 1; i >= 0; i--) targetGroup.RemoveMember(targets[i].target);
        
        foreach (var p in players) 
        {
            if (p != null) targetGroup.AddMember(p, 1f, 1.5f);
        }
    }

    void LateUpdate()
    {
        players.Clear();
        
        if (p1Ref != null && p1Ref.gameObject.activeInHierarchy)
        {
            PlayerBase pb1 = p1Ref.GetComponent<PlayerBase>();
            if (pb1 != null && !pb1.IsDead()) players.Add(p1Ref);
        }
        
        if (p2Ref != null && p2Ref.gameObject.activeInHierarchy)
        {
            PlayerBase pb2 = p2Ref.GetComponent<PlayerBase>();
            if (pb2 != null && !pb2.IsDead()) players.Add(p2Ref);
        }

        if (players.Count == 0) return;

        if (targetGroup != null && targetGroup.m_Targets.Length != players.Count)
        {
            UpdateTargetGroup();
        }
        UpdateZoomLogic(); 
        CheckAndTeleport();
    }
    
    void UpdateZoomLogic()
    {
        if (vcam == null || mainCam == null) return;

        float targetSize = maxCamSize;

        if (confiner != null && confiner.m_BoundingShape2D != null)
        {
            Collider2D limit = confiner.m_BoundingShape2D;
            float boundsHeight = limit.bounds.size.y;
            float boundsWidth = limit.bounds.size.x;
            
            float maxOrthoHeight = boundsHeight / 2f;
            float maxOrthoWidth = (boundsWidth / mainCam.aspect) / 2f;
            
            float fittingSize = Mathf.Min(maxOrthoHeight, maxOrthoWidth);
            targetSize = Mathf.Clamp(fittingSize, minCamSize, maxCamSize);
        }
        
        float oldSize = vcam.m_Lens.OrthographicSize;
        float newSize = Mathf.Lerp(oldSize, targetSize, Time.deltaTime * 5f);
        
        vcam.m_Lens.OrthographicSize = newSize;
        
        if (Mathf.Abs(oldSize - newSize) > 0.01f && confiner != null)
        {
            confiner.InvalidateCache();
        }
    }

    void CheckAndTeleport()
    {
        if (players.Count <= 1 || mainCam == null) { stuckTimer = 0f; return; }

        float currentHeight = mainCam.orthographicSize * 2f;
        float currentWidth = currentHeight * mainCam.aspect;
        float camX = mainCam.transform.position.x;
        float camY = mainCam.transform.position.y;

        bool someoneIsStuck = false;

        foreach (var p in players)
        {
            float distX = Mathf.Abs(p.position.x - camX);
            float distY = p.position.y - camY;

            bool touchLeftRight = distX >= (currentWidth / 2f) - 1.5f;
            bool touchBottom = distY <= -(currentHeight / 2f) + 1.5f;
            bool touchTop = distY >= (currentHeight / 2f) - 1.5f;

            if (touchLeftRight || touchBottom || touchTop)
            {
                if (GetGreatestDistance() > 5f) 
                {
                    someoneIsStuck = true;
                    break;
                }
            }
        }

        if (someoneIsStuck)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer >= timeToTeleport) { ExecuteTeleport(); stuckTimer = 0f; }
        }
        else { stuckTimer = 0f; }
    }

    void ExecuteTeleport()
    {
        Transform safePlayer = players[0];
        foreach (var p in players) if (p.position.y > safePlayer.position.y) safePlayer = p;

        foreach (var p in players)
        {
            if (p != safePlayer)
            {
                if (teleportVFXPrefab != null) Instantiate(teleportVFXPrefab, p.position, Quaternion.identity);
                p.position = safePlayer.position;
                if (teleportVFXPrefab != null) Instantiate(teleportVFXPrefab, p.position, Quaternion.identity);

                Rigidbody2D rb = p.GetComponent<Rigidbody2D>();
                if (rb != null) rb.velocity = Vector2.zero;
            }
        }
        SnapCamera(safePlayer.position);
    }

    float GetGreatestDistance()
    {
        if (players.Count <= 1) return 0f;
        var bounds = new Bounds(players[0].position, Vector3.zero);
        foreach (var p in players) bounds.Encapsulate(p.position);
        return Mathf.Max(bounds.size.x / mainCam.aspect, bounds.size.y);
    }

    public void ChangeLimit(Collider2D newLimit)
    {
        if (confiner != null)
        {
            confiner.m_BoundingShape2D = newLimit;
            if (mainCam != null)
            {
                float boundsHeight = newLimit.bounds.size.y;
                float boundsWidth = newLimit.bounds.size.x;
                
                float maxOrthoHeight = boundsHeight / 2f;
                float maxOrthoWidth = (boundsWidth / mainCam.aspect) / 2f;
                
                float fittingSize = Mathf.Min(maxOrthoHeight, maxOrthoWidth);
                float targetSize = Mathf.Clamp(fittingSize, minCamSize, maxCamSize);
                vcam.m_Lens.OrthographicSize = targetSize;
            }

            confiner.InvalidateCache(); 
        }
    }

    public void SnapCamera(Vector3 targetPosition)
    {
        if (vcam != null)
        {
            vcam.ForceCameraPosition(new Vector3(targetPosition.x, targetPosition.y, vcam.transform.position.z), vcam.transform.rotation);
        }
    }
}