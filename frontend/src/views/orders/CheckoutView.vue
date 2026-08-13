<template>
  <div class="checkout-view">
    <el-row :gutter="20">
      <el-col :span="14">
        <div id="print-area">
          <div class="order-card">
            <div class="card-header">
              <h3>订单信息</h3>
              <span class="order-no">订单号: {{ orderInfo.id ? orderInfo.id.substring(0, 8) + '...' : '-' }}</span>
            </div>
            <div class="card-body">
              <el-descriptions :column="2" border>
                <el-descriptions-item label="桌号">{{ orderInfo.tableCode || '-' }}</el-descriptions-item>
                <el-descriptions-item label="会员">{{ orderInfo.memberName || '散客' }}</el-descriptions-item>
                <el-descriptions-item label="开台时间">{{ orderInfo.startTime || '-' }}</el-descriptions-item>
                <el-descriptions-item label="时长">{{ duration }}分钟</el-descriptions-item>
              </el-descriptions>
            </div>
            <div class="card-section">
              <h4>计费项明细</h4>
              <el-table :data="billingItems" border style="width: 100%">
                <el-table-column prop="type" label="类型" width="120">
                  <template #default="{ row }">
                    {{ getItemTypeText(row.type) }}
                  </template>
                </el-table-column>
                <el-table-column prop="name" label="名称" />
                <el-table-column prop="desc" label="说明" width="220">
                  <template #default="{ row }">
                    <template v-if="row.type === 'time'">
                      {{ row.hourlyRate }}元/小时 x {{ row.hours }}小时
                    </template>
                    <template v-else-if="row.type === 'rental'">
                      {{ row.gameName }} x {{ row.hours }}小时 x {{ row.quantity }}件
                    </template>
                    <template v-else>
                      {{ row.quantity }}件
                    </template>
                  </template>
                </el-table-column>
                <el-table-column prop="subtotal" label="小计(元)" width="120" align="right">
                  <template #default="{ row }">
                    <MoneyDisplay :value="row.subtotal" />
                  </template>
                </el-table-column>
              </el-table>
            </div>
            <div class="card-footer">
              <div class="total-line">
                <span>合计金额</span>
                <span class="total-value">
                  <MoneyDisplay :value="totalAmount" :prefix="'￥'" />
                </span>
              </div>
            </div>
          </div>
        </div>
      </el-col>

      <el-col :span="10">
        <div class="checkout-card">
          <div class="card-header">
            <h3>结算操作</h3>
          </div>
          <div class="card-body">
            <div class="discount-section">
              <div class="section-item">
                <span class="label">会员等级折扣</span>
                <span class="value discount-value">
                  {{ memberDiscountText }}
                </span>
              </div>
              <el-form-item label="优惠券">
                <el-select
                  v-model="selectedCouponId"
                  placeholder="请选择优惠券"
                  clearable
                  style="width: 100%"
                  @change="handleCouponChange"
                >
                  <el-option
                    v-for="c in coupons"
                    :key="c.id"
                    :label="`${c.name} - 减￥${c.discountAmount}${c.minAmount ? ` (满￥${c.minAmount}可用)` : ''}`"
                    :value="c.id"
                    :disabled="c.minAmount && totalAmount.value < c.minAmount"
                  />
                </el-select>
              </el-form-item>
              <div class="section-item">
                <span class="label">积分抵扣</span>
                <div class="slider-wrap">
                  <el-slider
                    v-model="pointsToUse"
                    :min="0"
                    :max="maxPoints"
                    :step="10"
                    :disabled="!maxPoints"
                    @input="calculateFinal"
                  />
                  <span class="points-tip">
                    {{ pointsToUse }} 积分 抵扣 ￥{{ (pointsToUse * 0.01).toFixed(2) }}
                    <span v-if="maxPoints">(最大可用: {{ maxPoints }})</span>
                  </span>
                </div>
              </div>
              <div class="section-item">
                <span class="label">会员余额抵扣</span>
                <el-checkbox
                  v-model="useBalance"
                  :disabled="!(memberBalance > 0)"
                  @change="calculateFinal"
                >
                  使用余额 (￥{{ memberBalance?.toFixed(2) || '0.00' }})
                </el-checkbox>
              </div>
            </div>

            <el-divider />

            <div class="pay-section">
              <div class="section-item">
                <span class="label">支付方式</span>
                <el-radio-group v-model="payMethod">
                  <el-radio-button label="wechat">微信</el-radio-button>
                  <el-radio-button label="alipay">支付宝</el-radio-button>
                  <el-radio-button label="cash">现金</el-radio-button>
                  <el-radio-button label="balance">余额</el-radio-button>
                </el-radio-group>
              </div>
            </div>

            <div class="summary-section">
              <div class="summary-line">
                <span>原价</span>
                <MoneyDisplay :value="totalAmount" />
              </div>
              <div class="summary-line">
                <span>会员折扣</span>
                <span class="minus">- ￥{{ discountAmount.toFixed(2) }}</span>
              </div>
              <div class="summary-line">
                <span>优惠券</span>
                <span class="minus">- ￥{{ couponDiscount.toFixed(2) }}</span>
              </div>
              <div class="summary-line">
                <span>积分抵扣</span>
                <span class="minus">- ￥{{ pointsDiscount.toFixed(2) }}</span>
              </div>
              <div class="summary-line">
                <span>余额抵扣</span>
                <span class="minus">- ￥{{ balanceDiscount.toFixed(2) }}</span>
              </div>
              <el-divider />
              <div class="final-line">
                <span>实付金额</span>
                <span class="final-value">
                  <MoneyDisplay :value="finalAmount" :prefix="'￥'" />
                </span>
              </div>
            </div>

            <div class="action-buttons">
              <el-button
                type="primary"
                size="large"
                style="flex: 1"
                :loading="paying"
                @click="handlePay"
              >
                确认支付
              </el-button>
              <el-button
                size="large"
                :icon="Printer"
                @click="handlePrint"
              >
                打印小票
              </el-button>
            </div>
          </div>
        </div>
      </el-col>
    </el-row>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { Printer } from '@element-plus/icons-vue'
