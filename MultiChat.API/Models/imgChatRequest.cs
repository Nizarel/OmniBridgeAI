
using System.Text.Json.Serialization;
using System.Text.Json;
using SkiaSharp;

namespace MultiChat.API.Models
{
    public class imgChatRequest
    {
        public string SessionId { get; set; }
        public string? PromptText { get; set; }
        public Stream? imageFile { get; set; }
    }

  /* public class PayloadConverter : JsonConverter<imgChatRequest>
    {
        public override imgChatRequest Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Implement deserialization here if necessary


            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, imgChatRequest value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString("sessionId", value.SessionId);
            writer.WriteString("promptText", value.PromptText);
            writer.WriteBase64String("imageFile", ConvertStreamToByteArray(value.imageFile));

            writer.WriteEndObject();
        }

        private byte[] ConvertStreamToByteArray(Stream stream)
        {
            using var skData = SKData.Create(stream);
            using var skCodec = SKCodec.Create(skData);
            using var originalBitmap = SKBitmap.Decode(skCodec);

            SKImageInfo resizedInfo = new(640, 480);
            using var resizedBitmap = originalBitmap.Resize(resizedInfo, SKFilterQuality.High);
            using var image = SKImage.FromBitmap(resizedBitmap);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, 75);
            
            return data.ToArray();
        }
    }*/
}

