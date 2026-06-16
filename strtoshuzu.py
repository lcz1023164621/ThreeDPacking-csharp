import ast

def parse_position(position_str):
    position_str = position_str.strip()
    position_str = position_str.replace('，', ',')

    data = ast.literal_eval(position_str)

    # 多个坐标: "[...],[...]"
    if isinstance(data, tuple):
        data = list(data)

    # 单个坐标: "[0.19,0.19,0,0,0,0]"
    if isinstance(data, list) and len(data) > 0:
        if all(isinstance(x, (int, float)) for x in data):
            data = [data]

    # 统一转成 float
    result = []
    for p in data:
        result.append([float(x) for x in p])

    return result


def get_position_count(position_str):
    """
    返回坐标点数量 
    """
    positions = parse_position(position_str)
    return len(positions)


def get_position_by_index(position_str, index):
    """
    根据索引取第 index 个坐标
    index 从 0 开始
    """
    positions = parse_position(position_str)
    index = int(index)

    return positions[index]