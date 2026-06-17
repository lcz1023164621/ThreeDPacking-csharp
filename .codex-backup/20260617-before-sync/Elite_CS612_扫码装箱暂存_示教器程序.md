# Elite CS612 扫码装箱暂存示教器程序

这份程序和 C# 端新协议对应。当前 C# 端按“上位机主动连接机械臂两个端口”的方式实现：

| Socket | 方向 | 默认端口 | 类型 |
| --- | --- | --- | --- |
| `socket1` | PC -> Robot，机械臂接收命令 | `10000` | 字符串 |
| `socket2` | Robot -> PC，机械臂发送事件 | `10001` | 字符串 |

示教器里如果不能把节点命名为 `Server_RX` / `Server_TX`，就仍然用原来的 `Server(...)` 节点，但接收命令的节点选择 `socket1`，发送事件的节点选择 `socket2`。

## 通信消息

PC 发给机械臂：

```text
DATA_PACK_POS|count=3|p1=55,95,120,90,170,60,A#1|p2=160,85,140,100,150,80,C#1|p3=80,80,90,120,120,90,B#1
CMD_PICK_SCAN
CMD_PLACE_HELD|item=A#1|place=1|pose=BOX_ORIGIN
CMD_BUFFER_HELD|item=B#1|slot=1
CMD_TAKE_BUFFER|item=B#1|slot=1|place=3|pose=BOX_ORIGIN
CMD_DONE
CMD_ABORT|reason=operator_abort
VISION_OK
VISION_ROTATE
VISION_FAIL
```

机械臂发给 PC：

```text
EVT_ROBOT_READY
EVT_SCAN_READY|pick=0
EVT_HELD_PLACED|item=A#1|place=1
EVT_HELD_BUFFERED|item=B#1|slot=1
EVT_BUFFER_PLACED|item=B#1|slot=1|place=3
EVT_ACTION_DONE
EVT_PHOTO_AT_POSE
EVT_VISION_WAIT
EVT_DONE
EVT_ERROR|reason=xxx
```

`DATA_PACK_POS` 里的 `p1/p2/p3` 是装箱坐标列表。每个点格式为：

```text
中心X,中心Y,顶部Z,物体长,物体宽,物体高,物体ID
```

C# 端发的是装箱目标的顶部中心点，单位沿用算法结果的毫米值。示教器里的 `装箱原点` 要和算法坐标系一致。

## parse_action.script

把下面函数加入或替换到 `parse_action.script`。如果 Elite 脚本不支持其中某个字符串 API，就保留函数含义，用你当前 `strtoshuju.script` 的写法实现。

```python
def first_line(raw):
    if raw == None:
        return ""
    return str(raw).split("\n")[0].strip()

def get_action(cmd):
    return first_line(cmd).split("|")[0].strip()

def get_field(cmd, key):
    parts = first_line(cmd).split("|")
    for p in parts[1:]:
        kv = p.split("=", 1)
        if len(kv) == 2 and kv[0].strip() == key:
            return kv[1].strip()
    return ""

def get_int_field(cmd, key, default_value):
    v = get_field(cmd, key)
    if v == "":
        return default_value
    return int(v)

def get_pack_position_by_index(pack_data, index):
    # index 从 0 开始；DATA_PACK_POS 里字段从 p1 开始
    key = "p" + str(index + 1)
    value = get_field(pack_data, key)
    parts = value.split(",")
    return [
        float(parts[0]),
        float(parts[1]),
        float(parts[2]),
        float(parts[3]),
        float(parts[4]),
        float(parts[5])
    ]
```

## 主任务

下面按示教器任务树格式写。坐标点名称沿用你截图里的点位名称：`箱子上方`、`装箱原点`、`测放装箱`、`暂存开始点`、`扫码位` 等。

