<template>
  <div class="table-container">
    <table class="table custom-table">
      <thead>
        <tr>
          <th v-for="header in getHeaders" :key="header">
            {{ header }}
          </th>
        </tr>
      </thead>
      <tbody>
        <template v-for="item in data">
          <tr v-if="filterRows(item)" :key="item.Id">
            <td
              :class="setBackgroundForType(item.Type)"
              style="width: 65px !important"
            >
              {{ getType(item.Type) }}
              <span style="font-size: 10px">{{ isSystem(item) }}</span>
            </td>
            <td class="date">{{ formatDate(item.PayinTime) }}</td>
            <td v-if="title == 'Retail'" class="elipsis" :title="item.Location">
              {{ item.Location }}
            </td>
            <td v-else class="elipsis" :title="item.UserName">
              {{ item.UserName }}
            </td>
            <td :class="setBackgroundForPayin(item.Payin)">
              {{ convertToCurrency(item.Payin) }}
            </td>
            <td
              :class="
                setBackgroundForOdds(item.TicketLines.map((x) => x.OddValue))
              "
            >
              {{ convertToCurrency(item.MaxTotalOdd) }}
            </td>
            <td>{{ item.TicketLines.length }}</td>

            <td>
              <button
                type="button"
                id="ticketLoad"
                @click="loadTicket(item)"
                class="btn btn-outline-dark custom-button"
                data-bs-toggle="modal"
                data-bs-target="#exampleModalCenter"
              >
                Učitaj
              </button>
            </td>
          </tr>
        </template>
      </tbody>
    </table>
  </div>
</template>
<script>
import { formatDate } from "@/helpers/date-functions";
import { convertToCurrency } from "@/helpers/number-functions";
// import { SlipType } from "@/enums/slip-type";
import { Source } from "@/enums/source";

