<template>
  <div class="member-profile-page">
    <el-card class="profile-header-card" shadow="never">
      <div class="profile-header">
        <el-avatar :src="resolveAvatar(memberInfo.avatar)" :size="80">
          {{ memberInfo.nickName?.charAt(0) }}
        </el-avatar>
        <div class="profile-info">
          <div class="profile-name-row">
            <span class="nickname">{{ memberInfo.nickName || '-' }}</span>
            <el-tag :type="getLevelTagType(memberInfo.level)" effect="light" round size="small">
              {{ getLevelLabel(memberInfo.level) }}
            </el-tag>
          </div>
          <div class="profile-stats">
            <div class="stat-item">
              <span class="stat-label">积分</span>
              <span class="stat-value">{{ memberInfo.points || 0 }}</span>
            </div>
            <div class="stat-divider" />
            <div class="stat-item">
              <span class="stat-label">余额</span>
              <span class="stat-value money">
                <MoneyDisplay :value="memberInfo.balance" />
              </span>
            </div>
          </div>
        </div>
      </div>
    </el-card>

    <el-card class="profile-content-card" shadow="never">
      <el-tabs v-model="activeTab" class="profile-tabs">
        <el-tab-pane label="基本信息" name="info">
          <div class="info-section">
            <div class="section-header">
              <h3>基本信息</h3>
              <el-button
                v-if="!isEditing"
                type="primary"
                :icon="Edit"
                size="small"
                @click="startEdit"
              >
                编辑
              </el-button>
              <template v-else>
                <el-button size="small" @click="cancelEdit">取消</el-button>
                <el-button
                  type="primary"
                  :icon="Check"
                  size="small"
                  :loading="savingProfile"
                  @click="saveProfile"
                >
                  保存
                </el-button>
              </template>
            </div>

            <el-form
              :model="profileForm"
              :rules="profileFormRules"
              ref="profileFormRef"
              label-width="100px"
              class="profile-form"
              :disabled="!isEditing"
            >
              <el-form-item label="头像">
                <FileUpload v-model="profileForm.avatar" />
              </el-form-item>
              <el-form-item label="昵称" prop="nickName">
                <el-input
                  v-model="profileForm.nickName"
                  placeholder="请输入昵称"
                  maxlength="20"
                  :disabled="!isEditing"
                />
              </el-form-item>
              <el-form-item label="生日" prop="birthday">
                <el-date-picker
                  v-model="profileForm.birthday"
                  type="date"
                  placeholder="选择生日"
                  value-format="YYYY-MM-DD"
                  style="width: 100%"
                  :disabled="!isEditing"
                />
              </el-form-item>
              <el-form-item label="邮箱" prop="email">
                <el-input
                  v-model="profileForm.email"
                  placeholder="请输入邮箱"
                  :disabled="!isEditing"
                />
              </el-form-item>
            </el-form>
          </div>
        </el-tab-pane>

        <el-tab-pane label="充值记录" name="recharges">
          <DataTable
            :columns="rechargeColumns"
            :data="rechargeList"
            :loading="rechargeLoading"
            :pagination="rechargePagination"
            show-index
            @pagination-change="handleRechargePaginationChange"
          >
            <template #amount="{ row }">
              <MoneyDisplay :value="row.amount" />
            </template>
            <template #bonus="{ row }">
              <MoneyDisplay :value="row.bonus" />
            </template>
          </DataTable>
        </el-tab-pane>

        <el-tab-pane label="消费记录" name="orders">
          <DataTable
            :columns="orderColumns"
            :data="orderList"
            :loading="orderLoading"
            :pagination="orderPagination"
            show-index
            @pagination-change="handleOrderPaginationChange"
          >
            <template #totalAmount="{ row }">
              <MoneyDisplay :value="row.totalAmount" />
            </template>
          </DataTable>
        </el-tab-pane>

        <el-tab-pane label="优惠券" name="coupons">
          <div class="coupon-list" v-loading="couponLoading">
            <el-empty v-if="couponList.length === 0" description="暂无优惠券" />
            <el-row :gutter="16">
              <el-col
                v-for="coupon in couponList"
                :key="coupon.id"
                :xs="24"
                :sm="12"
                :md="8"
              >
                <el-card class="coupon-card" :class="{ 'is-used': coupon.status === '已使用', 'is-expired': coupon.status === '已过期' }" shadow="hover">
                  <div class="coupon-header">
                    <div class="coupon-discount">
                      <template v-if="coupon.type === '满减'">
                        <span class="currency">¥</span>
                        <span class="value">{{ coupon.discountValue }}</span>
                      </template>
                      <template v-else-if="coupon.type === '折扣'">
                        <span class="value">{{ coupon.discountValue }}</span>
                        <span class="unit">折</span>
                      </template>
                    </div>
                    <div class="coupon-title">{{ coupon.couponName }}</div>
                  </div>
                  <div class="coupon-body">
                    <div class="coupon-min" v-if="coupon.minAmount > 0">
                      满 <MoneyDisplay :value="coupon.minAmount" :prefix="''" /> 可用
                    </div>
                    <div class="coupon-validity">
                      有效期: {{ coupon.validFrom }} ~ {{ coupon.validTo }}
                    </div>
                  </div>
                  <div class="coupon-footer">
                    <StatusTag :status="coupon.status" />
                  </div>
                </el-card>
              </el-col>
            </el-row>
          </div>
        </el-tab-pane>
      </el-tabs>
    </el-card>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import { ElMessage } from 'element-plus'
