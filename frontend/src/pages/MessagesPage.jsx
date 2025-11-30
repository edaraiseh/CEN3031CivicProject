import React, { useState } from "react";
import InboxSidebar from "../components/messages/InboxSidebar.jsx";
import ChatWindow from "../components/messages/ChatWindow.jsx";
import "./Messages.css"; 
// import React, { useState, useEffect } from "react";  // TODO: uncomment when integrating

//TODO: Uncomment when integrating (both useEffect blocks)

/*
useEffect(() => {
  const loadContacts = async () => {
    try {
      const baseUrl = import.meta.env.VITE_API_BASE_URL;
      const res = await fetch(`${baseUrl}/contacts`); // TODO: replace with real endpoint!
      if (!res.ok) {
        throw new Error(`Failed to load contacts: ${res.status}`);
      }
      const data = await res.json();
      setContacts(data);
      if (data.length > 0) {
        setSelectedContactId(data[0].id);
      }
    } catch (err) {
      console.error("Failed to load contacts:", err);
    }
  };
  loadContacts();
}, []);

*/

//TODO: UNCOMMENT the useEffect block below too:
/*

useEffect(() => {
  const loadMessages = async () => {
    if (!selectedContactId) return;

    // Avoid refetching if we already have messages cached
    if (messagesByContact[selectedContactId]) return;

    try {
      const baseUrl = import.meta.env.VITE_API_BASE_URL;

      const res = await fetch(
        `${baseUrl}/conversations/${selectedContactId}/messages` // TODO: adjust endpoint
      );
      if (!res.ok) {
        throw new Error(`Failed to load messages: ${res.status}`);
      }

      const data = await res.json();

      setMessagesByContact((prev) => ({
        ...prev,
        [selectedContactId]: data,
      }));
    } catch (err) {
      console.error("Failed to load messages:", err);
    }
  };

  loadMessages();
}, [selectedContactId, messagesByContact]);

*/


//TODO: delete the whole intitialContacts block when integrating
const initialContacts = [
  {
    id: 1,
    name: "Jordan Smith",
    handle: "jordan_smith_rep",
    isVerified: true,
    lastMessagePreview: "Thanks for reaching out about the noise issue...",
  },
  {
    id: 2,
    name: "City Hall Support",
    handle: "cityhall_support",
    isVerified: true,
    lastMessagePreview: "We received your petition and are reviewing it.",
  },
  {
    id: 3,
    name: "Alex Johnson",
    handle: "alex_j",
    isVerified: false,
    lastMessagePreview: "Sounds good, I’ll sign and share it.",
  },
];

//TODO: delete the whole initialMessagesbyContact block when integrating
const initialMessagesByContact = {
  1: [
    {
      id: "1-1",
      sender: "user",
      text: "Hi Jordan, I wanted to follow up about the road safety issue near my neighborhood.",
      timestamp: "Mon 9:12 AM",
    },
    {
      id: "1-2",
      sender: "rep",
      text: "Thanks for reaching out! Can you share any photos or times of day it’s worst?",
      timestamp: "Mon 9:20 AM",
    },
  ],
  2: [
    {
      id: "2-1",
      sender: "rep",
      text: "We received your petition and are reviewing it this week.",
      timestamp: "Sun 3:45 PM",
    },
  ],
  3: [
    {
      id: "3-1",
      sender: "user",
      text: "Hey Alex, here’s the link to the petition!",
      timestamp: "Sat 7:02 PM",
    },
  ],
};

function App() {
  const [contacts, setContacts] = useState(initialContacts); //TODO: when integrating delete this line
  const [messagesByContact, setMessagesByContact] = useState(
    initialMessagesByContact
  ); //TODO: when integrating delete

  const [selectedContactId, setSelectedContactId] = useState(
    initialContacts[0]?.id ?? null
  ); //TODO: when integrating delete

  //TODO: when integrating, delete above 3 and uncomment these instead:
/*
const [contacts, setContacts] = useState([]);
const [messagesByContact, setMessagesByContact] = useState({});
const [selectedContactId, setSelectedContactId] = useState(null);
*/

  const [searchTerm, setSearchTerm] = useState("");
  const [showVerifiedOnly, setShowVerifiedOnly] = useState(false);

  const selectedContact = contacts.find(
    (c) => c.id === selectedContactId
  ) || null;

  const messages = selectedContact
    ? messagesByContact[selectedContact.id] || []
    : [];

    //TODO: delete this handleSendMessage and replace with the commented one below when integrating
  const handleSendMessage = (text) => {
    if (!selectedContact) return;

    const newMessage = {
      id: `${selectedContact.id}-${Date.now()}`,
      sender: "user",
      text,
      timestamp: "Just now",
    };

    setMessagesByContact((prev) => ({
      ...prev,
      [selectedContact.id]: [...(prev[selectedContact.id] || []), newMessage],
    }));

    setContacts((prev) =>
      prev.map((contact) =>
        contact.id === selectedContact.id
          ? { ...contact, lastMessagePreview: text }
          : contact
      )
    );
  };

  //TODO: Replace the mock version of handleSendMessage with this:
  /*
  const handleSendMessage = async (text) => {
  if (!selectedContact) return;

  const baseUrl = import.meta.env.VITE_API_BASE_URL;

  try {
    const res = await fetch(
      `${baseUrl}/conversations/${selectedContact.id}/messages`,
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ text }),
      }
    );

    if (!res.ok) {
      throw new Error(`Failed to send message: ${res.status}`);
    }

    const savedMessage = await res.json();

    //Add the message once backend returns it:
    setMessagesByContact((prev) => ({
      ...prev,
      [selectedContact.id]: [
        ...(prev[selectedContact.id] || []),
        savedMessage,
      ],
    }));

    //Update the sidebar preview
    setContacts((prev) =>
      prev.map((contact) =>
        contact.id === selectedContact.id
          ? { ...contact, lastMessagePreview: text }
          : contact
      )
    );
  } catch (err) {
    console.error("Failed to send message:", err);
  }
};
  */ 

  return (
  <div className="app-shell">
    <header className="page-header">
      <h1 className="page-title">Messages</h1>
    </header>

    <div className="messages-layout">
      <InboxSidebar
        contacts={contacts}
        selectedContactId={selectedContactId}
        onSelectContact={setSelectedContactId}
        searchTerm={searchTerm}
        onSearchTermChange={setSearchTerm}
        showVerifiedOnly={showVerifiedOnly}
        onToggleVerifiedOnly={setShowVerifiedOnly}
      />
      <ChatWindow
        contact={selectedContact}
        messages={messages}
        onSendMessage={handleSendMessage}
      />
    </div>
  </div>
);

}

export default App;
