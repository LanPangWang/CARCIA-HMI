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
        return world.Planning?.TrajPoints ?? new TrajectoryStamped();
    }

    public static RepeatedField<ParkingSpace> GetSlots(SimulationWorld world)
    {

        return world.Perception?.ParkingSlotsAll?.ParkingSlots_ ?? new RepeatedField<ParkingSpace>();
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
        RepeatedField<Object3D> ObstacleList = world?.Perception?.FusionPilot?.ObjectInfo.ObjectList ?? new RepeatedField<Object3D>();
        return ObstacleList;
    }

    public static RepeatedField<Point> GetFreeSpace(SimulationWorld world)
    {
        RepeatedField<Point> spacePoints = world?.Perception?.FreeSpace;
        return spacePoints ?? new RepeatedField<Point>();
    }
}
