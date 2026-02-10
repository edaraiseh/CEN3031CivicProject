import React from "react";

function InboxSidebar({
  contacts,
  selectedContactId,
  onSelectContact,
  searchTerm,
  onSearchTermChange,
  showVerifiedOnly,
  onToggleVerifiedOnly,
}) {
  const filteredContacts = contacts.filter((contact) => {
    const matchesSearch =
      contact.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      contact.handle.toLowerCase().includes(searchTerm.toLowerCase());

    const matchesVerified = showVerifiedOnly ? contact.isVerified : true;

    return matchesSearch && matchesVerified;
  });

  return (
    <aside className="inbox-sidebar">
      <div className="inbox-header">
        <h2>Inbox</h2>
      </div>

      <div className="inbox-search">
        <input
          type="text"
          placeholder="Search contacts or reps..."
          value={searchTerm}
          onChange={(e) => onSearchTermChange(e.target.value)}
        />
        <label className="verified-toggle">
          <input
            type="checkbox"
            checked={showVerifiedOnly}
            onChange={(e) => onToggleVerifiedOnly(e.target.checked)}
          />
          Only verified reps
        </label>
      </div>

      <ul className="contact-list">
        {filteredContacts.length === 0 && (
          <li className="contact-empty">No contacts found.</li>
        )}

        {filteredContacts.map((contact) => (
          <li
            key={contact.id}
            className={
              "contact-item" +
              (contact.id === selectedContactId ? " contact-item--active" : "")
            }
            onClick={() => onSelectContact(contact.id)}
          >
            <div className="contact-avatar">
              {contact.name
                .split(" ")
                .map((part) => part[0])
                .join("")
                .toUpperCase()}
            </div>
            <div className="contact-info">
              <div className="contact-main-row">
                <span className="contact-name">{contact.name}</span>
                {contact.isVerified && (
                  <span className="contact-badge">Verified</span>
                )}
              </div>
              <div className="contact-sub-row">
                <span className="contact-handle">@{contact.handle}</span>
                {contact.lastMessagePreview && (
                  <span className="contact-preview">
                    {contact.lastMessagePreview}
                  </span>
                )}
              </div>
            </div>
          </li>
        ))}
      </ul>
    </aside>
  );
}

export default InboxSidebar;
