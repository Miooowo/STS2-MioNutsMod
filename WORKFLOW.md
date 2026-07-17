# Session Workflow

本项目启用了项目级严格 Hook 工作流，配置文件在：

- `.cursor/hooks.json`
- `.cursor/hooks/track-changes.ps1`
- `.cursor/hooks/enforce-session-workflow.ps1`

## 规则

每次会话中，如果修改了代码或资源（本地化/图片），在会话结束时会自动执行并强制校验：

1. 修改了资源（`STS2-MioNutsMod/localization/**`、`STS2-MioNutsMod/images/**`）：
   - 执行 `dotnet publish`
2. 仅修改了代码（`STS2-MioNutsModCode/**` 或相关构建文件）：
   - 执行 `dotnet build`
3. 必须有至少一次新提交（相对于本会话起点 `HEAD`）。
4. 若改动值得记录，必须更新 `CHANGELOG.md`。

## 例外

若本次改动确实不需要记录在 `CHANGELOG.md`，可在提交信息中添加：

- `[no-changelog]`

Hook 会将其视为显式豁免标记。

## 说明

- `afterFileEdit` Hook 负责记录本会话改动类型。
- `stop` Hook 负责在会话结束阶段进行构建、提交与变更日志校验。
- 如果校验失败，会阻止会话正常结束，并返回修复提示。
