import { defineStore } from 'pinia'
import * as authApi from '@/api/auth'
import { resetRouter } from '@/router'

export const useUserStore = defineStore('user', {
  state: () => ({
    token: localStorage.getItem('token') || '',
    refreshToken: localStorage.getItem('refreshToken') || '',
    userId: '',
    memberId: '',
    userName: '',
    nickName: '',
    role: '',
    avatar: '',
    routes: [],
    rolesLoaded: false
  }),
  getters: {
    isLoggedIn: (state) => !!state.token,
    isAdmin: (state) => state.role === 'Admin',
    isManager: (state) => state.role === 'Manager',
    isCashier: (state) => state.role === 'Cashier',
    isMember: (state) => state.role === 'Member'
  },
  actions: {
    async login({ phone, password }) {
      const data = await authApi.login({ phone, password })
      this.token = data.accessToken
      this.refreshToken = data.refreshToken
      localStorage.setItem('token', data.accessToken)
      localStorage.setItem('refreshToken', data.refreshToken)
      return data
    },
    async register(payload) {
      const data = await authApi.register(payload)
      this.token = data.accessToken
      this.refreshToken = data.refreshToken
      localStorage.setItem('token', data.accessToken)
      localStorage.setItem('refreshToken', data.refreshToken)
      return data
    },
    async getUserInfo() {
      const data = await authApi.getUserInfo()
      this.userId = data.userId
      this.memberId = data.memberId || ''
      this.userName = data.userName
      this.nickName = data.nickName
      this.role = data.role
      this.avatar = data.avatar
      this.rolesLoaded = true
      return data
    },
    async logout() {
      // 【先同步紧急清场：先把本地状态全部清空，再通知后端，避免logout接口401触发递归】
      this.token = ''
      this.refreshToken = ''
      this.userId = ''
      this.memberId = ''
      this.userName = ''
      this.nickName = ''
      this.role = ''
      this.avatar = ''
      this.routes = []
      this.rolesLoaded = false
      localStorage.removeItem('token')
      localStorage.removeItem('refreshToken')
      resetRouter()

      // fire-and-forget：后端logout通知失败也没关系（本地已经全清了）
      try {
        await authApi.logout()
      } catch (e) {
        // ignore
      }
    },
    async refreshTokenAction() {
      try {
        const data = await authApi.refreshToken(this.refreshToken)
        this.token = data.accessToken
        this.refreshToken = data.refreshToken
        localStorage.setItem('token', data.accessToken)
        localStorage.setItem('refreshToken', data.refreshToken)
        return data
      } catch (e) {
        this.logout()
        throw e
      }
    },
    setToken(token) {
      this.token = token
      localStorage.setItem('token', token)
    }
  }
})
