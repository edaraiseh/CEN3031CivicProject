import { useMemo, useEffect, useState } from 'react';
import PetitionCard from '../components/PetitionCard.jsx';
import CreatePetitionForm from '../components/CreatePetitionForm.jsx';
import './Petitions.css';


// TODO: uncomment when you integrate!! 
import axios from 'axios';

// TODO: When integrating backend, create a .env file in the project root:
const VITE_API_BASE_URL='http://localhost:5272/api';

export default function PetitionsPage() {
  // MOCK DATA: replace later with backend fetch
  const [petitions, setPetitions] = useState([]); 
  const [sortBy, setSortBy] = useState('recent'); // 'recent' | 'popular'
  const [showForm, setShowForm] = useState(false);

  // TODO(back end): load data 
  useEffect(() => {
    axios.get('/api/petitions').then(res => setPetitions(res.data));
  }, []);

   // TODO:  uncomment when you integrate:
  // BACKEND INTEGRATION (LOAD PETITIONS)
  
useEffect(() => {
  axios
    .get(`${import.meta.env.VITE_API_BASE_URL}/petitions`)
    .then(res => setPetitions(res.data))
    .catch(err => console.error('Failed to load petitions:', err));
}, []);
  


  function handleCreate({ title, description }) {
    // TODO(back end): POST /api/petitions
    const newPetition = {
      id: crypto.randomUUID(),               // backend will send real id
      title: title.trim(),
      description: description.trim(),
      createdAt: new Date().toISOString(),   // backend will set server time
      signatures: 0
    };
    setPetitions(prev => [newPetition, ...prev]);
    setShowForm(false);
  }

  // TODO: when integrating uncomment these & delete handleCreate^^

  
// BACKEND INTEGRATION (CREATE PETITION)
async function handleCreate({ title, description }) {
  try {
    const { data } = await axios.post(
      `${import.meta.env.VITE_API_BASE_URL}/petitions`,
      { title, description }
    );
    // backend returns the new petition object
    setPetitions(prev => [data, ...prev]);
    setShowForm(false);
  } catch (err) {
    console.error('Failed to create petition:', err);
  }
}


  function handleSign(id) {
    // TODO(back end): POST /api/petitions/{id}/sign
    setPetitions(prev =>
      prev.map(p => p.id === id ? { ...p, signatures: p.signatures + 1 } : p)
    );
  }

  // TODO: when integrating delete handleSign and uncomment this:

  
// BACKEND INTEGRATION (SIGN PETITION)
async function handleSign(id) {
  try {
    await axios.post(
      `${import.meta.env.VITE_API_BASE_URL}/petitions/${id}/sign` 
    );
    //re-fetch all petitions if backend handles signature increment: assumes backend handles signatures!
    // const { data } = await axios.get(`${import.meta.env.VITE_APP_API_BASE_URL}/petitions`);
    // setPetitions(data);
  } catch (err) {
    console.error('Failed to sign petition:', err);
  }
}


  const sorted = useMemo(() => {
    const list = [...petitions];
    if (sortBy === 'popular') {
      list.sort((a, b) => b.signatures - a.signatures || new Date(b.createdAt) - new Date(a.createdAt));
    } else {
      list.sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));
    }
    return list;
  }, [petitions, sortBy]);

  return (
    <main className="container">
      <div className="page-header">
        <h1>Petitions</h1>
        <div className="actions">
          <label>
            Sort by{" "}
            <select value={sortBy} onChange={e => setSortBy(e.target.value)}>
              <option value="recent">Most Recent</option>
              <option value="popular">Most Popular</option>
            </select>
          </label>
          <button className="btn btn-accent" onClick={() => setShowForm(s => !s)}>
            {showForm ? "Close" : "Create Petition"}
          </button>
        </div>
      </div>

      {showForm && (<div className="card card-accent">
          <CreatePetitionForm onSubmit={handleCreate} />
        </div>
      )}

      {sorted.length === 0 ? (
        <p className="muted">No petitions yet. Be the first to create one!</p>
      ) : (
        <ul className="list">
          {sorted.map(p => (
            <li key={p.id} className="card card-accent">
              <PetitionCard petition={p} onSign={() => handleSign(p.id)} />
            </li>
          ))}
        </ul>
      )}
    </main>
  );
}
