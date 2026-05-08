using EmergencyHelp.Data;
using EmergencyHelp.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Speech.Recognition;
using System.Speech.AudioFormat;
using NAudio.Wave;
using System.IO;

namespace EmergencyHelp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmergencyRestController : ControllerBase
    {
        private readonly ApplicationDBContext dbcontext;
        public EmergencyRestController(ApplicationDBContext dbContext)
        {
            this.dbcontext = dbContext;
        }

        [HttpGet]
        public IActionResult GetAllEmergencies()
        {
            var allEmergencies = dbcontext.Emergencies.ToList();
            return Ok(allEmergencies);
        }

        [HttpGet]
        [Route("{id:int}")]
        public IActionResult GetEmergencyById(int id)
        {
            var emergency = dbcontext.Emergencies.Find(id);
            if (emergency == null)
            {
                return NotFound();
            }
            return Ok(emergency);
        }

        [HttpPost]
        public IActionResult AddEmergency(AddEmergencyDto emergency)
        {
            var emergencyEntity = new Emergency()
            {
                Name = emergency.Name,
                Description = emergency.Description,
                Location = emergency.Location,
                Phone = emergency.Phone
            };

            dbcontext.Emergencies.Add(emergencyEntity);
            dbcontext.SaveChanges();
            return Ok(emergencyEntity);
        }

        [HttpPut]
        [Route("{id:int}")]
        public IActionResult UpdateEmergency(int id, UpdateEmergencyDto updateEmergency)
        {
            var emergency = dbcontext.Emergencies.Find(id);
            if (emergency == null)
            {
                return NotFound();
            }
            emergency.Name = updateEmergency.Name;
            emergency.Description = updateEmergency.Description;
            emergency.Location = updateEmergency.Location;
            emergency.Phone = updateEmergency.Phone;
            dbcontext.SaveChanges();
            return Ok(emergency);
        }

        [HttpDelete]
        [Route("{id:int}")]
        public IActionResult DeleteEmergency(int id)
        {
            var emergency = dbcontext.Emergencies.Find(id);
            if (emergency == null)
            {
                return NotFound();
            }
            dbcontext.Emergencies.Remove(emergency);
            dbcontext.SaveChanges();
            return Ok();
        }

        [HttpPost]
        [Route("audio-to-text")]
        public async Task<IActionResult> AudioToText(IFormFile audio)
        {
            if (audio == null || audio.Length == 0)
            {
                return BadRequest("Audio file is not provided or is empty.");
            }

            string tempFilePath = Path.GetTempFileName();
            try
            {
                // Save the uploaded file to a temporary location
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await audio.CopyToAsync(stream);
                }

                using (var recognizer = new SpeechRecognitionEngine())
                {
                    // Set up audio input from the audio file
                    var audioFormat = new SpeechAudioFormatInfo(
                        EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null);

                    using (var waveStream = new WaveFileReader(tempFilePath)) // NAudio to read WAV file
                    {
                        recognizer.SetInputToAudioStream(waveStream, audioFormat);

                        // Load the DictationGrammar (for general speech recognition)
                        recognizer.LoadGrammar(new DictationGrammar());

                        // Start the recognition process
                        var result = recognizer.Recognize();

                        // Return recognized text or a failure message
                        if (result != null)
                        {
                            return Ok(result.Text);
                        }
                        else
                        {
                            return BadRequest("Speech could not be recognized.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
            finally
            {
                // Clean up the temporary file
                try
                {
                    if (System.IO.File.Exists(tempFilePath))
                    {
                        System.IO.File.Delete(tempFilePath);
                    }
                   
                }
                catch (IOException ioEx)
                {
                    // Log the exception or handle it as needed
                    Console.WriteLine($"Error deleting temporary file: {ioEx.Message}");
                }
            }
        }

    }
}
