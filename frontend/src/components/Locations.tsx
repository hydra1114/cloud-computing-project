import React, { useState, useEffect } from 'react';
import { locationsApi } from '../api';
import { Location, CreateLocationDto } from '../types';

export const Locations: React.FC = () => {
  const [locations, setLocations] = useState<Location[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [editingLocation, setEditingLocation] = useState<Location | null>(null);
  const [formData, setFormData] = useState<CreateLocationDto>({
    name: '',
    description: '',
    address: '',
    locationType: 'General',
    parentLocationId: undefined,
  });

  useEffect(() => {
    loadLocations();
  }, []);

  const loadLocations = async () => {
    try {
      setLoading(true);
      const response = await locationsApi.getAll();
      setLocations(response.data);
      setError(null);
    } catch (err) {
      setError('Failed to load locations');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (editingLocation) {
        await locationsApi.update(editingLocation.id, {
          ...editingLocation,
          ...formData,
        });
      } else {
        await locationsApi.create(formData);
      }
      setShowForm(false);
      setEditingLocation(null);
      setFormData({
        name: '',
        description: '',
        address: '',
        locationType: 'General',
        parentLocationId: undefined,
      });
      loadLocations();
    } catch (err) {
      setError('Failed to save location');
      console.error(err);
    }
  };

  const handleEdit = (location: Location) => {
    setEditingLocation(location);
    setFormData({
      name: location.name,
      description: location.description || '',
      address: location.address || '',
      locationType: location.locationType,
      parentLocationId: location.parentLocationId,
    });
    setShowForm(true);
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Are you sure you want to delete this location?')) return;
    
    try {
      await locationsApi.delete(id);
      loadLocations();
    } catch (err: any) {
      if (err.response?.data) {
        setError(err.response.data);
      } else {
        setError('Failed to delete location');
      }
      console.error(err);
    }
  };

  const handleCancel = () => {
    setShowForm(false);
    setEditingLocation(null);
    setFormData({
      name: '',
      description: '',
      address: '',
      locationType: 'General',
      parentLocationId: undefined,
    });
  };

  const getLocationPath = (location: Location): string => {
    if (!location.parentLocation) return location.name;
    return `${getLocationPath(location.parentLocation)} > ${location.name}`;
  };

  if (loading) return <div className="loading">Loading locations...</div>;

  return (
    <div className="container">
      <div className="page-header">
        <h2>Locations</h2>
        <button className="btn-primary" onClick={() => setShowForm(true)}>
          Add New Location
        </button>
      </div>

      {error && <div className="error">{error}</div>}

      {showForm && (
        <div className="card">
          <h3>{editingLocation ? 'Edit Location' : 'Create New Location'}</h3>
          <form onSubmit={handleSubmit}>
            <div className="form-group">
              <label>Name *</label>
              <input
                type="text"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                required
              />
            </div>
            <div className="form-group">
              <label>Location Type *</label>
              <select
                value={formData.locationType}
                onChange={(e) => setFormData({ ...formData, locationType: e.target.value })}
                required
              >
                <option value="General">General</option>
                <option value="Factory">Factory</option>
                <option value="Home">Home</option>
                <option value="Warehouse">Warehouse</option>
                <option value="Room">Room</option>
                <option value="Shelf">Shelf</option>
                <option value="Container">Container</option>
              </select>
            </div>
            <div className="form-group">
              <label>Parent Location</label>
              <select
                value={formData.parentLocationId || ''}
                onChange={(e) =>
                  setFormData({
                    ...formData,
                    parentLocationId: e.target.value ? parseInt(e.target.value) : undefined,
                  })
                }
              >
                <option value="">None (Top Level)</option>
                {locations
                  .filter((loc) => !editingLocation || loc.id !== editingLocation.id)
                  .map((loc) => (
                    <option key={loc.id} value={loc.id}>
                      {getLocationPath(loc)}
                    </option>
                  ))}
              </select>
            </div>
            <div className="form-group">
              <label>Address</label>
              <input
                type="text"
                value={formData.address}
                onChange={(e) => setFormData({ ...formData, address: e.target.value })}
              />
            </div>
            <div className="form-group">
              <label>Description</label>
              <textarea
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              />
            </div>
            <div className="btn-group">
              <button type="submit" className="btn-primary">
                {editingLocation ? 'Update' : 'Create'}
              </button>
              <button type="button" className="btn-secondary" onClick={handleCancel}>
                Cancel
              </button>
            </div>
          </form>
        </div>
      )}

      {locations.length === 0 ? (
        <div className="empty-state">
          <h3>No locations yet</h3>
          <p>Create your first location to get started</p>
        </div>
      ) : (
        <div className="card">
          <table>
            <thead>
              <tr>
                <th>Name</th>
                <th>Type</th>
                <th>Parent Location</th>
                <th>Address</th>
                <th>Items</th>
                <th>Child Locations</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {locations.map((location) => (
                <tr key={location.id}>
                  <td>{location.name}</td>
                  <td>
                    <span className="badge badge-info">{location.locationType}</span>
                  </td>
                  <td>{location.parentLocation?.name || '-'}</td>
                  <td>{location.address || '-'}</td>
                  <td>
                    <span className="badge badge-info">
                      {location.itemLocations?.length || 0} item(s)
                    </span>
                  </td>
                  <td>
                    <span className="badge badge-info">
                      {location.childLocations?.length || 0} child(ren)
                    </span>
                  </td>
                  <td>
                    <div className="btn-group">
                      <button
                        className="btn-secondary"
                        onClick={() => handleEdit(location)}
                      >
                        Edit
                      </button>
                      <button
                        className="btn-danger"
                        onClick={() => handleDelete(location.id)}
                      >
                        Delete
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};
