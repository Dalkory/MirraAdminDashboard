import api from './index';

export const getPayments = async (params = {}) => {
  return await api.get('/payments', { params });
};