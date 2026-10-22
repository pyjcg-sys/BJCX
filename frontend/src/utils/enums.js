// 后端枚举值 → 中文文案映射（后端已启用枚举字符串序列化）

export const LEAVE_TYPE_MAP = {
  Annual: '年假',
  Personal: '事假',
  Sick: '病假',
  Marriage: '婚假',
  Other: '其他'
}

export const LEAVE_STATUS_MAP = {
  Pending: '审批中',
  Approved: '已通过',
  Rejected: '已驳回'
}

export const APPROVAL_ACTION_MAP = {
  Pending: '待审批',
  Approved: '已同意',
  Rejected: '已驳回'
}

// 下拉选项
export const LEAVE_TYPE_OPTIONS = Object.entries(LEAVE_TYPE_MAP).map(([value, label]) => ({ value, label }))
