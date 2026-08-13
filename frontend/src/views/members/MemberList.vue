<template>
  <div class="member-list-page">
    <SearchBar :initial-values="searchInitial" @search="handleSearch" @reset="handleReset">
      <template #default="{ form }">
        <el-form-item label="关键词">
          <el-input
            v-model="form.keyword"
            placeholder="手机号/昵称"
            clearable
            style="width: 200px"
          />
        </el-form-item>
        <el-form-item label="等级">
          <el-select
            v-model="form.level"
            placeholder="全部等级"
            clearable
            style="width: 160px"
          >
            <el-option label="青铜" :value="1" />
            <el-option label="白银" :value="2" />
            <el-option label="黄金" :value="3" />
          </el-select>
        </el-form-item>
      </template>
    </SearchBar>

    <div class="page-toolbar">
      <el-button type="primary" :icon="Plus" @click="handleCreate">
        新增会员
      </el-button>
    </div>

    <DataTable
      :columns="columns"
      :data="tableData"
      :loading="loading"
      :pagination="pagination"
      show-index
      :actions-width="280"
      @pagination-change="handlePaginationChange"
    >
      <template #avatar="{ row }">
        <el-avatar :src="resolveAvatar(row.avatar)" :size="40">
          {{ row.nickName?.charAt(0) }}
        </el-avatar>
      </template>

      <template #level="{ row }">
        <el-tag :type="getLevelTagType(row.level)" effect="light" round>
          {{ row.levelName || getLevelLabel(row.level) || '-' }}
        </el-tag>
      </template>

      <template #balance="{ row }">
        <MoneyDisplay :value="row.balance" />
      </template>

      <template #actions="{ row }">
        <el-button link type="primary" @click="handleDetail(row)">详情</el-button>
        <el-button link type="success" @click="handleRecharge(row)">充值</el-button>
        <el-button link type="warning" @click="handleEdit(row)">编辑</el-button>
        <el-button link type="danger" @click="handleDelete(row)">删除</el-button>
      </template>
    </DataTable>

    <el-dialog
      v-model="formDialogVisible"
      :title="formDialogTitle"
      width="500px"
      :close-on-click-modal="false"
    >
      <el-form
        ref="memberFormRef"
        :model="memberForm"
        :rules="memberFormRules"
        label-width="80px"
      >
        <el-form-item label="头像" prop="avatar">
          <FileUpload v-model="memberForm.avatar" />
        </el-form-item>
        <el-form-item label="昵称" prop="nickName">
          <el-input v-model="memberForm.nickName" placeholder="请输入昵称" maxlength="20" />
        </el-form-item>
        <el-form-item label="手机号" prop="phone">
          <el-input v-model="memberForm.phone" placeholder="请输入手机号" maxlength="11" />
        </el-form-item>
        <el-form-item label="等级" prop="level">
          <el-select v-model="memberForm.level" placeholder="请选择等级" style="width: 100%">
            <el-option label="青铜会员" :value="1" />
            <el-option label="白银会员" :value="2" />
            <el-option label="黄金会员" :value="3" />
            <el-option label="钻石会员" :value="4" />
          </el-select>
        </el-form-item>
        <el-form-item label="生日" prop="birthday">
          <el-date-picker
            v-model="memberForm.birthday"
            type="date"
            placeholder="选择生日"
            value-format="YYYY-MM-DD"
            style="width: 100%"
          />
        </el-form-item>
        <el-form-item label="邮箱" prop="email">
          <el-input v-model="memberForm.email" placeholder="请输入邮箱" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="formDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="formSubmitting" @click="handleSubmitForm">
          确定
        </el-button>
      </template>
    </el-dialog>

    <el-dialog
      v-model="rechargeDialogVisible"
      title="会员充值"
      width="420px"
      :close-on-click-modal="false"
    >
      <div v-if="currentMember" class="recharge-member-info">
        <el-avatar :src="resolveAvatar(currentMember.avatar)" :size="48" />
        <div class="info-text">
          <div class="nickname">{{ currentMember.nickName }}</div>
          <div class="balance">
            当前余额: <MoneyDisplay :value="currentMember.balance" />
          </div>
        </div>
      </div>
      <el-form
        ref="rechargeFormRef"
        :model="rechargeForm"
        :rules="rechargeFormRules"
        label-width="100px"
        style="margin-top: 20px"
      >
        <el-form-item label="充值金额" prop="amount">
          <el-input-number
            v-model="rechargeForm.amount"
            :min="0"
            :precision="2"
            :step="10"
            style="width: 100%"
          />
        </el-form-item>
        <el-form-item label="赠送金额" prop="bonusAmount">
          <el-input-number
            v-model="rechargeForm.bonusAmount"
            :min="0"
            :precision="2"
            :step="5"
            style="width: 100%"
          />
        </el-form-item>
        <el-form-item label="支付方式" prop="paymentMethod">
          <el-select v-model="rechargeForm.paymentMethod" placeholder="请选择支付方式" style="width: 100%">
            <el-option label="现金" value="Cash" />
            <el-option label="微信支付" value="WeChat" />
            <el-option label="支付宝" value="Alipay" />
            <el-option label="会员余额" value="MemberBalance" />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="rechargeDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="rechargeSubmitting" @click="handleSubmitRecharge">
          确认充值
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus } from '@element-plus/icons-vue'
import SearchBar from '@/components/SearchBar/index.vue'
import DataTable from '@/components/DataTable/index.vue'
import StatusTag from '@/components/StatusTag/index.vue'
import MoneyDisplay from '@/components/MoneyDisplay/index.vue'
import FileUpload from '@/components/FileUpload/index.vue'
import * as memberApi from '@/api/member'

