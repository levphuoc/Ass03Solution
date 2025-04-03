using AutoMapper;
using AutoMapper.Execution;
using BLL.DTOs;
using DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Mappings
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            /*CreateMap<CreateMember, Member>().ReverseMap();*/

            CreateMap<TracingOrder, TrackingOrderDTO>().ReverseMap();
        }
    }
}
