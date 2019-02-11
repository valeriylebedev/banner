using AutoMapper;
using AutoMapper.Configuration;
using BannerTest.Api.Models;
using BannerTest.Persistence.Context.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BannerTest.Api.Components
{
    public static class MapperFactory
    {
        public static IMapper GetMapper()
        {
            return new Mapper(GetConfiguration());
        }

        private static IConfigurationProvider GetConfiguration()
        {
            var cfg = new MapperConfigurationExpression();

            cfg.CreateMap<Banner, BannerModel>()
               .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
               .ForMember(d => d.Created, o => o.MapFrom(s => s.Created))
               .ForMember(d => d.Modified, o => o.MapFrom(s => s.Modified))
               .ForMember(d => d.Html, o => o.MapFrom(s => s.Html))
               .ForMember(d => d.Title, o => o.MapFrom(s => s.Title));

            cfg.CreateMap<CreateBannerModel, Banner>()
               .ForMember(d => d.Title, o => o.MapFrom(s => s.Title))
               .ForMember(d => d.Html, o => o.MapFrom(s => s.Html))
               .ForMember(d => d.Created, o => o.MapFrom(s => DateTime.UtcNow));

            cfg.CreateMap<UpdateBannerModel, Banner>()
               .ForMember(d => d.Title, o => o.MapFrom(s => s.Title))
               .ForMember(d => d.Html, o => o.MapFrom(s => s.Html))
               .ForMember(d => d.Modified, o => o.MapFrom(s => DateTime.UtcNow));

            return new MapperConfiguration(cfg);
        }
    }
}
