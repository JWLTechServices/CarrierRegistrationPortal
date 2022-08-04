using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<CarrierViewModel, carrierusers>().ReverseMap();
        }
    }
}
