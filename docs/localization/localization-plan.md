# 多语言计划

更新时间：2026-06-09

## 首批语言

| 语言 | 代码 | 状态 |
| --- | --- | --- |
| 简体中文 | `zh-Hans` | 已建种子表 |
| 繁体中文 | `zh-Hant` | 已建种子表 |
| 英文 | `en` | 已建种子表 |
| 德文 | `de` | 已建种子表 |
| 法文 | `fr` | 已建种子表 |
| 日文 | `ja` | 已建种子表 |
| 阿拉伯文 | `ar` | 已建种子表 |

种子表位置：

```text
Assets/TornadoStrike/Resources/Localization/localization.tsv
```

## 当前实现

当前使用轻量 TSV 运行时加载：

- `LocalizationService` 读取 `Resources/Localization/localization.tsv`。
- 第一列为 key，其余列为语言代码。
- 支持系统语言映射。
- 支持默认语言和英文回退。
- HUD 文案支持格式化参数。

## 为什么先不用完整 Localization package

MVP 阶段优先验证玩法和 Google Play 构建链路，轻量 TSV 方案足够支持 UI 文本。Unity 官方 Localization package 支持 String localization、Asset localization、Pseudo-localization、CSV/XLIFF/Google Sheets 工作流，可在 P1/P2 升级。官方参考：[Unity Localization package](https://docs.unity.cn/Manual/com.unity.localization.html)。

## 字体计划

当前 uGUI 使用 Unity 内置 `LegacyRuntime.ttf`，只适合 MVP 验证。正式版本需要：

- Noto Sans CJK：简中、繁中、日文。
- Noto Sans Arabic：阿拉伯文。
- Noto Sans 或等价拉丁字体：英文、德文、法文。
- 如果升级 TextMeshPro，需要为每个脚本创建 fallback font assets。

## 阿拉伯文注意事项

- 阿拉伯文是 RTL 语言，uGUI `Text` 对复杂排版支持有限。
- P1 需要验证阿拉伯文连字、方向、数字混排。
- 正式版本建议使用 TextMeshPro + RTL 支持方案，或引入经过验证的 RTL 文本组件。

## 本地化 key 规范

- UI：`hud_score`、`hud_timer`。
- 对象：`object_car`、`object_bus`、`object_house`。
- 槽位：`slot_power_plant`、`slot_police_station`。
- 商店文案：`store_short_description`、`store_full_description`。

## 翻译流程

1. 产品新增 key，先填中文和英文。
2. 代码和场景只引用 key，不硬编码显示文案。
3. 每周导出 TSV 给翻译。
4. 回填后跑本地化检查：
   - key 是否缺失。
   - 文本是否溢出。
   - 参数 `{0}` 是否保留。
   - RTL 是否显示正常。

## P1 任务

- 添加语言切换调试菜单。
- 添加缺失 key 扫描器。
- 升级 TextMeshPro 字体链。
- 增加商店文案 key。
- 为 Play Store listing 准备 7 语言标题、短描述、长描述。
