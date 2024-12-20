using System;
using Google.Protobuf.Collections;
using Xviewer;
using System.Linq;
using UnityEngine;

public static class WorldUtils
{
    public static (
        RepeatedField<LaneLine> laneLines,
        RepeatedField<RepeatedField<LaneLine>> otherLines,
        RepeatedField<Lane> lanes,
        Lane currentLane,
        RepeatedField<StopLine> stopLines
    ) GetLaneLines(SimulationWorld world)
    {
        RepeatedField<LaneLine> laneLines = new RepeatedField<LaneLine>();
        RepeatedField<RepeatedField<LaneLine>> otherLines = new RepeatedField<RepeatedField<LaneLine>>();
        RepeatedField<Lane> lanes = new RepeatedField<Lane>();
        int egoLaneId = -1;
        Lane currentLane = new Lane();
        RepeatedField<StopLine> stopLines = new RepeatedField<StopLine>();

        try
        {
            int egoRoadId = -1;
            RepeatedField<Road> roads = new RepeatedField<Road>();
            egoRoadId = world.Perception.LaneLineRoadInfo.EgoRoadId;
            roads = world.Perception.LaneLineRoadInfo.Roads;

            foreach (Road road in roads)
            {
                if (road.RoadId == egoRoadId)
                {
                    laneLines = road.LaneLines;
                    egoLaneId = road.EgoLaneId;
                    lanes = road.Lanes;
                    stopLines = road.StopLines;
                }
                else
                {
                    otherLines.Add(road.LaneLines);
                }
            }

            currentLane = lanes.FirstOrDefault(lane => lane.LaneId == egoLaneId);

            return (laneLines, otherLines, lanes, currentLane, stopLines);
        }
        catch (Exception e)
        {
            return (laneLines, otherLines, lanes, currentLane, stopLines);
            throw new Exception("error", e);
        }
    }

    public static TrajectoryStamped GetGuideLine(SimulationWorld world)
    {
        return world?.Planning?.TrajPoints ?? new TrajectoryStamped();
    }

    public static RepeatedField<ParkingSpace> GetSlots(SimulationWorld world)
    {

        return world?.Perception?.ParkingSlotsAll?.ParkingSlots_ ?? new RepeatedField<ParkingSpace>();
    }

    public static (TrajectoryPoint center, float yaw) GetWorldCenter(SimulationWorld world)
    {
        double x = world.AutoDrivingCar.PositionX;
        double y = world.AutoDrivingCar.PositionY;
        TrajectoryPoint center = new TrajectoryPoint();
        center.X = x;
        center.Y = y;
        float yaw = (float)world.AutoDrivingCar.Heading; // 假设 yaw 是 world.AutoDrivingCar 中的一个属性

        return (center, yaw);
    }

    public static RepeatedField<Object3D> GetObstacleList(SimulationWorld world)
    {
        RepeatedField<Object3D> ObstacleList = world?.Perception?.FusionPilot?.ObjectInfo?.ObjectList ?? new RepeatedField<Object3D>();
        return ObstacleList;
    }

    public static RepeatedField<TrackBox> GetApaObstacleList(SimulationWorld world)
    {
        RepeatedField<TrackBox> ObstacleList = world?.Perception?.ApaPerceptionObject?.TrackBoxList_ ?? new RepeatedField<TrackBox>();
        return ObstacleList;
    }

    public static RepeatedField<Point> GetFreeSpace(SimulationWorld world)
    {
        RepeatedField<Point> spacePoints = world?.Perception?.FreeSpace;
        return spacePoints ?? new RepeatedField<Point>();
    }

    public static uint GetGear(SimulationWorld world)
    {
        uint gear = world?.Engineering?.Chassis?.VcuCrntGearLvlG ?? 0;
        return gear;
    }

    public static int GetSpeed(SimulationWorld world)
    {
        double speed = world?.Engineering?.Chassis?.BcsVehSpdG * 1.13f ?? 0;
        return (int)Math.Floor(speed);
    }

    public static int GetLockSlotId(SimulationWorld world)
    {
        return world?.Planning?.LockCarportId ?? -1;
    }

    public static int GetState(SimulationWorld world)
    {
        return world?.Planning?.State ?? 0;
    }

    public static int MakePilotState(int state)
    {
        return 1;
    }

    public static int GetApaPlanState(SimulationWorld world)
    {
        return world?.Planning?.ApaPlanState?.PlanState ?? 0;
    }

    public static bool IsInPilot(SimulationWorld world)
    {
        return world?.Engineering?.Chassis?.ScuLatCtrlMode == 2 && world.Engineering.Chassis.ScuLngCtrlMode == 2;
    }

    public static CarInfo GetCarInfo(SimulationWorld world)
    {
        CarInfo info = new CarInfo();
        if (world?.AutoDrivingCar?.PositionX != null && world?.AutoDrivingCar?.PositionY != null)
        {
            float x = (float)world.AutoDrivingCar.PositionX;
            float y = (float)world.AutoDrivingCar.PositionY;
            Vector3 p = new Vector3(x, y, 0);
            info.position = p;
        }
        if (world?.AutoDrivingCar?.Heading != null)
        {
            float heading = (float)world.AutoDrivingCar.Heading;
            info.heading = heading;
        }
        if (world?.Controling?.ControlVisual?.ES != null)
        {
            double es = world.Controling.ControlVisual.ES;
            info.es = (float)Math.Round(es, 1);
        } else
        {
            info.es = 0;
        }
        return info;
    }

    public static RepeatedField<Avaliableslot> GetValidSlots(SimulationWorld world)
    {
        return world?.Planning?.ApaPlanState?.AvaliableSlots ?? new RepeatedField<Avaliableslot>();
    }

    public static uint GetValidSlotDir(SimulationWorld world)
    {
        RepeatedField<Avaliableslot> validSlots = GetValidSlots(world);
        RepeatedField<ParkingSpace> slots = GetSlots(world);
        var targetSlot = slots.FirstOrDefault(slot => slot.Src == 4);
        //Debug.Log("targetSlot=====" + targetSlot);
        //Debug.Log("validSlots=====" + validSlots);
        if (targetSlot != null)
        {
            long id = targetSlot.Id;
            var customSlot = validSlots.FirstOrDefault(validSlot => (long)validSlot.ParkingSlotId == id);
            if (customSlot != null)
            {
                return customSlot.ParkingDir;
            }
        }
        return 0;
    }
}
