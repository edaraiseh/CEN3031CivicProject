// src/pages/Announcements.jsx
import React from "react";
import "./Announcements.css"; // we can create this later if needed

function Announcements() {
  return (
    <div className="announcements-page">
      <h1>Official Announcements</h1>
      <p>
        View official updates from your government officials.
      </p>

      <section className="announcements-list">
        <div className="announcement-card">
          <h2>Example Announcement Title</h2>
          <p>
            This is a placeholder announcement. Later we can replace this with
            real data from the backend.
          </p>
          <p className="announcement-meta">
            Posted on: 2025-11-19 • Category: General
          </p>
        </div>
      </section>
    </div>
  );
}

export default Announcements;
