# Tornado Strike / 龙卷突袭

龙卷突袭是一个 Unity 3D 超休闲吸收成长游戏：玩家控制龙卷风，在限定时间内吸取城市里的车辆、房屋和特殊建筑，龙卷风随吸收体量不断变大。项目优先跑通 Google Play Android 上架路径，同时保留 iOS 和 Meta Quest 扩展空间。

## 当前状态

- Unity 项目已创建，编辑器版本：`2022.3.52f1c1`。
- 远端已配置：`git@github.com:JNYoung/tornado_strike.git`。
- 已生成可打开场景：`Assets/TornadoStrike/Scenes/City_MVP.unity`。
- 已生成品牌入口：应用图标、开屏场景 `Assets/TornadoStrike/Scenes/Splash.unity`、主菜单场景 `Assets/TornadoStrike/Scenes/MainMenu.unity`。
- 已实现 MVP 闭环：移动、吸收、成长、分数、倒计时、完成面板。
- 已加入无限城市场景灰盒：运行时按区块流式生成道路、街区、小汽车、公交车、房屋、发电厂、警察局、消防局。
- 已加入 7 语言种子表：简体中文、繁体中文、英文、德文、法文、日文、阿拉伯文。
- 已加入 Android AAB 构建脚本。
- Android Debug AAB 已在临时英文路径验证成功：`/tmp/tornado_strike_build/Builds/Android/TornadoStrike-debug.aab`。

## 打开和生成场景

在 Unity 打开项目后，可以使用菜单：

```text
Tornado Strike/Generate City MVP Scene
Tornado Strike/Generate Branding and Menu Scenes
```

命令行生成：

```bash
/Applications/Unity/Hub/Editor/2022.3.52f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -quit \
  -projectPath "/Users/zhengjinyang/Documents/龙卷突袭" \
  -executeMethod TornadoStrike.Editor.CityMvpSceneGenerator.GenerateCitySceneBatch \
  -logFile -
```

## Android 构建

当前本机 Unity Android Tools 不接受中文项目路径，Android 构建请使用英文路径 checkout，或复制临时构建目录：

```bash
rsync -a --delete \
  --exclude '.git' --exclude 'Library' --exclude 'Temp' --exclude 'Obj' \
  --exclude 'Build' --exclude 'Builds' --exclude 'Logs' --exclude 'UserSettings' \
  "/Users/zhengjinyang/Documents/龙卷突袭/" /tmp/tornado_strike_build/

/Applications/Unity/Hub/Editor/2022.3.52f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -quit \
  -projectPath /tmp/tornado_strike_build \
  -executeMethod TornadoStrike.Editor.BuildAutomation.BuildAndroidDebugAab \
  -logFile -
```

Release AAB 使用环境变量读取签名：

```bash
export TORNADO_STRIKE_KEYSTORE=/absolute/path/upload.keystore
export TORNADO_STRIKE_KEYSTORE_PASS=...
export TORNADO_STRIKE_KEYALIAS=...
export TORNADO_STRIKE_KEYALIAS_PASS=...
```

## 文档入口

- 产品和路线图：[docs/product-plan.md](docs/product-plan.md)
- 技术架构：[docs/technical-architecture.md](docs/technical-architecture.md)
- 竞品调研：[docs/research/competitive-research.md](docs/research/competitive-research.md)
- 多端发布：[docs/release/multi-platform-release-plan.md](docs/release/multi-platform-release-plan.md)
- Google Play 上架：[docs/release/google-play-launch-plan.md](docs/release/google-play-launch-plan.md)
- 多语言计划：[docs/localization/localization-plan.md](docs/localization/localization-plan.md)
- 第一阶段冲刺：[docs/sprints/phase-01-city-mvp.md](docs/sprints/phase-01-city-mvp.md)
