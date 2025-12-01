import { useMemo, useEffect, useState } from 'react';
import PetitionCard from '../components/PetitionCard.jsx';
import CreatePetitionForm from '../components/CreatePetitionForm.jsx';
import './Petitions.css';
import axios from 'axios';


export default function PetitionsPage({ user }) {
  // MOCK DATA: replace later with backend fetch
  const [currentUser, setCurrentUser] = useState(user);
  const [petitions, setPetitions] = useState([]); 
  const [sortBy, setSortBy] = useState('recent'); // 'recent' | 'popular'
  const [showForm, setShowForm] = useState(false);

// BACKEND INTEGRATION (GET PETITIONS)
const getPetitions = () => {
  console.log(petitions);
  axios
    .get(`${import.meta.env.VITE_API_BASE_URL}/petitions?` + new URLSearchParams({
      page: 1,
      pageSize: 100,
      sortBy: 'createDat'
    }))
    .then(res => setPetitions(res.data.petitions))
    .catch(err => console.error('Failed to load petitions:', err));
    console.log(petitions);
}

useEffect(() => {
  getPetitions();
}, []);

  
// BACKEND INTEGRATION (CREATE PETITION)
async function handleCreate({ title, description }) {
  console.log(currentUser.token);
  const config = {
    headers: {
      'content-type': 'application/json',
      'Authorization': `bearer ${currentUser.token}`
    }
  }
  console.log(petitions);
  console.log(`user jwt ${currentUser.token}`);
  try {
    const { data } = await axios.put(
      `${import.meta.env.VITE_API_BASE_URL}/petitions`,
      { "title": title, "content": description }, 
      config
    );
    // backend returns the new petition object
    getPetitions();
    setShowForm(false);
  } catch (err) {
    console.error('Failed to create petition:', err);
  }
  console.log(petitions);
}

  
// BACKEND INTEGRATION (SIGN PETITION)
async function handleSign(id) {
  const config = {
    headers: {
      'content-type': 'application/json',
      'Authorization': `bearer ${currentUser.token}`
    }
  }
  try {
    await axios.put(
      `${import.meta.env.VITE_API_BASE_URL}/petitions/${id}/sign`,
      {},
      config
    );
    getPetitions();
  } catch (err) {
    if(err.response.status === 409) {
        deletePetitionSign(id);
        return;
    }
    console.error('Failed to sign petition:', err);
  }
}

async function deletePetitionSign(id) {
  const config = {
    headers: {
      'content-type': 'application/json',
      'Authorization': `bearer ${currentUser.token}`
    }
  }
  try {
    await axios.delete(
      `${import.meta.env.VITE_API_BASE_URL}/petitions/${id}/sign`,
      {},
      config
    )
    .then(res => {
      if(res.ok) {
        alert('Petition unsigned sucessfully!');
      }
    })
  } catch(err) {
    console.error('Failed to delete petition signature:', err);
  }
  getPetitions();
}


  const sorted = useMemo(() => {
    const list = Array.from(petitions);
    console.log(list);
    if (sortBy === 'popular') {
      list.sort((a, b) => b.signatures - a.signatures || new Date(b.createdAt) - new Date(a.createdAt));
    } else {
      list.sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));
    }
    console.log(petitions);
    return list;
  }, [petitions, sortBy]);

  console.log(sorted);

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
              <PetitionCard key={p.id} petition={p} onSign={() => handleSign(p.id)} />
            </li>
          ))}
        </ul>
      )}
    </main>
  );
}
