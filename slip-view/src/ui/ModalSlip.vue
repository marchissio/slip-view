<template>
  <div
    class="modal fade"
    id="exampleModalCenter"
    tabindex="-1"
    role="dialog"
    aria-labelledby="exampleModalCenterTitle"
    aria-hidden="true"
  >
    <div class="modal-dialog modal-lg modal-dialog-centered" role="document">
      <div class="modal-content" style="border: 2px solid #b3671e">
        <div class="modal-header">
          <h5 class="modal-title" id="exampleModalLongTitle">Pregled tiketa</h5>
          <button
            type="button"
            class="close"
            data-bs-dismiss="modal"
            aria-label="Close"
            @click="closeModal()"
          >
            <span aria-hidden="true">&times;</span>
          </button>
        </div>
        <div class="modal-body">
          <div class="tiket-info">
            <p>
              ID:
              <button @click="copyCode(allData.code)" :class="getClass()">
                {{ allData.code }}
              </button>
            </p>
            <p v-if="ticketData().Location">
              Upl. mesto: {{ ticketData().Location }}
            </p>
          </div>
          <p>
            Ulog: {{ convertToCurrency(ticketData().Payin) }}
            <span>{{ ticketData().Currency }}</span>
          </p>
          <p>Vreme: {{ formatDateWithDay(ticketData().PayinTime) }}</p>
          <table class="table table-bordered">
            <thead>
              <tr>
                <th scope="col">Match ID</th>
                <th scope="col">Domacin</th>
                <th scope="col">Gost</th>
                <th scope="col">Tip</th>
                <th scope="col">Kvota</th>
                <th scope="col">Parametar</th>
              </tr>
            </thead>
            <tbody>
              <template v-for="item in ticketData().TicketLinesNew">
                <template>
                  <div class="sistem-row" :key="item.id">
                    {{ getBasicTicketStructure(item.Title) }}
                  </div>
                </template>
                <template v-for="line in item.Lines">
                  <tr
                    v-bind:key="line.MatchCode"
                    :class="checkLineStatus(line)"
                  >
                    <td>{{ line.MatchCode }}</td>
                    <td>{{ line.Home }}</td>
                    <td>{{ line.Away }}</td>
                    <td>{{ line.BetTypeShortName }}</td>
                    <td>{{ convertToCurrency(line.OddValue) }}</td>
                    <td>{{ line.SpecialValue }}</td>
                  </tr>
                </template>
              </template>
            </tbody>
          </table>
        </div>
        <div class="modal-footer">
          <button
            type="button"
            class="btn btn-secondary"
            data-bs-dismiss="modal"
          >
            Zatvori
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
<script>
import { formatDate, formatDateWithDay } from "@/helpers/date-functions";
import { convertToCurrency } from "@/helpers/number-functions";
export default {
  props: ["lines", "ticketID"],
  data() {
    return {
      clicked: false,
    };
  },
  methods: {
    formatDate,
    formatDateWithDay,
    convertToCurrency,
    copyCode(code) {
      navigator.clipboard.writeText(code);
      this.$store.commit("updateNotificationsData", {
        message: "Kod meča " + code + " je kopiran",
        bgColor: "rgba(0,255,255.0.7)",
        duration: 3000,
      });
      this.clicked = true;
    },
    getClass() {
      if (this.clicked) {
        return "btn btn-secondary";
      } else {
        return "btn btn-primary";
      }
    },
    closeModal() {
      this.clicked = false;
    },
    getBasicTicketStructure(ticketStructure) {
      let firstDigit = "",
        secondDigit;
      if (ticketStructure.split("/").length > 2) {
        let parts = ticketStructure.split("/");
        for (let i = 1; i < parts.length; i++) {
          firstDigit += parts[i] + ",";
        }
        secondDigit = parts[0];
        ticketStructure = firstDigit.slice(0, -1) + "/" + secondDigit;
      } else {
        firstDigit = ticketStructure.split("/")[0];
        secondDigit = ticketStructure.split("/")[1];
        ticketStructure = secondDigit + "/" + firstDigit;
      }
      let label = firstDigit == secondDigit ? "Fiks: " : "Sistem: ";
      return label + ticketStructure;
    },
    checkLineStatus(line) {
      switch (line.OddStatus) {
        case -1:
          return "table-danger";
        case 1:
          return "table-success";
        case 0:
          return "table-active";
        default:
          return "table-warning";
      }
    },
    ticketData() {
      const ticket = this.allData.data.find(
        (ticket) => ticket.Id === this.allData.ticketId
      );
      return ticket
        ? {
            TicketLinesNew: ticket.TicketLinesNew,
            PayinTime: ticket.PayinTime,
            Payin: ticket.Payin,
            Location: ticket.Location,
            Currency: ticket.Currency,
          }
        : {};
    },
  },
  computed: {
    allData() {
      return this.$store.getters.getAllData;
    },
  },
};
</script>
<style scoped>
.tiket-info {
  display: flex;
  justify-content: space-between;
}
.sistem-row {
  background-color: rgb(247, 236, 221);
  width: 100% !important;
  padding-left: 1px;
}
tr {
  border: 1px solid black !important;
}
</style>
