using System.Web.Mvc;
using Web.Models.Data;

namespace Web.Models.ViewModels.Pages
{
    public class SidebarViewModel
    {
        public SidebarViewModel() { }

        public SidebarViewModel(SidebarDTO row)
        {
            Id = row.Id;
            Body = row.Body;
        }

        public int Id { get; set; }
        //[Required]
        //[StringLength(int.MaxValue, MinimumLength = 3)]
        [AllowHtml]
        public string Body { get; set; }
    }
}