# 示教器主装箱流程（字符串协议）

本文档对应 PC 端扫码、装箱、暂存、回取暂存的协调逻辑，供 Elite CS612 示教器程序实现参考。新协议不再使用 `1/2/6/7/8/9/10` 作为 PC 下发标志位，统一改为字符串。

## 通信端口

| 服务 | 端口 | 方向 | 说明 |
|------|------|------|------|
| 相机扫码 | 8000 | 相机/PC | 扫码器连接端口，来自扫码器设置默认值 |
| 装箱坐标 | 8055 | PC -> 臂 | 批次开始时 PC 一次性下发全部箱内下放点位 |
| 暂存坐标 | 8056 | PC -> 臂 | 暂存或回取暂存前，PC 下发一条 `seq,drop` 坐标 |
| 动作命令 | 10000 | PC -> 臂 | PC 下发扫码结果、装箱、暂存、回取、继续抓取命令 |
| 状态事件 | 15000 | 臂 -> PC | 机械臂上报到位、确认、动作完成、批次完成等事件 |

## PC -> 机械臂（10000）

每条字符串建议以换行结尾，避免 TCP 粘包后不好解析。

| 字符串 | 含义 |
|--------|------|
| `VISION_OK` | 扫码成功，可以继续当前抓取后的动作判断 |
| `VISION_RETRY` | 本次未扫到，需要进入拍照旋转/换姿态重试 |
| `CMD_PLACE_HELD\|seq=1\|box=...\|pose=DIRECT` | 当前已抓物体直接装箱，不需要长短边转换 |
| `CMD_PLACE_HELD\|seq=1\|box=...\|pose=SWAP_LONG_SHORT` | 当前已抓物体直接装箱，但装箱前需要长短边转换 |
| `CMD_BUFFER_HELD\|seq=2\|box=...` | 当前已抓物体放到暂存台 |
| `CMD_TAKE_BUFFER\|seq=2\|box=...\|pose=DIRECT` | 从暂存台取出 `seq=2` 的物体并装箱，不需要长短边转换 |
| `CMD_TAKE_BUFFER\|seq=2\|box=...\|pose=SWAP_LONG_SHORT` | 从暂存台取出 `seq=2` 的物体并装箱，装箱前需要长短边转换 |
| `CMD_PICK_SCAN` | 本轮动作链结束，回原料区抓下一件并扫码 |

重点：`CMD_TAKE_BUFFER` 已经同时表达“从暂存台取”和“装箱姿态”。示教器收到它后不要再等待第二条 `6/7` 姿态信号。

## 机械臂 -> PC（15000）

推荐同样用字符串事件。C# 端仍兼容旧数字，但现场新程序建议只发字符串。

| 字符串 | 兼容旧值 | 含义 |
|--------|----------|------|
| `EVT_SCAN_READY` | 0 | 机械臂到扫码位，请求 PC 扫码 |
| `EVT_ROBOT_ACK` | 3 | 机械臂确认已收到 `VISION_OK`，PC 可以提交本件扫码记录 |
| `EVT_SCAN_FAIL_ACK` | 5 | 机械臂确认已收到 `VISION_RETRY` |
| `EVT_ACTION_DONE` | 11 | 当前放置/暂存/回取动作完成，请求 PC 下发动作链下一条 |
| `EVT_BATCH_DONE` | 4 | 本批次结束 |

## 8056 暂存坐标格式

```
{装箱顺序},{暂存下放点位}
```

示例：

```text
2,[0.2,0.04,-0.42,0,0,0]
```

- 单位：米，与 8055 一致。
- `drop` 是相对暂存台坐标系原点的偏移。
- `CMD_BUFFER_HELD`：示教器读取 8056 后，移动到暂存 `drop` 点放下当前已抓物体。
- `CMD_TAKE_BUFFER`：示教器读取 8056 后，使用 `drop` 的 XY 定位暂存槽位；抓取 Z 由示教器现场测量值/下探偏移控制，再按 8055 中 `box_drop[seq]` 装箱。

PC 端当前实现会先把 8056 坐标写入暂存坐标 socket，再通过 10000 写动作命令；示教器在对应分支里读取 8056 即可。

## 动作链时序

```text
循环直到批次结束:
  MoveJ 原料区_扫码路点
  15000 发送 EVT_SCAN_READY
  10000 等待 VISION_OK 或 VISION_RETRY

  如果收到 VISION_RETRY:
    进入拍照旋转/换姿态
    再次发送 EVT_SCAN_READY
    再次等待 VISION_OK 或 VISION_RETRY

  收到 VISION_OK:
    15000 发送 EVT_ROBOT_ACK
    10000 等待 PC 下发动作命令

  如果命令是 CMD_BUFFER_HELD:
    8056 读取暂存坐标
    将当前已抓物体放到暂存台
    15000 发送 EVT_ACTION_DONE
    10000 等待下一条命令

  如果命令是 CMD_PLACE_HELD:
    根据 pose 判断是否做长短边转换
    将当前已抓物体装入 box_drop[seq]
    15000 发送 EVT_ACTION_DONE
    10000 等待下一条命令

  如果命令是 CMD_TAKE_BUFFER:
    8056 读取暂存坐标
    用暂存坐标 XY 和现场抓取 Z 从暂存台抓取 seq 对应物体
    根据 pose 判断是否做长短边转换
    将物体装入 box_drop[seq]
    15000 发送 EVT_ACTION_DONE
    10000 等待下一条命令

  如果命令是 CMD_PICK_SCAN:
    回原料区，进入下一轮扫码抓取
```

## BAC 示例

目标装箱顺序是 A、B、C，实际抓取顺序是 B、A、C：

| 步骤 | 扫码 | PC 下发 | 示教器动作 |
|------|------|---------|------------|
| 1 | B | 先发 8056 `2,drop`，再发 `CMD_BUFFER_HELD\|seq=2\|box=...` | 把 B 放到暂存台，完成后回 `EVT_ACTION_DONE` |
| 2 | A | `CMD_PLACE_HELD\|seq=1\|box=...\|pose=...` | 把 A 装箱，完成后回 `EVT_ACTION_DONE` |
| 2b | - | 先发 B 的 8056 `2,drop`，再发 `CMD_TAKE_BUFFER\|seq=2\|box=...\|pose=...` | 用暂存 XY 和现场抓取 Z 取 B 并装箱，完成后回 `EVT_ACTION_DONE` |
| 2c | - | `CMD_PICK_SCAN` | 回原料区继续抓取扫码 |
| 3 | C | `CMD_PLACE_HELD\|seq=3\|box=...\|pose=...` | 把 C 装箱 |

这样不会再出现原来的 `9\n6\n` 两段式命令导致示教器只处理 `9` 而没有继续处理姿态的问题。
