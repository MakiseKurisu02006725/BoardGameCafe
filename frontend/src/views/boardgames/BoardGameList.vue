<template>
  <div class="boardgame-list-page">
    <SearchBar :initial-values="searchInitial" @search="handleSearch" @reset="handleReset">
      <template #default="{ form }">
      <el-form-item label="关键词">
        <el-input
          v-model="form.keyword"
          placeholder="名称/编号"
          clearable
          style="width: 200px"
        />
      </el-form-item>
      <el-form-item label="分类">
        <el-select
          v-model="form.category"
          placeholder="全部分类"
          clearable
          style="width: 160px"
        >
          <el-option
            v-for="opt in categoryOptions"
            :key="opt.value"
            :label="opt.label"
            :value="opt.value"
          />
        </el-select>
      </el-form-item>
      <el-form-item label="玩家数">
        <el-input-number
          v-model="form.minPlayers"
          :min="1"
          :max="20"
          placeholder="最小"
          controls-position="right"
          style="width: 120px"
        />
        <span style="margin: 0 8px; color: #909399">至</span>
        <el-input-number
          v-model="form.maxPlayers"
          :min="1"
          :max="20"
          placeholder="最大"
          controls-position="right"
          style="width: 120px"
        />
      </el-form-item>
      <el-form-item label="库存">
        <el-checkbox v-model="form.onlyInStock">仅显示有货</el-checkbox>
      </el-form-item>
      </template>
    </SearchBar>

    <div class="page-toolbar">
      <el-button type="primary" :icon="Plus" @click="handleCreate">
        新增桌游
      </el-button>
      <el-button type="warning" plain :icon="BorrowIcon" @click="handleViewBorrowed">
        借出中
      </el-button>
    </div>

    <DataTable
      :columns="columns"
      :data="tableData"
      :loading="loading"
      :pagination="pagination"
      show-index
      actions-width="340"
      @pagination-change="handlePaginationChange"
    >
      <template #cover="{ row }">
        <el-image
          :src="resolveCover(row.cover)"
          :preview-src-list="[resolveCover(row.cover)]"
          fit="cover"
          class="cover-image"
        >
          <template #error>
            <div class="cover-placeholder">
              <el-icon :size="28"><Picture /></el-icon>
            </div>
          </template>
        </el-image>
      </template>

      <template #category="{ row }">
        <el-tag :type="getCategoryTagType(row.category)" effect="light" round>
          {{ getCategoryLabel(row.category) }}
        </el-tag>
      </template>

      <template #players="{ row }">
        {{ row.minPlayers }}-{{ row.maxPlayers }}
      </template>

      <template #stock="{ row }">
        <span :class="{ 'stock-warn': row.currentStock === 0 }">
          {{ row.currentStock || 0 }} / {{ row.totalStock || 0 }}
        </span>
      </template>

      <template #damaged="{ row }">
        <span v-if="row.damaged > 0" class="damaged-count">
          {{ row.damaged }}
        </span>
        <span v-else>-</span>
      </template>

      <template #pricePerHour="{ row }">
        <MoneyDisplay :value="row.pricePerHour" />
      </template>

      <template #actions="{ row }">
        <el-button link type="warning" @click="handleEdit(row)">编辑</el-button>
        <el-button
          link
          type="primary"
          :disabled="row.currentStock === 0"
          @click="handleBorrow(row)"
        >
          借出
        </el-button>
        <el-button
          link
          type="success"
          :disabled="(row.currentStock || 0) >= (row.totalStock || 0)"
          @click="handleReturn(row)"
        >
          归还
        </el-button>
        <el-button link type="danger" @click="handleDamage(row)">损坏</el-button>
        <el-popconfirm
          title="确认下架该桌游？"
          confirm-button-text="确定"
          cancel-button-text="取消"
          @confirm="handleRemove(row)"
        >
          <template #reference>
            <el-button link type="info">下架</el-button>
          </template>
        </el-popconfirm>
      </template>
    </DataTable>

    <el-dialog
      v-model="formDialogVisible"
      :title="formDialogTitle"
      width="600px"
      :close-on-click-modal="false"
    >
      <el-form
        ref="boardgameFormRef"
        :model="boardgameForm"
        :rules="boardgameFormRules"
        label-width="100px"
      >
        <el-form-item label="名称" prop="name">
          <el-input v-model="boardgameForm.name" placeholder="请输入桌游名称" maxlength="50" />
        </el-form-item>
        <el-form-item label="分类" prop="category">
          <el-select
            v-model="boardgameForm.category"
            placeholder="请选择分类"
            style="width: 100%"
          >
            <el-option
              v-for="opt in categoryOptions"
              :key="opt.value"
              :label="opt.label"
              :value="opt.value"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="最小玩家" prop="minPlayers">
          <el-input-number
            v-model="boardgameForm.minPlayers"
            :min="1"
            :max="20"
            style="width: 100%"
          />
        </el-form-item>
        <el-form-item label="最大玩家" prop="maxPlayers">
          <el-input-number
            v-model="boardgameForm.maxPlayers"
            :min="1"
            :max="20"
            style="width: 100%"
          />
        </el-form-item>
        <el-form-item label="描述" prop="description">
          <el-input
            v-model="boardgameForm.description"
            type="textarea"
            :rows="4"
            placeholder="请输入桌游描述"
            maxlength="500"
            show-word-limit
          />
        </el-form-item>
        <el-form-item label="封面图" prop="cover">
          <FileUpload v-model="boardgameForm.cover" />
        </el-form-item>
        <el-form-item label="总库存" prop="totalStock">
          <el-input-number
            v-model="boardgameForm.totalStock"
            :min="0"
            style="width: 100%"
          />
        </el-form-item>
        <el-form-item label="每小时价格" prop="pricePerHour">
          <el-input-number
            v-model="boardgameForm.pricePerHour"
            :min="0"
            :precision="2"
            :step="1"
            style="width: 100%"
          />
        </el-form-item>
      </el-form>
      <template v-if="isEdit">
        <el-divider content-position="left">补货</el-divider>
        <div class="restock-section">
          <el-form label-width="100px">
            <el-form-item label="补货数量">
              <el-input-number
                v-model="restockForm.quantity"
                :min="1"
                :max="100"
                style="width: 100%"
              />
            </el-form-item>
            <el-form-item label="补货备注">
              <el-input
                v-model="restockForm.remark"
                placeholder="补货备注（可选）"
                maxlength="200"
              />
            </el-form-item>
            <el-form-item>
              <el-button type="success" :loading="restocking" @click="handleRestock">
                确认补货
              </el-button>
            </el-form-item>
          </el-form>
        </div>
      </template>
      <template #footer>
        <el-button @click="formDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="formSubmitting" @click="handleSubmitForm">
          确定
        </el-button>
      </template>
    </el-dialog>

    <el-drawer
      v-model="borrowedDrawerVisible"
      title="借出中的桌游"
      direction="rtl"
      size="560px"
    >
      <el-alert
        title="开台/预约借出且尚未归还的桌游，结账或退款后自动归还"
        type="info"
        :closable="false"
        show-icon
        style="margin-bottom: 16px"
      />
      <el-table :data="borrowedList" v-loading="borrowedLoading" border size="small">
        <el-table-column prop="name" label="桌游名称" min-width="120" />
        <el-table-column prop="gameNo" label="编号" width="90" />
        <el-table-column prop="quantity" label="借出数" width="70" align="center" />
        <el-table-column prop="orderNo" label="关联订单号" width="140">
          <template #default="{ row }">
            {{ row.orderNo || '-' }}
          </template>
        </el-table-column>
        <el-table-column prop="tableCode" label="桌位" width="70" align="center">
          <template #default="{ row }">
            {{ row.tableCode || '-' }}
          </template>
        </el-table-column>
      </el-table>
      <el-empty
        v-if="!borrowedLoading && borrowedList.length === 0"
        description="暂无借出中的桌游"
      />
    </el-drawer>

    <el-dialog
      v-model="borrowDialogVisible"
      title="借出桌游"
      width="420px"
      :close-on-click-modal="false"
    >
      <el-form label-width="100px">
        <el-form-item label="桌游">
          <span class="dialog-readonly">{{ borrowForm.gameName }}</span>
        </el-form-item>
        <el-form-item label="副本编号">
          <span class="dialog-readonly text-muted">系统自动分配（借出后展示）</span>
        </el-form-item>
        <el-form-item label="关联订单号">
          <el-input v-model="borrowForm.orderNo" placeholder="请输入关联订单号（可选）" clearable />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="borrowDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="borrowSubmitting" @click="handleSubmitBorrow">
          确认借出
        </el-button>
      </template>
    </el-dialog>

    <el-dialog
      v-model="returnDialogVisible"
      title="归还桌游"
      width="420px"
      :close-on-click-modal="false"
    >
      <el-form label-width="100px">
        <el-form-item label="桌游">
          <span class="dialog-readonly">{{ returnForm.gameName }}</span>
        </el-form-item>
        <el-form-item label="当前库存">
          <span class="dialog-readonly">
            {{ returnForm.currentStock }} / {{ returnForm.totalStock }}
          </span>
        </el-form-item>
      </el-form>
      <el-alert
        title="提示：归还时可直接登记损坏"
        type="info"
        :closable="false"
        show-icon
        style="margin-bottom: 16px"
      />
      <el-checkbox v-model="returnWithDamage" style="margin-bottom: 16px">
        归还时登记损坏
      </el-checkbox>
      <div v-if="returnWithDamage" class="return-damage-form">
        <el-form label-width="100px">
          <el-form-item label="损坏数量">
            <el-input-number
              v-model="returnDamageForm.quantity"
              :min="1"
              :max="returnForm.currentStock || 1"
              style="width: 100%"
            />
          </el-form-item>
          <el-form-item label="损坏原因">
            <el-input
              v-model="returnDamageForm.reason"
              type="textarea"
              :rows="3"
              placeholder="请描述损坏原因"
              maxlength="200"
              show-word-limit
            />
          </el-form-item>
          <el-form-item label="赔偿金额">
            <el-input-number
              v-model="returnDamageForm.compensationAmount"
              :min="0"
              :precision="2"
              :step="1"
              style="width: 100%"
            />
          </el-form-item>
        </el-form>
      </div>
      <template #footer>
        <el-button @click="returnDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="returnSubmitting" @click="handleSubmitReturn">
          确认归还
        </el-button>
      </template>
    </el-dialog>

    <el-dialog
      v-model="damageDialogVisible"
      title="损坏登记"
      width="420px"
      :close-on-click-modal="false"
    >
      <el-form
        ref="damageFormRef"
        :model="damageForm"
        :rules="damageFormRules"
        label-width="100px"
      >
        <el-form-item label="桌游">
          <span class="dialog-readonly">{{ damageForm.gameName }}</span>
        </el-form-item>
        <el-form-item label="当前库存">
          <span class="dialog-readonly">
            {{ damageForm.currentStock }} / {{ damageForm.totalStock }}
          </span>
        </el-form-item>
        <el-form-item label="损坏数量" prop="quantity">
          <el-input-number
            v-model="damageForm.quantity"
            :min="1"
            :max="damageForm.totalStock || 1"
            style="width: 100%"
          />
        </el-form-item>
        <el-form-item label="损坏原因" prop="reason">
          <el-input
            v-model="damageForm.reason"
            type="textarea"
            :rows="3"
            placeholder="请描述损坏原因"
            maxlength="200"
            show-word-limit
          />
        </el-form-item>
        <el-form-item label="赔偿金额" prop="compensationAmount">
          <el-input-number
            v-model="damageForm.compensationAmount"
            :min="0"
            :precision="2"
            :step="1"
            style="width: 100%"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="damageDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="damageSubmitting" @click="handleSubmitDamage">
          确认登记
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Picture, Box as BorrowIcon } from '@element-plus/icons-vue'
import SearchBar from '@/components/SearchBar/index.vue'
import DataTable from '@/components/DataTable/index.vue'
import MoneyDisplay from '@/components/MoneyDisplay/index.vue'
import FileUpload from '@/components/FileUpload/index.vue'
import * as boardgameApi from '@/api/boardgame'

