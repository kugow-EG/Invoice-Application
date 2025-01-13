using AutoMapper;
using Invoice.Interactor.DTO;
using Invoice.Interactor.Models;
using Invoice.Repository.Entity;

namespace Invoice.API.Mappings
{
    public class AutoMapperProfile:Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<InvoiceEntity, InvoiceModel>().ReverseMap();
            CreateMap<InvoiceEntity, InvoiceDTO>().ReverseMap();
        }
    }
}
