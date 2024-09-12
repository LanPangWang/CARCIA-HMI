using UnityEngine;
using Xviewer;
using UnityEngine.UI; // 引入UI命名空间using System;
using System.Collections.Generic;

public class CustomSlotBtn : MonoBehaviour
{
    public Button entryBtn;
    public Button exitBtn;

    private SimulationWorld world;
    private bool show = false;

    private void Awake()
    {
        entryBtn.onClick.AddListener(OnEntryCustomSlot);
        exitBtn.onClick.AddListener(OnExitCustomSlot);
    }

    // Update is called once per frame
    void Update()
    {
        world = WebSocketNet.Instance.world;
        int speed = WorldUtils.GetSpeed(world);
        bool AvmOpen = StateManager.Instance.AvmOpen;
        bool shouldShow = speed <= 25 && !AvmOpen;

        if (shouldShow != show)
        {
            entryBtn.gameObject.SetActive(shouldShow);
            show = shouldShow;
            if (AvmOpen)
            {
                exitBtn.gameObject.SetActive(true);
            } else
            {
                exitBtn.gameObject.SetActive(false);
            }
        }
    }

    async void OnEntryCustomSlot()
    {
        StateManager.Instance.SetAvmOpen(true);
        uint frameId = StateManager.Instance.GetFrameId();

        var paramsDict = new Dictionary<string, object>
        {
            { "type", "HMIKeyDownEnvent" },
            { "hmi", new Dictionary<string, object>
                {
                    { "into_custom_parking", 1 },
                    { "custom_parking_frameId", frameId },
                }
            }
        };

        await HmiSocket.Instance.Send(paramsDict);
    }

    async void OnExitCustomSlot()
    {
        StateManager.Instance.SetAvmOpen(false);
        uint frameId = StateManager.Instance.GetFrameId();

        var paramsDict = new Dictionary<string, object>
        {
            { "type", "HMIKeyDownEnvent" },
            { "hmi", new Dictionary<string, object>
                {
                    { "into_custom_parking", 0 },
                    { "custom_parking_frameId", frameId },
                }
            }
        };

        await HmiSocket.Instance.Send(paramsDict);
    }
}
