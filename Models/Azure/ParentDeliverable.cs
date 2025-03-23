namespace doc_easy.Models.Azure;

public class ParentDeliverable
{
    public int IdDeliverable { get; set; }
    public string Title { get; set; }
    public List<Deliverable> Children { get; set; } = [];
}