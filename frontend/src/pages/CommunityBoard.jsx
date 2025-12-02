// src/pages/CommunityBoard.jsx
import { React, useState, useEffect } from "react";
import Post from '../components/Post.jsx';
import CreatePostForm from '../components/CreatePostForm.jsx';
import "./CommunityBoard.css"; // also optional for now

function CommunityBoard({ user }) { // user is passed in as a JSON object, no need to convert it to json
  const [currentUser, setCurrentUser] = useState(user);
  const [currentUserName, setCurrentUserName] = useState('');
  const [posts, setPosts] = useState([]);
  const [sortBy, setSortBy] = useState('recent'); // 'asc' | 'desc'
  const [showForm, setShowForm] = useState(false);
  const [baseUrl, setBaseUrl] = useState(import.meta.env.VITE_API_BASE_URL);

  const getPosts = async() => {  // if getting all posts, put null for searchTerm. Otherwise this function also doubles as a searching function
    console.log('shh...getPosts is starting');
    try {
      const response = await fetch(`${baseUrl}/posts?` + new URLSearchParams({
        page: 1,
        pageSize: 100,
        sortBy: 'createDat',
      }))
      .then((response) => response.json())
      .then((data) => {
        setPosts(data.posts);
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

  const addPost = async(content) => {
    console.log(`user auth token is ${currentUser.token}`);
    try {
      const response = await fetch(`${baseUrl}/posts`, {
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
      getPosts();
    }
  };

  const getCurrentUserName = async() => {
    try {
      const response = await fetch(`${baseUrl}/users/${user.userId}`, {
        method: 'GET',
        headers: {
          'content-type': 'application/json'
        }
      })
      setCurrentUserName(response.json().username);
      console.log(currentUserName);
      console.log(`response is ${response}`);
    } catch(e) {
      throw new Error('Error getting current username');
    }
  }

  const deletePost = async(postId) => {
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
      getPosts();
    }
  }

  const getAllReplies = async() => {
    posts.forEach(post => {
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
    getPosts();
  }, [sortBy]);

  // Get current user's username
  // useEffect(() => {
  //   getCurrentUserName();
  // })
  return (
    <div className="community-board-page">
      <div className="page-header">
        <h1>Community Board</h1>
        <div className="actions">
          <button className="btn btn-accent" onClick={() => setShowForm(s => !s)}>
            {showForm ? "Close" : "Create Post"}
          </button>
        </div>
      </div>

      {showForm && (<div className="card card-accent">
          <CreatePostForm onSubmit={addPost} />
        </div>
      )}

      <p>
        Share posts, discussions, or local updates with your community.
      </p>

      <section className="community-posts">
        {
          posts.map(post => (
            <Post key={post.id} id={post.id} userToken={currentUser.token} author={post.author} content={post.content} createdAt={post.createdAt} reactions={post.reactions} replies={post.replies} onDelete={() => deletePost(post.id)} />
          ))
        }
      </section>
    </div>
  );
}

export default CommunityBoard;
