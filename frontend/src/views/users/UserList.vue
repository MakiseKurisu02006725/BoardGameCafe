<template>
  <div class="user-list">
    <div class="page-header">
      <el-button
        v-if="userStore.isAdmin"
        type="primary"
        :icon="Plus"
        @click="handleAdd"
      >
        新增员工
      </el-button>
    </div>

    <SearchBar
      ref="searchBarRef"
      :initial-values="searchInitial"
      @search="handleSearch"
      @reset="handleReset"
    >
      <template #default="{ form }">
        <el-form-item label="关键词">
          <el-input
            v-model="form.keyword"
            placeholder="用户名/手机号/邮箱"
            clearable
            style="width: 200px"
          />
        </el-form-item>
        <el-form-item label="角色">
          <el-select v-model="form.role" placeholder="请选择角色" clearable style="width: 160px">
            <el-option label="管理员" value="Admin" />
            <el-option label="经理" value="Manager" />
            <el-option label="收银员" value="Cashier" />
          </el-select>
        </el-form-item>
      </template>
    </SearchBar>

    <DataTable
      :columns="columns"
      :data="tableData"
      :loading="loading"
      :pagination="pagination"
      show-index
      :actions-width="260"
      @pagination-change="handlePaginationChange"
    >
      <template #role="{ row }">
        <el-tag
          :type="getRoleTagType(row.role)"
          effect="dark"
          round
        >
          {{ getRoleText(row.role) }}
        </el-tag>
      </template>
      <template #passwordStatus="{ row }">
        <el-tag
          v-if="row.hasPassword !== false"
          type="success"
          effect="plain"
          size="small"
        >
          已设置
        </el-tag>
        <el-tag
          v-else
          type="danger"
          effect="plain"
          size="small"
        >
          未设置
        </el-tag>
      </template>
      <template #actions="{ row }">
        <el-button
          v-if="userStore.isAdmin || (userStore.isManager && row.role !== 'Admin')"
          type="primary"
          link
          size="small"
          @click.stop="handleEdit(row)"
        >
          编辑
        </el-button>
        <el-button
          v-if="userStore.isAdmin && row.role !== 'Admin'"
          type="danger"
          link
          size="small"
          @click.stop="handleDelete(row)"
        >
          删除
        </el-button>
        <el-button
          type="warning"
          link
          size="small"
          @click.stop="handleChangePassword(row)"
        >
          改密码
        </el-button>
      </template>
    </DataTable>

    <el-dialog
      v-model="dialogVisible"
      :title="isEdit ? '编辑员工' : '新增员工'"
      width="520px"
      @close="resetForm"
    >
      <el-form
        ref="formRef"
        :model="formData"
        :rules="formRules"
        label-width="100px"
      >
        <el-form-item label="用户名" prop="userName">
          <el-input
            v-model="formData.userName"
            placeholder="请输入用户名"
            :disabled="isEdit"
          />
        </el-form-item>
        <el-form-item v-if="!isEdit" label="密码" prop="password">
          <el-input
            v-model="formData.password"
            type="password"
            placeholder="6-20位密码"
            show-password
            maxlength="20"
          />
        </el-form-item>
        <el-form-item v-else label="密码">
          <el-input
            v-model="formData.password"
            type="password"
            placeholder="不修改请留空"
            show-password
            maxlength="20"
          />
          <div class="form-tip">留空则不修改密码</div>
        </el-form-item>
        <el-form-item label="手机号" prop="phone">
          <el-input
            v-model="formData.phone"
            placeholder="请输入手机号"
            maxlength="11"
          />
        </el-form-item>
        <el-form-item label="角色" prop="role">
          <el-select v-model="formData.role" placeholder="请选择角色" style="width: 100%">
            <el-option label="管理员" value="Admin" v-if="userStore.isAdmin" />
            <el-option label="经理" value="Manager" />
            <el-option label="收银员" value="Cashier" />
          </el-select>
        </el-form-item>
        <el-form-item label="邮箱" prop="email">
          <el-input v-model="formData.email" placeholder="请输入邮箱" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="submitting" @click="handleSubmit">确认</el-button>
      </template>
    </el-dialog>

    <el-dialog
      v-model="passwordDialogVisible"
      :title="passwordDialogTitle"
      width="460px"
      @close="resetPasswordForm"
    >
      <el-form
        ref="passwordFormRef"
        :model="passwordForm"
        :rules="passwordRules"
        label-width="100px"
      >
        <el-form-item v-if="!userStore.isAdmin" label="原密码" prop="oldPassword">
          <el-input
            v-model="passwordForm.oldPassword"
            type="password"
            placeholder="请输入原密码"
            show-password
            maxlength="20"
          />
        </el-form-item>
        <el-form-item label="新密码" prop="newPassword">
          <el-input
            v-model="passwordForm.newPassword"
            type="password"
            placeholder="6-20位新密码"
            show-password
            maxlength="20"
          />
        </el-form-item>
        <el-form-item label="确认新密码" prop="confirmPassword">
          <el-input
            v-model="passwordForm.confirmPassword"
            type="password"
            placeholder="请再次输入新密码"
            show-password
            maxlength="20"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="passwordDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="passwordSubmitting" @click="submitPassword">
          {{ userStore.isAdmin ? '确认重置' : '确认修改' }}
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus } from '@element-plus/icons-vue'
import SearchBar from '@/components/SearchBar/index.vue'
import DataTable from '@/components/DataTable/index.vue'
import { useUserStore } from '@/store/modules/user'
import * as userApi from '@/api/user'

