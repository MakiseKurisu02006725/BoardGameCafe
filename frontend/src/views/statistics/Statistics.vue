<template>
  <div class="statistics-view">
    <el-tabs v-model="activeTab" type="card">
      <el-tab-pane label="时段客流分析" name="traffic">
        <div class="tab-content">
          <div class="filter-bar">
            <el-date-picker
              v-model="dateRange"
              type="daterange"
              range-separator="至"
              start-placeholder="开始日期"
              end-placeholder="结束日期"
              value-format="YYYY-MM-DD"
              style="width: 280px"
              @change="fetchTrafficData"
            />
          </div>

          <div class="chart-card">
            <div class="card-title">
              <span>7天 x 24小时 客流热力图</span>
              <el-radio-group v-model="trafficChartType" size="small">
                <el-radio-button label="heatmap">热力图</el-radio-button>
                <el-radio-button label="line">折线图</el-radio-button>
              </el-radio-group>
            </div>
            <v-chart
              v-if="trafficChartType === 'heatmap'"
              class="chart"
              :option="heatmapOption"
              autoresize
            />
            <v-chart
              v-else
              class="chart"
              :option="trafficLineOption"
              autoresize
            />
          </div>
        </div>
      </el-tab-pane>

      <el-tab-pane label="会员RFM分析" name="rfm">
        <div class="tab-content">
          <div class="chart-card">
            <div class="card-title">
              <span>会员RFM分布</span>
              <el-radio-group v-model="rfmChartType" size="small">
                <el-radio-button label="scatter3D">3D散点图</el-radio-button>
                <el-radio-button label="table">表格视图</el-radio-button>
              </el-radio-group>
            </div>
            <div v-if="rfmChartType === 'scatter3D'" class="chart chart-lg">
              <v-chart
                v-if="rfmList.length"
                :option="rfmScatterOption"
                autoresize
                style="height: 100%; width: 100%"
              />
              <div v-else class="empty-tip">暂无数据</div>
            </div>
            <el-table
              v-else
              :data="rfmList"
              border
              stripe
              style="width: 100%"
            >
              <el-table-column type="index" label="序号" width="70" align="center" />
              <el-table-column prop="memberName" label="会员昵称" width="140" />
              <el-table-column label="R(最近消费天数)" width="140" align="center">
                <template #default="{ row }">
                  <el-tag :type="row.r <= 7 ? 'success' : row.r <= 30 ? 'warning' : 'danger'" size="small">
                    {{ row.r }} 天
                  </el-tag>
                </template>
              </el-table-column>
              <el-table-column label="F(消费频次)" width="120" align="center">
                <template #default="{ row }">
                  <el-tag :type="row.f >= 10 ? 'success' : row.f >= 5 ? 'warning' : 'info'" size="small">
                    {{ row.f }} 次
                  </el-tag>
                </template>
              </el-table-column>
              <el-table-column label="M(消费总额)" width="160" align="right">
                <template #default="{ row }">
                  ￥{{ (row.m || 0).toLocaleString() }}
                </template>
              </el-table-column>
              <el-table-column prop="score" label="RFM Score" width="120" align="center">
                <template #default="{ row }">
                  <el-progress
                    type="dashboard"
                    :percentage="row.score || 0"
                    :width="60"
                    :stroke-width="8"
                  />
                </template>
              </el-table-column>
              <el-table-column prop="tag" label="标签" width="140" align="center">
                <template #default="{ row }">
                  <StatusTag :status="row.tag" :type="getTagType(row.tag)" />
                </template>
              </el-table-column>
            </el-table>
          </div>

          <el-row :gutter="20" style="margin-top: 20px">
            <el-col :span="24">
              <div class="summary-card">
                <div class="card-title">会员分群统计</div>
                <div class="tag-summary">
                  <div
                    v-for="tag in tagSummary"
                    :key="tag.name"
                    class="tag-item"
                    :style="{ borderLeftColor: tag.color }"
                  >
                    <div class="tag-name">
                      <el-tag :color="tag.color" effect="dark" size="small">
                        {{ tag.name }}
                      </el-tag>
                    </div>
                    <div class="tag-count">{{ tag.count }} 人</div>
                    <div class="tag-desc">{{ tag.desc }}</div>
                  </div>
                </div>
              </div>
            </el-col>
          </el-row>
        </div>
      </el-tab-pane>
    </el-tabs>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import dayjs from 'dayjs'
