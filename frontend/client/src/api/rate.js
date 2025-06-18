import api from './index';

export const getRate = async () => {
  return await api.get('/rate');
};

export const updateRate = async (value) => {
  return await api.post('/rate', { value });
};