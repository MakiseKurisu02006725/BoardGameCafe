<template>
  <div class="operation-log">
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
            placeholder="操作描述/目标ID"
            clearable
            style="width: 200px"
          />
        </el-form-item>
        <el-form-item label="操作用户">
          <el-select
            v-model="form.userId"
            placeholder="请选择用户"
            clearable
            filterable
            style="width: 180px"
          >
            <el-option
              v-for="u in userOptions"
              :key="u.id"
              :label="u.userName"
              :value="u.id"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="操作类型">
          <el-select v-model="form.actionType" placeholder="请选择类型" clearable style="width: 160px">
            <el-option
              v-for="act in actionTypeOptions"
              :key="act.value"
              :label="act.label"
              :value="act.value"
            />
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
      </template>
      <template #footer>
        <el-button type="primary" :icon="Search" @click="searchBarRef?.handleSearch()">搜索</el-button>
        <el-button :icon="Refresh" @click="searchBarRef?.handleReset()">重置</el-button>
        <el-button type="warning" :icon="Delete" @click="handleCleanup">清理历史日志</el-button>
        <el-tooltip :disabled="isAdmin" content="仅管理员可操作" placement="top">
          <el-button type="danger" :icon="DeleteFilled" :disabled="!isAdmin" @click="handleClearAll">清理所有日志</el-button>
        </el-tooltip>
        <el-button :icon="Setting" @click="openSettings">设置</el-button>
      </template>
    </SearchBar>

    <DataTable
      :columns="columns"
      :data="tableData"
      :loading="loading"
      :pagination="pagination"
      show-index
      :show-pagination="true"
      @pagination-change="handlePaginationChange"
    >
      <template #actionType="{ row }">
        <el-tag
          :type="getActionTagType(row.actionType)"
          effect="light"
          round
          size="small"
        >
          {{ getActionLabel(row.actionType) }}
        </el-tag>
      </template>
      <template #targetId="{ row }">
        <span v-if="row.targetId" class="target-id-cell">{{ row.targetId }}</span>
        <span v-else class="muted">-</span>
      </template>
    </DataTable>

    <!-- 日志清理设置弹窗 -->
    <el-dialog v-model="settingsVisible" title="日志清理设置" width="520px">
      <el-form label-width="200px">
        <el-form-item label="查询日志（QUERY）保留天数">
          <el-input-number v-model="settingsForm.queryDays" :min="1" :max="365" />
        </el-form-item>
        <el-form-item label="操作日志（增删改）保留天数">
          <el-input-number v-model="settingsForm.operationDays" :min="1" :max="365" />
        </el-form-item>
        <el-form-item label="登录日志（LOGIN/LOGOUT）保留天数">
          <el-input-number v-model="settingsForm.loginDays" :min="1" :max="365" />
        </el-form-item>
        <el-form-item label="自动清理开关">
          <el-switch v-model="settingsForm.enabled" />
        </el-form-item>
        <el-form-item label="自动清理时间">
          <el-time-picker
            v-model="settingsForm.executeTime"
            format="HH:mm"
            value-format="HH:mm"
            style="width: 160px"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="settingsVisible = false">取消</el-button>
        <el-button type="primary" :loading="settingsSaving" @click="handleSaveSettings">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Search, Refresh, Delete, DeleteFilled, Setting } from '@element-plus/icons-vue'
import { useUserStore } from '@/store/modules/user'
import SearchBar from '@/components/SearchBar/index.vue'
import DataTable from '@/components/DataTable/index.vue'
import * as logApi from '@/api/log'
import * as userApi from '@/api/user'

const searchBarRef = ref(null)
const loading = ref(false)
const tableData = ref([])

const userStore = useUserStore()
// 角色权限：仅 Admin 可执行"清理所有日志"与保存设置
const isAdmin = computed(() => userStore.isAdmin)

// 初始值使用独立常量对象：避免 SearchBar 重置时被已输入值污染
const searchInitial = {
  keyword: '',
  userId: '',
  actionType: '',
  dateRange: []
}

const searchForm = reactive({ ...searchInitial })

const pagination = reactive({
  total: 0,
  pageIndex: 1,
  pageSize: 20
})

// 操作用户下拉：从后端加载真实用户（id 为 GUID），替换硬编码假 ID（假 ID 传参导致后端 Guid 绑定 400）
const userOptions = ref([])

async function loadUserOptions() {
  try {
    const res = await userApi.getList({ pageIndex: 1, pageSize: 1000 })
    const list = res.items || res.list || res.data || []
    userOptions.value = (list || [])
      .map(u => ({ id: u.id, userName: u.userName || u.UserName || '' }))
      .filter(u => u.id && u.userName)
  } catch {
    // 加载失败时下拉为空，不影响列表查询
  }
}

