import React from 'react';
import { useLocation, Link } from 'react-router-dom';
import { useState, useEffect } from 'react';
import PetitionsPage from './pages/PetitionsPage';
import CalendarPage from './pages/CalendarPage';
import './Dashboard.css';

function Dashboard() {
    const [currentComponent, setCurrentComponent] = useState(<PetitionsPage/>); // this will hold the current element to display in main-content. 
                                                                                // default for now is PetitionsPage, will eventually be OfficialAnnouncements
    const [currentMenuItem, setCurrentMenuItem] = useState(''); // this is just the currently selected menu item. Change in this item will cause
                                                              // change in currentComponent within useEffect
    const state = useLocation().state ? useLocation().state : null;
    console.log(`useLocation returns ${useLocation()}`)
    console.log(`state is ${state}`);
    console.log(`state.user is ${state.user}`);
    const currentUser = state ? state.user : null;

    console.log(`current user is ${currentUser}`);

    useEffect(() => {
        if(currentMenuItem === 'official-announcements') {
            // set currentComponent to OfficialAnnouncements
        } else if(currentMenuItem === 'community-board') {
            // set currentComponent to CommunityBoard
        } else if(currentMenuItem === 'petitions') {
            setCurrentComponent(<PetitionsPage/>);
        } else if(currentMenuItem === 'calendar') {
            setCurrentComponent(<CalendarPage/>);
        }
        console.log(`Current menu item is ${currentMenuItem}`);
    }, [currentMenuItem]); // change in currentMenuItem causes useEffect to be run

    return (
        currentUser ?
            (<div className='dashboard'>
                <div className='main-content'>
                    {/* This is where the Official Announcements/Community Board/Petitions/Calendar will be displayed */}
                    {/* depending on which menu option is chosen. */}
                    {currentComponent}
                </div>
                <ul className='bottom-menu'>
                    <li onClick={() => setCurrentMenuItem('official-announcements')}><strong>Official Announcements</strong></li>
                    <li onClick={() => setCurrentMenuItem('community-board')}><strong>Community Board</strong></li>
                    <li onClick={() => setCurrentMenuItem('petitions')}><strong>Petitions</strong></li>
                    <li onClick={() => setCurrentMenuItem('calendar')}><strong>Calendar</strong></li>
                </ul>
            </div>) : (
                <div className='dashboard'>
                    <div className='err-msg'>
                        <p>You are currently signed out and cannot access this page. Please <Link className='link' to='/sign-in'>sign in and try again.</Link></p>
                    </div>
                </div>
            )
    );
}

export default Dashboard;
