<template>
  <div class="order-list">
    <SearchBar
      ref="searchBarRef"
      :initial-values="searchInitial"
      @search="handleSearch"
      @reset="handleReset"
    >
      <template #default="{ form }">
        <el-form-item label="订单号">
          <el-input
            v-model="form.keyword"
            placeholder="订单号关键词"
            clearable
            style="width: 180px"
          />
        </el-form-item>
        <el-form-item label="状态">
          <el-select v-model="form.status" placeholder="请选择状态" clearable style="width: 160px">
            <el-option label="待支付" value="pending" />
            <el-option label="已支付" value="paid" />
            <el-option label="已完成" value="completed" />
            <el-option label="已退款" value="refunded" />
            <el-option label="已取消" value="cancelled" />
          </el-select>
        </el-form-item>
        <el-form-item label="日期范围">
          <el-date-picker
            v-model="form.dateRange"
            type="daterange"
            range-separator="至"
            start-placeholder="开始日期"
            end-placeholder="结束日期"
            value-format="YYYY-MM-DD"
            style="width: 280px"
          />
        </el-form-item>
        <el-form-item label="会员ID">
          <el-input
            v-model="form.memberId"
            placeholder="会员ID"
            clearable
            style="width: 160px"
          />
        </el-form-item>
      </template>
    </SearchBar>

    <DataTable
      :columns="columns"
      :data="tableData"
      :loading="loading"
      :pagination="pagination"
      show-index
      :actions-width="220"
      @pagination-change="handlePaginationChange"
    >
      <template #totalAmount="{ row }">
        <MoneyDisplay :value="row.totalAmount" />
      </template>
      <template #discount="{ row }">
        <span v-if="getDiscountText(row)">{{ getDiscountText(row) }}</span>
        <span v-else>-</span>
      </template>
      <template #paidAmount="{ row }">
        <MoneyDisplay :value="row.actualAmount" />
      </template>
      <template #status="{ row }">
        <StatusTag :status="row.statusName || getStatusText(row.status)" />
      </template>
      <template #actions="{ row }">
        <el-button type="primary" link size="small" @click.stop="handleDetail(row)">
          详情
        </el-button>
        <el-button
          v-if="row.status === 1 || row.statusName === '待支付'"
          type="primary"
          link
          size="small"
          @click.stop="handleCheckout(row)"
        >
          收银
        </el-button>
        <el-button
          v-if="row.status === 1 || row.statusName === '待支付'"
          type="success"
          link
          size="small"
          @click.stop="handlePay(row)"
        >
          支付
        </el-button>
        <el-button
          v-if="row.status === 2 || row.statusName === '已支付'"
          type="warning"
          link
          size="small"
          @click.stop="handleRefund(row)"
        >
          退款
        </el-button>
      </template>
    </DataTable>

    <el-drawer
      v-model="detailVisible"
      title="订单详情"
      direction="rtl"
      size="600px"
    >
      <div v-if="currentOrder" class="order-detail">
        <div class="detail-section">
          <h4>基本信息</h4>
          <el-descriptions :column="2" border size="small">
            <el-descriptions-item label="订单号">{{ currentOrder.id?.substring(0, 8) || '-' }}</el-descriptions-item>
            <el-descriptions-item label="状态">
              <StatusTag :status="currentOrder.statusName || getStatusText(currentOrder.status)" />
            </el-descriptions-item>
            <el-descriptions-item label="会员昵称">{{ currentOrder.memberName || '散客' }}</el-descriptions-item>
            <el-descriptions-item label="桌号">{{ currentOrder.tableCode || '-' }}</el-descriptions-item>
            <el-descriptions-item label="开台时间">{{ currentOrder.startTime || '-' }}</el-descriptions-item>
            <el-descriptions-item label="结账时间">{{ currentOrder.endTime || '-' }}</el-descriptions-item>
            <el-descriptions-item label="时长">{{ currentOrder.duration ? currentOrder.duration + '分钟' : '-' }}</el-descriptions-item>
            <el-descriptions-item label="支付方式">{{ getPayMethodText(currentOrder.paymentMethod) }}</el-descriptions-item>
            <el-descriptions-item label="总金额"><MoneyDisplay :value="currentOrder.totalAmount" /></el-descriptions-item>
            <el-descriptions-item label="实收金额"><MoneyDisplay :value="currentOrder.actualAmount" /></el-descriptions-item>
          </el-descriptions>
        </div>
        <div class="detail-section" v-if="currentOrder.boardGames?.length">
          <h4>游玩桌游</h4>
          <el-table :data="currentOrder.boardGames || []" border size="small">
            <el-table-column prop="name" label="桌游名称" min-width="120" />
            <el-table-column prop="gameNo" label="编号" width="100" />
            <el-table-column prop="quantity" label="数量" width="70" align="center" />
            <el-table-column label="状态" width="100" align="center">
              <template #default="{ row }">
                <el-tag :type="row.isReturned ? 'success' : 'warning'" size="small">
                  {{ row.isReturned ? '已归还' : '借出中' }}
                </el-tag>
              </template>
            </el-table-column>
          </el-table>
        </div>
        <div class="detail-section" v-if="currentOrder.orderItems?.length">
          <h4>订单项明细</h4>
          <el-table :data="currentOrder.orderItems || []" border size="small">
            <el-table-column prop="name" label="名称" />
            <el-table-column prop="quantity" label="数量" width="80" align="center" />
            <el-table-column prop="unitPrice" label="单价" width="100" align="center">
              <template #default="{ row }">
                <MoneyDisplay :value="row.unitPrice" />
              </template>
            </el-table-column>
            <el-table-column prop="subtotal" label="小计" width="100" align="center">
              <template #default="{ row }">
                <MoneyDisplay :value="row.subtotal" />
              </template>
            </el-table-column>
            <el-table-column prop="type" label="类型" width="100" align="center">
              <template #default="{ row }">
                {{ getItemTypeText(row.type) }}
              </template>
            </el-table-column>
          </el-table>
        </div>
      </div>
    </el-drawer>

    <el-dialog
      v-model="payDialogVisible"
      title="订单支付"
      width="520px"
      @close="resetPayForm"
    >
      <div v-if="currentOrder" class="pay-dialog">
        <el-descriptions :column="1" border size="small">
          <el-descriptions-item label="订单号">{{ currentOrder.orderNo }}</el-descriptions-item>
          <el-descriptions-item label="应付金额">
            <span class="pay-amount"><MoneyDisplay :value="currentOrder.totalAmount" /></span>
          </el-descriptions-item>
        </el-descriptions>
        <el-form
          ref="payFormRef"
          :model="payForm"
          :rules="payRules"
          label-width="100px"
          class="pay-form"
        >
          <el-form-item label="支付方式" prop="payMethod">
            <el-radio-group v-model="payForm.payMethod">
              <el-radio :label="1">微信</el-radio>
              <el-radio :label="2">支付宝</el-radio>
              <el-radio :label="3">现金</el-radio>
              <el-radio :label="4">会员余额</el-radio>
            </el-radio-group>
          </el-form-item>
          <el-form-item label="优惠券">
            <el-select
              v-model="payForm.couponId"
              placeholder="请选择优惠券"
              clearable
              style="width: 100%"
            >
              <el-option
                v-for="c in coupons"
                :key="c.id"
                :label="`${c.name} - 减¥${c.discountAmount}`"
                :value="c.id"
              />
            </el-select>
          </el-form-item>
          <el-form-item label="积分抵扣">
            <el-checkbox v-model="payForm.usePoints" :disabled="!memberPoints">
              可用积分 {{ memberPoints }} (1分=0.01元)
            </el-checkbox>
          </el-form-item>
          <el-form-item label="会员余额">
            <span>¥{{ memberBalance?.toFixed(2) || '0.00' }}</span>
          </el-form-item>
        </el-form>
      </div>
      <template #footer>
        <el-button @click="payDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="paySubmitting" @click="submitPay">确认支付</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import SearchBar from '@/components/SearchBar/index.vue'
