<template>
  <div class="table-board-page">
    <div class="board-header">
      <div class="board-stats">
        <div class="stat-card total">
          <div class="stat-icon"><el-icon :size="28"><Grid /></el-icon></div>
          <div class="stat-content">
            <div class="stat-value">{{ stats.total }}</div>
            <div class="stat-label">总台数</div>
          </div>
        </div>
        <div class="stat-card free">
          <div class="stat-icon"><el-icon :size="28"><CircleCheck /></el-icon></div>
          <div class="stat-content">
            <div class="stat-value">{{ stats.free }}</div>
            <div class="stat-label">空闲</div>
          </div>
        </div>
        <div class="stat-card occupied">
          <div class="stat-icon"><el-icon :size="28"><User /></el-icon></div>
          <div class="stat-content">
            <div class="stat-value">{{ stats.occupied }}</div>
            <div class="stat-label">占用</div>
          </div>
        </div>
        <div class="stat-card reserved">
          <div class="stat-icon"><el-icon :size="28"><Clock /></el-icon></div>
          <div class="stat-content">
            <div class="stat-value">{{ stats.reserved }}</div>
            <div class="stat-label">预约</div>
          </div>
        </div>
        <div class="stat-card maintenance">
          <div class="stat-icon"><el-icon :size="28"><Tools /></el-icon></div>
          <div class="stat-content">
            <div class="stat-value">{{ stats.maintenance }}</div>
            <div class="stat-label">维修</div>
          </div>
        </div>
        <div class="stat-card in-store">
          <div class="stat-icon"><el-icon :size="28"><UserFilled /></el-icon></div>
          <div class="stat-content">
            <div class="stat-value">{{ stats.inStorePeople }}</div>
            <div class="stat-label">在店人数</div>
          </div>
        </div>
      </div>
      <div class="board-refresh">
        <el-button :icon="Refresh" @click="fetchTables" :loading="loading">
          刷新
        </el-button>
        <span class="refresh-info">
          <el-icon :size="14"><Timer /></el-icon>
          自动刷新（15秒）：{{ lastRefreshTime || '-' }}
        </span>
      </div>
    </div>

    <div class="board-legend">
      <div class="legend-item">
        <span class="legend-dot free" />
        <span>空闲</span>
      </div>
      <div class="legend-item">
        <span class="legend-dot occupied" />
        <span>占用</span>
      </div>
      <div class="legend-item">
        <span class="legend-dot reserved" />
        <span>预约</span>
      </div>
      <div class="legend-item">
        <span class="legend-dot maintenance" />
        <span>维修</span>
      </div>
    </div>

    <div class="board-zones" v-loading="loading">
      <template v-for="zone in zones" :key="zone">
        <el-card class="zone-card" shadow="never">
          <template #header>
            <div class="zone-header">
              <span class="zone-title">{{ zone }}</span>
              <el-tag size="small" type="info" effect="plain">
                {{ getZoneTables(zone).length }} 桌
              </el-tag>
            </div>
          </template>
          <div class="zone-tables">
            <el-empty
              v-if="getZoneTables(zone).length === 0"
              :image-size="60"
              description="暂无桌位"
            />
            <el-row :gutter="16">
              <el-col
                v-for="table in getZoneTables(zone)"
                :key="table.id"
                :xs="24"
                :sm="12"
                :md="8"
                :lg="6"
              >
                <el-card
                  class="table-card"
                  :class="`is-${resolveStatusKey(table.statusName)}`"
                  shadow="hover"
                >
                  <div class="table-card-header">
                    <div class="table-no">{{ table.code }}</div>
                    <div class="table-seats">
                      <el-icon :size="14"><User /></el-icon>
                      {{ table.seats }}座
                    </div>
                  </div>

                  <div
                    v-if="table.statusName === '占用中'"
                    class="table-card-body occupied-info"
                  >
                    <div class="info-row">
                      <span class="info-label">会员：</span>
                      <span class="info-value">
                        {{ table.memberName || '散客' }}
                      </span>
                    </div>
                    <div class="info-row">
                      <span class="info-label">开台：</span>
                      <span class="info-value">{{ table.openTime || '-' }}</span>
                    </div>
                    <div class="info-row highlight">
                      <span class="info-label">时长：</span>
                      <span class="info-value duration">
                        {{ calcDuration(table.openTime) }}
                      </span>
                    </div>
                    <div class="info-row">
                      <span class="info-label">订单项：</span>
                      <span class="info-value">{{ table.orderItemCount || 0 }} 项</span>
                    </div>
                  </div>

                  <div
                    v-else-if="table.statusName === '已预约'"
                    class="table-card-body reserved-info"
                  >
                    <div class="info-row">
                      <span class="info-label">预约人：</span>
                      <span class="info-value">{{ table.reservedName || '-' }}</span>
                    </div>
                    <div class="info-row">
                      <span class="info-label">预约时间：</span>
                      <span class="info-value">{{ table.reservedTime || '-' }}</span>
                    </div>
                  </div>

                  <div
                    v-else-if="table.statusName === '维修'"
                    class="table-card-body maintenance-info"
                  >
                    <el-alert
                      title="维修中"
                      type="info"
                      :closable="false"
                      show-icon
                    />
                  </div>

                  <div v-else class="table-card-body free-info">
                    <div class="free-placeholder">
                      <el-icon :size="32" class="free-icon"><CircleCheck /></el-icon>
                      <span>空闲中</span>
                    </div>
                  </div>

                  <div class="table-card-footer">
                    <template v-if="table.statusName === '空闲'">
                      <div class="action-group">
                        <el-button
                          type="primary"
                          size="small"
                          @click="handleOpenTable(table)"
                        >
                          开台
                        </el-button>
                        <el-dropdown trigger="click" @command="(cmd) => handleMaintain(table, cmd)">
                          <el-button size="small" :icon="Tools">更多</el-button>
                          <template #dropdown>
                            <el-dropdown-menu>
                              <el-dropdown-item command="maintain">设置维修</el-dropdown-item>
                            </el-dropdown-menu>
                          </template>
                        </el-dropdown>
                      </div>
                    </template>
                    <template v-else-if="table.statusName === '维修'">
                      <el-button
                        type="info"
                        size="small"
                        style="width: 100%"
                        @click="handleMaintain(table, 'normal')"
                      >
                        解除维修
                      </el-button>
                    </template>
                    <template v-else-if="table.statusName === '占用中'">
                      <div class="action-group">
                        <el-button
                          type="success"
                          size="small"
                          @click="handleCheckout(table)"
                        >
                          结账
                        </el-button>
                        <el-button
                          type="warning"
                          size="small"
                          @click="handleChangeTable(table)"
                        >
                          换桌
                        </el-button>
                        <el-dropdown trigger="click" @command="(cmd) => handleMaintain(table, cmd)">
                          <el-button size="small" :icon="Tools" />
                          <template #dropdown>
                            <el-dropdown-menu>
                              <el-dropdown-item command="maintain">设置维修</el-dropdown-item>
                            </el-dropdown-menu>
                          </template>
                        </el-dropdown>
                      </div>
                    </template>
                    <template v-else-if="table.statusName === '已预约'">
                      <div class="action-group">
                        <el-button
                          v-if="table.reservationId"
                          type="primary"
                          size="small"
                          @click="handleConfirmReservation(table)"
                        >
                          确认入座
                        </el-button>
                        <el-tag v-else size="small" type="warning" effect="light">已预约</el-tag>
                        <el-dropdown trigger="click" @command="(cmd) => handleMaintain(table, cmd)">
                          <el-button size="small" :icon="Tools" />
                          <template #dropdown>
                            <el-dropdown-menu>
                              <el-dropdown-item command="maintain">设置维修</el-dropdown-item>
                            </el-dropdown-menu>
                          </template>
                        </el-dropdown>
                      </div>
                    </template>
                    <template v-else>
                      <el-tag size="small" type="info" effect="light">
                        {{ table.status }}
                      </el-tag>
                    </template>
                  </div>
                </el-card>
              </el-col>
            </el-row>
          </div>
        </el-card>
      </template>
    </div>

    <!-- 开台第一步：选择游玩桌游 -->
    <el-dialog
      v-model="gameDialogVisible"
      title="选择游玩桌游"
      width="640px"
      :close-on-click-modal="false"
    >
      <el-input
        v-model="gameSearch"
        placeholder="搜索桌游名称"
        clearable
        :prefix-icon="Search"
        style="margin-bottom: 12px"
      />
      <el-table
        ref="gameTableRef"
        :data="filteredGames"
        height="360"
        @selection-change="handleGameSelection"
      >
        <el-table-column type="selection" width="46" :selectable="isGameSelectable" />
        <el-table-column prop="name" label="桌游名称" min-width="150" />
        <el-table-column prop="gameNo" label="编号" width="90" />
        <el-table-column label="库存" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="row.currentStock > 0 ? 'success' : 'info'" size="small">
              {{ row.currentStock }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="每小时费率" width="100" align="center">
          <template #default="{ row }">
            ¥{{ row.pricePerHour }}
          </template>
        </el-table-column>
      </el-table>
      <div class="game-pick-tip">
        已选 {{ selectedGames.length }} 款，库存为 0 的桌游不可选择，至少选择一款才能进入下一步
      </div>
      <template #footer>
        <el-button @click="gameDialogVisible = false">取消</el-button>
        <el-button type="primary" :disabled="selectedGames.length === 0" @click="handleNextStep">
          下一步
        </el-button>
      </template>
    </el-dialog>

    <el-dialog
      v-model="openDialogVisible"
      title="开台"
      width="420px"
      :close-on-click-modal="false"
    >
      <el-form
        ref="openFormRef"
        :model="openForm"
        :rules="openFormRules"
        label-width="100px"
      >
        <el-form-item label="桌位">
          <span class="dialog-readonly">{{ openForm.tableNo }}</span>
        </el-form-item>
        <el-form-item label="游玩桌游">
          <span class="dialog-readonly">{{ openForm.boardGameNames || '-' }}</span>
        </el-form-item>
        <el-form-item label="会员ID" prop="memberId">
          <el-input
            v-model="openForm.memberId"
            placeholder="不填则为散客"
            clearable
          />
        </el-form-item>
        <el-form-item label="人数" prop="peopleCount">
          <el-input-number
            v-model="openForm.peopleCount"
            :min="1"
            :max="openForm.seatCount || 20"
            style="width: 100%"
          />
        </el-form-item>
        <el-form-item label="折扣(折)" prop="discount">
          <el-input-number
            v-model="openForm.discount"
            :min="0.1"
            :max="10"
            :step="0.1"
            :precision="1"
            style="width: 100%"
          />
          <div class="form-tip">折扣范围仅支持0.1~10.0折，10折为原价</div>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="openDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="openSubmitting" @click="handleSubmitOpen">
          确认开台
        </el-button>
      </template>
    </el-dialog>

    <el-dialog
      v-model="changeDialogVisible"
      title="换桌"
      width="420px"
      :close-on-click-modal="false"
    >
      <el-form
        ref="changeFormRef"
        :model="changeForm"
        :rules="changeFormRules"
        label-width="100px"
      >
        <el-form-item label="当前桌位">
          <span class="dialog-readonly">{{ changeForm.fromTableNo }}</span>
        </el-form-item>
        <el-form-item label="目标桌位" prop="toTableId">
          <el-select
            v-model="changeForm.toTableId"
            placeholder="请选择空闲桌位"
            filterable
            style="width: 100%"
          >
            <el-option
              v-for="t in freeTables"
              :key="t.id"
              :label="`${t.tableNo}（${t.zone} / ${t.seatCount}座）`"
              :value="t.id"
            />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="changeDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="changeSubmitting" @click="handleSubmitChange">
          确认换桌
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, onBeforeUnmount } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  Refresh,
  Grid,
  CircleCheck,
  User,
  Clock,
  UserFilled,
  Timer,
  Tools,
  Search
} from '@element-plus/icons-vue'
import * as tableApi from '@/api/table'
import * as reservationApi from '@/api/reservation'
import * as boardgameApi from '@/api/boardgame'

