Messages Frontend Integration Notes

This folder contains the frontend implementation of the Messages page for the
TownVoice web application. Currently, it uses mock data and local state to simulate an inbox of recent contacts, viewing message history with a selected contact, and sending a new message.

This document explains exactly what to remove and what to replace it with
when connecting to the real backend once the API is ready.

THIS EXACT SAME INFO IS AVAILABLE in the comments of MessagesPage.jsx because that's the only file that needs editing when integrating with backend; refer to this document in case of errors/ confusion. 

1. Environment Setup
Create a `.env` file in the project root directory (same level as `package.json`) with something like:

```bash
VITE_API_BASE_URL=https://localhost:7060/api
In the code examples below, I’ll refer to this as:
import.meta.env.VITE_API_BASE_URL
2. Relevant Files
Backend integration will only touch MessagesPage.jsx.
InboxSidebar.jsx, ChatWindow.jsx, and styles.css will stay as-is.
3. Mock Data & Local State (WHAT TO DELETE LATER)
All mock data and purely local behavior live in src/App.jsx.
3.1 Mock contacts (to delete when integrating)
// MessagesPage.jsx

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
→ When integrating with the backend: DELETE this entire initialContacts block. Contacts will instead be loaded from the API.

3.2 Mock messages by contact (to delete when integrating)
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
→ When integrating: DELETE this entire initialMessagesByContact block. Messages will instead come from backend endpoints.

3.3 State seeded from mock data (to modify when integrating)
Current (mock):
const [contacts, setContacts] = useState(initialContacts);
const [messagesByContact, setMessagesByContact] = useState(
  initialMessagesByContact
);

const [selectedContactId, setSelectedContactId] = useState(
  initialContacts[0]?.id ?? null
);

const [searchTerm, setSearchTerm] = useState("");
const [showVerifiedOnly, setShowVerifiedOnly] = useState(false);

When integrating:
REPLACE the first three state initializations above with:


// When integrating with real backend:
const [contacts, setContacts] = useState([]);
const [messagesByContact, setMessagesByContact] = useState({});
const [selectedContactId, setSelectedContactId] = useState(null);

// (Keep these as-is)
const [searchTerm, setSearchTerm] = useState("");
const [showVerifiedOnly, setShowVerifiedOnly] = useState(false);

Contacts and messages will then be filled using API calls (see sections 4 and 5).
4. Loading Contacts from the Backend
4.1 Current behavior (mock)
Right now, contacts never come from the backend. They are only pulled from initialContacts.
4.2 When integrating: add useEffect to fetch contacts
In MessagesPage.jsx, at the top of the component, add useEffect:
import React, { useState, useEffect } from "react";
Then, add the following block inside App:
useEffect(() => {
  const loadContacts = async () => {
    try {
      const baseUrl = import.meta.env.VITE_API_BASE_URL;
      const res = await fetch(`${baseUrl}/contacts`); // TODO: replace with real endpoint
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
→ You can change /contacts to whatever the actual endpoint is. 

5. Loading Messages for the Selected Contact
5.1 Current behavior (mock)
Right now, messages are read from the mock object:
const messages = selectedContact
  ? messagesByContact[selectedContact.id] || []
  : [];

And that messagesByContact comes from initialMessagesByContact.
5.2 When integrating: fetch messages per contact
Add this useEffect inside MessagesPage:
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

✅ The computed messages line can stay:
const messages = selectedContact
  ? messagesByContact[selectedContact.id] || []
  : [];
6. Sending a Message
6.1 Current (mock)
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

→ When integrating: Delete or comment out this entire function body and replace with the version below.
6.2 When integrating (backend sends/returns messages)
Example (using fetch; feel free to swap for Axios):
// Replace the mock version of handleSendMessage with this:
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

    // Add the REAL message once backend returns it:
    setMessagesByContact((prev) => ({
      ...prev,
      [selectedContact.id]: [
        ...(prev[selectedContact.id] || []),
        savedMessage,
      ],
    }));
    // Update the sidebar preview
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
7. What NOT to Delete
When integrating with the backend, do NOT delete:


The Messages title / layout code in MessagesPage.jsx


The props structure:


<InboxSidebar contacts={...} selectedContactId={...} onSelectContact={...} ... />


<ChatWindow contact={...} messages={...} onSendMessage={...} />


These components are designed to be presentational: they should continue to work as long as they receive the same shapes of data.
8. Notes for Backend Developers
Suggested endpoints (names can be changed if front-end is updated to match):
GET /api/contacts
 Returns a list of contacts / conversations:

 [
  {
    "id": 1,
    "name": "Jordan Smith",
    "handle": "jordan_smith_rep",
    "isVerified": true,
    "lastMessagePreview": "Thanks for reaching out about the noise issue..."
  }
]


GET /api/conversations/{id}/messages
 Returns messages for a single conversation:

 [
  {
    "id": "1-1",
    "sender": "user",   // or "rep"
    "text": "string",
    "timestamp": "2025-11-29T09:12:00Z" // or any human-readable string
  }
]


POST /api/conversations/{id}/messages
 Accepts:

 {
  "text": "Hi, I wanted to follow up about..."
}
 and returns the saved message object in the same format as above.


→ Responses should be JSON and CORS must allow the frontend origin (e.g. http://localhost:5173).
9. Quick Checklist for Integration
Delete initialContacts in MessagesPage.jsx


Delete initialMessagesByContact in MessagesPage.jsx


Change useState(initialContacts) → useState([])


Change useState(initialMessagesByContact) → useState({})


Change initial selectedContactId to null


Add useEffect to load contacts from ${VITE_API_BASE_URL}/...


Add useEffect to load messages when selectedContactId changes


Replace the mock handleSendMessage with the async backend version



