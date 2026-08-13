<template>
  <div class="sidebar-wrapper">
    <div class="sidebar-logo" @click="goHome">
      <el-icon class="logo-icon"><Grid /></el-icon>
      <span v-show="!isCollapse" class="logo-text">桌游馆管理系统</span>
    </div>
    <el-scrollbar class="sidebar-scrollbar">
      <el-menu
        :default-active="activeMenu"
        :collapse="isCollapse"
        :collapse-transition="false"
        background-color="#304156"
        text-color="#bfcbd9"
        active-text-color="#ffd04b"
        mode="vertical"
        router
        unique-opened
      >
        <template v-for="route in menuRoutes" :key="route.path">
          <el-menu-item
            v-if="!route.children || route.children.length === 0 || (route.children.length === 1 && !route.children[0].children)"
            :index="resolvePath(route.path)"
          >
            <el-icon v-if="route.meta && route.meta.icon">
              <component :is="route.meta.icon" />
            </el-icon>
            <template #title>{{ route.meta && route.meta.title }}</template>
          </el-menu-item>
          <el-sub-menu
            v-else
            :index="resolvePath(route.path)"
          >
            <template #title>
              <el-icon v-if="route.meta && route.meta.icon">
                <component :is="route.meta.icon" />
              </el-icon>
              <span>{{ route.meta && route.meta.title }}</span>
            </template>
            <el-menu-item
              v-for="child in route.children"
              :key="child.path"
              :index="resolvePath(route.path + '/' + child.path)"
            >
              <el-icon v-if="child.meta && child.meta.icon">
                <component :is="child.meta.icon" />
              </el-icon>
              <template #title>{{ child.meta && child.meta.title }}</template>
            </el-menu-item>
          </el-sub-menu>
        </template>
      </el-menu>
    </el-scrollbar>
    <div class="sidebar-collapse-btn" @click="toggleSidebar">
      <el-icon>
        <Fold v-if="!isCollapse" />
        <Expand v-else />
      </el-icon>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAppStore } from '@/store/modules/app'
import { useUserStore } from '@/store/modules/user'
import { Grid, Fold, Expand } from '@element-plus/icons-vue'

const route = useRoute()
const router = useRouter()
const appStore = useAppStore()
const userStore = useUserStore()

const isCollapse = computed(() => !appStore.sidebarOpened)

const menuRoutes = computed(() => {
  const routes = userStore.routes || []
  if (routes.length > 0 && routes[0].children) {
    // hidden: 详情页等隐藏路由不进入侧边栏
    return routes[0].children.filter(r => r.meta && r.meta.title && r.meta.icon && !r.meta.hidden)
  }
  return []
})

const activeMenu = computed(() => route.path)

function resolvePath(path) {
  if (path.startsWith('/')) return path
  return '/' + path
}

function toggleSidebar() {
  appStore.toggleSidebar()
}

function goHome() {
  router.push('/dashboard')
}
</script>

<style lang="scss" scoped>
.sidebar-wrapper {
  height: 100%;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.sidebar-logo {
  height: 60px;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: #2b2f3a;
  cursor: pointer;
  border-bottom: 1px solid #1f2d3d;
  flex-shrink: 0;

  .logo-icon {
    font-size: 28px;
    color: #ffd04b;
  }

  .logo-text {
    margin-left: 10px;
    font-size: 16px;
    font-weight: bold;
    color: #fff;
    white-space: nowrap;
  }
}

.sidebar-scrollbar {
  flex: 1;
  overflow-x: hidden;
}

:deep(.el-menu) {
  border-right: none;
}

:deep(.el-menu-item),
:deep(.el-sub-menu__title) {
  height: 50px;
  line-height: 50px;

  &:hover {
    background-color: #263445 !important;
  }
}

:deep(.el-menu-item.is-active) {
  background-color: #263445 !important;
}

:deep(.el-sub-menu .el-menu-item) {
  min-width: 220px;
  background-color: #1f2d3d !important;

  &:hover {
    background-color: #263445 !important;
  }

  &.is-active {
    color: #ffd04b;
  }
}

.sidebar-collapse-btn {
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: #2b2f3a;
  border-top: 1px solid #1f2d3d;
  color: #bfcbd9;
  cursor: pointer;
  transition: all 0.3s;
  flex-shrink: 0;

  &:hover {
    color: #ffd04b;
    background-color: #263445;
  }

  .el-icon {
    font-size: 18px;
  }
}
</style>
