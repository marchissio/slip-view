<template>
  <select
    @change="optionSelected()"
    class="form-select custom-select"
    v-model="selectedOption"
  >
    <option
      v-for="option in options"
      :key="option.id"
      :data-value="option.value"
      :value="option.id"
    >
      {{ option.title }}
    </option>
  </select>
</template>
<script>
export default {
  props: ["options", "id", "selected"],
  data() {
    return {
      selectedOption: this.selected | 0,
    };
  },
  methods: {
    optionSelected() {
      this.$emit("changeOption", { option: this.selectedOption, id: this.id });
    },
  },
};
</script>
<style>
select {
  width: 25% !important;
  background-color: rgb(242, 245, 243) !important;
  cursor: pointer;
}

select option {
  padding: 10px !important;
  border-bottom: 1px solid black !important;
}

.custom-select {
  box-shadow: 1px 2px 11px 0px !important;
  border-radius: 0px !important;
  border: 3px solid #5b3714 !important;
}
</style>