import StatusTag from '@/components/StatusTag/index.vue'
import * as statisticsApi from '@/api/statistics'

const activeTab = ref('traffic')
const trafficChartType = ref('heatmap')
const rfmChartType = ref('table')
const dateRange = ref([
  dayjs().subtract(6, 'day').format('YYYY-MM-DD'),
  dayjs().format('YYYY-MM-DD')
])

const trafficData = ref([])
const rfmList = ref([])

const TAG_CONFIG = {
  '重要价值': { color: '#f56c6c', type: 'danger', desc: 'R高F高M高 - 核心VIP，重点维护' },
  '重要发展': { color: '#e6a23c', type: 'warning', desc: 'R高F低M高 - 潜力客户，提升频次' },
  '重要保持': { color: '#409eff', type: 'primary', desc: 'R低F高M高 - 老客，唤醒召回' },
  '重要挽留': { color: '#909399', type: 'info', desc: 'R低F低M高 - 流失风险，促销挽留' },
  '一般价值': { color: '#67c23a', type: 'success', desc: 'R高F高M低 - 普通会员，稳步培养' },
  '一般发展': { color: '#b3d19d', type: 'success', desc: 'R高F低M低 - 新会员，引导复购' },
  '一般保持': { color: '#95b9e3', type: 'primary', desc: 'R低F高M低 - 高频低价，提升客单' },
  '流失会员': { color: '#c0c4cc', type: 'info', desc: 'R低F低M低 - 已流失，低成本触达' },
  '新客户': { color: '#2f9e8f', type: 'success', desc: '仅消费1次或注册30天内 - 单独培养' }
}

function getTagType(tag) {
  return TAG_CONFIG[tag]?.type || 'info'
}

const tagSummary = computed(() => {
  const counts = {}
  rfmList.value.forEach(m => {
    counts[m.tag] = (counts[m.tag] || 0) + 1
  })
  return Object.keys(TAG_CONFIG).map(name => ({
    name,
    count: counts[name] || 0,
    color: TAG_CONFIG[name].color,
    desc: TAG_CONFIG[name].desc
  }))
})

const weekDays = computed(() => {
  const days = []
  const [start, end] = dateRange.value
  if (!start || !end) return ['周一', '周二', '周三', '周四', '周五', '周六', '周日']
  let d = dayjs(start)
  const e = dayjs(end)
  while (d.isBefore(e) || d.isSame(e, 'day')) {
    days.push(d.format('MM-DD'))
    d = d.add(1, 'day')
  }
  return days
})

const hours = Array.from({ length: 24 }, (_, i) => `${i}:00`)

const heatmapOption = computed(() => {
  // max 取数据最大值与固定下限 20 的较大值，保证有数据时颜色能正确分层
  const maxVal = Math.max(20, ...trafficData.value.map(d => d[2]))
  return {
    tooltip: {
      position: 'top',
      formatter: (p) => `${weekDays.value[p.data[1]]} ${hours[p.data[0]]}<br/>客流: ${p.data[2]}人`
    },
    grid: { height: '60%', top: '10%', left: 80, right: 40 },
    xAxis: {
      type: 'category',
      data: hours,
      splitArea: { show: true }
    },
    yAxis: {
      type: 'category',
      data: weekDays.value,
      splitArea: { show: true }
    },
    visualMap: {
      min: 0,
      max: maxVal,
      calculable: true,
      orient: 'horizontal',
      left: 'center',
      bottom: '2%',
      inRange: {
        color: ['#ebedf0', '#c6e48b', '#7bc96f', '#239a3b', '#196127']
      }
    },
    series: [
      {
        name: '客流量',
        type: 'heatmap',
        data: trafficData.value,
        label: { show: false },
        emphasis: {
          itemStyle: {
            shadowBlur: 10,
            shadowColor: 'rgba(0, 0, 0, 0.4)'
          }
        }
      }
    ]
  }
})

