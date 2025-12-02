import { useState } from 'react';

function CreateReplyForm({ onSubmit }) {
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
        <textarea
          rows="1"
          placeholder="Reply to this post..."
          value={content}
          onChange={e => setContent(e.target.value)}
          required
        />
      </label>

      <div className="form-actions">
        <button type="submit" className="btn btn-accent">Send reply</button>
      </div>
    </form>
  );
}

export default CreateReplyForm;