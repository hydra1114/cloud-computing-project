import axios from 'axios';
import { Item, Location, ItemLocation, CreateItemDto, CreateLocationDto, CreateItemLocationDto } from './types';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Items API
export const itemsApi = {
  getAll: () => api.get<Item[]>('/items'),
  getById: (id: number) => api.get<Item>(`/items/${id}`),
  create: (item: CreateItemDto) => api.post<Item>('/items', item),
  update: (id: number, item: Item) => api.put(`/items/${id}`, item),
  delete: (id: number) => api.delete(`/items/${id}`),
};

// Locations API
export const locationsApi = {
  getAll: () => api.get<Location[]>('/locations'),
  getById: (id: number) => api.get<Location>(`/locations/${id}`),
  create: (location: CreateLocationDto) => api.post<Location>('/locations', location),
  update: (id: number, location: Location) => api.put(`/locations/${id}`, location),
  delete: (id: number) => api.delete(`/locations/${id}`),
};

// ItemLocations API
export const itemLocationsApi = {
  getAll: () => api.get<ItemLocation[]>('/itemlocations'),
  getById: (id: number) => api.get<ItemLocation>(`/itemlocations/${id}`),
  getByLocation: (locationId: number) => api.get<ItemLocation[]>(`/itemlocations/bylocation/${locationId}`),
  getByItem: (itemId: number) => api.get<ItemLocation[]>(`/itemlocations/byitem/${itemId}`),
  create: (itemLocation: CreateItemLocationDto) => api.post<ItemLocation>('/itemlocations', itemLocation),
  update: (id: number, itemLocation: ItemLocation) => api.put(`/itemlocations/${id}`, itemLocation),
  delete: (id: number) => api.delete(`/itemlocations/${id}`),
};