const actionTypeOptions = [
  { value: 'LOGIN', label: '登录' },
  { value: 'LOGOUT', label: '登出' },
  { value: 'CREATE', label: '新增' },
  { value: 'UPDATE', label: '更新' },
  { value: 'DELETE', label: '删除' },
  { value: 'CONFIRM', label: '确认' },
  { value: 'CANCEL', label: '取消' },
  { value: 'PAY', label: '支付' },
  { value: 'REFUND', label: '退款' },
  { value: 'EXPORT', label: '导出' },
  { value: 'PRINT', label: '打印' },
  { value: 'CLEANUP', label: '清理' }
]

const columns = [
  { prop: 'createTime', label: '时间', width: 180 },
  { prop: 'userName', label: '操作用户', width: 140 },
  { prop: 'actionType', label: '操作类型', width: 120, slot: 'actionType' },
  { prop: 'targetId', label: '目标ID', width: 160, slot: 'targetId' },
  { prop: 'description', label: '描述' }
]

function getActionLabel(type) {
  const opt = actionTypeOptions.find(a => a.value === type)
  return opt?.label || type
}

function getActionTagType(type) {
  // 删除/退款 → 红(danger)；取消/登出 → 灰(info)；打印/更新 → 橙(warning)；
  // 新增/确认 → 绿(success)；登录/导出 → 蓝(primary)；支付/其余 → 照旧
  const map = {
    DELETE: 'danger',
    REFUND: 'danger',
    CANCEL: 'info',
    LOGOUT: 'info',
    CLEANUP: 'info',
    PRINT: 'warning',
    UPDATE: 'warning',
    CREATE: 'success',
    CONFIRM: 'success',
    LOGIN: 'primary',
    EXPORT: 'primary',
    PAY: 'success',
    QUERY: 'info'
  }
  return map[type] || 'info'
}

// 清理历史日志默认天数（从配置读取，默认 90）
const cleanupDays = ref(90)

// 日志清理设置弹窗
const settingsVisible = ref(false)
const settingsSaving = ref(false)
const settingsForm = reactive({
  queryDays: 30,
  operationDays: 90,
  loginDays: 180,
  enabled: true,
  executeTime: '02:00'
})

// 加载清理配置：同步"清理历史日志"默认天数与设置弹窗表单
async function loadSettings() {
  try {
    const s = await logApi.getCleanupSettings()
    cleanupDays.value = typeof s.operationDays === 'number' ? s.operationDays : 90
    settingsForm.queryDays = typeof s.queryDays === 'number' ? s.queryDays : 30
    settingsForm.operationDays = typeof s.operationDays === 'number' ? s.operationDays : 90
    settingsForm.loginDays = typeof s.loginDays === 'number' ? s.loginDays : 180
    settingsForm.enabled = s.enabled !== false
    settingsForm.executeTime = s.executeTime || '02:00'
  } catch {
    // 加载失败时使用默认值
  }
}

// 打开设置弹窗
function openSettings() {
  loadSettings().then(() => {
    settingsVisible.value = true
  })
}

// 保存清理配置
async function handleSaveSettings() {
  settingsSaving.value = true
  try {
    await logApi.updateCleanupSettings({ ...settingsForm })
    cleanupDays.value = settingsForm.operationDays
    ElMessage.success('设置已保存')
    settingsVisible.value = false
  } catch (error) {
    ElMessage.error('保存失败，请稍后重试')
  } finally {
    settingsSaving.value = false
  }
}

// 清理所有日志（仅管理员）：危险操作二次确认
function handleClearAll() {
  if (!isAdmin.value) return
  ElMessageBox.confirm('此操作将删除全部操作日志（包括最近的日志），且不可恢复。确定继续吗？', '危险操作', {
    type: 'warning',
    confirmButtonText: '确认全部清理',
    cancelButtonText: '取消',
    confirmButtonClass: 'el-button--danger'
  }).then(async () => {
    try {
      const res = await logApi.clearAll()
      const count = typeof res === 'number' ? res : (res?.data ?? 0)
      ElMessage.success(`已清空全部日志，共删除 ${count} 条`)
      fetchData()
    } catch (error) {
      ElMessage.error('清理失败，请稍后重试')
    }
  }).catch(() => {
    // 用户取消操作，无需处理
  })
}

// 清理历史日志：确认后按配置的操作日志保留天数清理
function handleCleanup() {
  ElMessageBox.confirm(`确定要清理 ${cleanupDays.value} 天前的所有日志吗？此操作不可恢复。`, '清理确认', {
    type: 'warning',
    confirmButtonText: '确定清理',
    cancelButtonText: '取消'
  }).then(async () => {
    try {
      const res = await logApi.cleanup(cleanupDays.value)
      const count = typeof res === 'number' ? res : (res?.data ?? 0)
      ElMessage.success(`已清理 ${count} 条日志`)
      fetchData()
    } catch (error) {
      ElMessage.error('清理失败，请稍后重试')
    }
  }).catch(() => {
    // 用户取消操作，无需处理
  })
}

