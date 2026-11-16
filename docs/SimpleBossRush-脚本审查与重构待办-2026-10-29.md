---
id: "fc8f2421-346e-49b1-93a3-4b113866dab5"
title: "SimpleBossRush Unity 项目脚本审查结论与重构待办（2026-10-29）"
description: "SimpleBossRush 弹幕 Boss 项目的目录结构、6 个脚本职责与行数、近期改动的质量评估、未修问题清单（Boss 空引用、场景名大小写、Boss.cs 拆分、关卡数据硬编码）及修复优先级。"
status: "active"
created_at: "2026-10-29T07:00:25.581Z"
updated_at: "2026-10-29T07:00:25.581Z"
content_hash: "7a10aaa54d63ce26fe6c6f6030559343b3e1b80e58bee7ac4188f6ad0593219a"
source_paths:
  - "Assets/Scripts/Boss.cs"
  - "Assets/Scripts/Player.cs"
  - "Assets/Scripts/PageControl.cs"
  - "Assets/Scripts/GameControl.cs"
  - "Assets/Scripts/Laser.cs"
  - "Assets/Scripts/Bullet.cs"
  - "Assets/Scenes/mainScene.unity"
  - "Assets/Prefabs"
session_ids:
  - "5af5a949-73cd-4abb-8e41-72eb3bfcc68c"
memory_body_ids:
  []
---

# SimpleBossRush Unity 项目脚本审查结论与重构待办

来源：2026-10-29 对 `E:\SoftData\Unity\SimpleBossRush`（项目路径已从旧机 `F:` 迁到 `E:`）的一次完整脚本审查。审查前用户已改动 `Boss.cs`、`Bullet.cs`、`GameControl.cs`、`PageControl.cs` 四个文件，改动方向是「防崩 + 规范化」。

## 一、目录结构（Assets 下）

```
Assets/
├── Materials/
├── Prefabs/
│   ├── GameObject/  Boss · player · CircleBullet · SquareBullet
│   │                Laser · PreLaser · RotateLaser · RotatePreLaser（共 8 个）
│   └── UI/          DifficultyChooseArea
├── Resources/       BGM/ · AudioClip/
├── Scenes/          自有场景 7 个 + TextMesh Pro 示例 30 个
├── Scripts/         6 个 .cs，全部平铺（无子目录）
└── TextMesh Pro/    第三方包
```

`Prefabs` 分 `GameObject` / `UI` 两层是值得保留的组织方式。

## 二、约束提醒
本工作区的 `AGENTS.md` 规定：只在 `Assets/` 内读写；`.unity` / `.prefab` 是编辑器生成的 YAML，不得手改；`.meta` 必须与资源成对处理；改完脚本要让用户切回 Unity 触发重编译并刷新编辑器。
