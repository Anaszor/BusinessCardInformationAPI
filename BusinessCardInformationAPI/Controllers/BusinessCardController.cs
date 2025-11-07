using Application.DTOs;
using BusinessCardInformationAPI.Application.DTOs;
using BusinessCardInformationAPI.Application.Interfaces;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.Globalization;
using System.Xml.Serialization;
using ZXing;

namespace BusinessCardInformationAPI.BusinessCardInformationAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BusinessCardController : ControllerBase
    {
        private readonly IBusinessCardService _service;

        public BusinessCardController(IBusinessCardService service)
        {
            _service = service;
        }

        // POST: api/businesscards
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBusinessCardDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _service.AddAsync(dto);
                return Ok(new { message = "Business card created successfully" });
            }
            catch (ArgumentException ex)
            {
                // Handles invalid Base64 or photo size
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred.", details = ex.Message });
            }
        }

        // POST: api/businesscards/import


        [HttpPost("import")]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty or missing");

            var extension = Path.GetExtension(file.FileName).ToLower();

            try
            {
                if (extension == ".csv")
                {
                    using var stream = new StreamReader(file.OpenReadStream());
                    bool isFirstLine = true;

                    while (!stream.EndOfStream)
                    {
                        var line = stream.ReadLine();
                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        // Skip the header line
                        if (isFirstLine)
                        {
                            isFirstLine = false;
                            continue;
                        }

                        var values = line.Split(',');
                        if (values.Length < 6) continue;

                        var dto = new CreateBusinessCardDto
                        {
                            Name = values[0] ?? "",
                            Gender = values[1] ?? "",
                            DateOfBirth = DateTime.Parse(values[2], CultureInfo.InvariantCulture),
                            Email = values[3] ?? "",
                            Phone = values[4] ?? "",
                            Address = values[5] ?? "",
                            Photo = values.Length > 6 ? values[6] : null,
                        };

                        // Only accept valid Base64 for photo
                        if (!string.IsNullOrEmpty(dto.Photo))
                        {
                            try
                            {
                                Convert.FromBase64String(dto.Photo);
                            }
                            catch
                            {
                                dto.Photo = null; // non-base64 => ignored
                            }
                        }

                        await _service.AddAsync(dto);
                    }
                }

                else if (extension == ".xml")
                {
                    try
                    {
                        // Deserialize into a root wrapper
                        var serializer = new XmlSerializer(typeof(BusinessCardXmlWrapper));
                        BusinessCardXmlWrapper wrapper;
                        using (var stream = file.OpenReadStream())
                        {
                            wrapper = (BusinessCardXmlWrapper?)serializer.Deserialize(stream)
                                      ?? new BusinessCardXmlWrapper { Cards = new List<CreateBusinessCardDto>() };
                        }

                        foreach (var card in wrapper.Cards)
                        {
                            // Validate Base64 photo
                            if (!string.IsNullOrEmpty(card.Photo))
                            {
                                try
                                {
                                    Convert.FromBase64String(card.Photo);
                                }
                                catch
                                {
                                    card.Photo = null; // invalid base64 => ignore
                                }
                            }

                            await _service.AddAsync(card);
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        return BadRequest($"Invalid XML format: {ex.Message}");
                    }
                }
                else
                {
                    return BadRequest("Unsupported file format. Only CSV or XML are allowed.");
                }

                return Ok(new { message = "File imported successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred.", details = ex.Message });
            }
        }


        // POST: api/businesscards/import-qr
        [HttpPost("import-qr")]
        public async Task<IActionResult> ImportQr(IFormFile qrFile)
        {
            if (qrFile == null || qrFile.Length == 0)
                return BadRequest("File is empty or missing");

            try
            {
                using var stream = qrFile.OpenReadStream();
                using var bitmap = new Bitmap(stream);

                var luminanceSource = new ZXing.Windows.Compatibility.BitmapLuminanceSource(bitmap);
                var result = new BarcodeReaderGeneric().Decode(luminanceSource);

                if (result == null)
                    return BadRequest("No QR code detected");

                CreateBusinessCardDto dto;

                string qrText = result.Text.Trim();

                // Detect JSON vs CSV
                if (qrText.StartsWith("{") && qrText.EndsWith("}"))
                {
                    // JSON QR
                    try
                    {
                        dto = System.Text.Json.JsonSerializer.Deserialize<CreateBusinessCardDto>(qrText)
                              ?? throw new Exception("QR code JSON parsing failed");
                    }
                    catch
                    {
                        return BadRequest("QR code JSON format is invalid");
                    }
                }
                else
                {
                    // CSV QR
                    var values = qrText.Split(',');
                    if (values.Length < 6)
                        return BadRequest("Invalid CSV QR code format. Required: Name,Gender,DateOfBirth,Email,Phone,Address[,Photo]");

                    // Ensure DateOfBirth is valid
                    if (!DateTime.TryParse(values[2], CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dob))
                        return BadRequest("Invalid DateOfBirth format in CSV QR code");

                    string? photo = null;
                    if (values.Length > 6 && !string.IsNullOrWhiteSpace(values[6]))
                    {
                        try { Convert.FromBase64String(values[6]); photo = values[6]; }
                        catch { photo = null; } // ignore invalid photo
                    }

                    dto = new CreateBusinessCardDto
                    {
                        Name = string.IsNullOrWhiteSpace(values[0]) ? "Unknown" : values[0],
                        Gender = string.IsNullOrWhiteSpace(values[1]) ? "Unknown" : values[1],
                        DateOfBirth = dob,
                        Email = string.IsNullOrWhiteSpace(values[3]) ? "unknown@example.com" : values[3],
                        Phone = string.IsNullOrWhiteSpace(values[4]) ? "0000000000" : values[4],
                        Address = string.IsNullOrWhiteSpace(values[5]) ? "-" : values[5],
                        Photo = photo
                    };
                }

                await _service.AddAsync(dto);

                return Ok(new { message = "QR code imported successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred.", details = ex.Message });
            }
        }



        // GET: api/businesscards
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search = null)
        {
            var result = await _service.GetAllAsync(search);

            return Ok(result);
        }

        // DELETE: api/businesscards/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok(new { message = "Business card deleted successfully" });
        }

        // GET: api/businesscards/export
        [HttpGet("export")]
        public async Task<IActionResult> Export([FromQuery] string format = "csv")
        {
            try
            {
                byte[] fileBytes;
                string contentType;
                string fileName;

                if (format.ToLower() == "xml")
                {
                    fileBytes = await _service.ExportToXmlAsync();
                    contentType = "application/xml";
                    fileName = "businesscards.xml";
                }
                else
                {
                    fileBytes = await _service.ExportToCsvAsync();
                    contentType = "text/csv";
                    fileName = "businesscards.csv";
                }

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred.", details = ex.Message });
            }
        }
    }
}
