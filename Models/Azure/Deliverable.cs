namespace doc_easy.Models.Azure;

public class Deliverable
{
    public int Id { get; set; }
    public string Titulo { get; set; }
    public string ParentId { get; set; }
    public string Descricao { get; set; }
    public bool Parent { get; set; }
    public TipoWorkItem Tipo { get; set; }
    public bool Finalizado { get; set; }
}