const UPLOAD_PREFIX = import.meta.env.VITE_UPLOAD_PREFIX || ''

const loading = ref(false)
const tableData = ref([])

const searchInitial = {
  keyword: '',
  category: '',
  minPlayers: undefined,
  maxPlayers: undefined,
  onlyInStock: false
}

const searchForm = reactive({ ...searchInitial })

const pagination = reactive({
  total: 0,
  pageIndex: 1,
  pageSize: 10
})

const columns = [
  { prop: 'cover', label: '封面图', width: 100, slot: 'cover' },
  { prop: 'name', label: '名称', minWidth: 160 },
  { prop: 'category', label: '分类', width: 100, slot: 'category' },
  { prop: 'players', label: '玩家数', width: 100, slot: 'players' },
  { prop: 'stock', label: '库存(当前/总)', width: 130, slot: 'stock' },
  { prop: 'damaged', label: '损坏', width: 80, slot: 'damaged' },
  { prop: 'pricePerHour', label: '每小时费率', width: 130, slot: 'pricePerHour' }
]

function resolveCover(url) {
  if (!url) return ''
  if (url.startsWith('http')) return url
  return UPLOAD_PREFIX + url
}

// 分类选项：value 为后端 GameCategory 枚举英文值，label 为中文显示
const categoryOptions = [
  { value: 'Strategy', label: '策略' },
  { value: 'Party', label: '聚会' },
  { value: 'Family', label: '家庭' },
  { value: 'Children', label: '亲子' },
  { value: 'Cooperation', label: '合作' },
  { value: 'WarGame', label: '战棋' },
  { value: 'CardGame', label: '卡牌' }
]