import dayjs from 'dayjs'
import MoneyDisplay from '@/components/MoneyDisplay/index.vue'
import * as orderApi from '@/api/order'
import * as tableApi from '@/api/table'
import * as memberApi from '@/api/member'

const route = useRoute()
const router = useRouter()
const paying = ref(false)
const orderLoaded = ref(false)

const orderInfo = reactive({
  id: '',
  tableCode: '',
  memberId: '',
  memberName: '',
  memberLevel: 'bronze',
  startTime: '',
  endTime: ''
})

const billingItems = ref([
  { type: 'time', name: '大厅计时费', hourlyRate: 20, hours: 2, subtotal: 40 },
  { type: 'rental', name: '桌游租赁费', gameName: '卡坦岛', hours: 2, quantity: 1, subtotal: 30 },
  { type: 'goods', name: '可口可乐', quantity: 2, subtotal: 12 }
])

const coupons = ref([
  { id: 'c1', name: '新人优惠券', discountAmount: 10, minAmount: 0 },
  { id: 'c2', name: '满100减20券', discountAmount: 20, minAmount: 100 }
])

const memberBalance = ref(500)
const maxPoints = ref(2000)
const selectedCouponId = ref('')
const pointsToUse = ref(0)
const useBalance = ref(false)
const payMethod = ref('wechat')

const duration = computed(() => {
  if (!orderInfo.startTime) return 0
  const start = dayjs(orderInfo.startTime)
  const end = dayjs(orderInfo.endTime || new Date())
  const minutes = end.diff(start, 'minute')
  return Math.max(1, minutes)
})

const totalAmount = computed(() => {
  return billingItems.value.reduce((sum, item) => sum + (item.subtotal || 0), 0)
})

const memberLevelMap = {
  bronze: { label: '青铜会员', discount: 1 },
  silver: { label: '白银会员', discount: 0.9 },
  gold: { label: '黄金会员', discount: 0.8 },
  diamond: { label: '钻石会员', discount: 0.7 }
}

const memberDiscountText = computed(() => {
  const level = memberLevelMap[orderInfo.memberLevel] || memberLevelMap.bronze
  return level.discount === 1 ? '无折扣' : `${(level.discount * 10).toFixed(0)}折`
})

const memberDiscount = computed(() => {
  const level = memberLevelMap[orderInfo.memberLevel] || memberLevelMap.bronze
  return level.discount
})

const discountAmount = computed(() => {
  return totalAmount.value * (1 - memberDiscount.value)
})