const userStore = useUserStore()
const searchBarRef = ref(null)
const formRef = ref(null)
const passwordFormRef = ref(null)
const loading = ref(false)
const submitting = ref(false)
const passwordSubmitting = ref(false)
const dialogVisible = ref(false)
const passwordDialogVisible = ref(false)
const isEdit = ref(false)
const currentUserId = ref(null)
const tableData = ref([])

// 初始值使用独立常量对象：避免 SearchBar 重置时被已输入值污染
const searchInitial = {
  keyword: '',
  role: ''
}

const searchForm = reactive({ ...searchInitial })

const pagination = reactive({
  total: 0,
  pageIndex: 1,
  pageSize: 10
})

const columns = [
  { prop: 'userName', label: '用户名', width: 140 },
  { prop: 'roleName', label: '角色', width: 120, slot: 'role' },
  { prop: 'phone', label: '手机号', width: 140 },
  { prop: 'email', label: '邮箱', width: 200 },
  { prop: 'createTime', label: '创建时间', width: 180 },
  { prop: 'hasPassword', label: '密码状态', width: 100, slot: 'passwordStatus' }
]

const formData = reactive({
  userName: '',
  password: '',
  phone: '',
  role: 'Cashier',
  email: ''
})

const validateConfirm = (rule, value, callback) => {
  if (value !== passwordForm.newPassword) {
    callback(new Error('两次密码不一致'))
  } else {
    callback()
  }
}

const formRules = {
  userName: [{ required: true, message: '请输入用户名', trigger: 'blur' }],
  password: [
    { required: true, message: '请输入密码', trigger: 'blur' },
    { min: 6, max: 20, message: '密码长度6-20位', trigger: 'blur' }
  ],
  phone: [
    { required: true, message: '请输入手机号', trigger: 'blur' },
    { pattern: /^1[3-9]\d{9}$/, message: '请输入正确的手机号', trigger: 'blur' }
  ],
  role: [{ required: true, message: '请选择角色', trigger: 'change' }],
  email: [
    { type: 'email', message: '请输入正确的邮箱地址', trigger: 'blur' }
  ]
}

const passwordForm = reactive({
  oldPassword: '',
  newPassword: '',
  confirmPassword: ''
})

const passwordRules = computed(() => ({
  oldPassword: userStore.isAdmin
    ? []
    : [{ required: true, message: '请输入原密码', trigger: 'blur' }],
  newPassword: [
    { required: true, message: '请输入新密码', trigger: 'blur' },
    { min: 6, max: 20, message: '密码长度6-20位', trigger: 'blur' }
  ],
  confirmPassword: [
    { required: true, message: '请再次输入新密码', trigger: 'blur' },
    { validator: validateConfirm, trigger: 'blur' }
  ]
}))

const passwordDialogTitle = computed(() =>
  userStore.isAdmin ? '重置密码' : '修改密码'
)

