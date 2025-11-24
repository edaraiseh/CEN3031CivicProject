import React from 'react';
import { useNavigate, Link } from 'react-router';
import { useState, useEffect } from 'react';
import './CreateAccount.css';

function CreateAccount() {
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [userName, setUserName] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [baseUrl, setBaseUrl] = useState(import.meta.env.VITE_API_BASE_URL);
  const [error, setError] = useState('');
  const [addingUser, setAddingUser] = useState(false);
  const navigate = useNavigate();

  const addUser = async(e) => {
    e.preventDefault();
    let userAuthenticated = false;
    
    if(name.trim().length <= 0) {
      alert('Name is required! Please enter your full name to create an account.');
      return;
    }
    if(email.trim().length <= 0) {
      alert('Email is required! Please enter your email to create an account.');
      return;
    }
    if(userName.trim().length <= 0) {
      alert('Username is required! Please enter a username to create an account.');
      return;
    }
    if(password.trim().length <= 0) {
      alert('Password cannot be empty!');
      return;
    }
    if(password !== confirmPassword) {
      alert('Passwords do not match. Please ensure both passwords are identical and try again.');
      return;
    }
    if(password.includes(' ')) {
      alert('Spaces are not permitted in passwords! Please remove all spaces from your password and try again.');
      return;
    }

    setName(name.trim()); // Remove unnecessary spaces from name
    setUserName(userName.trim()); // Remove unnecessary spaces from username

    try {
      setAddingUser(true);
      const response = await fetch(`${baseUrl}/users/register`, {
        method: 'PUT',
        credentials: 'include',
        headers: {
          'content-type': 'application/json'
        },
        body: JSON.stringify({
          'username': userName,
          'name': name,
          'email': email,
          'password': password,
          'profilepic': '',
          'bio': ''
        })
      });

      if(!response.ok) {
        throw new Error('Failed to add user.');
      }
      if(response.ok) {
        userAuthenticated = true;
      }
    } catch(e) {
      setError(e.message);
      alert('Failed to add user.');
      setAddingUser(false);
      return;
    } finally {
      setAddingUser(false);
    }

    alert(`Welcome to TownVoice, ${name}!`);
    if(userAuthenticated == true) {
      navigate('/sign-in');
    }
  }

  return (
    <div className='create-account'>
      <h1 className='create-account-header'>Create an account</h1>
        <div className='account-info-box'>
            <form onSubmit={addUser}>
                <input type='text' className='credential-input' id='name' placeholder='Enter your name' 
                  onChange={(e) => setName(e.target.value)}></input><br></br>
                <input type='email' className='credential-input' id='email' placeholder='Enter your email' 
                  onChange={(e) => setEmail(e.target.value)}></input><br></br>
                <input type='text' className='credential-input' id='username' placeholder='Create a username' 
                  onChange={(e) => setUserName(e.target.value)}></input><br></br>
                <input type='password' className='credential-input' id='password' placeholder='Create a password' 
                  onChange={(e) => setPassword(e.target.value)}></input><br></br>
                <input type='password' className='credential-input' id='password-confirmation' placeholder='Retype password' 
                  onChange={(e) => setConfirmPassword(e.target.value)}></input><br></br>
                <input type='submit' value='Submit' className='submit-btn'></input>
            </form>
            <Link className='sign-in-link' to='/sign-in'>Back to sign-in</Link>
        </div>
    </div>
  )
}

export default CreateAccount;