import { Edit, Check } from '@element-plus/icons-vue'
import DataTable from '@/components/DataTable/index.vue'
import StatusTag from '@/components/StatusTag/index.vue'
import MoneyDisplay from '@/components/MoneyDisplay/index.vue'
import FileUpload from '@/components/FileUpload/index.vue'
import * as memberApi from '@/api/member'
import { useUserStore } from '@/store/modules/user'

const route = useRoute()
const userStore = useUserStore()

const UPLOAD_PREFIX = import.meta.env.VITE_UPLOAD_PREFIX || ''

const activeTab = ref('info')

const memberInfo = ref({})
const memberInfoLoading = ref(false)

const resolvedMemberId = computed(() => {
  const routeId = route.params.id
  if (routeId && routeId !== 'profile') return routeId
  return userStore.memberId
})

function resolveAvatar(url) {
  if (!url) return ''
  if (url.startsWith('http')) return url
  return UPLOAD_PREFIX + url
}

function getLevelLabel(level) {
  const map = { Bronze: '青铜会员', Silver: '白银会员', Gold: '黄金会员', Diamond: '钻石会员', 1: '青铜会员', 2: '白银会员', 3: '黄金会员', 4: '钻石会员' }
  return map[level] || '青铜会员'
}

function getLevelTagType(level) {
  const map = {
    Bronze: 'info',
    Silver: 'primary',
    Gold: 'warning',
    Diamond: 'danger'
  }
  return map[level] || 'info'
}

async function fetchMemberInfo() {
  if (!resolvedMemberId.value) return
  memberInfoLoading.value = true
  try {
    const res = await memberApi.getById(resolvedMemberId.value)
    memberInfo.value = res.data || res
    syncProfileForm()
  } catch (error) {
    console.error('获取会员信息失败:', error)
  } finally {
    memberInfoLoading.value = false
  }
}

const isEditing = ref(false)
const savingProfile = ref(false)
const profileFormRef = ref(null)

const profileForm = reactive({
  avatar: '',
  nickName: '',
  birthday: '',
  email: ''
})

function syncProfileForm() {
  profileForm.avatar = memberInfo.value.avatar || ''
  profileForm.nickName = memberInfo.value.nickName || ''
  profileForm.birthday = memberInfo.value.birthday || ''
  profileForm.email = memberInfo.value.email || ''
}

