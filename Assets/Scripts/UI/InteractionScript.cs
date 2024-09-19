using UnityEngine;
using UnityEngine.UI;
using System;

public class InteractionScript : MonoBehaviour
{
    public Text Text;
    public UnityEngine.UI.Image Img;
    public GameObject TextPanel;
    public Button InteractionBtn;

    bool isButton = false;
    private Action cb;

    private void OnBtnClick()
    {
        if (cb != null)
        {
            cb();
        }
    }

    void Start()
    {
        InteractionBtn.onClick.AddListener(OnBtnClick);
    }

    // Update is called once per frame
    void Update()
    {
        Constants.PilotStateMap pilotState = StateManager.Instance.pilotState;
        InteractionInfo info = Constants.InteractionInfo[pilotState];
        string label = info.label;
        bool newIsButton = info.cb != null;

        // 如果不是按钮的
        if (!newIsButton)
        {
            Text.text = label;
            if (!string.IsNullOrEmpty(info.icon))
            {
                // 假设你已经有一个 Resources 文件夹，并且图标保存在其中
                Sprite newSprite = Resources.Load<Sprite>($"Images/{info.icon}"); // 加载 Sprite
                if (newSprite != null)
                {
                    Img.sprite = newSprite; // 设置 Img 的 source image
                }
                else
                {
                    newSprite = Resources.Load<Sprite>("Images/wheel");
                    Debug.LogWarning("无法找到图标: " + info.icon);
                    Img.sprite = newSprite;
                }
            }
        } else
        {
            cb = info.cb;
            Text buttonText = InteractionBtn.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = label;  // 设置按钮的文本
            }
        }

        if (newIsButton != isButton)
        {
            TextPanel.SetActive(!newIsButton);
            InteractionBtn.gameObject.SetActive(newIsButton);
            isButton = newIsButton;
        }
    }
}