const router = useRouter()

const REFRESH_INTERVAL = 15000
const zones = ['大厅', '卡座', '包间']

const loading = ref(false)
const allTables = ref([])
const lastRefreshTime = ref('')
let refreshTimer = null

const stats = reactive({
  total: 0,
  free: 0,
  occupied: 0,
  reserved: 0,
  maintenance: 0,
  inStorePeople: 0
})

const freeTables = computed(() =>
  allTables.value.filter((t) => t.statusName === '空闲')
)

function resolveStatusKey(status) {
  const map = {
    '空闲': 'free',
    '占用中': 'occupied',
    '已预约': 'reserved',
    '维修': 'maintenance'
  }
  return map[status] || 'free'
}

function formatTime(date) {
  if (!date) return ''
  const d = typeof date === 'string' ? new Date(date) : date
  if (isNaN(d.getTime())) return String(date)
  const pad = (n) => n.toString().padStart(2, '0')
  return `${pad(d.getHours())}:${pad(d.getMinutes())}:${pad(d.getSeconds())}`
}

function calcDuration(openTime) {
  if (!openTime) return '-'
  const start = typeof openTime === 'string' ? new Date(openTime) : openTime
  if (isNaN(start.getTime())) return '-'
  const now = Date.now()
  const diffMs = now - start.getTime()
  if (diffMs < 0) return '-'
  const diffMin = Math.floor(diffMs / 60000)
  const hours = Math.floor(diffMin / 60)
  const minutes = diffMin % 60
  if (hours > 0) {
    return `${hours}小时${minutes}分`
  }
  return `${minutes}分钟`
}

