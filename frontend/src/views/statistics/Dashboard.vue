<template>
  <div class="dashboard">
    <el-row :gutter="20">
      <el-col :span="6">
        <div class="stat-card card-gradient-1">
          <div class="stat-icon">
            <el-icon><Money /></el-icon>
          </div>
          <div class="stat-content">
            <div class="stat-label">今日营业额</div>
            <div class="stat-value">￥{{ dashboard.todayRevenue?.toLocaleString() || '0' }}</div>
            <div v-if="dashboard.revenueTrend != null" class="stat-trend" :class="dashboard.revenueTrend >= 0 ? 'up' : 'down'">
              <el-icon v-if="dashboard.revenueTrend >= 0"><CaretTop /></el-icon>
              <el-icon v-else><CaretBottom /></el-icon>
              <span>{{ formatTrend(dashboard.revenueTrend) }}</span>
              <span class="trend-label">环比</span>
            </div>
            <div v-else class="stat-trend no-data">
              <span>--</span>
              <span class="trend-label">环比</span>
            </div>
          </div>
        </div>
      </el-col>
      <el-col :span="6">
        <div class="stat-card card-gradient-2">
          <div class="stat-icon">
            <el-icon><Document /></el-icon>
          </div>
          <div class="stat-content">
            <div class="stat-label">今日订单数</div>
            <div class="stat-value">{{ dashboard.todayOrders || 0 }}</div>
            <div v-if="dashboard.ordersTrend != null" class="stat-trend" :class="dashboard.ordersTrend >= 0 ? 'up' : 'down'">
              <el-icon v-if="dashboard.ordersTrend >= 0"><CaretTop /></el-icon>
              <el-icon v-else><CaretBottom /></el-icon>
              <span>{{ formatTrend(dashboard.ordersTrend) }}</span>
              <span class="trend-label">环比</span>
            </div>
            <div v-else class="stat-trend no-data">
              <span>--</span>
              <span class="trend-label">环比</span>
            </div>
          </div>
        </div>
      </el-col>
      <el-col :span="6">
        <div class="stat-card card-gradient-3">
          <div class="stat-icon">
            <el-icon><Histogram /></el-icon>
          </div>
          <div class="stat-content">
            <div class="stat-label">开台率</div>
            <div class="stat-value">{{ dashboard.tableUsageRate || 0 }}%</div>
            <div class="stat-progress">
              <el-progress
                type="dashboard"
                :percentage="dashboard.tableUsageRate || 0"
                :width="80"
                :show-text="false"
                :color="'#ffffff'"
              />
            </div>
          </div>
        </div>
      </el-col>
      <el-col :span="6">
        <div class="stat-card card-gradient-4">
          <div class="stat-icon">
            <el-icon><User /></el-icon>
          </div>
          <div class="stat-content">
            <div class="stat-label">在店人数</div>
            <div class="stat-value">{{ dashboard.currentPeople || 0 }}</div>
            <div class="stat-trend realtime">
              <span class="dot"></span>
              <span>实时</span>
            </div>
          </div>
        </div>
      </el-col>
    </el-row>

    <el-row :gutter="20" style="margin-top: 20px">
      <el-col :span="12">
        <div class="chart-card">
          <div class="card-title">近7天营业额趋势</div>
          <v-chart class="chart" :option="revenueOption" autoresize />
        </div>
      </el-col>
      <el-col :span="12">
        <div class="chart-card">
          <div class="card-title">热门桌游 TOP10</div>
          <v-chart class="chart" :option="popularGamesOption" autoresize />
        </div>
      </el-col>
    </el-row>

    <el-row :gutter="20" style="margin-top: 20px">
      <el-col :span="12">
        <div class="chart-card">
          <div class="card-title">各区域桌位使用占比</div>
          <v-chart class="chart" :option="tableUsageOption" autoresize />
        </div>
      </el-col>
      <el-col :span="12">
        <div class="chart-card">
          <div class="card-title">会员消费排行 TOP10</div>
          <el-table :data="memberRankList" size="small" border style="width: 100%">
            <el-table-column type="index" label="排名" width="60" align="center">
              <template #default="{ $index }">
                <el-tag
                  v-if="$index < 3"
                  :type="['danger', 'warning', 'success'][$index]"
                  effect="dark"
                  round
                  size="small"
                >
                  {{ $index + 1 }}
                </el-tag>
                <span v-else>{{ $index + 1 }}</span>
              </template>
            </el-table-column>
            <el-table-column label="会员" width="180">
              <template #default="{ row }">
                <div class="member-cell">
                  <el-avatar :size="32" :src="row.avatar">
                    {{ row.nickname?.charAt(0) }}
                  </el-avatar>
                  <span class="member-name">{{ row.nickname }}</span>
                </div>
              </template>
            </el-table-column>
            <el-table-column prop="orderCount" label="消费次数" width="100" align="center" />
            <el-table-column label="消费金额" align="right">
              <template #default="{ row }">
                <span class="amount-cell">￥{{ (row.totalAmount || 0).toLocaleString() }}</span>
              </template>
            </el-table-column>
          </el-table>
        </div>
      </el-col>
    </el-row>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, onBeforeUnmount, computed } from 'vue'
