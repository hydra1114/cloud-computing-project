import React from 'react';
import { BrowserRouter, Routes, Route, Link, Navigate } from 'react-router-dom';
import { Items } from './components/Items';
import { Locations } from './components/Locations';
import { Inventory } from './components/Inventory';
import './App.css';

function App() {
  return (
    <BrowserRouter>
      <div className="app">
        <header className="navbar">
          <h1>Inventory Management System</h1>
          <nav>
            <Link to="/items">Items</Link>
            <Link to="/locations">Locations</Link>
            <Link to="/inventory">Inventory</Link>
          </nav>
        </header>
        <main>
          <Routes>
            <Route path="/" element={<Navigate to="/items" replace />} />
            <Route path="/items" element={<Items />} />
            <Route path="/locations" element={<Locations />} />
            <Route path="/inventory" element={<Inventory />} />
          </Routes>
        </main>
      </div>
    </BrowserRouter>
  );
}

export default App;
