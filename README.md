# 攀爬 / Climb Volume — 独立模块

这是一个**完全独立**的 Unity 2D 攀爬演示，不引用、不修改 `SmallVoice` 项目，也不依赖它的场景、脚本、URP 配置或美术库。

它只实现一件事：角色走到可攀爬障碍物旁，按 `W / ↑` 播一遍翻越序列帧，落到障碍物顶面；从顶面边缘走出去会掉回地面。

## 目录

```text
攀爬/
  README.md                         本说明
  ClimbDemo/                        可单独打开的 Unity 2022.3 工程
    Assets/
      Scripts/
        ClimbVolume.cs              可站立体积 / 攀爬触发（核心模块）
        ClimbDemoController.cs      独立角色控制 + 攀爬路径 + 序列帧
        ClimbDemoBootstrap.cs       运行时搭建演示场景
        ClimbDemoHUD.cs             操作提示与状态
        CameraFollow.cs             相机跟随
        ClimbSelfTest.cs            -thatgjClimbTest 自检
        ClimbDemoAutoShot.cs        截图辅助
      Editor/
        ClimbVolumeTools.cs         菜单放置工具 + Gizmos
        ClimbDemoSceneBuilder.cs    创建演示场景
        ClimbDemoBuild.cs           构建 Windows 演示
      Resources/Climb/              16 张翻越关键帧（来自参考仓库 ArtSource）
      Scenes/ClimbDemo.unity        空场景，运行时自动搭建
    ProjectSettings/                独立工程设置
    Packages/                       只包含 2D Sprite / Physics2D 等必要模块
    Build/ClimbDemo.exe             可直接运行的 Windows 演示
    Docs/Preview/                   4 张运行截图
```

## 与参考仓库的对应关系

参考仓库的“攀爬 / 可站立体积”章节定义了 `Content/Assets/Scripts/ClimbVolume.cs` 与 `ClimbVolume` 组件。本模块保留同一套语义，但类放在 `ClimbDemo` 命名空间，且不依赖参考项目：

| 参考仓库概念 | 本模块文件 | 说明 |
|---|---|---|
| `ClimbVolume` 组件 | `ClimbVolume.cs` | 挂在空物体 + 一个 `BoxCollider2D` 上 |
| `useColliderTop` / `topY` | 同名字段 | 顶边即站立高度；关掉 `useColliderTop` 后手填世界坐标 `topY` |
| `blocksWalking` | 同名字段 | `true` 是实体路障，必须爬；`false` 是可穿过的家具，只保留可站立顶面 |
| `climbDuration` | 同名字段 | 从起爬到站上顶面的时间 |
| `reach` | 同名字段 | 离体积多近可以按 `W` |
| `landingInset` | 同名字段 | 落点离顶面边缘的内缩 |
| 绿色框 / 黄色线 | `ClimbVolume.OnDrawGizmos` | 绿框 = 体积，黄线 = 站立脚底线 |
| 菜单 `Tools/ThatGJ/Add Climb Volume` | `Tools/Climb Demo/Add Climb Volume`（`Ctrl/Cmd+Shift+K`） | 在 Scene 视图中心生成 |
| 选中 SpriteRenderer 生成体积 | `Tools/Climb Demo/Create Volume From Selected Sprite` | 按包围盒生成，默认 `blocksWalking=false` |
| `CLIMB TEST PASS` | `ClimbSelfTest.cs` | 运行时自检：起爬 → 站上 → 走下 → 落地 |

## 单独运行

### 方式 A：直接运行成品

双击：

```text
攀爬/ClimbDemo/Build/ClimbDemo.exe
```

操作：

- `A / D`：左右移动
- `W / ↑`：靠近木箱时翻越
- 站上木箱后继续往右走：从边缘掉回地面

### 方式 B：在 Unity 中打开

1. Unity Hub → Add project from disk → 选择 `攀爬/ClimbDemo`
2. 用 Unity `2022.3.62f3c1` 打开
3. 打开 `Assets/Scenes/ClimbDemo.unity`，按 Play
4. 场景由 `ClimbDemoBootstrap` 在运行时自动生成，不需要手动拖任何对象

## 自检

在 `攀爬/ClimbDemo` 目录下运行：

```bat
Unity.exe -projectPath "C:\Users\20252\OneDrive - Coventry University\桌面\gamejam\攀爬\ClimbDemo" -thatgjClimbTest -logFile "C:\Users\20252\OneDrive - Coventry University\桌面\gamejam\攀爬\ClimbDemo\climb_test.log"
```

日志出现：

```text
CLIMB TEST PASS top=1.41 landed=0.01
```

表示三步全部通过：起爬 → 站上顶面 → 走下边缘掉回地面。

也可以直接对成品运行：

```bat
ClimbDemo.exe -batchmode -nographics -thatgjClimbTest -logFile climb_test.log
```

## 实现思路

1. **体积只负责定义空间**：`ClimbVolume` 的 `BoxCollider2D` 设为 Trigger，只提供 `Bounds` 和 `TopY`；运行时再生成一个实体 `Body`（`blocksWalking=true`）或一个单向 `TopPlatform`（`blocksWalking=false`）。
2. **角色只负责动作**：`ClimbDemoController` 检测 `W / ↑`，在 `reach` 内找到最近的 `ClimbVolume`，关闭自身碰撞体，按两段路径移动：
   - 前 55%：贴近墙面并上升；
   - 后 45%：越过顶边、落到 `landingInset` 内缩后的落点。
3. **序列帧**：16 张 `Resources/Climb/climb_*.png` 按 `climbDuration` 均分播放，帧率由时长自动推导；角色朝向由体积在左/右决定。
4. **站上与掉落**：`blocksWalking=true` 用实体碰撞体顶面站立；`blocksWalking=false` 用单向平台，且只在角色高于顶面时启用，避免走路时被侧面挡住。
5. **Gizmos**：绿色线框是体积，黄色线是 `TopY` 脚底线，方便对齐障碍物。

## 与现有项目的关系

- `SmallVoice/` 中**没有** `ClimbVolume`、`ClimbSelfTest`、`ClimbVolumeTools`、`Climb` 动画帧，也没有任何攀爬相关引用。
- 本目录是独立产出；复制/删除 `攀爬/` 不会影响 `SmallVoice/` 的编译、场景或构建。
- 如果要把 `ClimbVolume` 接到别的项目，只需要复制 `ClimbVolume.cs` 和 `Climb` 帧，并让目标角色在 `reach` 内调用 `BeginClimb`；不依赖本演示的 `ClimbDemoController`。