import DataTable from '@/components/DataTable/index.vue'
import StatusTag from '@/components/StatusTag/index.vue'
import MoneyDisplay from '@/components/MoneyDisplay/index.vue'
import * as orderApi from '@/api/order'

const router = useRouter()
const searchBarRef = ref(null)
const payFormRef = ref(null)
const loading = ref(false)
const paySubmitting = ref(false)
const detailVisible = ref(false)
const payDialogVisible = ref(false)
const tableData = ref([])
const currentOrder = ref(null)
const coupons = ref([])
const memberPoints = ref(0)
const memberBalance = ref(0)

// 初始值使用独立常量对象：避免 SearchBar 重置时被已输入值污染
const searchInitial = {
  keyword: '',
  status: '',
  dateRange: [],
  memberId: ''
}

const searchForm = reactive({ ...searchInitial })

const pagination = reactive({
  total: 0,
  pageIndex: 1,
  pageSize: 10
})

const columns = [
  { prop: 'id', label: '订单号', width: 160, formatter: (row) => row.id ? row.id.substring(0, 8) + '...' : '-' },
  { prop: 'memberName', label: '会员昵称', width: 120, formatter: (row) => row.memberName || '散客' },
  { prop: 'tableCode', label: '桌号', width: 80 },
  { prop: 'startTime', label: '开台时间', width: 160 },
  { prop: 'endTime', label: '结账时间', width: 160 },
  { prop: 'duration', label: '时长(分)', width: 90 },
  { prop: 'boardGameNames', label: '游玩桌游', minWidth: 140, formatter: (row) => row.boardGameNames || '-' },
  { prop: 'totalAmount', label: '总金额', width: 110, slot: 'totalAmount' },
  { prop: 'discount', label: '折扣', width: 90, slot: 'discount' },
  { prop: 'actualAmount', label: '实收', width: 110, slot: 'paidAmount' },
  { prop: 'statusName', label: '状态', width: 100, slot: 'status' },
  {
    prop: 'paymentMethod',
    label: '支付方式',
    width: 100,
    formatter: (row) => getPayMethodText(row.paymentMethod)
  }
]

