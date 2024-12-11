using System;
using System.Text.Json.Serialization;

namespace DuootApi.Models
{
    public class Vote
    {
        public int VoteID { get; set; }

        public int UserID { get; set; }

        public int PostID { get; set; }

        public int ChoiceID { get; set; }

        public DateTime VoteDate { get; set; }

        // Propiedades de Navegación
        [JsonIgnore] // Evita la serialización de User para prevenir bucles
        public User User { get; set; }

        [JsonIgnore] // Evita la serialización de Post para prevenir bucles
        public Post Post { get; set; }

        [JsonIgnore] // Evita la serialización de Choice para prevenir bucles
        public Choice Choice { get; set; }
    }
}
