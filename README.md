# 屁操作游戏系统使用指南

## 项目结构
- **Architecture**: 游戏架构定义
- **Model**: 数据层（玩家数据、游戏状态、配置）
- **System**: 系统层（游戏状态管理、屁值逻辑）
- **Controller**: 控制层（UI控制器、玩家控制器）
- **Command**: 命令层（游戏操作命令）
- **Event**: 事件定义

## 使用步骤

### 1. 场景设置
1. 在场景中创建一个空GameObject，命名为"GameManager"
2. 添加`GameManager.cs`脚本组件

### 2. 玩家设置
1. 创建玩家GameObject
2. 添加以下组件：
   - `PlayerController.cs`脚本
   - `Rigidbody`组件（可选）
   - `Collider`组件

### 3. UI设置
1. 创建Canvas
2. 主菜单UI：
   - 创建UI面板，添加`MainMenuUIController.cs`
   - 绑定开始按钮和退出按钮
3. 游戏内UI：
   - 创建UI面板，添加`GameplayUIController.cs`
   - 绑定屁值文本、模式文本、滑动条等UI元素

### 4. 游戏配置
在`GameConfigModel.cs`中调整游戏参数：
- 初始屁值
- 消耗速率
- 体型和速度映射曲线

## 操作说明
- **WASD**: 移动
- **空格**: 切换熏模式
- **ESC**: 暂停游戏

## 核心机制
1. **熏模式**: 开启后持续消耗屁值，无视碰撞体积
2. **屁值系统**: 屁值越高，体型越大，速度越慢
3. **自动关闭**: 屁值耗尽时自动关闭熏模式

## 扩展建议
- 添加音效系统
- 添加视觉特效（熏模式特效）
- 添加道具系统（增加屁值）
- 添加敌人系统
