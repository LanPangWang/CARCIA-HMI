using System;
using UnityEngine;
using Xviewer;
using TMPro;

public class DriveInfoScript : MonoBehaviour
{
    private SimulationWorld world;
    public TextMeshProUGUI GearObject;
    public TextMeshProUGUI SpeedObject;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        updateGear();
        updateSpeed();
    }

    void updateSpeed()
    {
        world = WebSocketNet.Instance.world;
        int speed = WorldUtils.GetSpeed(world);
        SpeedObject.text = speed.ToString();
    }

    void updateGear()
    {
        world = WebSocketNet.Instance.world;
        uint gear = WorldUtils.GetGear(world);
        string txt = MakeGearText(gear);
        GearObject.text = txt;
    }

    string MakeGearText(uint gear)
    {
        string color = "#FF0000";
        string txt = "";
        foreach (Constants.GearTypes type in Enum.GetValues(typeof(Constants.GearTypes)))
        {
            int TypeIndex = (int)type;
            if (gear == TypeIndex)
            {
                txt += $"<color={color}>" + type + "</color>";
            } else
            {
                txt += type;
            }
            if (TypeIndex != 4)
            {
                txt += "  ";
            }
        }
        return txt;
    }
}