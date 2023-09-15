<template>
  <div class="container-fluid">
    <div class="card custom-card">
      <TableSlip :title="title" :data="data"></TableSlip>
    </div>
  </div>
</template>

<script>
import TableSlip from "@/ui/TableSlip.vue";

export default {
  name: "SlipPreview",
  props: ["title", "data"],
  components: {
    TableSlip,
  },
  methods: {},
  computed: {},
};
</script>

<style scoped>
.custom-card {
  position: relative;
  padding: 0px !important;
  height: auto;
  max-height: 80vh;
  overflow-y: auto !important;
  overflow: scroll;
  border: 3px solid #7e4816 !important;
}
.section-title {
  position: absolute;
  left: 25px;

  color: rgb(255, 255, 255);
  font-family: "Times New Roman", Times, serif;
  font-size: 120%;
  background-color: #7e4816;
  text-align: center;
  margin-bottom: 5px;
}

h1 {
  display: flex;
  justify-content: space-between;
  font-size: xx-large;
  text-align: start;
  /*  right: 42%; */
}
.text {
  position: relative;
  bottom: 30px;
  padding-bottom: 20px;
}

.card {
  background-image: url("../assets/img/slipbg.jpg");
  background-repeat: no-repeat;
  background-size: cover;
  padding: 20px; /* Adjust padding as needed */
  border: 1px solid black;
  padding: 10px;
  width: 100%;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.2);
}
</style>
<!-- <p class="section-title">{{ title }}</p> -->
<!-- <div class="text">
      <h1>{{ title }}</h1>
    </div> -->
