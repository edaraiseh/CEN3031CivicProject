import React from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useState, useEffect } from 'react';
import './SignInScreen.css';

function SignInScreen() {
    const [userName, setUserName] = useState('');
    const [password, setPassword] = useState('');
    const [baseUrl, setBaseUrl] = useState('');
    const [error, setError] = useState('');
    const [signingIn, setSigningIn] = useState(false);
    let currentUser = '';
    const navigate = useNavigate();

    const signInUser = async(e) => {
        console.log(`current user originally ${currentUser}`);
        e.preventDefault();
        let userAuthenticated = false;

        if(userName.trim().length <= 0) {
            alert('Username is required! Please enter your username.');
            return;
        }
        if(password.trim().length <= 0) {
            alert('Password is required! Please enter a password.');
            return;
        }
        if(password.includes(' ')) {
            alert('Password cannot contain spaces!');
            return;
        }

        try {
            setSigningIn(true);
            const response = await fetch(`${baseUrl}/users/login`, {
                method: 'POST',
                credentials: 'include',
                headers: {
                  'content-type': 'application/json'
                },
                body: JSON.stringify({
                    'username': userName,
                    'password': password
                })
            });
            if(!response.ok) {
                throw new Error('Failed to sign in user.');
            }
            if(response.ok) {
                userAuthenticated = true;
                const json = await response.json();
                console.log(`json is ${json}`);
                currentUser = json;
                console.log(`current user is ${currentUser}`);
            }
        } catch(e) {
            setError(e.message);
            console.log(e.message);
            alert('Failed to sign in. Please check that your username and password are correct.');
            return;
        } finally {
            setSigningIn(false);
        }

        if(userAuthenticated) {
            console.log(`patching through to dashboard, currentUser is ${currentUser}`);
            navigate('/dashboard', {state: {user: currentUser}});
        }
    }

    return(
        <div className='sign-in-screen'>
            <h1 className='sign-in-header'>Sign Into Your TownVoice Community</h1>
            <div className='sign-in-box'>
                <form onSubmit={signInUser}>
                    <input type='text' className='credential-input' id='username' placeholder='Username' 
                        onChange={(e) => setUserName(e.target.value)}></input><br></br>
                    <input type='password' className='credential-input' id='password' placeholder='Password'
                        onChange={(e) => setPassword(e.target.value)}></input><br></br>
                    <input type='submit' value='Submit' className='submit-btn'></input>
                </form>
                <Link className='create-account-btn' to='/create-account'>Create a new account</Link>
            </div>
        </div>
    );
}

export default SignInScreen;