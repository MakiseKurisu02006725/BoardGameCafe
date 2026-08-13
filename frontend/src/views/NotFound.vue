<template>
  <div class="not-found-container">
    <el-empty description="抱歉，您访问的页面不存在" :image-size="200">
      <template #image>
        <div class="not-found-code">
          <span>4</span>
          <el-icon class="zero-icon"><CircleCloseFilled /></el-icon>
          <span>4</span>
        </div>
      </template>
      <div class="not-found-actions">
        <el-button type="primary" :icon="HomeFilled" @click="goHome">{{ isLoggedIn ? '返回首页' : '去登录' }}</el-button>
        <el-button :icon="Back" @click="goBack">返回上一页</el-button>
      </div>
    </el-empty>
  </div>
</template>

<script setup>
import { ref, onMounted, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import { HomeFilled, Back, CircleCloseFilled } from '@element-plus/icons-vue'
import { useUserStore } from '@/store/modules/user'
import { generateAndAddRoutes } from '@/router'

const router = useRouter()
const userStore = useUserStore()
const isLoggedIn = ref(false)

onMounted(() => {
  isLoggedIn.value = !!userStore.token
})

async function goHome() {
  if (userStore.token) {
    // 已登录：确保动态路由已注册后再跳转
    if (!userStore.rolesLoaded) {
      try {
        await userStore.getUserInfo()
      } catch {
        router.replace('/login')
        return
      }
    }
    const accessRoutes = generateAndAddRoutes(userStore.role)
    userStore.routes = accessRoutes
    await nextTick()
    // 根据角色跳转
    const roleRoutes = {
      'Admin': '/dashboard',
      'Manager': '/dashboard',
      'Cashier': '/orders',
      'Member': '/members/profile'
    }
    router.replace(roleRoutes[userStore.role] || '/dashboard')
  } else {
    // 未登录：跳转到登录页
    router.replace('/login')
  }
}

function goBack() {
  // 如果有历史记录则返回，否则跳转到首页
  if (window.history.length > 1) {
    router.back()
  } else {
    goHome()
  }
}
</script>

<style lang="scss" scoped>
.not-found-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: #f0f2f5;
}

.not-found-code {
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 120px;
  font-weight: bold;
  color: #409eff;
  line-height: 1;

  .zero-icon {
    font-size: 100px;
    color: #f56c6c;
    margin: 0 10px;
  }
}

.not-found-actions {
  margin-top: 20px;
  display: flex;
  gap: 16px;
  justify-content: center;
}
</style>
