# -*- coding: utf-8 -*-
"""
Created on Sun Mar 30 17:19:34 2025

@author: Mamiya
"""
import pandas as pd
from xml.etree.ElementTree import Element, SubElement, tostring
from xml.dom import minidom
import openpyxl
import os
import json
def main():
    config = load_config()
    # 持久化运行核心逻辑
    while True:  # 增加主循环
        try:
            print("\n=== Excel转XML工具 ===")
            print("1. 开始转换")
            print("2. 退出程序")
            choice = input("请选择操作 (1/2): ").strip()

            if choice == '1':
                # 文件处理流程
                last_file = config.get('last_input_file_path')
                if last_file:
                    # use_last = input(f"是否使用上次的文件路径 '{last_file}'? (y/n): ").strip().lower()
                    # if use_last == 'y'
                    #     file_dir = last_file
                    # else:
                    #     file_dir = input("请输入xlsx文件路径: ").strip()
                    file_dir = input(f"请输入xlsx文件路径(默认为上次的路径{last_file}):").strip() or last_file
                else:
                    file_dir = input("请输入xlsx文件路径: ").strip()
                dir_path = os.path.dirname(file_dir)
                # 提取文件名（带扩展名） → "FirstScript.xlsx"
                full_name = os.path.basename(file_dir)  
                # 分割文件名和扩展名 → ("FirstScript", ".xlsx")
                base_name = os.path.splitext(full_name)[0] 
                #default_output = f"{base_name}.xml"
                default_output = config.get('last_output_file_path')
                if default_output:
                    output_name = input(f"请输入输出文件名(默认为上次的路径{default_output}): ").strip() or default_output
                else:
                    _default_output = f"{base_name}.xml"
                    output_name = input(f"请输入输出文件名(默认为{_default_output}): ").strip() or _default_output
                #output_name = input(f"请输入输出文件名（默认{default_output}）: ").strip() or default_output
                
                excel_to_xml(file_dir, output_name)
                print(f"✓ 转换成功，文件已保存为 {output_name}")
                
                # 保存当前文件路径到配置
                config['last_input_file_path'] = file_dir
                config['last_output_file_path'] = output_name
                save_config(config)
            elif choice == '2':
                print("程序退出中...")
                break  # 退出循环
                
            else:
                print("无效输入，请重新选择")
                
        except Exception as e:
            print(f"× 发生错误: {str(e)}")
            # 可添加错误日志记录
            # with open("error.log", "a") as f:
            #     f.write(f"{datetime.now()} - {str(e)}\n")
            
        # 添加操作间隔
        input("\n按 Enter 继续...")  
        os.system('pause')
        # 可选清屏操作（跨平台）
        # os.system('cls' if os.name == 'nt' else 'clear')
# 定义节点类型及其参数映射
NODE_DICT = {
    'AddCharacter': {'Arg1': 'CharacterID', 'Arg2': 'From', 'Arg3': 'SendMessage'},
    'Speak': {'Arg1': 'CharacterID', 'Arg2': 'Content', 'Arg3': 'AudioPath', 'Arg4': 'TextType', 'Arg5': 'Skip'},
    'LongSpeak': {'Arg1': 'Continue', 'Arg2': 'Content', 'Arg3': 'End', 'Arg4': 'TextType', 'Arg5':'Skip', 'Arg6':'AudioPath'},
    'Choice': {'Arg1': 'JumpID', 'Arg2': 'Content', 'Arg4': 'TextType'},
    'NextScript': {'Arg2': 'NextScriptName'},
    'SetBGM': {'Arg3': 'BGMName'},
    'ChangeBackImg':{'Arg3': 'BackImgName'},
    'CharacterAnimate':{'Arg1': 'CharacterID', 'Arg3': 'SendMessage'},
    'DeleteCharacter':{'Arg1': 'CharacterID'},
    'CharacterPortrait':{'Arg1': 'CharacterID', 'Arg2': 'CharacterPortrait'}
}
def get_script_dir():
    """获取脚本所在的目录路径"""
    return os.path.dirname(os.path.abspath(__name__))

