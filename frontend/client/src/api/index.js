import axios from 'axios';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || '/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response.data,
  (error) => {
    if (error.response) {
      const { data, status } = error.response;
      
      const errorData = {
        message: data.title || 'Request failed',
        details: data.detail || 'An error occurred',
        validationErrors: data.errors || {},
        status,
      };
      
      return Promise.reject(errorData);
    }
    
    if (error.request) {
      return Promise.reject({
        message: 'Network Error',
        details: 'Could not connect to the server',
        status: 0,
      });
    }
    
    return Promise.reject({
      message: 'Request Error',
      details: error.message,
      status: -1,
    });
  },
);

export default api;