using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GlobalFader : MonoBehaviour
{
    public static GlobalFader Instance;

    [Header("UI Cấu hình")]
    [Tooltip("Kéo Component CanvasGroup của màn hình đen vào đây")]
    public CanvasGroup manHinhDen;
    public float thoiGianFade = 0.5f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (manHinhDen != null)
        {
            manHinhDen.alpha = 0;
            manHinhDen.blocksRaycasts = false;
        }
    }
    
    public IEnumerator ToiDan()
    {
        if (manHinhDen != null)
        {
            manHinhDen.blocksRaycasts = true;
            float timer = 0;
            while (timer < thoiGianFade)
            {
                timer += Time.unscaledDeltaTime; 
                manHinhDen.alpha = timer / thoiGianFade;
                yield return null;
            }
            manHinhDen.alpha = 1;
        }
    }
    
    public IEnumerator SangDan()
    {
        if (manHinhDen != null)
        {
            float timer = 0;
            while (timer < thoiGianFade)
            {
                timer += Time.unscaledDeltaTime;
                manHinhDen.alpha = 1 - (timer / thoiGianFade);
                yield return null;
            }
            manHinhDen.alpha = 0;
            manHinhDen.blocksRaycasts = false;
        }
    }
    
    public void ChuyenMap(string tenMapMoi)
    {
        StartCoroutine(HieuUngChuyenMap(tenMapMoi));
    }

    private IEnumerator HieuUngChuyenMap(string tenMapMoi)
    {
        yield return StartCoroutine(ToiDan());
        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(tenMapMoi);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        yield return null; 
        yield return null;
        yield return null;
        yield return null; 
        
        yield return new WaitForSecondsRealtime(1.0f); 
        
        yield return StartCoroutine(SangDan());
    }
}