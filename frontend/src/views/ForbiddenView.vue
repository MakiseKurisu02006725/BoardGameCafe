<template>
  <div class="forbidden-container">
    <el-empty description="抱歉，您没有权限访问该页面" :image-size="200">
      <template #image>
        <div class="forbidden-code">
          <el-icon class="forbidden-icon"><WarningFilled /></el-icon>
          <span>401</span>
        </div>
      </template>
      <div class="forbidden-actions">
        <el-button type="primary" :icon="HomeFilled" @click="goHome">返回首页</el-button>
        <el-button :icon="Back" @click="goBack">返回上一页</el-button>
      </div>
    </el-empty>
  </div>
</template>

<script setup>
import { useRouter } from 'vue-router'
import { HomeFilled, Back, WarningFilled } from '@element-plus/icons-vue'

const router = useRouter()

function goHome() {
  router.push('/dashboard')
}

function goBack() {
  router.back()
}
</script>

<style lang="scss" scoped>
.forbidden-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: #f0f2f5;
}

.forbidden-code {
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 120px;
  font-weight: bold;
  color: #e6a23c;
  line-height: 1;

  .forbidden-icon {
    font-size: 100px;
    color: #f56c6c;
    margin-right: 10px;
  }
}

.forbidden-actions {
  margin-top: 20px;
  display: flex;
  gap: 16px;
  justify-content: center;
}
</style>