const payForm = reactive({
  payMethod: 1,
  couponId: null,
  usePoints: false
})

const payRules = {
  payMethod: [{ required: true, message: '请选择支付方式', trigger: 'change' }]
}

function getStatusText(status) {
  const map = {
    pending: '待支付',
    paid: '已支付',
    completed: '已完成',
    refunded: '已退款',
    cancelled: '已取消'
  }
  return map[status] || status
}

function getPayMethodText(method) {
  const map = {
    1: '微信',
    2: '支付宝',
    3: '现金',
    4: '会员余额',
    'wechat': '微信',
    'alipay': '支付宝',
    'cash': '现金',
    'balance': '会员余额',
    // 后端枚举字符串（JsonStringEnumConverter 大写）
    WeChat: '微信',
    Alipay: '支付宝',
    Cash: '现金',
    MemberBalance: '会员余额'
  }
  return map[method] || '-'
}

function getItemTypeText(type) {
  const map = {
    time: '计时费',
    rental: '桌游租赁',
    goods: '商品',
    service: '服务费'
  }
  return map[type] || type
}

// 折扣展示：10折（原价）/异常数据统一显示 '-'
function getDiscountText(row) {
  const rate = typeof row.discountRate === 'number' ? row.discountRate
    : typeof row.discount === 'number' ? row.discount
    : 10
  // 异常数据（<=0 或 >10）兜底显示 '-'
  if (rate <= 0 || rate > 10) return ''
  // 10 折 = 原价无优惠，显示 '-'
  if (rate >= 10) return ''
  return `${rate.toFixed(1)}折`
}

