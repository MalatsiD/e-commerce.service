using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Core.Specifications
{
    public class ProductSpecParams
    {
        private readonly int MaxPageSize = 100;
        public int PageIndex { get; set; } = 1;

        private int _pageSize = 6;

        public int PageSize
        {
            get => _pageSize; 
            set => _pageSize  = (value > MaxPageSize) ? MaxPageSize : value; 
        }


        private List<string> _brands = [];

		public List<string> Brands
		{
			get => _brands;
			set 
			{
				_brands = value.SelectMany(x =>
					x.Split(',', StringSplitOptions.RemoveEmptyEntries)
				).ToList();
			}
		}

        private List<string> _types = [];

        public List<string> Types
        {
            get { return _types; }
            set
            {
                _types = value.SelectMany(x =>
                    x.Split(',', StringSplitOptions.RemoveEmptyEntries)
                ).ToList();
            }
        }

        public string Sort { get; set; } = string.Empty;

        private string? _search;

        public string Search
        {
            get => _search ?? string.Empty; 
            set => _search = value.ToLower(); 
        }


    }
}