const router = useRouter()

const UPLOAD_PREFIX = import.meta.env.VITE_UPLOAD_PREFIX || ''

const loading = ref(false)
const tableData = ref([])

const searchInitial = {
  keyword: '',
  level: ''
}

const searchForm = reactive({ ...searchInitial })

const pagination = reactive({
  total: 0,
  pageIndex: 1,
  pageSize: 10
})

const columns = [
  { prop: 'avatar', label: '头像', width: 80, slot: 'avatar' },
  { prop: 'nickName', label: '昵称', width: 100 },
  { prop: 'phone', label: '手机号', width: 130 },
  { prop: 'levelName', label: '等级', width: 100, slot: 'level' },
  { prop: 'points', label: '积分', width: 80 },
  { prop: 'balance', label: '余额', width: 120, slot: 'balance' },
  { prop: 'birthday', label: '生日', width: 120 },
  { prop: 'createdAt', label: '注册时间', width: 160 }
]

function resolveAvatar(url) {
  if (!url) return ''
  if (url.startsWith('http')) return url
  return UPLOAD_PREFIX + url
}

function getLevelLabel(level) {
  const map = { 1: '青铜', 2: '白银', 3: '黄金', Bronze: '青铜', Silver: '白银', Gold: '黄金' }
  return map[level] || ''
}

function getLevelTagType(level) {
  const map = { 1: 'info', 2: 'primary', 3: 'warning', Bronze: 'info', Silver: 'primary', Gold: 'warning' }
  return map[level] || 'info'
}

async function fetchList() {
  loading.value = true
  try {
    const params = {
      pageIndex: pagination.pageIndex,
      pageSize: pagination.pageSize,
      ...searchForm
    }
    const res = await memberApi.getList(params)
    if (Array.isArray(res)) {
      tableData.value = res
      pagination.total = res.length
    } else if (res && res.items) {
      tableData.value = res.items
      pagination.total = res.total || res.items.length
    } else if (res && Array.isArray(res)) {
      tableData.value = res
      pagination.total = res.length
    } else {
      tableData.value = []
      pagination.total = 0
    }
  } catch (error) {
    console.error('获取会员列表失败:', error)
    tableData.value = []
    pagination.total = 0
  } finally {
    loading.value = false
  }
}

function handleSearch(values) {
  Object.assign(searchForm, values)
  pagination.pageIndex = 1
  fetchList()
}

function handleReset(values) {
  Object.assign(searchForm, values)
  pagination.pageIndex = 1
  fetchList()
}

function handlePaginationChange({ pageIndex, pageSize }) {
  pagination.pageIndex = pageIndex
  pagination.pageSize = pageSize
  fetchList()
}

const formDialogVisible = ref(false)
const formDialogTitle = computed(() => (isEdit.value ? '编辑会员' : '新增会员'))
const isEdit = ref(false)
const formSubmitting = ref(false)
const memberFormRef = ref(null)
const currentMemberId = ref(null)

