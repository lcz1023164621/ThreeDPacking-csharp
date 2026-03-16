using System;
using System.Collections.Generic;

namespace ThreeDPacking.Unity
{
    /// <summary>
    /// 装箱结果数据模型 - 用于Unity导入
    /// 与ResultSerializer.cs导出的JSON格式匹配
    /// </summary>
    [Serializable]
    public class PackingResult
    {
        public List<ContainerData> containers;
    }

    [Serializable]
    public class ContainerData
    {
        public string name;
        public string id;
        public int dx;
        public int dy;
        public int dz;
        public int loadDx;
        public int loadDy;
        public int loadDz;
        public int step;
        public StackData stack;
        public string type;
    }

    [Serializable]
    public class StackData
    {
        public List<PlacementData> placements;
    }

    [Serializable]
    public class PlacementData
    {
        public int x;
        public int y;
        public int z;
        public int step;
        public StackableData stackable;
    }

    [Serializable]
    public class StackableData
    {
        public string id;
        public string name;
        public int dx;
        public int dy;
        public int dz;
        public int step;
        public string type;
    }
}
