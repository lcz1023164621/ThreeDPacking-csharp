using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using ThreeDPacking.Core.Models;

namespace ThreeDPacking.Core.IO
{
    /// <summary>
    /// 装箱结果序列化
    /// </summary>
    public static class ResultSerializer
    {
        public static string Serialize(List<Container> containers)
        {
            var result = new ResultData { Containers = new List<ContainerData>() };
            int step = 0;

            foreach (var container in containers)
            {
                var cd = new ContainerData
                {
                    Name = container.Description ?? container.Id,
                    Id = container.Id,
                    Dx = container.Dx,
                    Dy = container.Dy,
                    Dz = container.Dz,
                    LoadDx = container.LoadDx,
                    LoadDy = container.LoadDy,
                    LoadDz = container.LoadDz,
                    Step = step++,
                    Stack = new StackData { Placements = new List<PlacementData>() }
                };

                if (container.Stack != null)
                {
                    foreach (var p in container.Stack.Placements)
                    {
                        cd.Stack.Placements.Add(new PlacementData
                        {
                            X = p.X,
                            Y = p.Y,
                            Z = p.Z,
                            Step = step++,
                            Stackable = new StackableData
                            {
                                Id = p.StackValue.Box?.Id,
                                Name = p.StackValue.Box?.Description ?? p.StackValue.Box?.Id,
                                Dx = p.StackValue.Dx,
                                Dy = p.StackValue.Dy,
                                Dz = p.StackValue.Dz,
                                Step = step,
                                Type = "box"
                            }
                        });
                    }
                }

                result.Containers.Add(cd);
            }

            return JsonConvert.SerializeObject(result, Formatting.Indented);
        }

        public static void SerializeToFile(List<Container> containers, string filePath)
        {
            string json = Serialize(containers);
            File.WriteAllText(filePath, json);
        }

        #region JSON Data Classes

        private class ResultData
        {
            [JsonProperty("containers")]
            public List<ContainerData> Containers { get; set; }
        }

        private class ContainerData
        {
            [JsonProperty("name")] public string Name { get; set; }
            [JsonProperty("id")] public string Id { get; set; }
            [JsonProperty("dx")] public int Dx { get; set; }
            [JsonProperty("dy")] public int Dy { get; set; }
            [JsonProperty("dz")] public int Dz { get; set; }
            [JsonProperty("loadDx")] public int LoadDx { get; set; }
            [JsonProperty("loadDy")] public int LoadDy { get; set; }
            [JsonProperty("loadDz")] public int LoadDz { get; set; }
            [JsonProperty("step")] public int Step { get; set; }
            [JsonProperty("stack")] public StackData Stack { get; set; }
            [JsonProperty("type")] public string Type => "container";
        }

        private class StackData
        {
            [JsonProperty("placements")]
            public List<PlacementData> Placements { get; set; }
        }

        private class PlacementData
        {
            [JsonProperty("x")] public int X { get; set; }
            [JsonProperty("y")] public int Y { get; set; }
            [JsonProperty("z")] public int Z { get; set; }
            [JsonProperty("step")] public int Step { get; set; }
            [JsonProperty("stackable")] public StackableData Stackable { get; set; }
        }

        private class StackableData
        {
            [JsonProperty("id")] public string Id { get; set; }
            [JsonProperty("name")] public string Name { get; set; }
            [JsonProperty("dx")] public int Dx { get; set; }
            [JsonProperty("dy")] public int Dy { get; set; }
            [JsonProperty("dz")] public int Dz { get; set; }
            [JsonProperty("step")] public int Step { get; set; }
            [JsonProperty("type")] public string Type { get; set; }
        }

        #endregion
    }
}
