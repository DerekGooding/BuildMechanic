using System;
using System.Collections.Generic;
using System.Linq;

namespace BuildMechanic.Inspector
{
    public class ModelCatalog : IDisposable
    {
        private readonly Dictionary<ModelCategory, List<InspectableModel>> _catalog = new();
        private readonly List<IDisposable> _disposables = new();

        public ModelCategory ActiveCategory { get; private set; } = ModelCategory.ChassisAndCore;
        public int ActiveModelIndex { get; private set; } = 0;

        public IReadOnlyList<ModelCategory> Categories { get; } = Enum.GetValues<ModelCategory>();

        public IReadOnlyList<InspectableModel> ActiveCategoryModels =>
            _catalog.TryGetValue(ActiveCategory, out var list) ? list : Array.Empty<InspectableModel>();

        public InspectableModel ActiveModel
        {
            get
            {
                var list = ActiveCategoryModels;
                if (list.Count == 0) return null;
                int idx = Math.Clamp(ActiveModelIndex, 0, list.Count - 1);
                return list[idx];
            }
        }

        public void RegisterModel(InspectableModel model)
        {
            if (!_catalog.ContainsKey(model.Category))
            {
                _catalog[model.Category] = new List<InspectableModel>();
            }
            _catalog[model.Category].Add(model);
        }

        public void RegisterDisposable(IDisposable disposable)
        {
            if (disposable != null) _disposables.Add(disposable);
        }

        public void SelectCategory(ModelCategory category)
        {
            if (ActiveCategory != category)
            {
                ActiveCategory = category;
                ActiveModelIndex = 0;
            }
        }

        public void NextCategory()
        {
            int idx = Array.IndexOf(Categories.ToArray(), ActiveCategory);
            idx = (idx + 1) % Categories.Count;
            SelectCategory(Categories[idx]);
        }

        public void PrevCategory()
        {
            int idx = Array.IndexOf(Categories.ToArray(), ActiveCategory);
            idx = (idx - 1 + Categories.Count) % Categories.Count;
            SelectCategory(Categories[idx]);
        }

        public void SelectModel(int index)
        {
            var list = ActiveCategoryModels;
            if (list.Count > 0)
            {
                ActiveModelIndex = Math.Clamp(index, 0, list.Count - 1);
            }
        }

        public void NextModel()
        {
            var list = ActiveCategoryModels;
            if (list.Count > 0)
            {
                ActiveModelIndex = (ActiveModelIndex + 1) % list.Count;
            }
        }

        public void PrevModel()
        {
            var list = ActiveCategoryModels;
            if (list.Count > 0)
            {
                ActiveModelIndex = (ActiveModelIndex - 1 + list.Count) % list.Count;
            }
        }

        public int GetTotalModelCount()
        {
            return _catalog.Values.Sum(v => v.Count);
        }

        public void Dispose()
        {
            foreach (var d in _disposables)
            {
                d?.Dispose();
            }
            _disposables.Clear();
            _catalog.Clear();
        }
    }
}
