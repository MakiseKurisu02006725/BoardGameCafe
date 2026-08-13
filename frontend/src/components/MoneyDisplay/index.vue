<template>
  <span class="money-display" :class="{ 'is-negative': isNegative }">
    <span class="money-prefix">{{ prefix }}</span>
    <span class="money-value">{{ formattedValue }}</span>
  </span>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  value: {
    type: [Number, String],
    default: 0
  },
  prefix: {
    type: String,
    default: '¥'
  },
  decimals: {
    type: Number,
    default: 2
  }
})

const numericValue = computed(() => {
  const val = parseFloat(props.value)
  return isNaN(val) ? 0 : val
})

const isNegative = computed(() => numericValue.value < 0)

const formattedValue = computed(() => {
  const absValue = Math.abs(numericValue.value)
  return absValue.toLocaleString('zh-CN', {
    minimumFractionDigits: props.decimals,
    maximumFractionDigits: props.decimals
  })
})
</script>

<style lang="scss" scoped>
.money-display {
  font-variant-numeric: tabular-nums;
  color: #303133;
  font-weight: 500;

  &.is-negative {
    color: #f56c6c;
  }
}

.money-prefix {
  margin-right: 2px;
}

.money-value {
  font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif;
}
</style>
