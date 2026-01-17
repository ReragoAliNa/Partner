# 🐾 Desktop Pet (桌面宠物)

<p align="center">
  <img src="DesktopPet/Assets/pet.png" alt="Desktop Pet" width="150"/>
</p>

<p align="center">
  <b>一款高性能、基于物理引擎的桌面伴侣应用</b><br>
  <i>采用现代 Neumorphism 设计风格</i>
</p>

---

## ✨ 功能特性

### 🎮 智能交互
- **像素级拖拽** - 支持 DPI 感知，在任何分辨率下都能精准拖动
- **物理投掷** - 快速拖动后松开，宠物会被"扔"出去并弹跳
- **自动避让** - 鼠标悬停时宠物自动变透明，不影响工作

### 🧠 AI 行为系统
- **状态机驱动** - 宠物拥有 Idle、Walk、Fall、Drag 等多种状态
- **随机漫步** - 闲置时会自主在屏幕底部走动
- **重力物理** - 真实的下落动画和落地弹跳效果

### 🎨 精美界面
- **Neumorphism 设计** - 现代软 UI 风格的控制面板
- **动画效果** - 面板打开时带有平滑的缩放和淡入动画
- **呼吸动画** - 宠物待机时有细腻的呼吸效果

---

## 🚀 快速开始

### 方式一：直接运行（推荐）
1. 下载 [Release](https://github.com/ReragoAliNa/Partner/releases) 中的 `DesktopPet.exe`
2. 双击运行，宠物就会出现在桌面上！

### 方式二：从源码构建
```powershell
# 克隆仓库
git clone https://github.com/ReragoAliNa/Partner.git
cd Partner/DesktopPet

# 运行
dotnet run

# 发布独立 EXE
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

---

## 🎮 操作指南

| 操作 | 说明 |
|------|------|
| **左键拖动** | 拖动宠物到任意位置 |
| **快速拖动后松开** | 投掷宠物（带物理效果） |
| **右键点击宠物** | 打开控制菜单 |
| **右键系统托盘图标** | 打开控制菜单 |
| **按住 Ctrl** | 强制显示透明状态的宠物 |
| **ESC 键** | 关闭控制面板 |

---

## 🛠️ 控制面板功能

- **开机自启** - 设置 Windows 启动时自动运行
- **宠物大小** - 调整宠物的显示比例 (50% - 200%)
- **快捷提示** - 查看操作指南

---

## 📁 项目结构

```
DesktopPet/
├── Assets/           # 资源文件 (pet.png)
├── Core/             # 核心模块
│   ├── Animation/    # 动画控制器
│   └── NativeMethods.cs  # Win32 API 封装
├── States/           # 状态机
│   ├── IdleState.cs      # 待机状态
│   ├── WalkState.cs      # 行走状态
│   ├── FallState.cs      # 下落状态
│   └── DragState.cs      # 拖拽状态
├── MainWindow.xaml       # 主窗口
├── SettingsWindow.xaml   # 控制面板
└── PetContext.cs         # 宠物上下文
```

---

## ⚖️ 版权声明

**代码**: 本项目代码采用 MIT 许可证开源。

**素材**: `Assets/` 文件夹中的图片仅供演示使用。
> ⚠️ 建议用户替换为自己的原创素材或有授权的图片，以避免版权问题。

---

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！

---

<p align="center">
  Made with ❤️ by Desktop Pet Studio
</p>