import {
  Money,
  Document,
  Histogram,
  User,
  CaretTop,
  CaretBottom
} from '@element-plus/icons-vue'
import * as statisticsApi from '@/api/statistics'

const dashboard = reactive({
  todayRevenue: 0,
  revenueTrend: 0,
  todayOrders: 0,
  ordersTrend: 0,
  tableUsageRate: 0,
  currentPeople: 0
})

const revenueTrendData = ref({ dates: [], amounts: [] })
const popularGamesData = ref([])
const tableUsageData = ref([])
const memberRankList = ref([])

let refreshTimer = null

// 环比安全处理：非有限数字一律返回 null（前端显示 --）
function safePercent(value) {
  const num = Number(value)
  return Number.isFinite(num) ? num : null
}

function formatTrend(value) {
  if (value == null) return '--'
  return `${Math.abs(value)}%`
}

const revenueOption = computed(() => ({
  tooltip: { trigger: 'axis' },
  grid: { left: 50, right: 20, top: 30, bottom: 40 },
  xAxis: {
    type: 'category',
    data: revenueTrendData.value.dates,
    boundaryGap: false,
    axisLine: { lineStyle: { color: '#dcdfe6' } }
  },
  yAxis: {
    type: 'value',
    axisLabel: { formatter: '￥{value}' }
  },
  series: [
    {
      type: 'line',
      smooth: true,
      symbol: 'circle',
      symbolSize: 8,
      data: revenueTrendData.value.amounts,
      lineStyle: { color: '#409eff', width: 3 },
      itemStyle: { color: '#409eff' },
      areaStyle: {
        color: {
          type: 'linear',
          x: 0, y: 0, x2: 0, y2: 1,
          colorStops: [
            { offset: 0, color: 'rgba(64, 158, 255, 0.3)' },
            { offset: 1, color: 'rgba(64, 158, 255, 0.02)' }
          ]
        }
      }
    }
  ]
}))

const popularGamesOption = computed(() => ({
  tooltip: { trigger: 'axis', axisPointer: { type: 'shadow' } },
  grid: { left: 110, right: 30, top: 20, bottom: 30 },
  xAxis: { type: 'value' },
  yAxis: {
    type: 'category',
    data: popularGamesData.value.map(x => x.name).reverse(),
    axisLabel: { interval: 0 }
  },
  series: [
    {
      type: 'bar',
      data: popularGamesData.value.map(x => x.count).reverse(),
      barWidth: 16,
      itemStyle: {
        borderRadius: [0, 8, 8, 0],
        color: {
          type: 'linear',
          x: 0, y: 0, x2: 1, y2: 0,
          colorStops: [
            { offset: 0, color: '#67C23A' },
            { offset: 1, color: '#95D475' }
          ]
        }
      }
    }
  ]
}))

const tableUsageOption = computed(() => ({
  tooltip: { trigger: 'item', formatter: '{b}: {c}桌 ({d}%)' },
  legend: { orient: 'vertical', right: 20, top: 'center' },
  series: [
    {
      type: 'pie',
      radius: ['45%', '70%'],
      center: ['40%', '50%'],
      avoidLabelOverlap: false,
      itemStyle: { borderRadius: 8, borderColor: '#fff', borderWidth: 2 },
      label: { show: false },
      emphasis: {
        label: { show: true, fontSize: 16, fontWeight: 'bold' }
      },
      data: tableUsageData.value.map((x, i) => ({
        name: x.name,
        value: x.count,
        itemStyle: { color: ['#409eff', '#67c23a', '#e6a23c'][i % 3] }
      }))
    }
  ]
}))

async function fetchDashboard() {
  try {
    const res = await statisticsApi.getDashboard()
    const data = res.data || res
    Object.assign(dashboard, {
      todayRevenue: data.todayRevenue ?? 12580,
      revenueTrend: safePercent(data.revenueTrendPercent) ?? safePercent(data.revenueTrend),
      todayOrders: data.todayOrders ?? 48,
      ordersTrend: safePercent(data.ordersTrendPercent) ?? safePercent(data.ordersTrend),
      tableUsageRate: data.tableUsageRate ?? 76,
      currentPeople: data.currentPeople ?? 62
    })
    if (data.memberRank?.length) {
      memberRankList.value = data.memberRank
    } else {
      memberRankList.value = generateMockMembers()
    }
  } catch {
    Object.assign(dashboard, {
      todayRevenue: 12580, revenueTrend: 12.5,
      todayOrders: 48, ordersTrend: 8.3,
      tableUsageRate: 76, currentPeople: 62
    })
    memberRankList.value = generateMockMembers()
  }
}

