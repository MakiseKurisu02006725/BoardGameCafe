import { createApp } from 'vue'
import App from './App.vue'
import ElementPlus from 'element-plus'
import zhCn from 'element-plus/es/locale/lang/zh-cn'
import * as ElementPlusIconsVue from '@element-plus/icons-vue'
import { createPinia } from 'pinia'
import router from '@/router'
import { use } from 'echarts/core'
import { CanvasRenderer } from 'echarts/renderers'
import { LineChart, BarChart, PieChart, HeatmapChart } from 'echarts/charts'
import {
  TitleComponent,
  TooltipComponent,
  GridComponent,
  LegendComponent,
  VisualMapComponent
} from 'echarts/components'
import 'echarts-gl'
import VChart from 'vue-echarts'
import { ElMessage, ElMessageBox, ElNotification } from 'element-plus'
import dayjs from 'dayjs'
import 'dayjs/locale/zh-cn'
import 'element-plus/dist/index.css'
import 'nprogress/nprogress.css'
import '@/styles/index.scss'

// dayjs 中文本地化
dayjs.locale('zh-cn')

// 【关键】注册 ECharts 核心+渲染器+所需图表/组件 —— 否则 renderer='undefined' 报错
// 热力图需要 HeatmapChart + VisualMapComponent；3D 散点图由 echarts-gl 提供
use([
  CanvasRenderer,
  LineChart,
  BarChart,
  PieChart,
  HeatmapChart,
  TitleComponent,
  TooltipComponent,
  GridComponent,
  LegendComponent,
  VisualMapComponent
])

const app = createApp(App)
const pinia = createPinia()

for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
  app.component(key, component)
}

app.component('VChart', VChart)

app.use(ElementPlus, { locale: zhCn })
app.use(pinia)
app.use(router)

app.config.globalProperties.$message = ElMessage
app.config.globalProperties.$messageBox = ElMessageBox
app.config.globalProperties.$notify = ElNotification

app.mount('#app')
