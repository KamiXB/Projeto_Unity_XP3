using System.Collections;
using UnityEngine;
using UnityEngine.UI;
#if TMP_PRESENT
using TMPro;
#endif

public class PickupUI : MonoBehaviour
{
    public static PickupUI Instance;

#if TMP_PRESENT
    [SerializeField] private TextMeshProUGUI tmpText;
#else
    [SerializeField] private Component tmpText; // fallback if TMP symbol not defined
#endif
    [SerializeField] private Text uiText;
    [SerializeField] private float defaultDuration = 2f;
    [SerializeField] private float fadeOutTime = 0.5f;

    private Coroutine hideCoroutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        DontDestroyOnLoad(gameObject);

#if TMP_PRESENT
        if (tmpText == null) tmpText = FindObjectOfType<TextMeshProUGUI>();
#else
        if (tmpText == null)
        {
            // try to find TMP without compile symbol
            var ttype = System.Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
            if (ttype != null)
            {
                var found = Object.FindObjectOfType(ttype);
                if (found != null) tmpText = found as Component;
            }
        }
#endif
        if (uiText == null) uiText = FindObjectOfType<Text>();

        if (GetActiveTextComponent() != null)
        {
            GetActiveTextComponent().gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("PickupUI: no TMP or UI.Text found in scene. Assign one in the inspector.");
        }
    }

    private Component GetActiveTextComponent()
    {
#if TMP_PRESENT
        if (tmpText != null) return tmpText;
#else
        if (tmpText != null) return tmpText as Component;
#endif
        if (uiText != null) return uiText as Component;
        return null;
    }

    public void ShowMessage(string message, float duration = -1f)
    {
        var comp = GetActiveTextComponent();
        if (comp == null) return;
        if (duration <= 0f) duration = defaultDuration;

        if (hideCoroutine != null) StopCoroutine(hideCoroutine);

#if TMP_PRESENT
        if (tmpText != null)
        {
            tmpText.text = message;
            tmpText.gameObject.SetActive(true);
            hideCoroutine = StartCoroutine(HideAfter(tmpText.gameObject, duration));
            return;
        }
#else
        // if TMP not present or not assigned fall through
#endif

        if (uiText != null)
        {
            uiText.text = message;
            uiText.gameObject.SetActive(true);
            hideCoroutine = StartCoroutine(HideAfter(uiText.gameObject, duration));
            return;
        }
    }

    private IEnumerator HideAfter(GameObject go, float duration)
    {
        yield return new WaitForSeconds(duration);

        float t = 0f;
        var graphic = go.GetComponent<Graphic>();
        Color orig = graphic != null ? graphic.color : Color.white;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, t / fadeOutTime);
            if (graphic != null)
            {
                var c = graphic.color;
                c.a = a * orig.a;
                graphic.color = c;
            }
            yield return null;
        }

        go.SetActive(false);
        if (graphic != null) graphic.color = orig;
        hideCoroutine = null;
    }
}
