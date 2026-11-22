using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Data.Entities
{
    public class Specialization
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public List<Doctor> Doctors{ get; set; }
    }
}
