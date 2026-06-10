# 技术架构

更新时间：2026-06-09

## Unity 基线

- Editor：Unity `2022.3.52f1c1`。
- 渲染：内置渲染管线，MVP 不引入 URP，降低构建和兼容成本。
- UI：`com.unity.ugui@1.0.0`。
- 输入：旧版 Input Manager，支持 WASD、鼠标拖动、触摸拖动。
- 物理：Unity 3D Physics，龙卷风用 trigger 检测可吸收对象。

## 目录结构

```text
Assets/TornadoStrike/
  Editor/
    BuildAutomation.cs
    CityMvpSceneGenerator.cs
  Resources/Localization/
    localization.tsv
  Scenes/
    City_MVP.unity
  Scripts/
    Camera/
    Core/
    Gameplay/
    Localization/
    Player/
    UI/
```

## 运行时模块

| 模块 | 文件 | 责任 |
| --- | --- | --- |
| 吸收对象 | `Absorbable.cs` | 体量门槛、分数、成长值、吸收动画 |
| 场景槽位 | `SceneSlot.cs` | 特殊建筑/未来场景的可配置槽位 |
| 玩家移动 | `TornadoController.cs` | 键盘、鼠标、触摸输入和边界限制 |
| 成长系统 | `TornadoGrowth.cs` | 半径判断、吸收结算、视觉缩放、事件 |
| 摄像机 | `FollowCamera.cs` | 平滑跟随玩家 |
| 启动 | `GameBootstrap.cs` | 帧率、质量设置、语言初始化 |
| 本地化 | `LocalizationService.cs` | TSV 字符串表加载和语言回退 |
| HUD | `LevelProgressHud.cs` | 分数、半径、倒计时、完成状态 |

## Editor 工具

### CityMvpSceneGenerator

菜单：

```text
Tornado Strike/Generate City MVP Scene
```

功能：

- 新建 `City_MVP.unity`。
- 创建城市地面、道路、街区。
- 创建玩家龙卷风、相机、灯光、HUD、系统对象。
- 放置房屋、小汽车、公交车。
- 放置发电厂、警察局、消防局特殊槽位。
- 配置 Build Settings 场景列表。

### BuildAutomation

菜单：

```text
Tornado Strike/Configure Project Settings
Tornado Strike/Build/Android Debug AAB
Tornado Strike/Build/Android Release AAB
```

Release 构建要求环境变量：

- `TORNADO_STRIKE_KEYSTORE`
- `TORNADO_STRIKE_KEYSTORE_PASS`
- `TORNADO_STRIKE_KEYALIAS`
- `TORNADO_STRIKE_KEYALIAS_PASS`

## 已验证

- Unity 批处理生成场景成功。
- Unity 批处理配置项目成功。
- Android Debug AAB 在临时英文路径 `/tmp/tornado_strike_build` 构建成功。
- 构建产物：`/tmp/tornado_strike_build/Builds/Android/TornadoStrike-debug.aab`。
- AAB 文件大小：约 26 MB；Unity Build Report 完整构建体量约 366 MB。

## 当前技术约束

- 本机 Android Tools 不接受真实项目路径中的中文字符，必须在英文路径 checkout 或用 CI 英文工作目录构建。
- 当前使用 uGUI 内置字体 `LegacyRuntime.ttf`，后续需要引入覆盖中文、日文、阿拉伯文、德文、法文的正式字体资产。
- 当前本地化是轻量 TSV 方案，后续可以升级到 Unity Localization package。
- 当前 Meta Quest 只是预留方向，尚未安装 XR Plug-in Management / OpenXR / Meta OpenXR 包。

## 后续技术任务

- 引入配置化关卡数据：ScriptableObject 或 JSON。
- 把灰盒材质和 primitive 替换为低多边形资源。
- 为可吸收物加入对象池，降低移动端 GC 和实例销毁成本。
- 添加 PlayMode 测试：半径门槛、吸收结算、分数目标。
- 添加自动截图工具，用于 Play Store listing。
- Release AAB 增加 native debug symbols 产物输出和上传流程。
