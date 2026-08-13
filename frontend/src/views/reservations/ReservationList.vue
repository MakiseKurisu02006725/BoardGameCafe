<template>
  <div class="reservation-list">
    <div class="page-header">
      <el-button type="primary" :icon="Plus" @click="handleAdd">新增预约</el-button>
    </div>

    <SearchBar
      ref="searchBarRef"
      :initial-values="searchInitial"
      @search="handleSearch"
      @reset="handleReset"
    >
      <template #default="{ form }">
        <el-form-item label="状态">
          <el-select v-model="form.status" placeholder="请选择状态" clearable style="width: 160px">
            <el-option label="待确认" :value="1" />
            <el-option label="已确认" :value="2" />
            <el-option label="已完成" :value="3" />
            <el-option label="已取消" :value="4" />
            <el-option label="已过期" :value="5" />
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
        <el-form-item label="会员关键词">
          <el-input
            v-model="form.keyword"
            placeholder="会员昵称/手机号"
            clearable
            style="width: 200px"
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
      :actions-width="260"
      @pagination-change="handlePaginationChange"
    >
      <template #status="{ row }">
        <StatusTag :status="getStatusText(row.statusName || row.status)" />
      </template>
      <template #deposit="{ row }">
        <MoneyDisplay :value="row.deposit" />
      </template>
      <template #actions="{ row }">
        <el-button
          v-if="row.status === 'Pending'"
          type="success"
          link
          size="small"
          @click.stop="handleConfirm(row)"
        >
          确认
        </el-button>
        <el-button
          v-if="row.status === 'Pending' || row.status === 'Confirmed'"
          type="danger"
          link
          size="small"
          @click.stop="handleCancel(row)"
        >
          取消
        </el-button>
        <el-button
          v-if="row.status === 'Confirmed'"
          type="success"
          link
          size="small"
          @click.stop="handleComplete(row)"
        >
          完成
        </el-button>
      </template>
    </DataTable>

    <el-dialog
      v-model="dialogVisible"
      :title="dialogTitle"
      width="560px"
      @close="resetForm"
    >
      <el-form
        ref="formRef"
        :model="formData"
        :rules="formRules"
        label-width="100px"
      >
        <el-form-item label="桌位类型" prop="tableType">
          <el-radio-group v-model="formData.tableType" @change="handleTableTypeChange">
            <el-radio label="hall">大厅</el-radio>
            <el-radio label="room">包间</el-radio>
            <el-radio label="booth">卡座</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="桌号">
          <el-select
            v-model="formData.tableId"
            placeholder="请选择空闲桌位"
            clearable
            filterable
            :disabled="!formData.tableType"
            style="width: 100%"
          >
            <el-option
              v-for="t in filteredTableOptions"
              :key="t.id"
              :label="`${t.code} - ${t.areaName}`"
              :value="t.id"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="预约桌游">
          <el-select
            v-model="formData.gameIds"
            placeholder="请选择桌游（可多选）"
            multiple
            clearable
            filterable
            style="width: 100%"
          >
            <el-option
              v-for="g in availableGames"
              :key="g.id"
              :label="g.name"
              :value="g.id"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="日期" prop="date">
          <el-date-picker
            v-model="formData.date"
            type="date"
            placeholder="选择日期"
            value-format="YYYY-MM-DD"
            style="width: 100%"
          />
        </el-form-item>
        <el-form-item label="开始时间" prop="startTime">
          <el-time-picker
            v-model="formData.startTime"
            placeholder="选择开始时间"
            value-format="HH:mm"
            style="width: 100%"
          />
        </el-form-item>
        <el-form-item label="结束时间" prop="endTime">
          <el-time-picker
            v-model="formData.endTime"
            placeholder="选择结束时间"
            value-format="HH:mm"
            style="width: 100%"
          />
        </el-form-item>
        <el-form-item label="人数" prop="peopleCount">
          <el-input-number
            v-model="formData.peopleCount"
            :min="1"
            :max="20"
            style="width: 100%"
          />
        </el-form-item>
        <el-form-item label="联系电话" prop="phone">
          <el-input v-model="formData.phone" placeholder="请输入联系电话" maxlength="11" />
        </el-form-item>
        <el-form-item label="备注">
          <el-input
            v-model="formData.remark"
            type="textarea"
            :rows="3"
            placeholder="请输入备注"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="submitting" @click="handleSubmit">确认</el-button>
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
import StatusTag from '@/components/StatusTag/index.vue'
import MoneyDisplay from '@/components/MoneyDisplay/index.vue'
import * as reservationApi from '@/api/reservation'
import * as tableApi from '@/api/table'
import * as boardgameApi from '@/api/boardgame'