# 修改配置路径到脚本目录
def load_config():
    """加载配置文件"""
    config_path = os.path.join(get_script_dir(), "excel_to_xml_config.json")
    if os.path.exists(config_path):
        try:
            with open(config_path, 'r', encoding='utf-8') as f:
                return json.load(f)
        except Exception as e:
            print(f"配置文件损坏，将创建新配置: {str(e)}")
    return {}

def save_config(config):
    """保存配置文件"""
    config_path = os.path.join(get_script_dir(), "excel_to_xml_config.json")
    with open(config_path, 'w', encoding='utf-8') as f:
        json.dump(config, f, indent=2)
def create_node(parent, node_type, row):
    """根据节点类型和数据行创建XML子节点"""
    node = SubElement(parent, node_type)
    for key, attr_name in NODE_DICT[node_type].items():
        if pd.notna(row[key]):
            try:
                node.set(attr_name, str(int(row[key])))
            except:
                node.set(attr_name, str(row[key]))
    return node

def process_main_plot(main_plot, df):
    """处理主剧情脚本"""
    current_speak = None
    count = 0
    branch_list = []

    for _, row in df.iterrows():
        node_type = row['类型']

        if node_type in ['AddCharacter', 'Speak', 'LongSpeak', 
            'ChangeBackImg', 'NextScript', 'SetBGM','ChangeBackImg',
            'CharacterAnimate','DeleteCharacter','CharacterPortrait']:
            node = create_node(main_plot, node_type, row)
            if node_type in ['Speak', 'LongSpeak']:
                current_speak = node
                node.set('Id', str(count))
                count += 1
        elif node_type == 'Choice' and current_speak is not None:
            option_text = str(row['Arg2'])
            choice = SubElement(current_speak, 'Choice')
            choice.set(NODE_DICT['Choice']['Arg1'], str(row['Arg1']))
            choice.set(NODE_DICT['Choice']['Arg4'], str(row['Arg4']))
            choice.text = option_text
            branch_list.append(str(row['Arg1']))


    return branch_list

def process_branch_plot(branch_plot, branch_id, excel_path):
    """处理分支剧情脚本"""
    df = pd.read_excel(excel_path, sheet_name=branch_id)
    branch_node = SubElement(branch_plot, 'BranchPlotNode')
    branch_node.set('ID', branch_id)

    count = 0
    for _, row in df.iterrows():
        node_type = row['类型']

        if node_type in ['AddCharacter', 'Speak', 'LongSpeak', 'ChangeBackImg', 'NextScript', 'SetBGM']:
            node = create_node(branch_node, node_type, row)
            if node_type in ['Speak', 'LongSpeak']:
                node.set('Id', f"{branch_id}-{count}")
                count += 1
        


def excel_to_xml(excel_path, output_path):
    """将Excel剧本转换为XML格式"""
    # 读取Excel数据
    script_df = pd.read_excel(excel_path, sheet_name='剧情脚本')
    info_df = pd.read_excel(excel_path, sheet_name='章节信息')

    # 创建XML根节点
    root = Element('data')
    
    # 添加标题和简介
    title = SubElement(root, 'title')
    title.text = info_df.iloc[0]['标题']
    synopsis = SubElement(root, 'Synopsis')
    synopsis.text = info_df.iloc[0]['简介']
    
    # 创建主分支和分支结构
    branch_plot = SubElement(root, 'BranchPlot')
    main_plot = SubElement(root, 'MainPlot')

    # 处理主剧情脚本并获取分支列表
    branch_list = process_main_plot(main_plot, script_df)

    # 处理分支剧情脚本
    for branch_id in branch_list:
        process_branch_plot(branch_plot, branch_id, excel_path)

    # 美化输出
    xml_str = minidom.parseString(tostring(root)).toprettyxml(encoding='utf-8')
    
    with open(output_path, 'wb') as f:
        f.write(xml_str)
if __name__ == "__main__":
    main()
