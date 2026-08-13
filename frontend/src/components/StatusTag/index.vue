<template>
  <el-tag :type="resolvedType" effect="light" round>
    {{ displayText }}
  </el-tag>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  status: {
    type: String,
    default: ''
  },
  type: {
    type: String,
    default: ''
  }
})

const STATUS_MAP = {
  success: ['空闲', '未使用', '可用', '正常', '已激活', '启用'],
  danger: ['占用', '已完成', '已结束', '已禁用', '停用', '异常'],
  warning: ['预约', '待处理', '待审核', '进行中', '待支付', '待确认'],
  info: ['维修', '已取消', '已删除', '已过期', '已失效', '草稿'],
  primary: ['其他']
}

const resolvedType = computed(() => {
  if (props.type) return props.type
  for (const [type, keywords] of Object.entries(STATUS_MAP)) {
    if (keywords.some(k => props.status && props.status.includes(k))) {
      return type
    }
  }
  return 'info'
})

const displayText = computed(() => props.status || '-')
</script>

<style lang="scss" scoped>
</style>
