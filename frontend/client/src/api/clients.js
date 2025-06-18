import api from './index';

export const getClients = async () => {
  return await api.get('/clients');
};

export const getClient = async (id) => {
  return await api.get(`/clients/${id}`);
};

export const createClient = async (client) => {
  return await api.post('/clients', client);
};

export const updateClient = async (id, client) => {
  return await api.put(`/clients/${id}`, client);
};

export const deleteClient = async (id) => {
  return await api.delete(`/clients/${id}`);
};