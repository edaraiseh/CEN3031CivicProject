// src/pages/Announcements.jsx
import { React, useState, useEffect } from "react";
import Post from '../components/Post.jsx';
import CreatePostForm from '../components/CreatePostForm.jsx';
import "./Announcements.css"; // we can create this later if needed

function Announcements({ user }) {
  const [currentUser, setCurrentUser] = useState(user);
    const [currentUserName, setCurrentUserName] = useState('');
    const [announcements, setAnnouncements] = useState([]);
    const [sortBy, setSortBy] = useState('recent'); // 'asc' | 'desc'
    const [showForm, setShowForm] = useState(false);
    const [baseUrl, setBaseUrl] = useState(import.meta.env.VITE_API_BASE_URL);
  
    const getAnnouncements = async() => {  // if getting all posts, put null for searchTerm. Otherwise this function also doubles as a searching function
      console.log('shh...getAnnouncements is starting');
      try {
        const response = await fetch(`${baseUrl}/posts/official?` + new URLSearchParams({
          page: 1,
          pageSize: 100,
          sortBy: 'createDat',
        }))
        .then((response) => response.json())
        .then((data) => {
          setAnnouncements(data.posts);
          console.log(`posts is ${posts}`);
        });
        if(!response.ok) {
          throw new Error('Error fetching posts');
        }
      } catch(e) {
        console.log(e.message);
        return;
      }
    };
  
    const addAnnouncement = async(content) => {
      console.log(`user auth token is ${currentUser.token}`);
      try {
        const response = await fetch(`${baseUrl}/posts/official`, {
          method: 'PUT',
          headers: {
            'content-type': 'application/json',
            'Authorization': `bearer ${currentUser.token}`
          },
          body: JSON.stringify({
            "content": content
          })
        })
        console.log(`response is ${response}`);
        console.log(response);
      } catch(e) {
        throw new Error('Error creating post');
      } finally {
        getAnnouncements();
      }
    };
  
    const deleteAnnouncement = async(postId) => {
      try {
        const response = await fetch(`${baseUrl}/posts/${postId}`, {
          method: 'DELETE',
          headers: {
            'content-type': 'application/json',
            'Authorization': `bearer ${currentUser.token}`
          }
        })
        console.log(`response is ${response}`);
        console.log(response);
      } catch(e) {
        throw new Error('Error deleting post');
      } finally {
        getAnnouncements();
      }
    }

    const getAllReplies = async() => {
      announcements.forEach(post => {
        getReplies(post);
      }); 
    }

    const getReplies = async(post) => {
      try {
        const response = await fetch(`${baseUrl}/posts/${post.id}/replies`, {
          method: 'GET',
          headers: {
            'content-type': 'application/json',
            'Authorization': `bearer ${currentUser.token}`
          }
        })
        const data = await response.json();
        post.replies = data;
        console.log(data);
      } catch(e) {
        throw new Error(`Error getting post ${postId} replies.`);
      }
    }
    
    // Get current posts
    useEffect(() => {
      getAnnouncements();
    }, [sortBy]);

    useEffect(() => {
        getAllReplies();
        console.log(announcements);
    }, [announcements]);
  
    // Get current user's username
    // useEffect(() => {
    //   getCurrentUserName();
    // })
  return (
    <div className="announcements-page">
      <div className="page-header">
        <h1>Official Announcements</h1>
        <div className="actions">
          <button className="btn btn-accent" onClick={() => setShowForm(s => !s)}>
            {showForm ? "Close" : "Create Post"}
          </button>
        </div>
      </div>

      {showForm && (<div className="card card-accent">
          <CreatePostForm onSubmit={addAnnouncement} />
        </div>
      )}

      <p>
        View official updates from government officials.
      </p>

      <section className="announcements-list">
        {
          announcements.map(post => (
            <Post key={post.id} id={post.id} userToken={currentUser.token} author={post.author} content={post.content} createdAt={post.createdAt} reactions={post.reactions} replies={post.replies} onDelete={() => deleteAnnouncement(post.id)} isOfficial={true} />
          ))
        }
      </section>
    </div>
  );
}

export default Announcements;
