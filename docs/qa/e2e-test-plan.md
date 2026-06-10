# E2E 测试计划

更新时间：2026-06-09

## 覆盖目标

- 开屏加载到主菜单。
- 主菜单多语言切换。
- 首次进入前展示隐私同意。
- 广告占位按钮触发奖励广告完成状态。
- 进入城市场景后可以移动、吸收、成长、计分。
- 单局目标处于 3-10 分钟设计范围。

## 自动化层

| 层级 | 工具 | 验证 |
| --- | --- | --- |
| EditMode | Unity Test Framework | 本地化表完整性、构建场景、平衡常量、菜单/城市场景关键对象 |
| Android smoke | adb | 安装、启动、前台 Activity、基础截图、logcat crash 检查 |
| Gameplay soak | Unity batch + 真机手测 | 3-10 分钟单局节奏、吸收链路、无限区块连续生成 |

## Unity EditMode 命令

```bash
/Applications/Unity/Hub/Editor/2022.3.52f1/Unity.app/Contents/MacOS/Unity \
  -batchmode \
  -quit \
  -projectPath /tmp/tornado_strike_build \
  -runTests \
  -testPlatform EditMode \
  -testResults /tmp/tornado_strike_build/editmode-results.xml
```

## Android smoke 命令

```bash
$HOME/Library/Android/sdk/platform-tools/adb devices
$HOME/Library/Android/sdk/platform-tools/adb install -r -d /tmp/tornado_strike_build/Builds/Android/TornadoStrike-debug.apk
$HOME/Library/Android/sdk/platform-tools/adb shell am start -n com.jnyoung.tornadostrike/com.unity3d.player.UnityPlayerActivity
$HOME/Library/Android/sdk/platform-tools/adb shell dumpsys window | rg com.jnyoung.tornadostrike
$HOME/Library/Android/sdk/platform-tools/adb logcat -d -b crash
```

## 真机玩法脚本

1. 首次启动，确认开屏进入主菜单。
2. 未同意隐私时点开始，确认隐私面板出现。
3. 点同意并继续，确认面板关闭。
4. 点语言按钮 7 次，确认简中、繁中、英文、德文、法文、日文、阿拉伯文都可显示。
5. 点测试奖励广告，确认状态切换为奖励广告完成。
6. 点开始进入 `City_MVP`。
7. 拖动龙卷风穿过灯杆、树、消防栓、小汽车，确认分数和半径增长。
8. 半径超过 2 后吸收公交车和小房屋。
9. 继续移动到特殊建筑槽位，确认发电厂、警察局、消防局可被吸收。
10. 连续游玩至少 6 分钟，确认没有明显空地图、卡死或崩溃。

## 失败判定

- 启动崩溃或黑屏。
- 未同意隐私也能直接进入关卡。
- 任意目标语言显示 key 原文。
- 开局 30 秒内找不到可吸收对象。
- 半径增长后仍无法吸收对应阶梯对象。
- 玩家移动后无限城市出现大面积空白区块。
