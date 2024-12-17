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
            Sprite newSprite = Resources.Load<Sprite>($"Images/wheel");
            if (!string.IsNullOrEmpty(info.icon))
            {
                newSprite = Resources.Load<Sprite>($"Images/{info.icon}");
            }
            Img.sprite = newSprite;
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