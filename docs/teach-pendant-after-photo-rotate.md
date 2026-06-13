# 示教器主流程（从「调用拍照旋转」之后）

行号仅作编排参考，接入你现有工程时顺延即可。  
**拍照旋转**子任务保持不动；主任务在 **调用 拍照旋转** 返回后接下文。  
每轮结束：**`拍照:=1`** → `MoveJ 原料区_扫码路点`。

---

## 参考：拍照旋转（子任务，已有，勿改）

```text
58  拍照旋转
59    MoveJ  路点_2
60    Server (发送数据: 0)
61    Server (issucceed:=接收到的值)
62    var_17:=issucceed.split('\n')[0].strip()
63    循环  var_17 != '1'
64      MoveL  路点_4
65      Server (issucceed:=接收到的值)
66      var_17:=issucceed.split('\n')[0].strip()
67      If  var_17 ?= '2'
68        MoveL  路点_5
69        Server (issucceed:=接收到的值)
70        var_17:=issucceed.split('\n')[0].strip()
71      End If
72    End 循环
73    Server (发送数据: 3)
```

---

## 主任务（接在「调用 拍照旋转」之后）

```text
…（前略：MoveJ 箱子上方 → 抓取 → catch:=1 → 向上偏移）…

57  调用  拍照旋转

58  Server (action_signal:=接收到的值)          // 端口 10000，等 PC 发 6/7/8/9/10
59  脚本: parse_action.script                   // 本轮只调用一次，var_action 为 int
60  var_action:=parse_action(action_signal)      // 6/7/8/9/10，未识别为 0

61  If  var_action = 8                           // ── 信号8：放暂存台 ──
62    Server (buffer_str:=接收到的值)            // 端口 8056，例：2,[0.25,0.04,-0.42,0,0,0]
63    脚本: strtobuffer.script
64    buffer_seq:=get_buffer_seq(buffer_str)
65    var_current_position:=get_var_current_position(buffer_str)
66    MoveJ  暂存原点
67    调用  测放暂存
68    catch:=0
69    Server (next_signal:=接收到的值)            // 等信号 10，直接整型比较，不再调脚本
70    next_signal:=int(next_signal)
71    拍照:=1
72    MoveJ  原料区_扫码路点
73  End If

74  ElseIf  var_action = 6 或 var_action = 7      // ── 信号6/7：装箱（正放侧放合一）──
75    If  var_action = 7
76      MoveL  长短边转换姿态                      // 仅信号7；信号6跳过
78    End If
79    脚本: strtoshuzu.script
80    var_current_position:=get_position_by_index(下放坐标, var_index)
81    MoveJ  装箱原点
82    调用  测放装箱
83    catch:=0
84    var_index:=var_index+1
85    Server (next_signal:=接收到的值)            // PC 可能紧接发 9
86    next_signal:=int(next_signal)
87    If  next_signal = 9                          // ── 信号9：从暂存取回装箱 ──
88      Server (buffer_str:=接收到的值)
90      脚本: strtobuffer.script
91      buffer_seq:=get_buffer_seq(buffer_str)
92      var_current_position:=get_var_current_position(buffer_str)
93      MoveJ  暂存原点
94      调用  暂存抓取
95      脚本: strtoshuzu.script
96      var_current_position:=get_position_by_index(下放坐标, buffer_seq-1)
97      Server (pose_signal:=接收到的值)          // PC 对回取件再发 6 或 7
98      pose_signal:=int(pose_signal)
99      If  pose_signal = 7
100       MoveL  长短边转换姿态
101     End If
102     MoveJ  装箱原点
103     调用  测放装箱
104     catch:=0
105     var_index:=var_index+1
106   End If
107   Server (next_signal:=接收到的值)          // 等信号 10
108   next_signal:=int(next_signal)
109   拍照:=1
110   MoveJ  原料区_扫码路点
111 End If

112 Else
113   弹出窗口: 未知动作信号
114   拍照:=1
115   MoveJ  原料区_扫码路点
116 End If
```