function getRoleText(role) {
  const map = { Admin: '管理员', Manager: '经理', Cashier: '收银员' }
  return map[role] || role
}

function getRoleTagType(role) {
  const map = { Admin: 'danger', Manager: 'primary', Cashier: 'success' }
  return map[role] || 'info'
}

function fetchData() {
  loading.value = true
  const params = {
    pageIndex: pagination.pageIndex,
    pageSize: pagination.pageSize,
    keyword: searchForm.keyword || undefined,
    role: searchForm.role || undefined
  }
  userApi.getList(params).then((res) => {
    const data = res.items || res.data || []
    tableData.value = data.map(u => ({
      ...u,
      createTime: u.createTime || u.createdAt || u.CreatedAt || '-',
      hasPassword: u.hasPassword !== undefined ? u.hasPassword : true
    }))
    pagination.total = res.totalCount || res.total || 0
  }).catch((err) => {
    console.error('获取员工列表失败:', err)
    tableData.value = []
    pagination.total = 0
  }).finally(() => {
    loading.value = false
  })
}

function handleSearch() {
  pagination.pageIndex = 1
  fetchData()
}

function handleReset() {
  pagination.pageIndex = 1
  fetchData()
}

function handlePaginationChange(val) {
  pagination.pageIndex = val.pageIndex
  pagination.pageSize = val.pageSize
  fetchData()
}

function handleAdd() {
  isEdit.value = false
  currentUserId.value = null
  resetForm()
  dialogVisible.value = true
}

function handleEdit(row) {
  isEdit.value = true
  currentUserId.value = row.id
  Object.assign(formData, {
    userName: row.userName,
    password: '',
    phone: row.phone,
    role: row.role,
    email: row.email
  })
  dialogVisible.value = true
}

function resetForm() {
  Object.assign(formData, {
    userName: '',
    password: '',
    phone: '',
    role: 'Cashier',
    email: ''
  })
  formRef.value?.clearValidate()
}

async function handleSubmit() {
  try {
    await formRef.value.validate()
  } catch {
    return
  }
  submitting.value = true
  const payload = { ...formData }
  if (isEdit.value && !payload.password) {
    delete payload.password
  }
  const req = isEdit.value
    ? userApi.update(currentUserId.value, payload)
    : userApi.create(payload)
  req.then(() => {
    ElMessage.success(isEdit.value ? '编辑成功' : '创建成功')
    dialogVisible.value = false
    fetchData()
  }).catch((err) => {
    ElMessage.error(err?.message || '操作失败')
  }).finally(() => {
    submitting.value = false
  })
}

function handleDelete(row) {
  ElMessageBox.confirm(`确定删除员工【${row.userName}】？`, '提示', {
    type: 'warning',
    confirmButtonText: '删除',
    cancelButtonText: '取消'
  }).then(() => {
    userApi.remove(row.id).then(() => {
      ElMessage.success('删除成功')
      fetchData()
    })
  }).catch(() => {})
}

function handleChangePassword(row) {
  currentUserId.value = row.id
  resetPasswordForm()
  passwordDialogVisible.value = true
}

function resetPasswordForm() {
  Object.assign(passwordForm, {
    oldPassword: '',
    newPassword: '',
    confirmPassword: ''
  })
  passwordFormRef.value?.clearValidate()
}

async function submitPassword() {
  try {
    await passwordFormRef.value.validate()
  } catch {
    return
  }
  passwordSubmitting.value = true

  const payload = {
    newPassword: passwordForm.newPassword
  }

  const req = userStore.isAdmin
    ? userApi.resetPassword(currentUserId.value, payload)
    : userApi.changePassword({
        oldPassword: passwordForm.oldPassword,
        newPassword: passwordForm.newPassword
      })

  req.then(() => {
    ElMessage.success(userStore.isAdmin ? '密码重置成功' : '密码修改成功')
    passwordDialogVisible.value = false
  }).catch((err) => {
    ElMessage.error(err?.message || '操作失败')
  }).finally(() => {
    passwordSubmitting.value = false
  })
}

onMounted(() => {
  fetchData()
})
</script>

<style lang="scss" scoped>
.user-list {
  .page-header {
    margin-bottom: 20px;
  }

  .form-tip {
    font-size: 12px;
    color: #909399;
    margin-top: 4px;
  }
}
</style>
