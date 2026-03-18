using System;
using System.Collections.Generic;
using OpenTK;
using ThreeDPacking.Core.Models;

namespace ThreeDPacking.App.Rendering
{
    /// <summary>
    /// 鼠标拾取检测，用于选中特定物品高亮显示
    /// </summary>
    public static class HitTester
    {
        /// <summary>
        /// Test if a ray intersects an axis-aligned bounding box (AABB).
        /// Returns the distance along the ray, or -1 if no intersection.
        /// </summary>
        public static float RayIntersectsAABB(Vector3 rayOrigin, Vector3 rayDir,
            Vector3 boxMin, Vector3 boxMax)
        {
            float tmin = float.NegativeInfinity;
            float tmax = float.PositiveInfinity;

            for (int i = 0; i < 3; i++)
            {
                float origin = i == 0 ? rayOrigin.X : i == 1 ? rayOrigin.Y : rayOrigin.Z;
                float dir = i == 0 ? rayDir.X : i == 1 ? rayDir.Y : rayDir.Z;
                float min = i == 0 ? boxMin.X : i == 1 ? boxMin.Y : boxMin.Z;
                float max = i == 0 ? boxMax.X : i == 1 ? boxMax.Y : boxMax.Z;

                if (Math.Abs(dir) < 1e-8f)
                {
                    if (origin < min || origin > max)
                        return -1;
                }
                else
                {
                    float t1 = (min - origin) / dir;
                    float t2 = (max - origin) / dir;
                    if (t1 > t2)
                    {
                        float tmp = t1;
                        t1 = t2;
                        t2 = tmp;
                    }
                    tmin = Math.Max(tmin, t1);
                    tmax = Math.Min(tmax, t2);
                    if (tmin > tmax)
                        return -1;
                }
            }

            return tmin >= 0 ? tmin : tmax >= 0 ? tmax : -1;
        }

        /// <summary>
        /// Find the closest placement hit by a ray.
        /// </summary>
        public static Placement FindHit(Vector3 rayOrigin, Vector3 rayDir, Container container, int maxStep)
        {
            return FindHit(rayOrigin, rayDir, container, maxStep, Vector3.Zero);
        }

        /// <summary>
        /// Find the closest placement hit by a ray with container offset.
        /// </summary>
        public static Placement FindHit(Vector3 rayOrigin, Vector3 rayDir, Container container, int maxStep, Vector3 containerOffset)
        {
            if (container?.Stack == null) return null;

            Placement closest = null;
            float closestDist = float.MaxValue;
            int step = 0;

            foreach (var p in container.Stack.Placements)
            {
                step++;
                if (step > maxStep) break;

                var boxMin = new Vector3(p.X + containerOffset.X, p.Z + containerOffset.Y, p.Y + containerOffset.Z);
                var boxMax = new Vector3(p.AbsoluteEndX + 1 + containerOffset.X, p.AbsoluteEndZ + 1 + containerOffset.Y, p.AbsoluteEndY + 1 + containerOffset.Z);

                float dist = RayIntersectsAABB(rayOrigin, rayDir, boxMin, boxMax);
                if (dist >= 0 && dist < closestDist)
                {
                    closestDist = dist;
                    closest = p;
                }
            }

            return closest;
        }

        /// <summary>
        /// Find the closest placement hit by a ray across multiple containers.
        /// </summary>
        public static Placement FindHit(Vector3 rayOrigin, Vector3 rayDir, List<Container> containers, int maxStep, out Container hitContainer)
        {
            hitContainer = null;
            if (containers == null || containers.Count == 0) return null;

            Placement closest = null;
            float closestDist = float.MaxValue;
            float offsetX = 0;

            foreach (var container in containers)
            {
                if (container?.Stack == null) continue;

                var containerOffset = new Vector3(offsetX, 0, 0);
                
                // Find hit within this container with offset
                Placement hit = null;
                float hitDist = float.MaxValue;
                int step = 0;

                foreach (var p in container.Stack.Placements)
                {
                    step++;
                    if (step > maxStep) break;

                    var boxMin = new Vector3(p.X + containerOffset.X, p.Z + containerOffset.Y, p.Y + containerOffset.Z);
                    var boxMax = new Vector3(p.AbsoluteEndX + 1 + containerOffset.X, p.AbsoluteEndZ + 1 + containerOffset.Y, p.AbsoluteEndY + 1 + containerOffset.Z);

                    float dist = RayIntersectsAABB(rayOrigin, rayDir, boxMin, boxMax);
                    if (dist >= 0 && dist < hitDist)
                    {
                        hitDist = dist;
                        hit = p;
                    }
                }

                if (hit != null && hitDist < closestDist)
                {
                    closestDist = hitDist;
                    closest = hit;
                    hitContainer = container;
                }

                // Move next container to the right with spacing (must match PackingRenderer spacing)
                offsetX += container.LoadDx + 150;
            }

            return closest;
        }
    }
}
