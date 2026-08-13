import axios from 'axios'
import { ElMessage } from 'element-plus'
import { useUserStore } from '@/store/modules/user'
import router, { resetRouter } from '@/router'

const CLIENT_ID = 'boardgame-cafe-web'

const service = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '/api',
  timeout: 30000
})

// 401处理一次性门禁：防止 logout 接口本身因 [Authorize] 而 401，递归触发 handle401Error 造成上万次请求
let isHandling401 = false
// Token刷新锁：防止多个请求同时触发token刷新
let isRefreshing = false
// 等待token刷新的请求队列
let pendingRequests = []

function parseJwt(token) {
  try {
    const base64Url = token.split('.')[1]
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/')
    const jsonPayload = decodeURIComponent(
      atob(base64).split('').map(c =>
        '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)
      ).join('')
    )
    return JSON.parse(jsonPayload)
  } catch {
    return null
  }
}

function isTokenExpiringSoon(token) {
  const payload = parseJwt(token)
  if (!payload || !payload.exp) return true
  const now = Math.floor(Date.now() / 1000)
  // 如果token将在5分钟内过期，视为即将过期
  return payload.exp - now < 300
}

async function refreshAccessToken() {
  const userStore = useUserStore()
  if (!userStore.refreshToken) {
    throw new Error('无刷新令牌')
  }

  if (isRefreshing) {
    // 如果正在刷新，等待其他刷新请求完成
    return new Promise((resolve, reject) => {
      pendingRequests.push({ resolve, reject })
    })
  }

  isRefreshing = true
  try {
    const response = await axios.post(
      `${import.meta.env.VITE_API_BASE_URL || '/api'}/auth/refresh`,
      { refreshToken: userStore.refreshToken },
      { headers: { 'X-ClientId': CLIENT_ID } }
    )
    const data = response.data?.data || response.data
    if (data?.accessToken) {
      userStore.token = data.accessToken
      userStore.refreshToken = data.refreshToken
      localStorage.setItem('token', data.accessToken)
      localStorage.setItem('refreshToken', data.refreshToken)
      // 通知等待的请求
      pendingRequests.forEach(cb => cb.resolve(data.accessToken))
      pendingRequests = []
      return data.accessToken
    } else {
      throw new Error('刷新令牌响应格式错误')
    }
  } catch (error) {
    pendingRequests.forEach(cb => cb.reject(error))
    pendingRequests = []
    throw error
  } finally {
    isRefreshing = false
  }
}

service.interceptors.request.use(
  async (config) => {
    const userStore = useUserStore()
    if (userStore.token) {
      config.headers['Authorization'] = `Bearer ${userStore.token}`

      // 检查token是否即将过期，如果是则尝试刷新
      if (!config.skipTokenRefresh && isTokenExpiringSoon(userStore.token) && userStore.refreshToken) {
        try {
          const newToken = await refreshAccessToken()
          config.headers['Authorization'] = `Bearer ${newToken}`
        } catch {
          // 刷新失败不阻塞请求，请求会收到401
        }
      }
    }
    config.headers['X-ClientId'] = CLIENT_ID
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

service.interceptors.response.use(
  (response) => {
    const res = response.data
    if (res && typeof res === 'object' && 'code' in res) {
      const { code, message, data } = res
      if (code === 200) {
        return data
      }
      return handleApiError(code, message, response.config, response.status)
    }
    return res
  },
  async (error) => {
    const config = error.response?.config || error.config
    if (error.response) {
      const { status, data } = error.response

      if (status === 429) {
        ElMessage.warning('请求过于频繁，请稍后再试')
        return Promise.reject(error)
      }

      if (status === 401 && !config?._retry) {
        // 尝试用refreshToken刷新accessToken后重试
        const userStore = useUserStore()
        if (userStore.refreshToken && !isHandling401) {
          try {
            const newToken = await refreshAccessToken()
            config._retry = true
            config.headers['Authorization'] = `Bearer ${newToken}`
            return service(config)
          } catch {
            // refresh失败，走强制登出
          }
        }
        return handle401Error(config)
      }

      if (status === 403) {
        ElMessage.error('权限不足')
        return Promise.reject(error)
      }

      if (data && data.message) {
        ElMessage.error(data.message)
      } else {
        ElMessage.error(error.message || '请求失败')
      }
    } else {
      ElMessage.error(error.message || '网络错误，请检查网络连接')
    }
    return Promise.reject(error)
  }
)

function handleApiError(code, message, config, httpStatus) {
  switch (code) {
    case 403:
      ElMessage.error(message || '权限不足')
      return Promise.reject(new Error(message || 'Forbidden'))
    case 429:
      ElMessage.warning(message || '请求过于频繁，请稍后再试')
      return Promise.reject(new Error(message || 'Too Many Requests'))
    default:
      ElMessage.error(message || '请求失败')
      return Promise.reject(new Error(message || 'Error'))
  }
}

async function handle401Error(config) {
  if (isHandling401) {
    return Promise.reject(new Error('Unauthorized'))
  }
  isHandling401 = true

  try {
    const userStore = useUserStore()
    userStore.token = ''
    userStore.refreshToken = ''
    userStore.rolesLoaded = false
    userStore.routes = []

    localStorage.removeItem('token')
    localStorage.removeItem('refreshToken')

    resetRouter()

    ElMessage.warning('登录已过期，请重新登录')
    setTimeout(() => {
      window.location.replace('/login')
    }, 500)
  } finally {
    setTimeout(() => { isHandling401 = false }, 3000)
  }

  return Promise.reject(new Error('Unauthorized'))
}

export default service