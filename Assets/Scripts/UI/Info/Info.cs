using UnityEngine;
using System;
using UnityEngine.UIElements;

public class InteractionScript2 : MonoBehaviour
{
    public UIDocument uiDocument;

    bool isButton = false;
    private Action cb;
    private Button InteractionBtn;

    private void OnBtnClick()
    {
        if (cb != null)
        {
            cb();
        }
    }

    void Start()
    {
        InteractionBtn = uiDocument.rootVisualElement.Q<Button>("Button");
        InteractionBtn.clicked += OnBtnClick;
    }

    void UpdateParkDis()
    {
        float es = StateManager.Instance.carInfo.es;
        VisualElement disContainer = uiDocument.rootVisualElement.Q<VisualElement>("DisInfo");
        if (es == 0)
        {
            disContainer.style.display = DisplayStyle.None;
        } else
        {
            disContainer.style.display = DisplayStyle.Flex;
        }
        uint gear = StateManager.Instance.gear;
        Constants.GearTypes gearStr = (Constants.GearTypes)gear;
        Label gearElement = uiDocument.rootVisualElement.Q<Label>("Gear");
        gearElement.text = gearStr.ToString();
        string forward = "";
        if (gearStr == Constants.GearTypes.D)
        {
            forward = "前";
        } else if (gearStr == Constants.GearTypes.R)
        {
            forward = "后";
        }
        string disStr = "继续向" + forward + es + "米";

        Label disElement = uiDocument.rootVisualElement.Q<Label>("Dis");
        disElement.text = disStr;
    }

    void UpdateInfo()
    {

        Constants.PilotStateMap pilotState = StateManager.Instance.pilotState;
        InteractionInfo info = Constants.InteractionInfo[pilotState];
        string label = info.label;
        bool newIsButton = info.cb != null;
        Label labelElement = uiDocument.rootVisualElement.Q<Label>("Label");
        VisualElement visualElement = uiDocument.rootVisualElement.Q<VisualElement>("VisualElement");
        GroupBox container = uiDocument.rootVisualElement.Q<GroupBox>("Container");

        // 如果不是按钮的
        if (!newIsButton)
        {
            labelElement.text = label;
            if (label.Length == 6)
            {
                labelElement.style.fontSize = 24; // 设置字体大小为 24px
            }
            else if (label.Length < 6)
            {
                labelElement.style.fontSize = 28; // 设置字体大小为 28px
            }

            if (!string.IsNullOrEmpty(info.icon))
            {
                // 加载图标
                Sprite newSprite = Resources.Load<Sprite>($"Images/{info.icon}");
                if (newSprite == null)
                {
                    newSprite = Resources.Load<Sprite>("Images/wheel"); // 默认图标
                    Debug.LogWarning("无法找到图标: " + info.icon);
                }

                // 将 Sprite 转换为 Texture2D 并设置为背景
                if (newSprite != null && newSprite.texture != null)
                {
                    visualElement.style.backgroundImage = new StyleBackground(newSprite.texture);
                }
            }
        }
        else
        {
            // 设置按钮文本
            InteractionBtn.text = label;
            cb = info.cb;
        }

        // 根据 newIsButton 显示或隐藏组件
        if (newIsButton != isButton)
        {
            container.style.display = newIsButton ? DisplayStyle.None : DisplayStyle.Flex; // 显示或隐藏 container
            InteractionBtn.style.display = newIsButton ? DisplayStyle.Flex : DisplayStyle.None; // 显示或隐藏按钮
            isButton = newIsButton;
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateInfo();
        UpdateParkDis();
    }
}
