using AutoMapper;
using Coursework.Data.Entities;
using Coursework.Data.IONetwork;
using Coursework.Gui.Dto;

namespace Coursework.Gui.Initializers
{
    public static class MapperInitializer
    {
        public static void InitializeMapper(IMapperConfigurationExpression configuration)
        {
            configuration.CreateMap<Channel, ChannelDto>();
            configuration.CreateMap<Channel, ChannelIoDto>();
            configuration.CreateMap<ChannelDto, Channel>();
            configuration.CreateMap<Node, NodeDto>();
            configuration.CreateMap<Node, NodeIoDto>();
        }
    }
}