const searchBarRef = ref(null)
const formRef = ref(null)
const loading = ref(false)
const submitting = ref(false)
const dialogVisible = ref(false)
const tableData = ref([])

// 初始值使用独立常量对象：避免 SearchBar 重置时被已输入值污染
const searchInitial = {
  status: '',
  dateRange: [],
  keyword: ''
}

const searchForm = reactive({ ...searchInitial })

const pagination = reactive({
  total: 0,
  pageIndex: 1,
  pageSize: 10
})

const columns = [
  { prop: 'id', label: '预约ID', width: 180, formatter: (row) => row.id?.substring(0, 8) + '...' },
  { prop: 'memberName', label: '会员昵称', width: 120 },
  { prop: 'tableCode', label: '桌号', width: 80 },
  {
    prop: 'tableTypeName',
    label: '桌位类型',
    width: 100
  },
  { prop: 'reservedGames', label: '预约桌游', minWidth: 140, formatter: (row) => row.reservedGames || '-' },
  { prop: 'date', label: '日期', width: 120 },
  {
    prop: 'startTime',
    label: '时段',
    width: 140,
    formatter: (row) => {
      const start = row.startTime ? (row.startTime instanceof String ? row.startTime : row.startTime.split('T')[1]?.substring(0,5) || row.startTime) : ''
      const end = row.endTime ? (row.endTime instanceof String ? row.endTime : row.endTime.split('T')[1]?.substring(0,5) || row.endTime) : ''
      return `${start} - ${end}`
    }
  },
  { prop: 'guestCount', label: '人数', width: 70 },
  { prop: 'deposit', label: '押金', width: 100, slot: 'deposit' },
  { prop: 'statusName', label: '状态', width: 100, slot: 'status' },
  { prop: 'contactPhone', label: '联系电话', width: 130 }
]

const idleTableOptions = ref([])
const availableGames = ref([])

// 桌位类型 → 区域名称映射（与后端 TableArea 枚举一致）
const TABLE_AREA_MAP = { hall: '大厅', room: '包间', booth: '卡座' }

// 按所选桌位类型过滤空闲桌位：只显示对应区域（大厅/包间/卡座）
const filteredTableOptions = computed(() => {
  const area = TABLE_AREA_MAP[formData.tableType]
  if (!area) return []
  return idleTableOptions.value.filter(t => t.areaName === area)
})

// 切换桌位类型时清空已选桌号，避免残留其他区域的桌位
function handleTableTypeChange() {
  formData.tableId = null
}

async function loadOptions() {
  try {
    const tableRes = await tableApi.getList()
    const tables = tableRes.data || tableRes || []
    // 只显示空闲桌位
    idleTableOptions.value = (Array.isArray(tables) ? tables : []).filter(t => t.statusName === '空闲' || t.status === 'Idle')
  } catch {
    idleTableOptions.value = []
  }
  try {
    const gameRes = await boardgameApi.getList({ pageIndex: 1, pageSize: 100, onlyInStock: true })
    const items = gameRes.items || gameRes.data?.items || gameRes.data || []
    availableGames.value = (Array.isArray(items) ? items : []).filter(g => (g.currentStock || 0) > 0)
  } catch {
    availableGames.value = []
  }
}

