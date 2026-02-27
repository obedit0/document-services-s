using Application.Adapters.Common;
using Application.Adapters.SignatureContracts;
using Application.Usecases.Signature;
using Domain.Entities.Client;
using Domain.Entities.SignatureContracts;
using Domain.Enums;

namespace Application.Mapper.DocumentSignatureMapper
{
    internal static class DocumentSignatureMapper
    {
        internal static SignatureEntity MapToDomain(SignatureRequest request, SignatureHeaderRequest header)
        {
            var firmantes = request.Clientes
                ?.Select(cliente =>
                {
                    var identity = int.TryParse(cliente.IdCliente, out var id) ? id : (int?)null;
                    var contact = string.IsNullOrWhiteSpace(cliente.Email) && string.IsNullOrWhiteSpace(cliente.Telefono)
                        ? null
                        : new ContactEntity
                        {
                            Email = cliente.Email,
                            PhoneNumber = cliente.Telefono
                        };

                    var identityDocument = string.IsNullOrWhiteSpace(cliente.NumeroDocumento)
                        ? null
                        : new IdentityDocumentEntity
                        {
                            Number = cliente.NumeroDocumento
                        };

                    return new NaturalClientEntity
                    {
                        Identity = identity,
                        FullName = cliente.NombreCompleto,
                        Contact = contact,
                        IdentityDocument = identityDocument
                    };
                })
                .ToList();

            var documentos = request.Documentos
                ?.Select(documento => new DocumentEntity
                {
                    Name = documento.Name!,
                    OwnerClients = documento.OwnerClientId ?? [],
                    S3KeyOriginal = documento.S3KeyOriginal!
                })
                .ToList();

            var canal = SignatureUsecaseHelper.ParseChannel(header.ChannelIdentification) ?? throw new ArgumentException("El valor de Canal es inválido.", nameof(header.ChannelIdentification));

            var ordenFirma = new SignatureEntity
            {
                Reference = request.Keyword?.ToString() ?? string.Empty,
                Keyword = request.Keyword ?? 0,
                Title = request.Titulo ?? string.Empty,
                Description = request.Descripcion ?? string.Empty,
                Channel = canal,
                ExpirationTime = request.HoraExpiracion!.Value,
                NotificationTypeIds = request.IdTiposNotificacion!,
                PromissoryNote = request.Pagare,
                Clients = firmantes,
                Documents = documentos,
                Status = SignatureStatus.PENDIENTE,
                CreatedAt = DateTime.UtcNow.AddHours(-5),
                UpdatedAt = DateTime.UtcNow.AddHours(-5),
                History = new List<HistoryEntity>
                {
                    new HistoryEntity
                    {
                        EventDate = DateTime.UtcNow.AddHours(-5),
                        Source = "API",
                        NewStatus = SignatureStatus.PENDIENTE
                    }
                }
            };

            return ordenFirma;
        }

        internal static SignatureResponse MapToResponse(SignatureEntity entity)
        {
            return new SignatureResponse
            {
                IdFirma = entity.SignatureId,
                Referencia = entity.Reference,
                Estado = entity.Status.ToString(),
                FechaCreacion = entity.CreatedAt,
                FechaActualizacion = entity.UpdatedAt
            };
        }
    }
}
