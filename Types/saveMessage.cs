using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TL;

namespace TeleBot.Types
{
    class saveMessage
    {
        public long id { get; set; }
        public long? from_id { get; set; }
        public string message { get; set; }
        public string mediaType { get; set; }
        public DateTime date { get; set; }
        public DateTime edit_date { get; set; }
        public MessageEntity[] entities { get; set; }
        public MessageReplyHeaderBase reply_to { get; set; }

        public static saveMessage fromMessage(Message msg)
        {
            string mediaType = "";
            if (msg.media != null)
            {
                if (msg.media is TL.MessageMediaPhoto)
                    mediaType = "photo";
                else if (msg.media is TL.MessageMediaDocument docMedia && docMedia.document is TL.Document doc)
                {
                    if (doc.mime_type == "image/webp")
                        mediaType = "sticker";
                    else if (doc.mime_type == "image/gif")
                        mediaType = "gif";
                    else if (doc.mime_type == "audio/ogg")
                        mediaType = "voice";
                    else if (doc.mime_type.StartsWith("video/"))
                        mediaType = "video";
                    else if (doc.mime_type.StartsWith("audio/"))
                        mediaType = "audio";
                    else
                        mediaType = "file";
                }
                else if (msg.media is TL.MessageMediaContact)
                    mediaType = "contact";
                else if (msg.media is TL.MessageMediaGeo)
                    mediaType = "geo";
                else
                    mediaType = "none";
            }

            try
            {
                return new saveMessage()
                {
                    id = msg.id,
                    from_id = (msg.from_id == null) ? null : msg.from_id.ID,
                    message = msg.message,
                    mediaType = mediaType,
                    date = msg.date,
                    edit_date = msg.edit_date,
                    entities = msg.entities,
                    reply_to = msg.reply_to
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting message: {ex.Message}");
                return null;
            }
        }
    }
}
