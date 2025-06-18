import api from './index';

export const getTags = async () => {
  return await api.get('/tags');
};

export const getTag = async (id) => {
  return await api.get(`/tags/${id}`);
};

export const createTag = async (tag) => {
  return await api.post('/tags', tag);
};

export const updateTag = async (id, tag) => {
  return await api.put(`/tags/${id}`, tag);
};

export const deleteTag = async (id) => {
  return await api.delete(`/tags/${id}`);
};

export const updateClientTags = async (clientId, tagIds) => {
  return await api.put(`/tags/clients/${clientId}`, { TagIds: tagIds });
};