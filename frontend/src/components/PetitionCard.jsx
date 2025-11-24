function formatDate(iso) {
  const d = new Date(iso);
  const mm = String(d.getMonth() + 1).padStart(2, '0');
  const dd = String(d.getDate()).padStart(2, '0');
  const yy = String(d.getFullYear()).slice(-2);
  const hh = String(d.getHours()).padStart(2, '0');
  const mi = String(d.getMinutes()).padStart(2, '0');
  return `${mm}/${dd}/${yy} ${hh}:${mi}`;
}

export default function PetitionCard({ petition, onSign }) {
  const { title, description, createdAt, signatures } = petition;

  return (
    <article>
      <div className="card-head">
        <h3 className="card-title">{title}</h3>
        <div className="card-meta">{formatDate(createdAt)}</div>
      </div>

      <p className="card-body">{description}</p>

      <div className="card-foot">
        <button className="btn btn-accent" onClick={onSign}>Sign This Petition</button>
        <span className="pill">
          {signatures} {signatures === 1 ? "signature" : "signatures"}
        </span>
      </div>
    </article>
  );
}