---

## 子任务：测放装箱（信号6/7 正放侧放共用）

```text
200  测放装箱
201    偏移位姿:=var_2(偏移自 '装箱原点')
202    箱子上方原点tcp:=get_actual_tcp_pose()
203    箱子上方XY偏移:=[var_current_position[0], var_current_position[1], 0, 0, 0, 0]
204    偏移位姿:=装箱前在上方XY偏移(偏移自 '箱子上方原点tcp')
205    MoveL  箱子上方XY偏移
206    下放前tcp:=get_actual_tcp_pose()
207    下放点位:=[0, 0, var_current_position[2], 0, 0, 0]
208    偏移位姿:=物体装箱下放(偏移自 '下放前tcp')
209    MoveL  物体装箱下放
210    偏移位姿:=物体装箱后上抬(偏移自 '下放前tcp')
211    MoveL  回到箱子上方
```

---

## 子任务：测放暂存（信号8）

```text
300  测放暂存
301    偏移位姿:=var_2(偏移自 '暂存原点')
302    台面上方原点tcp:=get_actual_tcp_pose()
303    台面上方XY偏移:=[var_current_position[0], var_current_position[1], 0, 0, 0, 0]
304    偏移位姿:=暂存前在上方XY偏移(偏移自 '台面上方原点tcp')
305    MoveL  台面上方XY偏移
306    下放前tcp:=get_actual_tcp_pose()
307    下放点位:=[0, 0, var_current_position[2], 0, 0, 0]
308    偏移位姿:=物体暂存下放(偏移自 '下放前tcp')
309    MoveL  物体暂存下放
310    偏移位姿:=物体暂存后上抬(偏移自 '下放前tcp')
311    MoveL  回到台面上方
```

---

## 子任务：暂存抓取（信号9 前半段）

```text
400  暂存抓取
401    偏移位姿:=var_2(偏移自 '暂存原点')
402    台面上方原点tcp:=get_actual_tcp_pose()
403    台面上方XY偏移:=[var_current_position[0], var_current_position[1], 0, 0, 0, 0]
404    偏移位姿:=暂存前在上方XY偏移(偏移自 '台面上方原点tcp')
405    MoveL  台面上方XY偏移
406    下放前tcp:=get_actual_tcp_pose()
407    下探点位:=[0, 0, var_current_position[2], 0, 0, 0]
408    偏移位姿:=暂存下探抓取(偏移自 '下放前tcp')
409    MoveL  暂存下探抓取
410    catch:=1
411    偏移位姿:=暂存抓取后上抬(偏移自 '下放前tcp')
412    MoveL  回到台面上方
```

---

## 脚本 parse_action.script（新建，勿用 1.script）

仓库源文件：`parse_action.py`（拷到示教器后命名为 `parse_action.script`）。

```python
def parse_action(text):
    s = str(text).strip()
    if '10' in s:
        return 10
    if '8' in s:
        return 8
    if '9' in s:
        return 9
    if '7' in s:
        return 7
    if '6' in s:
        return 6
    return 0
```

示教器调用：

```text
Server (action_signal:=接收到的值)
脚本: parse_action.script
var_action:=parse_action(action_signal)
```

---

## 脚本 strtobuffer.script（8056 单行）

输入示例：`2,[0.25,0.04,-0.42,0,0,0]`

```text
Server (buffer_str:=接收到的值)
脚本: strtobuffer.script
buffer_seq:=get_buffer_seq(buffer_str)
var_current_position:=get_var_current_position(buffer_str)
```

`get_var_current_position` 返回 `[X, Y, Z]`（米），直接赋给 `var_current_position`，接入测放暂存/测放装箱。

---

## BAC 单轮时序

| 扫码 | PC 信号 | 示教器走行 |
|------|---------|------------|
| B | 8 + 8056 | 行61-73 |
| A | 6/7，再 9 + 8056 | 行75-104 |
| C | 6/7 | 行75-109（无内层88-104） |