```text
开始前
  脚本: strtobuffer.script
  脚本: parse_action.script

  pick_index := 0
  catch := 0
  pack_data := ""
  cmd_raw := ""
  cmd := ""
  action := ""
  item := ""
  slot := ""
  place_index := 0
  pose_mode := ""
  pack_position := [0,0,0,0,0,0]
  buffer_position := [0,0,0,0,0,0]

  # 先告诉上位机机械臂程序已启动
  Server_TX(发送数据:="EVT_ROBOT_READY")

机器人主任务
  循环 True
    Server_RX(cmd_raw:=接收到的值)
    cmd := first_line(cmd_raw)
    action := get_action(cmd)
    弹出窗口: cmd       # 调试稳定后可删除

    If action ?= "DATA_PACK_POS"
      pack_data := cmd
      Server_TX(发送数据:="EVT_ACTION_DONE")

    Elsif action ?= "CMD_PICK_SCAN"
      # 1. 从来料区按 pick_index 抓取下一个物体
      current_position := get_position_by_index(来料坐标, pick_index)
      处理坐标
      MoveJ 箱子上方
      抓取
      catch := 1

      抓取后tcp := get_actual_tcp_pose()
      向上偏移 := [0,0,0.25,0,0,0]
      偏移 位姿: 向上偏移(偏移自 "抓取后tcp")
      MoveL 向上偏移

      # 2. 到扫码/拍照位
      MoveJ 扫码位
      调用 拍照旋转

      Server_TX(发送数据:="EVT_SCAN_READY|pick=" + to_str(pick_index))
      pick_index := pick_index + 1

    Elsif action ?= "CMD_BUFFER_HELD"
      item := get_field(cmd, "item")
      slot := get_field(cmd, "slot")
      buffer_position := get_var_current_position(slot)

      MoveJ 暂存开始点
      暂存xy偏移后 := get_actual_tcp_pose()
      暂存xy偏移 := [buffer_position[0], buffer_position[1], 0, 0, 0, 0]
      偏移 位姿: 暂存xy偏移后(偏移自 "暂存xy偏移后")
      MoveL 暂存xy偏移

      暂存z偏移后 := get_actual_tcp_pose()
      暂存下放偏移 := [0,0,buffer_position[2]-0.25,0,0,0]
      偏移 位姿: 暂存下放偏移(偏移自 "暂存z偏移后")
      MoveL 暂存下放偏移
      释放
      catch := 0

      当前位 := get_actual_tcp_pose()
      向上偏移固定 := [0,0,0.25,0,0,0]
      偏移 位姿: 暂存放料上升(偏移自 "当前位")
      MoveL 暂存放料上升

      Server_TX(发送数据:="EVT_HELD_BUFFERED|item=" + item + "|slot=" + slot)
      Server_TX(发送数据:="EVT_ACTION_DONE")

    Elsif action ?= "CMD_PLACE_HELD"
      item := get_field(cmd, "item")
      place_index := get_int_field(cmd, "place", 1) - 1
      pose_mode := get_field(cmd, "pose")
      pack_position := get_pack_position_by_index(pack_data, place_index)

      If pose_mode ?= "TEST_BOX"
        MoveJ 测放装箱
      Else
        MoveJ 装箱原点

      箱子上方原点tcp := get_actual_tcp_pose()
      箱子上方XY偏移 := [pack_position[0], pack_position[1], 0, 0, 0, 0]
      偏移 位姿: 装箱前在上方xy偏移(偏移自 "箱子上方原点tcp")
      MoveL 装箱前在上方xy偏移

      下放前tcp := get_actual_tcp_pose()
      下放点位 := [0,0,pack_position[2],0,0,0]
      偏移 位姿: 物体装箱下放(偏移自 "下放前tcp")
      MoveL 物体装箱下放
      释放
      catch := 0

      当前位 := get_actual_tcp_pose()
      向上偏移固定 := [0,0,0.25,0,0,0]
      偏移 位姿: 装箱后上升(偏移自 "当前位")
      MoveL 装箱后上升

      Server_TX(发送数据:="EVT_HELD_PLACED|item=" + item + "|place=" + to_str(place_index + 1))
      Server_TX(发送数据:="EVT_ACTION_DONE")

    Elsif action ?= "CMD_TAKE_BUFFER"
      item := get_field(cmd, "item")
      slot := get_field(cmd, "slot")
      place_index := get_int_field(cmd, "place", 1) - 1
      pose_mode := get_field(cmd, "pose")
      buffer_position := get_var_current_position(slot)
      pack_position := get_pack_position_by_index(pack_data, place_index)

      # 1. 从暂存台取回物体
      MoveJ 暂存开始点
      暂存xy偏移后 := get_actual_tcp_pose()
      暂存xy偏移 := [buffer_position[0], buffer_position[1], 0, 0, 0, 0]
      偏移 位姿: 暂存xy偏移(偏移自 "暂存xy偏移后")
      MoveL 暂存xy偏移

      暂存向下偏移 := get_actual_tcp_pose()
      暂存取料下放 := [0,0,buffer_position[2]-0.25,0,0,0]
      偏移 位姿: 暂存取料下放(偏移自 "暂存向下偏移")
      MoveL 暂存取料下放
      抓取
      catch := 1

      当前位 := get_actual_tcp_pose()
      向上偏移固定 := [0,0,0.25,0,0,0]
      偏移 位姿: 暂存取料上升(偏移自 "当前位")
      MoveL 暂存取料上升

      # 2. 放入对应装箱位
      If pose_mode ?= "TEST_BOX"
        MoveJ 测放装箱
      Else
        MoveJ 装箱原点

      箱子上方原点tcp := get_actual_tcp_pose()
      箱子上方XY偏移 := [pack_position[0], pack_position[1], 0, 0, 0, 0]
      偏移 位姿: 装箱前在上方xy偏移(偏移自 "箱子上方原点tcp")
      MoveL 装箱前在上方xy偏移

      下放前tcp := get_actual_tcp_pose()
      下放点位 := [0,0,pack_position[2],0,0,0]
      偏移 位姿: 物体装箱下放(偏移自 "下放前tcp")
      MoveL 物体装箱下放
      释放
      catch := 0

      当前位 := get_actual_tcp_pose()
      向上偏移固定 := [0,0,0.25,0,0,0]
      偏移 位姿: 装箱后上升(偏移自 "当前位")
      MoveL 装箱后上升

      Server_TX(发送数据:="EVT_BUFFER_PLACED|item=" + item + "|slot=" + slot + "|place=" + to_str(place_index + 1))
      Server_TX(发送数据:="EVT_ACTION_DONE")

    Elsif action ?= "CMD_DONE"
      Server_TX(发送数据:="EVT_DONE")
      退出循环

    Elsif action ?= "CMD_ABORT"
      Server_TX(发送数据:="EVT_ERROR|reason=abort")
      退出循环

    Else
      Server_TX(发送数据:="EVT_ERROR|reason=unknown_command|cmd=" + cmd)
```

