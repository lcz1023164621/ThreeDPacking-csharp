using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace ThreeDPacking.Unity
{
    /// <summary>
    /// Unity装箱结果加载器 - 从JSON加载并在场景中生成物理箱子
    /// </summary>
    public class PackingLoader : MonoBehaviour
    {
        [Header("文件设置")]
        [Tooltip("JSON文件路径，相对于StreamingAssets或绝对路径")]
        public string jsonFilePath = "packing_result.json";
        
        [Tooltip("使用StreamingAssets文件夹")]
        public bool useStreamingAssets = true;

        [Header("缩放设置")]
        [Tooltip("单位缩放因子 (例如: 0.001 表示毫米转米)")]
        public float scaleFactor = 0.001f;

        [Header("容器设置")]
        [Tooltip("容器材质")]
        public Material containerMaterial;
        [Tooltip("容器透明度")]
        [Range(0f, 1f)]
        public float containerAlpha = 0.2f;
        [Tooltip("显示容器边框")]
        public bool showContainerWireframe = true;
        [Tooltip("容器边框颜色")]
        public Color containerWireColor = Color.green;

        [Header("箱子设置")]
        [Tooltip("箱子材质")]
        public Material boxMaterial;
        [Tooltip("箱子物理材质")]
        public PhysicMaterial boxPhysicMaterial;
        [Tooltip("为每个箱子随机颜色")]
        public bool randomBoxColors = true;

        [Header("物理模拟设置")]
        [Tooltip("启用重力模拟")]
        public bool enableGravity = true;
        [Tooltip("箱子刚体质量计算密度 (kg/m³)")]
        public float boxDensity = 200f;
        [Tooltip("箱子之间的间距")]
        public float boxSpacing = 0.002f;
        [Tooltip("从高处模拟机械臂放置")]
        public bool simulateRobotArm = true;
        [Tooltip("机械臂抓取高度（在目标位置上方）")]
        public float grabHeight = 1f;
        [Tooltip("放置速度（越大越快）")]
        public float placementSpeed = 2f;
        [Tooltip("顺序放置间隔（秒）")]
        public float placementInterval = 0.5f;
        
        [Header("容器物理设置")]
        [Tooltip("创建带碰撞器的实体容器（而非仅可视化）")]
        public bool createPhysicalContainer = true;
        [Tooltip("容器壁厚度")]
        public float containerWallThickness = 0.02f;
        [Tooltip("容器材质")]
        public PhysicMaterial containerPhysicMaterial;
        
        [Header("地面设置")]
        [Tooltip("创建地面平面（防止物品掉落）")]
        public bool createGroundPlane = true;
        [Tooltip("地面Y坐标")]
        public float groundY = 0f;
        [Tooltip("地面尺寸")]
        public Vector2 groundSize = new Vector2(50f, 50f);

        [Header("验证设置")]
        [Tooltip("放置后验证碰撞")]
        public bool validateCollisions = true;
        [Tooltip("碰撞时显示错误颜色")]
        public bool showCollisionErrors = true;
        public Color collisionErrorColor = Color.red;

        // 生成的物体列表
        private List<GameObject> _placedBoxes = new List<GameObject>();
        private List<GameObject> _containers = new List<GameObject>();
        private PackingResult _packingData;

        void Start()
        {
            LoadAndGenerate();
        }

        [ContextMenu("重新加载")]
        public void LoadAndGenerate()
        {
            ClearScene();
            
            if (LoadJson())
            {
                GenerateScene();
                if (validateCollisions)
                {
                    ValidatePlacements();
                }
            }
        }

        [ContextMenu("清除场景")]
        public void ClearScene()
        {
            foreach (var box in _placedBoxes)
            {
                if (box != null) Destroy(box);
            }
            _placedBoxes.Clear();

            foreach (var container in _containers)
            {
                if (container != null) Destroy(container);
            }
            _containers.Clear();
        }

        /// <summary>
        /// 加载JSON文件
        /// </summary>
        bool LoadJson()
        {
            // 默认优先 StreamingAssets，便于Unity打包与跨平台读取
            var candidatePaths = new List<string>();

            if (useStreamingAssets)
            {
                candidatePaths.Add(Path.Combine(Application.streamingAssetsPath, jsonFilePath));
            }

            // 允许直接传入绝对路径
            candidatePaths.Add(jsonFilePath);

            // 回退：若你使用本仓库的导出约定（解决方案根目录/unity/packing_result.json），Unity端也能自动找到
            // Application.dataPath = <Project>/Assets
            // 解决方案根目录通常就是Unity工程根的上一级或同级，这里优先尝试 <ProjectRoot>/unity/packing_result.json
            try
            {
                var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
                if (!string.IsNullOrEmpty(projectRoot))
                {
                    candidatePaths.Add(Path.Combine(projectRoot, "unity", Path.GetFileName(jsonFilePath)));
                    candidatePaths.Add(Path.Combine(projectRoot, "packing_result.json"));
                }
            }
            catch { /* ignore */ }

            string fullPath = candidatePaths.FirstOrDefault(File.Exists);

            if (string.IsNullOrEmpty(fullPath))
            {
                Debug.LogError("[PackingLoader] 找不到JSON文件。已尝试路径:\n" + string.Join("\n", candidatePaths));
                return false;
            }

            try
            {
                string json = File.ReadAllText(fullPath);
                _packingData = JsonUtility.FromJson<PackingResult>(json);
                Debug.Log($"[PackingLoader] 成功加载 {_packingData.containers.Count} 个容器 | 文件: {fullPath}");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PackingLoader] 解析JSON失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 生成场景
        /// </summary>
        void GenerateScene()
        {
            if (_packingData == null || _packingData.containers == null) return;

            // 创建地面平面
            if (createGroundPlane)
            {
                CreateGroundPlane();
            }

            float offsetX = 0f;

            foreach (var container in _packingData.containers)
            {
                // 创建容器
                CreateContainer(container, offsetX);

                // 创建箱子（从stack.placements中读取）
                if (container.stack != null && container.stack.placements != null)
                {
                    if (simulateRobotArm)
                    {
                        // 使用协程一个一个顺序放置
                        StartCoroutine(PlaceBoxesSequentially(container, offsetX));
                    }
                    else
                    {
                        // 一次性创建所有箱子
                        foreach (var placement in container.stack.placements)
                        {
                            CreateBox(placement, container, offsetX);
                        }
                    }
                }

                // 更新偏移量（为下一个容器留出空间）
                offsetX += (container.loadDx + 100) * scaleFactor;
            }
        }

        /// <summary>
        /// 协程：顺序放置箱子（模拟机械臂一个一个放置）
        /// </summary>
        System.Collections.IEnumerator PlaceBoxesSequentially(ContainerData container, float offsetX)
        {
            if (container == null || container.stack == null || container.stack.placements == null)
                yield break;

            // 按装箱步骤时序放置：每个 placement（物体或牛皮纸）都有一步展示/放置
            // 规则：Z 从小到大；同 Z 时牛皮纸先于物品；再按 X、Y 做稳定排序。
            var orderedPlacements = new List<PlacementData>(container.stack.placements);
            orderedPlacements.Sort((a, b) =>
            {
                if (a == null || a.stackable == null) return 1;
                if (b == null || b.stackable == null) return -1;

                int c = a.z.CompareTo(b.z);
                if (c != 0) return c;

                bool aPad = a.stackable.type == "padding";
                bool bPad = b.stackable.type == "padding";
                // 同 z：物体优先于牛皮纸
                if (aPad != bPad) return aPad ? 1 : -1; // padding 在后

                c = a.x.CompareTo(b.x);
                if (c != 0) return c;
                return a.y.CompareTo(b.y);
            });

            foreach (var placement in orderedPlacements)
            {
                if (placement == null || placement.stackable == null) continue;

                // padding：不走机械臂动画，直接放最终位置
                if (placement.stackable.type == "padding")
                {
                    CreateBox(placement, container, offsetX);
                    yield return new WaitForSeconds(placementInterval);
                    continue;
                }

                // 非 padding：机械臂放置并等待控制器完成
                var controller = CreateBoxWithRobotArm(placement, container, offsetX);
                if (controller != null)
                    yield return new WaitUntil(() => controller == null);

                // 给物理一点时间继续结算（尤其是 enableGravity 打开时）
                yield return new WaitForSeconds(placementInterval);
            }
            
            Debug.Log($"[PackingLoader] 容器 {container.id} 的所有箱子放置完成");
        }

        /// <summary>
        /// 创建地面平面
        /// </summary>
        void CreateGroundPlane()
        {
            GameObject ground = new GameObject("GroundPlane");
            ground.transform.parent = transform;
            
            // 添加碰撞器
            BoxCollider groundCollider = ground.AddComponent<BoxCollider>();
            groundCollider.size = new Vector3(groundSize.x, 0.1f, groundSize.y);
            groundCollider.center = new Vector3(0f, groundY - 0.05f, 0f);
            
            // 可选：添加可视化（调试用，可在Inspector中禁用Renderer）
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "GroundVisual";
            visual.transform.parent = ground.transform;
            visual.transform.localScale = new Vector3(groundSize.x, 0.01f, groundSize.y);
            visual.transform.position = new Vector3(0f, groundY, 0f);
            
            // 设置地面材质（半透明灰色）
            Renderer rend = visual.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.3f, 0.3f, 0.3f, 0.3f);
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = 3000;
            rend.material = mat;
            
            Destroy(visual.GetComponent<Collider>());
            
            _containers.Add(ground);
        }

        /// <summary>
        /// 创建容器（带物理碰撞器）
        /// </summary>
        void CreateContainer(ContainerData data, float offsetX)
        {
            GameObject container = new GameObject($"Container_{data.id}");
            container.transform.parent = transform;

            // 容器尺寸
            Vector3 size = new Vector3(
                data.loadDx * scaleFactor,
                data.loadDz * scaleFactor,
                data.loadDy * scaleFactor
            );
            Vector3 center = new Vector3(
                offsetX + size.x / 2f,
                size.y / 2f,
                size.z / 2f
            );

            // 绘制线框（显示容器边界）
            if (showContainerWireframe)
            {
                DrawWireframe(container.transform, size, center);
            }

            // 创建物理容器（5个面：底面+4个侧面）
            if (createPhysicalContainer)
            {
                CreateContainerWalls(container.transform, size, center, data, offsetX);
            }

            _containers.Add(container);
        }

        /// <summary>
        /// 创建容器的5个物理壁面（底面+4侧面），侧面透明可见
        /// </summary>
        void CreateContainerWalls(Transform parent, Vector3 size, Vector3 center, ContainerData data, float offsetX)
        {
            float t = containerWallThickness;
            PhysicMaterial mat = containerPhysicMaterial;

            // 底面（不透明，作为地面）
            GameObject bottom = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bottom.name = "Bottom";
            bottom.transform.parent = parent;
            bottom.transform.localScale = new Vector3(size.x, t, size.z);
            bottom.transform.position = new Vector3(center.x, t / 2f, center.z);
            Destroy(bottom.GetComponent<Renderer>());
            bottom.GetComponent<Collider>().material = mat;

            // 创建透明侧面材质
            Material transparentSideMat = null;
            if (containerMaterial != null)
            {
                transparentSideMat = new Material(containerMaterial);
                Color c = containerWireColor;
                c.a = containerAlpha;
                transparentSideMat.color = c;
                transparentSideMat.SetFloat("_Mode", 3);
                transparentSideMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                transparentSideMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                transparentSideMat.SetInt("_ZWrite", 0);
                transparentSideMat.DisableKeyword("_ALPHATEST_ON");
                transparentSideMat.EnableKeyword("_ALPHABLEND_ON");
                transparentSideMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                transparentSideMat.renderQueue = 3000;
                transparentSideMat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            }

            // 前面 (Z-) - 透明可见
            GameObject front = GameObject.CreatePrimitive(PrimitiveType.Cube);
            front.name = "Front";
            front.transform.parent = parent;
            front.transform.localScale = new Vector3(size.x, size.y, t);
            front.transform.position = new Vector3(center.x, center.y, center.z - size.z / 2f - t / 2f);
            if (transparentSideMat != null)
            {
                front.GetComponent<Renderer>().material = transparentSideMat;
            }
            front.GetComponent<Collider>().material = mat;

            // 后面 (Z+) - 透明可见
            GameObject back = GameObject.CreatePrimitive(PrimitiveType.Cube);
            back.name = "Back";
            back.transform.parent = parent;
            back.transform.localScale = new Vector3(size.x, size.y, t);
            back.transform.position = new Vector3(center.x, center.y, center.z + size.z / 2f + t / 2f);
            if (transparentSideMat != null)
            {
                back.GetComponent<Renderer>().material = transparentSideMat;
            }
            back.GetComponent<Collider>().material = mat;

            // 左面 (X-) - 透明可见
            GameObject left = GameObject.CreatePrimitive(PrimitiveType.Cube);
            left.name = "Left";
            left.transform.parent = parent;
            left.transform.localScale = new Vector3(t, size.y, size.z + 2 * t);
            left.transform.position = new Vector3(center.x - size.x / 2f - t / 2f, center.y, center.z);
            if (transparentSideMat != null)
            {
                left.GetComponent<Renderer>().material = transparentSideMat;
            }
            left.GetComponent<Collider>().material = mat;

            // 右面 (X+) - 透明可见
            GameObject right = GameObject.CreatePrimitive(PrimitiveType.Cube);
            right.name = "Right";
            right.transform.parent = parent;
            right.transform.localScale = new Vector3(t, size.y, size.z + 2 * t);
            right.transform.position = new Vector3(center.x + size.x / 2f + t / 2f, center.y, center.z);
            if (transparentSideMat != null)
            {
                right.GetComponent<Renderer>().material = transparentSideMat;
            }
            right.GetComponent<Collider>().material = mat;
        }

        /// <summary>
        /// 创建箱子（带物理刚体）- 普通创建
        /// </summary>
        void CreateBox(PlacementData placement, ContainerData container, float offsetX)
        {
            // 从stackable获取物品信息
            var stackable = placement.stackable;
            if (stackable == null) return;

            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = $"Box_{stackable.id}_{placement.step}";
            box.transform.parent = transform;

            // 设置尺寸（坐标系转换：项目X,Y,Z → Unity X,Z,Y）
            Vector3 size = new Vector3(
                Mathf.Max(0.001f, stackable.dx * scaleFactor - boxSpacing),
                Mathf.Max(0.001f, stackable.dz * scaleFactor - boxSpacing),
                Mathf.Max(0.001f, stackable.dy * scaleFactor - boxSpacing)
            );
            box.transform.localScale = size;

            // 计算放置位置（项目坐标系左下角 → Unity中心点，Z→Y转换）
            float floorY = createPhysicalContainer ? containerWallThickness : 0f;
            Vector3 finalPosition = new Vector3(
                (placement.x + stackable.dx / 2f) * scaleFactor + offsetX,
                floorY + (placement.z + stackable.dz / 2f) * scaleFactor,
                (placement.y + stackable.dy / 2f) * scaleFactor
            );
            
            box.transform.position = finalPosition;
            
            // 设置材质
            Renderer renderer = box.GetComponent<Renderer>();
            if (boxMaterial != null)
            {
                Material mat = new Material(boxMaterial);
                if (stackable.type == "padding")
                {
                    // 牛皮纸：固定颜色，方便区分
                    mat.color = new Color(1f, 0.8f, 0.2f, 0.85f);
                }
                else if (randomBoxColors)
                {
                    mat.color = UnityEngine.Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.7f, 1f);
                }
                renderer.material = mat;
            }

            // 添加刚体
            Rigidbody rb = box.AddComponent<Rigidbody>();
            rb.mass = size.x * size.y * size.z * boxDensity;
            rb.drag = 0.1f;
            rb.angularDrag = 0.05f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            // 牛皮纸不参与物理下落，避免被“机械臂投放/重力”带偏位置（它是装箱后填充的几何体）
            if (stackable.type == "padding")
            {
                rb.useGravity = false;
                rb.isKinematic = true;
            }
            else
            {
                rb.useGravity = enableGravity;
            }

            // 设置物理材质
            Collider col = box.GetComponent<Collider>();
            if (boxPhysicMaterial != null)
            {
                col.material = boxPhysicMaterial;
            }

            _placedBoxes.Add(box);
        }

        /// <summary>
        /// 使用机械臂方式创建箱子（从高处平稳下落到JSON指定位置）
        /// </summary>
        RobotArmPlacementController CreateBoxWithRobotArm(PlacementData placement, ContainerData container, float offsetX)
        {
            // 从stackable获取物品信息
            var stackable = placement.stackable;
            if (stackable == null) return null;

            // 牛皮纸：不走机械臂动画，直接放到最终位置
            if (stackable.type == "padding")
            {
                CreateBox(placement, container, offsetX);
                return null;
            }

            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = $"Box_{stackable.id}_{placement.step}";
            box.transform.parent = transform;

            // 设置尺寸（坐标系转换：项目X,Y,Z → Unity X,Z,Y）
            Vector3 size = new Vector3(
                Mathf.Max(0.001f, stackable.dx * scaleFactor - boxSpacing),
                Mathf.Max(0.001f, stackable.dz * scaleFactor - boxSpacing),
                Mathf.Max(0.001f, stackable.dy * scaleFactor - boxSpacing)
            );
            box.transform.localScale = size;

            // 计算目标位置（JSON指定的最终位置）
            float floorY = createPhysicalContainer ? containerWallThickness : 0f;
            Vector3 targetPosition = new Vector3(
                (placement.x + stackable.dx / 2f) * scaleFactor + offsetX,
                floorY + (placement.z + stackable.dz / 2f) * scaleFactor,
                (placement.y + stackable.dy / 2f) * scaleFactor
            );
            
            // 起始位置：在目标位置上方（模拟机械臂抓取位置）
            Vector3 startPosition = targetPosition + Vector3.up * grabHeight;
            box.transform.position = startPosition;
            
            // 设置材质
            Renderer renderer = box.GetComponent<Renderer>();
            if (boxMaterial != null)
            {
                Material mat = new Material(boxMaterial);
                if (randomBoxColors)
                {
                    mat.color = UnityEngine.Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.7f, 1f);
                }
                renderer.material = mat;
            }

            // 添加刚体
            Rigidbody rb = box.AddComponent<Rigidbody>();
            rb.mass = size.x * size.y * size.z * boxDensity;
            rb.drag = 0.1f;
            rb.angularDrag = 0.05f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            
            // 初始禁用重力，由脚本控制平稳下落
            rb.useGravity = false;
            rb.isKinematic = true;

            // 设置物理材质
            Collider col = box.GetComponent<Collider>();
            if (boxPhysicMaterial != null)
            {
                col.material = boxPhysicMaterial;
            }

            // 添加机械臂放置控制器
            var controller = box.AddComponent<RobotArmPlacementController>();
            controller.Initialize(targetPosition, placementSpeed, enableGravity);
            
            Debug.Log($"[PackingLoader] 机械臂放置箱子 {box.name} 从 {startPosition.y:F2}m 到目标 {targetPosition.y:F2}m");

            _placedBoxes.Add(box);

            return controller;
        }

        /// <summary>
        /// 绘制容器线框
        /// </summary>
        void DrawWireframe(Transform parent, Vector3 size, Vector3 center)
        {
            Vector3 half = size / 2f;
            Vector3[] corners = new Vector3[8];
            
            // 计算8个角点
            for (int i = 0; i < 8; i++)
            {
                corners[i] = center + new Vector3(
                    ((i & 1) == 0) ? -half.x : half.x,
                    ((i & 2) == 0) ? -half.y : half.y,
                    ((i & 4) == 0) ? -half.z : half.z
                );
            }

            // 创建线框游戏对象
            GameObject wireframe = new GameObject("Wireframe");
            wireframe.transform.parent = parent;

            // 绘制12条边
            int[,] edges = new int[,] {
                {0,1}, {0,2}, {0,4}, {1,3}, {1,5}, {2,3},
                {2,6}, {3,7}, {4,5}, {4,6}, {5,7}, {6,7}
            };

            for (int i = 0; i < 12; i++)
            {
                DrawLine(wireframe.transform, corners[edges[i,0]], corners[edges[i,1]], containerWireColor);
            }
        }

        /// <summary>
        /// 绘制单条线
        /// </summary>
        void DrawLine(Transform parent, Vector3 start, Vector3 end, Color color)
        {
            GameObject line = new GameObject("Line");
            line.transform.parent = parent;
            
            LineRenderer lr = line.AddComponent<LineRenderer>();
            lr.startWidth = 0.005f;
            lr.endWidth = 0.005f;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = color;
            lr.endColor = color;
            lr.positionCount = 2;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }

        /// <summary>
        /// 验证放置是否有碰撞
        /// </summary>
        void ValidatePlacements()
        {
            int collisionCount = 0;

            foreach (var box in _placedBoxes)
            {
                Collider boxCollider = box.GetComponent<Collider>();
                Vector3 center = boxCollider.bounds.center;
                Vector3 halfExtents = boxCollider.bounds.extents * 0.99f; // 略微缩小避免接触误判

                Collider[] overlaps = Physics.OverlapBox(center, halfExtents, box.transform.rotation);

                foreach (var other in overlaps)
                {
                    if (other.gameObject != box && other.gameObject.name.StartsWith("Box_"))
                    {
                        collisionCount++;
                        
                        if (showCollisionErrors)
                        {
                            Renderer rend = box.GetComponent<Renderer>();
                            rend.material.color = collisionErrorColor;
                            Debug.LogWarning($"[PackingLoader] 检测到碰撞: {box.name} 与 {other.gameObject.name}");
                        }
                        break;
                    }
                }
            }

            if (collisionCount > 0)
            {
                Debug.LogWarning($"[PackingLoader] 共检测到 {collisionCount} 处碰撞");
            }
            else
            {
                Debug.Log("[PackingLoader] 验证通过：无碰撞检测");
            }
        }

        /// <summary>
        /// 重置物理模拟（重新加载场景）
        /// </summary>
        [ContextMenu("重置物理模拟")]
        public void ResetPhysicsSimulation()
        {
            LoadAndGenerate();
            Debug.Log("[PackingLoader] 物理模拟已重置");
        }

        /// <summary>
        /// 检查稳定性（检测是否有箱子发生倾斜或位移）
        /// </summary>
        [ContextMenu("检查稳定性")]
        public void CheckStability()
        {
            int unstableCount = 0;
            float tiltThreshold = 5f; // 倾斜角度阈值（度）
            float displacementThreshold = 0.01f; // 位移阈值（米）

            foreach (var box in _placedBoxes)
            {
                // 检查倾斜角度
                float tiltX = Mathf.Abs(box.transform.rotation.eulerAngles.x);
                float tiltZ = Mathf.Abs(box.transform.rotation.eulerAngles.z);
                // 标准化角度到0-180范围
                if (tiltX > 180f) tiltX = 360f - tiltX;
                if (tiltZ > 180f) tiltZ = 360f - tiltZ;

                if (tiltX > tiltThreshold || tiltZ > tiltThreshold)
                {
                    unstableCount++;
                    Debug.LogWarning($"[PackingLoader] {box.name} 发生倾斜 - X:{tiltX:F1}°, Z:{tiltZ:F1}°");
                    
                    // 高亮显示不稳定的箱子
                    Renderer rend = box.GetComponent<Renderer>();
                    if (rend != null)
                    {
                        rend.material.color = Color.red;
                    }
                }
            }

            if (unstableCount > 0)
            {
                Debug.LogWarning($"[PackingLoader] 检测到 {unstableCount}/{_placedBoxes.Count} 个箱子不稳定");
            }
            else
            {
                Debug.Log("[PackingLoader] 稳定性检查通过：所有箱子保持稳定");
            }
        }
    }
}
