import React from "react";
// import { ReactComponent as VerifiedSvg } from '../assets/verified.svg';
import "./Post.css";

function Post({author, content, createdAt, reactions, onDelete, isOfficial}) {
  const formatDate = (iso) => {
    const d = new Date(iso);
    const mm = String(d.getMonth() + 1).padStart(2, '0');
    const dd = String(d.getDate()).padStart(2, '0');
    const yy = String(d.getFullYear()).slice(-2);
    const hh = String(d.getHours()).padStart(2, '0');
    const mi = String(d.getMinutes()).padStart(2, '0');
    return `${mm}/${dd}/${yy} ${hh}:${mi}`;
  }

  return(
      <div className="post-card">
        <h2>{author}</h2>
        <button className="delete-btn" onSubmit={onDelete}>Delete</button>
        <p>
          {content}
        </p>
        <p className="post-meta">
          {reactions /* will format later */}  {formatDate(createdAt)}
        </p>
      </div>
  );
}

export default Post;