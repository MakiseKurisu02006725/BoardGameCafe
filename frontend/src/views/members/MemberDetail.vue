<template>
  <div class="member-detail">
    <el-page-header @back="goBack" content="会员详情" style="margin-bottom: 20px" />

    <el-row :gutter="20">
      <el-col :span="8">
        <el-card v-loading="loading">
          <template #header>
            <span>基本信息</span>
          </template>
          <div class="member-info">
            <el-avatar :src="resolveAvatar(member.avatar)" :size="80">
              {{ member.nickName?.charAt(0) }}
            </el-avatar>
            <h3>{{ member.nickName }}</h3>
            <el-tag :type="getLevelTagType(member.level)" effect="dark" round>
              {{ getLevelLabel(member.level) }}
            </el-tag>
          </div>
          <el-descriptions :column="1" border size="small" style="margin-top: 20px">
            <el-descriptions-item label="手机号">{{ member.phone || '-' }}</el-descriptions-item>
            <el-descriptions-item label="邮箱">{{ member.email || '-' }}</el-descriptions-item>
            <el-descriptions-item label="生日">{{ member.birthday || '-' }}</el-descriptions-item>
            <el-descriptions-item label="注册时间">{{ member.createdAt || '-' }}</el-descriptions-item>
          </el-descriptions>
        </el-card>
      </el-col>

      <el-col :span="16">
        <el-row :gutter="20">
          <el-col :span="8">
            <el-card>
              <div class="stat-item">
                <div class="stat-value">{{ member.points || 0 }}</div>
                <div class="stat-label">积分</div>
              </div>
            </el-card>
          </el-col>
          <el-col :span="8">
            <el-card>
              <div class="stat-item">
                <div class="stat-value">¥{{ member.balance?.toFixed(2) || '0.00' }}</div>
                <div class="stat-label">余额</div>
              </div>
            </el-card>
          </el-col>
          <el-col :span="8">
            <el-card>
              <div class="stat-item">
                <div class="stat-value">{{ orderCount }}</div>
                <div class="stat-label">订单数</div>
              </div>
            </el-card>
          </el-col>
        </el-row>

        <el-card style="margin-top: 20px">
          <template #header>
            <div class="card-header">
              <span>操作记录</span>
              <el-button type="primary" link @click="goEdit">编辑会员</el-button>
            </div>
          </template>
          <el-tabs v-model="activeTab">
            <el-tab-pane label="充值记录" name="recharge">
              <el-table :data="rechargeList" border size="small">
                <el-table-column prop="createdAt" label="时间" width="180" />
                <el-table-column prop="amount" label="充值金额" width="120">
                  <template #default="{ row }">
                    <span style="color: #67c23a">+¥{{ row.amount }}</span>
                  </template>
                </el-table-column>
                <el-table-column prop="bonus" label="赠送金额" width="120">
                  <template #default="{ row }">
                    <span v-if="row.bonus" style="color: #e6a23c">+¥{{ row.bonus }}</span>
                    <span v-else>-</span>
                  </template>
                </el-table-column>
                <el-table-column label="支付方式" width="120">
                  <template #default="{ row }">
                    {{ getPayMethodText(row.payMethod) }}
                  </template>
                </el-table-column>
              </el-table>
              <el-empty v-if="!rechargeList.length" description="暂无充值记录" />
            </el-tab-pane>

            <el-tab-pane label="消费记录" name="order">
              <el-table :data="orderList" border size="small">
                <el-table-column prop="orderNo" label="订单号" width="180" />
                <el-table-column prop="createdAt" label="时间" width="180" />
                <el-table-column prop="totalAmount" label="消费金额" width="120">
                  <template #default="{ row }">
                    ¥{{ row.totalAmount }}
                  </template>
                </el-table-column>
                <el-table-column label="支付方式" width="120">
                  <template #default="{ row }">
                    {{ getPayMethodText(row.paymentMethod) }}
                  </template>
                </el-table-column>
                <el-table-column prop="status" label="状态" width="100">
                  <template #default="{ row }">
                    {{ getStatusText(row.status) }}
                  </template>
                </el-table-column>
              </el-table>
              <el-empty v-if="!orderList.length" description="暂无消费记录" />
            </el-tab-pane>

            <el-tab-pane label="余额流水" name="balance">
              <el-table :data="balanceRecordList" border size="small">
                <el-table-column label="时间" width="180">
                  <template #default="{ row }">
                    {{ formatTime(row.createdAt) }}
                  </template>
                </el-table-column>
                <el-table-column prop="typeName" label="类型" width="120" />
                <el-table-column label="金额" width="120">
                  <template #default="{ row }">
                    <span :style="{ color: row.amount < 0 ? '#f56c6c' : '#67c23a' }">
                      {{ row.amount < 0 ? '-' : '+' }}¥{{ Math.abs(row.amount).toFixed(2) }}
                    </span>
                  </template>
                </el-table-column>
                <el-table-column prop="orderNo" label="关联订单" width="170">
                  <template #default="{ row }">{{ row.orderNo || '-' }}</template>
                </el-table-column>
                <el-table-column prop="description" label="说明" />
              </el-table>
              <el-empty v-if="!balanceRecordList.length" description="暂无余额流水" />
            </el-tab-pane>
          </el-tabs>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import MoneyDisplay from '@/components/MoneyDisplay/index.vue'
