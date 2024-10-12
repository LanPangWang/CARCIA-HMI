using UnityEngine;
using UnityEngine.UI; // 引入UI命名空间using System;
using System.Collections.Generic;

public class CustomSlot : MonoBehaviour
{
    public Button entryBtn;
    public Button exitBtn;
    private AvmCameraScript avmScript;

    private void Awake()
    {
        Camera avmCamera = GameObject.Find("AvmCamera").GetComponent<Camera>();
        avmScript = avmCamera.GetComponent<AvmCameraScript>();
        entryBtn.onClick.AddListener(OnEntryCustomSlot);
        exitBtn.onClick.AddListener(OnExitCustomSlot);
    }

    // Update is called once per frame
    void Update()
    {
        int speed = StateManager.Instance.speed;
        bool AvmOpen = StateManager.Instance.AvmOpen;
        Constants.PilotStateMap pilotState = StateManager.Instance.pilotState;
        //if (AvmOpen) // 如果打开了avm 则只显示关闭
        //{
        //    entryBtn.gameObject.SetActive(false);
        //    exitBtn.gameObject.SetActive(true);
        //}
        //else
        //{
        //    entryBtn.gameObject.SetActive(true);
        //    exitBtn.gameObject.SetActive(false);
        //}
        if (speed > 25) // 速度大于25 都不显示
        {
            entryBtn.gameObject.SetActive(false);
            exitBtn.gameObject.SetActive(false);
        }
        else if (AvmOpen) // 如果打开了avm 则只显示关闭
        {
            entryBtn.gameObject.SetActive(false);
            exitBtn.gameObject.SetActive(true);
        }
        else
        {
            entryBtn.gameObject.SetActive(true);
            exitBtn.gameObject.SetActive(false);
        }

        //bool shouldShow = speed <= 25 && !AvmOpen && pilotState == 0;
        //if (shouldShow != show)
        //{
        //    entryBtn.gameObject.SetActive(shouldShow);
        //    show = shouldShow;
        //    exitBtn.gameObject.SetActive(AvmOpen);
        //}
    }

    async void OnEntryCustomSlot()
    {
        StateManager.Instance.SetAvmOpen(true);
        uint frameId = StateManager.Instance.GetFrameId();
        await HmiSocket.Instance.EntryCustomSlot(frameId);
        Vector3[] vertices = avmScript.GetRectangleVertices(Constants.CustomSlotCenter);
        List<string> points = Utils.GetCustomSlotPoints(vertices);
        frameId = StateManager.Instance.GetFrameId();
        await HmiSocket.Instance.LockCustomSlot(points, frameId);
    }

    async void OnExitCustomSlot()
    {
        StateManager.Instance.SetAvmOpen(false);
        uint frameId = StateManager.Instance.GetFrameId();
        await HmiSocket.Instance.ExitCustomSlot(frameId);
    }
}
