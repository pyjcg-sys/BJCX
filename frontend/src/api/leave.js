import request from './request'

// 提交请假申请（multipart，含附件）
export function submitLeave(formData) {
  return request.post('/leaves/submit', formData)
}

// 我的待办分页列表
export function getPendingLeaves(params) {
  return request.get('/leaves/pending', { params })
}

// 我提交的申请分页列表
export function getMyLeaves(params) {
  return request.get('/leaves/mine', { params })
}

// 请假单据详情（基础信息 + 附件 + 审批记录）
export function getLeaveDetail(id) {
  return request.get(`/leaves/${id}`)
}

// 审批同意
export function approveLeave(id, comment) {
  return request.post(`/leaves/${id}/approve`, { comment })
}

// 审批驳回
export function rejectLeave(id, comment) {
  return request.post(`/leaves/${id}/reject`, { comment })
}

// 下载附件（返回 blob，用于预览/下载）
export function downloadAttachment(id) {
  return request.get(`/leaves/attachments/${id}`, { responseType: 'blob' })
}
