# 示教器改造说明（基于当前装箱工程）

你截图里的工程现在是一个单一路径：

```text
Server(position:=接收到的值)
脚本: strtoshuzu.script
var_index:=0
机器人主任务:
  下放坐标:=position
  var_current_position:=get_position_by_index(下放坐标,var_index)
  按 var_current_position 做 XY 偏移和 Z 下放
  catch:=0
  var_index:=var_index+1
```

这个结构只能按装箱顺序连续放，无法处理“先抓到 B，需要暂存；放完 A 后再从暂存取 B”的动作链。需要改成“扫码后等待 PC 字符串命令，再按命令走装箱 / 放暂存 / 取暂存”。

## 关于变量区分

通信协议继续使用 8056 的旧格式：

```text
seq,drop
```

这里不新增 pick。原因是你说的现场逻辑成立：暂存抓取和暂存下放的 XY 是同一个槽位，差异主要是 Z，而 Z 更适合在示教器里按现场高度、夹爪、物体厚度去量和调整。

但示教器内部仍建议区分变量名，不要全部叫 `var_current_position`：

| 变量 | 来源 | 用途 |
|------|------|------|
| `box_place_position` | 8055 `下放坐标` | 箱内下放 |
| `buffer_drop_position` | 8056 `seq,drop` | 把当前夹持物放到暂存台 |
| `buffer_slot_position` | 8056 `seq,drop` | 从暂存台取物时使用它的 XY |
| `buffer_pick_z_offset` | 示教器现场变量 | 从暂存台抓取时的 Z 下探量 |
| `box_target_index` | `seq-1` | 箱内目标索引 |
| `buffer_seq` | 8056 第一段 | 暂存物体对应的装箱顺序 |
| `action_signal` | 10000 收到的字符串 | 原始命令 |
| `action_name` | `parse_action(action_signal)` | 字符串动作分支 |

这样区分的目的不是让 PC 多发数据，而是避免示教器动作之间互相覆盖变量。

## 开始前

保留你现有的 8055 装箱坐标接收，再导入脚本：

```text
1  初始化变量
2  开始前
3    Server(position:=接收到的值)       // 8055，一次接收所有箱内下放坐标
4    脚本: strtoshuzu.script
5    脚本: strtobuffer.script
6    脚本: parse_action.script
7    下放坐标:=position
8    var_index:=0
9    buffer_pick_z_offset:=现场测得的暂存抓取Z偏移
```

8056 示例：

```text
2,[0.2,0.04,-0.42,0,0,0]
```

## 主流程

把你现在“机器人主任务”里直接放箱的逻辑，改为下面这种等待命令的结构。抓取、拍照旋转、夹爪动作点位按你现场已有工程接入。

```text
机器人主任务:
  MoveJ 原料区_扫码路点
  抓取当前物体
  catch:=1

  调用 拍照旋转

等待动作命令:
  Server(action_signal:=接收到的值)       // 10000，PC -> 机械臂
  action_name:=parse_action(action_signal)

  If action_name ?= "CMD_BUFFER_HELD":
    // CMD_BUFFER_HELD：当前夹持物放暂存
    Server(buffer_str:=接收到的值)        // 8056，格式 seq,drop
    buffer_seq:=get_buffer_seq(buffer_str)
    buffer_drop_position:=get_buffer_drop_position(buffer_str)

    MoveJ 暂存原点
    调用 测放暂存
    catch:=0

    Server(发送数据:="EVT_ACTION_DONE\n") // 15000，机械臂 -> PC
    Goto 等待动作命令
  End If

  If action_name ?= "CMD_PLACE_HELD":
    // CMD_PLACE_HELD：当前夹持物直接装箱
    seq_text:=get_field(action_signal,"seq")
    If seq_text != "":
      box_target_index:=int(seq_text)-1
    Else:
      box_target_index:=var_index
    End If

    box_place_position:=get_position_by_index(下放坐标,box_target_index)

    If is_swap_pose(action_signal):
      MoveL 长短边转换姿态
    End If

    MoveJ 装箱原点
    调用 测放装箱
    catch:=0
    var_index:=box_target_index+1

    Server(发送数据:="EVT_ACTION_DONE\n")
    Goto 等待动作命令
  End If

  If action_name ?= "CMD_TAKE_BUFFER":
    // CMD_TAKE_BUFFER：从暂存取出指定物体，再装箱
    Server(buffer_str:=接收到的值)        // 8056，格式 seq,drop
    buffer_seq:=get_buffer_seq(buffer_str)
    buffer_slot_position:=get_buffer_drop_position(buffer_str)

    MoveJ 暂存原点
    调用 暂存抓取
    catch:=1

    box_target_index:=buffer_seq-1
    box_place_position:=get_position_by_index(下放坐标,box_target_index)

    If is_swap_pose(action_signal):
      MoveL 长短边转换姿态
    End If

    MoveJ 装箱原点
    调用 测放装箱
    catch:=0
    var_index:=box_target_index+1

    Server(发送数据:="EVT_ACTION_DONE\n")
    Goto 等待动作命令
  End If

  If action_name ?= "CMD_PICK_SCAN":
    // CMD_PICK_SCAN：本轮动作链结束，回去抓下一件
    拍照:=1
    MoveJ 原料区_扫码路点
    Goto 机器人主任务
  End If
```

