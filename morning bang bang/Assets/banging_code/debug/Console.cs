using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class Console : MonoBehaviour
{
    public static Console Instance;
    private TMP_Text text;

    private string current;
    private string last;

    private void Awake()
    {
        if(Instance != null) Destroy(gameObject);
        
        DontDestroyOnLoad(gameObject.transform.parent);
        text = GetComponent<TMP_Text>();
        Instance = this;
    }

    public void PushMessage(string message)
    {
        last = message;

        current += "\n >" + message;
    }

    public void Clear()
    {
        current = "";
        last = "";
    }

    private void Update()
    {
        text.text = current;
        
    }
}
