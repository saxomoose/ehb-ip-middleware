using Middleware.Shared.Enums;
using Middleware.Shared.Models;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Middleware.Shared.Messages
{
    [Serializable]
    [XmlType("AttendeeEvent")]
    [XmlRoot("AttendeeEvent")]
    public class AttendeeEventMessage
    {
        [MinLength(32)]
        [XmlElement("UUID_Nr")]
        public string UUID_Nr { get; set; } = String.Empty;
        [XmlElement("SourceEntityId")]
        public string SourceEntityId { get; set; } = String.Empty;
        [XmlElement("EntityVersion")]
        public int EntityVersion { get ; set; }
        [XmlElement("EntityType")]
        public EntityType EntityType { get; set; }
        [XmlElement("Source")]
        public Source Source { get; set; }
        [MaxLength(30)]
        [XmlElement("Method")]
        public CrudMethod Method { get; set; }
        [MaxLength(30)]
        [XmlElement("Name")]
        public string Name { get; set; } = String.Empty;
        [XmlElement("LastName")]
        public string LastName { get; set; } = String.Empty;
        [XmlElement("Email")]
        public string Email { get; set; } = String.Empty;

        public AttendeeEventMessage() {}
        
        public AttendeeEventMessage(Resource resource, dynamic dto, CrudMethod crudMethod)
        {
            UUID_Nr = resource.Uuid.ToString();
            SourceEntityId = resource.SourceEntityId.ToString();
            EntityType = resource.EntityType;
            EntityVersion = resource.EntityVersion;
            Source = resource.Source;
            Method = crudMethod;
            Name = dto.FirstName;
            LastName = dto.LastName;
            Email = dto.Email;
        }
    }
}