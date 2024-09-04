using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    public static StateManager Instance { get; private set; }

    public bool AvmOpen { get; private set; }

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

    public void SetAvmOpen(bool open)
    {
        AvmOpen = open;
        // 处理暂停状态的逻辑，例如暂停游戏，显示暂停菜单等
    }
}
