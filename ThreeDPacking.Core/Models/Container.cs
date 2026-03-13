using System;
using System.Collections.Generic;

namespace ThreeDPacking.Core.Models
{
    /// <summary>
    /// 容器运行时状态，管理已放置的物品列表
    /// </summary>
    public class Container
    {
        public string Id { get; }
        public string Description { get; }
        //容器外尺寸
        public int Dx { get; }
        public int Dy { get; }
        public int Dz { get; }
        //容器内尺寸（可装载空间），默认为外尺寸
        public int LoadDx { get; }
        public int LoadDy { get; }
        public int LoadDz { get; }
        public int EmptyWeight { get; }
        public int MaxLoadWeight { get; }
        //外体积
        public long Volume { get; }
        //最大可用体积
        public long MaxLoadVolume { get; }
        public long MaximumArea { get; }
        //已装载物品列表
        public PackStack Stack { get; }

        public Container(string id, string description, int dx, int dy, int dz,
            int emptyWeight, int maxLoadWeight,
            int loadDx = -1, int loadDy = -1, int loadDz = -1,
            PackStack stack = null)
        {
            Id = id;
            Description = description;
            Dx = dx;
            Dy = dy;
            Dz = dz;
            LoadDx = loadDx == -1 ? dx : loadDx;
            LoadDy = loadDy == -1 ? dy : loadDy;
            LoadDz = loadDz == -1 ? dz : loadDz;
            EmptyWeight = emptyWeight;
            MaxLoadWeight = maxLoadWeight;
            Volume = (long)dx * dy * dz;
            MaximumArea = (long)LoadDx * LoadDy;
            MaxLoadVolume = MaximumArea * LoadDz;
            Stack = stack ?? new PackStack();
        }
        /// <summary>
        /// 判斷單件此物品是否能放入（體積、重量、尺寸任一旋轉是否可行）
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public bool CanLoad(Box box)
        {
            if (box.Volume > MaxLoadVolume) return false;
            if (box.Weight > MaxLoadWeight) return false;
            foreach (var sv in box.StackValues)
            {
                if (sv.FitsInside3D(LoadDx, LoadDy, LoadDz))
                    return true;
            }
            return false;
        }
        /// <summary>
        /// 判斷某個具體旋轉是否能放入（純尺寸檢查）
        /// </summary>
        /// <param name="sv"></param>
        /// <returns></returns>
        public bool CanLoad(BoxStackValue sv)
        {
            return sv.Dx <= LoadDx && sv.Dy <= LoadDy && sv.Dz <= LoadDz;
        }

        public bool FitsInside(Box box)
        {
            if (box.Volume > MaxLoadVolume) return false;
            if (box.Weight > MaxLoadWeight) return false;
            foreach (var sv in box.StackValues)
            {
                if (sv.FitsInside3D(LoadDx, LoadDy, LoadDz))
                    return true;
            }
            return false;
        }

        public bool FitsInside(BoxItem boxItem)
        {
            if (boxItem.GetVolume() > MaxLoadVolume) return false;
            if (boxItem.GetWeight() > MaxLoadWeight) return false;
            return FitsInside(boxItem.Box);
        }

        public Container Clone()
        {
            return new Container(Id, Description, Dx, Dy, Dz, EmptyWeight, MaxLoadWeight,
                LoadDx, LoadDy, LoadDz, new PackStack());
        }

        public override string ToString()
        {
            return $"Container[{Id ?? ""} {Dx}x{Dy}x{Dz}]";
        }
    }
}
