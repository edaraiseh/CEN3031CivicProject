import { useState } from 'react';

function CreatePostForm({ onSubmit }) {
  const [content, setContent] = useState('');

  function submit(e) {
    e.preventDefault();
    if (!content.trim()) return;
    onSubmit(content);
    setContent('');
  }

  return (
    <form onSubmit={submit} className="form">
      <label>
        Post Content
        <textarea
          rows="5"
          placeholder="Write what's on your mind..."
          value={content}
          onChange={e => setContent(e.target.value)}
          required
        />
      </label>

      <div className="form-actions">
        <button type="submit" className="btn btn-accent">Create</button>
      </div>
    </form>
  );
}

export default CreatePostForm;