const CATEGORY_LABELS = categoryOptions.reduce((acc, opt) => {
  acc[opt.value] = opt.label
  return acc
}, {})

function getCategoryLabel(category) {
  return CATEGORY_LABELS[category] || category || '-'
}

function getCategoryTagType(category) {
  const map = {
    Strategy: 'primary',
    Party: 'success',
    Family: 'warning',
    Children: 'danger',
    Cooperation: 'primary',
    WarGame: 'info',
    CardGame: 'success'
  }
  return map[category] || 'info'
}

function getErrorMessage(error, fallback) {
  return error?.response?.data?.message || error?.message || fallback
}

// 是否为 API 请求失败（有 response/message 的是请求错误；
// 无这些字段的是表单校验 reject 的字段错误对象，字段级红字已展示）
function isRequestError(error) {
  return !!(error && (error.response || error.request || error.message))
}

async function fetchList() {
  loading.value = true
  try {
    const params = {
      pageIndex: pagination.pageIndex,
      pageSize: pagination.pageSize,
      keyword: searchForm.keyword || undefined,
      category: searchForm.category || undefined,
      minPlayers: searchForm.minPlayers,
      maxPlayers: searchForm.maxPlayers,
      onlyInStock: searchForm.onlyInStock ? true : undefined
    }
    Object.keys(params).forEach((k) => {
      if (params[k] === undefined || params[k] === null) delete params[k]
    })
    const res = await boardgameApi.getList(params)
    tableData.value = res.items || res.data?.items || res.data || []
    // 兼容后端 PagedResult 的 totalCount 字段（request.js 已解包 data）
    pagination.total = res.totalCount || res.total || res.data?.totalCount || res.data?.total || 0
  } catch (error) {
    console.error('获取桌游列表失败:', error)
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
const formDialogTitle = computed(() => (isEdit.value ? '编辑桌游' : '新增桌游'))
const isEdit = ref(false)
const formSubmitting = ref(false)
const boardgameFormRef = ref(null)
const currentGameId = ref(null)

// 补货表单
const restockForm = reactive({ quantity: 1, remark: '' })
const restocking = ref(false)

function resetRestockForm() {
  Object.assign(restockForm, { quantity: 1, remark: '' })
}

async function handleRestock() {
  if (!isEdit.value || !currentGameId.value) return
  if (!restockForm.quantity || restockForm.quantity <= 0) {
    ElMessage.warning('请输入补货数量')
    return
  }
  restocking.value = true
  try {
    await boardgameApi.restock(currentGameId.value, {
      quantity: restockForm.quantity,
      remark: restockForm.remark || null
    })
    ElMessage.success(`补货成功，新增库存 ${restockForm.quantity} 件`)
    resetRestockForm()
    fetchList()
  } catch (error) {
    ElMessage.error(getErrorMessage(error, '补货失败'))
  } finally {
    restocking.value = false
  }
}

const boardgameForm = reactive({
  name: '',
  category: '',
  minPlayers: 2,
  maxPlayers: 4,
  description: '',
  cover: '',
  totalStock: 0,
  pricePerHour: 0
})

const validatePlayers = (rule, value, callback) => {
  if (value === null || value === undefined) {
    callback(new Error('请输入玩家数'))
    return
  }
  if (rule.fullField?.includes('maxPlayers') && value < boardgameForm.minPlayers) {
    callback(new Error('最大玩家不能小于最小玩家'))
    return
  }
  callback()
}

const boardgameFormRules = {
  name: [
    { required: true, message: '请输入桌游名称', trigger: 'blur' },
    { min: 1, max: 50, message: '名称长度为1-50位', trigger: 'blur' }
  ],
  category: [{ required: true, message: '请选择分类', trigger: 'change' }],
  minPlayers: [{ required: true, validator: validatePlayers, trigger: 'blur' }],
  maxPlayers: [{ required: true, validator: validatePlayers, trigger: 'blur' }],
  totalStock: [{ required: true, message: '请输入总库存', trigger: 'blur' }],
  pricePerHour: [{ required: true, message: '请输入每小时价格', trigger: 'blur' }]
}

function handleCreate() {
  isEdit.value = false
  currentGameId.value = null
  resetRestockForm()
  Object.assign(boardgameForm, {
    name: '',
    category: '',
    minPlayers: 2,
    maxPlayers: 4,
    description: '',
    cover: '',
    totalStock: 0,
    pricePerHour: 0
  })
  formDialogVisible.value = true
}

function handleEdit(row) {
  isEdit.value = true
  currentGameId.value = row.id
  resetRestockForm()
  Object.assign(boardgameForm, {
    name: row.name || '',
    category: row.category || '',
    minPlayers: row.minPlayers ?? 2,
    maxPlayers: row.maxPlayers ?? 4,
    description: row.description || '',
    cover: row.cover || '',
    totalStock: row.totalStock ?? 0,
    pricePerHour: row.pricePerHour ?? 0
  })
  formDialogVisible.value = true
}

async function handleSubmitForm() {
  if (!boardgameFormRef.value) return
  try {
    await boardgameFormRef.value.validate()
    formSubmitting.value = true
    if (isEdit.value) {
      await boardgameApi.update(currentGameId.value, { ...boardgameForm })
      ElMessage.success('编辑成功')
    } else {
      await boardgameApi.create({ ...boardgameForm })
      ElMessage.success('新增成功')
    }
    formDialogVisible.value = false
    fetchList()
  } catch (error) {
    if (isRequestError(error)) {
      ElMessage.error(getErrorMessage(error, isEdit.value ? '编辑失败' : '新增失败'))
    } else if (error !== false) {
      // 表单校验未通过：字段级红字已展示，不再重复提示
      console.warn('表单校验未通过:', error)
    }
  } finally {
    formSubmitting.value = false
  }
}

async function handleRemove(row) {
  try {
    await boardgameApi.remove(row.id)
    ElMessage.success('下架成功')
    fetchList()
  } catch (error) {
    ElMessage.error(getErrorMessage(error, '下架失败'))
    console.error('下架失败:', error)
  }
}

// 借出中的桌游（开台/预约借出未归还）
const borrowedDrawerVisible = ref(false)
const borrowedLoading = ref(false)
const borrowedList = ref([])

async function handleViewBorrowed() {
  borrowedDrawerVisible.value = true
  borrowedLoading.value = true
  try {
    const res = await boardgameApi.getBorrowed()
    borrowedList.value = Array.isArray(res) ? res : res?.data || []
  } catch (error) {
    ElMessage.error(getErrorMessage(error, '获取借出列表失败'))
    borrowedList.value = []
  } finally {
    borrowedLoading.value = false
  }
}

const borrowDialogVisible = ref(false)
const borrowSubmitting = ref(false)

const borrowForm = reactive({
  gameId: null,
  gameName: '',
  orderNo: ''
})

function handleBorrow(row) {
  borrowForm.gameId = row.id
  borrowForm.gameName = row.name
  borrowForm.orderNo = ''
  borrowDialogVisible.value = true
}

async function handleSubmitBorrow() {
  borrowSubmitting.value = true
  try {
    const res = await boardgameApi.borrow(borrowForm.gameId)
    const copyNos = res.data || res || []
    const nos = Array.isArray(copyNos) ? copyNos.join('、') : String(copyNos)
    ElMessage.success(nos ? `借出成功，副本编号: ${nos}` : '借出成功')
    borrowDialogVisible.value = false
    fetchList()
  } catch (error) {
    if (isRequestError(error)) {
      ElMessage.error(getErrorMessage(error, '借出失败'))
    }
  } finally {
    borrowSubmitting.value = false
  }
}

const returnDialogVisible = ref(false)
const returnSubmitting = ref(false)
const returnWithDamage = ref(false)

const returnForm = reactive({
  gameId: null,
  gameName: '',
  currentStock: 0,
  totalStock: 0
})

const returnDamageForm = reactive({
  quantity: 1,
  reason: '',
  compensationAmount: 0
})

function handleReturn(row) {
  returnForm.gameId = row.id
  returnForm.gameName = row.name
  returnForm.currentStock = row.currentStock || 0
  returnForm.totalStock = row.totalStock || 0
  returnWithDamage.value = false
  Object.assign(returnDamageForm, {
    quantity: 1,
    reason: '',
    compensationAmount: 0
  })
  returnDialogVisible.value = true
}

async function handleSubmitReturn() {
  try {
    returnSubmitting.value = true
    await boardgameApi.returnItem(returnForm.gameId)
    if (returnWithDamage.value) {
      await boardgameApi.recordDamage({
        boardgameId: returnForm.gameId,
        quantity: returnDamageForm.quantity,
        reason: returnDamageForm.reason,
        compensation: returnDamageForm.compensationAmount
      })
    }
    ElMessage.success(returnWithDamage.value ? '归还成功，损坏已登记' : '归还成功')
    returnDialogVisible.value = false
    fetchList()
  } catch (error) {
    if (isRequestError(error)) {
      ElMessage.error(getErrorMessage(error, '归还失败'))
    } else if (error !== false) {
      console.warn('表单校验未通过:', error)
    }
  } finally {
    returnSubmitting.value = false
  }
}

const damageDialogVisible = ref(false)
const damageSubmitting = ref(false)
const damageFormRef = ref(null)

const damageForm = reactive({
  boardgameId: null,
  gameName: '',
  currentStock: 0,
  totalStock: 0,
  quantity: 1,
  reason: '',
  compensationAmount: 0
})

const damageFormRules = {
  quantity: [{ required: true, message: '请输入损坏数量', trigger: 'blur' }],
  reason: [{ required: true, message: '请输入损坏原因', trigger: 'blur' }],
  compensationAmount: [{ required: true, message: '请输入赔偿金额', trigger: 'blur' }]
}

function handleDamage(row) {
  damageForm.boardgameId = row.id
  damageForm.gameName = row.name
  damageForm.currentStock = row.currentStock || 0
  damageForm.totalStock = row.totalStock || 0
  damageForm.quantity = 1
  damageForm.reason = ''
  damageForm.compensationAmount = 0
  damageDialogVisible.value = true
}

async function handleSubmitDamage() {
  if (!damageFormRef.value) return
  try {
    await damageFormRef.value.validate()
    await ElMessageBox.confirm(
      `确认登记损坏？数量: ${damageForm.quantity}，赔偿金额: ¥${damageForm.compensationAmount}`,
      '损坏登记确认',
      {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      }
    )
    damageSubmitting.value = true
    await boardgameApi.recordDamage({
      boardgameId: damageForm.boardgameId,
      quantity: damageForm.quantity,
      reason: damageForm.reason,
      compensation: damageForm.compensationAmount
    })
    ElMessage.success('损坏登记成功')
    damageDialogVisible.value = false
    fetchList()
  } catch (error) {
    if (error === 'cancel' || error === false) return
    if (isRequestError(error)) {
      ElMessage.error(getErrorMessage(error, '损坏登记失败'))
    } else {
      console.warn('表单校验未通过:', error)
    }
  } finally {
    damageSubmitting.value = false
  }
}

onMounted(() => {
  fetchList()
})
</script>

<style lang="scss" scoped>
.boardgame-list-page {
  .page-toolbar {
    margin-bottom: 16px;
  }

  .cover-image {
    width: 60px;
    height: 60px;
    border-radius: 4px;
    display: block;
  }

  .cover-placeholder {
    width: 60px;
    height: 60px;
    display: flex;
    align-items: center;
    justify-content: center;
    background-color: #f5f7fa;
    color: #c0c4cc;
    border-radius: 4px;
  }

  .stock-warn {
    color: #f56c6c;
    font-weight: 500;
  }

  .damaged-count {
    color: #f56c6c;
    font-weight: 600;
  }

  .dialog-readonly {
    color: #303133;
    font-weight: 500;
  }

  .text-muted {
    color: #909399;
    font-weight: 400;
  }

  .restock-section {
    background-color: #f8f9fb;
    border: 1px dashed #dcdfe6;
    border-radius: 6px;
    padding: 16px;
  }
}

.return-damage-form {
  padding: 16px;
  background-color: #f5f7fa;
  border-radius: 6px;
}
</style>
