<template>
  <div class="data-table-wrapper">
    <el-table
      :data="data"
      v-loading="loading"
      border
      stripe
      style="width: 100%"
      @selection-change="handleSelectionChange"
      @row-click="handleRowClick"
    >
      <el-table-column
        v-if="showSelection"
        type="selection"
        width="55"
        align="center"
      />
      <el-table-column
        v-if="showIndex"
        type="index"
        label="序号"
        width="70"
        align="center"
      />
      <el-table-column
        v-for="col in columns"
        :key="col.prop || col.slot || col.label"
        :prop="col.prop"
        :label="col.label"
        :width="col.width"
        :min-width="col.minWidth"
        :fixed="col.fixed"
        :align="col.align || 'center'"
      >
        <template #default="scope">
          <slot
            v-if="col.slot"
            :name="col.slot"
            :row="scope.row"
            :index="scope.$index"
            :column="col"
          ></slot>
          <span v-else-if="col.formatter">
            {{ col.formatter(scope.row, scope.$index) }}
          </span>
          <span v-else>
            {{ scope.row[col.prop] }}
          </span>
        </template>
      </el-table-column>
      <el-table-column
        v-if="$slots.actions"
        label="操作"
        :width="actionsWidth"
        fixed="right"
        align="center"
      >
        <template #default="scope">
          <slot name="actions" :row="scope.row" :index="scope.$index"></slot>
        </template>
      </el-table-column>
    </el-table>
    <Pagination
      v-if="showPagination"
      :total="pagination.total"
      v-model:page-index="pagination.pageIndex"
      v-model:page-size="pagination.pageSize"
      @change="handlePaginationChange"
    />
  </div>
</template>

<script setup>
import Pagination from '@/components/Pagination/index.vue'

const props = defineProps({
  columns: {
    type: Array,
    default: () => []
  },
  data: {
    type: Array,
    default: () => []
  },
  loading: {
    type: Boolean,
    default: false
  },
  showPagination: {
    type: Boolean,
    default: true
  },
  pagination: {
    type: Object,
    default: () => ({
      total: 0,
      pageIndex: 1,
      pageSize: 10
    })
  },
  showSelection: {
    type: Boolean,
    default: false
  },
  showIndex: {
    type: Boolean,
    default: false
  },
  actionsWidth: {
    type: [Number, String],
    default: 200
  }
})

const emit = defineEmits(['selection-change', 'pagination-change', 'row-click'])

function handleSelectionChange(selection) {
  emit('selection-change', selection)
}

function handleRowClick(row, column, event) {
  emit('row-click', row, column, event)
}

function handlePaginationChange(val) {
  emit('pagination-change', val)
}
</script>

<style lang="scss" scoped>
.data-table-wrapper {
  background-color: #fff;
  padding: 20px;
  border-radius: 4px;
}
</style>
