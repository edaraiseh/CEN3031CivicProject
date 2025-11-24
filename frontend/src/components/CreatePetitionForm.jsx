import { useState } from 'react';

export default function CreatePetitionForm({ onSubmit }) {
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');

  function submit(e) {
    e.preventDefault();
    if (!title.trim() || !description.trim()) return;
    onSubmit({ title, description });
    setTitle('');
    setDescription('');
  }

  return (
    <form onSubmit={submit} className="form">
      <label>
        Petition Title
        <input
          type="text"
          placeholder="e.g., Build a crosswalk near Maple St."
          value={title}
          onChange={e => setTitle(e.target.value)}
          required
        />
      </label>

      <label>
        Description
        <textarea
          rows="5"
          placeholder="Describe the issue and suggested action..."
          value={description}
          onChange={e => setDescription(e.target.value)}
          required
        />
      </label>

      <div className="form-actions">
        <button type="submit" className="btn btn-accent">Create</button>
      </div>
    </form>
  );
}
