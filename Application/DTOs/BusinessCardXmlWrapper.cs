using BusinessCardInformationAPI.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Application.DTOs;

[XmlRoot("BusinessCards")]
public class BusinessCardXmlWrapper
{
    [XmlElement("BusinessCard")]
    public List<CreateBusinessCardDto> Cards { get; set; } = new();
}