<template>
  <div class="input-text">
    <input
      type="text"
      class="form-control custom-input"
      v-model="inputValue"
      :placeholder="placeholder"
    />
  </div>
</template>

<script>
export default {
  props: ["placeholder"],
  data() {
    return {
      inputValue: "",
    };
  },
  watch: {
    inputValue(newValue) {
      const numericValue = newValue.replace(/\D/g, "");

      this.inputValue = numericValue;

      this.$emit("changeValue", numericValue);
    },
  },
};
</script>

<style scoped>
input {
  width: 80px;
}
.input-text {
  margin: 0px;
}
.custom-input{
  box-shadow: 1px 2px 11px 0px;
    border-radius: 0px;
    border: 3px solid #5b3714;
}
</style>