function calcStats() {
  stats.total = allTables.value.length
  stats.free = allTables.value.filter((t) => t.statusName === '空闲').length
  stats.occupied = allTables.value.filter((t) => t.statusName === '占用中').length
  stats.reserved = allTables.value.filter((t) => t.statusName === '已预约').length
  stats.maintenance = allTables.value.filter((t) => t.statusName === '维修').length
  stats.inStorePeople = allTables.value
    .filter((t) => t.statusName === '占用中')
    .reduce((sum, t) => sum + (t.peopleCount || 0), 0)
}

function getZoneTables(zone) {
  return allTables.value.filter((t) => t.areaName === zone)
}

async function fetchTables() {
  loading.value = true
  try {
    const res = await tableApi.getList()
    console.log('fetchTables res:', res)
    // request.js 拦截器已解包，res直接是数组
    if (Array.isArray(res)) {
      allTables.value = res
    } else if (res && Array.isArray(res.items)) {
      allTables.value = res.items
    } else if (res && Array.isArray(res.data)) {
      allTables.value = res.data
    } else {
      allTables.value = []
    }
    calcStats()
    const now = new Date()
    lastRefreshTime.value = formatTime(now)
  } catch (error) {
    console.error('获取桌位数据失败:', error)
    allTables.value = []
  } finally {
    loading.value = false
  }
}

