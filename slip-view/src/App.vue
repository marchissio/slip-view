<template>
  <div id="app">
    <MainView></MainView>
  </div>
</template>

<script>
// import SlipPreview from "./components/SlipPreview.vue";

import MainView from "@/views/MainView.vue";
import { SlipType } from "./enums/slip-type";
export default {
  name: "App",
  components: {
    MainView,
  },
  created() {
    this.$store.dispatch("connectToSingalR").then((response) => {
      if (response) {
        this.start();
        this.registerEvents();
      } else {
        alert("GRESKA PRI POVEZIVANJU NA SIGNALR");
      }
    });
  },
  computed: {
    signalRInstance() {
      return this.$store.getters.getSignalRInstance;
    },
  },
  methods: {
    start() {
      this.signalRInstance
        .start()
        .then(() => {
          console.log("SignalR Connected.");
          this.subscribeOnTickets(SlipType.LIVE);
          this.subscribeOnTickets(SlipType.MIX);
        })
        .catch((err) => {
          console.log(err);
          setTimeout(this.start(), 5000);
        });
    },
    registerEvents() {
      this.signalRInstance.onclose(() => {
        this.start();
      });
      this.signalRInstance.on("TicketChange", (message) => {
        let slip = JSON.parse(message);
        // this.setTicketType([slip]);
        this.transformLines([slip]);
        this.$store.commit("updateData", slip);
      });

      this.signalRInstance.on("AllTicketsForType", (message) => {
        let slips = JSON.parse(message);

        this.transformLines(slips);
        this.$store.commit("setData", slips);
      });
    },
    subscribeOnTickets(type) {
      console.log("subscribeOnTickets pressed");
      return this.signalRInstance
        .invoke("SubscribeOnTickets", type)
        .catch((err) => {
          console.error(err);
        });
    },
    unsubscribeFromTickets(type) {
      console.log("unsubscribeFromTickets pressed");
      return this.signalRInstance
        .invoke("UnsubscribeFromTickets", type)
        .catch((err) => {
          console.error(err);
        });
    },

    transformLines(slips) {
      for (let i = 0; i < slips.length; i++) {
        let slip = slips[i];
        let ticketStructuresArr = this.getTicketStructures(slip); // [{TicketSystem:1, Title:"4/4"},{TicketSystem:2, Title:"4/3"}]
        slip.TicketLinesNew = this.addLines(
          slip.TicketLines,
          ticketStructuresArr
        );
      }
    },
    getTicketStructures(slip) {
      let ticketStructuresArr = slip.TicketStructure.slice(0, -1).split(";");
      let finalArr = [];
      for (let i = 0; i < ticketStructuresArr.length; i++) {
        finalArr.push({
          TicketSystem: i + 1,
          Title: ticketStructuresArr[i],
          Lines: [],
        });
      }
      return finalArr;
    },
    addLines(slipLines, ticketStructureArr) {
      for (let i = 0; i < slipLines.length; i++) {
        for (let j = 0; j < ticketStructureArr.length; j++) {
          if (slipLines[i].TicketSystem == ticketStructureArr[j].TicketSystem) {
            ticketStructureArr[j].Lines.push(slipLines[i]);
          }
        }
      }
      return ticketStructureArr;
    },
  },
};
</script>

<style>
@import "../src/assets/style.css";

body {
  /* background: url("https://wallpapers.com/images/hd/aesthetically-pleasing-royal-wallpaper-por6uarznxxhtub7.jpg"); */
  background: url("./assets/img/abstract-luxury-gradient-blue-background-smooth-dark-blue-with-black-vignette-studio-banner.jpg");
  background-repeat: no-repeat;
  background-size: cover;
  background-position-y: -21px;
}
</style>
