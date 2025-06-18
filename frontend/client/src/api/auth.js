import api from './index';

export const login = async ({ email, password }) => {
  const response = await api.post('/auth/login', { email, password });
  return response;
};

export const refreshToken = async (token) => {
  const response = await api.post('/auth/refresh-token', { token });
  return response;
};