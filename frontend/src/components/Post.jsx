import { React, useState } from "react";
import Reply from "./Reply.jsx";
import CreateReplyForm from "./CreateReplyForm.jsx";
// import { ReactComponent as VerifiedSvg } from '../assets/verified.svg';
import "./Post.css";

function Post({id, userToken, author, content, createdAt, reactions, onDelete, replies, isOfficial}) {
  const [postReplies, setPostReplies] = useState(replies ? replies : []);
  const [showForm, setShowForm] = useState(false);
  const [baseUrl, setBaseUrl] = useState(import.meta.env.VITE_API_BASE_URL);

  const formatDate = (iso) => {
    const d = new Date(iso);
    const mm = String(d.getMonth() + 1).padStart(2, '0');
    const dd = String(d.getDate()).padStart(2, '0');
    const yy = String(d.getFullYear()).slice(-2);
    const hh = String(d.getHours()).padStart(2, '0');
    const mi = String(d.getMinutes()).padStart(2, '0');
    return `${mm}/${dd}/${yy} ${hh}:${mi}`;
  }

  const submitReply = async(content) => {
    console.log(userToken);
    try {
      const response = await fetch(`${baseUrl}/posts/${id}/replies`, {
        method: 'PUT',
        headers: {
          'content-type': 'application/json',
          'Authorization': `bearer ${userToken}`
        },
        body: {
          'content': content
        }
      })
    } catch(e) {
      console.error(`Error adding reply to post ${id}: ${e}`);
    }
  }

  console.log(replies);
  console.log(postReplies);

  return(
      <div className="post-card">
        <h2>{author}</h2>
        <button className="delete-btn" onClick={onDelete}>Delete</button>
        <p>
          {content}
        </p>
        <p className="post-meta">
          <button className='replies-btn' onClick={() => setShowForm(s => !s)}>Replies</button>{reactions /* will format later */}  {formatDate(createdAt)}
        </p>
        {showForm && (<section className='reply-container'>
          <CreateReplyForm onSubmit={submitReply}/>
          {
            postReplies.map(reply => {
              <Reply key={reply.id} author={reply.author} content={reply.content} createdAt={reply.createdAt} /> // add onDelete attribute with deletion function
            })
          }
        </section>
        )}
        
      </div>
  );
}

export default Post;