// src/pages/CommunityBoard.jsx
import { React, useState, useEffect } from "react";
import Post from '../components/Post.jsx';
import "./CommunityBoard.css"; // also optional for now

function CommunityBoard({ user }) { // user is passed in as a JSON object, no need to convert it to json
  const [currentUser, setCurrentUser] = useState(user);
  const [posts, setPosts] = useState([]);
  const [baseUrl, setBaseUrl] = useState(import.meta.env.VITE_API_BASE_URL);

  const getPosts = async(searchTerm) => {  // if getting all posts, put null for searchTerm. Otherwise this function also doubles as a searching function
    try {
      const response = await fetch(`${baseUrl}/posts?` + new URLSearchParams({
        page: 1,
        pageSize: 100,
        sortBy: 'updatedat',
        sortOrder: 'desc',
        search: searchTerm
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
    } catch(e) {
      throw new Error('Error creating post');
    } finally {
      getPosts(null);
    }
  };

  // Get current posts
  useEffect(() => {
    getPosts(null);
  }, []);
  return (
    <div className="community-board-page">
      <h1>Community Board</h1>
      <p>
        Share posts, discussions, or local updates with your community.
      </p>

      <section className="community-posts">
        {
          posts.map(post => (
            <Post key={post.id} author={post.author} content={post.content} createdAt={post.createdAt} reactions={post.reactions}/>
          ))
        }
      </section>
         
      <button className='create-button' onClick={() => addPost('Dog loose somewhere')}><img src='/assets/add.svg'/></button>
    </div>
  );
}

export default CommunityBoard;
