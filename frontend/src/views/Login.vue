<template>
  <div class="login-container">
    <div class="login-bg"></div>
    <el-card class="login-card" shadow="hover">
      <div class="login-header">
        <div class="login-logo">
          <el-icon class="logo-icon"><Grid /></el-icon>
          <span class="logo-title">桌游馆管理系统</span>
        </div>
        <p class="login-subtitle">欢迎登录</p>
      </div>
      <el-tabs v-model="activeTab" class="login-tabs">
        <el-tab-pane label="登录" name="login">
          <el-form
            ref="loginFormRef"
            :model="loginForm"
            :rules="loginRules"
            size="large"
            class="login-form"
          >
            <el-form-item prop="phone">
              <el-input
                v-model="loginForm.phone"
                placeholder="请输入手机号"
                :prefix-icon="Iphone"
                maxlength="11"
              />
            </el-form-item>
            <el-form-item prop="password">
              <el-input
                v-model="loginForm.password"
                type="password"
                placeholder="请输入密码"
                :prefix-icon="Lock"
                show-password
                maxlength="20"
                @keyup.enter="handleLogin"
              />
            </el-form-item>
            <el-form-item>
              <div class="login-options">
                <el-checkbox v-model="loginForm.rememberMe">记住我</el-checkbox>
              </div>
            </el-form-item>
            <el-form-item>
              <el-button
                type="primary"
                class="login-btn"
                :loading="loginLoading"
                @click="handleLogin"
              >
                登录
              </el-button>
            </el-form-item>
          </el-form>
        </el-tab-pane>
        <el-tab-pane label="注册" name="register">
          <el-form
            ref="registerFormRef"
            :model="registerForm"
            :rules="registerRules"
            size="large"
            class="register-form"
          >
            <el-form-item prop="phone">
              <el-input
                v-model="registerForm.phone"
                placeholder="请输入手机号"
                :prefix-icon="Iphone"
                maxlength="11"
              />
            </el-form-item>
            <el-form-item prop="nickName">
              <el-input
                v-model="registerForm.nickName"
                placeholder="请输入昵称"
                :prefix-icon="User"
                maxlength="20"
              />
            </el-form-item>
            <el-form-item prop="password">
              <el-input
                v-model="registerForm.password"
                type="password"
                placeholder="请输入密码 (8-20位)"
                :prefix-icon="Lock"
                show-password
                maxlength="20"
              />
            </el-form-item>
            <el-form-item prop="confirmPassword">
              <el-input
                v-model="registerForm.confirmPassword"
                type="password"
                placeholder="请再次输入密码"
                :prefix-icon="Lock"
                show-password
                maxlength="20"
                @keyup.enter="handleRegister"
              />
            </el-form-item>
            <el-form-item>
              <el-button
                type="primary"
                class="register-btn"
                :loading="registerLoading"
                @click="handleRegister"
              >
                注册
              </el-button>
            </el-form-item>
          </el-form>
        </el-tab-pane>
      </el-tabs>
    </el-card>
  </div>
</template>

<script setup>
import { ref, reactive, nextTick } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { useUserStore } from '@/store/modules/user'
import { Grid, Iphone, Lock, User } from '@element-plus/icons-vue'
import { generateAndAddRoutes } from '@/router'

const route = useRoute()
const router = useRouter()
const userStore = useUserStore()

const props = defineProps({
  defaultTab: {
    type: String,
    default: 'login'
  }
})

const activeTab = ref(props.defaultTab)
const loginLoading = ref(false)
const registerLoading = ref(false)
const loginFormRef = ref(null)
const registerFormRef = ref(null)

const loginForm = reactive({
  phone: '',
  password: '',
  rememberMe: false
})

const registerForm = reactive({
  phone: '',
  nickName: '',
  password: '',
  confirmPassword: ''
})

const isPhone = (rule, value, callback) => {
  if (!value) {
    callback(new Error('请输入手机号'))
    return
  }
  const phoneReg = /^1[3-9]\d{9}$/
  if (!phoneReg.test(value)) {
    callback(new Error('请输入正确的手机号'))
    return
  }
  callback()
}

const validateConfirmPassword = (rule, value, callback) => {
  if (!value) {
    callback(new Error('请再次输入密码'))
    return
  }
  if (value !== registerForm.password) {
    callback(new Error('两次输入的密码不一致'))
    return
  }
  callback()
}

