import { defineStore } from 'pinia'

export const useAppStore = defineStore('app', {
  state: () => ({
    sidebarOpened: true,
    language: 'zh-CN',
    loading: false
  }),
  actions: {
    toggleSidebar() {
      this.sidebarOpened = !this.sidebarOpened
    },
    setSidebarOpened(opened) {
      this.sidebarOpened = opened
    },
    setLanguage(lang) {
      this.language = lang
    },
    setLoading(loading) {
      this.loading = loading
    }
  }
})
