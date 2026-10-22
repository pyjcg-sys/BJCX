import request from './request'

// 用户列表（流程配置选择审批人）
export function getUsers() {
  return request.get('/users')
}