export default {
  components: {},

  props: ["data", "title"],
  data() {
    return {};
  },
  methods: {
    getType(type) {
      switch (type) {
        case 1:
          return "Prematch";
        case 2:
          return "Live";
        case 3:
          return "Mix";
        default:
          return "Unknown";
      }
    },
    filterRows(item) {
      if (this.title == "Web") {
        if (
          this.checkStatus(item, "web") &&
          this.checkStatusObrade(item, "web")
        ) {
          if (this.allData.minUlogWeb === "") return true;
          return (
            item.Payin >= this.allData.minUlogWeb &&
            this.checkStatus(item, "web") &&
            this.checkStatusObrade(item, "web")
          );
        } else {
          return false;
        }
      } else if (this.title == "Retail") {
        if (
          this.checkStatus(item, "retail") &&
          this.checkStatusObrade(item, "retail")
        ) {
          if (this.allData.minUlogRetail === "") return true;
          return (
            item.Payin >= this.allData.minUlogRetail &&
            this.checkStatus(item, "retail") &&
            this.checkStatusObrade(item, "retail")
          );
        }
      } else if (this.title == "Favorite") {
        if (
          this.checkStatus(item, "favorites") &&
          this.checkStatusObrade(item, "favorites")
        ) {
          if (this.allData.minUlogFavorites === "") return true;
          return (
            item.Payin >= this.allData.minUlogFavorites &&
            this.checkStatus(item, "favorites") &&
            this.checkStatusObrade(item, "favorites")
          );
        }
      }
    },
    checkStatus(item, type) {
      let selected;
      if (type === "web") {
        selected = this.allData.status.web;
      } else if (type === "retail") {
        selected = this.allData.status.retail;
      } else if (type === "favorites") {
        selected = this.allData.status.favorites;
      }
      let status = item.Status;

      return status == selected;
    },
    checkStatusObrade(item, type) {
      let selected;
      if (type === "web") {
        selected = this.allData.statusObrade.web;
      } else if (type === "retail") {
        selected = this.allData.statusObrade.retail;
      } else if (type === "favorites") {
        selected = this.allData.statusObrade.favorites;
      }
      let status = item.PStatus;

      return status == selected;
    },
    formatDate,
    convertToCurrency,
    tipBackground(tip) {
      if (tip == "mix") {
        return "bg-red";
      } else {
        return "";
      }
    },
    loadTicket(value) {
      this.$store.commit("updateTicketData", value);
    },
    setBackgroundForType(type) {
      return type == 3 ? "bg-red" : "";
    },
    setBackgroundForPayin(payin) {
      if (this.title == "Web") {
        if (this.allData.ulogWeb === "") return "";
        return payin >= this.allData.ulogWeb ? "bg-red" : "";
      } else if (this.title == "Retail") {
        if (this.allData.ulogRetail === "") return "";
        return payin >= this.allData.ulogRetail ? "bg-red" : "";
      } else if (this.title == "Favorite") {
        if (this.allData.ulogFavorites === "") return "";
        return payin >= this.allData.ulogFavorites ? "bg-red" : "";
      }
    },
    setBackgroundForOdds(odds) {
      if (this.title == "Web") {
        if (this.allData.kvotaWeb === "") return "";
        return odds.some((odd) => this.checkOdd(odd, Source.WEB)) ||
          this.allData.kvotaWeb === ""
          ? "bg-blue"
          : "";
      } else if (this.title == "Retail") {
        if (this.allData.kvotaRetail === "") return "";
        return odds.some((odd) => this.checkOdd(odd, Source.RETAIL)) ||
          this.allData.kvotaRetail === ""
          ? "bg-blue"
          : "";
      } else if (this.title == "Favorite") {
        if (this.allData.kvotaFavorites === "") return "";
        return odds.some((odd) => this.checkOdd(odd, Source.FAVORITES)) ||
          this.allData.kvotaFavorites === ""
          ? "bg-blue"
          : "";
      }
    },
    checkOdd(odd, type) {
      let kvota;
      if (type === Source.RETAIL) {
        kvota = parseFloat(this.allData.kvotaRetail);
      } else if (type === Source.WEB) {
        kvota = parseFloat(this.allData.kvotaWeb);
      } else if (type === Source.FAVORITES) {
        kvota = parseFloat(this.allData.kvotaFavorites);
      }

      return odd >= kvota;
    },
    checkOddRetail(odd) {
      return odd >= parseFloat(this.allData.kvotaRetail);
    },
    checkOddRetailWeb(odd) {
      return odd >= parseFloat(this.allData.kvotaWeb);
    },
    isSystem(ticket) {
      let ticketStructure = ticket.TicketStructure.slice(0, -1).split(";");
      let firstDigit = ticketStructure[0].split("/")[0];
      let secondDigit = ticketStructure[0].split("/")[1];
      return ticketStructure.length > 1 || firstDigit != secondDigit
        ? "(s)"
        : "";
    },
  },
  computed: {
    getHeaders() {
      let headers = [];
      if (this.title == "Retail") {
        headers = [
          "Šifra",
          "Vreme",
          "Upl. mesto",
          "Ulog",
          "Uk. kvota",
          "Br. p",
          "Detalji",
        ];
      } else {
        headers = [
          "Šifra",
          "Vreme",
          "Username",
          "Ulog",
          "Uk. kvota",
          "Br. p",
          "Detalji",
        ];
      }
      return headers;
    },
    allData() {
      return this.$store.getters.getAllData;
    },
  },
};
</script>
<style scoped>
.elipsis {
  white-space: nowrap;
  text-overflow: ellipsis;
  overflow: hidden;
  max-width: 70px !important;
}
.table-container {
  background-image: url("../assets/img/slipbg.jpg");
  background-repeat: no-repeat;
  background-size: cover;
  padding-top: 3px !important;
}
.bg-red {
  background-color: #d86981 !important;
  /* rgb(253, 70, 70); */
}

.bg-blue {
  background-color: rgb(33, 182, 241) !important;
}

.custom-table {
  padding: 0px !important;
}

.custom-table td,
.custom-table th {
  color: black !important;
  border-color: black !important;
}

.date {
  width: 20%;
}

td {
  height: 10px;
}

table {
  font-size: 14px;
}

table tr td,
table tr th {
  background-color: rgba(244, 240, 246, 0.1) !important;
  width: 10px;
}
#ticketLoad {
  height: 25px;
  display: flex;
  justify-content: center;
  align-items: center;
  font-size: 14px;
}
/* scrollbar */
</style>
