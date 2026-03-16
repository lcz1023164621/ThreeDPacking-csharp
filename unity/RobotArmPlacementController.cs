using UnityEngine;

namespace ThreeDPacking.Unity
{
    /// <summary>
    /// 机械臂放置控制器 - 从高处平稳下落到目标位置，然后启用重力
    /// </summary>
    public class RobotArmPlacementController : MonoBehaviour
    {
        private Vector3 targetPosition;
        private float placementSpeed;
        private bool enableGravityAfterPlacement;
        private bool hasPlaced = false;
        private Rigidbody rb;
        
        // 放置检测参数
        private float placementThreshold = 0.005f;
        
        public void Initialize(Vector3 target, float speed, bool enableGravity)
        {
            targetPosition = target;
            placementSpeed = speed;
            enableGravityAfterPlacement = enableGravity;
        }
        
        void Start()
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogError($"[RobotArmPlacementController] {gameObject.name} 没有找到Rigidbody");
                enabled = false;
                return;
            }
            
            // 确保初始状态正确
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            
            Debug.Log($"[RobotArmPlacementController] {gameObject.name} 开始从 {transform.position.y:F2}m 下落到目标 {targetPosition.y:F2}m");
        }
        
        void Update()
        {
            if (hasPlaced) return;
            
            // 计算到目标位置的垂直距离
            float verticalDistance = transform.position.y - targetPosition.y;
            
            // 如果到达或低于目标位置，完成放置
            if (verticalDistance <= placementThreshold)
            {
                CompletePlacement();
                return;
            }
            
            // 使用平滑的插值移动（模拟机械臂的精确控制）
            // 计算新的Y位置
            float newY = Mathf.MoveTowards(transform.position.y, targetPosition.y, placementSpeed * Time.deltaTime);
            
            // 直接设置位置（水平位置保持精确）
            transform.position = new Vector3(targetPosition.x, newY, targetPosition.z);
            
            // 保持水平稳定（无旋转）
            transform.rotation = Quaternion.identity;
        }
        
        void CompletePlacement()
        {
            hasPlaced = true;
            
            // 精确设置到目标位置
            transform.position = targetPosition;
            transform.rotation = Quaternion.identity;
            
            // "放开"机械臂，启用重力
            if (enableGravityAfterPlacement)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.useGravity = true;
                rb.isKinematic = false;
                
                Debug.Log($"[RobotArmPlacementController] {gameObject.name} 已放置到目标位置 {targetPosition.y:F3}m，机械臂放开，启用重力");
            }
            else
            {
                // 如果不启用重力，保持kinematic静止
                rb.isKinematic = true;
                Debug.Log($"[RobotArmPlacementController] {gameObject.name} 已放置，保持静止");
            }
            
            // 完成使命，销毁此脚本
            Destroy(this);
        }
        
        void OnDrawGizmosSelected()
        {
            if (Application.isPlaying && !hasPlaced)
            {
                // 绘制目标位置
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(targetPosition, transform.localScale);
                
                // 绘制连接线
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, targetPosition);
                
                // 绘制向下的箭头表示下落方向
                Gizmos.color = Color.red;
                Vector3 arrowStart = transform.position + Vector3.up * 0.1f;
                Vector3 arrowEnd = transform.position;
                Gizmos.DrawLine(arrowStart, arrowEnd);
                Gizmos.DrawWireSphere(arrowEnd, 0.02f);
            }
        }
    }
}
