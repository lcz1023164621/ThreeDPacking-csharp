def parse_action(text):
    """
    解析 PC 经端口 10000 下发的动作信号字符串。

    返回值: 6 / 7 / 8 / 9 / 10（int），未识别返回 0。
    """
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
