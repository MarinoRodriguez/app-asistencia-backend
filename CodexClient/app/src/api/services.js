import { apiRequest } from "./http";

export const personApi = {
  getAll: () => apiRequest("api/persons"),
  search: (term) => apiRequest(`api/persons/search?term=${encodeURIComponent(term)}`),
  getById: (id) => apiRequest(`api/persons/${id}`),
  create: (person) => apiRequest("api/persons", { method: "POST", body: JSON.stringify(person) }),
  update: (id, person) => apiRequest(`api/persons/${id}`, { method: "PUT", body: JSON.stringify(person) }),
  remove: (id) => apiRequest(`api/persons/${id}`, { method: "DELETE" }),
};

export const groupApi = {
  getAll: (includeInactive = true) => apiRequest(`api/groups?includeInactive=${includeInactive}`),
  getById: (id) => apiRequest(`api/groups/${id}`),
  create: (group) => apiRequest("api/groups", { method: "POST", body: JSON.stringify(group) }),
  update: (id, group) => apiRequest(`api/groups/${id}`, { method: "PUT", body: JSON.stringify(group) }),
  remove: (id) => apiRequest(`api/groups/${id}`, { method: "DELETE" }),
};

export const eventApi = {
  getAll: (state) => {
    const query = typeof state === "number" ? `?state=${state}` : "";
    return apiRequest(`api/events${query}`);
  },
  getById: (id) => apiRequest(`api/events/${id}`),
  create: (event) => apiRequest("api/events", { method: "POST", body: JSON.stringify(event) }),
  update: (id, event) => apiRequest(`api/events/${id}`, { method: "PUT", body: JSON.stringify(event) }),
  start: (id) => apiRequest(`api/events/${id}/start`, { method: "POST" }),
  finish: (id) => apiRequest(`api/events/${id}/finish`, { method: "POST" }),
  invitePerson: (eventId, personId) => apiRequest(`api/events/${eventId}/invite/person/${personId}`, { method: "POST" }),
  inviteGroup: (eventId, groupId) => apiRequest(`api/events/${eventId}/invite/group/${groupId}`, { method: "POST" }),
  removeInvitation: (eventId, personId) => apiRequest(`api/events/${eventId}/invite/person/${personId}`, { method: "DELETE" }),
};

export const attendanceApi = {
  getByEvent: (eventId) => apiRequest(`api/attendance/event/${eventId}`),
  getRoster: (eventId) => apiRequest(`api/attendance/event/${eventId}/roster`),
  mark: (eventId, personId, type) =>
    apiRequest("api/attendance/mark", {
      method: "POST",
      body: JSON.stringify({ eventId, personId, type }),
    }),
  registerExternal: (eventId, person) =>
    apiRequest(`api/attendance/external/${eventId}`, {
      method: "POST",
      body: JSON.stringify(person),
    }),
  remove: (eventId, personId) =>
    apiRequest(`api/attendance/event/${eventId}/person/${personId}`, { method: "DELETE" }),
};
