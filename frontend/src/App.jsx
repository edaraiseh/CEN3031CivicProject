import { Routes, Route, Navigate } from 'react-router-dom';
import HomeScreen from './HomeScreen.jsx';
import SignInScreen from './SignInScreen.jsx';
import CreateAccount from './CreateAccount.jsx';
import Dashboard from './Dashboard.jsx';
import Announcements from './pages/Announcements.jsx';
import CommunityBoard from './pages/CommunityBoard.jsx';



import './App.css';



function App() {
  return (
    <div className='app-container'>
      <Routes>
        <Route path='/' element={<HomeScreen/>}/>
        <Route path='/sign-in' element={<SignInScreen/>}/>
        <Route path='/create-account' element={<CreateAccount/>}/>
        <Route path='/dashboard' element={<Dashboard/>}/>

        <Route path='/announcements' element={<Announcements />} />
        <Route path='/community-board' element={<CommunityBoard />} />
        <Route path='*' element={<Navigate to='/' replace/>}/> 
      </Routes>
    </div>
  )
}

export default App;
