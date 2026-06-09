# 多端发布计划

更新时间：2026-06-09

## 平台优先级

1. Android / Google Play
2. iOS / App Store
3. Meta Quest / App Lab 或 Meta Horizon Store

## Android

当前已完成：

- AAB 构建脚本。
- ARM64 + IL2CPP。
- Debug AAB 临时英文路径构建成功。

待完成：

- Release 签名。
- Google Play Console app 创建。
- Data safety 和隐私策略。
- 封闭测试。
- 商店截图和视频。
- 崩溃和性能监控。

特别注意：

- 构建路径必须是英文。
- API 35 是当前 Google Play 方向。
- 首发不要接入不必要权限，降低 Data safety 和审核复杂度。

## iOS

目标：

- 保持同一套核心玩法和本地化表。
- 使用 IL2CPP。
- 先做 TestFlight 灰盒包，再做 App Store。

待完成：

- 安装或确认 Unity iOS Build Support。
- 配置 Apple Developer Team ID。
- Bundle ID：`com.jnyoung.tornadostrike`。
- 处理 iOS 横竖屏、安全区、触摸输入。
- 准备隐私清单、App Privacy、ATT 决策。
- App Store 7 语言素材和截图。

## Meta Quest

定位：

- Meta 不是第一阶段发布目标，而是中后期扩展方向。
- MVP 可以先做“俯视平面版龙卷风”，Quest 版本再改为 VR 上帝视角或沉浸式吸收体验。

推荐技术路线：

- Unity OpenXR。
- XR Plug-in Management。
- Unity OpenXR: Meta package。
- Android target 仍保持 ARM64。

官方设置参考：[Unity OpenXR Meta Project setup](https://docs.unity.cn/Packages/com.unity.xr.meta-openxr%401.0/manual/project-setup.html)。

待验证：

- Quest 上是否使用手柄射线、摇杆或头控来移动龙卷风。
- 摄像机模式：俯视桌面沙盘、第一人称风暴、还是第三人称上帝视角。
- 帧率预算：优先稳定舒适，不直接复用移动端所有特效。

## 共享与差异

| 系统 | Android | iOS | Meta |
| --- | --- | --- | --- |
| 核心吸收成长 | 共享 | 共享 | 共享 |
| 关卡配置 | 共享 | 共享 | 共享 |
| 本地化 | 共享 | 共享 | 共享 |
| 输入 | 触摸/拖动 | 触摸/拖动 | 手柄/头显 |
| UI | 竖屏 HUD | 竖屏 HUD | VR HUD 或世界空间 UI |
| 构建格式 | AAB | Xcode archive | APK/AAB |
| 发布优先级 | P0 | P2 | P3 |
