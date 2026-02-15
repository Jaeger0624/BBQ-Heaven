#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Luban Configuration Table Generator
====================================
This script generates and updates Luban-compatible Excel configuration files from JSON schema definitions.

Features:
- Generate new configuration tables from JSON schema
- Update existing tables with new data (incremental mode)
- Add/Update/Delete records in existing tables
- Preserve existing table structure and formatting

Usage:
    python luban_table_generator.py <schema_json_file> [output_dir]
    python luban_table_generator.py --update <schema_json_file> [output_dir]
    python luban_table_generator.py --sample [output_file]

Author: BBQ Game Project
Date: 2026-02-15
"""

import json
import sys
import os
from typing import Dict, List, Any, Optional
from openpyxl import Workbook, load_workbook
from openpyxl.styles import Font, Alignment, PatternFill, Border, Side
from openpyxl.utils import get_column_letter


class LubanTableGenerator:
    """
    Luban配置表生成器
    
    负责将JSON格式的数据结构定义转换为符合Luban规范的Excel配置表
    支持增量更新模式，可以在原表基础上修改数据
    """
    
    def __init__(self, output_dir: str = "Config/Datas", update_mode: bool = False):
        """
        初始化生成器
        
        Args:
            output_dir: 输出目录路径
            update_mode: 是否为更新模式（True=增量更新，False=完全覆盖）
        """
        self.output_dir = output_dir
        self.update_mode = update_mode
        self.beans = []
        self.enums = []
        self.tables = []
        
        # 样式定义
        self.header_font = Font(bold=True, size=11)
        self.header_fill = PatternFill(start_color="D3D3D3", end_color="D3D3D3", fill_type="solid")
        self.comment_fill = PatternFill(start_color="FFFACD", end_color="FFFACD", fill_type="solid")
        self.border = Border(
            left=Side(style='thin'),
            right=Side(style='thin'),
            top=Side(style='thin'),
            bottom=Side(style='thin')
        )
    
    def load_schema(self, schema_file: str) -> bool:
        """
        从JSON文件加载schema定义
        
        Args:
            schema_file: JSON schema文件路径
            
        Returns:
            是否加载成功
        """
        try:
            with open(schema_file, 'r', encoding='utf-8') as f:
                schema = json.load(f)
            
            self.beans = schema.get('beans', [])
            self.enums = schema.get('enums', [])
            self.tables = schema.get('tables', [])
            
            print(f"[LubanGenerator] 成功加载schema: {len(self.beans)} beans, {len(self.enums)} enums, {len(self.tables)} tables")
            return True
            
        except Exception as e:
            print(f"[LubanGenerator] 加载schema失败: {e}")
            return False
    
    def generate_all(self) -> bool:
        """
        生成所有配置文件
        
        Returns:
            是否生成成功
        """
        try:
            # 确保输出目录存在
            os.makedirs(self.output_dir, exist_ok=True)
            
            # 生成beans定义文件
            if self.beans:
                self._generate_beans_file()
            
            # 生成enums定义文件
            if self.enums:
                self._generate_enums_file()
            
            # 生成tables定义文件
            if self.tables:
                self._generate_tables_file()
            
            # 生成各个数据表
            for table in self.tables:
                self._generate_data_table(table)
            
            print(f"[LubanGenerator] 所有配置文件生成完成！")
            return True
            
        except Exception as e:
            print(f"[LubanGenerator] 生成配置失败: {e}")
            import traceback
            traceback.print_exc()
            return False
    
    def _generate_beans_file(self):
        """
        生成 __beans__.xlsx 文件
        
        Luban Bean定义格式：
        - 第1行: ##var | full_name | parent | valueType | sep | alias | comment | group | tags | # | *fields...
        - 第2行: ##var | (空) | ... | ... | ... | ... | ... | ... | ... | ... | name | alias | type | group | comment | tags | variants
        - 第3行: ## | 中文说明行
        - 第4行起: Bean定义，每个bean包含多行（bean声明 + 字段定义）
        """
        wb = Workbook()
        ws = wb.active
        ws.title = "Sheet1"
        
        # 第1行：主标题行
        headers_row1 = ['##var', 'full_name', 'parent', 'valueType', 'sep', 'alias', 
                       'comment', 'group', 'tags', '#', '*fields']
        # 补充字段列标题（最多支持20个字段）
        for i in range(20):
            headers_row1.append(None)
        
        ws.append(headers_row1)
        
        # 第2行：字段属性行
        headers_row2 = ['##var', None, None, None, None, None, None, None, None, None]
        field_headers = ['name', 'alias', 'type', 'group', 'comment', 'tags', 'variants']
        for _ in range(20):
            headers_row2.extend(field_headers if len(headers_row2) == 10 else [None] * 7)
            if len(headers_row2) >= 10 + 20 * 7:
                break
        
        ws.append(headers_row2[:len(headers_row1)])
        
        # 第3行：中文注释行
        comment_row = ['##', '全名(包含模块名)', None, None, '分隔符', None, None, None, None, None,
                      '字段名', '字段别名', '类型', '分组', '注释', None, '字段标签']
        ws.append(comment_row[:len(headers_row1)])
        
        # 添加每个bean的定义
        for bean in self.beans:
            self._add_bean_to_sheet(ws, bean)
        
        # 应用样式
        self._apply_header_style(ws, len(headers_row1))
        
        # 保存文件
        output_path = os.path.join(self.output_dir, "__beans__.xlsx")
        wb.save(output_path)
        print(f"[LubanGenerator] 生成beans文件: {output_path}")
    
    def _add_bean_to_sheet(self, ws, bean: Dict[str, Any]):
        """
        向worksheet添加一个bean定义
        
        Args:
            ws: worksheet对象
            bean: bean定义字典
        """
        # Bean声明行
        bean_row = [None, bean['name'], bean.get('parent'), bean.get('valueType'), 
                   bean.get('sep'), bean.get('alias'), bean.get('comment'),
                   bean.get('group'), bean.get('tags'), None]
        
        # 添加第一个字段（如果有）
        fields = bean.get('fields', [])
        if fields:
            first_field = fields[0]
            bean_row.extend([
                first_field.get('name'),
                first_field.get('alias'),
                first_field.get('type'),
                first_field.get('group'),
                first_field.get('comment'),
                first_field.get('tags'),
                first_field.get('variants')
            ])
        
        ws.append(bean_row)
        
        # 添加剩余字段
        for field in fields[1:]:
            field_row = [None] * 10
            field_row.extend([
                field.get('name'),
                field.get('alias'),
                field.get('type'),
                field.get('group'),
                field.get('comment'),
                field.get('tags'),
                field.get('variants')
            ])
            ws.append(field_row)
        
        # 添加空行分隔
        ws.append([None] * 17)
    
    def _generate_enums_file(self):
        """
        生成 __enums__.xlsx 文件
        
        Luban Enum定义格式：
        - 第1行: ##var | name | alias | comment | tags | *items...
        - 第2行: ##var | ... | ... | ... | ... | name | alias | value | comment | tags
        - 第3行: ## | 中文说明
        - 第4行起: Enum定义
        """
        wb = Workbook()
        ws = wb.active
        ws.title = "Sheet1"
        
        # 第1行：主标题行
        headers_row1 = ['##var', 'name', 'alias', 'comment', 'tags', '*items']
        for i in range(20):
            headers_row1.append(None)
        ws.append(headers_row1)
        
        # 第2行：字段属性行
        headers_row2 = ['##var', None, None, None, None, None]
        item_headers = ['name', 'alias', 'value', 'comment', 'tags']
        for _ in range(20):
            headers_row2.extend(item_headers)
            if len(headers_row2) >= 6 + 20 * 5:
                break
        ws.append(headers_row2[:len(headers_row1)])
        
        # 第3行：中文注释行
        comment_row = ['##', '枚举名', None, '注释', None, '枚举项名', '别名', '值', '注释', None]
        ws.append(comment_row[:len(headers_row1)])
        
        # 添加每个enum的定义
        for enum in self.enums:
            self._add_enum_to_sheet(ws, enum)
        
        # 应用样式
        self._apply_header_style(ws, len(headers_row1))
        
        # 保存文件
        output_path = os.path.join(self.output_dir, "__enums__.xlsx")
        wb.save(output_path)
        print(f"[LubanGenerator] 生成enums文件: {output_path}")
    
    def _add_enum_to_sheet(self, ws, enum: Dict[str, Any]):
        """
        向worksheet添加一个enum定义
        
        Args:
            ws: worksheet对象
            enum: enum定义字典
        """
        # Enum声明行
        enum_row = [None, enum['name'], enum.get('alias'), enum.get('comment'), enum.get('tags'), None]
        
        # 添加第一个枚举项（如果有）
        items = enum.get('items', [])
        if items:
            first_item = items[0]
            enum_row.extend([
                first_item.get('name'),
                first_item.get('alias'),
                first_item.get('value'),
                first_item.get('comment'),
                first_item.get('tags')
            ])
        
        ws.append(enum_row)
        
        # 添加剩余枚举项
        for item in items[1:]:
            item_row = [None] * 5 + [None]
            item_row.extend([
                item.get('name'),
                item.get('alias'),
                item.get('value'),
                item.get('comment'),
                item.get('tags')
            ])
            ws.append(item_row)
        
        # 添加空行分隔
        ws.append([None] * 26)
    
    def _generate_tables_file(self):
        """
        生成 __tables__.xlsx 文件
        
        Luban Table定义格式：
        - 第1行: ##var | name | value | index | mode | group | comment | tags | *inputFiles
        - 第2行: ##var | ... | ... | ... | ... | ... | ... | ... | 文件路径
        - 第3行: ## | 中文说明
        - 第4行起: Table定义
        """
        wb = Workbook()
        ws = wb.active
        ws.title = "Sheet1"
        
        # 第1行：主标题行
        headers_row1 = ['##var', 'name', 'value', 'index', 'mode', 'group', 'comment', 'tags', '*inputFiles']
        ws.append(headers_row1)
        
        # 第2行：字段属性行
        headers_row2 = ['##var', None, None, None, None, None, None, None, '文件路径']
        ws.append(headers_row2)
        
        # 第3行：中文注释行
        comment_row = ['##', '表名', '值类型', '主键', '模式', '分组', '注释', None, '数据文件']
        ws.append(comment_row)
        
        # 添加每个table的定义
        for table in self.tables:
            table_row = [
                None,
                table['name'],
                table.get('value'),
                table.get('index'),
                table.get('mode', 'one'),
                table.get('group'),
                table.get('comment'),
                table.get('tags'),
                table.get('inputFile')
            ]
            ws.append(table_row)
        
        # 应用样式
        self._apply_header_style(ws, len(headers_row1))
        
        # 保存文件
        output_path = os.path.join(self.output_dir, "__tables__.xlsx")
        wb.save(output_path)
        print(f"[LubanGenerator] 生成tables文件: {output_path}")
    
    def _generate_data_table(self, table: Dict[str, Any]):
        """
        生成或更新数据表Excel文件
        
        Args:
            table: table定义字典
        """
        table_name = table['name']
        input_file = table.get('inputFile')
        
        if not input_file:
            print(f"[LubanGenerator] 警告: 表 {table_name} 没有指定inputFile，跳过数据表生成")
            return
        
        # 创建数据表目录
        table_dir = os.path.join(self.output_dir, table_name)
        os.makedirs(table_dir, exist_ok=True)
        
        # 获取数据
        data = table.get('data', [])
        update_operations = table.get('operations', [])  # 增量更新操作
        
        # 获取字段定义
        fields = table.get('fields', [])
        
        # 检查是否存在原表文件
        output_path = os.path.join(table_dir, f"{table_name}.xlsx")
        existing_file = os.path.exists(output_path)
        
        if self.update_mode and existing_file:
            # 更新模式：在原表基础上修改
            self._update_existing_table(output_path, table, data, update_operations)
        else:
            # 生成模式：创建新表
            if not data:
                print(f"[LubanGenerator] 警告: 表 {table_name} 没有数据，生成空表")
            
            if not fields and data:
                # 如果没有字段定义，从第一条数据推断
                fields = [{'name': k, 'type': 'string'} for k in data[0].keys()]
            
            self._create_new_table(output_path, table, data, fields)
    
    def _create_new_table(self, output_path: str, table: Dict[str, Any], data: List[Dict], fields: List[Dict]):
        """
        创建新的数据表
        
        Args:
            output_path: 输出文件路径
            table: table定义
            data: 数据列表
            fields: 字段定义列表
        """
        table_name = table['name']
        wb = Workbook()
        ws = wb.active
        ws.title = table_name
        
        # 第1行：字段名行（##var行）
        header_row = ['##var'] + [f.get('name') for f in fields]
        ws.append(header_row)
        
        # 第2行：字段类型行
        type_row = ['##var'] + [f.get('type', 'string') for f in fields]
        ws.append(type_row)
        
        # 第3行：中文注释行
        comment_row = ['##'] + [f.get('comment', '') for f in fields]
        ws.append(comment_row)
        
        # 添加数据行
        for row_data in data:
            row = []
            for field in fields:
                field_name = field['name']
                value = row_data.get(field_name, '')
                
                # 处理不同类型的值
                if isinstance(value, (list, dict)):
                    # 复杂类型转为JSON字符串或Luban格式
                    value = json.dumps(value, ensure_ascii=False)
                
                row.append(value)
            ws.append(row)
        
        # 应用样式
        self._apply_data_table_style(ws, len(fields) + 1)
        
        # 保存文件
        wb.save(output_path)
        print(f"[LubanGenerator] 生成数据表: {output_path} ({len(data)} 行数据)")
    
    def _update_existing_table(self, output_path: str, table: Dict[str, Any],
                               data: List[Dict], operations: List[Dict]):
        """
        更新现有的数据表
        
        Args:
            output_path: 输出文件路径
            table: table定义
            data: 新增数据列表
            operations: 更新操作列表
        """
        table_name = table['name']
        primary_key = table.get('index', 'ID')  # 主键字段
        
        print(f"[LubanGenerator] 更新表: {table_name}, 主键: {primary_key}")
        print(f"[LubanGenerator] 操作数: {len(operations)}, 新增数据数: {len(data)}")
        
        try:
            # 加载现有Excel文件
            wb = load_workbook(output_path)
            ws = wb.active
            
            # 读取表头信息（第1行）
            header_row = [cell.value for cell in ws[1]]
            field_names = header_row[1:]  # 跳过第一列的 ##var
            
            print(f"[LubanGenerator] 字段列表: {field_names}")
            
            # 读取现有数据到字典（以主键为key）
            existing_data = {}
            for row_idx in range(4, ws.max_row + 1):  # 从第4行开始是数据
                row_values = [cell.value for cell in ws[row_idx]]
                # row_values[0] 是 ##var 列，实际数据从 row_values[1] 开始
                if len(row_values) > 1 and row_values[1]:  # 确保有数据（检查第一个字段）
                    record = {}
                    for i, field_name in enumerate(field_names):
                        # row_values[0] 是 ##var，所以字段从 row_values[1] 开始
                        record[field_name] = row_values[i + 1] if i + 1 < len(row_values) else None
                    
                    pk_value = record.get(primary_key)
                    if pk_value:
                        existing_data[pk_value] = {'row_idx': row_idx, 'data': record}
                        print(f"[LubanGenerator] 读取行 {row_idx}: {primary_key}={pk_value}")
            
            # 处理更新操作
            updated_count = 0
            added_count = 0
            deleted_count = 0
            
            print(f"[LubanGenerator] 现有数据记录数: {len(existing_data)}")
            print(f"[LubanGenerator] 现有数据键: {list(existing_data.keys())}")
            
            # 1. 处理删除操作
            for op in operations:
                if op.get('action') == 'delete':
                    pk_value = op.get('key')
                    print(f"[LubanGenerator] 尝试删除: {pk_value}, 存在: {pk_value in existing_data}")
                    if pk_value in existing_data:
                        # 删除行（通过清空内容）
                        row_idx = existing_data[pk_value]['row_idx']
                        for col_idx in range(1, len(field_names) + 2):
                            ws.cell(row=row_idx, column=col_idx).value = None
                        del existing_data[pk_value]
                        deleted_count += 1
                        print(f"[LubanGenerator] 删除记录: {pk_value}")
            
            # 2. 处理更新操作
            for op in operations:
                if op.get('action') == 'update':
                    pk_value = op.get('key')
                    update_data = op.get('data', {})
                    
                    print(f"[LubanGenerator] 尝试更新: {pk_value}, 存在: {pk_value in existing_data}")
                    if pk_value in existing_data:
                        # 更新现有记录
                        row_idx = existing_data[pk_value]['row_idx']
                        for field_name, value in update_data.items():
                            if field_name in field_names:
                                col_idx = field_names.index(field_name) + 2  # +2 因为第1列是 ##var
                                
                                # 处理复杂类型
                                if isinstance(value, (list, dict)):
                                    value = json.dumps(value, ensure_ascii=False)
                                
                                ws.cell(row=row_idx, column=col_idx).value = value
                                print(f"[LubanGenerator] 更新字段 {field_name} = {value}")
                        
                        updated_count += 1
                        print(f"[LubanGenerator] 更新记录: {pk_value}")
            
            # 3. 处理新增操作（来自data字段）
            for new_record in data:
                pk_value = new_record.get(primary_key)
                
                if pk_value and pk_value not in existing_data:
                    # 添加新记录
                    new_row_idx = ws.max_row + 1
                    
                    # 填充数据
                    for i, field_name in enumerate(field_names):
                        value = new_record.get(field_name, '')
                        
                        # 处理复杂类型
                        if isinstance(value, (list, dict)):
                            value = json.dumps(value, ensure_ascii=False)
                        
                        ws.cell(row=new_row_idx, column=i + 2).value = value
                    
                    added_count += 1
                    print(f"[LubanGenerator] 新增记录: {pk_value}")
            
            # 保存文件
            wb.save(output_path)
            print(f"[LubanGenerator] 更新数据表: {output_path}")
            print(f"  - 新增: {added_count} 条")
            print(f"  - 更新: {updated_count} 条")
            print(f"  - 删除: {deleted_count} 条")
            
        except Exception as e:
            print(f"[LubanGenerator] 更新表失败: {e}")
            import traceback
            traceback.print_exc()
    
    def _apply_header_style(self, ws, col_count: int):
        """
        应用表头样式
        
        Args:
            ws: worksheet对象
            col_count: 列数
        """
        for col in range(1, col_count + 1):
            cell = ws.cell(row=1, column=col)
            cell.font = self.header_font
            cell.fill = self.header_fill
            cell.alignment = Alignment(horizontal='center', vertical='center')
            
            # 第3行注释行样式
            comment_cell = ws.cell(row=3, column=col)
            comment_cell.fill = self.comment_fill
    
    def _apply_data_table_style(self, ws, col_count: int):
        """
        应用数据表样式
        
        Args:
            ws: worksheet对象
            col_count: 列数
        """
        # 设置列宽
        for col in range(1, col_count + 1):
            ws.column_dimensions[get_column_letter(col)].width = 15
        
        # 应用表头样式
        self._apply_header_style(ws, col_count)


def create_sample_schema(output_file: str = "sample_schema.json"):
    """
    创建示例schema文件
    
    Args:
        output_file: 输出文件路径
    """
    sample_schema = {
        "beans": [
            {
                "name": "CardData",
                "comment": "卡牌系统 - 数据结构",
                "fields": [
                    {"name": "ID", "type": "string", "comment": "卡牌唯一标识符"},
                    {"name": "name", "type": "string", "comment": "卡牌名称"},
                    {"name": "description", "type": "string", "comment": "卡牌描述"},
                    {"name": "cost", "type": "int", "comment": "卡牌费用"},
                    {"name": "target", "type": "CardTargetType", "comment": "目标类型"}
                ]
            },
            {
                "name": "FoodData",
                "comment": "食材系统 - 数据结构",
                "fields": [
                    {"name": "ID", "type": "string", "comment": "唯一标识符"},
                    {"name": "name", "type": "string", "comment": "食材名称"},
                    {"name": "tastiness", "type": "int", "comment": "美味度"},
                    {"name": "rarity", "type": "int", "comment": "珍稀度"}
                ]
            }
        ],
        "enums": [
            {
                "name": "CardTargetType",
                "comment": "卡牌目标类型",
                "items": [
                    {"name": "None", "value": 0, "comment": "无目标"},
                    {"name": "Single", "value": 1, "comment": "单体"},
                    {"name": "All", "value": 2, "comment": "全体"}
                ]
            }
        ],
        "tables": [
            {
                "name": "TbCard",
                "value": "CardData",
                "index": "ID",
                "mode": "map",
                "comment": "卡牌配置表",
                "inputFile": "CardDatas/CardData.xlsx",
                "fields": [
                    {"name": "ID", "type": "string", "comment": "卡牌ID"},
                    {"name": "name", "type": "string", "comment": "卡牌名称"},
                    {"name": "description", "type": "string", "comment": "卡牌描述"},
                    {"name": "cost", "type": "int", "comment": "费用"},
                    {"name": "target", "type": "CardTargetType", "comment": "目标类型"}
                ],
                "data": [
                    {
                        "ID": "card_001",
                        "name": "火球术",
                        "description": "对目标造成100点伤害",
                        "cost": 3,
                        "target": "Single"
                    },
                    {
                        "ID": "card_002",
                        "name": "治愈术",
                        "description": "恢复目标50点生命值",
                        "cost": 2,
                        "target": "Single"
                    }
                ],
                "operations": [
                    {
                        "action": "update",
                        "key": "card_001",
                        "comment": "更新火球术的费用",
                        "data": {
                            "cost": 4
                        }
                    },
                    {
                        "action": "delete",
                        "key": "card_002",
                        "comment": "删除治愈术"
                    }
                ]
            }
        ]
    }
    
    with open(output_file, 'w', encoding='utf-8') as f:
        json.dump(sample_schema, f, ensure_ascii=False, indent=2)
    
    print(f"[LubanGenerator] 创建示例schema文件: {output_file}")


def main():
    """主函数"""
    if len(sys.argv) < 2:
        print("Luban Configuration Table Generator")
        print("=" * 50)
        print("用法:")
        print("  python luban_table_generator.py <schema.json> [output_dir]")
        print("  python luban_table_generator.py --update <schema.json> [output_dir]")
        print("  python luban_table_generator.py --sample [output_file]")
        print()
        print("模式说明:")
        print("  默认模式: 生成新表（覆盖现有文件）")
        print("  --update: 更新模式（在原表基础上增量修改）")
        print()
        print("示例:")
        print("  # 生成新表")
        print("  python luban_table_generator.py my_schema.json")
        print("  python luban_table_generator.py my_schema.json Config/Datas")
        print()
        print("  # 更新现有表")
        print("  python luban_table_generator.py --update my_schema.json Config/Datas")
        print()
        print("  # 生成示例schema")
        print("  python luban_table_generator.py --sample sample_schema.json")
        return
    
    # 生成示例文件
    if sys.argv[1] == '--sample':
        output_file = sys.argv[2] if len(sys.argv) > 2 else "sample_schema.json"
        create_sample_schema(output_file)
        return
    
    # 检查是否为更新模式
    update_mode = False
    args = sys.argv[1:]
    
    if args[0] == '--update':
        update_mode = True
        args = args[1:]
    
    if not args:
        print("[ERROR] 请指定schema文件路径")
        sys.exit(1)
    
    # 加载并生成配置
    schema_file = args[0]
    output_dir = args[1] if len(args) > 1 else "Config/Datas"
    
    generator = LubanTableGenerator(output_dir, update_mode=update_mode)
    
    mode_str = "更新" if update_mode else "生成"
    print(f"[LubanGenerator] 运行模式: {mode_str}模式")
    
    if generator.load_schema(schema_file):
        if generator.generate_all():
            print(f"\n[SUCCESS] 配置表{mode_str}成功！")
        else:
            print(f"\n[ERROR] 配置表{mode_str}失败！")
            sys.exit(1)
    else:
        print("\n[ERROR] Schema加载失败！")
        sys.exit(1)


if __name__ == "__main__":
    main()
