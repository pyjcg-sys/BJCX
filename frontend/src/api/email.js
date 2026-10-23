import request from './request'

// 邮件记录分页列表
export function getEmails(params) {
  return request.get('/emails', { params })
}