import * as memberApi from '@/api/member'

const route = useRoute()
const router = useRouter()

const loading = ref(false)
const activeTab = ref('recharge')
const rechargeList = ref([])
const orderList = ref([])
const balanceRecordList = ref([])

const member = reactive({
  id: '',
  avatar: '',
  nickName: '',
  phone: '',
  email: '',
  level: 1,
  points: 0,
  balance: 0,
  birthday: '',
  createdAt: ''
})

const orderCount = computed(() => orderList.value.length)

// Guid 格式校验：避免无效ID（如 undefined）导致后端返回 400
const GUID_PATTERN = /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/

function getValidMemberId() {
  const id = route.params.id
  if (!id || !GUID_PATTERN.test(id)) {
    // 无有效ID时自动跳回会员列表，而非停留在错误页
    ElMessage.warning('无效的会员ID，已返回会员列表')
    router.replace('/members')
    return null
  }
  return id
}

function resolveAvatar(url) {
  if (!url) return ''
  if (url.startsWith('http')) return url
  const prefix = import.meta.env.VITE_UPLOAD_PREFIX || ''
  return prefix + url
}

function getLevelLabel(level) {
  const map = { 1: '青铜', 2: '白银', 3: '黄金', 4: '钻石', Bronze: '青铜', Silver: '白银', Gold: '黄金', Diamond: '钻石' }
  return map[level] || ''
}

function getLevelTagType(level) {
  const map = { 1: 'info', 2: 'primary', 3: 'warning', 4: 'danger', Bronze: 'info', Silver: 'primary', Gold: 'warning', Diamond: 'danger' }
  return map[level] || 'info'
}

function getStatusText(status) {
  const map = { 1: '待支付', 2: '已支付', 3: '已取消', 4: '已退款' }
  return map[status] || status
}

function getPayMethodText(method) {
  const map = {
    WeChat: '微信支付',
    Alipay: '支付宝',
    Cash: '现金',
    MemberBalance: '会员余额',
    1: '微信支付',
    2: '支付宝',
    3: '现金',
    4: '会员余额'
  }
  return map[method] || method || '-'
}

async function fetchData() {
  const id = getValidMemberId()
  if (!id) return
  loading.value = true
  try {
    const res = await memberApi.getById(id)
    const data = res.data || res
    Object.assign(member, {
      id: data.id || id,
      avatar: data.avatar || '',
      nickName: data.nickName || data.nickname || '',
      phone: data.phone || data.Phone || '',
      email: data.email || data.Email || '',
      level: data.level || data.Level || 1,
      points: data.points || data.Points || 0,
      balance: data.balance || data.Balance || 0,
      birthday: data.birthday || (data.Birthday ? new Date(data.Birthday).toLocaleDateString() : ''),
      createdAt: data.createdAt || (data.CreatedAt ? new Date(data.CreatedAt).toLocaleString() : '')
    })
  } catch (error) {
    console.error('获取会员详情失败:', error)
    ElMessage.error('获取会员详情失败')
  } finally {
    loading.value = false
  }
}

async function fetchRecharges() {
  const id = getValidMemberId()
  if (!id) return
  try {
    const res = await memberApi.getRecharges(id, { pageSize: 20 })
    rechargeList.value = res.items || res.data || res || []
  } catch {
    rechargeList.value = []
  }
}

async function fetchOrders() {
  const id = getValidMemberId()
  if (!id) return
  try {
    const res = await memberApi.getOrders(id, { pageSize: 20 })
    orderList.value = res.items || res.data || res || []
  } catch {
    orderList.value = []
  }
}

async function fetchBalanceRecords() {
  const id = getValidMemberId()
  if (!id) return
  try {
    const res = await memberApi.getBalanceRecords(id, { pageSize: 20 })
    balanceRecordList.value = res.items || res.data || res || []
  } catch {
    balanceRecordList.value = []
  }
}

function formatTime(value) {
  if (!value) return '-'
  const d = new Date(value)
  if (isNaN(d.getTime())) return value
  const pad = (n) => (n < 10 ? '0' + n : '' + n)
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}`
}

function goBack() {
  router.push('/members')
}

function goEdit() {
  router.push('/members')
}

onMounted(() => {
  fetchData()
  fetchRecharges()
  fetchOrders()
  fetchBalanceRecords()
})
</script>

<style lang="scss" scoped>
.member-detail {
  .member-info {
    text-align: center;
    padding: 20px 0;

    h3 {
      margin: 12px 0 8px;
      font-size: 18px;
    }
  }

  .stat-item {
    text-align: center;
    padding: 10px 0;

    .stat-value {
      font-size: 28px;
      font-weight: 600;
      color: #409eff;
    }

    .stat-label {
      font-size: 14px;
      color: #909399;
      margin-top: 4px;
    }
  }

  .card-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
  }
}
</style>