function startAutoRefresh() {
  stopAutoRefresh()
  refreshTimer = setInterval(() => {
    fetchTables()
  }, REFRESH_INTERVAL)
}

function stopAutoRefresh() {
  if (refreshTimer) {
    clearInterval(refreshTimer)
    refreshTimer = null
  }
}

const openDialogVisible = ref(false)
const openSubmitting = ref(false)
const openFormRef = ref(null)

const openForm = reactive({
  tableId: null,
  tableNo: '',
  seatCount: 0,
  memberId: '',
  peopleCount: 1,
  discount: 10,
  boardGameIds: [],
  boardGameNames: ''
})

const openFormRules = {
  peopleCount: [{ required: true, message: '请输入人数', trigger: 'blur' }]
}

// 开台第一步：桌游选择弹窗
const gameDialogVisible = ref(false)
const gameSearch = ref('')
const allGames = ref([])
const selectedGames = ref([])
const gameTableRef = ref(null)
let pendingTable = null

const filteredGames = computed(() => {
  const kw = gameSearch.value.trim().toLowerCase()
  if (!kw) return allGames.value
  return allGames.value.filter((g) => g.name.toLowerCase().includes(kw))
})

// 库存为 0 的桌游不可勾选
function isGameSelectable(row) {
  return (row.currentStock || 0) > 0
}