function startEdit() {
  syncProfileForm()
  isEditing.value = true
}

function cancelEdit() {
  syncProfileForm()
  isEditing.value = false
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

const profileFormRules = {
  nickName: [
    { required: true, message: '请输入昵称', trigger: 'blur' },
    { min: 2, max: 20, message: '昵称长度为2-20位', trigger: 'blur' }
  ],
  email: [{ validator: isEmail, trigger: 'blur' }]
}

async function saveProfile() {
  if (!profileFormRef.value) return
  try {
    await profileFormRef.value.validate()
    savingProfile.value = true
    const data = { ...profileForm }
    if (!resolvedMemberId.value || userStore.role === 'Member') {
      await memberApi.updateProfile(data)
    } else {
      await memberApi.update(resolvedMemberId.value, data)
    }
    ElMessage.success('保存成功')
    memberInfo.value = { ...memberInfo.value, ...profileForm }
    if (userStore.role === 'Member') {
      userStore.nickName = profileForm.nickName
      userStore.avatar = profileForm.avatar
    }
    isEditing.value = false
  } catch (error) {
    if (error !== false) {
      console.error('保存失败:', error)
    }
  } finally {
    savingProfile.value = false
  }
}

const rechargeColumns = [
  { prop: 'amount', label: '充值金额', width: 140, slot: 'amount' },
  { prop: 'bonus', label: '赠送金额', width: 140, slot: 'bonus' },
  { prop: 'payMethod', label: '支付方式', width: 140, formatter: (row) => getPayMethodText(row.payMethod) },
  { prop: 'createdAt', label: '时间', width: 180 }
]

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

const rechargeList = ref([])
const rechargeLoading = ref(false)
const rechargePagination = reactive({
  total: 0,
  pageIndex: 1,
  pageSize: 10
})

async function fetchRecharges() {
  if (!resolvedMemberId.value) return
  rechargeLoading.value = true
  try {
    const params = {
      pageIndex: rechargePagination.pageIndex,
      pageSize: rechargePagination.pageSize
    }
    const res = await memberApi.getRecharges(resolvedMemberId.value, params)
    rechargeList.value = res.items || res.data?.items || res.data || []
    rechargePagination.total = res.total || res.data?.total || 0
  } catch (error) {
    console.error('获取充值记录失败:', error)
  } finally {
    rechargeLoading.value = false
  }
}

function handleRechargePaginationChange({ pageIndex, pageSize }) {
  rechargePagination.pageIndex = pageIndex
  rechargePagination.pageSize = pageSize
  fetchRecharges()
}

const orderColumns = [
  { prop: 'orderNo', label: '订单号', width: 200 },
  { prop: 'tableNo', label: '桌位', width: 120 },
  { prop: 'totalAmount', label: '金额', width: 140, slot: 'totalAmount' },
  { prop: 'paymentMethod', label: '支付方式', width: 140 },
  { prop: 'createdAt', label: '时间', width: 180 }
]

const orderList = ref([])
const orderLoading = ref(false)
const orderPagination = reactive({
  total: 0,
  pageIndex: 1,
  pageSize: 10
})

async function fetchOrders() {
  if (!resolvedMemberId.value) return
  orderLoading.value = true
  try {
    const params = {
      pageIndex: orderPagination.pageIndex,
      pageSize: orderPagination.pageSize
    }
    const res = await memberApi.getOrders(resolvedMemberId.value, params)
    orderList.value = res.items || res.data?.items || res.data || []
    orderPagination.total = res.total || res.data?.total || 0
  } catch (error) {
    console.error('获取消费记录失败:', error)
  } finally {
    orderLoading.value = false
  }
}

function handleOrderPaginationChange({ pageIndex, pageSize }) {
  orderPagination.pageIndex = pageIndex
  orderPagination.pageSize = pageSize
  fetchOrders()
}

const couponList = ref([])
const couponLoading = ref(false)

async function fetchCoupons() {
  if (!resolvedMemberId.value) return
  couponLoading.value = true
  try {
    const res = await memberApi.getCoupons(resolvedMemberId.value)
    // request.js 已自动解析 data 字段，res 就是 List<MemberCouponDto>
    couponList.value = Array.isArray(res) ? res : (res?.items || res?.data || [])
  } catch (error) {
    console.error('获取优惠券失败:', error)
    couponList.value = []
  } finally {
    couponLoading.value = false
  }
}

watch(activeTab, (val) => {
  if (val === 'recharges' && rechargeList.value.length === 0) {
    fetchRecharges()
  } else if (val === 'orders' && orderList.value.length === 0) {
    fetchOrders()
  } else if (val === 'coupons' && couponList.value.length === 0) {
    fetchCoupons()
  }
})

onMounted(() => {
  fetchMemberInfo()
})
</script>

<style lang="scss" scoped>
.member-profile-page {
  padding: 20px;

  .profile-header-card {
    margin-bottom: 20px;
  }

  .profile-content-card {
    min-height: 600px;
  }
}

.profile-header {
  display: flex;
  align-items: center;
  gap: 24px;

  .profile-info {
    flex: 1;
  }

  .profile-name-row {
    display: flex;
    align-items: center;
    gap: 12px;
    margin-bottom: 16px;

    .nickname {
      font-size: 24px;
      font-weight: 600;
      color: #303133;
    }
  }

  .profile-stats {
    display: flex;
    align-items: center;
    gap: 24px;

    .stat-item {
      display: flex;
      flex-direction: column;
      gap: 4px;

      .stat-label {
        font-size: 13px;
        color: #909399;
      }

      .stat-value {
        font-size: 20px;
        font-weight: 600;
        color: #303133;

        &.money {
          color: #e6a23c;
        }
      }
    }

    .stat-divider {
      width: 1px;
      height: 36px;
      background-color: #ebeef5;
    }
  }
}

.info-section {
  padding: 0 20px;

  .section-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: 24px;

    h3 {
      margin: 0;
      font-size: 18px;
      font-weight: 500;
    }
  }
}

