# Elite CS612 完整示教器流程（无嵌套子任务版）

本版本按你的示教器限制编写：**子任务中不调用子任务**。  
只有“机器人主任务”可以调用子任务；子任务内部只保留动作、赋值、Socket 收发、Return，不再调用其它子任务。

当前协议：

- 8055：PC 下发整批箱内下放坐标，格式由 `strtoshuzu.script` 解析。
- 8056：PC 下发暂存坐标，格式为 `seq,drop`，不包含 pick。
- 10000：PC 下发字符串命令。
- 15000：机械臂回传字符串事件。

## 一、开始前

```text
1  初始化变量
2    position:=""
3    下放坐标:=""
4    var_index:=0
5    var_count:=0
6    action_signal:=""
7    action_name:=""
8    scan_result:=""
9    buffer_str:=""
10   buffer_seq:=0
11   box_target_index:=0
12   seq_text:=""
13   box_place_position:=[0,0,0,0,0,0]
14   buffer_drop_position:=[0,0,0]
15   buffer_slot_position:=[0,0,0]
16   buffer_pick_z_offset:=现场测得的暂存抓取Z偏移
17   catch:=0
18   拍照:=1
19   action_matched:=0

20 开始前
21   Socket 8055 接收 position
22   脚本: strtoshuzu.script
23   脚本: strtobuffer.script
24   脚本: parse_action.script
25   下放坐标:=position
26   var_count:=get_position_count(下放坐标)
27   var_index:=0
```

## 二、机器人主任务

下面第 2-13 行保留你的固定 3D 相机和吸盘抓取流程：

- `运行工程`：启动 3D 相机并返回抓取坐标。
- `alson_State ?= 402`：相机正常返回。
- `alson_number ≠ 0`：当前识别到的待抓物体数量。
- `处理坐标`：把 3D 相机坐标适配成机械臂坐标。
- `抓取`：移动到处理后的抓取位置。
- `catch:=1`：吸盘 IO 抽真空。

新增逻辑从第 14 行开始。

```text
1  机器人主任务
2    运行工程
3    If alson_State ?= 402
4      循环 alson_number ≠ 0
5        处理坐标
6        MoveJ
7          箱子上方
8        抓取
9        catch:=1
10       抓取后tcp:=get_actual_tcp_pose()
11       偏移 位姿: 向上偏移(偏移自 "抓取后tcp")
12       MoveL
13         向上偏移

14       调用 子任务_拍照旋转

15       action_name:=""
16       循环 action_name != "CMD_PICK_SCAN"
17         Socket 10000 接收 action_signal
18         action_name:=parse_action(action_signal)
19         action_matched:=0

20         If action_name ?= "CMD_BUFFER_HELD"
21           action_matched:=1
22           Socket 8056 接收 buffer_str
23           buffer_seq:=get_buffer_seq(buffer_str)
24           buffer_drop_position:=get_buffer_drop_position(buffer_str)
25           MoveJ 暂存原点
26           调用 子任务_测放暂存
27           catch:=0
28           Socket 15000 发送 "EVT_ACTION_DONE\n"
29         End If

30         If action_name ?= "CMD_PLACE_HELD"
31           action_matched:=1
32           seq_text:=get_field(action_signal,"seq")
33           If seq_text != ""
34             box_target_index:=int(seq_text)-1
35           End If
36           If seq_text = ""
37             box_target_index:=var_index
38           End If
39           box_place_position:=get_position_by_index(下放坐标,box_target_index)
40           If is_swap_pose(action_signal)
41             调用 子任务_长短边转换姿态
42           End If
43           MoveJ 装箱原点
44           调用 子任务_测放装箱
45           catch:=0
46           var_index:=box_target_index+1
47           Socket 15000 发送 "EVT_ACTION_DONE\n"
48         End If

49         If action_name ?= "CMD_TAKE_BUFFER"
50           action_matched:=1
51           Socket 8056 接收 buffer_str
52           buffer_seq:=get_buffer_seq(buffer_str)
53           buffer_slot_position:=get_buffer_drop_position(buffer_str)
54           MoveJ 暂存原点
55           调用 子任务_暂存抓取
56           catch:=1

57           box_target_index:=buffer_seq-1
58           box_place_position:=get_position_by_index(下放坐标,box_target_index)

59           If is_swap_pose(action_signal)
60             调用 子任务_长短边转换姿态
61           End If

62           MoveJ 装箱原点
63           调用 子任务_测放装箱
64           catch:=0
65           var_index:=box_target_index+1
66           Socket 15000 发送 "EVT_ACTION_DONE\n"
67         End If

68         If action_name ?= "CMD_PICK_SCAN"
69           action_matched:=1
71         End If

72         If action_matched = 0
73           弹出窗口: 未知动作命令 action_signal
74           Socket 15000 发送 "EVT_ACTION_DONE\n"
75         End If
76       End 循环

77       拍照:=1
78     End 循环
79   End If
```

旧程序中 `var_current_position:=get_position_by_index(下放坐标,var_index)` 是装箱部分提前取箱内下放点。新流程不要在主抓取段提前取箱内点，因为当前抓到的物体顺序要等扫码后由 PC 下发 `seq` 才确定。

## 三、子任务：拍照旋转

此子任务内部不调用其它子任务。  
如果你已有拍照旋转流程，可以保留动作点，只把数字 `0/1/2/3/5` 改成下面这些字符串收发。

