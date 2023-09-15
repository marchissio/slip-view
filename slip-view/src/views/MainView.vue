<template>
  <div class="container-fluid">
    <div class="row slip-row">
      <div class="col-md-4">
        <div class="row flex">
          <div class="col-md-3">
            <h3 class="titles">{{ sections.retail }}</h3>
          </div>
          <div class="col-md-6">
            <input
              id="tiketID"
              type="text"
              v-model="inputValue"
              autocomplete="off"
              class="form-control custom-input"
              placeholder="Unesi ID tiketa"
              @keydown.enter="loadTicketOnEnter"
            />
          </div>

          <div class="col-md-3 button-id">
            <button
              id="ucitajID"
              type="button"
              class="btn custom-button"
              @click="loadTicketOnButton"
            >
              Učitaj
            </button>
            <ModalSlip :lines="allData.data" :ticketID="inputValue"></ModalSlip>
          </div>
        </div>
      </div>
      <div class="col-md-4">
        <h3 class="titles">{{ sections.web }}</h3>
      </div>
      <div class="col-md-4">
        <h3 class="titles">{{ sections.favorites }}</h3>
      </div>
    </div>
    <!-- <p class="red" v-if="isTrue">Uneli ste pogresan ID tiketa!</p> -->
    <div class="row">
      <div class="col-md-4">
        <div class="filters">
          <ButtonDropdown
            :options="statusOptions"
            @changeOption="statusHandler"
            :selected="this.allData.status.retail"
            id="retail-status"
          ></ButtonDropdown>
          <ButtonDropdown
            :options="statusObradeOptions"
            @changeOption="statusObradeHandler"
            :selected="this.allData.statusObrade.retail"
            id="retail-status-obrade"
          ></ButtonDropdown>
          <TextFilter
            v-model="minUlogRetail"
            placeholder="Min.Ulog"
            @changeValue="minUlogHandler('retail', $event)"
          ></TextFilter>
          <TextFilter
            placeholder="Ulog"
            @changeValue="ulogHandler('retail', $event)"
          ></TextFilter>
          <TextFilter
            placeholder="Kvota"
            @changeValue="kvotaHandler('retail', $event)"
          ></TextFilter>
        </div>
        <SlipPreview
          :data="allData.retailData"
          :title="sections.retail"
        ></SlipPreview>
      </div>
      <div class="col-md-4">
        <div class="filters">
          <ButtonDropdown
            :options="statusOptions"
            @changeOption="statusHandler"
            :selected="this.allData.status.web"
            id="web-status"
          ></ButtonDropdown>
          <ButtonDropdown
            :options="statusObradeOptions"
            @changeOption="statusObradeHandler"
            :selected="this.allData.statusObrade.web"
            id="web-status-obrade"
          ></ButtonDropdown>
          <TextFilter
            v-model="minUlogWeb"
            placeholder="Min.Ulog"
            @changeValue="minUlogHandler('web', $event)"
          ></TextFilter>
          <TextFilter
            placeholder="Ulog"
            @changeValue="ulogHandler('web', $event)"
          ></TextFilter>
          <TextFilter
            placeholder="Kvota"
            @changeValue="kvotaHandler('web', $event)"
          ></TextFilter>
        </div>
        <SlipPreview
          :data="allData.webData"
          :title="sections.web"
        ></SlipPreview>
      </div>
      <div class="col-md-4">
        <div class="filters">
          <ButtonDropdown
            :options="statusOptions"
            @changeOption="statusHandler"
            :selected="this.allData.status.favorites"
            id="favorites-status"
          ></ButtonDropdown>
          <ButtonDropdown
            :options="statusObradeOptions"
            @changeOption="statusObradeHandler"
            :selected="this.allData.statusObrade.favorites"
            id="favorites-status-obrade"
          ></ButtonDropdown>
          <TextFilter
            v-model="minUlogFavorites"
            placeholder="Min.Ulog"
            @changeValue="minUlogHandler('favorites', $event)"
          ></TextFilter>
          <TextFilter
            placeholder="Ulog"
            @changeValue="ulogHandler('favorites', $event)"
          ></TextFilter>
          <TextFilter
            placeholder="Kvota"
            @changeValue="kvotaHandler('favorites', $event)"
          ></TextFilter>
        </div>
        <SlipPreview
          :data="allData.favoritesData"
          :title="sections.favorites"
        ></SlipPreview>
      </div>
    </div>
  </div>
</template>
<script>
import ButtonDropdown from "@/ui/ButtonDropdown.vue";
import TextFilter from "@/ui/TextFilter.vue";
import SlipPreview from "@/components/SlipPreview.vue";
import ModalSlip from "@/ui/ModalSlip.vue";

