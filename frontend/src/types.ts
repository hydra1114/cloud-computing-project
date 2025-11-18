export interface Item {
  id: number;
  name: string;
  price: number;
  description?: string;
  sku?: string;
  createdAt: string;
  updatedAt: string;
  itemLocations?: ItemLocation[];
}

export interface Location {
  id: number;
  name: string;
  description?: string;
  address?: string;
  locationType: string;
  parentLocationId?: number;
  createdAt: string;
  updatedAt: string;
  parentLocation?: Location;
  childLocations?: Location[];
  itemLocations?: ItemLocation[];
}

export interface ItemLocation {
  id: number;
  itemId: number;
  locationId: number;
  quantity: number;
  createdAt: string;
  updatedAt: string;
  item?: Item;
  location?: Location;
}

export interface CreateItemDto {
  name: string;
  price: number;
  description?: string;
  sku?: string;
}

export interface CreateLocationDto {
  name: string;
  description?: string;
  address?: string;
  locationType: string;
  parentLocationId?: number;
}

export interface CreateItemLocationDto {
  itemId: number;
  locationId: number;
  quantity: number;
}
