using AutoMapper;
using BJCX.Application.DTOs;
using BJCX.Domain.Entities;

namespace BJCX.Application.Mapping;

/// <summary>
/// AutoMapper 映射配置：实体 ↔ DTO 统一在此处配置，Handler 内不做逐字段赋值。
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // 用户
        CreateMap<User, UserDto>();

        // 请假单据 → 基础信息（列表项）
        CreateMap<LeaveRequest, LeaveDto>();

        // 请假单据 → 详情（含附件与审批记录）
        CreateMap<LeaveRequest, LeaveDetailDto>();

        // 附件
        CreateMap<LeaveAttachment, AttachmentDto>();

        // 审批记录
        CreateMap<ApprovalRecord, ApprovalRecordDto>();

        // 流程节点（ApproverName 来自审批人实体的 DisplayName）
        CreateMap<WorkflowNode, WorkflowNodeDto>()
            .ForMember(d => d.ApproverName, o => o.MapFrom(s => s.Approver.DisplayName));

        // 流程定义 → 详情（含节点）
        CreateMap<WorkflowDefinition, WorkflowDefinitionDto>();

        // 流程定义 → 列表项（NodeCount 来自节点集合数量）
        CreateMap<WorkflowDefinition, WorkflowListItemDto>()
            .ForMember(d => d.NodeCount, o => o.MapFrom(s => s.Nodes.Count));

        // 邮件消息
        CreateMap<EmailMessage, EmailMessageDto>();
    }
}