const couponDiscount = computed(() => {
  if (!selectedCouponId.value) return 0
  const c = coupons.value.find(x => x.id === selectedCouponId.value)
  if (!c) return 0
  // 检查订单金额是否满足优惠券使用条件
  const currentTotal = totalAmount.value
  if (c.minAmount && currentTotal < c.minAmount) {
    return 0
  }
  return c.discountAmount || 0
})

const pointsDiscount = computed(() => {
  return pointsToUse.value * 0.01
})

const balanceDiscount = computed(() => {
  if (!useBalance.value) return 0
  const beforeBalance = totalAmount.value - discountAmount.value - couponDiscount.value - pointsDiscount.value
  return Math.min(memberBalance.value, Math.max(0, beforeBalance))
})

const finalAmount = computed(() => {
  const v = totalAmount.value - discountAmount.value - couponDiscount.value - pointsDiscount.value - balanceDiscount.value
  return Math.max(0, v)
})

function getItemTypeText(type) {
  const map = { time: '计时费', rental: '桌游租赁', goods: '商品', service: '服务费' }
  return map[type] || type
}

function calculateFinal() {
}

function handleCouponChange(value) {
  if (!value) {
    calculateFinal()
    return
  }
  const c = coupons.value.find(x => x.id === value)
  if (c && c.minAmount && totalAmount.value < c.minAmount) {
    ElMessage.warning(`订单金额需满￥${c.minAmount}才能使用该优惠券`)
    selectedCouponId.value = ''
  }
  calculateFinal()
}

const GUID_PATTERN = /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/

async function fetchOrder() {
  const id = route.params.id
  if (!id || !GUID_PATTERN.test(id)) {
    // 无有效订单ID：自动跳回订单列表，而非停留在错误页
    ElMessage.warning('无效的订单ID，已返回订单列表')
    router.replace('/orders')
    return
  }
  try {
    const res = await orderApi.getById(id)
    const data = res.data || res
    Object.assign(orderInfo, {
      id: data.id || id,
      tableCode: data.tableCode || '-',
      memberId: data.memberId || '',
      memberName: data.memberName || '散客',
      memberLevel: data.memberLevel || 'gold',
      startTime: data.startTime ? dayjs(data.startTime).format('YYYY-MM-DD HH:mm:ss') : dayjs().subtract(2, 'hour').format('YYYY-MM-DD HH:mm:ss'),
      endTime: data.endTime ? dayjs(data.endTime).format('YYYY-MM-DD HH:mm:ss') : ''
    })

    // 映射后端订单项到前端计费项格式
    if (data.orderItems?.length) {
      billingItems.value = data.orderItems.map(item => ({
        type: mapOrderItemType(item.type),
        name: item.name || '',
        desc: formatItemDesc(item),
        quantity: item.quantity || 0,
        unitPrice: item.unitPrice || 0,
        subtotal: item.subtotal || 0,
        hourlyRate: item.unitPrice || 0,
        hours: 1
      }))
    }

    // 获取会员信息（等级、余额、积分）
    if (data.memberId) {
      try {
        const memberRes = await memberApi.getById(data.memberId)
        const memberData = memberRes.data || memberRes
        memberBalance.value = memberData.balance || 0
        maxPoints.value = memberData.points || 0
        orderInfo.memberLevel = mapMemberLevel(memberData.level)
      } catch {
        memberBalance.value = 0
        maxPoints.value = 0
      }
    }

    // 如果后端有总金额数据，更新计费项
    if (data.totalAmount && data.totalAmount > 0) {
      // 后端已经计算好总金额，使用它
    }

    orderLoaded.value = true
  } catch (err) {
    console.error('加载订单数据失败:', err)
    Object.assign(orderInfo, {
      id: id,
      tableCode: '-',
      memberName: '散客',
      memberLevel: 'gold',
      startTime: dayjs().subtract(2, 'hour').format('YYYY-MM-DD HH:mm:ss')
    })
    orderLoaded.value = false
    ElMessage.warning('订单数据加载不完整，请从订单列表进入')
  }
}

function mapOrderItemType(type) {
  const map = { 1: 'time', 2: 'rental', 3: 'goods', 4: 'service' }
  return map[type] || 'goods'
}

function mapMemberLevel(level) {
  const map = { 1: 'bronze', 2: 'silver', 3: 'gold', 4: 'diamond', Bronze: 'bronze', Silver: 'silver', Gold: 'gold', Diamond: 'diamond' }
  return map[level] || 'bronze'
}

