# Phase 01 - 城市 MVP

更新时间：2026-06-09

## 目标

在 Unity 中完成第一版可玩的城市场景，并跑通 Android AAB 构建。这个阶段的标准不是美术完成，而是玩法闭环、场景槽位和发布链路真实可用。

## 已完成

- 创建 Unity 项目。
- 配置 Git 远端。
- 新增 `.gitignore`，排除 Unity 生成目录和签名文件。
- 新增核心脚本：
  - `Absorbable`
  - `SceneSlot`
  - `TornadoController`
  - `TornadoGrowth`
  - `FollowCamera`
  - `GameBootstrap`
  - `LocalizationService`
  - `LocalizedText`
  - `LevelProgressHud`
- 新增 Editor 工具：
  - `CityMvpSceneGenerator`
  - `BuildAutomation`
- 加入 `com.unity.ugui`。
- 生成 `Assets/TornadoStrike/Scenes/City_MVP.unity`。
- 场景包含：
  - 城市道路网格。
  - 房屋。
  - 小汽车。
  - 公交车。
  - 发电厂槽位。
  - 警察局槽位。
  - 消防局槽位。
  - 雨林未来槽位。
- 加入 7 语言种子表。
- Unity 批处理生成场景通过。
- Android Debug AAB 在临时英文路径构建成功。

## 验收结果

| 验收项 | 状态 | 备注 |
| --- | --- | --- |
| Unity 项目可导入 | 通过 | Unity 2022.3.52f1c1 |
| 城市场景可生成 | 通过 | 菜单和批处理均可用 |
| C# 编译 | 通过 | 已修复 uGUI 和字体兼容问题 |
| 吸收成长闭环 | 通过 | trigger + 半径门槛 + 分数 |
| 特殊建筑槽位 | 通过 | 发电厂、警察局、消防局 |
| 多语言种子 | 通过 | 7 语言 TSV |
| Android AAB | 通过 | 需英文路径构建 |

## 当前阻塞

- 真实仓库路径含中文，Unity Android Tools 直接构建会失败。

解决：

- 移动或 checkout 到英文路径。
- CI 使用英文 workspace。

## 下一步任务

1. 把仓库正式迁移或克隆到英文路径并验证 Release AAB。
2. 创建 upload keystore。
3. 加入正式字体资产。
4. 增加小物件密度：路灯、树、垃圾桶、邮箱、消防栓。
5. 增加吸收失败反馈：对象高亮、轻微弹开、HUD 提示。
6. 加入对象池。
7. 加入 PlayMode 测试。
8. 加入截图生成工具，为 Google Play 素材做准备。

## 推荐下一次推进顺序

1. 迁移到英文路径。
2. 生成 Release AAB。
3. 接入低多边形城市资产或继续程序化扩展灰盒。
4. 做 Google Play 商店素材和 7 语言文案。