const trafficLineOption = computed(() => {
  const seriesData = weekDays.value.map((d, di) => ({
    name: d,
    type: 'line',
    smooth: true,
    symbol: 'circle',
    symbolSize: 6,
    data: hours.map((_, hi) => {
      const item = trafficData.value.find(t => t[0] === hi && t[1] === di)
      return item ? item[2] : 0
    })
  }))
  return {
    tooltip: { trigger: 'axis' },
    legend: { data: weekDays.value, top: 0 },
    grid: { left: 50, right: 30, top: 50, bottom: 40 },
    xAxis: { type: 'category', data: hours, boundaryGap: false },
    yAxis: { type: 'value', name: '人数' },
    series: seriesData
  }
})

const rfmScatterOption = computed(() => {
  const tags = Object.keys(TAG_CONFIG)
  // 3D 散点图数据格式：[R值, F值, M值]，每个点以对象携带会员名称
  const series = tags
    .map(tag => ({
      name: tag,
      type: 'scatter3D',
      itemStyle: { color: TAG_CONFIG[tag].color, opacity: 0.85 },
      symbolSize: 12,
      data: rfmList.value
        .filter(m => m.tag === tag)
        .map(m => ({ value: [m.r, m.f, m.m], name: m.memberName }))
    }))
    .filter(s => s.data.length > 0)
  return {
    tooltip: {
      formatter: (p) => `${p.name || p.data?.name || ''}<br/>R:${p.value[0]}天 F:${p.value[1]}次 M:￥${(p.value[2] || 0).toFixed(0)}`
    },
    legend: { data: series.map(s => s.name), top: 0 },
    grid3D: {
      viewControl: { autoRotate: true, autoRotateSpeed: 8 },
      boxWidth: 100,
      boxDepth: 80,
      boxHeight: 60,
      light: { main: { intensity: 1.2 }, ambient: { intensity: 0.3 } }
    },
    xAxis3D: { name: 'R(天)', type: 'value', min: 0 },
    yAxis3D: { name: 'F(次)', type: 'value', min: 0 },
    zAxis3D: { name: 'M(元)', type: 'value', min: 0 },
    series
  }
})

async function fetchTrafficData() {
  const [start, end] = dateRange.value
  if (!start || !end) return
  try {
    const res = await statisticsApi.getFlow({ startDate: start, endDate: end })
    const flowList = Array.isArray(res) ? res : (res?.data?.list || res?.data || [])
    // 后端返回 [{ date, hour, count }]，转换为 [hourIndex, dayIndex, value]，无数据的时段补 0
    const map = {}
    flowList.forEach(item => {
      const key = `${dayjs(item.date).format('YYYY-MM-DD')}-${item.hour}`
      map[key] = item.count || 0
    })
    const data = []
    weekDays.value.forEach((d, di) => {
      const dateStr = dayjs(dateRange.value[0]).add(di, 'day').format('YYYY-MM-DD')
      for (let hi = 0; hi < 24; hi++) {
        data.push([hi, di, map[`${dateStr}-${hi}`] || 0])
      }
    })
    trafficData.value = data
  } catch (e) {
    console.warn('获取客流数据失败:', e)
    trafficData.value = []
  }
}

