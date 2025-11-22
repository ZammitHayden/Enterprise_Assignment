using Domain.Interfaces;
using System.Collections.Generic;

namespace Presentation.Models
{
    public class CatalogViewModel
    {
        public CatalogViewModel() { }
        public IEnumerable<IItemValidating> Items { get; set; }
        public string ViewType { get; set; } = "card";
        public bool IsApprovalPage { get; set; }
    }
}