.profile-form {
  max-width: 500px;
}

.profile-tabs {
  :deep(.el-tabs__header) {
    margin-bottom: 24px;
  }
}

.coupon-list {
  padding: 0 20px;
}

.coupon-card {
  margin-bottom: 16px;
  position: relative;
  overflow: hidden;

  &::before {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    width: 100px;
    height: 100%;
    background: linear-gradient(135deg, #f56c6c 0%, #e6a23c 100%);
    clip-path: polygon(0 0, 100% 0, 85% 100%, 0 100%);
  }

  &.is-used,
  &.is-expired {
    opacity: 0.6;

    &::before {
      background: linear-gradient(135deg, #909399 0%, #606266 100%);
    }
  }

  .coupon-header {
    position: relative;
    z-index: 1;
    display: flex;
    align-items: center;
    padding: 8px 16px 8px 16px;
    min-height: 80px;
    color: #fff;
    gap: 24px;

    .coupon-discount {
      margin-left: 4px;
      display: flex;
      align-items: baseline;

      .currency {
        font-size: 16px;
        margin-right: 2px;
      }

      .value {
        font-size: 36px;
        font-weight: 700;
        line-height: 1;
      }

      .unit {
        font-size: 16px;
        margin-left: 4px;
      }
    }

    .coupon-title {
      font-size: 15px;
      font-weight: 500;
    }
  }

  .coupon-body {
    padding: 12px 16px;
    border-top: 1px dashed #ebeef5;

    > div {
      font-size: 13px;
      color: #606266;
      margin-bottom: 6px;

      &:last-child {
        margin-bottom: 0;
      }
    }
  }

  .coupon-footer {
    padding: 8px 16px 12px;
    display: flex;
    justify-content: flex-end;
  }
}
</style>
