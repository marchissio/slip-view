import Vue from "vue";
import Vuex from "vuex";
import { HubConnectionBuilder } from "@microsoft/signalr";

Vue.use(Vuex);

const ticketExists = (state, id) => {
  let exists = state.data.some((x) => x.Id == id);
  return exists;
};

const updateTicket = (state, ticket) => {
  state.data = state.data.filter((x) => x.Id != ticket.Id);
  state.data.unshift(ticket);
};

const addTicket = (state, ticket) => {
  state.data.unshift(ticket);
  if (state.data.length > 100) {
    state.data.pop();
  }
};

const moduleSlipPreview = {
  state: {
    web: {
      minUlog: "",
      ulog: "",
      kvota: "",
    },
    retail: {
      minUlog: "",
      ulog: "",
      kvota: "",
    },
    favorites: {
      minUlog: "",
      ulog: "",
      kvota: "",
    },
    ticketId: "",
    code: "",
    ticketStructure: "",
    status: {
      web: 0,
      retail: 0,
      favorites: 0,
    },
    statusObrade: {
      web: 0,
      retail: 0,
      favorites: 0,
    },
    data: [],
    signalR: null,
  },
  getters: {
    getAllData(state) {
      return {
        ticketId: state.ticketId,
        data: state.data,
        code: state.code,
        ticketStructure: state.ticketStructure,
        status: {
          web: state.status.web,
          retail: state.status.retail,
          favorites: state.status.favorites,
        },
        statusObrade: {
          web: state.statusObrade.web,
          retail: state.statusObrade.retail,
          favorites: state.statusObrade.favorites,
        },
        retailData: state.data.filter((x) => x.Source == 1),
        webData: state.data.filter((x) => x.Source == 2),
        favoritesData: state.data.filter((x) => x.IsFavourite == true),
        minUlogRetail: state.retail.minUlog,
        ulogRetail: state.retail.ulog,
        kvotaRetail: state.retail.kvota,
        minUlogWeb: state.web.minUlog,
        ulogWeb: state.web.ulog,
        kvotaWeb: state.web.kvota,
        minUlogFavorites: state.favorites.minUlog,
        ulogFavorites: state.favorites.ulog,
        kvotaFavorites: state.favorites.kvota,
      };
    },
    getSignalRInstance(state) {
      return state.signalR;
    },
    getStatus(state, section) {
      switch (section) {
        case "retail":
          return state.status.retail;
        case "web":
          return state.status.web;
        case "favorites":
          return state.status.favorites;
        default:
          break;
      }
    },
  },
  mutations: {
    updateMinUlogRetail(state, value) {
      state.retail.minUlog = value;
    },
    updateUlogRetail(state, value) {
      state.retail.ulog = value;
    },
    updateKvotaRetail(state, value) {
      state.retail.kvota = value;
    },
    updateMinUlogFavorites(state, value) {
      state.favorites.minUlog = value;
    },
    updateKvotaFavorites(state, value) {
      state.favorites.kvota = value;
    },
    updateUlogFavorites(state, value) {
      state.favorites.ulog = value;
    },
    updateMinUlogWeb(state, value) {
      state.web.minUlog = value;
    },
    updateUlogWeb(state, value) {
      state.web.ulog = value;
    },
    updateKvotaWeb(state, value) {
      state.web.kvota = value;
    },
    updateTicketData(state, value) {
      state.code = value.Code;
      state.ticketId = value.Id;
      state.ticketStructure = value.TicketStructure;
    },
    updateStatus(state, value) {
      console.log(value);
      switch (value.id) {
        case "retail-status":
          state.status.retail = value.option;
          break;
        case "web-status":
          state.status.web = value.option;
          break;
        case "favorites-status":
          state.status.favorites = value.option;
          break;
        default:
          break;
      }
    },
    updateStatusObrade(state, value) {
      switch (value.id) {
        case "retail-status-obrade":
          state.statusObrade.retail = value.option;
          break;
        case "web-status-obrade":
          state.statusObrade.web = value.option;
          break;
        case "favorites-status-obrade":
          state.statusObrade.favorites = value.option;
          break;
        default:
          break;
      }
    },

    setSignalR(state, connection) {
      state.signalR = connection;
    },
    setData(state, value) {
      console.log(value);
      state.data = state.data
        .concat(value)
        .sort((a, b) => new Date(b.PayinTime) - new Date(a.PayinTime));
    },
    updateData(state, value) {
      if (ticketExists(state, value.Id)) {
        updateTicket(state, value);
      } else {
        addTicket(state, value);
      }
    },
  },
  actions: {
    connectToSingalR({ commit }) {
      return new Promise((resolve, reject) => {
        try {
          const connection = new HubConnectionBuilder()
            .withUrl("http://localhost:5066/hubs/tickets")
            .build();
          commit("setSignalR", connection);
          resolve(true);
        } catch (e) {
          reject(false);
        }
      });
    },
  },
};

export default new Vuex.Store({
  modules: {
    moduleSlipPreview,
  },
});