const loginRules = {
  phone: [
    { required: true, validator: isPhone, trigger: 'blur' }
  ],
  password: [
    { required: true, message: '请输入密码', trigger: 'blur' }
  ]
}

const registerRules = {
  phone: [
    { required: true, validator: isPhone, trigger: 'blur' }
  ],
  nickName: [
    { required: true, message: '请输入昵称', trigger: 'blur' },
    { min: 2, max: 20, message: '昵称长度为2-20位', trigger: 'blur' }
  ],
  password: [
    { required: true, message: '请输入密码', trigger: 'blur' },
    { min: 8, max: 20, message: '密码长度为8-20位', trigger: 'blur' }
  ],
  confirmPassword: [
    { required: true, validator: validateConfirmPassword, trigger: 'blur' }
  ]
}

function getDefaultRouteByRole(role) {
  const roleRoutes = {
    'Admin': '/dashboard',
    'Manager': '/dashboard',
    'Cashier': '/orders',
    'Member': '/members/profile'
  }
  return roleRoutes[role] || '/dashboard'
}

async function handleLogin() {
  if (!loginFormRef.value) return
  try {
    await loginFormRef.value.validate()
    loginLoading.value = true
    await userStore.login({
      phone: loginForm.phone,
      password: loginForm.password
    })
    await userStore.getUserInfo()
    // 登录成功后立即注册动态路由
    const accessRoutes = generateAndAddRoutes(userStore.role)
    userStore.routes = accessRoutes
    ElMessage.success('登录成功')
    // 使用nextTick确保路由系统更新后再跳转
    await nextTick()
    const redirect = route.query.redirect
    if (redirect) {
      router.replace(decodeURIComponent(redirect))
    } else {
      router.replace(getDefaultRouteByRole(userStore.role))
    }
  } catch (error) {
    if (error !== false) {
      console.error('Login failed:', error)
    }
  } finally {
    loginLoading.value = false
  }
}

async function handleRegister() {
  if (!registerFormRef.value) return
  try {
    await registerFormRef.value.validate()
    registerLoading.value = true
    await userStore.register({
      phone: registerForm.phone,
      nickName: registerForm.nickName,
      password: registerForm.password
    })
    await userStore.getUserInfo()
    // 注册成功后立即注册动态路由
    const accessRoutes = generateAndAddRoutes(userStore.role)
    userStore.routes = accessRoutes
    ElMessage.success('注册成功，已自动登录')
    // 使用nextTick确保路由系统更新后再跳转
    await nextTick()
    const redirect = route.query.redirect
    if (redirect) {
      router.replace(decodeURIComponent(redirect))
    } else {
      router.replace(getDefaultRouteByRole(userStore.role))
    }
  } catch (error) {
    if (error !== false) {
      console.error('Register failed:', error)
    }
  } finally {
    registerLoading.value = false
  }
}
</script>

<style lang="scss" scoped>
.login-container {
  position: relative;
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
}

.login-bg {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  z-index: 0;

  &::before {
    content: '';
    position: absolute;
    top: -50%;
    right: -20%;
    width: 600px;
    height: 600px;
    background: rgba(255, 255, 255, 0.08);
    border-radius: 50%;
  }

  &::after {
    content: '';
    position: absolute;
    bottom: -30%;
    left: -10%;
    width: 400px;
    height: 400px;
    background: rgba(255, 255, 255, 0.06);
    border-radius: 50%;
  }
}

.login-card {
  position: relative;
  z-index: 1;
  width: 420px;
  padding: 20px 30px;
  border-radius: 12px;
}

.login-header {
  text-align: center;
  margin-bottom: 20px;
}

.login-logo {
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 10px;

  .logo-icon {
    font-size: 36px;
    color: #409eff;
    margin-right: 10px;
  }

  .logo-title {
    font-size: 24px;
    font-weight: bold;
    color: #303133;
  }
}

.login-subtitle {
  color: #909399;
  font-size: 14px;
  margin: 0;
}

.login-tabs {
  :deep(.el-tabs__header) {
    margin-bottom: 24px;
  }

  :deep(.el-tabs__nav-wrap::after) {
    height: 1px;
  }

  :deep(.el-tabs__item) {
    font-size: 16px;
    font-weight: 500;
  }
}

.login-form,
.register-form {
  .login-btn,
  .register-btn {
    width: 100%;
  }
}

.login-options {
  width: 100%;
  display: flex;
  justify-content: flex-start;
}
</style>