const formData = reactive({
  tableType: 'hall',
  tableId: null,
  gameIds: [],
  date: '',
  startTime: '',
  endTime: '',
  peopleCount: 2,
  phone: '',
  remark: ''
})

const formRules = {
  tableType: [{ required: true, message: '请选择桌位类型', trigger: 'change' }],
  date: [{ required: true, message: '请选择日期', trigger: 'change' }],
  startTime: [{ required: true, message: '请选择开始时间', trigger: 'change' }],
  endTime: [{ required: true, message: '请选择结束时间', trigger: 'change' }],
  peopleCount: [{ required: true, message: '请输入人数', trigger: 'blur' }],
  phone: [
    { required: true, message: '请输入联系电话', trigger: 'blur' },
    { pattern: /^1[3-9]\d{9}$/, message: '请输入正确的手机号', trigger: 'blur' }
  ]
}

const dialogTitle = '新增预约'

function getStatusText(status) {
  const map = {
    1: '待确认', 2: '已确认', 3: '已完成', 4: '已取消', 5: '已过期',
    pending: '待确认', confirmed: '已确认', completed: '已完成', cancelled: '已取消', expired: '已过期'
  }
  return map[status] || status || ''
}

function fetchData() {
  loading.value = true
  const params = {
    pageIndex: pagination.pageIndex,
    pageSize: pagination.pageSize,
    status: searchForm.status || undefined,
    keyword: searchForm.keyword || undefined,
    dateFrom: searchForm.dateRange?.[0] || undefined,
    dateTo: searchForm.dateRange?.[1] || undefined
  }
  reservationApi.getList(params).then((res) => {
    if (Array.isArray(res)) {
      tableData.value = res
      pagination.total = res.length
    } else if (res && Array.isArray(res.items)) {
      tableData.value = res.items
      pagination.total = res.total || res.items.length
    } else {
      tableData.value = []
      pagination.total = 0
    }
  }).finally(() => {
    loading.value = false
  })
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

function handleAdd() {
  resetForm()
  dialogVisible.value = true
}

function resetForm() {
  Object.assign(formData, {
    tableType: 'hall',
    tableId: null,
    gameIds: [],
    date: '',
    startTime: '',
    endTime: '',
    peopleCount: 2,
    phone: '',
    remark: ''
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
  reservationApi.create({
    ...formData,
    tableId: formData.tableId || null,
    gameIds: formData.gameIds || []
  }).then(() => {
    ElMessage.success('预约成功')
    dialogVisible.value = false
    fetchData()
    loadOptions()
  }).catch((err) => {
    ElMessage.error(err?.message || '预约冲突或创建失败')
  }).finally(() => {
    submitting.value = false
  })
}

function handleConfirm(row) {
  ElMessageBox.confirm('确认该预约？', '提示', { type: 'warning' }).then(() => {
    reservationApi.confirm(row.id).then(() => {
      ElMessage.success('确认成功')
      fetchData()
      loadOptions()
    })
  }).catch(() => {})
}

function handleCancel(row) {
  ElMessageBox.confirm('确定取消该预约？', '提示', { type: 'warning' }).then(() => {
    reservationApi.cancel(row.id).then(() => {
      ElMessage.success('取消成功')
      fetchData()
      loadOptions()
    })
  }).catch(() => {})
}

function handleComplete(row) {
  ElMessageBox.confirm('确认完成该预约吗？', '提示', { type: 'warning' }).then(() => {
    reservationApi.complete(row.id).then(() => {
      ElMessage.success('预约已完成')
      fetchData()
      loadOptions()
    }).catch((err) => {
      ElMessage.error(err?.message || '操作失败，请重试')
    })
  }).catch(() => {})
}

onMounted(() => {
  fetchData()
  loadOptions()
})
</script>

<style lang="scss" scoped>
.reservation-list {
  .page-header {
    margin-bottom: 20px;
  }
}
</style>
