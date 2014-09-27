﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pineapple.Model;

namespace Pineapple.Data
{
    public interface ICategoryData
    {
        Category SaveCategory(Category category);
        Category GetCategoryById(int categoryId);
        List<Category> LoadCategoriesByCatalogId(int catalogId);
        List<Category> LoadCategoriesByCategoryIds(IEnumerable<int> categoryIds);
        bool DeleteCategory(int categoryId);

        CategoryItem SaveCategoryItem(CategoryItem categoryItem);
        CategoryItem GetCategoryItemById(int categoryItemId);
        List<CategoryItem> LoadCategoryItemsByCategoryId(int categoryId); 
        bool DeleteCategoryItems(IEnumerable<int> categoryItemIds);
    }
}