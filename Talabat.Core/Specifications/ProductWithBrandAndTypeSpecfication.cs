using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities;

namespace Talabat.Core.Specifications
{
    public class ProductWithBrandAndTypeSpecfication : BaseSpecification<Product>
    {
        public ProductWithBrandAndTypeSpecfication(ProductSpecParams productParams)
            : base(P =>
                (string.IsNullOrEmpty(productParams.Search) || P.Name.ToLower().Contains(productParams.Search)) &&
                (!productParams.BrandId.HasValue || P.ProductBrandId == productParams.BrandId.Value) &&
                (!productParams.TypeId.HasValue || P.ProductTypeId == productParams.TypeId.Value)
            )
        {
            Includes.Add(P => P.ProductBrand);
            Includes.Add(P => P.ProductType);

            if (!string.IsNullOrEmpty(productParams.Sort))
            {
                switch (productParams.Sort)
                {
                    case "priceAsc":
                        AddOrderBy(P => P.Price);
                        break;
                    case "priceDesc":
                        AddOrderByDesc(P => P.Price);
                        break;
                    default:
                        AddOrderBy(P => P.Name);
                        break;
                }
            }

            ApplyPagination(productParams.PageSize * (productParams.PageIndex - 1), productParams.PageSize);
        }

        public ProductWithBrandAndTypeSpecfication(int id) : base(P => P.Id == id)
        {
            Includes.Add(P => P.ProductBrand);
            Includes.Add(P => P.ProductType);
        }
    }
}
