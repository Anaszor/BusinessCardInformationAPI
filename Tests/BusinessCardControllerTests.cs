using System.Text;
using System.Globalization;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using BusinessCardInformationAPI.BusinessCardInformationAPI.Controllers;
using BusinessCardInformationAPI.Application.Interfaces;
using Application.DTOs;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using BusinessCardInformationAPI.Application.DTOs;

public class BusinessCardControllerTests
{
    private readonly Mock<IBusinessCardService> _serviceMock;
    private readonly BusinessCardController _controller;

    public BusinessCardControllerTests()
    {
        _serviceMock = new Mock<IBusinessCardService>();
        _controller = new BusinessCardController(_serviceMock.Object);
    }

    [Fact]
    public async Task Create_ValidDto_CallsServiceAndReturnsOk()
    {
        var dto = new CreateBusinessCardDto { Name = "A", Gender = "M", DateOfBirth = DateTime.UtcNow, Email = "a@b.com", Phone = "123", Address = "X" };

        _serviceMock.Setup(s => s.AddAsync(dto)).Returns(Task.CompletedTask).Verifiable();

        var result = await _controller.Create(dto);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Contains("message", ok.Value.ToString() ?? string.Empty);
        _serviceMock.Verify(s => s.AddAsync(dto), Times.Once);
    }

    [Fact]
    public async Task Import_Csv_CallsAddAsyncForEachRow()
    {
        // prepare CSV with header + 2 rows
        var csv = new StringBuilder();
        csv.AppendLine("Name,Gender,DateOfBirth,Email,Phone,Address");
        csv.AppendLine("John,M,1985-05-20,john@example.com,111,Addr1");
        csv.AppendLine("Jane,F,1990-09-15,jane@example.com,222,Addr2");
        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        using var stream = new MemoryStream(bytes);
        IFormFile file = new FormFile(stream, 0, bytes.Length, "file", "cards.csv")
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/csv"
        };

        _serviceMock.Setup(s => s.AddAsync(It.IsAny<CreateBusinessCardDto>())).Returns(Task.CompletedTask);

        var result = await _controller.Import(file);

        var ok = Assert.IsType<OkObjectResult>(result);
        _serviceMock.Verify(s => s.AddAsync(It.Is<CreateBusinessCardDto>(d => d.Name == "John")), Times.Once);
        _serviceMock.Verify(s => s.AddAsync(It.Is<CreateBusinessCardDto>(d => d.Name == "Jane")), Times.Once);
    }

    [Fact]
    public async Task Import_Xml_CallsAddAsyncForEachCard()
    {
        // build XML wrapper with two BusinessCard elements
        var wrapper = new Application.DTOs.BusinessCardXmlWrapper
        {
            Cards = new List<CreateBusinessCardDto>
            {
                new CreateBusinessCardDto { Name = "X", Gender = "M", DateOfBirth = DateTime.Parse("1980-01-01", CultureInfo.InvariantCulture), Email = "x@a.com", Phone = "1", Address = "A" },
                new CreateBusinessCardDto { Name = "Y", Gender = "F", DateOfBirth = DateTime.Parse("1990-02-02", CultureInfo.InvariantCulture), Email = "y@a.com", Phone = "2", Address = "B" }
            }
        };

        var serializer = new XmlSerializer(typeof(Application.DTOs.BusinessCardXmlWrapper));
        string xml;
        using (var ms = new MemoryStream())
        {
            serializer.Serialize(ms, wrapper);
            xml = Encoding.UTF8.GetString(ms.ToArray());
        }

        var bytes = Encoding.UTF8.GetBytes(xml);
        using var stream = new MemoryStream(bytes);
        IFormFile file = new FormFile(stream, 0, bytes.Length, "file", "cards.xml")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/xml"
        };

        _serviceMock.Setup(s => s.AddAsync(It.IsAny<CreateBusinessCardDto>())).Returns(Task.CompletedTask);

        var result = await _controller.Import(file);

        var ok = Assert.IsType<OkObjectResult>(result);
        _serviceMock.Verify(s => s.AddAsync(It.Is<CreateBusinessCardDto>(d => d.Name == "X")), Times.Once);
        _serviceMock.Verify(s => s.AddAsync(It.Is<CreateBusinessCardDto>(d => d.Name == "Y")), Times.Once);
    }

    [Fact]
    public async Task Export_ReturnsCsvFile_WhenFormatCsv()
    {
        var csvBytes = Encoding.UTF8.GetBytes("a,b,c");
        _serviceMock.Setup(s => s.ExportToCsvAsync()).ReturnsAsync(csvBytes);

        var result = await _controller.Export("csv");

        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("text/csv", fileResult.ContentType);
        Assert.Equal("businesscards.csv", fileResult.FileDownloadName);
        Assert.Equal(csvBytes, fileResult.FileContents);
    }

    [Fact]
    public async Task Export_ReturnsXmlFile_WhenFormatXml()
    {
        var xmlBytes = Encoding.UTF8.GetBytes("<root />");
        _serviceMock.Setup(s => s.ExportToXmlAsync()).ReturnsAsync(xmlBytes);

        var result = await _controller.Export("xml");

        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/xml", fileResult.ContentType);
        Assert.Equal("businesscards.xml", fileResult.FileDownloadName);
        Assert.Equal(xmlBytes, fileResult.FileContents);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithServiceResult()
    {
        var list = new List<BusinessCardDto>
        {
            new BusinessCardDto { Id = 1, Name = "A", Gender = "M", DateOfBirth = DateTime.UtcNow, Email = "a@b.com", Phone = "1", Address = "X" }
        };
        _serviceMock.Setup(s => s.GetAllAsync(null)).ReturnsAsync(list);

        var result = await _controller.GetAll(null);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(list, ok.Value);
    }

    [Fact]
    public async Task Delete_CallsServiceAndReturnsOk()
    {
        _serviceMock.Setup(s => s.DeleteAsync(5)).Returns(Task.CompletedTask).Verifiable();

        var result = await _controller.Delete(5);

        var ok = Assert.IsType<OkObjectResult>(result);
        _serviceMock.Verify(s => s.DeleteAsync(5), Times.Once);
    }
}