using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PreskriptorAPI
{
    public class Drug
    {
        [Required]
        public string TradeName { get; set; }
        public List<string> Composition { get; set; }
    }
}