function handleGameSelection(rows) {
  selectedGames.value = rows
}

async function loadGames() {
  try {
    const res = await boardgameApi.getList({ pageIndex: 1, pageSize: 100 })
    allGames.value = res.items || res.data?.items || res.data || []
  } catch (error) {
    console.error('获取桌游列表失败:', error)
    allGames.value = []
  }
}

function handleOpenTable(table) {
  pendingTable = table
  selectedGames.value = []
  gameSearch.value = ''
  gameDialogVisible.value = true
  loadGames()
}

// 开台第二步：进入基础开台弹窗（携带已选桌游）
function handleNextStep() {
  if (selectedGames.value.length === 0) {
    ElMessage.warning('请至少选择一款游玩桌游')
    return
  }
  openForm.tableId = pendingTable.id
  openForm.tableNo = pendingTable.code
  openForm.seatCount = pendingTable.seats || 0
  openForm.memberId = ''
  openForm.peopleCount = 1
  openForm.discount = 10
  openForm.boardGameIds = selectedGames.value.map((g) => g.id)
  openForm.boardGameNames = selectedGames.value.map((g) => g.name).join('、')
  gameDialogVisible.value = false
  openDialogVisible.value = true
}

async function handleSubmitOpen() {
  if (!openFormRef.value) return
  try {
    await openFormRef.value.validate()
    openSubmitting.value = true
    const data = {
      tableId: openForm.tableId,
      guestCount: openForm.peopleCount,
      discount: openForm.discount,
      boardGameIds: openForm.boardGameIds
    }
    if (openForm.memberId) {
      data.memberId = openForm.memberId
    }
    await tableApi.openTable(data)
    ElMessage.success('开台成功')
    openDialogVisible.value = false
    fetchTables()
  } catch (error) {
    if (error !== false) {
      console.error('开台失败:', error)
    }
  } finally {
    openSubmitting.value = false
  }
}

async function handleCheckout(table) {
  try {
    await ElMessageBox.confirm(
      `确认对桌位 ${table.code} 进行结账？\n会员：${table.memberName || '散客'}\n已用时长：${calcDuration(table.openTime)}`,
      '结账确认',
      {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning',
        dangerouslyUseHTMLString: false
      }
    )
    if (!table.orderId) {
      ElMessage.warning('未找到该桌位订单，无法结账')
      return
    }
    // 合并支付与关台为单次 closeTable 调用：后端统一处理金额计算、优惠、积分、余额抵扣及桌位/预约桌游归还
    // 实时看板快速结账默认现金支付，完整收银请使用收银结账页面
    await tableApi.closeTable({
      orderId: table.orderId,
      paymentMethod: 'Cash'
    })
    ElMessage.success('结账成功')
    fetchTables()
  } catch (error) {
    if (error !== 'cancel' && error !== false) {
      console.error('结账失败:', error)
      ElMessage.error(error?.response?.data?.message || '结账失败')
    }
  }
}

// 设置/解除桌位维修状态（占用/预约桌位拦截并提示）
function handleMaintain(table, cmd) {
  const isMaintain = cmd === 'maintain'
  if (isMaintain && table.statusName !== '空闲') {
    ElMessage.warning('占用/预约状态桌位不能设置维修，请先结束订单/取消预约')
    return
  }
  ElMessageBox.confirm(
    isMaintain
      ? `确认将桌位 ${table.code} 设置为维修状态？维修期间将暂停使用。`
      : `确认解除桌位 ${table.code} 的维修状态？`,
    isMaintain ? '设置维修' : '解除维修',
    {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    }
  ).then(async () => {
    try {
      await tableApi.maintain(table.id, cmd)
      ElMessage.success(isMaintain ? '已设置维修' : '已解除维修')
      fetchTables()
    } catch (error) {
      ElMessage.error(error?.response?.data?.message || '操作失败，请稍后重试')
    }
  }).catch(() => {
    // 用户取消操作，无需处理
  })
}

const changeDialogVisible = ref(false)
const changeSubmitting = ref(false)
const changeFormRef = ref(null)
const changeForm = reactive({
  fromTableId: null,
  fromTableNo: '',
  toTableId: null
})

