<template>
  <div class="navbar-wrapper">
    <div class="navbar-left">
      <div class="collapse-btn" @click="toggleSidebar">
        <el-icon>
          <Fold v-if="appStore.sidebarOpened" />
          <Expand v-else />
        </el-icon>
      </div>
      <el-breadcrumb separator="/" class="breadcrumb">
        <el-breadcrumb-item
          v-for="(item, index) in breadcrumbs"
          :key="index"
          :to="index < breadcrumbs.length - 1 ? item.path : undefined"
        >
          {{ item.meta && item.meta.title }}
        </el-breadcrumb-item>
      </el-breadcrumb>
    </div>
    <div class="navbar-right">
      <span class="today-date">
        <el-icon><Calendar /></el-icon>
        {{ todayDate }}
      </span>
      <el-dropdown
        trigger="click"
        @command="handleCommand"
        popper-class="navbar-user-dropdown"
        placement="bottom-end"
      >
        <div class="user-info">
          <el-avatar :size="32" class="user-avatar">
            <el-icon v-if="!userStore.avatar"><UserFilled /></el-icon>
            <img v-else :src="userStore.avatar" />
          </el-avatar>
          <span class="user-nickname">{{ userStore.nickName || userStore.userName || '用户' }}</span>
          <el-icon class="arrow-down"><ArrowDown /></el-icon>
        </div>
        <template #dropdown>
          <el-dropdown-menu>
            <el-dropdown-item command="profile">
              <el-icon><User /></el-icon>
              个人中心
            </el-dropdown-item>
            <el-dropdown-item command="password">
              <el-icon><Lock /></el-icon>
              修改密码
            </el-dropdown-item>
            <el-dropdown-item divided command="logout">
              <el-icon><SwitchButton /></el-icon>
              退出登录
            </el-dropdown-item>
          </el-dropdown-menu>
        </template>
      </el-dropdown>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessageBox, ElMessage } from 'element-plus'
import { useAppStore } from '@/store/modules/app'
import { useUserStore } from '@/store/modules/user'
import dayjs from 'dayjs'
import {
  Fold,
  Expand,
  Calendar,
  User,
  UserFilled,
  Lock,
  SwitchButton,
  ArrowDown
} from '@element-plus/icons-vue'

const route = useRoute()
const router = useRouter()
const appStore = useAppStore()
const userStore = useUserStore()

const todayDate = computed(() => dayjs().format('YYYY年MM月DD日 dddd'))

const breadcrumbs = computed(() => {
  return route.matched.filter(r => r.meta && r.meta.title)
})

function toggleSidebar() {
  appStore.toggleSidebar()
}

function handleCommand(command) {
  switch (command) {
    case 'profile':
      if (userStore.role === 'Member') {
        router.push('/members/profile')
      } else {
        ElMessage.info('该功能仅对会员开放，已为您跳转到会员管理页面')
        router.push('/members')
      }
      break
    case 'password':
      ElMessage.info('修改密码功能开发中')
      break
    case 'logout':
      handleLogout()
      break
  }
}

async function handleLogout() {
  try {
    await ElMessageBox.confirm('确定要退出登录吗？', '提示', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    })
    await userStore.logout()
    ElMessage.success('退出成功')
    router.push('/login')
  } catch (e) {
  }
}
</script>

<style lang="scss" scoped>
.navbar-wrapper {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 20px;
}

.navbar-left {
  display: flex;
  align-items: center;
  flex: 1;
  min-width: 0;
}

.collapse-btn {
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  border-radius: 4px;
  transition: all 0.3s;
  margin-right: 10px;

  &:hover {
    background-color: #f0f2f5;
  }

  .el-icon {
    font-size: 20px;
    color: #606266;
  }
}

.breadcrumb {
  flex: 1;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.navbar-right {
  display: flex;
  align-items: center;
  flex-shrink: 0;
}

.today-date {
  display: flex;
  align-items: center;
  margin-right: 20px;
  color: #606266;
  font-size: 14px;

  .el-icon {
    margin-right: 6px;
    color: #909399;
  }
}

.user-info {
  display: flex;
  align-items: center;
  cursor: pointer;
  padding: 5px 10px;
  border-radius: 4px;
  transition: all 0.3s;
  user-select: none;

  &:hover {
    background-color: #f0f2f5;
  }
}

.user-avatar {
  background-color: #409eff;
  display: flex;
  align-items: center;
  justify-content: center;

  .el-icon {
    font-size: 20px;
    color: #fff;
  }
}

.user-nickname {
  margin: 0 8px;
  font-size: 14px;
  color: #303133;
  max-width: 100px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.arrow-down {
  font-size: 12px;
  color: #909399;
}
</style>

<!-- 全局样式（非 scoped），因为弹出层会被 teleported 到 body，scoped 样式无法命中 -->
<style lang="scss">
.navbar-user-dropdown {
  z-index: 9999 !important;
  min-width: 160px;
  margin-top: 6px;
}

.navbar-user-dropdown .el-dropdown-menu {
  padding: 6px 0;
}

.navbar-user-dropdown .el-dropdown-menu__item {
  display: flex;
  align-items: center;
  gap: 8px;
}
</style>