function generateMockRfm() {
  const names = ['王小明', '李四', '赵六', '钱七', '孙八', '周九', '吴十', '郑十一', '冯十二', '陈十三',
    '褚十四', '卫十五', '蒋十六', '沈十七', '韩十八', '杨十九', '朱二十', '秦廿一', '尤廿二', '许廿三']
  const seeds = {
    '重要价值': { r: 5, f: 18, m: 8000 },
    '重要发展': { r: 6, f: 4, m: 6000 },
    '重要保持': { r: 45, f: 15, m: 7000 },
    '重要挽留': { r: 60, f: 3, m: 5000 },
    '一般价值': { r: 12, f: 6, m: 1800 },
    '一般发展': { r: 10, f: 2, m: 600 },
    '一般保持': { r: 40, f: 8, m: 900 },
    '流失会员': { r: 120, f: 1, m: 300 },
    '新客户': { r: 3, f: 1, m: 150 }
  }
  const tagNames = Object.keys(seeds)
  return names.map((name, i) => {
    const tag = tagNames[i % tagNames.length]
    const s = seeds[tag]
    const r = Math.max(1, Math.round(s.r + (Math.random() - 0.5) * s.r * 0.5))
    const f = Math.max(1, Math.round(s.f + (Math.random() - 0.5) * s.f * 0.4))
    const m = Math.max(1, Math.round(s.m + (Math.random() - 0.5) * s.m * 0.3))
    const rScore = r <= 7 ? 5 : r <= 15 ? 4 : r <= 30 ? 3 : r <= 60 ? 2 : 1
    const fScore = f >= 15 ? 5 : f >= 10 ? 4 : f >= 5 ? 3 : f >= 3 ? 2 : 1
    const mScore = m >= 6000 ? 5 : m >= 4000 ? 4 : m >= 2000 ? 3 : m >= 800 ? 2 : 1
    const score = Math.round(((rScore + fScore + mScore) / 15) * 100)

    return { memberName: name, r, f, m, score, tag }
  })
}

async function fetchRfmData() {
  try {
    const res = await statisticsApi.getMemberRfm()
    const data = Array.isArray(res) ? res : (res?.data?.list || res?.data || [])
    if (Array.isArray(data) && data.length) {
      rfmList.value = data.map(d => ({
        memberName: d.memberName || d.nickName || '会员',
        r: typeof d.r === 'number' ? d.r : (d.recency || 0),
        f: typeof d.f === 'number' ? d.f : (d.frequency || 0),
        m: typeof d.m === 'number' ? d.m : (d.monetary || 0),
        score: typeof d.score === 'number' ? d.score : (d.rfmScore || 0),
        tag: d.tag || '一般价值'
      }))
      return
    }
  } catch (e) {
    console.warn('获取RFM数据失败，使用模拟数据:', e)
  }
  rfmList.value = generateMockRfm()
}

onMounted(() => {
  fetchTrafficData()
  fetchRfmData()
})
</script>

<style lang="scss" scoped>
.statistics-view {
  :deep(.el-tabs__content) {
    padding: 0;
  }

  .tab-content {
    padding-top: 20px;
  }

  .filter-bar {
    margin-bottom: 20px;
    padding: 16px 20px;
    background: #fff;
    border-radius: 8px;
  }

  .chart-card,
  .summary-card {
    background: #fff;
    border-radius: 8px;
    padding: 20px;
    box-shadow: 0 2px 12px rgba(0, 0, 0, 0.06);

    .card-title {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 16px;
      padding-left: 10px;
      border-left: 4px solid #409eff;
      font-size: 16px;
      font-weight: 600;
    }

    .chart {
      height: 420px;
      width: 100%;

      &.chart-lg {
        height: 500px;
      }
    }

    .empty-tip {
      height: 100%;
      display: flex;
      align-items: center;
      justify-content: center;
      color: #c0c4cc;
      font-size: 14px;
    }
  }

  .summary-card {
    .tag-summary {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 16px;
    }

    .tag-item {
      padding: 16px;
      border-left: 4px solid;
      background: #f5f7fa;
      border-radius: 4px;

      .tag-name {
        margin-bottom: 8px;
      }

      .tag-count {
        font-size: 22px;
        font-weight: 700;
        margin-bottom: 4px;
      }

      .tag-desc {
        font-size: 12px;
        color: #909399;
      }
    }
  }
}
</style>
