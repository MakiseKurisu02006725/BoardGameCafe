<template>
  <el-container class="layout-container">
    <el-aside :width="appStore.sidebarOpened ? '220px' : '64px'" class="layout-aside">
      <Sidebar />
    </el-aside>
    <el-container class="layout-main-container">
      <el-header class="layout-header">
        <NavBar />
      </el-header>
      <el-main class="layout-main">
        <router-view v-slot="{ Component, route }">
          <transition name="fade-transform" mode="out-in">
            <keep-alive :include="cachedViews">
              <component :is="Component" :key="route.fullPath" />
            </keep-alive>
          </transition>
        </router-view>
      </el-main>
    </el-container>
  </el-container>
</template>

<script setup>
import { computed } from 'vue'
import { useAppStore } from '@/store/modules/app'
import Sidebar from './components/Sidebar.vue'
import NavBar from './components/NavBar.vue'

const appStore = useAppStore()

const cachedViews = computed(() => [])
</script>

<style lang="scss" scoped>
.layout-container {
  height: 100vh;
  width: 100%;
}

.layout-aside {
  background-color: #304156;
  transition: width 0.28s;
  overflow: hidden;
}

.layout-main-container {
  display: flex;
  flex-direction: column;
  min-width: 0;
}

.layout-header {
  background-color: #fff;
  border-bottom: 1px solid #e4e7ed;
  box-shadow: 0 1px 4px rgba(0, 21, 41, 0.08);
  padding: 0;
  height: 60px;
  display: flex;
  align-items: center;
}

.layout-main {
  background-color: #f0f2f5;
  padding: 20px;
  overflow-y: auto;
  flex: 1;
}

.fade-transform-enter-active,
.fade-transform-leave-active {
  transition: all 0.3s;
}

.fade-transform-enter-from {
  opacity: 0;
  transform: translateX(-20px);
}

.fade-transform-leave-to {
  opacity: 0;
  transform: translateX(20px);
}
</style>
