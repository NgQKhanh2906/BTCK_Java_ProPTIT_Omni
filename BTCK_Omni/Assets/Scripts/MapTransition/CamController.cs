using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Camera))]
public class CamController : MonoBehaviour
{
    public static CamController Instance;

    [Header("Player Settings")]
    public List<Transform> players = new List<Transform>();

    [Header("Zoom Settings (Luôn to hết cỡ)")]
    public float minCamSize = 5f;
    public float maxCamSize = 10f;
    public float smoothTime = 0.2f;

    [Header("Wall Margins (Chống lọt mép)")]
    public float topWallMargin = 1.5f;
    public float generalWallMargin = 0.5f;

    [Header("Teleport Settings (Dịch chuyển khi kẹt)")]
    [Tooltip("Thời gian (giây) người chơi bị kẹt ở mép tường trước khi bị dịch chuyển")]
    public float timeToTeleport = 1.2f;
    private float stuckTimer = 0f;
    public GameObject teleportVFXPrefab;

    [Header("Boundaries")]
    public Collider2D camLimit;
    
    private Camera cam;
    private Vector3 velocity;
    private BoxCollider2D topWall, bottomWall, leftWall, rightWall;
    
    private Transform p1Ref;
    private Transform p2Ref;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        cam = GetComponent<Camera>();
        SetupCameraWalls();
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
        if (p1Ref != null) 
        {
            SnapCamera(p1Ref.position);
        }
        else if (p2Ref != null)
        {
            SnapCamera(p2Ref.position);
        }
    }

    void LateUpdate()
    {
        players.Clear();
        if (p1Ref != null && p1Ref.gameObject.activeInHierarchy)
        {
            PlayerBase pb1 = p1Ref.GetComponent<PlayerBase>();
            if (pb1 != null && !pb1.IsDead())
            {
                players.Add(p1Ref);
            }
        }
        if (p2Ref != null && p2Ref.gameObject.activeInHierarchy)
        {
            PlayerBase pb2 = p2Ref.GetComponent<PlayerBase>();
            if (pb2 != null && !pb2.IsDead())
            {
                players.Add(p2Ref);
            }
        }
        if (players.Count == 0) return;
        UpdateCameraLogic();
        CheckAndTeleport();
    }

    void UpdateCameraLogic()
    {
        float targetSize = maxCamSize;

        if (camLimit != null)
        {
            float boundsHeight = camLimit.bounds.size.y;
            float boundsWidth = camLimit.bounds.size.x;
            
            float maxOrthoHeight = boundsHeight / 2f;
            float maxOrthoWidth = (boundsWidth / cam.aspect) / 2f;
            
            float fittingSize = Mathf.Min(maxOrthoHeight, maxOrthoWidth);
            targetSize = Mathf.Clamp(fittingSize, minCamSize, maxCamSize);
        }
        
        float finalSize = Mathf.Lerp(cam.orthographicSize, targetSize, Time.deltaTime * 5f);
        cam.orthographicSize = finalSize;

        Vector3 centerPoint = GetCenterPoint();
        Vector3 targetPos = new Vector3(centerPoint.x, centerPoint.y, transform.position.z);
        if (camLimit != null)
        {
            float camH = finalSize;
            float camW = finalSize * cam.aspect;
            float minX = camLimit.bounds.min.x + camW;
            float maxX = camLimit.bounds.max.x - camW;
            float minY = camLimit.bounds.min.y + camH;
            float maxY = camLimit.bounds.max.y - camH;

            if (maxX < minX) minX = maxX = camLimit.bounds.center.x;
            if (maxY < minY) minY = maxY = camLimit.bounds.center.y;

            targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
            targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);
        }

        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
        
        float height = finalSize * 2;
        float width = height * cam.aspect;
        float thickness = 2f;

        topWall.size = new Vector2(width, thickness);
        topWall.transform.position = new Vector3(targetPos.x, targetPos.y + finalSize - topWallMargin + thickness / 2, 0);

        bottomWall.size = new Vector2(width, thickness);
        bottomWall.transform.position = new Vector3(targetPos.x, targetPos.y - finalSize + generalWallMargin - thickness / 2, 0);

        leftWall.size = new Vector2(thickness, height);
        leftWall.transform.position = new Vector3(targetPos.x - width / 2 + generalWallMargin - thickness / 2, targetPos.y, 0);

        rightWall.size = new Vector2(thickness, height);
        rightWall.transform.position = new Vector3(targetPos.x + width / 2 - generalWallMargin + thickness / 2, targetPos.y, 0);
    }
    void CheckAndTeleport()
    {
        if (players.Count <= 1) { stuckTimer = 0f; return; }

        float currentHeight = cam.orthographicSize * 2f;
        float currentWidth = currentHeight * cam.aspect;
        float camX = transform.position.x;
        float camY = transform.position.y;

        bool someoneIsStuck = false;

        foreach (var p in players)
        {
            float distX = Mathf.Abs(p.position.x - camX);
            float distY = p.position.y - camY; // Giữ nguyên dấu để phân biệt trên/dưới
            bool touchLeftRight = distX >= (currentWidth / 2f) - generalWallMargin - 1.5f;
            bool touchBottom = distY <= -(currentHeight / 2f) + generalWallMargin + 1.5f;
            bool touchTop = distY >= (currentHeight / 2f) - topWallMargin - 1.5f;

            if (touchLeftRight || touchBottom || touchTop)
            {
                float distToOthers = GetGreatestDistance();
                if (distToOthers > minCamSize) 
                {
                    someoneIsStuck = true;
                    break;
                }
            }
        }

        if (someoneIsStuck)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer >= timeToTeleport)
            {
                ExecuteTeleport();
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f; 
        }
    }

    float GetGreatestDistance() 
    {
        if (players.Count <= 1) return 0f;
        
        var bounds = new Bounds(players[0].position, Vector3.zero);
        foreach (var p in players) 
        {
            bounds.Encapsulate(p.position);
        }
        return Mathf.Max(bounds.size.x / cam.aspect, bounds.size.y); 
    }
    void ExecuteTeleport()
    {
        Transform safePlayer = players[0];
        foreach (var p in players)
        {
            if (p.position.y > safePlayer.position.y) safePlayer = p;
        }
        
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
        Vector3 snapPos = new Vector3(safePlayer.position.x, safePlayer.position.y, transform.position.z);
        SnapCamera(snapPos);
    }
    
    Vector3 GetCenterPoint() {
        if (players.Count == 1) return players[0].position;
        var bounds = new Bounds(players[0].position, Vector3.zero);
        foreach (var p in players) bounds.Encapsulate(p.position);
        return bounds.center;
    }

    void SetupCameraWalls() {
        topWall = CreateWall("TopWall");
        bottomWall = CreateWall("BottomWall");
        leftWall = CreateWall("LeftWall");
        rightWall = CreateWall("RightWall");
    }

    BoxCollider2D CreateWall(string name) {
        GameObject wall = new GameObject(name);
        wall.transform.parent = transform;
        BoxCollider2D col = wall.AddComponent<BoxCollider2D>();
        col.sharedMaterial = new PhysicsMaterial2D { friction = 0, bounciness = 0 };
        return col;
    }

    public void ChangeLimit(Collider2D newLimit) { camLimit = newLimit; }
    public void SnapCamera(Vector3 targetPosition) { transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z); velocity = Vector3.zero; }
}