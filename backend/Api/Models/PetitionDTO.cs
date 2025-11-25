namespace Api.Models
{
    public class PetitionDTO
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required DateTime? UpdatedAt { get; set; }
        public int SignatureCount { get; set; }
        public required string Author { get; set; }
    }
    public class PetitionQueryDTO
    {
        public required int TotalItems { get; set; }
        public required int Page { get; set; }
        public required int PageSize { get; set; }
        public required int TotalPages { get; set; }
        public required List<PetitionDTO> Petitions { get; set; }
    }
    public class CreatePetitionDTO
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Content { get; set; }
    }
    public class DeletePetitionDTO
    {
        public int Id { get; set; }
    }
    public class CreatePetitionSignatureDTO
    {
        public int PetitionId { get; set; }
        public int UserId { get; set; }
    }
}