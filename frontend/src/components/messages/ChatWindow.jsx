import React, { useState } from "react";

function ChatWindow({ contact, messages, onSendMessage }) {
  const [draft, setDraft] = useState("");

  if (!contact) {
    return (
      <section className="chat-window chat-window--empty">
        <p>Select a contact from the inbox to view messages.</p>
      </section>
    );
  }

  const handleSend = () => {
    const trimmed = draft.trim();
    if (!trimmed) return;

    onSendMessage(trimmed);
    setDraft("");
  };

  const handleKeyDown = (e) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  return (
    <section className="chat-window">
      <header className="chat-header">
        <div>
          <h2>{contact.name}</h2>
          <p className="chat-subtitle">@{contact.handle}</p>
        </div>
        {contact.isVerified && (
          <span className="chat-verified-chip">Verified representative</span>
        )}
      </header>

      <div className="chat-history">
        {messages.length === 0 && (
          <div className="chat-empty-state">
            <p>No messages yet. Say hi! 👋</p>
          </div>
        )}

        {messages.map((msg) => (
          <div
            key={msg.id}
            className={
              "chat-message " +
              (msg.sender === "user"
                ? "chat-message--outgoing"
                : "chat-message--incoming")
            }
          >
            <div className="chat-bubble">
              <p>{msg.text}</p>
              <span className="chat-timestamp">{msg.timestamp}</span>
            </div>
          </div>
        ))}
      </div>

      <footer className="chat-input-row">
        <textarea
          className="chat-input"
          placeholder={`Message ${contact.name}...`}
          value={draft}
          onChange={(e) => setDraft(e.target.value)}
          onKeyDown={handleKeyDown}
          rows={2}
        />
        <button className="chat-send-button" onClick={handleSend}>
          Send
        </button>
      </footer>
    </section>
  );
}

export default ChatWindow;
