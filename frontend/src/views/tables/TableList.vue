<template>
  <div class="table-list-page">
    <div class="page-toolbar">
      <el-button type="primary" :icon="Plus" @click="handleCreate">
        新增桌位
      </el-button>
      <el-button :icon="DataBoard" @click="goToBoard">
        实时看板
      </el-button>
    </div>

    <DataTable
      :columns="columns"
      :data="tableData"
      :loading="loading"
      :pagination="pagination"
      show-index
      @pagination-change="handlePaginationChange"
    >
      <template #area="{ row }">
        <el-tag :type="getAreaTagType(row.area)" effect="light" round>
          {{ row.areaName || getAreaLabel(row.area) }}
        </el-tag>
      </template>

      <template #status="{ row }">
        <StatusTag :status="row.statusName || row.status" />
      </template>

      <template #hourlyRate="{ row }">
        <MoneyDisplay :value="row.hourlyRate" />
      </template>

      <template #actions="{ row }">
        <el-button
          link
          type="primary"
          :disabled="row.status !== 1 && row.status !== '空闲'"
          @click="handleOpen(row)"
        >
          开台
        </el-button>
        <el-button
          link
          :type="isMaintenanceRow(row) ? 'success' : 'warning'"
          @click="handleMaintain(row)"
        >
          {{ isMaintenanceRow(row) ? '解除维修' : '设置维修' }}
        </el-button>
        <el-button link type="warning" @click="handleEdit(row)">编辑</el-button>
        <el-popconfirm
          title="确认删除该桌位？"
          confirm-button-text="确定"
          cancel-button-text="取消"
          @confirm="handleRemove(row)"
        >
          <template #reference>
            <el-button link type="danger">删除</el-button>
          </template>
        </el-popconfirm>
      </template>
    </DataTable>

    <el-dialog
      v-model="formDialogVisible"
      :title="formDialogTitle"
      width="480px"
      :close-on-click-modal="false"
    >
      <el-form
        ref="tableFormRef"
        :model="tableForm"
        :rules="tableFormRules"
        label-width="100px"
      >
        <el-form-item label="编号" prop="code">
          <el-input
            v-model="tableForm.code"
            placeholder="请输入桌位编号（如 A01、VIP8）"
            maxlength="10"
          />
        </el-form-item>
        <el-form-item label="区域" prop="area">
          <el-select v-model="tableForm.area" placeholder="请选择区域" style="width: 100%">
            <el-option label="大厅" :value="1" />
            <el-option label="包间" :value="2" />
            <el-option label="卡座" :value="3" />
          </el-select>
        </el-form-item>
        <el-form-item label="座位数" prop="seats">
          <el-input-number
            v-model="tableForm.seats"
            :min="1"
            :max="30"
            style="width: 100%"
          />
        </el-form-item>
        <el-form-item label="每小时费率" prop="hourlyRate">
          <el-input-number
            v-model="tableForm.hourlyRate"
            :min="0"
            :precision="2"
            :step="1"
            style="width: 100%"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="formDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="formSubmitting" @click="handleSubmitForm">
          确定
        </el-button>
      </template>
    </el-dialog>

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
          <span class="dialog-readonly">{{ openForm.tableCode }}</span>
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
        <el-form-item label="人数" prop="guestCount">
          <el-input-number
            v-model="openForm.guestCount"
            :min="1"
            :max="openForm.seats || 20"
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
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, DataBoard, Search } from '@element-plus/icons-vue'
import DataTable from '@/components/DataTable/index.vue'
import StatusTag from '@/components/StatusTag/index.vue'
import MoneyDisplay from '@/components/MoneyDisplay/index.vue'
import * as tableApi from '@/api/table'
import * as boardgameApi from '@/api/boardgame'

const router = useRouter()

const loading = ref(false)
const tableData = ref([])

const pagination = reactive({
  total: 0,
  pageIndex: 1,
  pageSize: 20
})

const columns = [
  { prop: 'code', label: '编号', width: 100 },
  { prop: 'areaName', label: '区域', width: 100, slot: 'area' },
  { prop: 'seats', label: '座位数', width: 100 },
  { prop: 'statusName', label: '状态', width: 100, slot: 'status' },
  { prop: 'hourlyRate', label: '每小时费率', width: 140, slot: 'hourlyRate' }
]

// 操作列由 DataTable 内置的 $slots.actions 渲染，不在 columns 数组中定义

const AREA_MAP = {
  1: '大厅', 2: '包间', 3: '卡座',
  Hall: '大厅', PrivateRoom: '包间', Booth: '卡座'
}
const AREA_TAG_TYPE = {
  1: 'primary',
  2: 'warning',
  3: 'success',
  Hall: 'primary',
  PrivateRoom: 'warning',
  Booth: 'success'
}

function getAreaLabel(area) {
  return AREA_MAP[area] || '未知'
}

function getAreaTagType(area) {
  return AREA_TAG_TYPE[area] || 'primary'
}

async function fetchList() {
  loading.value = true
  try {
    const params = {
      pageIndex: pagination.pageIndex,
      pageSize: pagination.pageSize
    }
    const res = await tableApi.getList(params)
    console.log('fetchList res:', res)
    // request.js 拦截器已解包 ApiResponse：code===200 时直接返回 data 字段
    // 后端 GetAll 返回 List<TableDto>，所以 res 直接是数组
    if (Array.isArray(res)) {
      tableData.value = res
      pagination.total = res.length
    } else if (res && Array.isArray(res.items)) {
      tableData.value = res.items
      pagination.total = res.total || res.items.length
    } else if (res && Array.isArray(res.data)) {
      tableData.value = res.data
      pagination.total = res.total || res.data.length
    } else {
      tableData.value = []
      pagination.total = 0
    }
  } catch (error) {
    console.error('获取桌位列表失败:', error)
    tableData.value = []
    pagination.total = 0
  } finally {
    loading.value = false
  }
}