function fetchData() {
  loading.value = true
  const params = {
    pageIndex: pagination.pageIndex,
    pageSize: pagination.pageSize,
    keyword: searchForm.keyword || undefined,
    status: mapStatusToEnum(searchForm.status) || undefined,
    memberId: searchForm.memberId || undefined,
    dateFrom: searchForm.dateRange?.[0] || undefined,
    dateTo: searchForm.dateRange?.[1] || undefined
  }
  orderApi.getList(params).then((res) => {
    // 兼容不同的返回结构
    const data = res
    if (Array.isArray(data)) {
      tableData.value = data
      pagination.total = data.length
    } else {
      tableData.value = data.items || data.list || data.rows || []
      pagination.total = data.totalCount || data.total || 0
    }
  }).catch((err) => {
    console.error('Fetch orders failed:', err)
    tableData.value = []
    pagination.total = 0
  }).finally(() => {
    loading.value = false
  })
}

function mapStatusToEnum(status) {
  if (!status) return null
  const map = {
    'pending': 1,
    'paid': 2,
    'cancelled': 3,
    'refunded': 4
  }
  return map[status] || null
}

function handleSearch(values) {
  Object.assign(searchForm, values || {})
  pagination.pageIndex = 1
  fetchData()
}

function handleReset(values) {
  Object.assign(searchForm, values || searchInitial)
  pagination.pageIndex = 1
  fetchData()
}

function handlePaginationChange(val) {
  pagination.pageIndex = val.pageIndex
  pagination.pageSize = val.pageSize
  fetchData()
}

async function handleDetail(row) {
  try {
    const res = await orderApi.getById(row.id)
    currentOrder.value = res.data || res
    detailVisible.value = true
  } catch {
    currentOrder.value = row
    detailVisible.value = true
  }
}

function handlePay(row) {
  currentOrder.value = row
  resetPayForm()
  coupons.value = []
  memberPoints.value = 1000
  memberBalance.value = 500
  payDialogVisible.value = true
}

function handleCheckout(row) {
  if (!row.id) {
    ElMessage.error('订单ID无效，无法进入收银')
    return
  }
  router.push(`/orders/checkout/${row.id}`)
}

function resetPayForm() {
  Object.assign(payForm, {
    payMethod: 1,
    couponId: null,
    usePoints: false
  })
  payFormRef.value?.clearValidate()
}

async function submitPay() {
  try {
    await payFormRef.value.validate()
  } catch {
    return
  }
  paySubmitting.value = true
  const payload = {
    orderId: currentOrder.value.id,
    paymentMethod: getPayMethodEnum(payForm.payMethod),
    couponId: payForm.couponId || null,
    pointsDeduct: payForm.usePoints ? 100 : null,
    balanceDeduct: null
  }
  orderApi.pay(payload).then(() => {
    ElMessage.success('支付成功')
    payDialogVisible.value = false
    fetchData()
  }).catch((err) => {
    ElMessage.error(err?.message || '支付失败')
  }).finally(() => {
    paySubmitting.value = false
  })
}

// 后端启用 JsonStringEnumConverter，枚举必须传字符串
function getPayMethodEnum(method) {
  const map = { 1: 'WeChat', 2: 'Alipay', 3: 'Cash', 4: 'MemberBalance' }
  return map[method] || 'Cash'
}

function handleRefund(row) {
  const message = row.memberId
    ? '确定要退款该订单吗？退款金额将退回会员余额。'
    : '该订单无关联会员，退款仅记录状态，请线下退款。确定继续吗？'
  ElMessageBox.confirm(message, '订单退款', {
    confirmButtonText: '确认退款',
    cancelButtonText: '取消',
    type: 'warning'
  }).then(() => {
    doRefund(row.id)
  }).catch(() => {})
}

function doRefund(id) {
  orderApi.refund(id).then(() => {
    ElMessage.success('退款成功')
    fetchData()
  }).catch((err) => {
    ElMessage.error(err?.response?.data?.message || err?.message || '退款失败')
  })
}

onMounted(() => {
  fetchData()
})
</script>

<style lang="scss" scoped>
.order-list {
  .order-detail {
    .detail-section {
      margin-bottom: 24px;

      h4 {
        margin: 0 0 12px;
        padding-left: 8px;
        border-left: 3px solid #409eff;
      }
    }
  }

  .pay-dialog {
    .pay-amount {
      font-size: 18px;
      font-weight: 600;
      color: #f56c6c;
    }

    .pay-form {
      margin-top: 20px;
    }
  }
}
</style>