## 子任务 1：测放装箱

这就是你截图里第 9-23 行的原装箱动作，只把 `var_current_position` 改成 `box_place_position`。

```text
测放装箱:
  箱子上方原点tcp:=get_actual_tcp_pose()
  箱子上方XY偏移:=[box_place_position[0],box_place_position[1],0,0,0,0]
  偏移 位姿: 装箱前在上方xy偏移(偏移自 "箱子上方原点tcp")
  MoveL 装箱前在上方xy偏移

  装箱下放前tcp:=get_actual_tcp_pose()
  装箱下放点位偏移:=[0,0,box_place_position[2],0,0,0]
  偏移 位姿: 物体实际装箱位置(偏移自 "装箱下放前tcp")
  MoveL 物体实际装箱位置

  catch:=0
  MoveJ 装箱下放前tcp
```

## 子任务 2：测放暂存

从 `测放装箱` 复制出来，原点和变量换成暂存下放专用。

```text
测放暂存:
  暂存上方原点tcp:=get_actual_tcp_pose()
  暂存上方XY偏移:=[buffer_drop_position[0],buffer_drop_position[1],0,0,0,0]
  偏移 位姿: 暂存前在上方xy偏移(偏移自 "暂存上方原点tcp")
  MoveL 暂存前在上方xy偏移

  暂存下放前tcp:=get_actual_tcp_pose()
  暂存下放点位偏移:=[0,0,buffer_drop_position[2],0,0,0]
  偏移 位姿: 物体实际暂存位置(偏移自 "暂存下放前tcp")
  MoveL 物体实际暂存位置

  catch:=0
  MoveJ 暂存下放前tcp
```

## 子任务 3：暂存抓取

从暂存取物只需要暂存槽位 XY。Z 使用现场测得的 `buffer_pick_z_offset`，不从 PC 传。

```text
暂存抓取:
  暂存抓取上方原点tcp:=get_actual_tcp_pose()
  暂存抓取XY偏移:=[buffer_slot_position[0],buffer_slot_position[1],0,0,0,0]
  偏移 位姿: 暂存抓取上方xy偏移(偏移自 "暂存抓取上方原点tcp")
  MoveL 暂存抓取上方xy偏移

  暂存抓取前tcp:=get_actual_tcp_pose()
  暂存抓取下探偏移:=[0,0,buffer_pick_z_offset,0,0,0]
  偏移 位姿: 物体实际暂存抓取位置(偏移自 "暂存抓取前tcp")
  MoveL 物体实际暂存抓取位置

  catch:=1
  MoveJ 暂存抓取前tcp
```

## BAC 时序

```text
B:
  PC 先发 8056: 2,drop
  PC 再发 10000: CMD_BUFFER_HELD|seq=2|box=...
  机械臂放 B 到暂存，回 EVT_ACTION_DONE

A:
  PC 发 10000: CMD_PLACE_HELD|seq=1|box=...|pose=...
  机械臂装 A，回 EVT_ACTION_DONE

B 回取:
  PC 先发 8056: 2,drop
  PC 再发 10000: CMD_TAKE_BUFFER|seq=2|box=...|pose=...
  机械臂用 drop 的 XY 和现场抓取 Z 从暂存取 B，再装 B，回 EVT_ACTION_DONE

继续:
  PC 发 10000: CMD_PICK_SCAN
  机械臂回原料区抓下一件
```

关键点：`CMD_TAKE_BUFFER` 自己已经带 `pose`，示教器不要再等第二条 `6/7`。
