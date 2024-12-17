using UnityEngine;
using System;
using UnityEngine.UIElements;

public class InteractionScript2 : MonoBehaviour
{
    public UIDocument uiDocument;
    private Action cb;
    private Button InteractionBtn;
    private bool isButton = false;

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
        }
        else
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
        }
        else if (gearStr == Constants.GearTypes.R)
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
                labelElement.style.fontSize = 12; // 字体大小为 12px
            }
            else if (label.Length < 6)
            {
                labelElement.style.fontSize = 14; // 字体大小为 14px
            }

            Sprite newSprite = Resources.Load<Sprite>($"Images/wheel");
            if (!string.IsNullOrEmpty(info.icon))
            {
                newSprite = Resources.Load<Sprite>($"Images/{info.icon}");
            }
            visualElement.style.backgroundImage = new StyleBackground(newSprite.texture);
        }
        else
        {
            // 设置按钮文本和字体
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

    void Update()
    {
        UpdateInfo();
        UpdateParkDis();
    }
}
