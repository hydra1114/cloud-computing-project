import React, { useState, useEffect } from 'react';
import { itemsApi, locationsApi, itemLocationsApi } from '../api';
import { Item, Location, ItemLocation, CreateItemLocationDto } from '../types';

export const Inventory: React.FC = () => {
  const [items, setItems] = useState<Item[]>([]);
  const [locations, setLocations] = useState<Location[]>([]);
  const [itemLocations, setItemLocations] = useState<ItemLocation[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [editingItemLocation, setEditingItemLocation] = useState<ItemLocation | null>(null);
  const [formData, setFormData] = useState<CreateItemLocationDto>({
    itemId: 0,
    locationId: 0,
    quantity: 0,
  });
  const [filterLocation, setFilterLocation] = useState<number | null>(null);
  const [filterItem, setFilterItem] = useState<number | null>(null);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      const [itemsRes, locationsRes, itemLocationsRes] = await Promise.all([
        itemsApi.getAll(),
        locationsApi.getAll(),
        itemLocationsApi.getAll(),
      ]);
      setItems(itemsRes.data);
      setLocations(locationsRes.data);
      setItemLocations(itemLocationsRes.data);
      setError(null);
    } catch (err) {
      setError('Failed to load data');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (editingItemLocation) {
        await itemLocationsApi.update(editingItemLocation.id, {
          ...editingItemLocation,
          quantity: formData.quantity,
        });
      } else {
        await itemLocationsApi.create(formData);
      }
      setShowForm(false);
      setEditingItemLocation(null);
      setFormData({ itemId: 0, locationId: 0, quantity: 0 });
      loadData();
    } catch (err: any) {
      if (err.response?.data) {
        setError(err.response.data);
      } else {
        setError('Failed to save inventory record');
      }
      console.error(err);
    }
  };

  const handleEdit = (itemLocation: ItemLocation) => {
    setEditingItemLocation(itemLocation);
    setFormData({
      itemId: itemLocation.itemId,
      locationId: itemLocation.locationId,
      quantity: itemLocation.quantity,
    });
    setShowForm(true);
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Are you sure you want to remove this item from the location?')) return;
    
    try {
      await itemLocationsApi.delete(id);
      loadData();
    } catch (err) {
      setError('Failed to delete inventory record');
      console.error(err);
    }
  };

  const handleCancel = () => {
    setShowForm(false);
    setEditingItemLocation(null);
    setFormData({ itemId: 0, locationId: 0, quantity: 0 });
  };

  const getFilteredItemLocations = () => {
    let filtered = itemLocations;
    if (filterLocation) {
      filtered = filtered.filter((il) => il.locationId === filterLocation);
    }
    if (filterItem) {
      filtered = filtered.filter((il) => il.itemId === filterItem);
    }
    return filtered;
  };

  const getLocationPath = (locationId: number): string => {
    const location = locations.find((l) => l.id === locationId);
    if (!location) return 'Unknown';
    if (!location.parentLocationId) return location.name;
    const parent = locations.find((l) => l.id === location.parentLocationId);
    if (!parent) return location.name;
    return `${parent.name} > ${location.name}`;
  };

  if (loading) return <div className="loading">Loading inventory...</div>;

  const filteredItemLocations = getFilteredItemLocations();

  return (
    <div className="container">
      <div className="page-header">
        <h2>Inventory Management</h2>
        <button
          className="btn-primary"
          onClick={() => setShowForm(true)}
          disabled={items.length === 0 || locations.length === 0}
        >
          Add Item to Location
        </button>
      </div>

      {error && <div className="error">{error}</div>}

      {items.length === 0 || locations.length === 0 ? (
        <div className="empty-state">
          <h3>No items or locations available</h3>
          <p>Please create items and locations before managing inventory</p>
        </div>
      ) : (
        <>
          {showForm && (
            <div className="card">
              <h3>{editingItemLocation ? 'Edit Inventory' : 'Add Item to Location'}</h3>
              <form onSubmit={handleSubmit}>
                <div className="form-group">
                  <label>Item *</label>
                  <select
                    value={formData.itemId}
                    onChange={(e) => setFormData({ ...formData, itemId: parseInt(e.target.value) })}
                    required
                    disabled={!!editingItemLocation}
                  >
                    <option value="0">Select an item</option>
                    {items.map((item) => (
                      <option key={item.id} value={item.id}>
                        {item.name} - ${item.price.toFixed(2)}
                      </option>
                    ))}
                  </select>
                </div>
                <div className="form-group">
                  <label>Location *</label>
                  <select
                    value={formData.locationId}
                    onChange={(e) => setFormData({ ...formData, locationId: parseInt(e.target.value) })}
                    required
                    disabled={!!editingItemLocation}
                  >
                    <option value="0">Select a location</option>
                    {locations.map((location) => (
                      <option key={location.id} value={location.id}>
                        {getLocationPath(location.id)} ({location.locationType})
                      </option>
                    ))}
                  </select>
                </div>
                <div className="form-group">
                  <label>Quantity *</label>
                  <input
                    type="number"
                    min="0"
                    value={formData.quantity}
                    onChange={(e) => setFormData({ ...formData, quantity: parseInt(e.target.value) })}
                    required
                  />
                </div>
                <div className="btn-group">
                  <button type="submit" className="btn-primary">
                    {editingItemLocation ? 'Update' : 'Add'}
                  </button>
                  <button type="button" className="btn-secondary" onClick={handleCancel}>
                    Cancel
                  </button>
                </div>
              </form>
            </div>
          )}

          <div className="card">
            <h3>Filters</h3>
            <div className="grid grid-2">
              <div className="form-group">
                <label>Filter by Location</label>
                <select
                  value={filterLocation || ''}
                  onChange={(e) => setFilterLocation(e.target.value ? parseInt(e.target.value) : null)}
                >
                  <option value="">All Locations</option>
                  {locations.map((location) => (
                    <option key={location.id} value={location.id}>
                      {getLocationPath(location.id)}
                    </option>
                  ))}
                </select>
              </div>
              <div className="form-group">
                <label>Filter by Item</label>
                <select
                  value={filterItem || ''}
                  onChange={(e) => setFilterItem(e.target.value ? parseInt(e.target.value) : null)}
                >
                  <option value="">All Items</option>
                  {items.map((item) => (
                    <option key={item.id} value={item.id}>
                      {item.name}
                    </option>
                  ))}
                </select>
              </div>
            </div>
          </div>

          {filteredItemLocations.length === 0 ? (
            <div className="empty-state">
              <h3>No inventory records found</h3>
              <p>Add items to locations to start tracking your inventory</p>
            </div>
          ) : (
            <div className="card">
              <h3>Inventory ({filteredItemLocations.length} records)</h3>
              <table>
                <thead>
                  <tr>
                    <th>Item</th>
                    <th>SKU</th>
                    <th>Price</th>
                    <th>Location</th>
                    <th>Location Type</th>
                    <th>Quantity</th>
                    <th>Total Value</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredItemLocations.map((il) => {
                    const item = items.find((i) => i.id === il.itemId);
                    const location = locations.find((l) => l.id === il.locationId);
                    if (!item || !location) return null;
                    const totalValue = item.price * il.quantity;
                    return (
                      <tr key={il.id}>
                        <td>{item.name}</td>
                        <td>{item.sku || '-'}</td>
                        <td>${item.price.toFixed(2)}</td>
                        <td>{getLocationPath(location.id)}</td>
                        <td>
                          <span className="badge badge-info">{location.locationType}</span>
                        </td>
                        <td>{il.quantity}</td>
                        <td>${totalValue.toFixed(2)}</td>
                        <td>
                          <div className="btn-group">
                            <button
                              className="btn-secondary"
                              onClick={() => handleEdit(il)}
                            >
                              Edit
                            </button>
                            <button
                              className="btn-danger"
                              onClick={() => handleDelete(il.id)}
                            >
                              Remove
                            </button>
                          </div>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          )}
        </>
      )}
    </div>
  );
};