async function fetchRevenueTrend() {
  try {
    const res = await statisticsApi.getRevenueTrend()
    const data = res.data || res
    revenueTrendData.value = {
      dates: data.dates || generateMockDates(7),
      amounts: data.amounts || [8000, 9200, 10100, 9800, 11200, 12000, 12580]
    }
  } catch {
    revenueTrendData.value = {
      dates: generateMockDates(7),
      amounts: [8000, 9200, 10100, 9800, 11200, 12000, 12580]
    }
  }
}

async function fetchPopularGames() {
  try {
    const res = await statisticsApi.getPopularGames()
    popularGamesData.value = res.data?.list || res.data || res || generateMockGames()
  } catch {
    popularGamesData.value = generateMockGames()
  }
  tableUsageData.value = [
    { name: '大厅', count: 18 },
    { name: '包间', count: 6 },
    { name: '卡座', count: 8 }
  ]
}

function generateMockDates(n) {
  const days = []
  for (let i = n - 1; i >= 0; i--) {
    const d = new Date()
    d.setDate(d.getDate() - i)
    days.push(`${d.getMonth() + 1}/${d.getDate()}`)
  }
  return days
}

function generateMockGames() {
  const games = ['卡坦岛', '璀璨宝石', '马尼拉', '三国杀', '狼人杀', 'UNO', '大富翁', '冷战热斗', '镰刀战争', '展翅翱翔']
  return games.map((g, i) => ({ name: g, count: 50 - i * 3 - Math.floor(Math.random() * 5) }))
}

function generateMockMembers() {
  const names = ['王小明', '李四', '赵六', '钱七', '孙八', '周九', '吴十', '郑十一', '冯十二', '陈十三']
  return names.map((n, i) => ({
    nickname: n,
    avatar: '',
    orderCount: 50 - i * 4,
    totalAmount: 8000 - i * 600
  }))
}

function fetchAll() {
  fetchDashboard()
  fetchRevenueTrend()
  fetchPopularGames()
}

onMounted(() => {
  fetchAll()
  refreshTimer = setInterval(fetchAll, 30000)
})

onBeforeUnmount(() => {
  if (refreshTimer) {
    clearInterval(refreshTimer)
  }
})
</script>

<style lang="scss" scoped>
.dashboard {
  .stat-card {
    position: relative;
    padding: 20px;
    border-radius: 12px;
    color: #fff;
    display: flex;
    align-items: center;
    overflow: hidden;
    min-height: 140px;

    .stat-icon {
      width: 60px;
      height: 60px;
      border-radius: 50%;
      background: rgba(255, 255, 255, 0.2);
      display: flex;
      align-items: center;
      justify-content: center;
      margin-right: 16px;

      .el-icon {
        font-size: 30px;
      }
    }

    .stat-content {
      flex: 1;
      position: relative;
      z-index: 2;
    }

    .stat-label {
      font-size: 14px;
      opacity: 0.85;
      margin-bottom: 8px;
    }

    .stat-value {
      font-size: 28px;
      font-weight: 700;
      margin-bottom: 8px;
      font-variant-numeric: tabular-nums;
    }

    .stat-trend {
      display: flex;
      align-items: center;
      gap: 4px;
      font-size: 13px;

      &.up { color: #ffe4b5; }
      &.down { color: #ffcccc; }

      &.no-data {
        color: rgba(255, 255, 255, 0.75);
      }

      .trend-label {
        opacity: 0.7;
        margin-left: 4px;
      }

      &.realtime {
        opacity: 0.85;

        .dot {
          width: 8px;
          height: 8px;
          border-radius: 50%;
          background: #67c23a;
          animation: pulse 1.5s infinite;
        }
      }
    }

    .stat-progress {
      position: absolute;
      right: 10px;
      top: 50%;
      transform: translateY(-50%);
    }

    &::before {
      content: '';
      position: absolute;
      right: -30px;
      top: -30px;
      width: 160px;
      height: 160px;
      border-radius: 50%;
      background: rgba(255, 255, 255, 0.08);
    }
  }

  .card-gradient-1 {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  }

  .card-gradient-2 {
    background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
  }

  .card-gradient-3 {
    background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);
  }

  .card-gradient-4 {
    background: linear-gradient(135deg, #43e97b 0%, #38f9d7 100%);
  }

  .chart-card {
    background: #fff;
    border-radius: 8px;
    padding: 20px;
    box-shadow: 0 2px 12px rgba(0, 0, 0, 0.06);
    min-height: 380px;

    .card-title {
      font-size: 16px;
      font-weight: 600;
      margin-bottom: 16px;
      padding-left: 10px;
      border-left: 4px solid #409eff;
    }

    .chart {
      height: 320px;
      width: 100%;
    }
  }

  .member-cell {
    display: flex;
    align-items: center;
    gap: 10px;

    .member-name {
      font-weight: 500;
    }
  }

  .amount-cell {
    color: #f56c6c;
    font-weight: 600;
    font-variant-numeric: tabular-nums;
  }
}

@keyframes pulse {
  0% { opacity: 1; }
  50% { opacity: 0.4; }
  100% { opacity: 1; }
}
</style>