function handlePaginationChange({ pageIndex, pageSize }) {
  pagination.pageIndex = pageIndex
  pagination.pageSize = pageSize
  fetchList()
}

function goToBoard() {
  router.push('/tables/board')
}

const formDialogVisible = ref(false)
const formDialogTitle = computed(() => (isEdit.value ? '编辑桌位' : '新增桌位'))
const isEdit = ref(false)
const formSubmitting = ref(false)
const tableFormRef = ref(null)
const currentTableId = ref(null)

// 字段名严格对齐后端 CreateTableDto / UpdateTableDto
const tableForm = reactive({
  code: '',
  area: 1,      // TableArea: 1=大厅, 2=包间, 3=卡座
  seats: 4,
  hourlyRate: 0
})

const tableFormRules = {
  code: [
    { required: true, message: '请输入桌位编号', trigger: 'blur' },
    { min: 1, max: 10, message: '编号长度为1-10位', trigger: 'blur' }
  ],
  area: [{ required: true, message: '请选择区域', trigger: 'change' }],
  seats: [{ required: true, message: '请输入座位数', trigger: 'blur' }],
  hourlyRate: [{ required: true, message: '请输入每小时费率', trigger: 'blur' }]
}

function handleCreate() {
  isEdit.value = false
  currentTableId.value = null
  Object.assign(tableForm, {
    code: '',
    area: 1,
    seats: 4,
    hourlyRate: 0
  })
  formDialogVisible.value = true
}

function handleEdit(row) {
  isEdit.value = true
  currentTableId.value = row.id
  Object.assign(tableForm, {
    code: row.code || '',
    area: row.area ?? 1,
    seats: row.seats ?? 4,
    hourlyRate: row.hourlyRate ?? 0
  })
  formDialogVisible.value = true
}

async function handleSubmitForm() {
  if (!tableFormRef.value) return
  try {
    await tableFormRef.value.validate()
    formSubmitting.value = true
    if (isEdit.value) {
      await tableApi.update(currentTableId.value, { ...tableForm })
      ElMessage.success('编辑成功')
    } else {
      await tableApi.create({ ...tableForm })
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

async function handleRemove(row) {
  try {
    await tableApi.remove(row.id)
    ElMessage.success('删除成功')
    fetchList()
  } catch (error) {
    console.error('删除失败:', error)
  }
}

function isMaintenanceRow(row) {
  return row.status === 4 || row.statusName === '维修'
}

// 设置/解除桌位维修状态（占用/预约桌位拦截并提示）
function handleMaintain(row) {
  const isMaintain = !isMaintenanceRow(row)
  if (isMaintain && row.statusName !== '空闲' && row.status !== 1) {
    ElMessage.warning('占用/预约状态桌位不能设置维修，请先结束订单/取消预约')
    return
  }
  ElMessageBox.confirm(
    isMaintain
      ? `确认将桌位 ${row.code} 设置为维修状态？维修期间将暂停使用。`
      : `确认解除桌位 ${row.code} 的维修状态？`,
    isMaintain ? '设置维修' : '解除维修',
    {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    }
  ).then(async () => {
    try {
      await tableApi.maintain(row.id, isMaintain ? 'maintain' : 'normal')
      ElMessage.success(isMaintain ? '已设置维修' : '已解除维修')
      fetchList()
    } catch (error) {
      ElMessage.error(error?.response?.data?.message || '操作失败，请稍后重试')
    }
  }).catch(() => {
    // 用户取消操作，无需处理
  })
}

const openDialogVisible = ref(false)
const openSubmitting = ref(false)
const openFormRef = ref(null)

const openForm = reactive({
  tableId: null,
  tableCode: '',
  seats: 0,
  memberId: '',
  guestCount: 1,
  discount: 10,
  boardGameIds: [],
  boardGameNames: ''
})

const openFormRules = {
  guestCount: [{ required: true, message: '请输入人数', trigger: 'blur' }]
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

function handleOpen(row) {
  pendingTable = row
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
  openForm.tableCode = pendingTable.code
  openForm.seats = pendingTable.seats || 0
  openForm.memberId = ''
  openForm.guestCount = 1
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
    await ElMessageBox.confirm(
      `确认开台？桌位：${openForm.tableCode}，游玩桌游：${openForm.boardGameNames}，人数：${openForm.guestCount}，折扣：${openForm.discount}折${openForm.memberId ? `，会员ID：${openForm.memberId}` : '（散客）'}`,
      '开台确认',
      {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      }
    )
    openSubmitting.value = true
    const data = {
      tableId: openForm.tableId,
      guestCount: openForm.guestCount,
      discount: openForm.discount,
      boardGameIds: openForm.boardGameIds
    }
    if (openForm.memberId) {
      data.memberId = openForm.memberId
    }
    await tableApi.openTable(data)
    ElMessage.success('开台成功')
    openDialogVisible.value = false
    fetchList()
  } catch (error) {
    if (error !== 'cancel' && error !== false) {
      console.error('开台失败:', error)
    }
  } finally {
    openSubmitting.value = false
  }
}

onMounted(() => {
  fetchList()
})
</script>

<style lang="scss" scoped>
.table-list-page {
  .page-toolbar {
    margin-bottom: 16px;
    display: flex;
    gap: 12px;
  }

  .dialog-readonly {
    color: #303133;
    font-weight: 500;
  }
}
</style>