```text
1  子任务_拍照旋转
2    scan_result:=""
3    扫码成功:=0

4    循环 扫码成功 = 0
5      MoveJ 扫码拍照位_1
6      Socket 15000 发送 "EVT_SCAN_READY\n"
7      Socket 10000 接收 scan_result

8      If scan_result ?= "VISION_OK"
9        Socket 15000 发送 "EVT_ROBOT_ACK\n"
10       扫码成功:=1
11     End If

12     If 扫码成功 = 0
13       MoveL 扫码拍照位_2
14       Socket 15000 发送 "EVT_SCAN_READY\n"
15       Socket 10000 接收 scan_result
16     End If

17     If scan_result ?= "VISION_OK"
18       Socket 15000 发送 "EVT_ROBOT_ACK\n"
19       扫码成功:=1
20     End If

21     If 扫码成功 = 0
22       MoveL 扫码拍照位_3
23       Socket 15000 发送 "EVT_SCAN_READY\n"
24       Socket 10000 接收 scan_result
25     End If

26     If scan_result ?= "VISION_OK"
27       Socket 15000 发送 "EVT_ROBOT_ACK\n"
28       扫码成功:=1
29     End If

30     If 扫码成功 = 0
31       Socket 15000 发送 "EVT_SCAN_FAIL_ACK\n"
32     End If
33   End 循环

34   Return
```

## 四、子任务：测放装箱

此子任务只负责把当前吸盘夹持物放入箱内，不调用其它子任务。

```text
1  子任务_测放装箱
2    箱子上方原点tcp:=get_actual_tcp_pose()
3    箱子上方XY偏移:=[box_place_position[0],box_place_position[1],0,0,0,0]
4    偏移 位姿: 装箱前在上方xy偏移(偏移自 "箱子上方原点tcp")
5    MoveL 装箱前在上方xy偏移

6    装箱下放前tcp:=get_actual_tcp_pose()
7    装箱下放点位偏移:=[0,0,box_place_position[2],0,0,0]
8    偏移 位姿: 物体实际装箱位置(偏移自 "装箱下放前tcp")
9    MoveL 物体实际装箱位置

10   catch:=0
11   MoveJ 装箱下放前tcp
12   Return
```

## 五、子任务：测放暂存

此子任务只负责把当前吸盘夹持物放到暂存台，不调用其它子任务。

```text
1  子任务_测放暂存
2    暂存上方原点tcp:=get_actual_tcp_pose()
3    暂存上方XY偏移:=[buffer_drop_position[0],buffer_drop_position[1],0,0,0,0]
4    偏移 位姿: 暂存前在上方xy偏移(偏移自 "暂存上方原点tcp")
5    MoveL 暂存前在上方xy偏移

6    暂存下放前tcp:=get_actual_tcp_pose()
7    暂存下放点位偏移:=[0,0,buffer_drop_position[2],0,0,0]
8    偏移 位姿: 物体实际暂存位置(偏移自 "暂存下放前tcp")
9    MoveL 物体实际暂存位置

10   catch:=0
11   MoveJ 暂存下放前tcp
12   Return
```

## 六、子任务：暂存抓取

此子任务只负责从暂存台吸取物体，不调用其它子任务。  
8056 只给 `seq,drop`，所以这里用 `buffer_slot_position` 的 XY，Z 用现场测得的 `buffer_pick_z_offset`。

```text
1  子任务_暂存抓取
2    暂存抓取上方原点tcp:=get_actual_tcp_pose()
3    暂存抓取XY偏移:=[buffer_slot_position[0],buffer_slot_position[1],0,0,0,0]
4    偏移 位姿: 暂存抓取上方xy偏移(偏移自 "暂存抓取上方原点tcp")
5    MoveL 暂存抓取上方xy偏移

6    暂存抓取前tcp:=get_actual_tcp_pose()
7    暂存抓取下探偏移:=[0,0,buffer_pick_z_offset,0,0,0]
8    偏移 位姿: 物体实际暂存抓取位置(偏移自 "暂存抓取前tcp")
9    MoveL 物体实际暂存抓取位置

10   catch:=1
11   MoveJ 暂存抓取前tcp
12   Return
```

## 七、子任务：长短边转换姿态

此子任务只负责姿态转换，不调用其它子任务。

```text
1  子任务_长短边转换姿态
2    MoveJ 长短边转换_过渡点
3    MoveL 长短边转换_姿态点
4    Return
```

## 八、BAC 对照

```text
1  抓到 B
2    PC 8056 发送: 2,[暂存X,暂存Y,暂存下放Z,0,0,0]
3    PC 10000 发送: CMD_BUFFER_HELD|seq=2|box=...
4    主任务 CMD_BUFFER_HELD 分支执行:
5      接收 8056
6      调用 子任务_测放暂存
7      发送 EVT_ACTION_DONE

8  抓到 A
9    PC 10000 发送: CMD_PLACE_HELD|seq=1|box=...|pose=DIRECT 或 SWAP_LONG_SHORT
10   主任务 CMD_PLACE_HELD 分支执行:
11     必要时调用 子任务_长短边转换姿态
12     调用 子任务_测放装箱
13     发送 EVT_ACTION_DONE

14 回取 B
15   PC 8056 发送: 2,[暂存X,暂存Y,暂存下放Z,0,0,0]
16   PC 10000 发送: CMD_TAKE_BUFFER|seq=2|box=...|pose=DIRECT 或 SWAP_LONG_SHORT
17   主任务 CMD_TAKE_BUFFER 分支执行:
18     接收 8056
19     调用 子任务_暂存抓取
20     必要时调用 子任务_长短边转换姿态
21     调用 子任务_测放装箱
22     发送 EVT_ACTION_DONE

23 继续下一件
24   PC 10000 发送: CMD_PICK_SCAN
25   主任务 CMD_PICK_SCAN 分支执行:
26     action_name 已经是 "CMD_PICK_SCAN"，动作链循环结束
```