function formatItemDesc(item) {
  const type = mapOrderItemType(item.type)
  const qty = item.quantity || 0
  const price = item.unitPrice || 0
  switch (type) {
    case 'time': return `${price.toFixed(2)}元/小时`
    case 'rental': return `${item.name || ''} × ${qty}件`
    case 'goods': return `单价${price.toFixed(2)}元 × ${qty}件`
    default: return `${qty}件`
  }
}

async function handlePay() {
    if (!route.params.id) {
      ElMessage.warning('缺少订单ID参数')
      return
    }
    if (!orderLoaded.value) {
      ElMessage.warning('订单数据未正确加载，请返回重新开台')
      return
    }
    paying.value = true
    try {
      const payload = {
        orderId: route.params.id,
        paymentMethod: getPaymentMethodValue(payMethod.value),
        couponId: selectedCouponId.value || null,
        pointsDeduct: pointsToUse.value || 0,
        balanceDeduct: useBalance.value ? memberBalance.value : 0
      }
      await tableApi.closeTable(payload)
      ElMessage.success('支付成功')
      setTimeout(() => {
        router.push('/orders')
      }, 800)
  } catch (err) {
    console.error('支付失败:', err)
    ElMessage.error(err?.message || '支付失败，请重试')
  } finally {
    paying.value = false
  }
}

function getPaymentMethodValue(method) {
  // 后端启用 JsonStringEnumConverter，枚举必须传字符串
  const map = {
    'wechat': 'WeChat',
    'alipay': 'Alipay',
    'cash': 'Cash',
    'balance': 'MemberBalance'
  }
  return map[method] || 'Cash'
}

function handlePrint() {
  window.print()
}

watch([selectedCouponId, pointsToUse, useBalance], () => {
  calculateFinal()
})

onMounted(() => {
  fetchOrder()
})
</script>

<style lang="scss" scoped>
.checkout-view {
  .order-card,
  .checkout-card {
    background: #fff;
    border-radius: 8px;
    overflow: hidden;
    box-shadow: 0 2px 12px rgba(0, 0, 0, 0.06);
  }

  .card-header {
    padding: 16px 20px;
    border-bottom: 1px solid #ebeef5;
    display: flex;
    justify-content: space-between;
    align-items: center;

    h3 {
      margin: 0;
      font-size: 16px;
      font-weight: 600;
    }

    .order-no {
      color: #909399;
      font-size: 13px;
    }
  }

  .card-body {
    padding: 20px;
  }

  .card-section {
    padding: 0 20px 20px;

    h4 {
      margin: 0 0 12px;
      padding-left: 8px;
      border-left: 3px solid #409eff;
      font-size: 14px;
    }
  }

  .card-footer {
    padding: 16px 20px;
    border-top: 1px solid #ebeef5;
    background: #fafafa;

    .total-line {
      display: flex;
      justify-content: space-between;
      align-items: center;
      font-size: 16px;

      .total-value {
        font-size: 22px;
        font-weight: 700;
        color: #f56c6c;
      }
    }
  }

  .discount-section {
    .section-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;

      .label {
        font-weight: 500;
        color: #303133;
      }

      .value {
        color: #606266;
      }

      .discount-value {
        color: #e6a23c;
        font-weight: 600;
      }
    }

    .slider-wrap {
      flex: 1;
      max-width: 70%;
      display: flex;
      flex-direction: column;
      gap: 4px;

      .points-tip {
        font-size: 12px;
        color: #909399;
      }
    }
  }

  .pay-section {
    .section-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;

      .label {
        font-weight: 500;
        color: #303133;
      }
    }
  }

  .summary-section {
    padding: 16px;
    background: #f5f7fa;
    border-radius: 6px;
    margin-bottom: 20px;

    .summary-line {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 4px 0;
      font-size: 13px;
      color: #606266;

      .minus {
        color: #67c23a;
      }
    }

    .final-line {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding-top: 8px;

      span:first-child {
        font-size: 15px;
        font-weight: 600;
      }

      .final-value {
        font-size: 28px;
        font-weight: 700;
        color: #f56c6c;
      }
    }
  }

  .action-buttons {
    display: flex;
    gap: 12px;
  }
}

@media print {
  .checkout-view :deep(.el-col:last-child),
  .checkout-view :deep(.el-header),
  .checkout-view :deep(.el-aside),
  .checkout-view .action-buttons {
    display: none !important;
  }

  #print-area {
    width: 100%;
  }
}
</style>
