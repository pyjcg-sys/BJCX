import request from './request'

// 登录：返回 { token, userId, userName, displayName, email }
export function login(data) {
  return request.post('/auth/login', data)
}
