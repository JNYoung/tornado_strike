# Google Play 上架计划

更新时间：2026-06-09

## 优先级

Google Play 是最高优先级发布平台。所有技术和产品决策优先保证 Android AAB 能稳定构建、签名、上传、封闭测试、最终申请生产访问。

## 当前事实

- 包名：`com.jnyoung.tornadostrike`
- 版本名：`0.1.0`
- 版本号：`1`
- Android 架构：ARM64
- Scripting Backend：IL2CPP
- 构建格式：AAB
- 本机 Unity SDK 已安装 Android API：33、34、35
- 项目设置使用 `AndroidSdkVersions.AndroidApiLevelAuto`，当前工具链可满足 API 35 方向。
- Debug AAB 已在英文临时路径构建成功。

## 关键规则

- Google Play 新应用需要使用 Android App Bundle。官方说明见：[Share app bundles and APKs internally](https://support.google.com/googleplay/android-developer/answer/9844679?hl=en) 和 [Android App Bundle FAQ](https://developer.android.com/guide/app-bundle/faq)。
- 从 2025-08-31 起，新应用和应用更新需要 target Android 15 / API 35 或更高。官方说明见：[Target API level requirements](https://support.google.com/googleplay/android-developer/answer/11926878?hl=en-419)。
- 2023-11-13 后创建的个人开发者账号，生产访问前需要至少 12 名 tester 连续 14 天 opted-in 的封闭测试。官方说明见：[App testing requirements for new personal developer accounts](https://support.google.com/googleplay/android-developer/answer/14151465?hl=en-EN)。

## 本机路径要求

Unity Android Tools 不支持项目真实路径含非 ASCII 字符。本仓库当前路径包含中文：

```text
/Users/zhengjinyang/Documents/龙卷突袭
```

解决方案：

- 推荐：把仓库 checkout 到英文路径，例如 `/Users/zhengjinyang/Projects/tornado_strike`。
- CI：使用英文 workspace，例如 `/workspace/tornado_strike`。
- 临时验证：复制到 `/tmp/tornado_strike_build` 构建。

## 构建命令

Debug AAB：

```bash
/Applications/Unity/Hub/Editor/2022.3.52f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -quit \
  -projectPath /tmp/tornado_strike_build \
  -executeMethod TornadoStrike.Editor.BuildAutomation.BuildAndroidDebugAab \
  -logFile -
```

Release AAB：

```bash
export TORNADO_STRIKE_KEYSTORE=/absolute/path/upload.keystore
export TORNADO_STRIKE_KEYSTORE_PASS=...
export TORNADO_STRIKE_KEYALIAS=...
export TORNADO_STRIKE_KEYALIAS_PASS=...

/Applications/Unity/Hub/Editor/2022.3.52f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -quit \
  -projectPath /path/to/ascii/tornado_strike \
  -executeMethod TornadoStrike.Editor.BuildAutomation.BuildAndroidReleaseAab \
  -logFile -
```

## Play Console 准备清单

- 创建应用：名称暂定 `Tornado Strike`。
- 默认语言：英文或简体中文，建议英文作为商店默认语言，简中作为首批本地化。
- 应用类别：Game / Casual。
- 包名固定：`com.jnyoung.tornadostrike`。
- 上传签名：开启 Play App Signing，保管 upload keystore。
- App content：
  - Content rating questionnaire。
  - Target audience and content。
  - Ads declaration。
  - Data safety。
  - Privacy policy URL。
  - App access，如无账号系统填写无特殊访问。
- Store listing：
  - 应用图标 512x512。
  - Feature graphic 1024x500。
  - 手机截图至少 2 张，建议 6 到 8 张。
  - 平板截图视 Play Console 要求补齐。
  - 7 语言标题、短描述、完整描述。
- Testing：
  - Internal test 先上传 Release AAB 冒烟。
  - Closed test 使用 Google Group。
  - 至少 12 名 tester 连续 14 天 opted-in。

## 推荐测试组

- Group name：`Tornado Strike Alpha Testers`
- Group slug：`tornado-strike-testers`
- Group email：`tornado-strike-testers@googlegroups.com`

链接模板：

```text
Group: https://groups.google.com/g/tornado-strike-testers
Web opt-in: https://play.google.com/apps/testing/com.jnyoung.tornadostrike
Store listing: https://play.google.com/store/apps/details?id=com.jnyoung.tornadostrike
Leave testing: https://play.google.com/apps/testing/com.jnyoung.tornadostrike/leave
```

不要把 `/leave` 链接发给测试者作为加入链接。

## 中文互测邀请模板

```text
✅ 邀请加入 Tornado Strike / 龙卷突袭 封闭测试！

龙卷突袭是一款控制龙卷风吸收城市物件并不断变大的休闲小游戏。

1️⃣ 第一步（加入群组）：https://groups.google.com/g/tornado-strike-testers

2️⃣ 第二步（下载应用）：
网页测试：https://play.google.com/apps/testing/com.jnyoung.tornadostrike

Google Play 下载：https://play.google.com/store/apps/details?id=com.jnyoung.tornadostrike

感谢支持！如果你也有应用需要回测，请随时告诉我。

备注：如果暂时无法下载，说明 Google Play 封闭测试版本还在审核中；请先加入群组，审核通过后再打开链接安装。
```

## 下一步

1. 把仓库移动或 checkout 到英文路径。
2. 生成 upload keystore，妥善备份。
3. 构建 Release AAB。
4. 创建 Play Console app 和 internal test。
5. 上传 AAB，处理 Play Console 的 SDK、manifest、签名、Data safety 警告。
6. 准备商店素材和 7 语言文案。
7. 创建 Google Group 并绑定 closed testing。
8. 发出互测邀请，收集 12 人以上连续 14 天 opted-in。
9. 汇总反馈，申请 production access。
