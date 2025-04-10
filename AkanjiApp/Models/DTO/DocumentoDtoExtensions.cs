namespace AkanjiApp.Models.DTO
{
    public static class DocumentoDtoExtensions
    {
        public static Documento ToEntity(this DocumentoDTO dto)
        {
            if (dto == null) return null;

            return new Documento
            {
                DOI = dto.DOI,
                Titulo = dto.Titulo,
                FechaPublicacion = dto.FechaPublicacion,
                ResourceType = dto.ResourceType,
                Description = dto.Description,
                Keywords = dto.Keywords,
                Language = dto.Language,
                Version = dto.Version,
                Publisher = dto.Publisher,
                RelatedIdentifiers = dto.RelatedIdentifiers?.Select(r => new RelatedIdentifier
                {
                    Identifier = r.Identifier,
                    RelationType = r.RelationType,
                    ResourceTypeGeneral = r.ResourceTypeGeneral
                }).ToList(),
                RightsList = dto.RightsList?.Select(r => new LicenciaDerechos
                {
                    Rights = r.Rights,
                    RightsUri = r.RightsUri
                }).ToList(),
                Subjects = dto.Subjects?.Select(s => new Subject
                {
                    Text = s.Text
                }).ToList(),
                AlternateIdentifiers = dto.AlternateIdentifiers?.Select(a => new AlternateIdentifier
                {
                    Identifier = a.Identifier,
                    Type = a.Type
                }).ToList(),
                Autores = dto.Autores?.Select(a => new DocumentoAutor
                {
                    Name = a.Name,
                    Affiliation = a.Affiliation,
                    ORCID = a.ORCID,
                    Role = a.Role,
                    Tipo = a.Tipo
                }).ToList(),
                Contributors = dto.Contributors?.Select(c => new DocumentoAutor
                {
                    Name = c.Name,
                    Affiliation = c.Affiliation,
                    ORCID = c.ORCID,
                    Role = c.Role,
                    Tipo = c.Tipo
                }).ToList()
            };
        }
    }
}
