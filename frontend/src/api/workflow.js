import request from './request'

// 流程列表
export function getWorkflows() {
  return request.get('/workflows')
}

// 创建流程
export function createWorkflow(data) {
  return request.post('/workflows', data)
}

// 流程详情
export function getWorkflowDetail(id) {
  return request.get(`/workflows/${id}`)
}
