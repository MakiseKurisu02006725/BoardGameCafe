/**
 * 角色权限判断工具
 * 从Pinia user store中获取用户角色进行权限判断
 */

/**
 * 获取当前用户角色列表
 * @returns {Array<string>} 角色标识数组
 */
function getUserRoles() {
  try {
    const pinia = window.__pinia
    if (!pinia) {
      return []
    }
    const userStore = pinia._s.get('user') || pinia.state.value.user
    if (userStore && userStore.roles) {
      return Array.isArray(userStore.roles) ? userStore.roles : [userStore.roles]
    }
    return []
  } catch (e) {
    console.warn('获取用户角色失败:', e)
    return []
  }
}

/**
 * 判断用户是否拥有指定的所有角色
 * @param {string|Array<string>} roles - 需要校验的角色（单个角色字符串或角色数组）
 * @returns {boolean} 是否拥有全部指定角色
 */
export function hasRole(roles) {
  const userRoles = getUserRoles()
  if (userRoles.includes('admin') || userRoles.includes('super_admin')) {
    return true
  }
  const requiredRoles = Array.isArray(roles) ? roles : [roles]
  return requiredRoles.every((role) => userRoles.includes(role))
}

/**
 * 判断用户是否拥有指定角色中的任意一个
 * @param {string|Array<string>} roles - 需要校验的角色（单个角色字符串或角色数组）
 * @returns {boolean} 是否拥有任意一个指定角色
 */
export function hasAnyRole(roles) {
  const userRoles = getUserRoles()
  if (userRoles.includes('admin') || userRoles.includes('super_admin')) {
    return true
  }
  const requiredRoles = Array.isArray(roles) ? roles : [roles]
  return requiredRoles.some((role) => userRoles.includes(role))
}
