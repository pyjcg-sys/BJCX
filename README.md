# 请假审批流程系统（.NET / C#）

北京创新数智科技有限公司面试 —— 请假审批流程系统后端。

采用 **CQRS 架构**，基于 ASP.NET Core 8 + MediatR + Autofac + FluentValidation + AutoMapper + EF Core(SQLite) + Redis + Serilog + JWT 构建。

## 一、架构与技术规范

1. **CQRS 模式**：区分 Command（写）/ Query（读）与对应 Handler，Controller 层只做请求接收与调用 MediatR，无业务逻辑。
2. **Autofac 模块化依赖注入**：按 `ApplicationModule` / `InfrastructureModule` 划分模块注册，避免集中在 Program.cs。
3. **MediatR 管道行为**：三个管道 —— 入参校验管道、日志管道、事务管道（`IPipelineBehavior`）。
4. **身份验证**：JWT + Swagger（Bearer），用户身份解析统一在 `CurrentUserFilter` 中完成。
5. **FluentValidation 入参校验**：每个 Command/Query 对应一个 Validator，校验统一走校验管道。
6. **AutoMapper**：`MappingProfile` 统一配置实体 ↔ DTO 映射。
7. **Redis 缓存**：封装 `ICacheService`，Query 结果缓存，写操作触发对应缓存清理。
8. **Serilog 结构化日志**：记录操作人、单据编号、操作耗时、异常信息。
9. **EF Core BaseRepository**：`IRepository<T>` 统一增删改查，读查询 `AsNoTracking` + 异步。
10. **文件存储 / 邮件服务**：`IFileStorageService` / `IEmailService` 内部抽象，不接真实第三方。

## 二、项目结构

```
BJCX.sln
├── src/
│   ├── BJCX.Domain/          实体、枚举、密码哈希工具
│   ├── BJCX.Application/     Command/Query/Handler/Validator/DTO/Profile/管道/接口抽象
│   ├── BJCX.Infrastructure/  DbContext/BaseRepository/Redis/文件/邮件/JWT/种子数据/Autofac 模块
│   └── BJCX.Api/             Controller/Filter/异常中间件/Program/appsettings
└── tests/
    └── BJCX.Tests/           xUnit 单元测试
```

## 三、运行要求

- .NET 8 SDK
- Redis（本机 `localhost:6379`，缓存必需）

## 四、快速开始

```bash
# 1. 启动 Redis（确保 6379 端口可用）

# 2. 运行 API（首次启动自动建库并写入种子数据）
dotnet run --project src/BJCX.Api

# 3. 打开 Swagger 调试界面
#    http://localhost:5107/swagger
```

## 五、演示账号（密码均为 123456）

| 登录名 | 姓名 | 角色 |
| ------ | ---- | ---- |
| zhangsan | 张三 | 申请人（研发部） |
| lisi | 李四 | 直属领导 |
| wangwu | 王五 | 部门经理 |

## 六、审批流程

默认流程（可自由配置）：**申请人提交 → 直属领导审批 → 部门经理审批**，节点串行流转。

- 提交后流转到第一个审批节点，自动给当前待审批人发通知邮件。
- 同意时校验操作人权限与单据状态，按串行规则流转到下一节点并通知。
- 驳回时校验权限与状态，驳回单据并通知申请人（含驳回意见）。
- 全部节点通过后，单据状态置为「已通过」，通知申请人。

## 七、接口清单

| 方法 | 路径 | 说明 |
| ---- | ---- | ---- |
| POST | `/api/auth/login` | 登录，返回 JWT |
| POST | `/api/leaves/submit` | 提交请假申请（multipart，支持多附件） |
| POST | `/api/leaves/{id}/approve` | 审批同意 |
| POST | `/api/leaves/{id}/reject` | 审批驳回 |
| GET | `/api/leaves/{id}` | 单据详情（附件 + 审批记录，Redis 缓存） |
| GET | `/api/leaves/pending` | 我的待办分页列表（Redis 缓存） |
| POST | `/api/workflows` | 创建审批流程 |
| GET | `/api/workflows` | 流程列表 |
| GET | `/api/workflows/{id}` | 流程详情 |

## 八、测试

```bash
dotnet test
```

覆盖提交、同意流转、驳回、越权/重复审批、详情、待办列表、校验器与缓存失效共 19 项用例。