const memberForm = reactive({
  avatar: '',
  nickName: '',
  phone: '',
  level: 1,
  birthday: '',
  email: ''
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

const isEmail = (rule, value, callback) => {
  if (!value) {
    callback()
    return
  }
  const emailReg = /^[\w.-]+@[\w.-]+\.\w+$/
  if (!emailReg.test(value)) {
    callback(new Error('请输入正确的邮箱'))
    return
  }
  callback()
}

const memberFormRules = {
  nickName: [
    { required: true, message: '请输入昵称', trigger: 'blur' },
    { min: 2, max: 20, message: '昵称长度为2-20位', trigger: 'blur' }
  ],
  phone: [{ required: true, validator: isPhone, trigger: 'blur' }],
  level: [{ required: true, message: '请选择等级', trigger: 'change' }],
  email: [{ validator: isEmail, trigger: 'blur' }]
}

function handleCreate() {
  isEdit.value = false
  currentMemberId.value = null
  Object.assign(memberForm, {
    avatar: '',
    nickName: '',
    phone: '',
    level: 1,
    birthday: '',
    email: ''
  })
  formDialogVisible.value = true
}

// 后端 MemberDto.Level 序列化为枚举字符串（Bronze/Silver/Gold/Diamond），下拉用的是数字 1/2/3/4
function normalizeLevel(level) {
  const map = { Bronze: 1, Silver: 2, Gold: 3, Diamond: 4 }
  if (typeof level === 'number') return level
  return map[level] || 1
}

// 提交时数字 → 后端枚举字符串
function getLevelEnum(level) {
  const map = { 1: 'Bronze', 2: 'Silver', 3: 'Gold', 4: 'Diamond' }
  return map[level] || 'Bronze'
}

function handleEdit(row) {
  isEdit.value = true
  currentMemberId.value = row.id
  Object.assign(memberForm, {
    avatar: row.avatar || '',
    nickName: row.nickName || '',
    phone: row.phone || '',
    level: normalizeLevel(row.level),
    birthday: row.birthday || '',
    email: row.email || ''
  })
  formDialogVisible.value = true
}

async function handleSubmitForm() {
  if (!memberFormRef.value) return
  try {
    await memberFormRef.value.validate()
    formSubmitting.value = true
    if (isEdit.value) {
      const updateData = {
        nickName: memberForm.nickName,
        birthday: memberForm.birthday,
        email: memberForm.email,
        avatar: memberForm.avatar,
        phone: memberForm.phone,
        level: getLevelEnum(memberForm.level)
      }
      await memberApi.update(currentMemberId.value, updateData)
      ElMessage.success('编辑成功')
    } else {
      await memberApi.create({ ...memberForm })
      ElMessage.success('新增成功')
    }
    formDialogVisible.value = false
    fetchList()
  } catch (error) {
    if (error !== false) {
      console.error('提交失败:', error)
    }
  } finally {
    formSubmitting.value = false
  }
}

function handleDetail(row) {
  if (!row.id) {
    ElMessage.error('会员ID无效，无法查看详情')
    return
  }
  router.push(`/members/${row.id}`)
}

function handleDelete(row) {
  ElMessageBox.confirm(`确定删除会员【${row.nickName}】？删除后不可恢复`, '删除确认', {
    confirmButtonText: '删除',
    cancelButtonText: '取消',
    type: 'warning'
  }).then(async () => {
    try {
      await memberApi.remove(row.id)
      ElMessage.success('删除成功')
      fetchList()
    } catch (error) {
      if (error !== false) {
        console.error('删除失败:', error)
      }
    }
  }).catch(() => {})
}

const rechargeDialogVisible = ref(false)
const rechargeSubmitting = ref(false)
const rechargeFormRef = ref(null)
const currentMember = ref(null)

const rechargeForm = reactive({
  memberId: null,
  amount: 0,
  bonusAmount: 0,
  paymentMethod: ''
})

const rechargeFormRules = {
  amount: [{ required: true, message: '请输入充值金额', trigger: 'blur' }],
  paymentMethod: [{ required: true, message: '请选择支付方式', trigger: 'change' }]
}

function handleRecharge(row) {
  currentMember.value = row
  rechargeForm.memberId = row.id
  rechargeForm.amount = 0
  rechargeForm.bonusAmount = 0
  rechargeForm.paymentMethod = ''
  rechargeDialogVisible.value = true
}

async function handleSubmitRecharge() {
  if (!rechargeFormRef.value) return
  try {
    await rechargeFormRef.value.validate()
    await ElMessageBox.confirm(
      `确认充值 ¥${rechargeForm.amount}${rechargeForm.bonusAmount > 0 ? ` (赠送 ¥${rechargeForm.bonusAmount})` : ''}？`,
      '充值确认',
      {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      }
    )
    rechargeSubmitting.value = true
    await memberApi.recharge({
      memberId: rechargeForm.memberId,
      amount: rechargeForm.amount,
      bonus: rechargeForm.bonusAmount,
      payMethod: rechargeForm.paymentMethod
    })
    ElMessage.success('充值成功')
    rechargeDialogVisible.value = false
    fetchList()
  } catch (error) {
    if (error !== 'cancel' && error !== false) {
      ElMessage.error(error?.response?.data?.message || error?.message || '充值失败')
      console.error('充值失败:', error)
    }
  } finally {
    rechargeSubmitting.value = false
  }
}

onMounted(() => {
  fetchList()
})
</script>

<style lang="scss" scoped>
.member-list-page {
  .page-toolbar {
    margin-bottom: 16px;
  }
}

.recharge-member-info {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 12px 16px;
  background-color: #f5f7fa;
  border-radius: 8px;

  .info-text {
    .nickname {
      font-size: 16px;
      font-weight: 500;
      margin-bottom: 4px;
    }

    .balance {
      font-size: 14px;
      color: #606266;
    }
  }
}
</style>