export default {
  name: "App",
  components: {
    SlipPreview,
    TextFilter,
    ModalSlip,
    ButtonDropdown,
  },
  data() {
    return {
      sections: {
        retail: "Retail",
        web: "Web",
        favorites: "Favorite",
      },
      statusOptions: [
        {
          id: 0,
          title: "Otvoren",
        },
        {
          id: 1,
          title: "Isplaćeni",
        },
        {
          id: 2,
          title: "Storniran",
        },
        {
          id: 3,
          title: "Odbijen",
        },
      ],
      statusObradeOptions: [
        {
          id: -1,
          title: "Gubitan",
        },
        {
          id: 0,
          title: "Aktivan",
        },
        {
          id: 1,
          title: "Dobitan",
        },
      ],
      tiketHeaders: ["Domaćin", "Gost", "Tip", "Parametri", "Kvota"],
      inputValue: "",
      minUlogRetail: "",
      minUlogWeb: "",
      minUlogFavorites: "",
      isTrue: false,
      modalTarget: "#exampleModalCenter",
    };
  },
  created() {
    let localStorageData = JSON.parse(localStorage.getItem("filters"));
    if (localStorageData) {
      this.updateLocalStorageStatus(
        "retail-status",
        localStorageData.retailStatus
      );
      this.updateLocalStorageStatusObrade(
        "retail-status-obrade",
        localStorageData.retailStatusObrade
      );
      this.updateLocalStorageStatus("web-status", localStorageData.webStatus);
      this.updateLocalStorageStatusObrade(
        "web-status-obrade",
        localStorageData.webStatusObrade
      );
      this.updateLocalStorageStatus(
        "favorites-status",
        localStorageData.favoritesStatus
      );
      this.updateLocalStorageStatusObrade(
        "favorites-status-obrade",
        localStorageData.favoritesStatusObrade
      );
    }

    let obj = {
      retailStatus: this.allData.status.retail,
      retailStatusObrade: this.allData.statusObrade.retail,
      webStatus: this.allData.status.web,
      webStatusObrade: this.allData.statusObrade.web,
      favoritesStatus: this.allData.status.favorites,
      favoritesStatusObrade: this.allData.statusObrade.favorites,
    };

    localStorage.setItem("filters", JSON.stringify(obj));
    console.log(obj);
  },
  methods: {
    updateLocalStorageStatus(id, option) {
      this.$store.commit("updateStatus", { id, option });
    },
    updateLocalStorageStatusObrade(id, option) {
      this.$store.commit("updateStatusObrade", { id, option });
    },

    minUlogHandler(type, value) {
      if (type === "retail") {
        this.$store.commit("updateMinUlogRetail", value);
      } else if (type === "web") {
        this.$store.commit("updateMinUlogWeb", value);
      } else if (type === "favorites") {
        this.$store.commit("updateMinUlogFavorites", value);
      }
    },
    ulogHandler(type, value) {
      if (type === "retail") {
        this.$store.commit("updateUlogRetail", value);
      } else if (type === "web") {
        this.$store.commit("updateUlogWeb", value);
      } else if (type === "favorites") {
        this.$store.commit("updateUlogFavorites", value);
      }
    },
    kvotaHandler(type, value) {
      if (type === "retail") {
        this.$store.commit("updateKvotaRetail", value);
      } else if (type === "web") {
        this.$store.commit("updateKvotaWeb", value);
      } else if (type === "favorites") {
        this.$store.commit("updateKvotaFavorites", value);
      }
    },
    statusHandler(value) {
      let localStorageData = JSON.parse(localStorage.getItem("filters"));
      console.log(value);
      switch (value.id) {
        case "retail-status":
          localStorageData.retailStatus = value.option;
          break;
        case "web-status":
          localStorageData.webStatus = value.option;
          break;
        case "favorites-status":
          localStorageData.favoritesStatus = value.option;
          break;
        default:
          break;
      }
      localStorage.setItem("filters", JSON.stringify(localStorageData));
      this.$store.commit("updateStatus", value);
      console.log("AAA");
    },
    statusObradeHandler(value) {
      let localStorageData = JSON.parse(localStorage.getItem("filters"));
      switch (value.id) {
        case "retail-status-obrade":
          localStorageData.retailStatusObrade = value.option;
          break;
        case "web-status-obrade":
          localStorageData.webStatusObrade = value.option;
          break;
        case "favorites-status-obrade":
          localStorageData.favoritesStatusObrade = value.option;
          break;
        default:
          break;
      }
      localStorage.setItem("filters", JSON.stringify(localStorageData));
      this.$store.commit("updateStatusObrade", value);
      console.log("AAA");
    },

    loadTicketOnEnter() {
      const button = document.getElementById("ucitajID");
      const ticket = this.allData.data.find(
        (ticket) => ticket.code === this.inputValue
      );
      if (ticket) {
        button.click();
        this.$store.commit("updateTicketId", this.inputValue);
      } else {
        alert("Uneli ste pogrešan ID tiketa! Pokušajte ponovo.");
      }
    },
    loadTicketOnButton() {
      const ticket = this.allData.data.find(
        (ticket) => ticket.code === this.inputValue
      );
      if (ticket) {
        this.$store.commit("updateTicketData", this.inputValue);
      } else {
        alert("Uneli ste pogrešan ID tiketa! Pokušajte ponovo.");
      }
    },
  },
  computed: {
    allData() {
      return this.$store.getters.getAllData;
    },
    sortedData() {
      return this.data
        .slice()
        .sort((a, b) => new Date(a.date) - new Date(b.date));
    },
    getFilteredDataRetail() {
      if (this.minUlogRetail === "") {
        return this.getRetailData;
      } else {
        return this.getRetailData.filter(
          (item) => item.amount >= parseFloat(this.minUlogRetail)
        );
      }
    },
  },
};
</script>
<style scoped>
@import url("https://fonts.googleapis.com/css2?family=Fira+Sans&display=swap");

.titles {
  /* font-family: "Fira Sans", sans-serif; */

  color: rgb(248, 241, 241);
  /* font-style: italic; */
}
input {
  padding-right: 0px;
  /* width: 400px; */
  font-style: italic;
  text-align: center;
}
button {
  margin-left: 2%;
  border-radius: 5px;
  border-color: grey;
}
.filters {
  display: flex;
  justify-content: space-around;
  margin-bottom: 20px;
}
.slip-row {
  margin-bottom: 20px;
  padding-left: 12px;
}
.custom-input {
  box-shadow: 1px 2px 11px 0px;
  border-radius: 0px;
  border: 3px solid #5b3714;
}
.red-border {
  border: 2px solid red;
}
.red {
  color: red;
}
</style>
