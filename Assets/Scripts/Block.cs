using UnityEngine;
using UnityEngine.UI;
using Util;

public class Block : MonoBehaviour
{
    private Button button;

    private Pos pos;

    public void Init(Pos pos)
    {
        this.pos = pos;
        button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            PangManager.Instance.SelectObject(pos);
        });
    }
}