## 拍照旋转子程序

如果你暂时只做扫码，不需要旋转识别，可以把这个子程序简化成 `MoveJ 扫码位` 后直接返回。若继续保留原来的拍照旋转逻辑，改成下面的字符串协议。

```text
拍照旋转
  Server_TX(发送数据:="EVT_PHOTO_AT_POSE")
  Server_RX(vision_raw:=接收到的值)
  vision := first_line(vision_raw)

  If vision ?= "VISION_OK"
    返回

  Elsif vision ?= "VISION_ROTATE"
    执行旋转动作
    Server_TX(发送数据:="EVT_VISION_WAIT")
    Server_RX(vision_raw:=接收到的值)
    vision := first_line(vision_raw)

    If vision ?= "VISION_OK"
      返回
    Elsif vision ?= "VISION_FAIL"
      Server_TX(发送数据:="EVT_ERROR|reason=vision_fail")
      返回
    Else
      返回

  Elsif vision ?= "VISION_FAIL"
    Server_TX(发送数据:="EVT_ERROR|reason=vision_fail")
    返回

  Else
    Server_TX(发送数据:="EVT_ERROR|reason=unknown_vision_result|value=" + vision)
    返回
```

## BAC 抓取、ACB 放置示例

装箱计划顺序为 `A, C, B`，实际来料抓取顺序为 `B, A, C`：

```text
PC -> Robot: DATA_PACK_POS|count=3|p1=...A#1|p2=...C#1|p3=...B#1
PC -> Robot: CMD_PICK_SCAN
Robot -> PC: EVT_SCAN_READY|pick=0
扫码结果: B
PC -> Robot: CMD_BUFFER_HELD|item=B#1|slot=1
Robot -> PC: EVT_ACTION_DONE

PC -> Robot: CMD_PICK_SCAN
Robot -> PC: EVT_SCAN_READY|pick=1
扫码结果: A
PC -> Robot: CMD_PLACE_HELD|item=A#1|place=1|pose=BOX_ORIGIN
Robot -> PC: EVT_ACTION_DONE

PC -> Robot: CMD_PICK_SCAN
Robot -> PC: EVT_SCAN_READY|pick=2
扫码结果: C
PC -> Robot: CMD_PLACE_HELD|item=C#1|place=2|pose=BOX_ORIGIN
Robot -> PC: EVT_ACTION_DONE

PC -> Robot: CMD_TAKE_BUFFER|item=B#1|slot=1|place=3|pose=BOX_ORIGIN
Robot -> PC: EVT_ACTION_DONE

PC -> Robot: CMD_DONE
Robot -> PC: EVT_DONE
```

这里不会再出现 `9\n6\n`。回取暂存物时，`slot` 和 `place` 在同一条命令里。