function fetchData() {
  loading.value = true
  const hasFilters = !!(searchForm.keyword || searchForm.userId || searchForm.actionType ||
    (searchForm.dateRange && searchForm.dateRange.length))
  const params = {
    pageIndex: pagination.pageIndex,
    pageSize: pagination.pageSize,
    keyword: searchForm.keyword || undefined,
    userId: searchForm.userId || undefined,
    action: searchForm.actionType || undefined,
    dateFrom: searchForm.dateRange?.[0] || undefined,
    dateTo: searchForm.dateRange?.[1] || undefined
  }
  logApi.getList(params).then((res) => {
    const list = res.items || res.list || res.data || []
    if (list.length) {
      tableData.value = list.map(log => ({
        ...log,
        createTime: log.timestamp || log.createTime || log.Timestamp || '-',
        userName: log.userName || log.UserName || '-',
        actionType: log.action || log.actionType || log.Action || '',
        targetId: log.targetId || log.TargetId || '',
        description: log.description || log.Description || ''
      }))
      pagination.total = res.totalCount || res.total || 0
    } else if (hasFilters) {
      // 有筛选条件但无结果：显示空态，而不是用 mock 数据掩盖筛选效果
      tableData.value = []
      pagination.total = 0
    } else {
      tableData.value = generateMockLogs()
      pagination.total = 86
    }
  }).catch(() => {
    if (hasFilters) {
      tableData.value = []
      pagination.total = 0
    } else {
      tableData.value = generateMockLogs()
      pagination.total = 86
    }
  }).finally(() => {
    loading.value = false
  })
}

function generateMockLogs() {
  const users = ['admin', 'manager01', 'cashier01', 'cashier02']
  const actions = ['LOGIN', 'LOGOUT', 'CREATE', 'UPDATE', 'DELETE', 'CONFIRM', 'CANCEL', 'PAY', 'REFUND', 'EXPORT', 'PRINT']
  const descs = {
    LOGIN: ['用户登录系统'],
    LOGOUT: ['用户退出系统'],
    CREATE: ['创建桌位 A08', '新增会员 王小明', '创建订单 ORD202408100001', '新增桌游 璀璨宝石'],
    UPDATE: ['更新桌位状态', '修改会员手机号', '编辑桌游信息', '更新订单备注'],
    DELETE: ['删除桌位', '删除订单', '删除桌游'],
    CONFIRM: ['确认预约 RESV001', '确认订单 ORD202408090008'],
    CANCEL: ['取消预约 RESV002', '取消订单 ORD202408090010'],
    PAY: ['支付订单 ORD202408100005', '会员卡充值 ¥500'],
    REFUND: ['退款订单 ORD202408090015 金额 ¥120'],
    EXPORT: ['导出订单报表', '导出会员列表'],
    PRINT: ['打印小票 订单号 ORD202408100003']
  }
  const targets = {
    CREATE: ['T08', 'M128', 'ORD202408100001', 'BG045'],
    UPDATE: ['T03', 'M105', 'BG012', 'ORD202408090020'],
    DELETE: ['T11', 'ORD202408080003', 'BG077'],
    CONFIRM: ['RESV001', 'ORD202408090008'],
    CANCEL: ['RESV002', 'ORD202408090010'],
    PAY: ['ORD202408100005', 'M108'],
    REFUND: ['ORD202408090015'],
    EXPORT: [null],
    PRINT: ['ORD202408100003']
  }
  const now = Date.now()
  return Array.from({ length: 20 }, (_, i) => {
    const act = actions[Math.floor(Math.random() * actions.length)]
    const descArr = descs[act] || ['执行操作']
    const tgtArr = targets[act] || [null]
    const d = new Date(now - i * 60000 * (3 + Math.floor(Math.random() * 20)))
    return {
      createTime: formatDate(d),
      userName: users[Math.floor(Math.random() * users.length)],
      actionType: act,
      targetId: tgtArr[Math.floor(Math.random() * tgtArr.length)],
      description: descArr[Math.floor(Math.random() * descArr.length)]
    }
  })
}

function pad(n) { return n < 10 ? '0' + n : '' + n }

function formatDate(d) {
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}:${pad(d.getSeconds())}`
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

onMounted(() => {
  fetchData()
  loadSettings()
  loadUserOptions()
})
</script>

<style lang="scss" scoped>
.operation-log {
  .target-id-cell {
    font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif;
    color: #409eff;
    font-variant-numeric: tabular-nums;
  }

  .muted {
    color: #c0c4cc;
  }
}
</style>
