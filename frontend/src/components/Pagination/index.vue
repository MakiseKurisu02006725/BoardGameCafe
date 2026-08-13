<template>
  <div class="pagination-wrapper">
    <el-pagination
      :total="total"
      v-model:current-page="currentPage"
      v-model:page-size="currentPageSize"
      :page-sizes="[10, 20, 50, 100]"
      :layout="'total, sizes, prev, pager, next, jumper'"
      background
      @size-change="handleSizeChange"
      @current-change="handleCurrentChange"
    />
  </div>
</template>

<script setup>
import { computed, watch } from 'vue'

const props = defineProps({
  total: {
    type: Number,
    default: 0
  },
  pageIndex: {
    type: Number,
    default: 1
  },
  pageSize: {
    type: Number,
    default: 10
  }
})

const emit = defineEmits(['update:pageIndex', 'update:pageSize', 'change'])

const currentPage = computed({
  get: () => props.pageIndex,
  set: (val) => emit('update:pageIndex', val)
})

const currentPageSize = computed({
  get: () => props.pageSize,
  set: (val) => emit('update:pageSize', val)
})

function handleSizeChange(val) {
  emit('update:pageSize', val)
  emit('change', { pageIndex: props.pageIndex, pageSize: val })
}

function handleCurrentChange(val) {
  emit('update:pageIndex', val)
  emit('change', { pageIndex: val, pageSize: props.pageSize })
}
</script>

<style lang="scss" scoped>
.pagination-wrapper {
  display: flex;
  justify-content: flex-end;
  padding: 20px 0 0;
}
</style>
