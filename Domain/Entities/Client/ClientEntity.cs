
namespace Domain.Entities.Client;

public abstract class ClientEntity
{
    public int? Identity { get; set; }
    public IdentityDocumentEntity? IdentityDocument { get; set; }
    public ContactEntity? Contact { get; set; }
    public List<AddressEntity>? Addresses { get; set; }
}
