// Token存储键名
const TOKEN_KEY = 'bgc_access_token'
const REFRESH_KEY = 'bgc_refresh_token'

/**
 * 获取访问Token
 * @returns {string} Token字符串
 */
export function getToken() {
  return localStorage.getItem(TOKEN_KEY) || ''
}

/**
 * 设置访问Token
 * @param {string} token - Token字符串
 */
export function setToken(token) {
  localStorage.setItem(TOKEN_KEY, token)
}

/**
 * 移除访问Token
 */
export function removeToken() {
  localStorage.removeItem(TOKEN_KEY)
}

/**
 * 获取刷新Token
 * @returns {string} 刷新Token字符串
 */
export function getRefreshToken() {
  return localStorage.getItem(REFRESH_KEY) || ''
}

/**
 * 设置刷新Token
 * @param {string} refreshToken - 刷新Token字符串
 */
export function setRefreshToken(refreshToken) {
  localStorage.setItem(REFRESH_KEY, refreshToken)
}

/**
 * 移除刷新Token
 */
export function removeRefreshToken() {
  localStorage.removeItem(REFRESH_KEY)
}

/**
 * 清除所有认证信息（访问Token和刷新Token）
 */
export function clearAll() {
  removeToken()
  removeRefreshToken()
}