const changeFormRules = {
  toTableId: [{ required: true, message: '请选择目标桌位', trigger: 'change' }]
}

function handleChangeTable(table) {
  changeForm.fromTableId = table.id
  changeForm.fromTableNo = table.code
  changeForm.toTableId = null
  changeDialogVisible.value = true
}

async function handleSubmitChange() {
  if (!changeFormRef.value) return
  try {
    await changeFormRef.value.validate()
    const targetTable = freeTables.value.find((t) => t.id === changeForm.toTableId)
    await ElMessageBox.confirm(
      `确认将桌位 ${changeForm.fromTableNo} 换至 ${targetTable?.tableNo || ''}？`,
      '换桌确认',
      {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      }
    )
    changeSubmitting.value = true
    await tableApi.changeTable({
      fromTableId: changeForm.fromTableId,
      toTableId: changeForm.toTableId
    })
    ElMessage.success('换桌成功')
    changeDialogVisible.value = false
    fetchTables()
  } catch (error) {
    if (error !== 'cancel' && error !== false) {
      console.error('换桌失败:', error)
    }
  } finally {
    changeSubmitting.value = false
  }
}

async function handleConfirmReservation(table) {
  try {
    const resvId = table.reservationId
    if (!resvId) {
      ElMessage.warning('该桌位未关联预约，无法确认入座')
      return
    }
    await ElMessageBox.confirm(
      `确认 ${table.reservedName || '预约用户'} 在桌位 ${table.code} 入座？`,
      '预约确认',
      {
        confirmButtonText: '确定入座',
        cancelButtonText: '取消',
        type: 'warning'
      }
    )
    // 预约转订单：后端将预约置为已完成、桌位置为占用中、创建订单并关联预约（结账时自动归还桌游）
    await reservationApi.convertToOrder(resvId, table.id)
    ElMessage.success('入座成功')
    fetchTables()
  } catch (error) {
    if (error !== 'cancel' && error !== false) {
      console.error('确认入座失败:', error)
      ElMessage.error(error?.response?.data?.message || '确认入座失败')
    }
  }
}

onMounted(() => {
  fetchTables()
  startAutoRefresh()
})

onBeforeUnmount(() => {
  stopAutoRefresh()
})
</script>

<style lang="scss" scoped>
.table-board-page {
  padding: 20px;
}

.board-header {
  background-color: #fff;
  padding: 20px 24px;
  border-radius: 8px;
  margin-bottom: 20px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 16px;
}

.board-stats {
  display: flex;
  gap: 20px;
  flex-wrap: wrap;
}

