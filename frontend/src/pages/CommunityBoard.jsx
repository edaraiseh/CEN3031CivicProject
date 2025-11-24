// src/pages/CommunityBoard.jsx
import React from "react";
import "./CommunityBoard.css"; // also optional for now

function CommunityBoard() {
  return (
    <div className="community-board-page">
      <h1>Community Board</h1>
      <p>
        This page will allow residents to share posts, discussions, or local
        updates.
      </p>

      <section className="community-posts">
        <div className="community-post-card">
          <h2>Example Community Post</h2>
          <p>
            This is a placeholder community post. We’ll later hook this up to
            backend data or forms.
          </p>
          <p className="community-post-meta">
            Posted by: Jane Doe • Neighborhood: Downtown
          </p>
        </div>
      </section>
    </div>
  );
}

export default CommunityBoard;