.stat-card {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 12px 20px;
  border-radius: 8px;
  min-width: 140px;

  .stat-icon {
    width: 48px;
    height: 48px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
  }

  .stat-content {
    .stat-value {
      font-size: 24px;
      font-weight: 700;
      line-height: 1.2;
    }

    .stat-label {
      font-size: 13px;
      color: #909399;
      margin-top: 2px;
    }
  }

  &.total {
    background: linear-gradient(135deg, #ecf5ff 0%, #d9ecff 100%);

    .stat-icon {
      background-color: #409eff;
      color: #fff;
    }

    .stat-value {
      color: #409eff;
    }
  }

  &.free {
    background: linear-gradient(135deg, #f0f9eb 0%, #e1f3d8 100%);

    .stat-icon {
      background-color: #67c23a;
      color: #fff;
    }

    .stat-value {
      color: #67c23a;
    }
  }

  &.occupied {
    background: linear-gradient(135deg, #fef0f0 0%, #fde2e2 100%);

    .stat-icon {
      background-color: #f56c6c;
      color: #fff;
    }

    .stat-value {
      color: #f56c6c;
    }
  }

  &.reserved {
    background: linear-gradient(135deg, #fdf6ec 0%, #faecd8 100%);

    .stat-icon {
      background-color: #e6a23c;
      color: #fff;
    }

    .stat-value {
      color: #e6a23c;
    }
  }

  &.maintenance {
    background: linear-gradient(135deg, #f4f4f5 0%, #e9e9eb 100%);

    .stat-icon {
      background-color: #909399;
      color: #fff;
    }

    .stat-value {
      color: #909399;
    }
  }

  &.in-store {
    background: linear-gradient(135deg, #f4f4f5 0%, #e9e9eb 100%);

    .stat-icon {
      background-color: #909399;
      color: #fff;
    }

    .stat-value {
      color: #303133;
    }
  }
}

.board-refresh {
  display: flex;
  align-items: center;
  gap: 12px;

  .refresh-info {
    display: flex;
    align-items: center;
    gap: 4px;
    color: #909399;
    font-size: 13px;
  }
}

.board-legend {
  background-color: #fff;
  padding: 12px 24px;
  border-radius: 8px;
  margin-bottom: 20px;
  display: flex;
  gap: 24px;
  align-items: center;

  .legend-item {
    display: flex;
    align-items: center;
    gap: 6px;
    font-size: 14px;
    color: #606266;
  }

  .legend-dot {
    width: 12px;
    height: 12px;
    border-radius: 50%;
    display: inline-block;

    &.free {
      background-color: #67c23a;
    }

    &.occupied {
      background-color: #f56c6c;
    }

    &.reserved {
      background-color: #e6a23c;
    }

    &.maintenance {
      background-color: #909399;
    }
  }
}

.board-zones {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.zone-card {
  :deep(.el-card__header) {
    padding: 16px 20px;
    background-color: #fafafa;
    border-bottom: 1px solid #ebeef5;
  }

  .zone-header {
    display: flex;
    align-items: center;
    justify-content: space-between;

    .zone-title {
      font-size: 16px;
      font-weight: 600;
      color: #303133;
    }
  }
}

.zone-tables {
  padding: 8px 0;
}

.table-card {
  margin-bottom: 16px;
  border-radius: 8px;
  transition: all 0.25s ease;
  position: relative;
  overflow: hidden;

  &::before {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    height: 4px;
  }

  &.is-free::before {
    background-color: #67c23a;
  }

  &.is-occupied::before {
    background-color: #f56c6c;
  }

  &.is-reserved::before {
    background-color: #e6a23c;
  }

  &.is-maintenance::before {
    background-color: #909399;
  }

  :deep(.el-card__body) {
    padding: 14px 16px 16px;
    display: flex;
    flex-direction: column;
    min-height: 220px;
  }

  &.is-occupied {
    border-color: #fbc4c4;

    :deep(.el-card__body) {
      background-color: #fff8f8;
    }
  }

  &.is-reserved {
    border-color: #f5dab1;

    :deep(.el-card__body) {
      background-color: #fffbf5;
    }
  }

  &.is-maintenance {
    :deep(.el-card__body) {
      background-color: #fafafa;
    }
  }
}

.table-card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding-bottom: 10px;
  border-bottom: 1px dashed #ebeef5;
  margin-bottom: 12px;

  .table-no {
    font-size: 18px;
    font-weight: 700;
    color: #303133;
  }

  .table-seats {
    display: flex;
    align-items: center;
    gap: 4px;
    color: #909399;
    font-size: 13px;
  }
}

.table-card-body {
  flex: 1;

  .info-row {
    display: flex;
    align-items: flex-start;
    margin-bottom: 8px;
    font-size: 13px;

    &.highlight {
      background-color: rgba(245, 108, 108, 0.08);
      margin: 8px -8px;
      padding: 6px 8px;
      border-radius: 4px;
    }

    .info-label {
      color: #909399;
      min-width: 64px;
      flex-shrink: 0;
    }

    .info-value {
      color: #303133;
      font-weight: 500;
      word-break: break-all;

      &.duration {
        color: #f56c6c;
        font-weight: 600;
        font-size: 14px;
      }
    }
  }

  .free-placeholder {
    height: 100%;
    min-height: 120px;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 8px;
    color: #67c23a;
    font-size: 14px;

    .free-icon {
      color: #67c23a;
    }
  }

  .maintenance-info {
    padding: 12px 0;
  }
}

.table-card-footer {
  margin-top: 12px;

  .action-group {
    display: flex;
    gap: 8px;

    .el-button {
      flex: 1;
    }
  }
}

.dialog-readonly {
  color: #303133;
  font-weight: 500;
}
</style>
